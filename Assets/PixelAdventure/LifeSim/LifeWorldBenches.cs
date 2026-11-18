using System.Collections.Generic;
using PixelAdventure.LifeSim.Pulang;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Satu titik duduk di bangku. Bangku panjang punya beberapa titik (kiri & kanan).
    /// </summary>
    public class BenchSeat
    {
        /// <summary>Posisi kaki (pivot sprite) pemain saat duduk.</summary>
        public Vector2 Pos;
        /// <summary>Titik di depan bangku: tempat mendekat untuk duduk dan tempat berdiri lagi.</summary>
        public Vector2 Exit;
        /// <summary>sortingOrder sprite bangku; yang duduk digambar tepat di depannya.</summary>
        public int BenchOrder;
        /// <summary>null = kosong. Diisi penghuni lain (mis. "kucing", id NPC) lewat <see cref="LifeWorld.TryReserveSeat"/>.</summary>
        public string Occupant;

        public bool Free => string.IsNullOrEmpty(Occupant);
    }

    /// <summary>
    /// Bangku yang bisa diduduki. Sprite "lc_bench" berukuran 40x28 px = 2,5 x 1,75 unit
    /// (pivot di tengah bawah). Kaki bangku menjadi penghalang tipis, dan setiap bangku
    /// punya dua titik duduk. Pemain mendekat dari depan (sisi bawah layar).
    /// </summary>
    public partial class LifeWorld
    {
        const float BenchFootW = 2.2f;                                   // penghalang kaki bangku
        const float BenchFootH = 0.45f;
        static readonly float[] BenchSeatOffsets = { -0.6f, 0.6f };     // kiri & kanan dari tengah
        const float BenchSeatHeight = 0.6f;                              // dudukan di atas dasar sprite
        const float BenchExitGap = 0.6f;                                 // titik berdiri di depan bangku

        readonly Dictionary<Region, List<BenchSeat>> _seats = new Dictionary<Region, List<BenchSeat>>();

        public IReadOnlyList<BenchSeat> SeatsIn(Region r) =>
            _seats.TryGetValue(r, out var list) ? list : (IReadOnlyList<BenchSeat>)System.Array.Empty<BenchSeat>();

        /// <summary>Pasang bangku taman yang bisa diduduki (sprite + penghalang + titik duduk).</summary>
        void Bench(float x, float y, string name)
        {
            Make("lc_bench", x, y, name, Order(y));
            AddObstacle(x - BenchFootW * 0.5f, y, BenchFootW, BenchFootH);
            RegisterBench(x, y);
        }

        /// <summary>Daftarkan titik duduk untuk sprite bangku yang sudah dipasang di (x, y).</summary>
        void RegisterBench(float x, float y)
        {
            if (!_seats.TryGetValue(_buildRegion, out var list)) _seats[_buildRegion] = list = new List<BenchSeat>();
            foreach (float ox in BenchSeatOffsets)
                list.Add(new BenchSeat
                {
                    Pos = new Vector2(x + ox, y + BenchSeatHeight),
                    Exit = new Vector2(x + ox, y - BenchExitGap),
                    BenchOrder = Order(y),
                });
        }

        /// <summary>Tandai titik duduk terdekat dari <paramref name="p"/> (wilayah yang sedang dibangun) sebagai terisi.</summary>
        void OccupySeatNear(Vector2 p, string who)
        {
            if (!_seats.TryGetValue(_buildRegion, out var list)) return;
            BenchSeat best = null;
            float bestD = 1f;
            foreach (var s in list)
            {
                float d = (s.Pos - p).sqrMagnitude;
                if (d < bestD) { bestD = d; best = s; }
            }
            if (best != null) best.Occupant = who;
        }

        /// <summary>
        /// Titik duduk kosong terdekat dari <paramref name="from"/> (diukur ke titik depan bangku),
        /// dalam jarak <paramref name="maxDist"/>. Null bila tidak ada / sedang di dalam ruangan.
        /// </summary>
        public BenchSeat NearestFreeSeat(Vector2 from, float maxDist)
        {
            if (InInterior) return null;
            BenchSeat best = null;
            float bestD = maxDist * maxDist;
            foreach (var s in SeatsIn(CurrentRegion))
            {
                if (!s.Free) continue;
                float d = (s.Exit - from).sqrMagnitude;
                if (d < bestD) { bestD = d; best = s; }
            }
            return best;
        }

        /// <summary>Titik berdiri aman di depan bangku; digeser ke titik kosong terdekat bila terhalang.</summary>
        public Vector2 SeatExit(BenchSeat s) => NearestStandable(s.Exit) ?? s.Exit;

        /// <summary>Untuk NPC: pesan titik duduk (false bila sudah terisi).</summary>
        public bool TryReserveSeat(BenchSeat s, string who)
        {
            if (s == null || !s.Free) return false;
            s.Occupant = who;
            return true;
        }

        public void ReleaseSeat(BenchSeat s, string who)
        {
            if (s != null && s.Occupant == who) s.Occupant = null;
        }
    }
}
