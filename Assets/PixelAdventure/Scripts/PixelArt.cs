using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure
{
    /// <summary>
    /// Tiny pixel-art factory. Every sprite in the game is generated at runtime from
    /// character maps or simple procedural drawing, so the project has zero art dependencies.
    /// </summary>
    public static partial class PixelArt
    {
        public const float PPU = 16f;

        public static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();

        static Material _material;

        public static Material SpriteMaterial
        {
            get
            {
                if (_material == null)
                {
                    Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                    if (shader == null) shader = Shader.Find("Sprites/Default");
                    _material = new Material(shader) { name = "PixelAdventureUnlit" };
                }
                return _material;
            }
        }

        /// <summary>Bright magenta fallback so missing art is obvious instead of invisible.</summary>
        static Sprite _missing;
        public static Sprite Missing
        {
            get
            {
                if (_missing == null)
                {
                    _missing = Make(new[]
                    {
                        "oooo", "oyyo", "oyyo", "oooo"
                    }, new Dictionary<char, Color32>
                    {
                        { 'o', Hex("2b0b2b") }, { 'y', Hex("ff00ff") }
                    }, new Vector2(0.5f, 0.5f));
                    _missing.name = "missing";
                }
                return _missing;
            }
        }

        public static Sprite Get(string key)
        {
            if (Sprites.TryGetValue(key, out Sprite sprite)) return sprite;
            Debug.LogWarning("[PixelAdventure] Missing sprite: " + key);
            return Missing;
        }

        public static Sprite SolidUnit { get; private set; }

        /// <summary>
        /// Runtime-created assets die when Play mode stops. This runs before every new
        /// session (even with Domain Reload disabled) so the caches never hold dead objects.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetRuntimeCache()
        {
            Sprites.Clear();
            _material = null;
            _missing = null;
            SolidUnit = null;
        }

        public static void BuildAll()
        {
            if (Sprites.Count > 0) return;
            BuildCore();
            BuildThemes();
            BuildFontSupport();
            BuildBackgrounds();

            SolidUnit = MakeSolid(Hex("ffffff"), 1f);
            SolidUnit.name = "solid_unit";
        }

        public static Color32 Hex(string hex)
        {
            if (hex.Length == 6) hex += "ff";
            byte r = (byte)System.Convert.ToInt32(hex.Substring(0, 2), 16);
            byte g = (byte)System.Convert.ToInt32(hex.Substring(2, 2), 16);
            byte b = (byte)System.Convert.ToInt32(hex.Substring(4, 2), 16);
            byte a = (byte)System.Convert.ToInt32(hex.Substring(6, 2), 16);
            return new Color32(r, g, b, a);
        }

        public static Dictionary<char, Color32> Palette(params object[] pairs)
        {
            var result = new Dictionary<char, Color32>();
            for (int i = 0; i + 1 < pairs.Length; i += 2)
            {
                result[(char)pairs[i]] = Hex((string)pairs[i + 1]);
            }
            return result;
        }

        public static Sprite Make(string[] rows, Dictionary<char, Color32> palette,
            Vector2 pivot, float ppu = PPU, FilterMode filter = FilterMode.Point, string key = null)
        {
            Texture2D tex = RowsToTexture(rows, palette, filter);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), pivot, ppu,
                0, SpriteMeshType.FullRect);
            sprite.name = key ?? "pixel_sprite";
            if (!string.IsNullOrEmpty(key)) Sprites[key] = sprite;
            return sprite;
        }

        public static Texture2D RowsToTexture(string[] rows, Dictionary<char, Color32> palette,
            FilterMode filter = FilterMode.Point)
        {
            int h = rows.Length;
            int w = 1;
            for (int i = 0; i < rows.Length; i++) w = Mathf.Max(w, rows[i].Length);

            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = filter,
                wrapMode = TextureWrapMode.Clamp,
                name = "pixel_rows"
            };

            var pixels = new Color32[w * h];
            var clear = new Color32(0, 0, 0, 0);
            for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

            for (int y = 0; y < h; y++)
            {
                string row = rows[y];
                for (int x = 0; x < row.Length; x++)
                {
                    char c = row[x];
                    if (c == '.' || c == ' ') continue;
                    if (palette.TryGetValue(c, out Color32 col))
                    {
                        pixels[(h - 1 - y) * w + x] = col;
                    }
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, false);
            return tex;
        }

        public static Sprite SpriteFromTexture(Texture2D tex, Vector2 pivot, float ppu = PPU, string key = null)
        {
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), pivot, ppu,
                0, SpriteMeshType.FullRect);
            sprite.name = key ?? "pixel_tex";
            if (!string.IsNullOrEmpty(key)) Sprites[key] = sprite;
            return sprite;
        }

        public static Sprite MakeSolid(Color32 color, float ppu)
        {
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            tex.SetPixel(0, 0, color);
            tex.Apply(false, false);
            return SpriteFromTexture(tex, new Vector2(0.5f, 0.5f), ppu);
        }

        public static Color C(string hex) => Hex(hex);

        public static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        /// <summary>Squash / stretch helper used by enemies, springs and the player.</summary>
        public static void SnapPixel(Transform t)
        {
            var p = t.localPosition;
            p.x = Mathf.Round(p.x * PPU) / PPU;
            p.y = Mathf.Round(p.y * PPU) / PPU;
            t.localPosition = p;
        }
    }
}
