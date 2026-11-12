using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>Tanah (ground) untuk lima wilayah PULANG. Satu peta 96x72 per wilayah.</summary>
    public static partial class LifeArt
    {
        public const int MapW = 96;
        public const int MapH = 72;

        public const string GroundDesa = "desa";
        public const string GroundKota = "kota";
        public const string GroundKampung = "kampung";
        public const string GroundPasar = "pasar";
        public const string GroundPantai = "pantai";

        static Color32 Grass => PixelArt.Hex("5fae53");
        static Color32 GrassDark => PixelArt.Hex("4e9a45");
        static Color32 GrassLight => PixelArt.Hex("74c066");
        static Color32 Dirt => PixelArt.Hex("c9a06a");
        static Color32 DirtDark => PixelArt.Hex("a97c4c");
        static Color32 DirtLight => PixelArt.Hex("dcb883");

        static void BuildGround()
        {
            BuildDesaGround();
            BuildKotaGround();
            BuildKampungGround();
            BuildPasarGround();
            BuildPantaiGround();
        }

        static PixelCanvas GrassCanvas(int seed)
        {
            var c = new PixelCanvas(MapW * 16, MapH * 16);
            for (int y = 0; y < c.Height; y++)
                for (int x = 0; x < c.Width; x++)
                {
                    uint h = Hash(x / 3 + seed, y / 3);
                    c.Set(x, y, h % 9 == 0 ? GrassDark : h % 13 == 0 ? GrassLight : Grass);
                }
            for (int i = 0; i < 260; i++)
            {
                int x = (int)(Hash(i, 7 + seed) % (uint)c.Width);
                int y = (int)(Hash(i, 31 + seed) % (uint)c.Height);
                c.Set(x, y, i % 3 == 0 ? PixelArt.Hex("ffd35c")
                    : i % 3 == 1 ? PixelArt.Hex("ff9fc0") : PixelArt.Hex("e8f6ff"));
            }
            return c;
        }

        static void Field(PixelCanvas c, int x, int y, int w, int h)
        {
            c.Rect(x * 16, y * 16, w * 16, h * 16, PixelArt.Hex("6cbd5c"));
            c.FrameRect(x * 16 + 8, y * 16 + 8, w * 16 - 16, h * 16 - 16, PixelArt.Hex("eef6e0"));
            c.Rect(x * 16 + w * 8, y * 16 + 8, 2, h * 16 - 16, PixelArt.Hex("eef6e0"));
        }

        static void Paddy(PixelCanvas c, int x, int y, int w, int h)
        {
            Color32 water = PixelArt.Hex("6fae9a");
            Color32 rice = PixelArt.Hex("8fd16a");
            Color32 dike = PixelArt.Hex("a98a55");
            c.Rect(x * 16, y * 16, w * 16, h * 16, dike);
            for (int py = y * 16 + 3; py < (y + h) * 16 - 3; py += 40)
                for (int px = x * 16 + 3; px < (x + w) * 16 - 3; px += 56)
                {
                    int bw = Mathf.Min(50, (x + w) * 16 - 3 - px);
                    int bh = Mathf.Min(34, (y + h) * 16 - 3 - py);
                    c.Rect(px, py, bw, bh, water);
                    for (int r = py + 3; r < py + bh - 2; r += 5)
                        for (int q = px + 3; q < px + bw - 2; q += 6)
                            c.Rect(q, r, 2, 3, rice);
                }
        }

        static void BuildDesaGround()
        {
            var c = GrassCanvas(0);
            // jalan desa, jalan ke sawah & kebun, dan halaman rumah menyatu dalam satu jaringan
            DirtNetwork(c, 7, T(0, 30, MapW, 3), T(50, 0, 3, MapH), T(13, 33, 2, 3));
            Field(c, 8, 48, 24, 16);                 // lapangan desa
            Paddy(c, 58, 4, 34, 22);                 // sawah
            c.Rect(62 * 16, 46 * 16, 30 * 16, 20 * 16, PixelArt.Hex("4a8a3f")); // perkebunan
            Add(c, Vector2.zero, "ground_" + GroundDesa);
        }

        static void BuildKotaGround()
        {
            var c = new PixelCanvas(MapW * 16, MapH * 16);
            for (int y = 0; y < c.Height; y++)
                for (int x = 0; x < c.Width; x++)
                {
                    uint h = Hash(x / 3, y / 3 + 5);
                    c.Set(x, y, h % 8 == 0 ? PixelArt.Hex("a79f92") : PixelArt.Hex("bcb4a6"));
                }
            // Trotoar dulu di keempat sisi jalan, lalu jalan menimpanya di persimpangan
            // (trotoar tidak lagi melintang di atas jalan simpang).
            Pavement(c, 0, 28 * 16, c.Width, 2 * 16);
            Pavement(c, 0, 38 * 16, c.Width, 2 * 16);
            Pavement(c, 42 * 16, 0, 2 * 16, c.Height);
            Pavement(c, 52 * 16, 0, 2 * 16, c.Height);
            AsphaltNetwork(c, 23, true,
                T(0, 30, MapW, 8),      // jalan utama
                T(44, 0, 8, MapH));     // jalan simpang

            // Taman Pelangi
            c.Ellipse(18 * 16, 58 * 16, 14 * 16, 10 * 16, PixelArt.Hex("3f8639"));
            c.Ellipse(18 * 16, 58 * 16, 13 * 16, 9 * 16, Grass);
            Color32[] rainbow =
            {
                PixelArt.Hex("ff7a7a"), PixelArt.Hex("ffb35c"), PixelArt.Hex("ffe066"),
                PixelArt.Hex("7fd486"), PixelArt.Hex("6fb7ff"), PixelArt.Hex("b58bd6")
            };
            for (int i = 0; i < rainbow.Length; i++)
                c.Rect((9 + i * 3) * 16, 52 * 16, 2 * 16, 6, rainbow[i]);

            // Alun-Alun Merdeka
            c.Rect(66 * 16, 48 * 16, 28 * 16, 22 * 16, PixelArt.Hex("3f8639"));
            c.Rect(67 * 16, 49 * 16, 26 * 16, 20 * 16, Grass);
            c.Ellipse(80 * 16, 59 * 16, 6 * 16, 4 * 16, PixelArt.Hex("b8b2a5"));
            Add(c, Vector2.zero, "ground_" + GroundKota);
        }

        static void BuildKampungGround()
        {
            var c = GrassCanvas(17);
            // jalan kampung, jalan tegak, dan Gang Melati menyatu tanpa garis sambungan
            DirtNetwork(c, 17, T(0, 30, MapW, 3), T(46, 0, 3, MapH), T(30, 33, 2, 28));
            // Gang Melati: lorong sempit berbatu bunga
            for (int y = 34; y < 60; y += 2)
            {
                c.Rect(29 * 16 + 8, y * 16, 4, 4, PixelArt.Hex("fff6e8"));
                c.Rect(32 * 16 + 4, y * 16 + 8, 4, 4, PixelArt.Hex("fff6e8"));
            }
            Field(c, 60, 46, 22, 16);              // lapangan kampung
            c.Rect(58 * 16, 6 * 16, 18 * 16, 14 * 16, PixelArt.Hex("8a6a44")); // kebun warga
            for (int y = 7; y < 19; y += 2)
                c.Rect(59 * 16, y * 16, 16 * 16, 6, PixelArt.Hex("6e9e4a"));
            c.FrameRect(78 * 16, 6 * 16, 14 * 16, 14 * 16, PixelArt.Hex("8a5a3c")); // peternakan
            Add(c, Vector2.zero, "ground_" + GroundKampung);
        }

        static void BuildPasarGround()
        {
            var c = new PixelCanvas(MapW * 16, MapH * 16);
            for (int y = 0; y < c.Height; y++)
                for (int x = 0; x < c.Width; x++)
                {
                    uint h = Hash(x / 3 + 9, y / 3);
                    c.Set(x, y, h % 7 == 0 ? PixelArt.Hex("888378") : PixelArt.Hex("9b958a"));
                }
            AsphaltNetwork(c, 31, false, T(0, 30, MapW, 4));
            Pavement(c, 0, 34 * 16, c.Width, 3 * 16);
            Pavement(c, 0, 27 * 16, c.Width, 3 * 16);
            for (int y = 6; y < 26; y += 2)
                c.Rect(8 * 16, y * 16, 80 * 16, 1, PixelArt.Hex("7d786f"));
            Add(c, Vector2.zero, "ground_" + GroundPasar);
        }

        static void BuildPantaiGround()
        {
            var c = new PixelCanvas(MapW * 16, MapH * 16);
            int shore = 46 * 16;
            for (int y = 0; y < c.Height; y++)
            {
                float t = Mathf.Clamp01((y - shore) / (float)(c.Height - shore));
                for (int x = 0; x < c.Width; x++)
                {
                    uint h = Hash(x / 4, y / 3);
                    Color32 col = y < shore
                        ? (h % 17 == 0 ? PixelArt.Hex("c3a86f") : PixelArt.Hex("d6bd83"))
                        : Color32.Lerp(PixelArt.Hex("58b7d5"), PixelArt.Hex("236b9c"), t);
                    if (y >= shore && h % 23 == 0) col = PixelArt.Hex("76cce0");
                    c.Set(x, y, col);
                }
            }
            c.Rect(0, shore - 3, c.Width, 3, PixelArt.Hex("fff1d6"));
            c.Rect(0, 4 * 16, c.Width, 6 * 16, GrassDark);   // jalur cemara
            AsphaltNetwork(c, 37, false, T(0, 10, MapW, 4));
            Add(c, Vector2.zero, "ground_" + GroundPantai);
        }

        // ------------------------------------------------------------------ jaringan jalan
        //
        // Semua ruas jalan digabung dalam satu "mask" dulu, lalu tepinya digambar sekali di
        // sekeliling seluruh jaringan. Hasilnya: persimpangan dan mulut gang menyatu tanpa garis
        // sambungan, dan garis tengah berhenti sebelum persimpangan.

        static bool[] RoadMask(PixelCanvas c, RectInt[] tiles)
        {
            var mask = new bool[c.Width * c.Height];
            foreach (var r in tiles)
                for (int y = Mathf.Max(0, r.y * 16); y < Mathf.Min(c.Height, r.yMax * 16); y++)
                    for (int x = Mathf.Max(0, r.x * 16); x < Mathf.Min(c.Width, r.xMax * 16); x++)
                        mask[y * c.Width + x] = true;
            return mask;
        }

        /// <summary>Di luar kanvas dianggap jalan, supaya jalan yang keluar peta tidak diberi tepi.</summary>
        static bool InRoad(bool[] mask, PixelCanvas c, int x, int y)
        {
            if (x < 0 || y < 0 || x >= c.Width || y >= c.Height) return true;
            return mask[y * c.Width + x];
        }

        static bool NearEdge(bool[] mask, PixelCanvas c, int x, int y, int d)
        {
            return !InRoad(mask, c, x - d, y) || !InRoad(mask, c, x + d, y) ||
                   !InRoad(mask, c, x, y - d) || !InRoad(mask, c, x, y + d) ||
                   !InRoad(mask, c, x - d + 1, y - d + 1) || !InRoad(mask, c, x + d - 1, y - d + 1) ||
                   !InRoad(mask, c, x - d + 1, y + d - 1) || !InRoad(mask, c, x + d - 1, y + d - 1);
        }

        /// <summary>Jalan tanah desa/kampung, ruas dalam satuan petak (16 px).</summary>
        static void DirtNetwork(PixelCanvas c, int seed, params RectInt[] tiles)
        {
            bool[] mask = RoadMask(c, tiles);
            for (int y = 0; y < c.Height; y++)
                for (int x = 0; x < c.Width; x++)
                {
                    if (!mask[y * c.Width + x]) continue;
                    Color32 col;
                    if (NearEdge(mask, c, x, y, 1)) col = DirtDark;
                    else if (NearEdge(mask, c, x, y, 2)) col = PixelArt.Hex("b88f5c");
                    else
                    {
                        uint h = Hash((x >> 1) + seed, y >> 1);
                        col = h % 11 == 0 ? DirtDark : h % 17 == 0 ? DirtLight : Dirt;
                    }
                    c.Set(x, y, col);
                }
        }

        /// <summary>Jalan aspal dengan kerb, garis tengah putus-putus, dan zebra cross di persimpangan.</summary>
        static void AsphaltNetwork(PixelCanvas c, int seed, bool crosswalks, params RectInt[] tiles)
        {
            Color32 curb = PixelArt.Hex("c5bda9");
            Color32 curbDark = PixelArt.Hex("8d887e");
            Color32 asphalt = PixelArt.Hex("454b52");
            Color32 asphaltLight = PixelArt.Hex("525960");
            Color32 line = PixelArt.Hex("f3d873");
            Color32 zebra = PixelArt.Hex("eeeae0");

            bool[] mask = RoadMask(c, tiles);
            for (int y = 0; y < c.Height; y++)
                for (int x = 0; x < c.Width; x++)
                {
                    if (!mask[y * c.Width + x]) continue;
                    Color32 col;
                    if (NearEdge(mask, c, x, y, 1)) col = curbDark;
                    else if (NearEdge(mask, c, x, y, 3)) col = curb;
                    else col = Hash((x >> 2) + seed, y >> 2) % 13 == 0 ? asphaltLight : asphalt;
                    c.Set(x, y, col);
                }

            // Kotak persimpangan (+ jarak zebra) tempat garis tengah tidak digambar.
            var junctions = new System.Collections.Generic.List<RectInt>();
            for (int i = 0; i < tiles.Length; i++)
                for (int j = i + 1; j < tiles.Length; j++)
                {
                    RectInt a = tiles[i], b = tiles[j];
                    int x0 = Mathf.Max(a.x, b.x), x1 = Mathf.Min(a.xMax, b.xMax);
                    int y0 = Mathf.Max(a.y, b.y), y1 = Mathf.Min(a.yMax, b.yMax);
                    if (x1 > x0 && y1 > y0) junctions.Add(new RectInt(x0 * 16, y0 * 16, (x1 - x0) * 16, (y1 - y0) * 16));
                }

            bool NearJunction(int px, int py, int margin)
            {
                foreach (var jr in junctions)
                    if (px >= jr.x - margin && px < jr.xMax + margin && py >= jr.y - margin && py < jr.yMax + margin)
                        return true;
                return false;
            }

            const int ZebraGap = 6, ZebraLen = 26;
            int clear = crosswalks ? ZebraGap + ZebraLen + 6 : 4;

            // Garis tengah putus-putus
            foreach (var r in tiles)
            {
                bool horizontal = r.width >= r.height;
                if (horizontal)
                {
                    int cy = r.y * 16 + r.height * 8;
                    for (int x = r.x * 16 + 10; x < r.xMax * 16 - 8; x += 18)
                        if (!NearJunction(x, cy, clear) && !NearJunction(x + 8, cy, clear))
                            c.Rect(x, cy, 8, 1, line);
                }
                else
                {
                    int cx = r.x * 16 + r.width * 8;
                    for (int y = r.y * 16 + 10; y < r.yMax * 16 - 8; y += 18)
                        if (!NearJunction(cx, y, clear) && !NearJunction(cx, y + 8, clear))
                            c.Rect(cx, y, 1, 8, line);
                }
            }

            if (!crosswalks) return;

            // Zebra cross di keempat sisi persimpangan
            foreach (var jr in junctions)
            {
                foreach (var r in tiles)
                {
                    bool horizontal = r.width >= r.height;
                    var rp = new RectInt(r.x * 16, r.y * 16, r.width * 16, r.height * 16);
                    if (horizontal && rp.y <= jr.y && rp.yMax >= jr.yMax && rp.width > jr.width)
                    {
                        foreach (int x0 in new[] { jr.x - ZebraGap - ZebraLen, jr.xMax + ZebraGap })
                            for (int y = rp.y + 5; y < rp.yMax - 6; y += 6)
                                c.Rect(x0, y, ZebraLen, 3, zebra);
                    }
                    else if (!horizontal && rp.x <= jr.x && rp.xMax >= jr.xMax && rp.height > jr.height)
                    {
                        foreach (int y0 in new[] { jr.y - ZebraGap - ZebraLen, jr.yMax + ZebraGap })
                            for (int x = rp.x + 5; x < rp.xMax - 6; x += 6)
                                c.Rect(x, y0, 3, ZebraLen, zebra);
                    }
                }
            }
        }

        static RectInt T(int x, int y, int w, int h) => new RectInt(x, y, w, h);

        static void Pavement(PixelCanvas c, int x, int y, int w, int h)
        {
            c.Rect(x, y, w, h, PixelArt.Hex("b8b2a5"));
            for (int iy = y + 3; iy < y + h; iy += 8)
                for (int ix = x + 3; ix < x + w; ix += 8)
                    c.Rect(ix, iy, 1, 1, PixelArt.Hex("8d887e"));
        }
    }
}
