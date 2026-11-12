using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure
{
    /// <summary>A tiny 5x7 bitmap font rendered to sprites, so all UI text matches the pixel art.</summary>
    public static class PixelFont
    {
        const int GlyphW = 5;
        const int GlyphH = 7;

        static readonly Dictionary<char, string[]> Glyphs = new Dictionary<char, string[]>();
        static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        static PixelFont() { BuildGlyphs(); }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetRuntimeCache() => Cache.Clear();

        public static Sprite Render(string text, Color mainColor, Color outlineColor, int scale = 2,
            Vector2? pivot = null)
        {
            if (string.IsNullOrEmpty(text)) text = " ";
            scale = Mathf.Max(1, scale);
            Vector2 resolvedPivot = pivot ?? new Vector2(0f, 0.5f);
            string pivotKey = resolvedPivot.x.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) +
                              "," + resolvedPivot.y.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
            string key = text + "|" + ColorUtility.ToHtmlStringRGBA(mainColor) + "|" +
                         ColorUtility.ToHtmlStringRGBA(outlineColor) + "|" + scale + "|" + pivotKey;
            if (Cache.TryGetValue(key, out Sprite cached) && cached != null) return cached;

            int pad = 1;
            int cell = 6 * scale;
            int contentW = text.Length * cell;
            int texW = contentW + pad * 2;
            int texH = GlyphH * scale + pad * 2;
            bool[] mask = new bool[texW * texH];

            for (int i = 0; i < text.Length; i++)
            {
                char ch = char.ToUpperInvariant(text[i]);
                if (!Glyphs.TryGetValue(ch, out string[] glyph)) glyph = Glyphs['?'];
                int originX = pad + i * cell;
                for (int row = 0; row < GlyphH; row++)
                {
                    string line = glyph[row];
                    for (int col = 0; col < GlyphW; col++)
                    {
                        if (col >= line.Length || line[col] != '#') continue;
                        int bx = originX + col * scale;
                        int by = pad + (GlyphH - 1 - row) * scale;
                        for (int yy = 0; yy < scale; yy++)
                            for (int xx = 0; xx < scale; xx++)
                                mask[(by + yy) * texW + bx + xx] = true;
                    }
                }
            }

            var pixels = new Color32[texW * texH];
            Color32 main = mainColor;
            Color32 outline = outlineColor;

            // outline: any empty pixel touching a masked pixel
            for (int y = 0; y < texH; y++)
            {
                for (int x = 0; x < texW; x++)
                {
                    if (mask[y * texW + x]) continue;
                    bool near = false;
                    for (int oy = -1; oy <= 1 && !near; oy++)
                        for (int ox = -1; ox <= 1; ox++)
                        {
                            int nx = x + ox, ny = y + oy;
                            if (nx < 0 || ny < 0 || nx >= texW || ny >= texH) continue;
                            if (mask[ny * texW + nx]) { near = true; break; }
                        }
                    if (near) pixels[y * texW + x] = outline;
                }
            }

            for (int i = 0; i < mask.Length; i++)
                if (mask[i]) pixels[i] = main;

            var tex = new Texture2D(texW, texH, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "pixel_text"
            };
            tex.SetPixels32(pixels);
            tex.Apply(false, false);

            var sprite = Sprite.Create(tex, new Rect(0, 0, texW, texH),
                resolvedPivot, PixelArt.PPU, 0, SpriteMeshType.FullRect);
            sprite.name = "text_" + text;
            Cache[key] = sprite;
            return sprite;
        }

        static void G(char ch, params string[] rows) { Glyphs[ch] = rows; }

        static void BuildGlyphs()
        {
            G(' ', ".....", ".....", ".....", ".....", ".....", ".....", ".....");
            G('A', ".###.", "#...#", "#...#", "#####", "#...#", "#...#", "#...#");
            G('B', "####.", "#...#", "#...#", "####.", "#...#", "#...#", "####.");
            G('C', ".####", "#....", "#....", "#....", "#....", "#....", ".####");
            G('D', "####.", "#...#", "#...#", "#...#", "#...#", "#...#", "####.");
            G('E', "#####", "#....", "#....", "####.", "#....", "#....", "#####");
            G('F', "#####", "#....", "#....", "####.", "#....", "#....", "#....");
            G('G', ".####", "#....", "#....", "#.###", "#...#", "#...#", ".####");
            G('H', "#...#", "#...#", "#...#", "#####", "#...#", "#...#", "#...#");
            G('I', "#####", "..#..", "..#..", "..#..", "..#..", "..#..", "#####");
            G('J', "....#", "....#", "....#", "....#", "#...#", "#...#", ".###.");
            G('K', "#...#", "#..#.", "#.#..", "##...", "#.#..", "#..#.", "#...#");
            G('L', "#....", "#....", "#....", "#....", "#....", "#....", "#####");
            G('M', "#...#", "##.##", "#.#.#", "#...#", "#...#", "#...#", "#...#");
            G('N', "#...#", "##..#", "#.#.#", "#..##", "#...#", "#...#", "#...#");
            G('O', ".###.", "#...#", "#...#", "#...#", "#...#", "#...#", ".###.");
            G('P', "####.", "#...#", "#...#", "####.", "#....", "#....", "#....");
            G('Q', ".###.", "#...#", "#...#", "#...#", "#.#.#", "#..#.", ".##.#");
            G('R', "####.", "#...#", "#...#", "####.", "#.#..", "#..#.", "#...#");
            G('S', ".####", "#....", "#....", ".###.", "....#", "....#", "####.");
            G('T', "#####", "..#..", "..#..", "..#..", "..#..", "..#..", "..#..");
            G('U', "#...#", "#...#", "#...#", "#...#", "#...#", "#...#", ".###.");
            G('V', "#...#", "#...#", "#...#", "#...#", "#...#", ".#.#.", "..#..");
            G('W', "#...#", "#...#", "#...#", "#...#", "#.#.#", "##.##", "#...#");
            G('X', "#...#", "#...#", ".#.#.", "..#..", ".#.#.", "#...#", "#...#");
            G('Y', "#...#", "#...#", ".#.#.", "..#..", "..#..", "..#..", "..#..");
            G('Z', "#####", "....#", "...#.", "..#..", ".#...", "#....", "#####");
            G('0', ".###.", "#...#", "#..##", "#.#.#", "##..#", "#...#", ".###.");
            G('1', "..#..", ".##..", "..#..", "..#..", "..#..", "..#..", ".###.");
            G('2', ".###.", "#...#", "....#", "...#.", "..#..", ".#...", "#####");
            G('3', "####.", "....#", "....#", ".###.", "....#", "....#", "####.");
            G('4', "...#.", "..##.", ".#.#.", "#..#.", "#####", "...#.", "...#.");
            G('5', "#####", "#....", "####.", "....#", "....#", "#...#", ".###.");
            G('6', ".###.", "#....", "#....", "####.", "#...#", "#...#", ".###.");
            G('7', "#####", "....#", "...#.", "..#..", ".#...", ".#...", ".#...");
            G('8', ".###.", "#...#", "#...#", ".###.", "#...#", "#...#", ".###.");
            G('9', ".###.", "#...#", "#...#", ".####", "....#", "....#", ".###.");
            G(':', ".....", "..#..", "..#..", ".....", "..#..", "..#..", ".....");
            G('/', "....#", "....#", "...#.", "..#..", ".#...", "#....", "#....");
            G('!', "..#..", "..#..", "..#..", "..#..", "..#..", ".....", "..#..");
            G('.', ".....", ".....", ".....", ".....", ".....", ".....", "..#..");
            G(',', ".....", ".....", ".....", ".....", ".....", "..#..", ".#...");
            G('-', ".....", ".....", ".....", "#####", ".....", ".....", ".....");
            G('?', ".###.", "#...#", "....#", "...#.", "..#..", ".....", "..#..");
            G('+', ".....", "..#..", "..#..", "#####", "..#..", "..#..", ".....");
            G('>', ".#...", "..#..", "...#.", "....#", "...#.", "..#..", ".#...");
            G('<', "...#.", "..#..", ".#...", "#....", ".#...", "..#..", "...#.");
            G('\'', "..#..", "..#..", ".....", ".....", ".....", ".....", ".....");
            G('&', ".##..", "#..#.", "#.#..", ".#...", "#.#.#", "#..#.", ".##.#");
            G('%', "##..#", "##.#.", "...#.", "..#..", ".#...", "#.##.", "#..##");
            G('(', "...#.", "..#..", ".#...", ".#...", ".#...", "..#..", "...#.");
            G(')', ".#...", "..#..", "...#.", "...#.", "...#.", "..#..", ".#...");
            G('"', ".#.#.", ".#.#.", ".....", ".....", ".....", ".....", ".....");
            G(';', ".....", "..#..", "..#..", ".....", "..#..", "..#..", ".#...");
            G('=', ".....", ".....", "#####", ".....", "#####", ".....", ".....");
            G('_', ".....", ".....", ".....", ".....", ".....", ".....", "#####");
            G('*', ".....", "#.#.#", ".###.", "#####", ".###.", "#.#.#", ".....");
            G('[', "..###", "..#..", "..#..", "..#..", "..#..", "..#..", "..###");
            G(']', "###..", "..#..", "..#..", "..#..", "..#..", "..#..", "###..");
            // Bintang penanda misi (dipakai di menu tujuan bus).
            G('★', "..#..", "..#..", "#####", ".###.", ".###.", "##.##", "#...#");
            // Ikon daftar tugas: segitiga (bisa), centang (selesai), gembok (terkunci).
            G('▶', "#....", "##...", "###..", "####.", "###..", "##...", "#....");
            G('✓', ".....", "....#", "...##", "#.##.", "###..", ".#...", ".....");
            G('▲', ".....", "..#..", ".###.", "#####", ".....", ".....", ".....");
            G('▼', ".....", ".....", ".....", "#####", ".###.", "..#..", ".....");
            G('▣',".###.", "#...#", "#...#", "#####", "##.##", "##.##", "#####");
        }
    }
}
