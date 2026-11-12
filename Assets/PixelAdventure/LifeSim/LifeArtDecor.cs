using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Bangunan dekorasi (tidak bisa dimasuki) untuk menghidupkan kota:
    /// rumah warga (beberapa warna atap), toko tutup (rolling door + gembok), bengkel terbengkalai.
    /// Pivot tengah bawah, 16 px = 1 unit.
    /// </summary>
    public static partial class LifeArt
    {
        static void BuildDecorArt()
        {
            MakeHouseVariant("house_blue", "3b6fb6", "284f86", "5a8fd6", "f0e6d2");
            MakeHouseVariant("house_green", "4f8a4a", "356535", "6fae62", "efe2c4");
            MakeHouseVariant("house_brown", "8a5a3c", "5e3826", "a8754a", "e9dcc0");
            MakeClosedShop();
            MakeRuinedWorkshop();
        }

        /// <summary>Rumah warga sederhana (96x84): dinding, atap pelana, dua jendela, pintu tertutup.</summary>
        static void MakeHouseVariant(string key, string roofHex, string roofDarkHex, string roofLightHex, string wallHex)
        {
            var c = new PixelCanvas(96, 84);
            Color32 ink = PixelArt.Hex("322b36");
            Color32 wall = PixelArt.Hex(wallHex);
            Color32 wallShade = Color32.Lerp(wall, ink, 0.18f);
            Color32 roof = PixelArt.Hex(roofHex), roofDark = PixelArt.Hex(roofDarkHex), roofLight = PixelArt.Hex(roofLightHex);
            Color32 glass = PixelArt.Hex("7fc7e8"), glassLight = PixelArt.Hex("c7f0ff"), wood = PixelArt.Hex("7b4a2a");

            Outlined(c, 4, 0, 88, 54, wall, ink);
            c.Rect(6, 0, 4, 54, wallShade);
            c.Rect(4, 0, 88, 4, wallShade);                         // pondasi
            for (int y = 50; y < 84; y++)
            {
                float t = (y - 50) / 33f;
                int half = Mathf.RoundToInt(Mathf.Lerp(50f, 4f, t));
                c.Rect(48 - half, y, half * 2, 1, y % 4 == 0 ? roofDark : roof);
                if (y % 4 == 2 && half > 3) c.Rect(48 - half + 2, y, half * 2 - 4, 1, roofLight);
            }
            c.Rect(0, 50, 96, 3, roofDark);                          // lis atap
            Window(c, 12, 22, 18, 16, glass, glassLight, ink, wood);
            Window(c, 66, 22, 18, 16, glass, glassLight, ink, wood);
            c.Rect(11, 20, 20, 2, wood); c.Rect(65, 20, 20, 2, wood);
            Outlined(c, 39, 0, 18, 30, wood, ink);                  // pintu tertutup
            c.Rect(42, 3, 12, 24, PixelArt.Hex("5e3826"));
            c.Set(52, 14, PixelArt.Hex("ffd35c"));
            c.Rect(36, 0, 24, 2, PixelArt.Hex("9c9384"));            // anak tangga
            // pot bunga kecil di samping pintu
            Outlined(c, 30, 0, 6, 5, PixelArt.Hex("c8794a"), ink);
            c.Disc(33f, 8f, 3f, PixelArt.Hex("4fae62")); c.Set(33, 10, PixelArt.Hex("ff9fc0"));
            Add(c, new Vector2(0.5f, 0f), key);
        }

        /// <summary>Toko tutup (100x80): papan nama kosong, rolling door turun + gembok, poster pudar.</summary>
        static void MakeClosedShop()
        {
            var c = new PixelCanvas(100, 80);
            Color32 ink = PixelArt.Hex("322b36");
            Color32 wall = PixelArt.Hex("d9cfbd"), wallShade = PixelArt.Hex("bdb19c");
            Color32 shutter = PixelArt.Hex("9aa2ab"), shutterDark = PixelArt.Hex("6b737c");

            Outlined(c, 2, 0, 96, 64, wall, ink);
            c.Rect(4, 0, 4, 64, wallShade);
            // atap datar + kanopi pudar
            Outlined(c, 0, 62, 100, 8, PixelArt.Hex("8a8f96"), ink);
            for (int x = 6; x < 94; x++)
                c.Rect(x, 54, 1, 7, ((x - 6) / 8) % 2 == 0 ? PixelArt.Hex("b8857a") : PixelArt.Hex("e8ded0"));
            c.Rect(6, 53, 88, 1, ink);
            // rolling door (sebagian besar depan)
            Outlined(c, 12, 0, 76, 50, shutter, ink);
            for (int y = 3; y < 49; y += 4) c.Rect(13, y, 74, 1, shutterDark);
            c.Rect(12, 0, 76, 3, shutterDark);
            Outlined(c, 46, 4, 8, 8, PixelArt.Hex("e8b23a"), ink);   // gembok
            c.Rect(48, 11, 4, 3, ink);
            // poster pudar & coretan
            Outlined(c, 20, 26, 12, 14, PixelArt.Hex("efe2c4"), wallShade);
            c.Rect(22, 34, 8, 2, PixelArt.Hex("c96a5a")); c.Rect(22, 30, 6, 1, wallShade);
            Outlined(c, 66, 30, 14, 10, PixelArt.Hex("e9e2c9"), wallShade);
            c.Rect(68, 34, 10, 1, PixelArt.Hex("6f8fd6"));
            // retakan dinding
            c.Set(92, 40, ink); c.Set(93, 39, ink); c.Set(93, 38, ink); c.Set(94, 37, ink);
            Add(c, new Vector2(0.5f, 0f), "shop_closed");
        }

        /// <summary>Bengkel terbengkalai (104x78): seng berkarat, atap bolong, jendela dipaku papan, rumput liar.</summary>
        static void MakeRuinedWorkshop()
        {
            var c = new PixelCanvas(104, 78);
            Color32 ink = PixelArt.Hex("2a2430");
            Color32 rust = PixelArt.Hex("9a5a34"), rustDark = PixelArt.Hex("6e3d24"), rustLight = PixelArt.Hex("b8784a");
            Color32 metal = PixelArt.Hex("7d828a"), metalDark = PixelArt.Hex("5b5f66");

            // dinding seng bergelombang (berkarat bergaris)
            Outlined(c, 2, 0, 100, 58, metal, ink);
            for (int x = 3; x < 101; x++)
            {
                Color32 col = x % 6 < 2 ? metalDark : metal;
                if (Hash(x / 3, 7) % 3 == 0) col = Color32.Lerp(col, rust, 0.65f);
                c.Rect(x, 1, 1, 56, col);
            }
            for (int i = 0; i < 40; i++)                              // bercak karat
                c.Rect(4 + (int)(Hash(i, 3) % 94), 2 + (int)(Hash(i, 9) % 50), 3, 2, i % 2 == 0 ? rust : rustDark);
            // atap miring berkarat dengan lubang
            for (int y = 56; y < 72; y++)
                c.Rect(0, y, 104, 1, y % 3 == 0 ? rustDark : rust);
            c.Rect(0, 72, 104, 2, rustLight);
            c.Rect(62, 62, 14, 8, PixelArt.Hex("1a1620"));            // lubang atap
            c.Rect(60, 64, 2, 4, rustDark); c.Rect(76, 63, 2, 5, rustDark);
            // pintu garasi setengah rusak (gelap di dalam)
            Outlined(c, 34, 0, 36, 40, PixelArt.Hex("1a1620"), ink);
            for (int y = 22; y < 39; y += 4) c.Rect(35, y, 34, 2, metalDark);    // daun pintu tersangkut di atas
            c.Line(36, 22, 52, 14, metalDark, 2);                      // lembaran miring jatuh
            // jendela dipaku papan
            Outlined(c, 8, 22, 18, 16, PixelArt.Hex("1a1620"), ink);
            c.Line(8, 24, 25, 36, PixelArt.Hex("a8754a"), 3);
            c.Line(8, 36, 25, 24, PixelArt.Hex("a8754a"), 3);
            Outlined(c, 78, 22, 18, 16, PixelArt.Hex("4a6a7a"), ink);
            c.Line(80, 36, 86, 28, ink, 1); c.Line(86, 28, 90, 33, ink, 1);   // kaca retak
            // ban bekas & rumput liar
            c.Disc(16f, 6f, 5f, ink); c.Disc(16f, 6f, 2f, metalDark);
            c.Disc(22f, 4f, 4f, ink); c.Disc(22f, 4f, 1.5f, metalDark);
            for (int x = 2; x < 102; x += 5)
            {
                int h = 3 + (int)(Hash(x, 1) % 5);
                c.Rect(x, 0, 1, h, PixelArt.Hex("4a8a3f"));
                c.Rect(x + 2, 0, 1, h - 1, PixelArt.Hex("6cbd5c"));
            }
            Add(c, new Vector2(0.5f, 0f), "workshop_ruin");
        }
    }
}
