using UnityEngine;

namespace PixelAdventure
{
    /// <summary>Small immediate-mode pixel canvas used to draw procedural sprites.</summary>
    public class PixelCanvas
    {
        public readonly int Width;
        public readonly int Height;
        public readonly Color32[] Pixels;

        public PixelCanvas(int width, int height)
        {
            Width = width;
            Height = height;
            Pixels = new Color32[width * height];
        }

        public void Set(int x, int y, Color32 c)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return;
            Pixels[y * Width + x] = c;
        }

        public Color32 Get(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return new Color32(0, 0, 0, 0);
            return Pixels[y * Width + x];
        }

        public void Blend(int x, int y, Color32 c)
        {
            if (c.a == 0) return;
            if (x < 0 || y < 0 || x >= Width || y >= Height) return;
            int index = y * Width + x;
            Color32 dst = Pixels[index];
            if (c.a == 255 || dst.a == 0)
            {
                Pixels[index] = c;
                return;
            }
            float sa = c.a / 255f;
            float da = dst.a / 255f;
            float outA = sa + da * (1f - sa);
            if (outA <= 0.001f)
            {
                Pixels[index] = new Color32(0, 0, 0, 0);
                return;
            }
            float r = (c.r * sa + dst.r * da * (1f - sa)) / outA;
            float g = (c.g * sa + dst.g * da * (1f - sa)) / outA;
            float b = (c.b * sa + dst.b * da * (1f - sa)) / outA;
            Pixels[index] = new Color32((byte)r, (byte)g, (byte)b, (byte)(outA * 255f));
        }

        public void Rect(int x, int y, int w, int h, Color32 c)
        {
            for (int iy = y; iy < y + h; iy++)
                for (int ix = x; ix < x + w; ix++)
                    Set(ix, iy, c);
        }

        public void FrameRect(int x, int y, int w, int h, Color32 c)
        {
            for (int ix = x; ix < x + w; ix++) { Set(ix, y, c); Set(ix, y + h - 1, c); }
            for (int iy = y; iy < y + h; iy++) { Set(x, iy, c); Set(x + w - 1, iy, c); }
        }

        public void Line(int x0, int y0, int x1, int y1, Color32 c, int thickness = 1)
        {
            int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;
            int guard = 0;
            while (guard++ < 4096)
            {
                if (thickness <= 1) Set(x0, y0, c);
                else Disc(x0, y0, thickness / 2, c);
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
            }
        }

        public void Disc(float cx, float cy, float radius, Color32 c)
        {
            int minX = Mathf.FloorToInt(cx - radius), maxX = Mathf.CeilToInt(cx + radius);
            int minY = Mathf.FloorToInt(cy - radius), maxY = Mathf.CeilToInt(cy + radius);
            float r2 = radius * radius;
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float dx = x + 0.5f - cx;
                    float dy = y + 0.5f - cy;
                    if (dx * dx + dy * dy <= r2) Set(x, y, c);
                }
            }
        }

        public void Ellipse(float cx, float cy, float rx, float ry, Color32 c)
        {
            int minX = Mathf.FloorToInt(cx - rx), maxX = Mathf.CeilToInt(cx + rx);
            int minY = Mathf.FloorToInt(cy - ry), maxY = Mathf.CeilToInt(cy + ry);
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float dx = (x + 0.5f - cx) / Mathf.Max(0.001f, rx);
                    float dy = (y + 0.5f - cy) / Mathf.Max(0.001f, ry);
                    if (dx * dx + dy * dy <= 1f) Set(x, y, c);
                }
            }
        }

        public void Triangle(Vector2 a, Vector2 b, Vector2 c, Color32 col)
        {
            int minX = Mathf.FloorToInt(Mathf.Min(a.x, Mathf.Min(b.x, c.x)));
            int maxX = Mathf.CeilToInt(Mathf.Max(a.x, Mathf.Max(b.x, c.x)));
            int minY = Mathf.FloorToInt(Mathf.Min(a.y, Mathf.Min(b.y, c.y)));
            int maxY = Mathf.CeilToInt(Mathf.Max(a.y, Mathf.Max(b.y, c.y)));

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (InsideTriangle(new Vector2(x + 0.5f, y + 0.5f), a, b, c)) Set(x, y, col);
                }
            }
        }

        static bool InsideTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = Sign(p, a, b);
            float d2 = Sign(p, b, c);
            float d3 = Sign(p, c, a);
            bool hasNeg = d1 < 0 || d2 < 0 || d3 < 0;
            bool hasPos = d1 > 0 || d2 > 0 || d3 > 0;
            return !(hasNeg && hasPos);
        }

        static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }

        public void VGradient(int x, int y, int w, int h, Color32 bottom, Color32 top)
        {
            for (int iy = 0; iy < h; iy++)
            {
                float t = h <= 1 ? 0f : iy / (float)(h - 1);
                Color32 c = Color32.Lerp(bottom, top, t);
                for (int ix = 0; ix < w; ix++) Set(x + ix, y + iy, c);
            }
        }

        public Texture2D ToTexture(FilterMode filter = FilterMode.Point, bool readable = false)
        {
            var tex = new Texture2D(Width, Height, TextureFormat.RGBA32, false)
            {
                filterMode = filter,
                wrapMode = TextureWrapMode.Clamp,
                name = "pixel_canvas"
            };
            tex.SetPixels32(Pixels);
            tex.Apply(false, !readable);
            return tex;
        }

        public Sprite ToSprite(Vector2 pivot, float ppu = PixelArt.PPU, string key = null,
            FilterMode filter = FilterMode.Point)
        {
            Texture2D tex = ToTexture(filter, true);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, Width, Height), pivot, ppu,
                0, SpriteMeshType.FullRect);
            sprite.name = key ?? "procedural_sprite";
            if (!string.IsNullOrEmpty(key)) PixelArt.Sprites[key] = sprite;
            return sprite;
        }
    }
}
