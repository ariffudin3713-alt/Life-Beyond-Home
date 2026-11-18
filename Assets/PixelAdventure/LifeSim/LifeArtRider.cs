using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Pengendara sepeda / skuter / motor:
    /// - sosok pemain dipotong kakinya (6 baris bawah) dan didudukkan di jok (lihat LifePlayer.RiderSprite);
    /// - kaki & tangan digambar terpisah di sini: paha ke lutut, betis ke pijakan / pedal, sepatu,
    ///   lengan ke setang. Warna celana, sepatu, dan baju diambil dari pakaian yang sedang dipakai.
    /// Ukuran kanvas & pivot sama persis dengan gambar kendaraannya (tengah bawah), jadi ikut
    /// dicerminkan (flipX) bersama kendaraan saat menghadap kiri.
    /// </summary>
    public static partial class LifeArt
    {
        /// <summary>Posisi duduk pengendara (piksel kanvas kendaraan).</summary>
        public struct RideSeatInfo
        {
            public int Hip;     // tinggi pinggul (dasar badan yang sudah dipotong kakinya)
            public int Shift;   // geser ke jok pada tampak samping (kendaraan menghadap kanan)
        }

        /// <summary>view: 0 = depan, 1 = belakang, 2 = samping. False = kendaraan tanpa pengendara terlihat (mobil).</summary>
        public static bool RideSeat(string vehicle, int view, out RideSeatInfo seat)
        {
            seat = default;
            switch (vehicle)
            {
                case "motor":  seat.Hip = view == 1 ? 12 : 14; seat.Shift = view == 2 ? -6 : 0; return true;
                case "skuter": seat.Hip = view == 1 ? 10 : 12; seat.Shift = view == 2 ? -6 : 0; return true;
                case "sepeda": seat.Hip = view == 1 ? 10 : 13; seat.Shift = view == 2 ? -3 : 0; return true;
                default: return false;
            }
        }

        static readonly Dictionary<string, Sprite> RiderCache = new Dictionary<string, Sprite>();
        static readonly List<string> RiderOrder = new List<string>();
        const int RiderCacheSize = 48;

        /// <summary>
        /// Kaki & tangan pengendara untuk kendaraan + sudut pandang ini.
        /// <paramref name="phase"/> = fase kayuhan sepeda (0/1); kendaraan bermesin selalu 0.
        /// </summary>
        public static Sprite RiderLimbs(string vehicle, int view, int phase, Color32 pants, Color32 shoe, Color32 shirt)
        {
            if (!RideSeat(vehicle, view, out _)) return null;
            string key = vehicle + view + phase + "_" + Pack(pants) + "_" + Pack(shoe) + "_" + Pack(shirt);
            if (RiderCache.TryGetValue(key, out Sprite s) && s != null) { Touch(RiderOrder, key); return s; }

            PixelCanvas c;
            switch (vehicle)
            {
                case "motor":  c = MotorRider(view, pants, shoe, shirt); break;
                case "skuter": c = ScooterRider(view, pants, shoe, shirt); break;
                default:       c = BicycleRider(view, phase, pants, shoe, shirt); break;
            }
            s = c.ToSprite(BottomCenter);
            s.name = "lc_rider_" + key;
            RiderCache[key] = s;
            Touch(RiderOrder, key);
            while (RiderOrder.Count > RiderCacheSize)
            {
                string old = RiderOrder[0];
                RiderOrder.RemoveAt(0);
                if (RiderCache.TryGetValue(old, out Sprite os) && os != null) DestroySprite(os);
                RiderCache.Remove(old);
            }
            return s;
        }

        static uint Pack(Color32 c) => ((uint)c.r << 24) | ((uint)c.g << 16) | ((uint)c.b << 8) | c.a;

        // ------------------------------------------------------------------ alat gambar

        /// <summary>Anggota badan 2 piksel bergaris tepi gelap.</summary>
        static void Limb(PixelCanvas c, int x0, int y0, int x1, int y1, Color32 col)
        {
            c.Line(x0, y0, x1, y1, VInk, 4);
            c.Line(x0, y0, x1, y1, col, 2);
        }

        /// <summary>Paha -> lutut -> betis, lalu sepatu (lebar 4, tinggi 2) di ujung betis.</summary>
        static void Leg(PixelCanvas c, int hx, int hy, int kx, int ky, int fx, int fy, int toe, Color32 pants, Color32 shoe)
        {
            Limb(c, hx, hy, kx, ky, pants);
            Limb(c, kx, ky, fx, fy, pants);
            c.Rect(kx, ky, 1, 1, pants);   // sambungan lutut tanpa garis tepi di tengah
            int sx = toe > 0 ? fx - 1 : toe < 0 ? fx - 2 : fx - 1;
            int sw = toe != 0 ? 4 : 3;
            Outlined(c, sx, fy - 2, sw, 3, shoe, VInk);
        }

        /// <summary>Lengan dari bahu ke setang + telapak tangan.</summary>
        static void Arm(PixelCanvas c, int sx, int sy, int hx, int hy, Color32 shirt)
        {
            c.Line(sx, sy, hx, hy, VInk, 3);
            c.Line(sx, sy, hx, hy, shirt, 1);
            Outlined(c, hx - 1, hy - 1, 3, 3, VSkin, VInk);
        }

        // ------------------------------------------------------------------ motor (36x22 / 16x22)

        static PixelCanvas MotorRider(int view, Color32 pants, Color32 shoe, Color32 shirt)
        {
            if (view == 2)
            {
                var s = new PixelCanvas(36, 22);
                // Duduk di jok (x 6..17), kaki menapak pijakan di depan mesin, tangan di setang (x 22..27, y 18).
                Leg(s, 13, 14, 19, 13, 19, 8, 1, pants, shoe);
                Arm(s, 14, 20, 23, 18, shirt);
                return s;
            }
            var f = new PixelCanvas(16, 22);
            if (view == 0)
            {
                // Lutut terbuka ke samping bodi depan, tangan di ujung setang.
                Leg(f, 5, 13, 2, 11, 2, 7, 0, pants, shoe);
                Leg(f, 10, 13, 13, 11, 13, 7, 0, pants, shoe);
                Arm(f, 4, 20, 1, 17, shirt);
                Arm(f, 11, 20, 14, 17, shirt);
            }
            else
            {
                Leg(f, 5, 12, 2, 10, 2, 7, 0, pants, shoe);
                Leg(f, 10, 12, 13, 10, 13, 7, 0, pants, shoe);
            }
            return f;
        }

        // ------------------------------------------------------------------ skuter (32x20 / 16x20)

        static PixelCanvas ScooterRider(int view, Color32 pants, Color32 shoe, Color32 shirt)
        {
            if (view == 2)
            {
                var s = new PixelCanvas(32, 20);
                // Jok x 4..14; kaki ke dek (y 5..6) di belakang tameng depan, tangan di setang (x 21..26, y 18).
                Leg(s, 11, 12, 16, 11, 17, 8, 1, pants, shoe);
                Arm(s, 12, 18, 22, 17, shirt);
                return s;
            }
            var f = new PixelCanvas(16, 20);
            if (view == 0)
            {
                Leg(f, 5, 12, 3, 10, 3, 7, 0, pants, shoe);
                Leg(f, 10, 12, 12, 10, 12, 7, 0, pants, shoe);
                Arm(f, 4, 19, 1, 16, shirt);
                Arm(f, 11, 19, 14, 16, shirt);
            }
            else
            {
                Leg(f, 5, 10, 3, 8, 3, 6, 0, pants, shoe);
                Leg(f, 10, 10, 12, 8, 12, 6, 0, pants, shoe);
            }
            return f;
        }

        // ------------------------------------------------------------------ sepeda (32x20 / 16x20)

        static PixelCanvas BicycleRider(int view, int phase, Color32 pants, Color32 shoe, Color32 shirt)
        {
            if (view == 2)
            {
                var s = new PixelCanvas(32, 20);
                // Jok x 10..15 (y 13..14), gir di (15, 6): kaki bergantian mengayuh.
                if (phase == 0) Leg(s, 14, 13, 19, 11, 16, 5, 1, pants, shoe);
                else Leg(s, 14, 13, 18, 12, 18, 8, 1, pants, shoe);
                Arm(s, 15, 19, 20, 16, shirt);
                return s;
            }
            var f = new PixelCanvas(16, 20);
            int up = phase == 0 ? 2 : 0, down = 2 - up;   // satu pedal di atas, satu di bawah
            if (view == 0)
            {
                Leg(f, 6, 13, 5, 9, 5, 4 + up, 0, pants, shoe);
                Leg(f, 9, 13, 10, 9, 10, 4 + down, 0, pants, shoe);
                Arm(f, 4, 19, 1, 16, shirt);
                Arm(f, 11, 19, 14, 16, shirt);
            }
            else
            {
                Leg(f, 6, 10, 5, 7, 5, 3 + up, 0, pants, shoe);
                Leg(f, 9, 10, 10, 7, 10, 3 + down, 0, pants, shoe);
            }
            return f;
        }
    }
}
