using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>Gaya papan nama.</summary>
    public enum PlateStyle
    {
        Toko,    // papan kayu di dinding toko/warung
        Umum,    // papan biru gelap untuk bangunan umum
        Jalan,   // papan hijau nama jalan, bertiang
        Tempat,  // papan kayu kecil bertiang untuk taman, sawah, pantai
        Halte,   // plat biru di atap halte
    }

    public static partial class LifeArt
    {
        public const int PlatePostPx = 11;

        public static Color32 PlateText(PlateStyle s) =>
            s == PlateStyle.Toko || s == PlateStyle.Tempat ? PixelArt.Hex("fff1d6") : PixelArt.Hex("ffffff");

        public static Color32 PlateTextOutline(PlateStyle s)
        {
            switch (s)
            {
                case PlateStyle.Toko:
                case PlateStyle.Tempat: return PixelArt.Hex("4a2e1e");
                case PlateStyle.Jalan: return PixelArt.Hex("0f4a2a");
                default: return PixelArt.Hex("14334a");
            }
        }

        /// <summary>
        /// Papan berukuran pas dengan teks. Papan bertiang (Jalan, Tempat) memakai pivot di kaki tiang;
        /// papan dinding memakai pivot tengah.
        /// </summary>
        public static Sprite Plate(int boardW, int boardH, PlateStyle style)
        {
            bool post = style == PlateStyle.Jalan || style == PlateStyle.Tempat;
            string key = Prefix + "plate_" + style + "_" + boardW + "x" + boardH;
            if (PixelArt.Sprites.TryGetValue(key, out Sprite cached)) return cached;

            int postH = post ? PlatePostPx : 0;
            var c = new PixelCanvas(boardW, boardH + postH);
            var clear = new Color32(0, 0, 0, 0);
            c.Rect(0, 0, c.Width, c.Height, clear);

            Color32 edge, fill, light, dark;
            switch (style)
            {
                case PlateStyle.Toko:
                case PlateStyle.Tempat:
                    edge = PixelArt.Hex("3a2418"); fill = PixelArt.Hex("8a5a3c");
                    light = PixelArt.Hex("a9744c"); dark = PixelArt.Hex("6e4529");
                    break;
                case PlateStyle.Jalan:
                    edge = PixelArt.Hex("0c3320"); fill = PixelArt.Hex("1f7a4a");
                    light = PixelArt.Hex("f4f7f2"); dark = PixelArt.Hex("176039");
                    break;
                case PlateStyle.Halte:
                    edge = PixelArt.Hex("10303f"); fill = PixelArt.Hex("266a88");
                    light = PixelArt.Hex("478dac"); dark = PixelArt.Hex("1d5570");
                    break;
                default:
                    edge = PixelArt.Hex("10283a"); fill = PixelArt.Hex("2f4f6b");
                    light = PixelArt.Hex("4d7392"); dark = PixelArt.Hex("243f57");
                    break;
            }

            // tiang
            if (post)
            {
                Color32 pole = style == PlateStyle.Jalan ? PixelArt.Hex("8d939a") : PixelArt.Hex("6e4529");
                c.Ellipse(boardW / 2f, 1.5f, 5f, 1.5f, new Color32(18, 12, 24, 70));
                c.Rect(boardW / 2 - 1, 0, 3, postH + 1, edge);
                c.Rect(boardW / 2, 0, 1, postH + 1, pole);
            }

            int y0 = postH;
            // papan dengan sudut terpotong
            c.Rect(1, y0, boardW - 2, boardH, edge);
            c.Rect(0, y0 + 1, boardW, boardH - 2, edge);
            c.Rect(1, y0 + 1, boardW - 2, boardH - 2, fill);

            if (style == PlateStyle.Jalan)
            {
                // garis putih tipis khas papan nama jalan
                c.FrameRect(2, y0 + 2, boardW - 4, boardH - 4, light);
            }
            else if (style == PlateStyle.Toko || style == PlateStyle.Tempat)
            {
                // serat kayu + paku
                for (int y = y0 + 3; y < y0 + boardH - 2; y += 3)
                    for (int x = 3; x < boardW - 3; x += 7)
                        c.Rect(x + (y % 2), y, 3, 1, dark);
                c.Rect(1, y0 + boardH - 2, boardW - 2, 1, light);
                c.Set(2, y0 + 2, light);
                c.Set(boardW - 3, y0 + 2, light);
            }
            else
            {
                c.Rect(1, y0 + boardH - 2, boardW - 2, 1, light);
                c.Rect(1, y0 + 1, boardW - 2, 1, dark);
            }

            var pivot = post ? new Vector2(0.5f, 0f) : new Vector2(0.5f, 0.5f);
            var sprite = c.ToSprite(pivot, PixelArt.PPU, key);
            PixelArt.Sprites[key] = sprite;
            return sprite;
        }
    }
}
