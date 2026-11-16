using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Fasad khusus tiap jenis bangunan agar mudah dikenali: sekolah, rumah sakit, perpustakaan,
    /// bengkel, balai desa, kantor, toko baju, minimarket, toko bunga, toko buku, toko elektronik,
    /// dealer kendaraan, toko kelontong (2 warna), toko pancing, kost; serta ruko & mushola pengisi kota.
    /// Pivot tengah bawah, 16 px = 1 unit.
    /// </summary>
    public static partial class LifeArt
    {
        static readonly Color32 FInk = new Color32(0x2c, 0x25, 0x32, 255);
        static readonly Color32 FGlass = new Color32(0x9f, 0xd8, 0xe8, 255);
        static readonly Color32 FGlassHi = new Color32(0xd8, 0xf4, 0xff, 255);
        static readonly Color32 FDoor = new Color32(0x6b, 0x41, 0x26, 255);

        static void BuildFacadeArt()
        {
            School(); Hospital(); Library(); Workshop(); Balai(); Office();
            ClothesShop(); Minimarket(); FlowerShop(); BookShop(); Electronics(); Dealer();
            Grocery("bld_grocery_a", "3b6fb6", "e9dcc0"); Grocery("bld_grocery_b", "2e8f62", "efe2c4");
            FishingShop(); Kost(); Ruko(); Mushola();
        }

        // ------------------------------------------------------------------ util kecil

        static void Body(PixelCanvas c, int x, int w, int h, Color32 wall)
        {
            Outlined(c, x, 0, w, h, wall, FInk);
            c.Rect(x + 2, 0, 3, h - 1, Color32.Lerp(wall, FInk, 0.15f));
            c.Rect(x, 0, w, 3, Color32.Lerp(wall, FInk, 0.25f));     // pondasi
        }

        static void FlatRoof(PixelCanvas c, int x, int w, int y, Color32 col)
        {
            Outlined(c, x - 2, y, w + 4, 6, col, FInk);
            c.Rect(x - 1, y + 4, w + 2, 1, Color32.Lerp(col, Color.white, 0.25f));
        }

        static void Gable(PixelCanvas c, int cx, int halfW, int y, int h, Color32 roof, Color32 dark)
        {
            for (int r = 0; r < h; r++)
            {
                int half = Mathf.RoundToInt(Mathf.Lerp(halfW, 3, r / (float)h));
                c.Rect(cx - half, y + r, half * 2, 1, r % 3 == 0 ? dark : roof);
            }
            c.Rect(cx - halfW - 1, y, halfW * 2 + 2, 2, dark);
        }

        static void Awning(PixelCanvas c, int x, int w, int y, Color32 a, Color32 b)
        {
            for (int i = 0; i < w; i++) c.Rect(x + i, y, 1, 6, ((i / 6) % 2 == 0) ? a : b);
            for (int i = 0; i < w; i += 6) c.Rect(x + i + 1, y - 1, 4, 1, ((i / 6) % 2 == 0) ? a : b);  // tepi bergelombang
            c.Rect(x, y + 6, w, 1, FInk);
        }

        static void GlassDoor(PixelCanvas c, int x, int w, int h)
        {
            Outlined(c, x, 0, w, h, FGlass, FInk);
            c.Rect(x + w / 2, 1, 1, h - 2, FInk);
            c.Rect(x + 2, h - 6, 3, 4, FGlassHi);
        }

        static void Display(PixelCanvas c, int x, int y, int w, int h)
        {
            Outlined(c, x, y, w, h, FGlass, FInk);
            c.Rect(x + 2, y + h - 5, 4, 3, FGlassHi);
        }

        // ------------------------------------------------------------------ bangunan umum

        /// <summary>Sekolah: gedung panjang putih, atap genteng merah, jendela berderet, tiang bendera merah putih.</summary>
        static void School()
        {
            var c = new PixelCanvas(128, 90);
            Body(c, 8, 112, 54, PixelArt.Hex("f2efe6"));
            Gable(c, 64, 62, 54, 22, PixelArt.Hex("c85c4a"), PixelArt.Hex("963a34"));
            c.Rect(8, 44, 112, 3, PixelArt.Hex("6f8fd6"));                     // lis biru
            for (int i = 0; i < 6; i++) if (i != 3) Window(c, 14 + i * 17, 18, 12, 16, FGlass, FGlassHi, FInk, PixelArt.Hex("6f8fd6"));
            Outlined(c, 58, 0, 14, 30, FDoor, FInk);                           // pintu kelas
            c.Rect(64, 2, 1, 26, FInk);
            Outlined(c, 46, 48, 36, 8, PixelArt.Hex("2e5590"), FInk);          // papan nama sekolah
            c.Rect(48, 51, 32, 2, PixelArt.Hex("f2efe6"));
            c.Rect(3, 0, 2, 86, PixelArt.Hex("9aa2ab"));                       // tiang bendera
            c.Rect(5, 78, 12, 4, PixelArt.Hex("d8384a")); c.Rect(5, 74, 12, 4, PixelArt.Hex("ffffff"));
            for (int x = 8; x < 120; x += 6) c.Rect(x, 0, 2, 6, PixelArt.Hex("4a6a8a"));   // pagar depan
            c.Rect(8, 5, 112, 1, PixelArt.Hex("4a6a8a"));
            c.Rect(56, 0, 18, 6, new Color32(0, 0, 0, 0));                     // gerbang terbuka
            Add(c, BottomCenter, "bld_school");
        }

        /// <summary>Rumah sakit: putih kehijauan, palang merah besar, pintu kaca geser, garis hijau.</summary>
        static void Hospital()
        {
            var c = new PixelCanvas(104, 96);
            Body(c, 4, 96, 82, PixelArt.Hex("f4f7f4"));
            FlatRoof(c, 4, 96, 82, PixelArt.Hex("7fb8a8"));
            c.Rect(4, 40, 96, 4, PixelArt.Hex("4fae8a"));
            for (int i = 0; i < 4; i++) { Window(c, 10 + i * 24, 52, 14, 14, FGlass, FGlassHi, FInk, PixelArt.Hex("c8d8d0")); }
            for (int i = 0; i < 4; i++) if (i != 1 && i != 2) Window(c, 10 + i * 24, 14, 14, 14, FGlass, FGlassHi, FInk, PixelArt.Hex("c8d8d0"));
            Outlined(c, 42, 66, 20, 20, PixelArt.Hex("ffffff"), FInk);         // palang merah
            c.Rect(49, 68, 6, 16, PixelArt.Hex("e0383c")); c.Rect(44, 73, 16, 6, PixelArt.Hex("e0383c"));
            GlassDoor(c, 38, 28, 30);
            Outlined(c, 34, 30, 36, 6, PixelArt.Hex("4fae8a"), FInk);          // kanopi pintu
            Add(c, BottomCenter, "bld_hospital");
        }

        /// <summary>Perpustakaan: gaya klasik dengan pilar, segitiga atap, ikon buku terbuka.</summary>
        static void Library()
        {
            var c = new PixelCanvas(104, 88);
            Body(c, 6, 92, 56, PixelArt.Hex("e9dcc0"));
            Gable(c, 52, 50, 56, 22, PixelArt.Hex("b8a07a"), PixelArt.Hex("8a7450"));
            Outlined(c, 40, 62, 24, 12, PixelArt.Hex("f4efe0"), FInk);         // ikon buku
            c.Rect(51, 63, 2, 10, FInk);
            c.Rect(43, 69, 6, 1, PixelArt.Hex("8a9098")); c.Rect(55, 69, 6, 1, PixelArt.Hex("8a9098"));
            for (int i = 0; i < 5; i++)                                          // pilar
            {
                int px = 12 + i * 20;
                c.Rect(px, 4, 6, 46, PixelArt.Hex("f8f2e2"));
                c.Rect(px, 4, 1, 46, PixelArt.Hex("cfc2a0"));
                c.Rect(px - 1, 48, 8, 3, PixelArt.Hex("cfc2a0"));
            }
            Outlined(c, 44, 0, 16, 30, FDoor, FInk);
            for (int i = 0; i < 2; i++) Window(c, 22 + i * 46, 18, 12, 18, FGlass, FGlassHi, FInk, PixelArt.Hex("8a7450"));
            c.Rect(6, 0, 92, 4, PixelArt.Hex("cfc2a0"));                        // anak tangga
            Add(c, BottomCenter, "bld_library");
        }

        /// <summary>Bengkel: garasi terbuka berisi motor, tumpukan ban, drum oli, papan kunci pas.</summary>
        static void Workshop()
        {
            var c = new PixelCanvas(104, 80);
            Body(c, 4, 96, 56, PixelArt.Hex("d9cfbd"));
            FlatRoof(c, 4, 96, 56, PixelArt.Hex("5b5f66"));
            Outlined(c, 14, 0, 56, 42, PixelArt.Hex("3a3640"), FInk);           // mulut garasi gelap
            for (int y = 34; y < 41; y += 3) c.Rect(15, y, 54, 1, PixelArt.Hex("6b737c"));   // rolling door terangkat
            c.Disc(30f, 7f, 5f, FInk); c.Disc(54f, 7f, 5f, FInk);               // motor di dalam
            c.Line(31, 8, 42, 16, PixelArt.Hex("d8584a"), 4); c.Line(42, 16, 53, 8, PixelArt.Hex("d8584a"), 3);
            for (int i = 0; i < 3; i++) { c.Disc(82f, 4f + i * 7f, 5f, FInk); c.Disc(82f, 4f + i * 7f, 2f, PixelArt.Hex("5b5f66")); }   // ban
            Outlined(c, 90, 0, 8, 14, PixelArt.Hex("3b6fb6"), FInk);            // drum oli
            Outlined(c, 28, 46, 44, 10, PixelArt.Hex("e8b23a"), FInk);          // papan bengkel
            c.Line(40, 48, 48, 54, FInk, 2); c.Disc(39f, 48f, 2f, FInk); c.Disc(49f, 54f, 2f, FInk);   // ikon kunci pas
            c.Rect(54, 50, 14, 2, FInk);
            Add(c, BottomCenter, "bld_workshop");
        }

        /// <summary>Balai desa: atap joglo bertingkat cokelat, pendopo berpilar kayu.</summary>
        static void Balai()
        {
            var c = new PixelCanvas(104, 84);
            c.Rect(4, 0, 96, 5, PixelArt.Hex("b8a07a"));                         // lantai pendopo
            for (int i = 0; i < 5; i++) Outlined(c, 10 + i * 20, 4, 5, 34, PixelArt.Hex("8a5a3c"), FInk);   // tiang kayu
            Outlined(c, 30, 4, 44, 26, PixelArt.Hex("efe2c4"), FInk);            // ruang tengah
            Outlined(c, 46, 4, 12, 22, FDoor, FInk);
            for (int t = 0; t < 3; t++)                                          // atap joglo 3 tingkat
            {
                int y = 36 + t * 14, half = 52 - t * 14;
                for (int r = 0; r < 14; r++)
                {
                    int h2 = Mathf.RoundToInt(Mathf.Lerp(half, half - 10, r / 13f));
                    c.Rect(52 - h2, y + r, h2 * 2, 1, r % 3 == 0 ? PixelArt.Hex("5e3826") : PixelArt.Hex("8a5a3c"));
                }
            }
            c.Rect(48, 78, 8, 4, PixelArt.Hex("5e3826"));
            Add(c, BottomCenter, "bld_balai");
        }

        /// <summary>Kantor PT: menara kaca biru bergaris, logo di atas pintu.</summary>
        static void Office()
        {
            var c = new PixelCanvas(72, 112);
            Outlined(c, 4, 0, 64, 104, PixelArt.Hex("4a6a8a"), FInk);
            for (int y = 14; y < 98; y += 10)
                for (int x = 8; x < 64; x += 10)
                    c.Rect(x, y, 8, 7, ((x + y) / 10) % 4 == 0 ? FGlassHi : FGlass);
            Outlined(c, 0, 100, 72, 8, PixelArt.Hex("2f4a66"), FInk);
            c.Disc(36f, 96f, 4f, PixelArt.Hex("e8b23a"));                        // logo
            GlassDoor(c, 26, 20, 12);
            Add(c, BottomCenter, "bld_office");
        }

        // ------------------------------------------------------------------ toko

        /// <summary>Toko baju: etalase dengan manekin & baju di gantungan, kanopi pink.</summary>
        static void ClothesShop()
        {
            var c = new PixelCanvas(104, 88);
            Body(c, 4, 96, 62, PixelArt.Hex("fbe8ee"));
            FlatRoof(c, 4, 96, 62, PixelArt.Hex("d86a8a"));
            Awning(c, 6, 92, 44, PixelArt.Hex("ff9fc0"), PixelArt.Hex("ffffff"));
            Display(c, 8, 4, 32, 36); Display(c, 64, 4, 32, 36);
            // manekin
            c.Disc(24f, 30f, 3f, PixelArt.Hex("f2c49b")); Outlined(c, 19, 12, 10, 15, PixelArt.Hex("d8384a"), FInk); c.Rect(23, 4, 2, 8, FInk);
            // baju di gantungan
            c.Rect(68, 32, 24, 1, FInk);
            Color32[] cl = { PixelArt.Hex("3b6fb6"), PixelArt.Hex("e8b23a"), PixelArt.Hex("4fae62"), PixelArt.Hex("8a63ad") };
            for (int i = 0; i < 4; i++) Outlined(c, 69 + i * 6, 18, 5, 13, cl[i], FInk);
            GlassDoor(c, 44, 16, 30);
            c.Disc(52f, 56f, 4f, PixelArt.Hex("ff9fc0")); c.Rect(46, 54, 12, 1, FInk);   // ikon kaus
            Add(c, BottomCenter, "bld_clothes");
        }

        /// <summary>Minimarket: kaca lebar, papan bergaris merah-biru-kuning, rak terlihat dari luar.</summary>
        static void Minimarket()
        {
            var c = new PixelCanvas(104, 84);
            Body(c, 4, 96, 58, PixelArt.Hex("f4f4f4"));
            Outlined(c, 2, 46, 100, 14, PixelArt.Hex("ffffff"), FInk);          // papan bergaris
            c.Rect(3, 55, 98, 4, PixelArt.Hex("e0383c")); c.Rect(3, 51, 98, 4, PixelArt.Hex("3b6fb6")); c.Rect(3, 47, 98, 4, PixelArt.Hex("ffd23f"));
            Display(c, 8, 4, 84, 38);
            for (int r = 0; r < 3; r++) for (int i = 0; i < 12; i++)            // rak berisi barang
                c.Rect(12 + i * 6, 8 + r * 10, 4, 6, r == 0 ? PixelArt.Hex("e46f5a") : r == 1 ? PixelArt.Hex("4fae62") : PixelArt.Hex("ffd23f"));
            GlassDoor(c, 40, 22, 32);
            Add(c, BottomCenter, "bld_minimarket");
        }

        /// <summary>Toko bunga: kanopi hijau, ember bunga warna-warni di depan, jendela berbunga.</summary>
        static void FlowerShop()
        {
            var c = new PixelCanvas(104, 84);
            Body(c, 4, 96, 58, PixelArt.Hex("f0f4e8"));
            FlatRoof(c, 4, 96, 58, PixelArt.Hex("4f8a4a"));
            Awning(c, 6, 92, 42, PixelArt.Hex("6fae62"), PixelArt.Hex("f4efe0"));
            Display(c, 10, 14, 26, 24); Display(c, 68, 14, 26, 24);
            Outlined(c, 44, 0, 16, 32, PixelArt.Hex("4f8a4a"), FInk);
            Color32[] fl = { PixelArt.Hex("ff6b8a"), PixelArt.Hex("ffd23f"), PixelArt.Hex("ffffff"), PixelArt.Hex("c79ad8"), PixelArt.Hex("ff9a4a") };
            for (int i = 0; i < 10; i++)                                         // ember bunga di depan
            {
                int bx = i < 5 ? 8 + i * 7 : 62 + (i - 5) * 7;
                Outlined(c, bx, 0, 6, 7, PixelArt.Hex("8a9098"), FInk);
                c.Disc(bx + 3f, 10f, 3f, PixelArt.Hex("3a8a4c"));
                c.Disc(bx + 2f, 11f, 1.6f, fl[i % fl.Length]); c.Disc(bx + 4.5f, 12f, 1.4f, fl[(i + 2) % fl.Length]);
            }
            for (int i = 0; i < 6; i++) c.Disc(14f + i * 4f, 20f, 1.6f, fl[i % fl.Length]);
            Add(c, BottomCenter, "bld_flower");
        }

        /// <summary>Toko buku: rak buku terlihat dari etalase, papan bergambar buku, kanopi biru tua.</summary>
        static void BookShop()
        {
            var c = new PixelCanvas(104, 84);
            Body(c, 4, 96, 58, PixelArt.Hex("efe2c4"));
            FlatRoof(c, 4, 96, 58, PixelArt.Hex("2e5590"));
            Awning(c, 6, 92, 44, PixelArt.Hex("3b6fb6"), PixelArt.Hex("efe2c4"));
            Display(c, 8, 4, 32, 36); Display(c, 64, 4, 32, 36);
            Color32[] bk = { PixelArt.Hex("b8503f"), PixelArt.Hex("3b6fb6"), PixelArt.Hex("2e8f62"), PixelArt.Hex("e8b23a"), PixelArt.Hex("8a63ad") };
            for (int s = 0; s < 2; s++) for (int r = 0; r < 3; r++) for (int i = 0; i < 9; i++)
                c.Rect(10 + s * 56 + i * 3, 7 + r * 11, 2, 8 + (i % 2), bk[(i + r) % bk.Length]);
            Outlined(c, 44, 0, 16, 32, FDoor, FInk);
            Outlined(c, 42, 50, 20, 10, PixelArt.Hex("f4efe0"), FInk); c.Rect(51, 51, 2, 8, FInk);   // ikon buku
            Add(c, BottomCenter, "bld_books");
        }

        /// <summary>Toko elektronik: TV menyala di etalase, antena parabola di atap, papan biru.</summary>
        static void Electronics()
        {
            var c = new PixelCanvas(104, 90);
            Body(c, 4, 96, 58, PixelArt.Hex("e6e8ee"));
            FlatRoof(c, 4, 96, 58, PixelArt.Hex("2a2d36"));
            c.Disc(84f, 74f, 7f, PixelArt.Hex("c8ccd4")); c.Disc(84f, 74f, 4f, PixelArt.Hex("9aa2ab")); c.Rect(83, 64, 2, 6, FInk);   // parabola
            Outlined(c, 6, 44, 92, 12, PixelArt.Hex("2e5590"), FInk);
            c.Rect(10, 48, 20, 4, PixelArt.Hex("6fb7ff")); c.Rect(74, 48, 20, 4, PixelArt.Hex("ffd23f"));
            Display(c, 8, 4, 34, 36); Display(c, 62, 4, 34, 36);
            for (int i = 0; i < 2; i++) for (int k = 0; k < 2; k++)              // TV menyala
            {
                int tx = 11 + k * 54 + i * 15, ty = 8 + (i % 2) * 14;
                Outlined(c, tx, ty, 13, 10, FInk, FInk);
                c.Rect(tx + 1, ty + 1, 11, 8, (i + k) % 2 == 0 ? PixelArt.Hex("6fb7ff") : PixelArt.Hex("7fd486"));
            }
            GlassDoor(c, 44, 16, 32);
            Add(c, BottomCenter, "bld_electronics");
        }

        /// <summary>Dealer kendaraan: ruang pamer kaca lebar dengan motor & sepeda, papan merah.</summary>
        static void Dealer()
        {
            var c = new PixelCanvas(112, 80);
            Body(c, 4, 104, 54, PixelArt.Hex("f2f2f2"));
            Outlined(c, 2, 44, 108, 12, PixelArt.Hex("d8384a"), FInk);
            c.Rect(8, 48, 30, 4, PixelArt.Hex("ffffff")); c.Rect(74, 48, 30, 4, PixelArt.Hex("ffffff"));
            Display(c, 8, 4, 96, 36);
            // motor
            c.Disc(22f, 10f, 5f, FInk); c.Disc(42f, 10f, 5f, FInk);
            c.Line(23, 11, 32, 18, PixelArt.Hex("3b6fb6"), 4); c.Line(32, 18, 41, 11, PixelArt.Hex("3b6fb6"), 3);
            c.Rect(28, 20, 8, 2, FInk);
            // sepeda
            c.Disc(66f, 10f, 5f, FInk); c.Disc(66f, 10f, 3.5f, FGlass); c.Disc(86f, 10f, 5f, FInk); c.Disc(86f, 10f, 3.5f, FGlass);
            c.Line(66, 10, 76, 18, PixelArt.Hex("e8b23a"), 2); c.Line(76, 18, 86, 10, PixelArt.Hex("e8b23a"), 2); c.Line(72, 18, 80, 18, PixelArt.Hex("e8b23a"), 2);
            GlassDoor(c, 48, 14, 30);
            Add(c, BottomCenter, "bld_dealer");
        }

        /// <summary>Toko kelontong: karung beras, galon, krat minuman di depan, spanduk berwarna.</summary>
        static void Grocery(string key, string signHex, string wallHex)
        {
            var c = new PixelCanvas(104, 84);
            Body(c, 4, 96, 58, PixelArt.Hex(wallHex));
            FlatRoof(c, 4, 96, 58, PixelArt.Hex("8a8f96"));
            Outlined(c, 8, 46, 88, 10, PixelArt.Hex(signHex), FInk);             // spanduk
            c.Rect(12, 50, 28, 2, PixelArt.Hex("ffffff")); c.Rect(64, 50, 28, 2, PixelArt.Hex("ffd23f"));
            Outlined(c, 10, 6, 80, 36, PixelArt.Hex("3a3640"), FInk);             // toko terbuka (rak di dalam)
            for (int r = 0; r < 3; r++) for (int i = 0; i < 12; i++)
                c.Rect(14 + i * 6, 12 + r * 9, 4, 5, (i + r) % 3 == 0 ? PixelArt.Hex("e46f5a") : (i + r) % 3 == 1 ? PixelArt.Hex("ffd23f") : PixelArt.Hex("6fae62"));
            for (int i = 0; i < 3; i++) Outlined(c, 8 + i * 9, 0, 8, 10, PixelArt.Hex("efe2c4"), FInk);   // karung beras
            for (int i = 0; i < 2; i++) { Outlined(c, 78 + i * 9, 0, 7, 12, PixelArt.Hex("9fd8e8"), FInk); c.Rect(80 + i * 9, 12, 3, 2, PixelArt.Hex("3b6fb6")); }   // galon
            Outlined(c, 40, 0, 22, 8, PixelArt.Hex("e0383c"), FInk);              // krat minuman
            Add(c, BottomCenter, key);
        }

        /// <summary>Toko pancing: pondok kayu, jaring tergantung, joran bersandar, papan ikan.</summary>
        static void FishingShop()
        {
            var c = new PixelCanvas(104, 84);
            Body(c, 4, 96, 54, PixelArt.Hex("c8925e"));
            for (int x = 6; x < 100; x += 6) c.Rect(x, 3, 1, 50, PixelArt.Hex("a8754a"));   // papan kayu
            Gable(c, 52, 52, 54, 20, PixelArt.Hex("3b6fb6"), PixelArt.Hex("2a5590"));
            Outlined(c, 44, 0, 16, 32, FDoor, FInk);
            Window(c, 14, 18, 18, 16, FGlass, FGlassHi, FInk, PixelArt.Hex("8a5a3c"));
            for (int y = 18; y < 40; y += 4) for (int x = 66; x < 92; x += 4) { c.Set(x, y, PixelArt.Hex("efe2c4")); c.Set(x + 2, y + 2, PixelArt.Hex("efe2c4")); }   // jaring
            for (int i = 0; i < 3; i++) c.Line(96 + i * 2, 0, 90 + i * 3, 46, PixelArt.Hex("5e3826"), 1);   // joran
            c.Ellipse(52f, 44f, 8f, 3f, PixelArt.Hex("6fb7ff")); c.Triangle(new Vector2(43f, 44f), new Vector2(40f, 47f), new Vector2(40f, 41f), PixelArt.Hex("6fb7ff"));   // papan ikan
            Add(c, BottomCenter, "bld_fishing");
        }

        /// <summary>Kost dua lantai: banyak pintu kamar, tangga samping, jemuran di balkon.</summary>
        static void Kost()
        {
            var c = new PixelCanvas(112, 100);
            Body(c, 6, 100, 72, PixelArt.Hex("f6e7c8"));
            Gable(c, 56, 56, 72, 24, PixelArt.Hex("6f8fd6"), PixelArt.Hex("4c6bb0"));
            c.Rect(6, 36, 100, 4, PixelArt.Hex("8a5a3c"));                       // balkon
            for (int x = 8; x < 104; x += 5) c.Rect(x, 40, 1, 7, PixelArt.Hex("8a5a3c"));
            c.Rect(6, 46, 100, 1, PixelArt.Hex("8a5a3c"));
            for (int i = 0; i < 4; i++)                                           // pintu kamar 2 lantai
            {
                Outlined(c, 14 + i * 24, 40, 10, 22, PixelArt.Hex("6fae62"), FInk);
                Window(c, 26 + i * 24, 48, 7, 8, FGlass, FGlassHi, FInk, PixelArt.Hex("8a5a3c"));
                if (i != 1) Outlined(c, 14 + i * 24, 0, 10, 24, PixelArt.Hex("6fae62"), FInk);
            }
            Outlined(c, 36, 0, 16, 28, FDoor, FInk);                              // pintu utama
            c.Rect(56, 60, 30, 1, FInk);                                          // jemuran
            Color32[] cl = { PixelArt.Hex("e46f5a"), PixelArt.Hex("ffffff"), PixelArt.Hex("6fb7ff") };
            for (int i = 0; i < 3; i++) Outlined(c, 58 + i * 9, 52, 6, 8, cl[i], FInk);
            Add(c, BottomCenter, "bld_kost");
        }

        // ------------------------------------------------------------------ pengisi kota

        /// <summary>Ruko dua lantai: lantai bawah rolling door + papan, lantai atas jendela berjeruji.</summary>
        static void Ruko()
        {
            var c = new PixelCanvas(104, 96);
            Body(c, 4, 96, 80, PixelArt.Hex("e6dccb"));
            FlatRoof(c, 4, 96, 80, PixelArt.Hex("9a8a7a"));
            c.Rect(52, 3, 2, 77, FInk);                                           // sekat dua unit
            for (int u = 0; u < 2; u++)
            {
                int ux = 8 + u * 48;
                Outlined(c, ux, 0, 40, 34, PixelArt.Hex("9aa2ab"), FInk);         // rolling door
                for (int y = 3; y < 33; y += 4) c.Rect(ux + 1, y, 38, 1, PixelArt.Hex("6b737c"));
                Outlined(c, ux, 36, 40, 9, u == 0 ? PixelArt.Hex("d8584a") : PixelArt.Hex("3b6fb6"), FInk);   // papan usaha
                c.Rect(ux + 4, 39, 32, 2, PixelArt.Hex("ffffff"));
                for (int w = 0; w < 2; w++)
                {
                    Window(c, ux + 4 + w * 18, 52, 14, 18, FGlass, FGlassHi, FInk, PixelArt.Hex("8a8f96"));
                    for (int b = 0; b < 4; b++) c.Rect(ux + 6 + w * 18 + b * 3, 52, 1, 18, FInk);   // teralis
                }
            }
            Add(c, BottomCenter, "ruko");
        }

        /// <summary>Mushola: bangunan putih-hijau, kubah hijau berbulan sabit, menara kecil, pintu lengkung.</summary>
        static void Mushola()
        {
            var c = new PixelCanvas(96, 104);
            Body(c, 14, 68, 48, PixelArt.Hex("f4f7f0"));
            c.Rect(14, 44, 68, 4, PixelArt.Hex("2e8f62"));
            c.Ellipse(48f, 62f, 22f, 16f, PixelArt.Hex("2e8f62"));                // kubah
            c.Ellipse(44f, 66f, 10f, 6f, PixelArt.Hex("4fae7a"));
            c.Rect(47, 76, 2, 8, PixelArt.Hex("e8b23a"));
            c.Disc(48f, 87f, 3.5f, PixelArt.Hex("e8b23a")); c.Disc(49.5f, 88f, 3f, new Color32(0, 0, 0, 0));   // bulan sabit
            Outlined(c, 84, 0, 8, 72, PixelArt.Hex("f4f7f0"), FInk);               // menara
            c.Rect(84, 60, 8, 4, PixelArt.Hex("2e8f62"));
            c.Triangle(new Vector2(88f, 84f), new Vector2(83f, 72f), new Vector2(93f, 72f), PixelArt.Hex("2e8f62"));
            Outlined(c, 40, 0, 16, 26, PixelArt.Hex("2e8f62"), FInk);              // pintu lengkung
            c.Disc(48f, 26f, 8f, PixelArt.Hex("2e8f62"));
            c.Rect(47, 2, 2, 22, FInk);
            for (int i = 0; i < 2; i++) { Outlined(c, 20 + i * 44, 14, 12, 18, FGlass, FInk); c.Disc(26f + i * 44f, 32f, 6f, FGlass); }   // jendela lengkung
            c.Rect(10, 0, 76, 3, PixelArt.Hex("cfc8b8"));                          // teras
            Add(c, BottomCenter, "mushola");
        }
    }
}
