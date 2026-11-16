using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Ornamen khas Indonesia: gerobak bakso, angkringan, warung tenda pecel lele, kios es kelapa;
    /// serta hewan taman: kupu-kupu (2 frame sayap) dan burung gereja (berdiri, mematuk, terbang).
    /// Pivot tengah bawah (kecuali hewan: tengah), 16 px = 1 unit.
    /// </summary>
    public static partial class LifeArt
    {
        static void BuildIndoArt()
        {
            MakeBaksoCart();
            MakeAngkringan();
            MakePecelTent();
            MakeCoconutKiosk();
            MakeCritters();
        }

        static readonly Color32 Ink2 = new Color32(0x2a, 0x24, 0x30, 255);

        /// <summary>Gerobak bakso: kotak kaca, panci kuah mengepul, roda, tulisan BAKSO.</summary>
        static void MakeBaksoCart()
        {
            var c = new PixelCanvas(56, 46);
            Color32 wood = PixelArt.Hex("d8584a"), woodDark = PixelArt.Hex("a8403a"), glass = PixelArt.Hex("bce5e8");
            c.Disc(12f, 6f, 5f, Ink2); c.Disc(12f, 6f, 2f, PixelArt.Hex("aab2b4"));          // roda
            c.Disc(44f, 6f, 5f, Ink2); c.Disc(44f, 6f, 2f, PixelArt.Hex("aab2b4"));
            Outlined(c, 4, 9, 48, 18, wood, Ink2);                                          // badan gerobak
            c.Rect(6, 11, 44, 3, woodDark);
            Outlined(c, 6, 27, 26, 12, glass, Ink2);                                        // etalase kaca
            for (int i = 0; i < 4; i++) c.Disc(11f + i * 5f, 31f, 2f, PixelArt.Hex("c8a46a")); // bakso
            c.Rect(8, 34, 22, 2, PixelArt.Hex("f4e2b0"));                                   // mi
            Outlined(c, 36, 27, 14, 11, PixelArt.Hex("9aa2ab"), Ink2);                      // panci kuah
            c.Rect(37, 37, 12, 2, PixelArt.Hex("6b737c"));
            for (int s = 0; s < 3; s++) c.Rect(39 + s * 4, 40 + s % 2, 1, 4, new Color32(255, 255, 255, 140)); // uap
            c.Rect(0, 14, 5, 2, PixelArt.Hex("8a5a3c"));                                    // pegangan dorong
            // tulisan BAKSO (huruf kecil 3x5) di badan gerobak
            DrawTiny(c, 10, 17, "BAKSO", PixelArt.Hex("fff1c0"));
            Add(c, BottomCenter, "cart_bakso");
        }

        /// <summary>Angkringan: gerobak kayu beratap terpal oranye, cerek, tikar & kursi panjang.</summary>
        static void MakeAngkringan()
        {
            var c = new PixelCanvas(72, 56);
            Color32 wood = PixelArt.Hex("8a5a3c"), woodLight = PixelArt.Hex("a8754a"), tarp = PixelArt.Hex("e8823a");
            c.Rect(6, 0, 3, 46, wood); c.Rect(63, 0, 3, 46, wood);                         // tiang
            for (int y = 44; y < 54; y++)                                                   // atap terpal
                c.Rect(2 + (y - 44) / 2, y, 68 - (y - 44), 1, y % 3 == 0 ? PixelArt.Hex("c96a2a") : tarp);
            Outlined(c, 14, 8, 44, 16, woodLight, Ink2);                                    // meja gerobak
            c.Rect(16, 22, 40, 3, wood);
            for (int i = 0; i < 6; i++) c.Disc(20f + i * 6f, 27f, 2.2f, i % 2 == 0 ? PixelArt.Hex("f4efe0") : PixelArt.Hex("6fae62")); // bungkus nasi kucing
            Outlined(c, 44, 25, 8, 9, PixelArt.Hex("5b5f66"), Ink2);                       // cerek
            c.Rect(52, 29, 3, 2, PixelArt.Hex("5b5f66"));
            c.Disc(30f, 36f, 3.5f, PixelArt.Hex("ffd98a"));                                // lampu senthir
            c.Rect(29, 39, 2, 5, Ink2);
            Outlined(c, 0, 0, 12, 7, wood, Ink2);                                           // kursi panjang
            Outlined(c, 60, 0, 12, 7, wood, Ink2);
            Add(c, BottomCenter, "angkringan");
        }

        /// <summary>Warung tenda pecel lele: tenda spanduk putih bergambar lele, meja & kursi plastik.</summary>
        static void MakePecelTent()
        {
            var c = new PixelCanvas(88, 60);
            Color32 banner = PixelArt.Hex("f4efe0"), red = PixelArt.Hex("d8584a"), blue = PixelArt.Hex("3b6fb6");
            c.Rect(4, 0, 2, 48, PixelArt.Hex("8a9098")); c.Rect(82, 0, 2, 48, PixelArt.Hex("8a9098"));
            for (int y = 46; y < 58; y++)                                                    // atap terpal biru
                c.Rect((y - 46), y, 88 - (y - 46) * 2, 1, y % 3 == 0 ? PixelArt.Hex("2a5590") : blue);
            Outlined(c, 6, 26, 76, 18, banner, Ink2);                                        // spanduk
            c.Rect(8, 40, 72, 2, red);
            DrawTiny(c, 12, 33, "PECEL LELE", red);
            // gambar ikan lele kecil
            c.Ellipse(66f, 33f, 7f, 3f, PixelArt.Hex("6b737c"));
            c.Triangle(new Vector2(58f, 33f), new Vector2(54f, 36f), new Vector2(54f, 30f), PixelArt.Hex("6b737c"));
            c.Set(70, 34, Ink2);
            c.Line(72, 32, 76, 30, Ink2, 1); c.Line(72, 33, 76, 35, Ink2, 1);                 // kumis
            // meja panjang + kursi plastik merah
            Outlined(c, 18, 8, 52, 6, PixelArt.Hex("d9cfbd"), Ink2);
            c.Rect(20, 0, 2, 8, Ink2); c.Rect(66, 0, 2, 8, Ink2);
            for (int i = 0; i < 4; i++)
            {
                Outlined(c, 14 + i * 16, 0, 7, 6, red, Ink2);
                c.Disc(30f + i * 10f, 16f, 2.5f, PixelArt.Hex("f4efe0"));                   // piring
            }
            Add(c, BottomCenter, "tent_pecel");
        }

        /// <summary>Kios es kelapa muda: meja bambu, tumpukan kelapa hijau, payung.</summary>
        static void MakeCoconutKiosk()
        {
            var c = new PixelCanvas(56, 50);
            Color32 bamboo = PixelArt.Hex("d8c27a"), bambooDark = PixelArt.Hex("a8904a");
            c.Rect(27, 0, 2, 40, PixelArt.Hex("8a5a3c"));                                   // tiang payung
            for (int y = 38; y < 48; y++)                                                    // payung bergaris
            {
                int half = 26 - (y - 38) * 2;
                for (int x = 28 - half; x < 28 + half; x++)
                    c.Set(x, y, ((x / 6) % 2 == 0) ? PixelArt.Hex("ff6b6b") : PixelArt.Hex("fff1d6"));
            }
            Outlined(c, 6, 6, 44, 12, bamboo, Ink2);                                         // meja bambu
            for (int x = 8; x < 48; x += 4) c.Rect(x, 7, 1, 10, bambooDark);
            c.Rect(8, 0, 2, 6, bambooDark); c.Rect(46, 0, 2, 6, bambooDark);
            for (int i = 0; i < 5; i++)                                                      // kelapa hijau
            {
                c.Disc(12f + i * 7f, 21f + (i % 2) * 2f, 4f, PixelArt.Hex("4f8a3a"));
                c.Disc(11f + i * 7f, 22f + (i % 2) * 2f, 1.5f, PixelArt.Hex("8fc06a"));
            }
            Outlined(c, 40, 18, 6, 9, PixelArt.Hex("bce5e8"), Ink2);                         // gelas es
            Add(c, BottomCenter, "kiosk_kelapa");
        }

        /// <summary>Huruf mini 3x5 untuk tulisan pada gerobak/spanduk.</summary>
        static void DrawTiny(PixelCanvas c, int x, int y, string text, Color32 col)
        {
            foreach (char ch in text)
            {
                string[] g = TinyGlyph(ch);
                for (int r = 0; r < 5; r++)
                    for (int k = 0; k < 3; k++)
                        if (g[r][k] == '#') c.Set(x + k, y + 4 - r, col);
                x += 4;
            }
        }

        static string[] TinyGlyph(char ch)
        {
            switch (ch)
            {
                case 'A': return new[] { "###", "#.#", "###", "#.#", "#.#" };
                case 'B': return new[] { "##.", "#.#", "##.", "#.#", "##." };
                case 'C': return new[] { "###", "#..", "#..", "#..", "###" };
                case 'E': return new[] { "###", "#..", "##.", "#..", "###" };
                case 'K': return new[] { "#.#", "#.#", "##.", "#.#", "#.#" };
                case 'L': return new[] { "#..", "#..", "#..", "#..", "###" };
                case 'O': return new[] { "###", "#.#", "#.#", "#.#", "###" };
                case 'P': return new[] { "###", "#.#", "###", "#..", "#.." };
                case 'S': return new[] { "###", "#..", "###", "..#", "###" };
                case 'D': return new[] { "##.", "#.#", "#.#", "#.#", "##." };
                case 'F': return new[] { "###", "#..", "##.", "#..", "#.." };
                case 'G': return new[] { "###", "#..", "#.#", "#.#", "###" };
                case 'H': return new[] { "#.#", "#.#", "###", "#.#", "#.#" };
                case 'I': return new[] { "###", ".#.", ".#.", ".#.", "###" };
                case 'J': return new[] { "..#", "..#", "..#", "#.#", "###" };
                case 'M': return new[] { "#.#", "###", "###", "#.#", "#.#" };
                case 'N': return new[] { "##.", "#.#", "#.#", "#.#", "#.#" };
                case 'Q': return new[] { "###", "#.#", "#.#", "###", "..#" };
                case 'R': return new[] { "##.", "#.#", "##.", "#.#", "#.#" };
                case 'T': return new[] { "###", ".#.", ".#.", ".#.", ".#." };
                case 'U': return new[] { "#.#", "#.#", "#.#", "#.#", "###" };
                case 'V': return new[] { "#.#", "#.#", "#.#", "#.#", ".#." };
                case 'W': return new[] { "#.#", "#.#", "###", "###", "#.#" };
                case 'X': return new[] { "#.#", "#.#", ".#.", "#.#", "#.#" };
                case 'Y': return new[] { "#.#", "#.#", ".#.", ".#.", ".#." };
                case 'Z': return new[] { "###", "..#", ".#.", "#..", "###" };
                case '0': return new[] { "###", "#.#", "#.#", "#.#", "###" };
                case '1': return new[] { ".#.", "##.", ".#.", ".#.", "###" };
                case '2': return new[] { "###", "..#", "###", "#..", "###" };
                case '3': return new[] { "###", "..#", ".##", "..#", "###" };
                case '4': return new[] { "#.#", "#.#", "###", "..#", "..#" };
                case '5': return new[] { "###", "#..", "###", "..#", "###" };
                case '6': return new[] { "###", "#..", "###", "#.#", "###" };
                case '7': return new[] { "###", "..#", "..#", ".#.", ".#." };
                case '8': return new[] { "###", "#.#", "###", "#.#", "###" };
                case '9': return new[] { "###", "#.#", "###", "..#", "###" };
                case '!': return new[] { ".#.", ".#.", ".#.", "...", ".#." };
                case '.': return new[] { "...", "...", "...", "...", ".#." };
                case '-': return new[] { "...", "...", "###", "...", "..." };
                default: return new[] { "...", "...", "...", "...", "..." };
            }
        }

        /// <summary>Kupu-kupu & burung gereja (+ kepulan debu langkah).</summary>
        static void MakeCritters()
        {
            // Kepulan debu lembut (diwarnai sesuai permukaan).
            var dust = new PixelCanvas(12, 8);
            dust.Ellipse(6f, 4f, 5.5f, 3.5f, new Color32(255, 255, 255, 120));
            dust.Ellipse(5f, 4.5f, 3.5f, 2.2f, new Color32(255, 255, 255, 200));
            Add(dust, new Vector2(0.5f, 0.3f), "dust");

            // Kupu-kupu putih (diwarnai lewat SpriteRenderer.color): sayap terbuka & tertutup.
            var b0 = new PixelCanvas(9, 7);
            b0.Rect(4, 1, 1, 5, Ink2);
            b0.Rect(0, 3, 4, 3, PixelArt.Hex("ffffff")); b0.Rect(5, 3, 4, 3, PixelArt.Hex("ffffff"));
            b0.Rect(1, 1, 3, 2, PixelArt.Hex("e8e8e8")); b0.Rect(5, 1, 3, 2, PixelArt.Hex("e8e8e8"));
            b0.Set(1, 4, Ink2); b0.Set(7, 4, Ink2);
            Add(b0, new Vector2(0.5f, 0.5f), "butterfly_0");
            var b1 = new PixelCanvas(9, 7);
            b1.Rect(4, 1, 1, 5, Ink2);
            b1.Rect(2, 3, 2, 3, PixelArt.Hex("ffffff")); b1.Rect(5, 3, 2, 3, PixelArt.Hex("ffffff"));
            b1.Rect(3, 1, 1, 2, PixelArt.Hex("e8e8e8")); b1.Rect(5, 1, 1, 2, PixelArt.Hex("e8e8e8"));
            Add(b1, new Vector2(0.5f, 0.5f), "butterfly_1");

            Color32 brown = PixelArt.Hex("8a5a3c"), light = PixelArt.Hex("d9b68a"), beak = PixelArt.Hex("e8b23a");
            // Burung gereja berdiri (menghadap kanan).
            var s0 = new PixelCanvas(10, 8);
            s0.Ellipse(4f, 3.5f, 3.5f, 2.5f, brown);
            s0.Ellipse(4f, 2.5f, 2.5f, 1.2f, light);
            s0.Disc(7f, 5.5f, 1.8f, brown);
            s0.Set(7, 6, Ink2); s0.Set(9, 5, beak);
            s0.Rect(0, 4, 2, 1, brown);
            s0.Set(3, 0, Ink2); s0.Set(5, 0, Ink2);
            Add(s0, new Vector2(0.5f, 0f), "bird_0");
            // Mematuk (kepala menunduk).
            var s1 = new PixelCanvas(10, 8);
            s1.Ellipse(4f, 3.5f, 3.5f, 2.5f, brown);
            s1.Ellipse(4f, 2.5f, 2.5f, 1.2f, light);
            s1.Disc(7.5f, 2.5f, 1.8f, brown);
            s1.Set(8, 3, Ink2); s1.Set(9, 1, beak);
            s1.Rect(0, 5, 2, 1, brown);
            s1.Set(3, 0, Ink2); s1.Set(5, 0, Ink2);
            Add(s1, new Vector2(0.5f, 0f), "bird_1");
            // Terbang: sayap naik & turun.
            var f0 = new PixelCanvas(11, 8);
            f0.Ellipse(5f, 3.5f, 3.5f, 2f, brown);
            f0.Line(2, 4, 4, 7, brown, 1); f0.Line(5, 4, 7, 7, light, 1);
            f0.Disc(8.5f, 4f, 1.5f, brown); f0.Set(10, 4, beak);
            Add(f0, new Vector2(0.5f, 0.5f), "bird_fly_0");
            var f1 = new PixelCanvas(11, 8);
            f1.Ellipse(5f, 4f, 3.5f, 2f, brown);
            f1.Line(2, 3, 4, 0, brown, 1); f1.Line(5, 3, 7, 0, light, 1);
            f1.Disc(8.5f, 4.5f, 1.5f, brown); f1.Set(10, 4, beak);
            Add(f1, new Vector2(0.5f, 0.5f), "bird_fly_1");
        }
    }
}
