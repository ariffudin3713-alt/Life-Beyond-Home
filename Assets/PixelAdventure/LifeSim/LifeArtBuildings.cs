using UnityEngine;

namespace PixelAdventure.LifeSim
{
    public static partial class LifeArt
    {
        // ------------------------------------------------------------------ buildings

        static void BuildBuildings()
        {
            BuildHouse();
            BuildCafe();
            BuildShop();
            BuildInn();
        }

        /// <summary>Penginapan: bangunan dua lantai, atap hijau tua, kanopi di atas pintu, papan "INN".</summary>
        static void BuildInn()
        {
            var c = new PixelCanvas(120, 112);
            Color32 wall = PixelArt.Hex("e9e2c9");
            Color32 wallShade = PixelArt.Hex("cfc6a8");
            Color32 trim = PixelArt.Hex("4f7a63");
            Color32 trimDark = PixelArt.Hex("2c4a3b");
            Color32 roof = PixelArt.Hex("3f7d5c");
            Color32 roofDark = PixelArt.Hex("285640");
            Color32 roofLight = PixelArt.Hex("5fa07a");
            Color32 glass = PixelArt.Hex("7fc7e8");
            Color32 glassLight = PixelArt.Hex("c7f0ff");
            Color32 warm = PixelArt.Hex("ffd98a");

            // dinding dua lantai
            Outlined(c, 4, 0, 112, 80, wall, trimDark);
            c.Rect(8, 0, 4, 80, wallShade);
            c.Rect(4, 38, 112, 4, trim);            // lis antar lantai
            c.Rect(4, 76, 112, 4, trim);

            // atap pelana rendah
            for (int y = 78; y < 104; y++)
            {
                float t = (y - 78) / 25f;
                int half = Mathf.RoundToInt(Mathf.Lerp(62f, 34f, t));
                c.Rect(60 - half, y, half * 2, 1, y % 4 == 0 ? roofDark : roof);
                if (y % 4 == 2) c.Rect(60 - half + 3, y, half * 2 - 6, 1, roofLight);
            }
            c.Rect(26, 102, 68, 4, roofDark);

            // jendela lantai 2 (lampu kamar menyala hangat)
            for (int i = 0; i < 4; i++)
            {
                int x = 12 + i * 26;
                Window(c, x, 48, 18, 20, i % 2 == 0 ? glass : warm, glassLight, trimDark, trim);
                c.Rect(x - 1, 46, 20, 2, trimDark);   // ambang
            }

            // jendela lantai 1
            Window(c, 12, 10, 20, 20, glass, glassLight, trimDark, trim);
            Window(c, 88, 10, 20, 20, glass, glassLight, trimDark, trim);

            // pintu ganda + kanopi bergaris
            Outlined(c, 46, 0, 28, 30, PixelArt.Hex("7b4a2a"), trimDark);
            c.Rect(59, 2, 2, 26, trimDark);
            c.Set(56, 14, warm);
            c.Set(63, 14, warm);
            for (int x = 40; x < 80; x++)
                c.Rect(x, 30, 1, 6, ((x - 40) / 5) % 2 == 0 ? PixelArt.Hex("d8584a") : PixelArt.Hex("fff1d6"));
            c.Rect(40, 29, 40, 1, trimDark);

            // papan gantung "INN" di sisi kanan pintu
            Outlined(c, 80, 32, 22, 12, PixelArt.Hex("ffd35c"), trimDark);
            // I
            c.Rect(84, 35, 2, 6, trimDark);
            // N
            c.Rect(88, 35, 1, 6, trimDark); c.Rect(91, 35, 1, 6, trimDark);
            c.Set(89, 39, trimDark); c.Set(90, 37, trimDark); c.Set(89, 38, trimDark); c.Set(90, 38, trimDark);
            // N
            c.Rect(94, 35, 1, 6, trimDark); c.Rect(97, 35, 1, 6, trimDark);
            c.Set(95, 39, trimDark); c.Set(96, 37, trimDark); c.Set(95, 38, trimDark); c.Set(96, 38, trimDark);

            // anak tangga
            c.Rect(42, 0, 36, 2, PixelArt.Hex("9c9384"));

            Add(c, new Vector2(0.5f, 0f), "inn");
        }

