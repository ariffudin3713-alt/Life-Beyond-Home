using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>Jenis ruangan dalam bangunan.</summary>
    public enum InteriorKind { Home, Shop, Cafe, Inn, Office, Library, Workshop }

    /// <summary>
    /// Gambar ruangan dalam: lantai + dinding belakang (dibuat sekali per jenis & ukuran)
    /// dan perabot (kasur, meja, kursi, rak, kasir, kulkas, sofa, meja kerja, rak buku, tanaman, karpet).
    /// Semua perabot berpivot di tengah bawah, 16 px = 1 unit.
    /// </summary>
    public static partial class LifeArt
    {
        /// <summary>Tinggi dinding belakang (unit) yang digambar di atas lantai.</summary>
        public const int RoomWallH = 3;

        static readonly Color32 FurnOutline = new Color32(0x32, 0x2b, 0x36, 255);

        static void BuildInteriorArt()
        {
            // ---- kasur
            var c = new PixelCanvas(32, 42);
            Outlined(c, 1, 0, 30, 40, PixelArt.Hex("8a5a3c"), FurnOutline);          // rangka kayu
            Outlined(c, 3, 2, 26, 34, PixelArt.Hex("f4efe0"), FurnOutline);          // kasur
            Outlined(c, 6, 28, 20, 7, PixelArt.Hex("ffffff"), PixelArt.Hex("c9c2b0")); // bantal
            Outlined(c, 3, 2, 26, 20, PixelArt.Hex("6f8fd6"), FurnOutline);          // selimut
            c.Rect(4, 18, 24, 2, PixelArt.Hex("8fa9e2"));
            Add(c, BottomCenter, "int_bed");

            // ---- meja makan
            c = new PixelCanvas(34, 26);
            c.Rect(4, 0, 3, 12, PixelArt.Hex("5e3826")); c.Rect(27, 0, 3, 12, PixelArt.Hex("5e3826"));
            Outlined(c, 0, 10, 34, 16, PixelArt.Hex("a8754a"), FurnOutline);
            c.Rect(2, 22, 30, 2, PixelArt.Hex("c8925e"));
            c.Disc(12f, 17f, 3f, PixelArt.Hex("f4efe0")); c.Disc(22f, 17f, 3f, PixelArt.Hex("f4efe0"));
            Add(c, BottomCenter, "int_table");

            // ---- kursi
            c = new PixelCanvas(12, 18);
            Outlined(c, 1, 0, 10, 9, PixelArt.Hex("8a5a3c"), FurnOutline);
            Outlined(c, 1, 8, 10, 10, PixelArt.Hex("a8754a"), FurnOutline);
            Add(c, BottomCenter, "int_chair");

            // ---- rak toko (barang warna-warni)
            c = new PixelCanvas(48, 40);
            Outlined(c, 0, 0, 48, 40, PixelArt.Hex("9aa2ab"), FurnOutline);
            Color32[] goods = { PixelArt.Hex("e46f5a"), PixelArt.Hex("ffd35c"), PixelArt.Hex("4fae62"),
                                PixelArt.Hex("6f8fd6"), PixelArt.Hex("ff9fc0"), PixelArt.Hex("f4efe0") };
            for (int row = 0; row < 3; row++)
            {
                int y = 3 + row * 12;
                c.Rect(2, y - 1, 44, 2, PixelArt.Hex("6b737c"));
                for (int x = 3, k = row; x < 44; x += 5, k++)
                    Outlined(c, x, y + 1, 4, 7 + (k % 2), goods[k % goods.Length], FurnOutline);
            }
            Add(c, BottomCenter, "int_shelf");

            // ---- rak buku
            c = new PixelCanvas(48, 42);
            Outlined(c, 0, 0, 48, 42, PixelArt.Hex("6b4126"), FurnOutline);
            Color32[] books = { PixelArt.Hex("b8503f"), PixelArt.Hex("3b6fb6"), PixelArt.Hex("2e8f62"),
                                PixelArt.Hex("e8b23a"), PixelArt.Hex("8a63ad"), PixelArt.Hex("e9e2c9") };
            for (int row = 0; row < 3; row++)
            {
                int y = 3 + row * 13;
                c.Rect(2, y - 1, 44, 2, PixelArt.Hex("4a2c18"));
                for (int x = 3, k = row * 2; x < 45; x += 3, k++)
                    c.Rect(x, y + 1, 2, 8 + (k % 3), books[k % books.Length]);
            }
            Add(c, BottomCenter, "int_bookshelf");

            // ---- meja kasir (dengan mesin kasir)
            c = new PixelCanvas(64, 30);
            Outlined(c, 0, 0, 64, 20, PixelArt.Hex("c8925e"), FurnOutline);
            c.Rect(2, 16, 60, 3, PixelArt.Hex("e0b07a"));
            c.Rect(2, 2, 60, 2, PixelArt.Hex("a8754a"));
            Outlined(c, 40, 19, 16, 10, PixelArt.Hex("3a3f52"), FurnOutline);       // mesin kasir
            c.Rect(43, 24, 10, 3, PixelArt.Hex("7fd486"));
            Outlined(c, 8, 19, 10, 6, PixelArt.Hex("ffd35c"), FurnOutline);         // keranjang
            Add(c, BottomCenter, "int_counter");

            // ---- kulkas minuman
            c = new PixelCanvas(24, 42);
            Outlined(c, 0, 0, 24, 42, PixelArt.Hex("dcdde2"), FurnOutline);
            Outlined(c, 3, 4, 18, 32, PixelArt.Hex("bce5e8"), FurnOutline);
            for (int row = 0; row < 3; row++)
                for (int x = 5; x < 19; x += 4)
                    c.Rect(x, 6 + row * 10, 2, 7, row == 0 ? PixelArt.Hex("e46f5a") : row == 1 ? PixelArt.Hex("4fae62") : PixelArt.Hex("ffd35c"));
            c.Rect(0, 38, 24, 4, PixelArt.Hex("e46f5a"));
            Add(c, BottomCenter, "int_fridge");

            // ---- tanaman pot
            c = new PixelCanvas(16, 24);
            Outlined(c, 4, 0, 8, 7, PixelArt.Hex("c8794a"), FurnOutline);
            c.Disc(8f, 13f, 6f, PixelArt.Hex("3a8a4c"));
            c.Disc(6f, 15f, 3.5f, PixelArt.Hex("4fae62"));
            c.Disc(10f, 18f, 3.5f, PixelArt.Hex("6cbd5c"));
            Add(c, BottomCenter, "int_plant");

            // ---- karpet (rata di lantai)
            c = new PixelCanvas(56, 32);
            Outlined(c, 0, 0, 56, 32, PixelArt.Hex("b8503f"), PixelArt.Hex("7a2e24"));
            c.FrameRect(4, 4, 48, 24, PixelArt.Hex("ffd35c"));
            c.FrameRect(10, 9, 36, 14, PixelArt.Hex("e9e2c9"));
            Add(c, BottomCenter, "int_rug");

            // ---- sofa
            c = new PixelCanvas(44, 26);
            Outlined(c, 0, 0, 44, 22, PixelArt.Hex("4c6bb0"), FurnOutline);
            Outlined(c, 0, 12, 44, 14, PixelArt.Hex("5a7cc4"), FurnOutline);
            c.Rect(4, 4, 17, 8, PixelArt.Hex("6f8fd6")); c.Rect(23, 4, 17, 8, PixelArt.Hex("6f8fd6"));
            Add(c, BottomCenter, "int_sofa");

            // ---- meja kerja + komputer
            c = new PixelCanvas(40, 30);
            c.Rect(3, 0, 3, 12, PixelArt.Hex("4a4f5c")); c.Rect(34, 0, 3, 12, PixelArt.Hex("4a4f5c"));
            Outlined(c, 0, 10, 40, 10, PixelArt.Hex("e0d6c0"), FurnOutline);
            Outlined(c, 12, 19, 16, 11, PixelArt.Hex("2a2d36"), FurnOutline);       // monitor
            c.Rect(14, 21, 12, 7, PixelArt.Hex("6fb7ff"));
            c.Rect(30, 19, 6, 3, PixelArt.Hex("f4efe0"));                            // kertas
            Add(c, BottomCenter, "int_desk");

            // ---- keset pintu (rata)
            c = new PixelCanvas(32, 10);
            Outlined(c, 0, 0, 32, 10, PixelArt.Hex("8a6a44"), PixelArt.Hex("5a4028"));
            for (int x = 3; x < 30; x += 3) c.Rect(x, 2, 1, 6, PixelArt.Hex("a8865a"));
            Add(c, BottomCenter, "int_doormat");

            // ---- jendela dinding
            c = new PixelCanvas(26, 20);
            Outlined(c, 0, 0, 26, 20, PixelArt.Hex("bce5e8"), PixelArt.Hex("6b4126"));
            c.Rect(12, 1, 2, 18, PixelArt.Hex("6b4126"));
            c.Rect(1, 9, 24, 2, PixelArt.Hex("6b4126"));
            c.Rect(3, 13, 6, 3, PixelArt.Hex("e8fbff"));
            Add(c, BottomCenter, "int_window");

            // ---- lukisan dinding
            c = new PixelCanvas(20, 16);
            Outlined(c, 0, 0, 20, 16, PixelArt.Hex("6fb7ff"), PixelArt.Hex("c8925e"));
            c.Rect(1, 1, 18, 6, PixelArt.Hex("4fae62"));
            c.Disc(14f, 11f, 2.5f, PixelArt.Hex("ffd35c"));
            Add(c, BottomCenter, "int_painting");
        }

        static readonly Vector2 BottomCenter = new Vector2(0.5f, 0f);

        /// <summary>Lantai + dinding ruangan (lebar w, tinggi lantai h, ditambah dinding belakang). Pivot kiri bawah.</summary>
        public static Sprite RoomSprite(InteriorKind kind, int w, int h)
        {
            string key = Prefix + "room_" + kind + "_" + w + "x" + h;
            if (PixelArt.Sprites.TryGetValue(key, out Sprite cached) && cached != null) return cached;

            int pw = w * 16, ph = (h + RoomWallH) * 16, floorH = h * 16;
            var c = new PixelCanvas(pw, ph);
            Color32 a, b, wall, wallDark;
            switch (kind)
            {
                case InteriorKind.Home:     a = PixelArt.Hex("c8925e"); b = PixelArt.Hex("b8824e"); wall = PixelArt.Hex("f0dcb8"); wallDark = PixelArt.Hex("c9a87a"); break;
                case InteriorKind.Cafe:     a = PixelArt.Hex("8a5a3c"); b = PixelArt.Hex("7a4e32"); wall = PixelArt.Hex("e9d2b0"); wallDark = PixelArt.Hex("6b4126"); break;
                case InteriorKind.Inn:      a = PixelArt.Hex("a8463c"); b = PixelArt.Hex("963c34"); wall = PixelArt.Hex("e9e2c9"); wallDark = PixelArt.Hex("4f7a63"); break;
                case InteriorKind.Library:  a = PixelArt.Hex("a8754a"); b = PixelArt.Hex("98683f"); wall = PixelArt.Hex("e6dcc4"); wallDark = PixelArt.Hex("6b4126"); break;
                case InteriorKind.Workshop: a = PixelArt.Hex("8f8b84"); b = PixelArt.Hex("85817a"); wall = PixelArt.Hex("c9c6be"); wallDark = PixelArt.Hex("5b5f66"); break;
                case InteriorKind.Office:   a = PixelArt.Hex("cfd3d8"); b = PixelArt.Hex("bfc4ca"); wall = PixelArt.Hex("eef0f2"); wallDark = PixelArt.Hex("6b737c"); break;
                default:                    a = PixelArt.Hex("eef0ee"); b = PixelArt.Hex("dfe3e0"); wall = PixelArt.Hex("f6f3ea"); wallDark = PixelArt.Hex("6fae9a"); break;   // toko
            }

            // Lantai.
            for (int y = 0; y < floorH; y++)
                for (int x = 0; x < pw; x++)
                {
                    Color32 col;
                    switch (kind)
                    {
                        case InteriorKind.Home: case InteriorKind.Cafe: case InteriorKind.Library:
                            // papan kayu: baris 8 px, sambungan berselang
                            int plank = y / 8;
                            int seam = (x + plank * 23) % 48;
                            col = plank % 2 == 0 ? a : b;
                            if (y % 8 == 0 || seam == 0) col = Color32.Lerp(col, FurnOutline, 0.35f);
                            break;
                        case InteriorKind.Inn:
                            col = ((x / 4 + y / 4) % 6 == 0) ? b : a;     // karpet bermotif
                            break;
                        case InteriorKind.Workshop:
                            col = Hash(x / 3, y / 3) % 9 == 0 ? b : a;    // beton
                            break;
                        default:
                            // ubin 16x16 dengan nat
                            bool grout = x % 16 == 0 || y % 16 == 0;
                            col = grout ? Color32.Lerp(a, FurnOutline, 0.25f) : (((x / 16 + y / 16) % 2 == 0) ? a : b);
                            break;
                    }
                    c.Set(x, y, col);
                }

            // Dinding belakang.
            c.Rect(0, floorH, pw, RoomWallH * 16, wall);
            c.Rect(0, floorH, pw, 4, wallDark);                       // plint
            c.Rect(0, ph - 3, pw, 3, Color32.Lerp(wall, FurnOutline, 0.4f));
            for (int x = 0; x < pw; x += 2)                           // bayangan dinding di lantai
                c.Blend(x, floorH - 1, new Color32(0, 0, 0, 70));

            // Dinding samping & bawah (tepi gelap), pintu di tengah bawah.
            Color32 edge = new Color32(0x1c, 0x17, 0x22, 255);
            c.Rect(0, 0, 6, ph, edge);
            c.Rect(pw - 6, 0, 6, ph, edge);
            int door0 = pw / 2 - 20, door1 = pw / 2 + 20;
            c.Rect(0, 0, door0, 5, edge);
            c.Rect(door1, 0, pw - door1, 5, edge);
            c.Rect(door0, 0, door1 - door0, 3, PixelArt.Hex("fff1c0"));  // cahaya dari pintu

            return c.ToSprite(Vector2.zero, PixelArt.PPU, key);
        }
    }
}
