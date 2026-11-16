using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Perabot khusus tiap jenis toko/gedung: sepeda & panggung motor & rak helm (dealer),
    /// dinding TV & etalase HP & speaker (elektronik), rak baju & manekin & kamar pas (toko baju),
    /// rak bunga (toko bunga), karung beras & galon (kelontong), rak joran & akuarium (pancing),
    /// ranjang pasien & kursi tunggu & tirai (rumah sakit), bangku sekolah & papan tulis & bendera,
    /// meja rapat (balai desa). Pivot tengah bawah, 16 px = 1 unit.
    /// </summary>
    public static partial class LifeArt
    {
        static void BuildInteriorArt2()
        {
            Color32 ink = FurnOutline;

            // ---- dealer
            var c = new PixelCanvas(36, 24);                                        // sepeda
            Color32 frame = PixelArt.Hex("e8b23a");
            c.Disc(8f, 7f, 6f, ink); c.Disc(8f, 7f, 4.5f, new Color32(0, 0, 0, 0));
            c.Disc(28f, 7f, 6f, ink); c.Disc(28f, 7f, 4.5f, new Color32(0, 0, 0, 0));
            c.Line(8, 7, 17, 16, frame, 2); c.Line(17, 16, 28, 7, frame, 2); c.Line(14, 16, 24, 16, frame, 2); c.Line(17, 7, 17, 16, frame, 1);
            c.Rect(13, 18, 6, 2, ink); c.Rect(23, 17, 5, 2, ink);
            Add(c, new Vector2(0.5f, 0f), "int_bicycle");

            c = new PixelCanvas(56, 10);                                            // panggung pamer (rata)
            Outlined(c, 0, 0, 56, 10, PixelArt.Hex("d8384a"), ink);
            c.Rect(2, 7, 52, 2, PixelArt.Hex("f26a6a"));
            Add(c, new Vector2(0.5f, 0f), "int_podium");

            c = new PixelCanvas(34, 36);                                            // rak helm
            Outlined(c, 0, 0, 34, 36, PixelArt.Hex("6b737c"), ink);
            Color32[] hc = { PixelArt.Hex("e0383c"), PixelArt.Hex("3b6fb6"), PixelArt.Hex("1d1b22"), PixelArt.Hex("ffd23f"), PixelArt.Hex("ffffff"), PixelArt.Hex("4fae62") };
            for (int r = 0; r < 2; r++)
            {
                c.Rect(2, 2 + r * 16, 30, 2, PixelArt.Hex("9aa2ab"));
                for (int i = 0; i < 3; i++) { c.Disc(7f + i * 10f, 9f + r * 16f, 4.5f, hc[(i + r * 3) % hc.Length]); c.Rect(4 + i * 10, 8 + r * 16, 6, 2, PixelArt.Hex("1d1b22")); }
            }
            Add(c, new Vector2(0.5f, 0f), "int_helmets");

            // ---- elektronik
            c = new PixelCanvas(64, 42);                                            // dinding TV
            Outlined(c, 0, 0, 64, 10, PixelArt.Hex("8a5a3c"), ink);
            for (int i = 0; i < 4; i++)
            {
                int tx = 2 + (i % 2) * 31, ty = 12 + (i / 2) * 15;
                Outlined(c, tx, ty, 29, 14, ink, ink);
                c.Rect(tx + 1, ty + 1, 27, 12, i % 3 == 0 ? PixelArt.Hex("6fb7ff") : i % 3 == 1 ? PixelArt.Hex("7fd486") : PixelArt.Hex("ff9a6a"));
                c.Rect(tx + 3, ty + 9, 8, 2, PixelArt.Hex("ffffff"));
            }
            Add(c, new Vector2(0.5f, 0f), "int_tv_wall");

            c = new PixelCanvas(42, 22);                                            // etalase HP
            Outlined(c, 0, 0, 42, 14, PixelArt.Hex("dcdde2"), ink);
            Outlined(c, 0, 12, 42, 10, PixelArt.Hex("bce5e8"), ink);
            for (int i = 0; i < 6; i++) { Outlined(c, 3 + i * 6, 13, 4, 7, PixelArt.Hex("2a2d36"), ink); c.Rect(4 + i * 6, 15, 2, 3, PixelArt.Hex("6fb7ff")); }
            Add(c, new Vector2(0.5f, 0f), "int_phone_case");

            c = new PixelCanvas(12, 22);                                            // speaker
            Outlined(c, 0, 0, 12, 22, PixelArt.Hex("2a2d36"), ink);
            c.Disc(6f, 7f, 4f, PixelArt.Hex("5b5f66")); c.Disc(6f, 16f, 2.5f, PixelArt.Hex("5b5f66"));
            Add(c, new Vector2(0.5f, 0f), "int_speaker");

            // ---- toko baju
            c = new PixelCanvas(46, 34);                                            // rak baju gantung
            c.Rect(2, 0, 2, 30, PixelArt.Hex("8a9098")); c.Rect(42, 0, 2, 30, PixelArt.Hex("8a9098")); c.Rect(2, 28, 42, 2, PixelArt.Hex("8a9098"));
            Color32[] cl = { PixelArt.Hex("e46f5a"), PixelArt.Hex("3b6fb6"), PixelArt.Hex("ffd23f"), PixelArt.Hex("4fae62"), PixelArt.Hex("ff9fc0"), PixelArt.Hex("8a63ad"), PixelArt.Hex("f4efe0") };
            for (int i = 0; i < 7; i++) { Outlined(c, 5 + i * 5, 10, 5, 17, cl[i], ink); c.Rect(7 + i * 5, 27, 1, 2, ink); }
            Add(c, new Vector2(0.5f, 0f), "int_clothes_rack");

            c = new PixelCanvas(14, 36);                                            // manekin
            c.Rect(6, 0, 2, 10, ink); c.Rect(3, 0, 8, 2, ink);
            Outlined(c, 2, 10, 10, 16, PixelArt.Hex("d8384a"), ink);
            c.Rect(2, 18, 10, 2, PixelArt.Hex("ffffff"));
            c.Disc(7f, 30f, 4f, PixelArt.Hex("f2e6d8"));
            Add(c, new Vector2(0.5f, 0f), "int_mannequin");

            c = new PixelCanvas(30, 46);                                            // kamar pas bertirai
            Outlined(c, 0, 0, 30, 46, PixelArt.Hex("8a5a3c"), ink);
            for (int x = 2; x < 28; x++) c.Rect(x, 2, 1, 40, (x / 3) % 2 == 0 ? PixelArt.Hex("8a63ad") : PixelArt.Hex("a87fc8"));
            c.Rect(2, 42, 26, 2, PixelArt.Hex("e8b23a"));
            Add(c, new Vector2(0.5f, 0f), "int_fitting");

            // ---- toko bunga
            c = new PixelCanvas(44, 30);                                            // rak bunga bertingkat
            Outlined(c, 0, 0, 44, 8, PixelArt.Hex("8a5a3c"), ink);
            Outlined(c, 6, 8, 32, 8, PixelArt.Hex("a8754a"), ink);
            Color32[] fl = { PixelArt.Hex("ff6b8a"), PixelArt.Hex("ffd23f"), PixelArt.Hex("ffffff"), PixelArt.Hex("c79ad8"), PixelArt.Hex("ff9a4a") };
            for (int i = 0; i < 6; i++) { c.Disc(5f + i * 7f, 10f, 3.2f, PixelArt.Hex("3a8a4c")); c.Disc(5f + i * 7f, 12f, 2f, fl[i % fl.Length]); }
            for (int i = 0; i < 4; i++) { c.Disc(11f + i * 7f, 19f, 3.2f, PixelArt.Hex("3a8a4c")); c.Disc(11f + i * 7f, 21f, 2f, fl[(i + 2) % fl.Length]); c.Disc(12.5f + i * 7f, 23f, 1.5f, fl[(i + 4) % fl.Length]); }
            Add(c, new Vector2(0.5f, 0f), "int_flower_stand");

            // ---- kelontong
            c = new PixelCanvas(38, 20);                                            // tumpukan karung beras
            for (int r = 0; r < 2; r++) for (int i = 0; i < 3 - r; i++)
            {
                Outlined(c, 2 + i * 12 + r * 6, r * 8, 11, 10, PixelArt.Hex("efe2c4"), ink);
                c.Rect(4 + i * 12 + r * 6, r * 8 + 4, 7, 2, PixelArt.Hex("4fae62"));
            }
            Add(c, new Vector2(0.5f, 0f), "int_rice_sacks");

            c = new PixelCanvas(26, 24);                                            // galon air
            for (int i = 0; i < 2; i++) { Outlined(c, 1 + i * 12, 0, 11, 18, PixelArt.Hex("9fd8e8"), ink); c.Rect(4 + i * 12, 18, 5, 3, PixelArt.Hex("3b6fb6")); c.Rect(3 + i * 12, 4, 2, 10, PixelArt.Hex("d8f4ff")); }
            Add(c, new Vector2(0.5f, 0f), "int_gallons");

            // ---- pancing
            c = new PixelCanvas(42, 42);                                            // rak joran
            Outlined(c, 0, 0, 42, 6, PixelArt.Hex("8a5a3c"), ink);
            for (int i = 0; i < 6; i++) { c.Line(4 + i * 7, 6, 2 + i * 7, 40, PixelArt.Hex("5e3826"), 1); c.Disc(5f + i * 7f, 12f, 2f, PixelArt.Hex("8a9098")); }
            for (int y = 16; y < 38; y += 4) for (int x = 2; x < 40; x += 4) if ((x + y) % 8 == 0) c.Set(x, y, PixelArt.Hex("efe2c4"));   // jaring
            Add(c, new Vector2(0.5f, 0f), "int_rods");

            c = new PixelCanvas(42, 32);                                            // akuarium
            Outlined(c, 0, 0, 42, 8, PixelArt.Hex("2a2d36"), ink);
            Outlined(c, 0, 8, 42, 22, PixelArt.Hex("6fb7ff"), ink);
            c.Rect(1, 9, 40, 3, PixelArt.Hex("c8a46a"));                            // pasir
            c.Ellipse(12f, 19f, 4f, 2f, PixelArt.Hex("ff9a4a")); c.Ellipse(28f, 23f, 4f, 2f, PixelArt.Hex("ffd23f")); c.Ellipse(20f, 15f, 3f, 1.5f, PixelArt.Hex("e46f5a"));
            c.Line(34, 11, 34, 22, PixelArt.Hex("4fae62"), 2); c.Line(7, 11, 6, 20, PixelArt.Hex("4fae62"), 2);
            Add(c, new Vector2(0.5f, 0f), "int_aquarium");

            // ---- rumah sakit
            c = new PixelCanvas(30, 42);                                            // ranjang pasien
            Outlined(c, 1, 0, 28, 38, PixelArt.Hex("aab2b4"), ink);
            Outlined(c, 3, 2, 24, 32, PixelArt.Hex("ffffff"), ink);
            Outlined(c, 6, 27, 18, 6, PixelArt.Hex("f4f7f4"), PixelArt.Hex("c8d0c8"));
            Outlined(c, 3, 2, 24, 18, PixelArt.Hex("8fc8e8"), ink);                 // selimut biru muda
            c.Rect(0, 36, 30, 3, PixelArt.Hex("aab2b4"));                           // pagar kepala
            Add(c, new Vector2(0.5f, 0f), "int_hospital_bed");

            c = new PixelCanvas(50, 18);                                            // kursi tunggu berderet
            c.Rect(2, 0, 2, 8, ink); c.Rect(46, 0, 2, 8, ink); c.Rect(2, 6, 46, 2, PixelArt.Hex("8a9098"));
            for (int i = 0; i < 4; i++) { Outlined(c, 2 + i * 12, 7, 11, 5, PixelArt.Hex("3b6fb6"), ink); Outlined(c, 2 + i * 12, 11, 11, 7, PixelArt.Hex("5a8fd6"), ink); }
            Add(c, new Vector2(0.5f, 0f), "int_waiting");

            c = new PixelCanvas(24, 42);                                            // tirai penyekat
            c.Rect(0, 0, 2, 42, PixelArt.Hex("aab2b4")); c.Rect(22, 0, 2, 42, PixelArt.Hex("aab2b4")); c.Rect(0, 40, 24, 2, PixelArt.Hex("aab2b4"));
            for (int x = 2; x < 22; x++) c.Rect(x, 4, 1, 35, (x / 3) % 2 == 0 ? PixelArt.Hex("8fd0c0") : PixelArt.Hex("a8e0d0"));
            Add(c, new Vector2(0.5f, 0f), "int_curtain");

            // ---- sekolah
            c = new PixelCanvas(26, 22);                                            // bangku + kursi siswa
            Outlined(c, 0, 8, 26, 8, PixelArt.Hex("c8925e"), ink);
            c.Rect(2, 0, 2, 9, PixelArt.Hex("5e3826")); c.Rect(22, 0, 2, 9, PixelArt.Hex("5e3826"));
            Outlined(c, 4, 15, 8, 4, PixelArt.Hex("f4efe0"), ink);                  // buku di meja
            Outlined(c, 9, 0, 9, 6, PixelArt.Hex("8a5a3c"), ink);
            Add(c, new Vector2(0.5f, 0f), "int_school_desk");

            c = new PixelCanvas(68, 34);                                            // papan tulis (hiasan dinding)
            Outlined(c, 0, 0, 68, 34, PixelArt.Hex("8a5a3c"), ink);
            Outlined(c, 3, 4, 62, 27, PixelArt.Hex("2f5a3e"), ink);
            c.Rect(8, 22, 20, 1, PixelArt.Hex("e8e8e8")); c.Rect(8, 18, 30, 1, PixelArt.Hex("e8e8e8")); c.Rect(8, 14, 16, 1, PixelArt.Hex("e8e8e8"));
            c.Rect(44, 10, 14, 14, new Color32(0, 0, 0, 0)); c.FrameRect(44, 10, 14, 14, PixelArt.Hex("e8e8e8"));
            c.Rect(5, 1, 60, 2, PixelArt.Hex("c8925e"));
            Add(c, new Vector2(0.5f, 0f), "int_blackboard");

            c = new PixelCanvas(14, 44);                                            // tiang bendera merah putih
            Outlined(c, 3, 0, 8, 4, PixelArt.Hex("8a9098"), ink);
            c.Rect(6, 4, 2, 38, PixelArt.Hex("aab2b4"));
            c.Rect(8, 34, 6, 4, PixelArt.Hex("d8384a")); c.Rect(8, 30, 6, 4, PixelArt.Hex("ffffff"));
            c.Disc(7f, 42f, 1.5f, PixelArt.Hex("e8b23a"));
            Add(c, new Vector2(0.5f, 0f), "int_flag");

            // ---- balai desa
            c = new PixelCanvas(68, 26);                                            // meja rapat panjang
            c.Rect(4, 0, 3, 12, PixelArt.Hex("5e3826")); c.Rect(61, 0, 3, 12, PixelArt.Hex("5e3826"));
            Outlined(c, 0, 10, 68, 14, PixelArt.Hex("a8754a"), ink);
            c.Rect(2, 20, 64, 2, PixelArt.Hex("c8925e"));
            for (int i = 0; i < 4; i++) Outlined(c, 8 + i * 14, 14, 9, 5, PixelArt.Hex("f4efe0"), ink);   // berkas
            Add(c, new Vector2(0.5f, 0f), "int_meeting_table");
        }
    }
}
