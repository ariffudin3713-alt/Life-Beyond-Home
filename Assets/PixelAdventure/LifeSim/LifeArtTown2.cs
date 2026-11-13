using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Bangunan & hiasan pengisi lahan kosong (lihat LifeWorldFill):
    /// apotek, pangkas rambut, laundry, warnet, toko roti, bank, kantor pos, pos polisi, ATM,
    /// pangkalan ojek, rumah joglo, lumbung, puskesmas, SD, kandang sapi, menara air,
    /// rumah makan Padang, toko emas, gudang, resto seafood, mercusuar, pos penjaga pantai, toko tani;
    /// serta pot tanaman, hidran, papan reklame, jerami, orang-orangan sawah, perahu, parkiran motor, kolam ikan.
    /// Pivot tengah bawah, 16 px = 1 unit.
    /// </summary>
    public static partial class LifeArt
    {
        static void BuildTownArt2()
        {
            Apotek(); Barber(); Laundry(); Warnet(); Bakery(); Bank(); PostOffice(); PolicePost(); AtmBooth();
            OjekBase(); Joglo(); Lumbung(); Puskesmas(); SdSchool(); CowShed(); WaterTower(); Padang();
            GoldShop(); Warehouse(); Seafood(); Lighthouse(); Lifeguard(); FarmShop();
            Planter(); Hydrant(); Billboard(); Haystack(); Scarecrow(); Boat(); ParkedMotors(); FishPond();
        }

        static Color32 H(string hex) => PixelArt.Hex(hex);

        /// <summary>Papan nama polos di dinding (teks asli memakai papan nama dunia).</summary>
        static void SignBoard(PixelCanvas c, int x, int y, int w, int h, Color32 col)
        {
            Outlined(c, x, y, w, h, col, FInk);
            c.Rect(x + 2, y + h / 2, w - 4, 1, Color32.Lerp(col, Color.white, 0.55f));
        }

        // ================================================================== toko sempit (4 unit)

        static void Apotek()
        {
            var c = new PixelCanvas(64, 80);
            Body(c, 2, 60, 60, H("f4f7f4"));
            FlatRoof(c, 2, 60, 60, H("4fae8a"));
            Outlined(c, 22, 66, 20, 12, H("ffffff"), FInk);              // palang hijau di atap
            c.Rect(30, 67, 4, 10, H("2e9f5a")); c.Rect(25, 70, 14, 4, H("2e9f5a"));
            SignBoard(c, 6, 40, 52, 10, H("2e9f5a"));
            Display(c, 6, 10, 18, 24);
            c.Rect(9, 14, 12, 2, H("e0383c")); c.Rect(9, 20, 12, 2, H("6fb7ff")); c.Rect(9, 26, 12, 2, H("ffd35c"));   // rak obat
            GlassDoor(c, 32, 22, 30);
            Add(c, BottomCenter, "bld_apotek");
        }

        static void Barber()
        {
            var c = new PixelCanvas(64, 76);
            Body(c, 2, 60, 58, H("6f9fd6"));
            FlatRoof(c, 2, 60, 58, H("3b5a8a"));
            SignBoard(c, 6, 40, 52, 10, H("f4f5f8"));
            c.Rect(10, 44, 44, 2, H("d8384a"));
            Display(c, 6, 8, 24, 26);
            c.Disc(18f, 22f, 4f, H("f2c5a0")); c.Rect(14, 23, 8, 3, H("3a2a2a"));   // pelanggan di cermin
            Outlined(c, 36, 0, 16, 32, H("f4f5f8"), FInk);                           // pintu
            c.Rect(38, 2, 12, 28, FGlass);
            Outlined(c, 55, 6, 6, 24, H("ffffff"), FInk);                            // tiang barber
            for (int y = 7; y < 29; y += 4) { c.Rect(56, y, 4, 1, H("d8384a")); c.Rect(56, y + 2, 4, 1, H("3b6fb6")); }
            c.Disc(58f, 31f, 3f, H("d8384a"));
            Add(c, BottomCenter, "bld_barber");
        }

        static void Laundry()
        {
            var c = new PixelCanvas(64, 76);
            Body(c, 2, 60, 58, H("dff1fb"));
            FlatRoof(c, 2, 60, 58, H("6fb7e0"));
            Awning(c, 4, 56, 36, H("6fb7e0"), H("ffffff"));
            SignBoard(c, 8, 46, 48, 9, H("3b8fd6"));
            Display(c, 6, 6, 30, 26);
            for (int i = 0; i < 2; i++)                                               // mesin cuci
            {
                Outlined(c, 9 + i * 13, 8, 11, 12, H("f4f5f8"), FInk);
                c.Disc(14.5f + i * 13, 13.5f, 3.6f, H("8a9098")); c.Disc(14.5f + i * 13, 13.5f, 2.4f, H("9fd8e8"));
            }
            c.Rect(9, 24, 24, 4, H("ff9fc0")); c.Rect(14, 24, 2, 4, H("ffd35c"));   // tumpukan baju
            GlassDoor(c, 40, 18, 30);
            Add(c, BottomCenter, "bld_laundry");
        }

        static void Warnet()
        {
            var c = new PixelCanvas(64, 76);
            Body(c, 2, 60, 58, H("4a3a6a"));
            FlatRoof(c, 2, 60, 58, H("2a2040"));
            SignBoard(c, 6, 40, 52, 11, H("2a2040"));
            c.Rect(10, 44, 44, 3, H("6ff0ff"));                                     // neon
            Display(c, 6, 8, 30, 26);
            for (int i = 0; i < 3; i++)                                               // monitor
            {
                Outlined(c, 9 + i * 9, 18, 8, 6, H("1a1424"), FInk);
                c.Rect(10 + i * 9, 19, 6, 4, i == 1 ? H("7fd486") : H("6fb7ff"));
            }
            Outlined(c, 40, 0, 16, 32, H("2a2040"), FInk);
            c.Rect(42, 2, 12, 26, FGlass);
            Add(c, BottomCenter, "bld_warnet");
        }

        static void Bakery()
        {
            var c = new PixelCanvas(64, 80);
            Body(c, 2, 60, 58, H("f6e7c8"));
            Gable(c, 32, 32, 58, 18, H("a8643a"), H("7a4428"));
            Awning(c, 4, 56, 36, H("a8643a"), H("f6e7c8"));
            SignBoard(c, 8, 46, 48, 9, H("7a4428"));
            Display(c, 6, 6, 28, 26);
            c.Ellipse(13f, 12f, 4f, 2f, H("e0a050")); c.Ellipse(22f, 12f, 4f, 2f, H("d89040"));   // roti
            c.Ellipse(17f, 20f, 5f, 2.4f, H("e8b860")); c.Rect(9, 9, 22, 1, H("8a5a3c"));
            Outlined(c, 40, 0, 16, 32, FDoor, FInk);
            c.Rect(42, 18, 12, 10, FGlass);
            Add(c, BottomCenter, "bld_bakery");
        }

        // ================================================================== bangunan kota

        static void Bank()
        {
            var c = new PixelCanvas(72, 88);
            Body(c, 2, 68, 64, H("e9e6de"));
            Gable(c, 36, 36, 64, 18, H("8a9098"), H("6b737c"));
            SignBoard(c, 14, 50, 44, 10, H("1f4a8a"));
            c.Rect(18, 53, 8, 4, H("ffd35c"));
            for (int i = 0; i < 4; i++)                                               // pilar
            {
                int px = 6 + i * 18;
                c.Rect(px, 4, 5, 44, H("f8f6f0")); c.Rect(px, 4, 1, 44, H("c8c4b8"));
                c.Rect(px - 1, 46, 7, 3, H("c8c4b8"));
            }
            GlassDoor(c, 28, 16, 30);
            Outlined(c, 54, 8, 8, 18, H("1f4a8a"), FInk);                             // mesin ATM
            c.Rect(55, 18, 6, 5, H("9fd8e8")); c.Rect(56, 12, 4, 2, H("ffd35c"));
            c.Rect(2, 0, 68, 4, H("c8c4b8"));                                          // tangga
            Add(c, BottomCenter, "bld_bank");
        }

        static void PostOffice()
        {
            var c = new PixelCanvas(72, 80);
            Body(c, 2, 68, 58, H("f4ece0"));
            FlatRoof(c, 2, 68, 58, H("e07a2a"));
            c.Rect(2, 36, 68, 4, H("e07a2a"));
            SignBoard(c, 10, 44, 52, 10, H("e07a2a"));
            c.Rect(14, 47, 6, 4, H("ffffff")); c.Line(14, 51, 17, 49, FInk); c.Line(17, 49, 20, 51, FInk);   // amplop
            Window(c, 6, 12, 16, 16, FGlass, FGlassHi, FInk, H("e07a2a"));
            Window(c, 50, 12, 16, 16, FGlass, FGlassHi, FInk, H("e07a2a"));
            Outlined(c, 28, 0, 16, 32, FDoor, FInk);
            c.Rect(35, 2, 1, 28, FInk);
            Add(c, BottomCenter, "bld_post");
        }

        static void PolicePost()
        {
            var c = new PixelCanvas(48, 64);
            Body(c, 2, 44, 42, H("f4f5f8"));
            FlatRoof(c, 2, 44, 42, H("2a3a6a"));
            c.Rect(2, 30, 44, 5, H("2a3a6a")); c.Rect(2, 32, 44, 1, H("ffd35c"));
            c.Rect(19, 52, 10, 4, H("e0383c")); c.Rect(29, 52, 4, 4, H("3b6fb6"));   // lampu rotator
            Window(c, 6, 12, 14, 14, FGlass, FGlassHi, FInk, H("2a3a6a"));
            Outlined(c, 26, 0, 14, 26, H("2a3a6a"), FInk);
            c.Rect(28, 12, 10, 10, FGlass);
            Add(c, BottomCenter, "bld_police");
        }

        static void AtmBooth()
        {
            var c = new PixelCanvas(40, 56);
            Body(c, 2, 36, 40, H("dfe6ee"));
            FlatRoof(c, 2, 36, 40, H("1f4a8a"));
            SignBoard(c, 6, 30, 28, 8, H("1f4a8a"));
            c.Rect(9, 33, 6, 3, H("ffd35c"));
            GlassDoor(c, 8, 24, 26);
            Outlined(c, 13, 6, 14, 16, H("1f4a8a"), FInk);
            c.Rect(15, 14, 10, 6, H("9fd8e8")); c.Rect(16, 9, 8, 2, H("ffd35c"));
            Add(c, BottomCenter, "bld_atm");
        }

        static void OjekBase()
        {
            var c = new PixelCanvas(48, 52);
            c.Rect(5, 0, 2, 30, H("6b4a2a")); c.Rect(41, 0, 2, 30, H("6b4a2a"));     // tiang
            Outlined(c, 1, 28, 46, 8, H("2e8f62"), FInk);                            // atap seng hijau
            for (int x = 3; x < 46; x += 4) c.Rect(x, 29, 1, 6, H("1f6a48"));
            SignBoard(c, 10, 38, 28, 8, H("ffd35c"));
            Outlined(c, 9, 4, 30, 4, H("8a5a3c"), FInk);                             // bangku kayu
            c.Rect(11, 0, 2, 4, H("6b4a2a")); c.Rect(35, 0, 2, 4, H("6b4a2a"));
            c.Disc(16f, 14f, 3f, H("2e8f62")); c.Rect(14, 9, 4, 4, H("2e8f62"));     // helm & jaket hijau
            c.Disc(30f, 13f, 3f, H("2e8f62"));
            Add(c, BottomCenter, "bld_ojek");
        }

        // ================================================================== desa & kampung

        static void Joglo()
        {
            var c = new PixelCanvas(96, 80);
            Color32 wood = H("8a5a3c"), woodDark = H("5e3a24"), roof = H("6b3a2a"), roofDark = H("4a2418");
            Body(c, 8, 80, 36, H("e9d8b8"));
            for (int i = 0; i < 6; i++) c.Rect(10 + i * 15, 3, 3, 32, wood);         // saka (tiang kayu)
            // atap joglo bertingkat: limasan bawah lebar + puncak tinggi
            for (int r = 0; r < 14; r++)
            {
                int half = 47 - r;
                c.Rect(48 - half, 36 + r, half * 2, 1, r % 3 == 0 ? roofDark : roof);
            }
            for (int r = 0; r < 22; r++)
            {
                int half = Mathf.RoundToInt(Mathf.Lerp(22, 6, r / 22f));
                c.Rect(48 - half, 50 + r, half * 2, 1, r % 3 == 0 ? roofDark : roof);
            }
            c.Rect(42, 72, 12, 2, roofDark);
            Outlined(c, 40, 0, 16, 26, woodDark, FInk);                               // pintu gebyok
            c.Rect(47, 2, 2, 22, wood);
            for (int i = 0; i < 2; i++) { Window(c, 18 + i * 50, 12, 12, 12, H("c8a46a"), H("e8d4a0"), FInk, wood); }
            c.Rect(4, 0, 88, 3, H("b8a07a"));                                         // pendopo / lantai
            Add(c, BottomCenter, "bld_joglo");
        }

        static void Lumbung()
        {
            var c = new PixelCanvas(64, 80);
            Color32 wood = H("8a5a3c"), thatch = H("c8a46a"), thatchDark = H("9a7a44");
            for (int i = 0; i < 4; i++) c.Rect(10 + i * 14, 0, 3, 18, H("5e3a24"));   // tiang panggung
            Outlined(c, 6, 16, 52, 4, wood, FInk);
            Outlined(c, 10, 20, 44, 22, H("b8865a"), FInk);                           // badan lumbung
            for (int x = 14; x < 52; x += 6) c.Rect(x, 21, 1, 20, H("8a5a3c"));
            Outlined(c, 26, 22, 12, 14, H("6b4a2a"), FInk);
            for (int r = 0; r < 34; r++)                                               // atap ijuk/jerami
            {
                int half = Mathf.RoundToInt(Mathf.Lerp(31, 2, r / 34f));
                c.Rect(32 - half, 42 + r, half * 2, 1, r % 4 == 0 ? thatchDark : thatch);
            }
            Add(c, BottomCenter, "bld_lumbung");
        }

        static void Puskesmas()
        {
            var c = new PixelCanvas(96, 80);
            Body(c, 4, 88, 50, H("f4f7f4"));
            Gable(c, 48, 48, 50, 20, H("4fae8a"), H("2e8f62"));
            c.Rect(4, 34, 88, 3, H("4fae8a"));
            SignBoard(c, 24, 38, 48, 10, H("2e8f62"));
            Outlined(c, 28, 40, 6, 6, H("ffffff"), FInk); c.Rect(30, 41, 2, 4, H("e0383c")); c.Rect(29, 42, 4, 2, H("e0383c"));
            for (int i = 0; i < 4; i++) if (i != 1 && i != 2) Window(c, 10 + i * 20, 12, 14, 14, FGlass, FGlassHi, FInk, H("c8d8d0"));
            GlassDoor(c, 36, 24, 30);
            Outlined(c, 32, 30, 32, 5, H("4fae8a"), FInk);
            Add(c, BottomCenter, "bld_puskesmas");
        }

        static void SdSchool()
        {
            var c = new PixelCanvas(112, 80);
            Body(c, 4, 104, 46, H("f2efe6"));
            Gable(c, 56, 56, 46, 20, H("c85c4a"), H("963a34"));
            c.Rect(4, 0, 104, 10, H("c85c4a"));                                       // dinding bawah merah
            c.Rect(4, 9, 104, 1, FInk);
            for (int i = 0; i < 5; i++)
            {
                if (i == 2) continue;
                Window(c, 10 + i * 20, 16, 12, 16, FGlass, FGlassHi, FInk, H("6f8fd6"));
            }
            Outlined(c, 50, 0, 12, 32, FDoor, FInk);
            SignBoard(c, 34, 36, 44, 8, H("2e5590"));
            Add(c, BottomCenter, "bld_sd");
        }

        static void CowShed()
        {
            var c = new PixelCanvas(80, 64);
            Color32 wood = H("8a5a3c");
            c.Rect(4, 0, 3, 34, wood); c.Rect(73, 0, 3, 34, wood); c.Rect(38, 0, 3, 34, wood);
            for (int r = 0; r < 16; r++) c.Rect(r / 2, 32 + r, 80 - r, 1, r % 3 == 0 ? H("6b737c") : H("9aa2ab"));   // atap seng
            for (int x = 6; x < 76; x += 2) c.Rect(x, 6, 1, 1, wood);
            c.Rect(4, 10, 72, 2, wood); c.Rect(4, 4, 72, 2, wood);                    // pagar
            // sapi
            Outlined(c, 12, 8, 18, 11, H("f4f0e8"), FInk);
            c.Rect(16, 12, 6, 4, H("2b2233")); c.Rect(24, 15, 4, 3, H("2b2233"));
            Outlined(c, 28, 14, 7, 7, H("f4f0e8"), FInk); c.Rect(33, 15, 2, 2, H("f2a0a0"));
            c.Rect(13, 2, 2, 6, FInk); c.Rect(26, 2, 2, 6, FInk);
            // jerami
            c.Ellipse(58f, 10f, 12f, 6f, H("e0c060")); c.Ellipse(58f, 12f, 9f, 4f, H("f0d880"));
            Add(c, BottomCenter, "bld_kandang");
        }

        static void WaterTower()
        {
            var c = new PixelCanvas(48, 112);
            Color32 steel = H("8a9098");
            c.Line(8, 0, 16, 64, steel, 2); c.Line(40, 0, 32, 64, steel, 2);
            c.Line(10, 16, 38, 40, steel, 1); c.Line(38, 16, 10, 40, steel, 1);
            c.Line(13, 40, 35, 60, steel, 1); c.Line(35, 40, 13, 60, steel, 1);
            c.Rect(10, 16, 28, 2, steel); c.Rect(13, 40, 22, 2, steel);
            Outlined(c, 8, 64, 32, 30, H("6fb7e0"), FInk);                           // tandon
            c.Rect(10, 66, 4, 26, H("9fd8f0"));
            c.Rect(8, 78, 32, 2, H("3b8fd6"));
            c.Triangle(new Vector2(24f, 106f), new Vector2(6f, 94f), new Vector2(42f, 94f), H("3b6fb6"));
            c.Rect(23, 32, 2, 32, H("6b737c"));                                       // pipa
            Add(c, BottomCenter, "bld_watertower");
        }

        // ================================================================== pasar & pantai

        static void Padang()
        {
            var c = new PixelCanvas(96, 88);
            Color32 red = H("b8322c"), gold = H("e8b23a"), roof = H("6b3a2a"), roofLine = H("4a2418");
            Body(c, 6, 84, 44, H("f6e7c8"));
            c.Rect(6, 40, 84, 4, red);
            for (int x = 8; x < 88; x += 6) c.Rect(x, 41, 3, 2, gold);              // ukiran
            // atap gonjong: tiga tanduk melengkung ke atas
            Outlined(c, 4, 44, 88, 10, roof, FInk);
            for (int g = 0; g < 3; g++)
            {
                int cx = 18 + g * 30;
                for (int r = 0; r < 28; r++)
                {
                    int half = Mathf.RoundToInt(Mathf.Lerp(14, 2, r / 28f));
                    int lean = g == 0 ? -r / 5 : g == 2 ? r / 5 : 0;
                    c.Rect(cx - half + lean, 54 + r, half * 2, 1, r % 4 == 0 ? roofLine : roof);
                }
            }
            SignBoard(c, 18, 28, 60, 10, red);
            c.Rect(22, 31, 52, 3, gold);
            Display(c, 10, 6, 26, 18);
            for (int i = 0; i < 3; i++)                                               // piring lauk bertumpuk
                for (int j = 0; j < 2; j++)
                    c.Ellipse(15f + i * 8, 10f + j * 7, 3f, 1.6f, j == 0 ? H("c8501e") : H("e8a030"));
            Outlined(c, 42, 0, 16, 26, FDoor, FInk);
            Display(c, 62, 6, 22, 18);
            Add(c, BottomCenter, "bld_padang");
        }

        static void GoldShop()
        {
            var c = new PixelCanvas(80, 80);
            Body(c, 4, 72, 56, H("c8322c"));
            FlatRoof(c, 4, 72, 56, H("8a1e1a"));
            SignBoard(c, 10, 38, 60, 12, H("e8b23a"));
            c.Rect(14, 42, 52, 4, H("fff1a0"));
            Display(c, 8, 6, 64, 26);
            for (int i = 0; i < 6; i++)                                               // kalung & gelang emas
            {
                c.Disc(14f + i * 10, 20f, 3f, H("e8b23a"));
                c.Disc(14f + i * 10, 20f, 1.6f, FGlass);
                c.Rect(12 + i * 10, 11, 5, 2, H("ffd35c"));
            }
            Add(c, BottomCenter, "bld_goldshop");
        }

        static void Warehouse()
        {
            var c = new PixelCanvas(96, 76);
            Body(c, 4, 88, 46, H("a8b0a8"));
            for (int x = 6; x < 92; x += 4) c.Rect(x, 3, 1, 42, H("8a9290"));       // seng bergelombang
            for (int r = 0; r < 20; r++)
            {
                int half = Mathf.RoundToInt(Mathf.Lerp(46, 20, r / 20f));
                c.Rect(48 - half, 46 + r, half * 2, 1, r % 3 == 0 ? H("6b737c") : H("8a9098"));
            }
            Outlined(c, 28, 0, 40, 34, H("9aa2ab"), FInk);                            // pintu geser
            for (int y = 3; y < 33; y += 4) c.Rect(29, y, 38, 1, H("6b737c"));
            for (int i = 0; i < 3; i++)                                               // karung beras
                Outlined(c, 8 + i * 6, 2, 6, 8, H("f0e8d0"), FInk);
            Outlined(c, 11, 9, 6, 7, H("f0e8d0"), FInk);
            SignBoard(c, 30, 36, 36, 8, H("3b6fb6"));
            Add(c, BottomCenter, "bld_warehouse");
        }

        static void Seafood()
        {
            var c = new PixelCanvas(96, 80);
            Color32 wood = H("a8784a"), dark = H("6b4a2a");
            for (int i = 0; i < 5; i++) c.Rect(6 + i * 20, 0, 3, 12, dark);           // panggung
            Outlined(c, 2, 10, 92, 4, dark, FInk);
            Outlined(c, 6, 14, 84, 30, wood, FInk);
            for (int y = 18; y < 44; y += 4) c.Rect(7, y, 82, 1, dark);
            for (int r = 0; r < 22; r++)                                              // atap rumbia
            {
                int half = Mathf.RoundToInt(Mathf.Lerp(48, 10, r / 22f));
                c.Rect(48 - half, 44 + r, half * 2, 1, r % 3 == 0 ? H("9a7a44") : H("c8a46a"));
            }
            SignBoard(c, 24, 34, 48, 9, H("3b8fd6"));
            c.Ellipse(36f, 38.5f, 6f, 2.4f, H("f08060")); c.Rect(42, 37, 3, 3, H("f08060"));   // ikan
            Outlined(c, 40, 14, 16, 18, dark, FInk);
            for (int i = 0; i < 2; i++) Window(c, 14 + i * 56, 20, 14, 10, FGlass, FGlassHi, FInk, dark);
            c.Line(2, 10, 0, 0, dark, 2);                                              // tangga
            Add(c, BottomCenter, "bld_seafood");
        }

        static void Lighthouse()
        {
            var c = new PixelCanvas(48, 128);
            Outlined(c, 6, 0, 36, 10, H("8a9098"), FInk);                              // dasar batu
            for (int y = 10; y < 92; y++)
            {
                int half = Mathf.RoundToInt(Mathf.Lerp(14, 9, (y - 10) / 82f));
                bool red = ((y - 10) / 14) % 2 == 0;
                c.Rect(24 - half - 1, y, half * 2 + 2, 1, FInk);
                c.Rect(24 - half, y, half * 2, 1, red ? H("d8384a") : H("f4f5f8"));
            }
            Outlined(c, 10, 92, 28, 4, H("2b2233"), FInk);                             // balkon
            Outlined(c, 14, 96, 20, 14, H("ffe08a"), FInk);                            // ruang lampu
            c.Rect(18, 98, 12, 10, H("fff6c0"));
            c.Triangle(new Vector2(24f, 124f), new Vector2(11f, 110f), new Vector2(37f, 110f), H("d8384a"));
            Outlined(c, 20, 10, 8, 14, FDoor, FInk);
            Add(c, BottomCenter, "bld_lighthouse");
        }

        static void Lifeguard()
        {
            var c = new PixelCanvas(48, 80);
            Color32 wood = H("a8784a");
            c.Line(6, 0, 12, 40, wood, 3); c.Line(42, 0, 36, 40, wood, 3);
            c.Line(9, 10, 39, 30, wood, 1); c.Line(39, 10, 9, 30, wood, 1);
            Outlined(c, 8, 40, 32, 20, H("f4f5f8"), FInk);
            c.Rect(9, 48, 30, 4, H("e0383c"));
            Outlined(c, 4, 60, 40, 6, H("e0383c"), FInk);                              // atap
            c.Rect(14, 42, 20, 5, FGlass);
            c.Rect(38, 60, 2, 16, H("8a9098")); c.Rect(40, 70, 7, 5, H("ffd35c"));  // bendera
            for (int y = 2; y < 40; y += 5) c.Rect(20, y, 8, 1, wood);                 // tangga
            Add(c, BottomCenter, "bld_lifeguard");
        }

        static void FarmShop()
        {
            var c = new PixelCanvas(64, 76);
            Body(c, 2, 60, 56, H("f0e6c8"));
            Gable(c, 32, 32, 56, 16, H("6fae62"), H("4a8a3f"));
            Awning(c, 4, 56, 34, H("4a8a3f"), H("f0e6c8"));
            SignBoard(c, 8, 44, 48, 9, H("4a8a3f"));
            for (int i = 0; i < 3; i++) Outlined(c, 6 + i * 8, 2, 7, 10, H("e8e0c8"), FInk);   // karung pupuk
            c.Rect(7, 7, 5, 2, H("6fae62")); c.Rect(15, 7, 5, 2, H("e0a050")); c.Rect(23, 7, 5, 2, H("6fae62"));
            Outlined(c, 36, 0, 18, 30, FDoor, FInk);
            c.Rect(38, 16, 14, 10, FGlass);
            c.Line(56, 0, 60, 26, H("8a9098"), 1); c.Rect(55, 0, 4, 2, H("8a9098"));   // cangkul
            Add(c, BottomCenter, "bld_tani");
        }

        // ================================================================== hiasan kecil

        static void Planter()
        {
            var c = new PixelCanvas(24, 26);
            Outlined(c, 4, 0, 16, 9, H("b8865a"), FInk);
            c.Rect(5, 6, 14, 1, H("d8a87a"));
            c.Disc(12f, 15f, 8f, H("3f8639")); c.Disc(9f, 17f, 5f, H("5aa64a")); c.Disc(15f, 19f, 4f, H("6cbd5c"));
            c.Disc(8f, 13f, 1.4f, H("ff7a7a")); c.Disc(16f, 14f, 1.4f, H("ffe066")); c.Disc(12f, 20f, 1.4f, H("ff9fc0"));
            Add(c, BottomCenter, "planter");
        }

        static void Hydrant()
        {
            var c = new PixelCanvas(12, 18);
            Outlined(c, 3, 0, 6, 13, H("d8384a"), FInk);
            Outlined(c, 1, 6, 10, 3, H("d8384a"), FInk);
            c.Disc(6f, 14f, 3f, H("b8322c"));
            c.Rect(4, 2, 1, 9, H("f07070"));
            Add(c, BottomCenter, "hydrant");
        }

        static void Billboard()
        {
            var c = new PixelCanvas(64, 72);
            c.Rect(14, 0, 3, 34, H("6b737c")); c.Rect(47, 0, 3, 34, H("6b737c"));
            Outlined(c, 0, 32, 64, 38, H("f4f5f8"), FInk);
            c.VGradient(2, 34, 60, 34, H("ffb35c"), H("ff7a7a"));
            c.Disc(16f, 52f, 9f, H("fff1d6"));                                        // gambar produk
            c.Rect(13, 46, 6, 12, H("6fb7ff")); c.Rect(13, 56, 6, 2, H("3b6fb6"));
            c.Rect(30, 56, 28, 4, H("ffffff")); c.Rect(30, 49, 24, 3, H("ffffff")); c.Rect(30, 42, 18, 3, H("2b2233"));
            Add(c, BottomCenter, "billboard");
        }

        static void Haystack()
        {
            var c = new PixelCanvas(32, 26);
            c.Ellipse(16f, 2f, 15f, 2f, new Color32(18, 12, 24, 70));
            c.Ellipse(16f, 10f, 14f, 10f, H("c8a040"));
            c.Ellipse(16f, 12f, 12f, 9f, H("e8c860"));
            for (int i = 0; i < 8; i++) c.Line(6 + i * 3, 6 + (i % 3), 8 + i * 3, 16 + (i % 2) * 2, H("b89030"));
            c.Disc(16f, 21f, 4f, H("f0d880"));
            Add(c, BottomCenter, "haystack");
        }

        static void Scarecrow()
        {
            var c = new PixelCanvas(24, 40);
            Color32 wood = H("8a5a3c");
            c.Rect(11, 0, 2, 30, wood);
            c.Rect(2, 22, 20, 2, wood);
            Outlined(c, 6, 14, 12, 12, H("d8584a"), FInk);                             // baju
            c.Rect(3, 22, 3, 3, H("e8c860")); c.Rect(18, 22, 3, 3, H("e8c860"));   // jerami di tangan
            c.Disc(12f, 30f, 4.5f, H("f0d8a0"));
            c.Rect(10, 30, 1, 1, FInk); c.Rect(14, 30, 1, 1, FInk); c.Rect(11, 28, 3, 1, FInk);
            c.Ellipse(12f, 34f, 9f, 2f, H("c8a040"));                                   // caping
            c.Triangle(new Vector2(12f, 39f), new Vector2(6f, 34f), new Vector2(18f, 34f), H("e8c860"));
            Add(c, BottomCenter, "scarecrow");
        }

        static void Boat()
        {
            var c = new PixelCanvas(64, 30);
            c.Ellipse(32f, 2f, 30f, 2f, new Color32(18, 12, 24, 70));
            for (int y = 2; y < 12; y++)
            {
                int inset = Mathf.RoundToInt((12 - y) * 0.9f);
                c.Rect(4 + inset, y, 56 - inset * 2, 1, y < 6 ? H("2a5290") : y < 9 ? H("f4f5f8") : H("d8384a"));
            }
            c.Rect(2, 11, 60, 2, FInk);
            c.Rect(4, 9, 3, 6, H("d8384a")); c.Rect(57, 9, 3, 6, H("d8384a"));    // haluan & buritan naik
            c.Rect(31, 12, 2, 16, H("8a5a3c"));                                         // tiang
            c.Rect(20, 13, 24, 1, H("8a5a3c"));
            c.Rect(12, 13, 6, 3, H("6b737c"));                                          // jaring
            Add(c, BottomCenter, "boat");
        }

        static void ParkedMotors()
        {
            var c = new PixelCanvas(72, 24);
            Color32[] cols = { H("e04b42"), H("2b2233"), H("3b6fb6"), H("f4f5f8") };
            for (int i = 0; i < 4; i++)
            {
                int x = 4 + i * 17;
                c.Disc(x + 3f, 4f, 3.5f, VRubber); c.Disc(x + 13f, 4f, 3.5f, VRubber);
                c.Disc(x + 3f, 4f, 1.5f, VRim); c.Disc(x + 13f, 4f, 1.5f, VRim);
                Outlined(c, x + 1, 6, 12, 5, cols[i], FInk);
                Outlined(c, x + 2, 10, 8, 3, VSeat, FInk);
                c.Line(x + 13, 4, x + 12, 14, VChrome, 1);
                c.Rect(x + 10, 14, 5, 1, FInk);
            }
            Add(c, BottomCenter, "parked_motors");
        }

        static void FishPond()
        {
            var c = new PixelCanvas(64, 36);
            c.Ellipse(32f, 16f, 31f, 15f, H("8a8378"));
            c.Ellipse(32f, 16f, 28f, 12.5f, H("3b8fb6"));
            c.Ellipse(28f, 19f, 18f, 6f, H("58b7d5"));
            c.Ellipse(20f, 12f, 3f, 1.4f, H("f08030")); c.Ellipse(40f, 18f, 3f, 1.4f, H("f4f5f8"));
            c.Ellipse(46f, 11f, 3f, 1.4f, H("f08030"));
            c.Disc(50f, 22f, 4f, H("4a8a3f")); c.Disc(50f, 22f, 1.2f, H("ff9fc0"));    // teratai
            c.Disc(14f, 20f, 3f, H("4a8a3f"));
            Add(c, BottomCenter, "fishpond");
        }
    }
}
