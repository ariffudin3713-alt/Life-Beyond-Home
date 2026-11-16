using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Ciri khas Indonesia (2): papan "DILARANG BUANG SAMPAH SEMBARANGAN", spanduk kebersihan,
    /// tong sampah tiga warna, bendera merah putih & umbul-umbul, gapura "SELAMAT DATANG",
    /// becak, gerobak sayur, warung kopi (kopi sachet berenceng), jemuran, pohon pisang,
    /// kucing oren, ayam kampung, dan sampah berserakan. Pivot tengah bawah, 16 px = 1 unit.
    /// </summary>
    public static partial class LifeArt
    {
        static void BuildIndoArt2()
        {
            Color32 red = PixelArt.Hex("d8303a"), white = PixelArt.Hex("f6f4ee");
            Color32 pole = PixelArt.Hex("8a9098"), wood = PixelArt.Hex("8a5a3c"), woodLight = PixelArt.Hex("a8754a");
            Color32 bamboo = PixelArt.Hex("d8c27a"), bambooDark = PixelArt.Hex("a8904a");

            // ---- papan larangan buang sampah
            var c = new PixelCanvas(56, 38);
            c.Rect(8, 0, 2, 16, pole); c.Rect(46, 0, 2, 16, pole);
            Outlined(c, 1, 14, 54, 23, white, Ink2);
            c.FrameRect(2, 15, 52, 21, red);
            DrawTiny(c, 12, 29, "DILARANG", red);
            DrawTiny(c, 4, 23, "BUANG SAMPAH", Ink2);
            DrawTiny(c, 6, 17, "SEMBARANGAN", red);
            Add(c, BottomCenter, "sign_litter");

            // ---- spanduk "JAGA KEBERSIHAN" di antara dua tiang bambu
            c = new PixelCanvas(72, 40);
            c.Rect(2, 0, 3, 40, bamboo); c.Rect(67, 0, 3, 40, bamboo);
            for (int y = 6; y < 40; y += 7) { c.Rect(2, y, 3, 1, bambooDark); c.Rect(67, y, 3, 1, bambooDark); }
            Outlined(c, 5, 22, 62, 15, PixelArt.Hex("3f8a4c"), Ink2);
            c.Rect(6, 23, 60, 2, PixelArt.Hex("2f6a3a"));
            DrawTiny(c, 6, 29, "JAGA KEBERSIHAN", white);
            c.Line(5, 36, 2, 39, Ink2); c.Line(66, 36, 69, 39, Ink2);                   // tali
            Add(c, BottomCenter, "banner_clean");

            // ---- tong sampah tiga warna: organik (hijau), anorganik (kuning), B3 (merah)
            c = new PixelCanvas(36, 20);
            Color32[] bin = { PixelArt.Hex("3f9a4c"), PixelArt.Hex("f0c02c"), PixelArt.Hex("d8303a") };
            for (int i = 0; i < 3; i++)
            {
                int bx = 1 + i * 12;
                Outlined(c, bx, 0, 10, 15, bin[i], Ink2);
                Outlined(c, bx - 1, 14, 12, 4, Color32.Lerp(bin[i], Ink2, 0.3f), Ink2);   // tutup
                c.Rect(bx + 3, 18, 4, 1, Ink2);
                c.Rect(bx + 3, 5, 4, 4, white);                                          // simbol
                c.Rect(bx + 4, 6, 2, 2, bin[i]);
            }
            Add(c, BottomCenter, "bins3");

            // ---- bendera merah putih bertiang
            c = new PixelCanvas(20, 64);
            Outlined(c, 2, 0, 10, 4, PixelArt.Hex("aab2b4"), Ink2);
            c.Rect(6, 4, 2, 58, white);
            c.Rect(8, 54, 11, 4, red); c.Rect(8, 50, 11, 4, white); c.FrameRect(8, 50, 11, 8, PixelArt.Hex("c8c4ba"));
            c.Rect(8, 54, 11, 4, red);
            c.Disc(7f, 62f, 1.6f, PixelArt.Hex("e8b23a"));
            Add(c, BottomCenter, "flag_pole");

            // ---- umbul-umbul (bambu melengkung, kain panjang berwarna)
            for (int v = 0; v < 2; v++)
            {
                c = new PixelCanvas(18, 70);
                for (int y = 0; y < 62; y++) c.Rect(4 + (y > 50 ? (y - 50) / 3 : 0), y, 2, 1, (y / 7) % 2 == 0 ? bamboo : bambooDark);
                Color32 a = v == 0 ? red : PixelArt.Hex("f0c02c"), b = v == 0 ? white : PixelArt.Hex("3b6fb6");
                for (int y = 20; y < 60; y++)
                {
                    int sway = (int)(Mathf.Sin(y * 0.25f + v) * 1.2f);
                    c.Rect(8 + sway, y, 5, 1, ((y - 20) / 5) % 2 == 0 ? a : b);
                }
                c.Triangle(new Vector2(8f, 20f), new Vector2(13f, 20f), new Vector2(10.5f, 14f), a);
                c.Rect(6, 59, 8, 1, Ink2);
                Add(c, BottomCenter, "umbul_" + v);
            }

            // ---- gapura "SELAMAT DATANG" (merah putih), celah tengah 68 px untuk jalan
            c = new PixelCanvas(88, 76);
            for (int side = 0; side < 2; side++)
            {
                int px = side == 0 ? 0 : 78;
                Outlined(c, px, 0, 10, 4, PixelArt.Hex("8a8478"), Ink2);
                for (int y = 4; y < 54; y++) c.Rect(px + 1, y, 8, 1, ((y - 4) / 8) % 2 == 0 ? red : white);
                c.Rect(px, 4, 1, 50, Ink2); c.Rect(px + 9, 4, 1, 50, Ink2);
                c.Triangle(new Vector2(px - 1, 72f), new Vector2(px + 11, 72f), new Vector2(px + 5, 76f), red);   // puncak tiang
                Outlined(c, px - 1, 66, 12, 6, white, Ink2);
            }
            Outlined(c, 6, 52, 76, 14, red, Ink2);                                       // papan atas
            c.Rect(8, 54, 72, 10, PixelArt.Hex("b82830"));
            DrawTiny(c, 16, 57, "SELAMAT DATANG", white);
            for (int x = 10; x < 80; x += 8) c.Rect(x, 50, 4, 2, white);                 // hiasan bawah
            Add(c, BottomCenter, "gapura");

            // ---- becak (penumpang di depan, pengayuh di belakang), menghadap kanan
            c = new PixelCanvas(44, 36);
            c.Disc(8f, 7f, 7f, Ink2); c.Disc(8f, 7f, 5.5f, new Color32(0, 0, 0, 0)); c.Disc(8f, 7f, 1.5f, pole);
            c.Disc(34f, 5f, 5f, Ink2); c.Disc(34f, 5f, 3.5f, new Color32(0, 0, 0, 0)); c.Disc(34f, 5f, 1.2f, pole);
            c.Line(8, 7, 22, 10, Ink2, 2);                                               // rangka
            c.Rect(5, 14, 7, 2, Ink2); c.Line(8, 7, 8, 14, Ink2, 1);                     // sadel pengayuh
            Outlined(c, 20, 8, 22, 10, PixelArt.Hex("3b6fb6"), Ink2);                    // bak penumpang
            for (int i = 0; i < 3; i++) c.Disc(25f + i * 6f, 13f, 1.6f, PixelArt.Hex("ffd23f")); // hiasan lukis
            Outlined(c, 22, 17, 18, 5, PixelArt.Hex("d8303a"), Ink2);                    // jok
            c.Rect(20, 18, 2, 14, Ink2);
            for (int y = 28; y < 35; y++) c.Rect(20, y, 22 - (y - 28), 1, y % 2 == 0 ? PixelArt.Hex("e8823a") : PixelArt.Hex("d06a2a")); // tenda
            Add(c, BottomCenter, "becak");

            // ---- gerobak sayur keliling
            c = new PixelCanvas(48, 32);
            c.Disc(10f, 5f, 4.5f, Ink2); c.Disc(10f, 5f, 1.8f, pole);
            c.Disc(38f, 5f, 4.5f, Ink2); c.Disc(38f, 5f, 1.8f, pole);
            Outlined(c, 4, 8, 40, 10, woodLight, Ink2);
            c.Rect(0, 14, 5, 2, wood);
            Color32[] veg = { PixelArt.Hex("4f9a3c"), PixelArt.Hex("e8823a"), PixelArt.Hex("d8303a"), PixelArt.Hex("8a63ad"), PixelArt.Hex("f0d060"), PixelArt.Hex("6ec04a") };
            for (int i = 0; i < 9; i++) c.Disc(8f + i * 4.2f, 19f + (i % 2) * 1.5f, 2.6f, veg[i % veg.Length]);
            c.Rect(6, 22, 2, 8, wood); c.Rect(40, 22, 2, 8, wood);
            for (int i = 0; i < 4; i++) c.Rect(9 + i * 8, 26, 4, 3, i % 2 == 0 ? PixelArt.Hex("f4efe0") : PixelArt.Hex("ffd23f")); // plastik gantung
            c.Rect(6, 29, 36, 2, wood);
            Add(c, BottomCenter, "cart_sayur");

            // ---- warung kopi: atap seng, kopi sachet berenceng, bangku panjang, papan WARKOP
            c = new PixelCanvas(68, 56);
            c.Rect(4, 0, 3, 44, wood); c.Rect(61, 0, 3, 44, wood);
            for (int y = 42; y < 52; y++)
                c.Rect(1, y, 66, 1, (y % 2 == 0) ? PixelArt.Hex("9aa2ab") : PixelArt.Hex("bfc6cc"));   // seng
            Outlined(c, 10, 8, 48, 14, woodLight, Ink2);                                  // meja warung
            c.Rect(12, 20, 44, 2, wood);
            for (int i = 0; i < 4; i++)                                                   // toples kerupuk
                Outlined(c, 14 + i * 7, 22, 6, 7, PixelArt.Hex("d8f0f4"), Ink2);
            Color32[] sachet = { PixelArt.Hex("8a3a2a"), PixelArt.Hex("e0383c"), PixelArt.Hex("f0c02c"), PixelArt.Hex("3b6fb6"), PixelArt.Hex("1d1b22") };
            for (int s = 0; s < 8; s++)                                                   // renceng kopi
                for (int k = 0; k < 4; k++)
                    c.Rect(9 + s * 7, 38 - k * 4, 4, 3, sachet[(s + k) % sachet.Length]);
            Outlined(c, 44, 22, 8, 8, PixelArt.Hex("5b5f66"), Ink2);                      // termos
            Outlined(c, 20, 44, 28, 8, white, Ink2);
            DrawTiny(c, 24, 46, "WARKOP", red);
            Outlined(c, 0, 0, 14, 6, wood, Ink2); Outlined(c, 54, 0, 14, 6, wood, Ink2); // bangku
            Add(c, BottomCenter, "warkop");

            // ---- jemuran baju
            c = new PixelCanvas(60, 34);
            c.Rect(2, 0, 2, 32, wood); c.Rect(56, 0, 2, 32, wood);
            c.Line(3, 29, 57, 27, Ink2);
            Color32[] cl = { PixelArt.Hex("e46f5a"), PixelArt.Hex("6fb7ff"), PixelArt.Hex("ffd23f"), PixelArt.Hex("f4efe0"), PixelArt.Hex("4fae62"), PixelArt.Hex("c79ad8") };
            for (int i = 0; i < 6; i++)
            {
                int x = 7 + i * 8, top = 28 - i * 2 / 6;
                int hgt = i % 3 == 1 ? 14 : 10;                                            // sarung lebih panjang
                Outlined(c, x, top - hgt, 6, hgt, cl[i], Ink2);
                if (i % 3 == 0) { c.Rect(x - 1, top - 3, 1, 3, cl[i]); c.Rect(x + 6, top - 3, 1, 3, cl[i]); } // lengan baju
            }
            Add(c, BottomCenter, "jemuran");

            // ---- pohon pisang
            c = new PixelCanvas(40, 50);
            c.Rect(18, 0, 5, 30, PixelArt.Hex("8aa04a")); c.Rect(19, 0, 1, 30, PixelArt.Hex("6e8a3a"));
            Color32 leaf = PixelArt.Hex("5aa83c"), leafDark = PixelArt.Hex("3f8a2e");
            c.Line(20, 30, 2, 40, leaf, 4); c.Line(20, 30, 38, 42, leaf, 4);
            c.Line(20, 30, 6, 48, leafDark, 4); c.Line(20, 30, 32, 49, leafDark, 4);
            c.Line(20, 30, 20, 49, leaf, 4);
            c.Line(20, 30, 2, 40, leafDark, 1); c.Line(20, 30, 38, 42, leafDark, 1);
            for (int i = 0; i < 3; i++) c.Disc(25f, 22f - i * 3f, 2f, PixelArt.Hex("e0d050"));   // tandan pisang
            c.Disc(25f, 13f, 2f, PixelArt.Hex("8a3a5a"));                                 // jantung pisang
            Add(c, BottomCenter, "banana");

            // ---- kucing oren (duduk) & kucing tidur
            c = new PixelCanvas(12, 12);
            Color32 orange = PixelArt.Hex("f0963a"), orangeDark = PixelArt.Hex("c8702a");
            c.Ellipse(5f, 3.5f, 4f, 3.5f, orange);
            c.Disc(6.5f, 8f, 3f, orange);
            c.Set(4, 11, orange); c.Set(8, 11, orange); c.Set(4, 10, orange); c.Set(8, 10, orange);
            c.Set(5, 8, Ink2); c.Set(8, 8, Ink2); c.Set(6, 7, PixelArt.Hex("ff9fb0"));
            c.Rect(3, 3, 4, 1, orangeDark); c.Rect(2, 5, 3, 1, orangeDark);
            c.Line(1, 1, 0, 5, orange, 1);                                                 // ekor
            Add(c, BottomCenter, "cat_sit");
            c = new PixelCanvas(16, 8);
            c.Ellipse(8f, 3f, 7f, 3f, orange);
            c.Disc(13f, 3.5f, 2.5f, orange); c.Set(14, 6, orange); c.Set(12, 6, orange);
            c.Rect(5, 3, 1, 3, orangeDark); c.Rect(8, 3, 1, 3, orangeDark);
            c.Rect(12, 3, 2, 1, Ink2);                                                      // mata terpejam
            Add(c, BottomCenter, "cat_sleep");

            // ---- ayam kampung: berdiri & mematuk (menghadap kanan)
            for (int f = 0; f < 2; f++)
            {
                c = new PixelCanvas(12, 12);
                Color32 body = PixelArt.Hex("b8582a"), tail = PixelArt.Hex("2a4a3a");
                c.Rect(4, 0, 1, 3, PixelArt.Hex("e8b23a")); c.Rect(7, 0, 1, 3, PixelArt.Hex("e8b23a"));
                c.Ellipse(5f, 5f, 4f, 2.8f, body);
                c.Triangle(new Vector2(1f, 5f), new Vector2(0f, 10f), new Vector2(3f, 7f), tail);
                if (f == 0)
                {
                    c.Disc(9f, 8f, 1.8f, body); c.Set(9, 10, red); c.Set(10, 10, red); c.Set(11, 8, PixelArt.Hex("e8b23a")); c.Set(9, 8, Ink2);
                }
                else
                {
                    c.Disc(9f, 4f, 1.8f, body); c.Set(9, 6, red); c.Set(11, 3, PixelArt.Hex("e8b23a")); c.Set(9, 4, Ink2);
                }
                Add(c, BottomCenter, "chicken_" + f);
            }

            // ---- sampah berserakan: botol plastik, bungkus, kaleng, kertas
            c = new PixelCanvas(9, 5); Outlined(c, 0, 0, 7, 4, PixelArt.Hex("bfe6f0"), PixelArt.Hex("6fa8c0")); c.Rect(7, 1, 2, 2, PixelArt.Hex("3b6fb6"));
            Add(c, new Vector2(0.5f, 0.2f), "litter_0");
            c = new PixelCanvas(7, 5); Outlined(c, 0, 0, 7, 5, PixelArt.Hex("e0383c"), Ink2); c.Rect(2, 2, 3, 1, PixelArt.Hex("ffd23f"));
            Add(c, new Vector2(0.5f, 0.2f), "litter_1");
            c = new PixelCanvas(6, 4); Outlined(c, 0, 0, 6, 4, PixelArt.Hex("aab2b4"), Ink2); c.Rect(1, 1, 2, 2, PixelArt.Hex("3f9a4c"));
            Add(c, new Vector2(0.5f, 0.2f), "litter_2");
            c = new PixelCanvas(7, 6); c.Disc(3.5f, 3f, 3f, PixelArt.Hex("f4efe0")); c.Set(2, 3, PixelArt.Hex("aaa49a")); c.Set(4, 2, PixelArt.Hex("aaa49a"));
            Add(c, new Vector2(0.5f, 0.2f), "litter_3");
        }
    }
}
