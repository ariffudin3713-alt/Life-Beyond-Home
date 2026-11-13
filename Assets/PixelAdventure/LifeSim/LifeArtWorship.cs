using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Tempat ibadah & taman:
    /// - Masjid (kubah hijau, dua menara) dan Gereja (atap pelana, menara lonceng, salib), beserta
    ///   isi ruangannya: mihrab, mimbar, bedug, saf sajadah, jadwal sholat, kaligrafi, tempat wudhu;
    ///   bangku gereja, altar, salib, kaca patri, lilin, organ.
    /// - Taman bermain: air mancur besar (2 frame), perosotan, ayunan (3 frame), jungkat-jungkit
    ///   (2 frame), gazebo, dan bedeng bunga.
    /// Pivot tengah bawah, 16 px = 1 unit.
    /// </summary>
    public static partial class LifeArt
    {
        static readonly Color32 Clear = new Color32(0, 0, 0, 0);

        static void BuildWorshipArt()
        {
            Mosque(); Church(); MosqueInterior(); ChurchInterior(); ParkArt();
        }

        static void Mosque()
        {
            Color32 green = PixelArt.Hex("2e8f62"), greenLight = PixelArt.Hex("4fae7a"), greenDark = PixelArt.Hex("1f6a48");
            Color32 wall = PixelArt.Hex("f4f0e2"), gold = PixelArt.Hex("e8b23a");
            var c = new PixelCanvas(112, 120);
            Body(c, 8, 96, 54, wall);
            c.Rect(9, 46, 94, 3, gold);
            for (int x = 12; x < 100; x += 6) c.Set(x, 47, green);                        // hiasan kaligrafi
            c.Rect(9, 50, 94, 4, green);
            c.Rect(14, 54, 84, 4, green);
            c.Ellipse(24f, 60f, 9f, 7f, green); c.Ellipse(88f, 60f, 9f, 7f, green);       // kubah kecil
            c.Ellipse(56f, 66f, 26f, 18f, green);                                         // kubah utama
            c.Ellipse(49f, 72f, 10f, 6f, greenLight);
            c.Rect(30, 56, 52, 4, gold);
            c.Rect(55, 84, 2, 8, gold);
            c.Disc(56f, 96f, 4f, gold); c.Disc(57.5f, 97f, 3.3f, Clear);                 // bulan sabit
            for (int side = 0; side < 2; side++)                                          // dua menara
            {
                int mx = side == 0 ? 0 : 102;
                Outlined(c, mx, 0, 10, 92, wall, FInk);
                for (int y = 20; y < 90; y += 22) c.Rect(mx + 1, y, 8, 3, green);
                Outlined(c, mx - 2, 72, 14, 4, green, FInk);                              // balkon
                c.Triangle(new Vector2(mx - 1, 92f), new Vector2(mx + 11, 92f), new Vector2(mx + 5, 108f), green);
                c.Disc(mx + 5f, 111f, 2f, gold);
            }
            Outlined(c, 44, 0, 24, 30, green, FInk); c.Disc(56f, 30f, 12f, green);        // pintu lengkung
            c.Rect(47, 0, 18, 28, greenDark); c.Disc(56f, 28f, 9f, greenDark);
            c.Rect(55, 1, 2, 30, FInk);
            for (int i = 0; i < 2; i++)                                                   // jendela lengkung
            {
                int wx = i == 0 ? 16 : 80;
                Outlined(c, wx, 14, 16, 22, FGlass, FInk); c.Disc(wx + 8f, 36f, 8f, FGlass);
                c.Rect(wx + 7, 14, 2, 28, FInk);
            }
            c.Rect(34, 0, 44, 3, PixelArt.Hex("cfc8b8"));                                // teras
            Add(c, BottomCenter, "bld_masjid");
        }

        static void Church()
        {
            Color32 wall = PixelArt.Hex("f2ece0"), roof = PixelArt.Hex("a8463c"), roofDark = PixelArt.Hex("86342e");
            Color32 gold = PixelArt.Hex("e8b23a");
            var c = new PixelCanvas(96, 128);
            Body(c, 8, 80, 60, wall);
            c.Triangle(new Vector2(4f, 60f), new Vector2(92f, 60f), new Vector2(48f, 92f), roof);
            c.Line(4, 60, 48, 92, roofDark, 2); c.Line(92, 60, 48, 92, roofDark, 2);
            c.Rect(4, 58, 88, 3, roofDark);
            Outlined(c, 38, 56, 20, 46, wall, FInk);                                       // menara lonceng
            c.Triangle(new Vector2(35f, 101f), new Vector2(61f, 101f), new Vector2(48f, 116f), roof);
            c.Rect(47, 116, 2, 11, gold); c.Rect(44, 121, 8, 2, gold);                     // salib
            Outlined(c, 43, 82, 10, 12, PixelArt.Hex("3a3040"), FInk); c.Disc(48f, 94f, 5f, PixelArt.Hex("3a3040"));
            c.Disc(48f, 88f, 3f, gold);                                                    // lonceng
            c.Disc(48f, 70f, 7f, FInk); c.Disc(48f, 70f, 6f, PixelArt.Hex("6fb7ff"));       // jendela mawar
            c.Disc(48f, 70f, 2f, PixelArt.Hex("ffd23f"));
            c.Disc(48f, 74f, 1.5f, PixelArt.Hex("d8303a")); c.Disc(48f, 66f, 1.5f, PixelArt.Hex("d8303a"));
            c.Disc(44f, 70f, 1.5f, PixelArt.Hex("4fae62")); c.Disc(52f, 70f, 1.5f, PixelArt.Hex("4fae62"));
            Outlined(c, 38, 0, 20, 30, FDoor, FInk); c.Disc(48f, 30f, 10f, FDoor);         // pintu
            c.Rect(47, 1, 2, 38, FInk);
            Color32[] pane = { PixelArt.Hex("d8303a"), PixelArt.Hex("3b6fb6"), PixelArt.Hex("ffd23f"), PixelArt.Hex("4fae62") };
            foreach (int wx in new[] { 16, 68 })                                           // kaca patri
            {
                Outlined(c, wx, 16, 12, 24, FInk, FInk); c.Disc(wx + 6f, 40f, 6f, FInk);
                for (int y = 17; y < 45; y++)
                    for (int x = wx + 1; x < wx + 11; x++)
                        if (c.Get(x, y).a > 0) c.Set(x, y, pane[((x - wx) / 5 + (y - 17) / 7) % pane.Length]);
                c.Rect(wx + 5, 16, 1, 30, FInk); c.Rect(wx, 30, 12, 1, FInk);
            }
            c.Rect(30, 0, 36, 3, PixelArt.Hex("cfc8b8"));
            Add(c, BottomCenter, "bld_gereja");
        }

        static void MosqueInterior()
        {
            Color32 ink = FurnOutline, green = PixelArt.Hex("2e8f62"), greenDark = PixelArt.Hex("1f6a48");
            Color32 gold = PixelArt.Hex("e8b23a"), wood = PixelArt.Hex("8a5a3c"), woodLight = PixelArt.Hex("a8754a");

            var c = new PixelCanvas(44, 46);                                               // mihrab (dinding)
            Outlined(c, 2, 0, 40, 28, gold, ink); c.Disc(22f, 28f, 20f, gold);
            Outlined(c, 5, 0, 34, 27, green, green); c.Disc(22f, 27f, 17f, green);
            c.Rect(10, 0, 24, 24, greenDark); c.Disc(22f, 24f, 12f, greenDark);
            c.Rect(21, 30, 2, 6, gold); c.Disc(22f, 28f, 2.5f, PixelArt.Hex("ffe9a0"));       // lampu gantung
            Add(c, BottomCenter, "int_mihrab");

            c = new PixelCanvas(30, 42);                                                   // mimbar
            Outlined(c, 0, 0, 10, 8, woodLight, ink);
            Outlined(c, 8, 0, 10, 16, woodLight, ink);
            Outlined(c, 16, 0, 14, 24, wood, ink);
            c.Rect(17, 24, 2, 12, wood); c.Rect(27, 24, 2, 12, wood);
            Outlined(c, 15, 35, 16, 4, green, ink); c.Disc(23f, 40f, 3f, green);
            Add(c, BottomCenter, "int_mimbar");

            c = new PixelCanvas(32, 30);                                                   // bedug
            c.Rect(4, 0, 3, 16, wood); c.Rect(25, 0, 3, 16, wood); c.Rect(4, 8, 24, 2, wood);
            c.Ellipse(16f, 19f, 13f, 9f, ink); c.Ellipse(16f, 19f, 12f, 8f, PixelArt.Hex("a8622e"));
            c.Ellipse(16f, 19f, 6f, 7f, PixelArt.Hex("efdcb0"));
            for (int i = 0; i < 6; i++) c.Set(10 + i * 2, 11, gold);
            c.Line(26, 26, 31, 29, wood, 1);                                               // pemukul
            Add(c, BottomCenter, "int_bedug");

            c = new PixelCanvas(96, 14);                                                   // satu saf sajadah (rata)
            for (int i = 0; i < 6; i++)
            {
                int x = 2 + i * 16;
                Color32 mat = i % 2 == 0 ? PixelArt.Hex("b8323a") : PixelArt.Hex("2f6fa8");
                Outlined(c, x, 0, 13, 10, mat, Color32.Lerp(mat, ink, 0.5f));
                c.Triangle(new Vector2(x, 10f), new Vector2(x + 13, 10f), new Vector2(x + 6.5f, 14f), mat);
                c.Rect(x + 2, 2, 9, 1, gold); c.Rect(x + 5, 5, 3, 3, gold);
            }
            Add(c, BottomCenter, "int_saf");

            c = new PixelCanvas(52, 36);                                                   // jadwal sholat (dinding)
            Outlined(c, 0, 0, 52, 36, PixelArt.Hex("1d2b24"), gold);
            string[] rows = { "SUBUH 04.30", "DZUHUR 12.00", "ASHAR 15.15", "MAGRIB 18.00", "ISYA 19.15" };
            for (int i = 0; i < rows.Length; i++) DrawTiny(c, 2, 29 - i * 6, rows[i], i % 2 == 0 ? PixelArt.Hex("ffe9a0") : PixelArt.Hex("8fe0b0"));
            Add(c, BottomCenter, "int_prayer_board");

            c = new PixelCanvas(40, 24);                                                   // kaligrafi (dinding)
            Outlined(c, 0, 0, 40, 24, gold, ink);
            c.Rect(2, 2, 36, 20, greenDark);
            c.Line(6, 8, 14, 16, gold); c.Line(14, 16, 20, 9, gold); c.Line(20, 9, 28, 17, gold); c.Line(28, 17, 34, 7, gold);
            c.Disc(10f, 15f, 1.2f, gold); c.Disc(25f, 7f, 1.2f, gold); c.Rect(6, 6, 28, 1, gold);
            Add(c, BottomCenter, "int_calligraphy");

            c = new PixelCanvas(56, 26);                                                   // tempat wudhu
            Outlined(c, 0, 0, 56, 14, PixelArt.Hex("cfe8f0"), ink);
            for (int x = 1; x < 55; x += 6) c.Rect(x, 1, 1, 12, PixelArt.Hex("aac8d4"));
            Outlined(c, 0, 12, 56, 4, PixelArt.Hex("9aa2ab"), ink);
            for (int i = 0; i < 4; i++)
            {
                int x = 6 + i * 13;
                c.Rect(x, 16, 2, 8, PixelArt.Hex("aab2b4")); c.Rect(x, 22, 6, 2, PixelArt.Hex("aab2b4"));
                c.Rect(x + 5, 18, 1, 3, PixelArt.Hex("6fb7ff"));
            }
            Add(c, BottomCenter, "int_wudhu");
        }

        static void ChurchInterior()
        {
            Color32 ink = FurnOutline, wood = PixelArt.Hex("8a5a3c"), woodDark = PixelArt.Hex("6b4126"), gold = PixelArt.Hex("e8b23a");

            var c = new PixelCanvas(64, 22);                                               // bangku gereja
            c.Rect(3, 0, 3, 6, woodDark); c.Rect(58, 0, 3, 6, woodDark);
            Outlined(c, 0, 5, 64, 6, wood, ink);
            Outlined(c, 0, 10, 64, 11, woodDark, ink);
            c.Rect(2, 18, 60, 1, wood);
            Add(c, BottomCenter, "int_pew");

            c = new PixelCanvas(60, 30);                                                   // altar
            Outlined(c, 0, 0, 60, 18, PixelArt.Hex("a8754a"), ink);
            Outlined(c, 0, 10, 60, 8, PixelArt.Hex("f6f4ee"), ink);
            c.Rect(24, 2, 12, 16, PixelArt.Hex("b8323a")); c.Rect(29, 6, 2, 8, gold); c.Rect(26, 10, 8, 2, gold);
            for (int i = 0; i < 2; i++)
            {
                int x = i == 0 ? 8 : 48;
                c.Rect(x, 18, 4, 8, PixelArt.Hex("f6f4ee")); c.Disc(x + 2f, 28f, 1.5f, PixelArt.Hex("ffd23f"));
            }
            Outlined(c, 20, 18, 12, 3, PixelArt.Hex("6b2a2a"), ink);                         // kitab
            Add(c, BottomCenter, "int_altar");

            c = new PixelCanvas(22, 34);                                                   // salib kayu (dinding)
            Outlined(c, 8, 0, 6, 34, wood, ink);
            Outlined(c, 0, 20, 22, 6, wood, ink);
            Add(c, BottomCenter, "int_cross");

            c = new PixelCanvas(22, 36);                                                   // kaca patri (dinding)
            Outlined(c, 0, 0, 22, 26, FInk, FInk); c.Disc(11f, 26f, 11f, FInk);
            Color32[] pane = { PixelArt.Hex("d8303a"), PixelArt.Hex("3b6fb6"), PixelArt.Hex("ffd23f"), PixelArt.Hex("4fae62"), PixelArt.Hex("a87fc8") };
            var shape = new PixelCanvas(22, 36);
            System.Array.Copy(c.Pixels, shape.Pixels, c.Pixels.Length);
            for (int y = 1; y < 36; y++)
                for (int x = 1; x < 21; x++)
                {
                    if (shape.Get(x, y).a == 0) continue;
                    bool edge = shape.Get(x - 1, y).a == 0 || shape.Get(x + 1, y).a == 0 || shape.Get(x, y + 1).a == 0;
                    if (edge) continue;
                    bool lead = x % 5 == 0 || y % 6 == 0;
                    c.Set(x, y, lead ? FInk : pane[(x / 5 + y / 6) % pane.Length]);
                }
            Add(c, BottomCenter, "int_stained");

            c = new PixelCanvas(12, 28);                                                   // tiang lilin
            Outlined(c, 2, 0, 8, 3, gold, ink); c.Rect(5, 3, 2, 16, gold);
            Outlined(c, 0, 18, 12, 3, gold, ink);
            for (int i = 0; i < 3; i++) { c.Rect(1 + i * 4, 21, 2, 4, PixelArt.Hex("f6f4ee")); c.Set(1 + i * 4, 26, PixelArt.Hex("ffd23f")); c.Set(2 + i * 4, 27, PixelArt.Hex("ff9a3a")); }
            Add(c, BottomCenter, "int_candles");

            c = new PixelCanvas(38, 40);                                                   // organ
            for (int i = 0; i < 7; i++) Outlined(c, 3 + i * 5, 20, 4, 12 + (i % 4) * 2 + (i == 3 ? 6 : 0), PixelArt.Hex("c8c8c8"), ink);
            Outlined(c, 0, 0, 38, 22, woodDark, ink);
            c.Rect(2, 14, 34, 4, PixelArt.Hex("f6f4ee"));
            for (int x = 4; x < 36; x += 3) c.Rect(x, 16, 1, 2, ink);
            Add(c, BottomCenter, "int_organ");
        }

        static void ParkArt()
        {
            Color32 ink = FurnOutline, stone = PixelArt.Hex("c9c3b5"), stoneDark = PixelArt.Hex("8f897d");
            Color32 water = PixelArt.Hex("4fa8d8"), waterLight = PixelArt.Hex("a8e0f8"), white = PixelArt.Hex("ffffff");

            // ---- air mancur besar bertingkat (2 frame percikan)
            for (int f = 0; f < 2; f++)
            {
                var c = new PixelCanvas(80, 56);
                c.Ellipse(40f, 11f, 39f, 11f, stoneDark); c.Ellipse(40f, 12f, 37f, 9.5f, stone);
                c.Ellipse(40f, 13f, 33f, 7f, water);
                for (int i = 0; i < 9; i++) c.Rect(14 + i * 6 + f * 2, 12 + (i % 3), 3, 1, waterLight);   // riak
                Outlined(c, 36, 12, 8, 18, stone, stoneDark);
                c.Ellipse(40f, 30f, 15f, 4f, stoneDark); c.Ellipse(40f, 31f, 14f, 3f, water);
                Outlined(c, 38, 31, 4, 10, stone, stoneDark);
                c.Ellipse(40f, 41f, 7f, 2f, stoneDark); c.Ellipse(40f, 42f, 6f, 1.5f, water);
                // pancuran utama + jatuhan air dari mangkuk
                for (int y = 43; y < 54; y++) c.Set(40 + ((y + f) % 3 == 0 ? 1 : 0), y, waterLight);
                for (int s = -1; s <= 1; s += 2)
                {
                    for (int k = 0; k < 7; k++) c.Set(40 + s * (3 + k), 52 - k * k / 3 - f, k % 2 == f ? white : waterLight);
                    for (int k = 0; k < 10; k++) c.Set(40 + s * (13 + k / 4), 29 - k * 1 - (k + f) % 2, waterLight);
                    for (int k = 0; k < 8; k++) c.Set(40 + s * (6 + k / 3), 40 - k, k % 2 == f ? white : waterLight);
                }
                Add(c, BottomCenter, "fountain_big_" + f);
            }

            // ---- perosotan
            {
                var c = new PixelCanvas(42, 40);
                Color32 red = PixelArt.Hex("e0483c"), blue = PixelArt.Hex("3b6fb6"), yellow = PixelArt.Hex("ffd23f");
                c.Rect(3, 0, 2, 30, blue); c.Rect(11, 0, 2, 30, blue);
                for (int y = 4; y < 28; y += 5) c.Rect(3, y, 10, 1, blue);                // tangga
                Outlined(c, 2, 28, 14, 4, red, ink);
                c.Rect(2, 32, 2, 6, blue); c.Rect(14, 32, 2, 6, blue); c.Rect(2, 37, 14, 2, red);  // pagar atas
                for (int i = 0; i < 26; i++)                                              // papan luncur
                {
                    int x = 15 + i, y = 29 - i;
                    c.Rect(x, Mathf.Max(0, y - 3), 1, 4, i % 2 == 0 ? yellow : PixelArt.Hex("f0b82c"));
                    c.Set(x, y + 1, ink);
                }
                Add(c, BottomCenter, "slide");
            }

            // ---- ayunan (3 frame) dengan anak di salah satu dudukan
            int[] swingOff = { -4, 0, 4 };
            for (int f = 0; f < 3; f++)
            {
                var c = new PixelCanvas(48, 42);
                Color32 post = PixelArt.Hex("e0483c");
                c.Line(2, 0, 7, 38, post, 2); c.Line(12, 0, 7, 38, post, 2);
                c.Line(36, 0, 41, 38, post, 2); c.Line(46, 0, 41, 38, post, 2);
                c.Rect(6, 37, 37, 3, PixelArt.Hex("3b6fb6"));
                for (int s = 0; s < 2; s++)
                {
                    int ax = 17 + s * 14, off = s == 0 ? swingOff[f] : -swingOff[f] / 2;
                    int sx = ax + off, sy = 8;
                    c.Line(ax - 3, 37, sx - 3, sy, PixelArt.Hex("6b737c"));
                    c.Line(ax + 3, 37, sx + 3, sy, PixelArt.Hex("6b737c"));
                    Outlined(c, sx - 5, sy - 2, 10, 3, PixelArt.Hex("ffd23f"), ink);
                    if (s == 0)                                                              // anak berayun
                    {
                        c.Rect(sx - 2, sy + 1, 5, 7, PixelArt.Hex("4fae62"));
                        c.Disc(sx + 0.5f, sy + 11f, 3f, PixelArt.Hex("f2c9a0"));
                        c.Rect(sx - 2, sy + 12, 5, 2, PixelArt.Hex("3a2a22"));
                        c.Rect(sx - 2 + off / 2, sy - 4, 2, 3, PixelArt.Hex("3b6fb6")); c.Rect(sx + 1 + off / 2, sy - 4, 2, 3, PixelArt.Hex("3b6fb6"));
                    }
                }
                Add(c, BottomCenter, "swing_" + f);
            }

            // ---- jungkat-jungkit (2 frame)
            for (int f = 0; f < 2; f++)
            {
                var c = new PixelCanvas(52, 20);
                c.Triangle(new Vector2(22f, 0f), new Vector2(30f, 0f), new Vector2(26f, 8f), PixelArt.Hex("6b737c"));
                int l = f == 0 ? 3 : 13, r = f == 0 ? 13 : 3;
                c.Line(2, l, 50, r, PixelArt.Hex("e0483c"), 3);
                Outlined(c, 0, l + 2, 6, 4, PixelArt.Hex("3b6fb6"), ink);
                Outlined(c, 46, r + 2, 6, 4, PixelArt.Hex("3b6fb6"), ink);
                c.Rect(4, l + 6, 1, 3, ink); c.Rect(47, r + 6, 1, 3, ink);                // pegangan
                Add(c, BottomCenter, "seesaw_" + f);
            }

            // ---- gazebo / pendopo
            {
                var c = new PixelCanvas(68, 60);
                Color32 wood = PixelArt.Hex("8a5a3c"), tile = PixelArt.Hex("b0503c"), tileDark = PixelArt.Hex("8a3a2e");
                Outlined(c, 2, 0, 64, 6, PixelArt.Hex("c8925e"), ink);
                foreach (int x in new[] { 6, 22, 44, 60 }) c.Rect(x, 6, 3, 28, wood);
                Outlined(c, 10, 6, 48, 4, PixelArt.Hex("a8754a"), ink);                    // bangku dalam
                for (int y = 32; y < 46; y++)
                    c.Rect(-2 + (y - 32), y, 72 - (y - 32) * 2, 1, (y / 2) % 2 == 0 ? tile : tileDark);
                for (int y = 46; y < 56; y++)
                    c.Rect(18 + (y - 46) * 2, y, 32 - (y - 46) * 4, 1, (y / 2) % 2 == 0 ? tile : tileDark);
                c.Rect(32, 54, 4, 6, PixelArt.Hex("e8b23a"));                               // mahkota atap
                Add(c, BottomCenter, "gazebo");
            }

            // ---- bedeng bunga
            {
                var c = new PixelCanvas(40, 16);
                c.Ellipse(20f, 6f, 19.5f, 6f, PixelArt.Hex("a8584a"));
                c.Ellipse(20f, 7f, 17.5f, 5f, PixelArt.Hex("5a3a2a"));
                Color32[] fl = { PixelArt.Hex("ff6b8a"), PixelArt.Hex("ffd23f"), PixelArt.Hex("ffffff"), PixelArt.Hex("c79ad8"), PixelArt.Hex("ff9a4a") };
                for (int i = 0; i < 9; i++)
                {
                    float x = 6f + i * 3.5f, y = 7f + (i % 3) * 2f;
                    c.Disc(x, y, 2.2f, PixelArt.Hex("3f8a4c"));
                    c.Disc(x, y + 1.5f, 1.4f, fl[i % fl.Length]);
                }
                Add(c, BottomCenter, "flowerbed");
            }
        }
    }
}
