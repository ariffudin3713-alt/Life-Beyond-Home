using System.Collections;
using PixelAdventure.LifeSim.Pulang;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Masuk & keluar bangunan (rumah, kost, minimarket, toko, kafe, penginapan, kantor,
    /// perpustakaan, bengkel, ...). Di dalam, menu tempat dibuka lewat kasir / kasur /
    /// resepsionis; keluar lewat keset pintu (berjalan ke bawah) atau tombol E di pintu.
    /// </summary>
    public partial class PulangGame
    {
        int _interiorSpot = -1;

        void EnterBuilding(LocationId loc)
        {
            if (!IsLocationOpen(loc))
            {
                Say(" ", "Tempat ini belum buka.");
                return;
            }
            StartCoroutine(DoorRoutine(() =>
            {
                Vector2 inside = _world.EnterInterior(loc);
                _player.Teleport(inside.x, inside.y);
                if (_cameraRig != null) { _cameraRig.Fixed = _world.InteriorCenter; _cameraRig.Snap(); }
                _hud.SetMinimapVisible(false);
                _hud.SetQuestTargets(null);
            }, () => _hud.ShowToast(PulangWorld.Get(loc).Name.ToUpperInvariant())));
        }

        void ExitBuilding()
        {
            if (!_world.InInterior) return;
            LocationId loc = _world.InteriorLocation;
            StartCoroutine(DoorRoutine(() =>
            {
                LeaveInteriorSilently();
                ScatterNpcs(_world.CurrentRegion);
                // Titik depan pintu; bila ternyata terhalang, geser ke titik kosong terdekat agar tidak tersangkut.
                Vector2 door = _world.NearestStandable(_world.Spot(loc)) ?? _world.ArrivalSpot(_world.CurrentRegion);
                _player.Teleport(door.x, door.y);
                _cameraRig?.Snap();
                _world.UpdateSigns(door);
            }, null, "door_close"));
        }

        /// <summary>Tutup ruangan tanpa animasi (dipakai juga saat tidur/pindah wilayah).</summary>
        void LeaveInteriorSilently()
        {
            if (_world == null || !_world.InInterior) return;
            _world.ExitInteriorRoom();
            if (_cameraRig != null) _cameraRig.Fixed = null;
            _hud.SetMinimapVisible(true);
            _interiorSpot = -1;
        }

        /// <summary>
        /// Suara latar sesuai tempat & waktu: di dalam bangunan hening; malam = jangkrik;
        /// kota & pasar = keramaian lalu lintas; pantai = ombak; desa & kampung = kicau burung.
        /// </summary>
        string AmbienceKey(float dark)
        {
            if (_world.InInterior) return null;
            if (dark > 0.6f) return _world.CurrentRegion == Region.PantaiCemara ? "beach" : "night";
            switch (_world.CurrentRegion)
            {
                case Region.KotaJogokerto:
                case Region.PasarRamaiJaya: return "city";
                case Region.PantaiCemara: return "beach";
                default: return "nature";
            }
        }

        /// <summary>Lagu latar: malam (19.00–05.00) lagu tenang; siang sesuai wilayah.</summary>
        int MusicFor(float minutes)
        {
            bool night = minutes >= 1140f || minutes < 300f;
            if (night) return GameAudio.MusicNight;
            switch (_world.CurrentRegion)
            {
                case Region.KotaJogokerto: return GameAudio.MusicTown;
                case Region.PasarRamaiJaya: return GameAudio.MusicMarket;
                case Region.PantaiCemara: return GameAudio.MusicBeach;
                default: return GameAudio.MusicVillage;     // Desa Suka Makmur & Kampung Cempaka
            }
        }

        /// <summary>Layar gelap sebentar saat lewat pintu.</summary>
        IEnumerator DoorRoutine(System.Action swap, System.Action after, string sound = "door_open")
        {
            _state = State.Busy;
            _hud.SetPrompt(null);
            GameAudio.Play(sound, 0.5f);
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime * 6f) { _hud.SetFade(t); yield return null; }
            _hud.SetFade(1f);
            swap();
            yield return null;
            for (float t = 1f; t > 0f; t -= Time.unscaledDeltaTime * 6f) { _hud.SetFade(t); yield return null; }
            _hud.SetFade(0f);
            _state = State.Playing;
            _inputLock = 0.2f;
            after?.Invoke();
        }

        /// <summary>Berjalan ke keset pintu (ke arah bawah) = keluar.</summary>
        bool CheckInteriorExit()
        {
            if (LifeInput.Y > -0.1f && !TapReachedExit()) return false;   // jalan ke bawah, atau ketuk keset pintu
            if (!_world.InteriorExitZone.Contains(_player.Feet)) return false;
            ExitBuilding();
            return true;
        }

        void UpdateInteriorInteraction()
        {
            _interiorSpot = -1;
            float best = 1.5f * 1.5f;
            var spots = _world.InteriorSpots;
            for (int i = 0; i < spots.Count; i++)
            {
                float d = (spots[i].Pos - _player.Feet).sqrMagnitude;
                if (d < best) { best = d; _interiorSpot = i; }
            }
            if (_interiorSpot < 0) { _hud.SetPrompt(null); return; }
            var s = spots[_interiorSpot];
            _hud.SetPrompt(s.Exit ? "E - KELUAR" : "E - " + s.Label);
        }

        void InteractInterior()
        {
            if (_interiorSpot < 0) return;
            var s = _world.InteriorSpots[_interiorSpot];
            if (s.Exit) { ExitBuilding(); return; }

            LocationId loc = _world.InteriorLocation;
            if (BuildLocationMenu(loc, null, null) > 0) OpenLocation(loc);
            else Say(" ", "Belum ada yang bisa dilakukan di sini sekarang.");
        }
    }
}
