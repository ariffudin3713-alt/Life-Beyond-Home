using System.Collections;
using PixelAdventure.LifeSim.Pulang;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Pindah wilayah dengan berjalan ke ujung peta (kiri/kanan), tanpa naik bus.
    ///
    /// - Wilayah yang bersebelahan diatur di PulangWorld.EdgeLeft / EdgeRight.
    /// - Mendekati ujung peta memunculkan petunjuk "&lt; KE ..." / "KE ... &gt;".
    /// - Melewati ujung peta: layar meredup, pemain muncul di ujung seberang wilayah tujuan.
    /// - Gratis, tetapi memakan waktu (jalan kaki lebih lama daripada berkendara).
    /// - Aturan cerita tetap berlaku (Desa terkunci, persiapan merantau harus naik bus, dst).
    /// - Bus di terminal tetap bisa dipakai seperti biasa.
    /// </summary>
    public partial class PulangGame
    {
        const float EdgeTrigger = 0.8f;    // jarak dari tepi peta yang memicu pindah wilayah
        const float EdgeRearm = 1.6f;      // harus menjauh sejauh ini dari tepi sebelum bisa memicu lagi
        const float EdgeHint = 4.5f;       // jarak dari tepi untuk menampilkan petunjuk
        const float EdgeArrive = 2.2f;     // jarak dari tepi tempat pemain muncul
        const int EdgeWalkMinutes = 20;

        bool _edgeArmed = true;

        /// <summary>Wilayah di balik ujung peta terdekat (bila pemain cukup dekat ke tepi).</summary>
        bool TryEdgeNeighbor(float within, out Region target, out bool left)
        {
            Vector2 me = _player.Feet;
            Region here = _world.CurrentRegion;
            left = me.x < LifeWorld.Width * 0.5f;
            float edgeDist = left ? me.x : LifeWorld.Width - me.x;
            target = here;
            if (edgeDist > within) return false;
            return left
                ? PulangWorld.EdgeLeft.TryGetValue(here, out target)
                : PulangWorld.EdgeRight.TryGetValue(here, out target);
        }

        /// <summary>Petunjuk di layar saat pemain berada dekat ujung peta yang tersambung.</summary>
        string EdgePrompt()
        {
            if (_world.InInterior || !TryEdgeNeighbor(EdgeHint, out Region target, out bool left)) return null;
            string name = PulangWorld.RegionNames[target].ToUpperInvariant();
            return left ? "< JALAN KE " + name : "JALAN KE " + name + " >";
        }

        /// <summary>Dipanggil tiap frame saat bermain di luar ruangan. True bila perpindahan dimulai.</summary>
        bool CheckEdgeTravel()
        {
            float x = _player.Feet.x;
            if (x > EdgeRearm && x < LifeWorld.Width - EdgeRearm) { _edgeArmed = true; return false; }
            if (!_edgeArmed || _inputLock > 0f) return false;
            if (!TryEdgeNeighbor(EdgeTrigger, out Region target, out bool left)) return false;

            _edgeArmed = false;   // gagal atau berhasil: jangan ulangi sampai pemain menjauh dari tepi
            if (!CanWalkTo(target, out string reason))
            {
                GameAudio.Play("error", 0.5f);
                _player.ClearPath();
                Say(PulangProfile.Token, reason);
                return true;
            }
            StartCoroutine(EdgeTravelRoutine(target, left));
            return true;
        }

        bool CanWalkTo(Region target, out string reason)
        {
            int next = NextPendingIndex();
            bool travelNext = next >= 0 && _story.Current.Objectives[next].Event == StoryEvent.Travel;
            if (D.state == StoryState.PrologPersiapan)
            {
                // Berangkat merantau adalah momen cerita: harus naik bus dari terminal desa.
                reason = travelNext
                    ? "Jogokerto jauh sekali. Aku harus naik bus dari terminal desa."
                    : "Aku harus menyelesaikan persiapan merantau dulu.";
                return false;
            }
            if (!PulangWorld.CanWalkTo(target, _story.Chapter, out reason)) return false;
            if (target == Region.DesaSukaMakmur && D.state == StoryState.Ch6PerjalananTerakhir && !travelNext)
            {
                reason = "Masih ada yang harus kuselesaikan di Jogokerto.";
                return false;
            }
            return true;
        }

        IEnumerator EdgeTravelRoutine(Region target, bool left)
        {
            _state = State.Busy;
            _hud.SetPrompt(null);
            _player.ClearPath();
            var vehicle = RidingVehicle;
            int minutes = vehicle == null ? EdgeWalkMinutes : Mathf.Max(5, vehicle.TravelMinutes / 3);
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime * 3f) { _hud.SetFade(t); yield return null; }
            _hud.SetFade(1f);

            _clock = Mathf.Min(1439f, _clock + minutes);
            D.minutes = (int)_clock;
            if (vehicle == null) D.energy = Mathf.Max(0, D.energy - 3);   // berjalan jauh itu melelahkan

            // Masuk dari ujung seberang: keluar lewat kiri -> muncul di kanan wilayah tujuan.
            float x = left ? LifeWorld.Width - EdgeArrive : EdgeArrive;
            PlacePlayerAt(target, EdgeArrivalSpot(target, x));
            _edgeArmed = false;
            yield return null;

            for (float t = 1f; t > 0f; t -= Time.unscaledDeltaTime * 3f) { _hud.SetFade(t); yield return null; }
            _hud.SetFade(0f);

            EnterPlaying();
            _hud.ShowToast(PulangWorld.RegionNames[target].ToUpperInvariant());
            _story.Notify(StoryEvent.Travel, target.ToString());
        }

        /// <summary>Titik berdiri terdekat dari jalan utama di ujung peta.</summary>
        Vector2 EdgeArrivalSpot(Region r, float x)
        {
            _world.SetRegion(r);   // agar pemeriksaan rintangan memakai wilayah tujuan
            float y = PulangWorld.EdgeRoadY.TryGetValue(r, out float ry) ? ry : LifeWorld.Height * 0.5f;
            for (int i = 0; i < 40; i++)
            {
                float dy = (i % 2 == 0 ? 1f : -1f) * (i / 2) * 0.5f;
                for (int j = 0; j < 6; j++)
                {
                    float dx = (x < LifeWorld.Width * 0.5f ? 1f : -1f) * j * 0.6f;
                    if (_world.CanStand(x + dx, y + dy)) return new Vector2(x + dx, y + dy);
                }
            }
            return _world.ArrivalSpot(r);
        }
    }
}
