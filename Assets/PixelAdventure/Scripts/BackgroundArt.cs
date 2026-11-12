using UnityEngine;

namespace PixelAdventure
{
    public static partial class PixelArt
    {
        static void BuildBackgrounds()
        {
            BuildSky("sky_grove", "4aa8e8", "bfe9ff");
            BuildSky("sky_cave", "0b0c24", "2a2050");
            BuildSky("sky_ruins", "2a1a4e", "ff9a5a");

            BuildHills("hills_grove_far", 101, "8bbfa1", "c8e6cf", false, 92f);
            BuildHills("hills_grove_near", 202, "4f9166", "77b884", false, 58f);
            BuildHills("hills_cave_far", 303, "1a1938", "332d5c", true, 84f);
            BuildHills("hills_cave_near", 404, "16152c", "2b2750", true, 62f);
            BuildRuins("ruins_far", 505, "6e3d5f", "a45a70");
            BuildRuins("ruins_near", 606, "3c2340", "5f3450");
        }

        static void BuildSky(string key, string topHex, string bottomHex)
        {
            var c = new PixelCanvas(4, 256);
            c.VGradient(0, 0, 4, 256, Hex(bottomHex), Hex(topHex));
            c.ToSprite(new Vector2(0.5f, 0.5f), PPU, key, FilterMode.Bilinear);
        }

        static void BuildHills(string key, int seed, string bottomHex, string topHex,
            bool jagged, float baseHeight)
        {
            const int w = 768;
            const int h = 192;
            var rng = new System.Random(seed);
            var c = new PixelCanvas(w, h);
            Color32 bottom = Hex(bottomHex);
            Color32 top = Hex(topHex);

            float phase = rng.Next(0, 1000) * 0.1f;
            int[] heights = new int[w];
            for (int x = 0; x < w; x++)
            {
                float t = x / (float)w;
                float noise = Mathf.Sin(t * 22f + phase) * 14f
                            + Mathf.Sin(t * 57f + phase * 2.1f) * 7f
                            + Mathf.Sin(t * 131f + phase * 0.7f) * 3.5f;
                float value;
                if (jagged)
                {
                    float peak = Mathf.Abs(Mathf.Sin(t * 13f + phase)) * 42f
                               + Mathf.Abs(Mathf.Sin(t * 31f + phase * 1.7f)) * 20f;
                    value = baseHeight + peak + noise;
                }
                else
                {
                    value = baseHeight + noise * 1.8f + Mathf.Sin(t * 7f + phase) * 12f;
                }
                heights[x] = Mathf.Clamp(Mathf.RoundToInt(value), 10, h - 4);
            }

            for (int x = 0; x < w; x++)
            {
                int height = heights[x];
                for (int y = 0; y < height; y++)
                {
                    float t = y / (float)Mathf.Max(1, height);
                    Color32 col = Color32.Lerp(bottom, top, t * t);
                    c.Set(x, y, col);
                }
                // rim light
                Color32 rim = Color32.Lerp(top, new Color32(255, 255, 255, 255), 0.22f);
                c.Set(x, height - 1, rim);
                if (height > 2) c.Set(x, height - 2, Color32.Lerp(top, rim, 0.4f));
            }

            // texture: sparse darker cracks / rock bands
            for (int i = 0; i < w * 3; i++)
            {
                int x = rng.Next(0, w);
                int maxY = heights[x] - 2;
                if (maxY < 2) continue;
                int y = rng.Next(1, maxY);
                Color32 col = c.Get(x, y);
                float shade = rng.Next(2) == 0 ? 0.9f : 1.06f;
                c.Set(x, y, new Color32(
                    (byte)Mathf.Clamp(col.r * shade, 0f, 255f),
                    (byte)Mathf.Clamp(col.g * shade, 0f, 255f),
                    (byte)Mathf.Clamp(col.b * shade, 0f, 255f), col.a));
            }

            c.ToSprite(new Vector2(0.5f, 0f), PPU, key);
        }

        static void BuildRuins(string key, int seed, string bottomHex, string topHex)
        {
            const int w = 768;
            const int h = 192;
            var rng = new System.Random(seed);
            var c = new PixelCanvas(w, h);
            Color32 bottom = Hex(bottomHex);
            Color32 top = Hex(topHex);

            int x = 0;
            while (x < w)
            {
                int blockW = rng.Next(18, 54);
                int blockH = rng.Next(70, 150);
                bool tower = rng.Next(5) == 0;
                if (tower) blockH = rng.Next(130, 182);

                for (int bx = x; bx < x + blockW && bx < w; bx++)
                {
                    for (int y = 0; y < blockH; y++)
                    {
                        float t = y / (float)blockH;
                        Color32 col = Color32.Lerp(bottom, top, t * 0.9f);
                        c.Set(bx, y, col);
                    }
                    if (blockH > 1) c.Set(bx, blockH - 1, Color32.Lerp(top, new Color32(255, 255, 255, 255), 0.18f));
                }

                // broken top notch
                if (!tower && blockW > 14)
                {
                    int notch = rng.Next(6, 16);
                    int notchStart = x + rng.Next(2, Mathf.Max(3, blockW - notch - 2));
                    for (int bx = notchStart; bx < notchStart + notch && bx < w; bx++)
                        for (int y = blockH - 9; y < blockH; y++)
                            c.Set(bx, y, new Color32(0, 0, 0, 0));
                }

                // windows / arches, darker
                if (blockW > 26 && rng.Next(2) == 0)
                {
                    int ww = rng.Next(5, 9);
                    int wh = rng.Next(10, 18);
                    int wx = x + rng.Next(5, blockW - ww - 5);
                    int wy = rng.Next(30, Mathf.Max(35, blockH - wh - 10));
                    for (int iy = wy; iy < wy + wh; iy++)
                        for (int ix = wx; ix < wx + ww; ix++)
                        {
                            Color32 col = c.Get(ix, iy);
                            c.Set(ix, iy, new Color32((byte)(col.r * 0.55f),
                                (byte)(col.g * 0.55f), (byte)(col.b * 0.6f), 255));
                        }
                }

                x += blockW + rng.Next(0, 4);
            }

            c.ToSprite(new Vector2(0.5f, 0f), PPU, key);
        }
    }
}
