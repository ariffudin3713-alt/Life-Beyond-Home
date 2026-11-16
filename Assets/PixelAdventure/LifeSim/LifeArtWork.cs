using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Gambar mini-game kerja: ikon tiap pekerjaan (16x16), panah kombo (atas/bawah/kiri/kanan),
    /// bidikan kamera/polisi, dan sasaran bidik. Pivot tengah.
    /// </summary>
    public static partial class LifeArt
    {
        static void BuildWorkArt()
        {
            Color32 ink = IconInk;
            Color32 C(string h) => PixelArt.Hex(h);

            var c = Icon();                                                    // penjaga warung: toples & etalase
            Outlined(c, 1, 1, 14, 6, C("a8754a"), ink);
            for (int i = 0; i < 3; i++) { Outlined(c, 2 + i * 4, 7, 4, 6, C("cfeaf0"), ink); c.Rect(3 + i * 4, 8, 2, 3, i == 0 ? C("e0a65a") : i == 1 ? C("e0383c") : C("4fae62")); }
            Add(c, Center, "job_penjaga_warung");

            c = Icon();                                                        // kurir: kotak antar di motor
            c.Disc(4f, 3f, 2.5f, ink); c.Disc(12f, 3f, 2.5f, ink);
            c.Rect(4, 4, 9, 2, C("e0383c"));
            Outlined(c, 2, 6, 7, 7, C("4fae62"), ink); c.Rect(4, 9, 3, 1, C("ffffff"));
            c.Rect(11, 6, 2, 5, ink);
            Add(c, Center, "job_kurir_makanan");

            c = Icon();                                                        // pelayan kafe: nampan & cangkir
            c.Rect(1, 3, 14, 2, C("aab2b4")); c.Rect(7, 1, 2, 2, C("8a9098"));
            Outlined(c, 3, 5, 5, 5, C("ffffff"), ink); Outlined(c, 9, 5, 4, 7, C("8a5a3c"), ink);
            c.Rect(4, 11, 1, 2, C("e8e8e8")); c.Rect(6, 12, 1, 2, C("e8e8e8"));
            Add(c, Center, "job_pelayan_kafe");

            c = Icon();                                                        // tukang kebun: sekop & tanaman
            c.Rect(3, 1, 10, 3, C("8a5a3c"));
            c.Line(8, 4, 8, 10, C("4fae62"), 1); c.Disc(6f, 9f, 2f, C("6ec04a")); c.Disc(10f, 10f, 2f, C("6ec04a"));
            c.Line(12, 4, 14, 13, C("8a5a3c"), 1); Outlined(c, 10, 1, 4, 4, C("aab2b4"), ink);
            Add(c, Center, "job_tukang_kebun");

            c = Icon();                                                        // montir: kunci pas & baut
            c.Line(3, 3, 11, 11, C("aab2b4"), 3);
            c.Disc(12f, 12f, 3f, C("aab2b4")); c.Disc(13f, 13f, 1.4f, new Color32(0, 0, 0, 0));
            Outlined(c, 1, 11, 4, 4, C("e8b23a"), ink);
            Add(c, Center, "job_bengkel");

            c = Icon();                                                        // fotografer: kamera
            Outlined(c, 1, 3, 14, 9, C("2a2d36"), ink);
            c.Disc(8f, 7.5f, 3.5f, C("8a9098")); c.Disc(8f, 7.5f, 2f, C("6fb7ff"));
            c.Rect(3, 12, 4, 2, C("2a2d36")); c.Rect(12, 10, 2, 1, C("ffd23f"));
            Add(c, Center, "job_fotografer");

            c = Icon();                                                        // tukang cukur: gunting & sisir
            c.Disc(4f, 4f, 2.5f, C("e0383c")); c.Disc(4f, 4f, 1.2f, new Color32(0, 0, 0, 0));
            c.Disc(10f, 4f, 2.5f, C("e0383c")); c.Disc(10f, 4f, 1.2f, new Color32(0, 0, 0, 0));
            c.Line(5, 6, 13, 14, C("cfd3d8"), 1); c.Line(9, 6, 2, 14, C("cfd3d8"), 1);
            Add(c, Center, "job_tukang_cukur");

            c = Icon();                                                        // nelayan: ikan & kail
            c.Ellipse(7f, 6f, 5f, 3f, C("4fa8d8")); c.Triangle(new Vector2(2f, 6f), new Vector2(0f, 9f), new Vector2(0f, 3f), C("4fa8d8"));
            c.Set(10, 7, ink); c.Line(13, 15, 13, 9, C("8a9098")); c.Line(13, 9, 11, 7, C("8a9098"));
            Add(c, Center, "job_nelayan");

            c = Icon();                                                        // petani: padi & caping
            c.Triangle(new Vector2(1f, 9f), new Vector2(15f, 9f), new Vector2(8f, 14f), C("e0c070"));
            c.Rect(3, 8, 10, 1, C("a8904a"));
            for (int i = 0; i < 3; i++) { c.Line(4 + i * 4, 1, 5 + i * 4, 7, C("6ec04a")); c.Disc(5f + i * 4f, 6f, 1.2f, C("f0d060")); }
            Add(c, Center, "job_petani");

            c = Icon();                                                        // koki: wajan & api
            c.Ellipse(7f, 7f, 6f, 2.5f, C("2a2d36")); c.Rect(12, 7, 4, 1, C("8a5a3c"));
            c.Disc(5f, 8.5f, 1.3f, C("ffd23f")); c.Disc(8f, 8.5f, 1.3f, C("e0383c")); c.Disc(10f, 8f, 1f, C("4fae62"));
            for (int i = 0; i < 4; i++) c.Rect(3 + i * 3, 2 + (i % 2), 2, 2, i % 2 == 0 ? C("ff9a3a") : C("ffd23f"));
            Add(c, Center, "job_koki_restoran");

            c = Icon();                                                        // pegawai kantor: map dokumen
            Outlined(c, 2, 1, 12, 11, C("e8b23a"), ink);
            Outlined(c, 4, 4, 9, 10, C("ffffff"), ink);
            for (int y = 6; y < 13; y += 2) c.Rect(5, y, 7, 1, C("8a9098"));
            Add(c, Center, "job_pegawai_kantor");

            c = Icon();                                                        // desainer: pena digital & palet
            c.Ellipse(7f, 7f, 6f, 5f, C("f4efe0"));
            c.Disc(4f, 8f, 1.3f, C("e0383c")); c.Disc(7f, 10f, 1.3f, C("3b6fb6")); c.Disc(10f, 8f, 1.3f, C("ffd23f")); c.Disc(7f, 5f, 1.3f, C("4fae62"));
            c.Line(9, 1, 15, 9, C("2a2d36"), 2);
            Add(c, Center, "job_desainer_grafis");

            c = Icon();                                                        // guru: papan tulis
            Outlined(c, 1, 4, 14, 10, C("2f5a3e"), C("8a5a3c"));
            c.Rect(3, 10, 6, 1, C("ffffff")); c.Rect(3, 7, 9, 1, C("ffffff"));
            c.Rect(5, 1, 1, 3, C("8a5a3c")); c.Rect(10, 1, 1, 3, C("8a5a3c"));
            Add(c, Center, "job_guru");

            c = Icon();                                                        // perawat: topi berpalang merah
            Outlined(c, 2, 3, 12, 8, C("ffffff"), ink);
            c.Rect(7, 4, 2, 6, C("e0383c")); c.Rect(5, 6, 6, 2, C("e0383c"));
            c.Rect(1, 2, 14, 2, C("e8e8e8"));
            Add(c, Center, "job_perawat");

            c = Icon();                                                        // polisi: lencana bintang
            c.Disc(8f, 8f, 6.5f, C("2f4f8a"));
            c.Disc(8f, 8f, 4.5f, C("e8b23a"));
            c.Triangle(new Vector2(8f, 11f), new Vector2(5f, 6f), new Vector2(11f, 6f), C("fff1c0"));
            c.Triangle(new Vector2(8f, 4.5f), new Vector2(5f, 9f), new Vector2(11f, 9f), C("fff1c0"));
            Add(c, Center, "job_polisi");

            c = Icon();                                                        // dokter: stetoskop
            c.Line(4, 14, 4, 8, C("2a2d36"), 1); c.Line(12, 14, 12, 8, C("2a2d36"), 1);
            c.Line(4, 8, 8, 4, C("2a2d36"), 1); c.Line(12, 8, 8, 4, C("2a2d36"), 1);
            c.Line(8, 4, 11, 2, C("2a2d36"), 1); c.Disc(12f, 2.5f, 2f, C("aab2b4"));
            c.Disc(4f, 14f, 1.2f, C("aab2b4")); c.Disc(12f, 14f, 1.2f, C("aab2b4"));
            Add(c, Center, "job_dokter");

            // ---- panah kombo (atas = 0, kanan = 1, bawah = 2, kiri = 3)
            string[] dirs = { "up", "right", "down", "left" };
            for (int d = 0; d < 4; d++)
            {
                c = Icon();
                Outlined(c, 0, 0, 16, 16, C("3a2f4a"), ink);
                for (int y = 0; y < 16; y++)
                    for (int x = 0; x < 16; x++)
                    {
                        // panah menghadap ke atas, lalu diputar
                        int u = x, v = y;
                        switch (d) { case 1: u = 15 - y; v = x; break; case 2: u = 15 - x; v = 15 - y; break; case 3: u = y; v = 15 - x; break; }
                        bool head = v >= 8 && v <= 13 && Mathf.Abs(u - 7.5f) <= (13 - v) + 0.5f;
                        bool shaft = v >= 3 && v < 8 && u >= 6 && u <= 9;
                        if (head || shaft) c.Set(x, y, C("fff1c0"));
                    }
                Add(c, Center, "arrow_" + dirs[d]);
            }

            // ---- bidikan
            c = new PixelCanvas(24, 24);
            Color32 red = C("ff4a4a");
            for (int i = 0; i < 24; i++)
            {
                float a = i / 24f * Mathf.PI * 2f;
                c.Set(12 + Mathf.RoundToInt(Mathf.Cos(a) * 10f), 12 + Mathf.RoundToInt(Mathf.Sin(a) * 10f), red);
            }
            c.Rect(11, 0, 2, 7, red); c.Rect(11, 17, 2, 7, red); c.Rect(0, 11, 7, 2, red); c.Rect(17, 11, 7, 2, red);
            c.Rect(11, 11, 2, 2, red);
            Add(c, Center, "crosshair");
        }
    }
}
