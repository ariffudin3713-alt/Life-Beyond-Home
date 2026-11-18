using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Duduk di bangku taman. Dekati bangku dari depan -> "E - DUDUK" -> pemain bergeser mulus
    /// ke titik duduk kosong terdekat (bangku panjang punya kursi kiri & kanan). Selama duduk
    /// jalan / lari / kendaraan / interaksi lain terkunci; E, Spasi, tombol arah, atau ketuk layar
    /// = berdiri ke titik aman di depan bangku. Menu (jurnal, inventori, tablet, jeda) tetap bisa dibuka.
    /// </summary>
    public partial class PulangGame
    {
        /// <summary>Jarak maksimal dari titik depan bangku agar prompt duduk muncul.</summary>
        const float SeatReach = 1.3f;
        const string SeatOccupantPlayer = "pemain";

        BenchSeat _seat;         // kursi yang sedang diduduki pemain
        BenchSeat _targetSeat;   // kursi kosong terdekat (target E - DUDUK)

        void SitOn(BenchSeat s)
        {
            if (s == null || !s.Free || _player.SitBusy) return;
            if (RidingVehicle != null)
            {
                GameAudio.Play("error", 0.4f);
                _hud.ShowToast("SIMPAN KENDARAAN DULU (V) UNTUK DUDUK");
                return;
            }
            if (!_player.SitDown(s.Pos, _world.SeatExit(s), s.BenchOrder)) return;
            _world.TryReserveSeat(s, SeatOccupantPlayer);
            _seat = s;
            _hud.SetPrompt(null);
            _inputLock = Mathf.Max(_inputLock, 0.15f);   // E yang sama tidak langsung membuat berdiri
            GameAudio.Play("click", 0.3f);
        }

        /// <summary>Dipanggil setiap frame saat duduk (pengganti UpdateInteraction): prompt & tombol berdiri.</summary>
        void UpdateSeated()
        {
            if (!_player.IsSeated) { _hud.SetPrompt(null); return; }   // sedang transisi
            _hud.SetPrompt("E - BERDIRI");
            if (_inputLock > 0f) return;

            bool move = Mathf.Abs(LifeInput.X) > 0.1f || Mathf.Abs(LifeInput.Y) > 0.1f;
            bool tap = LifeInput.PointerTapped && !_hud.PointerOverUi;
            if (LifeInput.InteractPressed || move || tap)
            {
                _player.StandUp();
                _hud.SetPrompt(null);
                GameAudio.Play("click", 0.2f);
            }
        }

        /// <summary>Lepaskan kursi begitu pemain tidak lagi duduk (selesai berdiri, teleport, pindah wilayah).</summary>
        void SyncSeat()
        {
            if (_seat == null || _player.SitBusy) return;
            _world.ReleaseSeat(_seat, SeatOccupantPlayer);
            _seat = null;
        }

        /// <summary>Kursi kosong terdekat untuk UpdateInteraction (null saat berkendara / di dalam ruangan).</summary>
        BenchSeat SeatCandidate(Vector2 me) =>
            RidingVehicle != null ? null : _world.NearestFreeSeat(me, SeatReach);
    }
}