        static void BuildHouse()
        {
            var c = new PixelCanvas(112, 96);
            Color32 wall = PixelArt.Hex("f0dcb8");
            Color32 wallShade = PixelArt.Hex("d8bd93");
            Color32 beam = PixelArt.Hex("8a5a3c");
            Color32 beamDark = PixelArt.Hex("5e3826");
            Color32 roof = PixelArt.Hex("c85c4a");
            Color32 roofDark = PixelArt.Hex("963a34");
            Color32 roofLight = PixelArt.Hex("e07a5f");
            Color32 glass = PixelArt.Hex("7fc7e8");
            Color32 glassLight = PixelArt.Hex("c7f0ff");

            // walls
            Outlined(c, 4, 0, 104, 64, wall, beamDark);
            c.Rect(8, 0, 6, 64, beam);
            c.Rect(98, 0, 6, 64, beam);
            c.Rect(4, 56, 104, 6, beam);
            c.Rect(14, 0, 2, 56, wallShade);

            // roof silhouette
            for (int y = 58; y < 96; y++)
            {
                float t = (y - 58) / 37f;
                int half = Mathf.RoundToInt(Mathf.Lerp(56f, 30f, t));
                c.Rect(56 - half - 2, y, half * 2 + 4, 1, roofDark);
            }
            for (int y = 60; y < 94; y++)
            {
                float t = (y - 60) / 33f;
                int half = Mathf.RoundToInt(Mathf.Lerp(54f, 29f, t));
                c.Rect(56 - half, y, half * 2, 1, y % 5 == 0 ? roofDark : roof);
                if (y % 4 == 0) c.Rect(56 - half + 3, y, half * 2 - 6, 1, roofLight);
            }
            c.Rect(24, 92, 64, 4, roofDark);
            c.Rect(30, 88, 52, 4, roof);

            // chimney
            Outlined(c, 84, 78, 14, 20, PixelArt.Hex("a9563f"), beamDark);
            c.Rect(86, 92, 10, 4, PixelArt.Hex("6b3a2b"));

            // windows
            Window(c, 16, 28, 18, 18, glass, glassLight, beamDark, beam);
            Window(c, 78, 28, 18, 18, glass, glassLight, beamDark, beam);
            c.Rect(16, 24, 18, 4, beamDark);
            c.Rect(78, 24, 18, 4, beamDark);

            // flower boxes
            for (int i = 0; i < 5; i++)
            {
                c.Set(18 + i * 3, 26, PixelArt.Hex("ff9fc0"));
                c.Set(80 + i * 3, 26, PixelArt.Hex("ffd35c"));
            }

            // door
            Outlined(c, 46, 0, 20, 32, PixelArt.Hex("7b4a2a"), beamDark);
            c.Rect(49, 4, 14, 25, PixelArt.Hex("5e3826"));
            c.Rect(50, 5, 12, 2, PixelArt.Hex("8a5a3c"));
            c.Set(59, 18, PixelArt.Hex("ffd35c"));
            c.Rect(42, 0, 28, 3, PixelArt.Hex("b7a58a"));

            // step stones
            c.Rect(44, 0, 24, 2, PixelArt.Hex("9c9384"));
            c.Rect(46, 0, 20, 1, PixelArt.Hex("c2b7a4"));

            Add(c, new Vector2(0.5f, 0f), "house");
        }

        static void BuildCafe()
        {
            var c = new PixelCanvas(128, 96);
            Color32 wall = PixelArt.Hex("d9a066");
            Color32 wallDark = PixelArt.Hex("b97b4a");
            Color32 beam = PixelArt.Hex("6b4229");
            Color32 beamDark = PixelArt.Hex("432818");
            Color32 awning = PixelArt.Hex("e85d5d");
            Color32 awningCream = PixelArt.Hex("fff1d6");
            Color32 glass = PixelArt.Hex("ffd98a");

            Outlined(c, 2, 0, 124, 64, wall, beamDark);
            c.Rect(4, 0, 4, 64, wallDark);
            c.Rect(120, 0, 4, 64, wallDark);
            c.Rect(4, 56, 120, 8, beam);

            // big window
            Outlined(c, 14, 22, 46, 30, glass, beamDark);
            c.Rect(16, 24, 42, 4, PixelArt.Hex("ffe9b8"));
            c.Rect(36, 24, 2, 26, beam);
            c.Rect(16, 36, 42, 2, beam);
            c.Rect(18, 26, 4, 8, PixelArt.Hex("fff7df"));

            // door
            Outlined(c, 74, 0, 24, 36, PixelArt.Hex("8a5a3c"), beamDark);
            c.Rect(77, 4, 18, 28, PixelArt.Hex("5e3826"));
            c.Rect(78, 6, 16, 2, PixelArt.Hex("a9764c"));
            c.Set(91, 20, PixelArt.Hex("ffd35c"));
            c.Rect(70, 0, 32, 3, PixelArt.Hex("b7a58a"));

            // menu board
            Outlined(c, 104, 18, 18, 26, PixelArt.Hex("3a2a24"), beam);
            c.Rect(106, 20, 14, 3, awningCream);
            c.Rect(106, 25, 10, 2, awningCream);
            c.Rect(106, 29, 12, 2, awningCream);
            c.Rect(106, 33, 8, 2, awningCream);

            // awning
            for (int x = 0; x < 128; x += 16)
            {
                Color32 col = (x / 16) % 2 == 0 ? awning : awningCream;
                c.Rect(x, 60, 16, 14, col);
            }
            for (int x = 0; x < 128; x += 16)
            {
                Color32 col = (x / 16) % 2 == 0 ? awning : awningCream;
                c.Rect(x, 58, 16, 2, beamDark);
                for (int i = 0; i < 8; i++)
                {
                    int yy = 60 - Mathf.RoundToInt(Mathf.Sqrt(16 - (i - 4) * (i - 4)) * 2f);
                    c.Set(x + 4 + i, yy + 8, col);
                }
            }
            c.Rect(0, 72, 128, 4, beam);
            c.Rect(0, 76, 128, 20, roofStrip(awning, awningCream));
            c.Rect(4, 92, 120, 4, beam);
            // string lights
            for (int x = 10; x < 120; x += 22)
            {
                c.Set(x, 70, PixelArt.Hex("ffe066"));
                c.Set(x + 1, 69, PixelArt.Hex("fff3b0"));
            }

            Add(c, new Vector2(0.5f, 0f), "cafe");
        }

