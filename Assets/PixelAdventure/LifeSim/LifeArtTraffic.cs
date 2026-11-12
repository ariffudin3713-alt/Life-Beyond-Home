using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Gambar lalu lintas: variasi mobil, bus kota, lampu lalu lintas (3 keadaan),
    /// rambu-rambu lalu lintas, dan warga pejalan kaki tanpa nama.
    /// Semua kendaraan menghadap ke KIRI (sama dengan mobil & angkot lama).
    /// </summary>
    public static partial class LifeArt
    {
        static void BuildTrafficArt()
        {
            MakeCar("vehicle_car_blue", "3b6fb6", "5a8fd6");
            MakeCar("vehicle_car_white", "dcdde2", "f4f5f8");
            MakeCar("vehicle_taxi", "e8b23a", "f6cf66");
            MakeBus();

            MakeTrafficLight("traffic_light_red", 0);
            MakeTrafficLight("traffic_light_yellow", 1);
            MakeTrafficLight("traffic_light_green", 2);

            MakeSignSpeed();
            MakeSignNoParking();
            MakeSignCrossing();
            MakeSignWarning();
            MakeSignNoEntry();

            // Warga pejalan kaki (tanpa papan nama).
            Cast("warga1", 0, "2b2420", "4a3c30", "e46f5a", "b8503f", "3a4058", "e8b98c");
            Cast("warga2", 1, "1f1a24", "3a3040", "f2a9c6", "cc7c9c", "4a3a50", "f0c9a4");
            Cast("warga3", 4, "6b6660", "9a958e", "5a8fc0", "3e6a94", "2f2a2a", "d9a97e");
            Cast("warga4", 3, "56353a", "98604b", "9ad07a", "6ea455", "56405a", "dca781");
            Cast("warga5", 5, "3b2a22", "6b4c38", "c79ad8", "9b70ad", "433027", "efc29c");
            Cast("warga6", 0, "322b23", "765638", "f2d36a", "c9a844", "2f3b52", "d8a17b");
        }

        // ------------------------------------------------------------------ kendaraan

        static void MakeCar(string key, string bodyHex, string lightHex)
        {
            Color32 outline = PixelArt.Hex("322b36");
            Color32 glass = PixelArt.Hex("bce5e8");
            Color32 rubber = PixelArt.Hex("241a2b");
            Color32 body = PixelArt.Hex(bodyHex);
            var car = new PixelCanvas(64, 34);
            car.Ellipse(32f, 5f, 30f, 4f, new Color32(18, 12, 24, 90));
            Outlined(car, 5, 8, 54, 14, body, outline);
            Outlined(car, 14, 21, 34, 10, body, outline);        // kabin
            car.Rect(17, 23, 13, 6, glass);
            car.Rect(32, 23, 13, 6, glass);
            car.Rect(30, 22, 2, 8, outline);
            car.Rect(8, 16, 48, 3, PixelArt.Hex(lightHex));
            car.Rect(6, 13, 4, 4, PixelArt.Hex("fff0b2"));        // lampu depan (kiri)
            car.Rect(54, 13, 4, 4, PixelArt.Hex("f27a69"));       // lampu belakang
            car.Disc(16f, 9f, 5f, rubber);
            car.Disc(48f, 9f, 5f, rubber);
            car.Disc(16f, 9f, 2f, PixelArt.Hex("c4c3b8"));
            car.Disc(48f, 9f, 2f, PixelArt.Hex("c4c3b8"));
            Add(car, new Vector2(0.5f, 0f), key);
        }

        static void MakeBus()
        {
            Color32 outline = PixelArt.Hex("322b36");
            Color32 glass = PixelArt.Hex("bce5e8");
            Color32 rubber = PixelArt.Hex("241a2b");
            var bus = new PixelCanvas(104, 44);
            bus.Ellipse(52f, 5f, 50f, 4f, new Color32(18, 12, 24, 90));
            Outlined(bus, 3, 7, 98, 34, PixelArt.Hex("2e8f62"), outline);
            bus.Rect(5, 9, 94, 8, PixelArt.Hex("e9e2c9"));        // lis bawah
            bus.Rect(5, 36, 94, 3, PixelArt.Hex("4fb07f"));       // atap terang
            for (int x = 20; x < 96; x += 13)
                Outlined(bus, x, 22, 11, 11, glass, outline);     // jendela penumpang
            Outlined(bus, 5, 18, 12, 18, glass, outline);         // kaca depan
            Outlined(bus, 19, 9, 9, 22, PixelArt.Hex("1f5f40"), outline);   // pintu
            bus.Rect(4, 11, 3, 4, PixelArt.Hex("fff0b2"));
            bus.Rect(97, 11, 3, 4, PixelArt.Hex("f27a69"));
            foreach (float wx in new[] { 18f, 84f })
            {
                bus.Disc(wx, 8f, 6f, rubber);
                bus.Disc(wx, 8f, 2.5f, PixelArt.Hex("c4c3b8"));
            }
            Add(bus, new Vector2(0.5f, 0f), "vehicle_bus");
        }

        // ------------------------------------------------------------------ lampu lalu lintas

        /// <param name="lit">0 merah, 1 kuning, 2 hijau.</param>
        static void MakeTrafficLight(string key, int lit)
        {
            var c = new PixelCanvas(16, 54);
            Color32 pole = PixelArt.Hex("5b5f66");
            Color32 poleLight = PixelArt.Hex("8a9098");
            Color32 housing = PixelArt.Hex("1d1b22");
            c.Ellipse(8f, 1.5f, 5f, 1.5f, new Color32(18, 12, 24, 90));
            c.Rect(7, 0, 3, 32, pole);
            c.Rect(7, 0, 1, 32, poleLight);
            c.Rect(5, 0, 7, 2, pole);
            Outlined(c, 2, 31, 13, 23, housing, PixelArt.Hex("0e0d12"));
            Color32[] on = { PixelArt.Hex("ff4b4b"), PixelArt.Hex("ffd23f"), PixelArt.Hex("4fe36a") };
            Color32[] off = { PixelArt.Hex("4a1f22"), PixelArt.Hex("4a3d1c"), PixelArt.Hex("1d3d24") };
            float[] ys = { 48f, 42f, 36f };
            for (int i = 0; i < 3; i++)
            {
                c.Disc(8.5f, ys[i], 2.6f, i == lit ? on[i] : off[i]);
                if (i == lit) c.Set(7, (int)ys[i] + 1, PixelArt.Hex("ffffff"));
            }
            Add(c, new Vector2(0.5f, 0f), key);
        }

        // ------------------------------------------------------------------ rambu

        /// <summary>Tiang rambu + kanvas; papan 14x14 berpusat di (8, 29).</summary>
        static PixelCanvas SignPole()
        {
            var c = new PixelCanvas(16, 38);
            c.Ellipse(8f, 1.5f, 4f, 1.2f, new Color32(18, 12, 24, 90));
            c.Rect(7, 0, 2, 24, PixelArt.Hex("8a9098"));
            c.Rect(7, 0, 1, 24, PixelArt.Hex("b9bec4"));
            return c;
        }

        static void Ring(PixelCanvas c, Color32 rim, Color32 fill)
        {
            c.Disc(8f, 29f, 7.2f, PixelArt.Hex("2a2630"));
            c.Disc(8f, 29f, 6.6f, rim);
            c.Disc(8f, 29f, 4.8f, fill);
        }

        /// <summary>Batas kecepatan 40.</summary>
        static void MakeSignSpeed()
        {
            var c = SignPole();
            Ring(c, PixelArt.Hex("e0383c"), PixelArt.Hex("ffffff"));
            Color32 ink = PixelArt.Hex("1d1b22");
            // angka "4" (3x5) di x 4..6, y 27..31
            c.Rect(4, 29, 1, 3, ink); c.Rect(6, 27, 1, 5, ink); c.Rect(4, 29, 3, 1, ink);
            // angka "0" di x 9..11
            c.Rect(9, 27, 3, 1, ink); c.Rect(9, 31, 3, 1, ink); c.Rect(9, 27, 1, 5, ink); c.Rect(11, 27, 1, 5, ink);
            Add(c, new Vector2(0.5f, 0f), "sign_speed");
        }

        /// <summary>Dilarang parkir: biru, tepi merah, garis miring merah.</summary>
        static void MakeSignNoParking()
        {
            var c = SignPole();
            Ring(c, PixelArt.Hex("e0383c"), PixelArt.Hex("2f63c8"));
            c.Line(4, 25, 12, 33, PixelArt.Hex("e0383c"), 2);
            Add(c, new Vector2(0.5f, 0f), "sign_no_parking");
        }

        /// <summary>Dilarang masuk: merah dengan palang putih.</summary>
        static void MakeSignNoEntry()
        {
            var c = SignPole();
            c.Disc(8f, 29f, 7.2f, PixelArt.Hex("2a2630"));
            c.Disc(8f, 29f, 6.6f, PixelArt.Hex("e0383c"));
            c.Rect(3, 28, 10, 3, PixelArt.Hex("ffffff"));
            Add(c, new Vector2(0.5f, 0f), "sign_no_entry");
        }

        /// <summary>Penyeberangan pejalan kaki: kotak biru, segitiga putih, orang berjalan.</summary>
        static void MakeSignCrossing()
        {
            var c = SignPole();
            Outlined(c, 1, 22, 14, 14, PixelArt.Hex("2f63c8"), PixelArt.Hex("2a2630"));
            c.Triangle(new Vector2(8f, 34f), new Vector2(2.5f, 24f), new Vector2(13.5f, 24f), PixelArt.Hex("ffffff"));
            Color32 ink = PixelArt.Hex("1d1b22");
            c.Set(8, 31, ink);                       // kepala
            c.Rect(8, 27, 1, 3, ink);                // badan
            c.Set(7, 26, ink); c.Set(9, 26, ink);    // kaki
            c.Set(6, 25, ink); c.Set(10, 25, ink);
            c.Set(7, 29, ink); c.Set(9, 28, ink);    // tangan
            for (int x = 3; x < 13; x += 2) c.Set(x, 24, PixelArt.Hex("2a2630"));   // zebra
            Add(c, new Vector2(0.5f, 0f), "sign_crossing");
        }

        /// <summary>Peringatan umum: belah ketupat kuning dengan tanda seru.</summary>
        static void MakeSignWarning()
        {
            var c = SignPole();
            Color32 edge = PixelArt.Hex("2a2630");
            Color32 yellow = PixelArt.Hex("ffd23f");
            for (int dy = -7; dy <= 7; dy++)
            {
                int half = 7 - Mathf.Abs(dy);
                c.Rect(8 - half, 29 + dy, half * 2 + 1, 1, edge);
                if (half > 1) c.Rect(8 - half + 1, 29 + dy, half * 2 - 1, 1, yellow);
            }
            c.Rect(8, 28, 1, 5, edge);
            c.Set(8, 26, edge);
            Add(c, new Vector2(0.5f, 0f), "sign_warning");
        }
    }
}