        static Color32 roofStrip(Color32 a, Color32 b)
        {
            return new Color32((byte)((a.r + b.r) / 2), (byte)((a.g + b.g) / 2), (byte)((a.b + b.b) / 2), 255);
        }

        static void BuildShop()
        {
            var c = new PixelCanvas(104, 80);
            Color32 wood = PixelArt.Hex("b97b4a");
            Color32 woodDark = PixelArt.Hex("7b4a2a");
            Color32 beam = PixelArt.Hex("5e3826");
            Color32 green = PixelArt.Hex("4fae62");
            Color32 greenLight = PixelArt.Hex("7fd486");
            Color32 interior = PixelArt.Hex("2c2030");

            // back wall / interior
            c.Rect(8, 0, 88, 58, interior);
            c.FrameRect(8, 0, 88, 58, beam);
            c.Rect(12, 8, 80, 42, PixelArt.Hex("3a2a3e"));

            // shelves with flowers
            for (int row = 0; row < 2; row++)
            {
                int y = 18 + row * 18;
                c.Rect(16, y, 72, 3, wood);
                for (int i = 0; i < 9; i++)
                {
                    Color32 flower = i % 3 == 0 ? PixelArt.Hex("ff9fc0")
                        : i % 3 == 1 ? PixelArt.Hex("ffd35c") : PixelArt.Hex("ff6b78");
                    c.Rect(19 + i * 8, y + 4, 2, 6, PixelArt.Hex("4a9c3f"));
                    c.Rect(18 + i * 8, y + 9, 5, 5, flower);
                }
            }

            // posts
            c.Rect(6, 0, 6, 72, woodDark);
            c.Rect(92, 0, 6, 72, woodDark);
            c.Rect(8, 0, 2, 72, wood);

            // awning
            for (int x = 0; x < 104; x += 13)
            {
                Color32 col = (x / 13) % 2 == 0 ? green : greenLight;
                c.Rect(x, 58, 13, 14, col);
            }
            c.Rect(0, 56, 104, 3, beam);
            c.Rect(0, 72, 104, 8, PixelArt.Hex("3f8a4a"));
            c.Rect(2, 76, 100, 4, green);

            // counter
            Outlined(c, 4, 0, 96, 20, wood, beam);
            c.Rect(8, 4, 88, 12, PixelArt.Hex("c98a55"));
            c.Rect(8, 14, 88, 3, woodDark);
            // little sign
            c.Rect(38, 24, 28, 14, PixelArt.Hex("2c2030"));
            c.Rect(40, 26, 24, 10, PixelArt.Hex("fff1d6"));
            c.Rect(43, 28, 18, 2, green);
            c.Rect(43, 31, 12, 2, woodDark);

            Add(c, new Vector2(0.5f, 0f), "shop");
        }

        static void Window(PixelCanvas c, int x, int y, int w, int h,
            Color32 glass, Color32 light, Color32 frame, Color32 frameLight)
        {
            Outlined(c, x, y, w, h, glass, frame);
            c.Rect(x + 2, y + 2, w - 4, 3, light);
            c.Rect(x + w / 2, y + 2, 2, h - 4, frameLight);
            c.Rect(x + 2, y + h / 2, w - 4, 2, frameLight);
        }
    }
}
