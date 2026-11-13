using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Procedural art for the life-sim town. Everything is generated at runtime with
    /// PixelCanvas so the project still ships with zero external assets.
    /// </summary>
    public static partial class LifeArt
    {
        public const string Prefix = "lc_";

        static readonly Color32 Outline = PixelArt.Hex("241a2b");
        static readonly Color32 Cream = PixelArt.Hex("fff1d6");
        static readonly Color32 Gold = PixelArt.Hex("ffd35c");

        public static void BuildAll()
        {
            if (PixelArt.Sprites.ContainsKey(Prefix + "ground_" + GroundDesa)) return;

            BuildGround();
            BuildBuildings();
            BuildProps();
            BuildPlots();
            BuildShadow();
            BuildPanels();
            BuildCharacters();
            BuildTrafficArt();
            BuildTabletArt();
            BuildInteriorArt();
            BuildInteriorArt2();
            BuildDecorArt();
            BuildHomeArt();
            BuildIndoArt();
            BuildIndoArt2();
            BuildWorshipArt();
            BuildQuestItemArt();
            BuildWorkArt();
            BuildFacadeArt();
            BuildVehicleArt();
            BuildTownArt2();

        }

        static Sprite Add(PixelCanvas c, Vector2 pivot, string key)
        {
            return c.ToSprite(pivot, PixelArt.PPU, Prefix + key);
        }

        static uint Hash(int x, int y)
        {
            uint h = (uint)(x * 374761393 + y * 668265263);
            h = (h ^ (h >> 13)) * 1274126177u;
            return h ^ (h >> 16);
        }

        static void Outlined(PixelCanvas c, int x, int y, int w, int h, Color32 fill, Color32 edge)
        {
            c.Rect(x, y, w, h, edge);
            if (w > 2 && h > 2) c.Rect(x + 1, y + 1, w - 2, h - 2, fill);
        }

        // ------------------------------------------------------------------ panels / ui

        static void BuildPanels()
        {
            MakePanel("panel", PixelArt.Hex("2a1e33"), PixelArt.Hex("151020"), PixelArt.Hex("6b5478"));
            MakePanel("panel_gold", PixelArt.Hex("2f2230"), PixelArt.Hex("150f17"), PixelArt.Hex("d8a84e"));
            MakePanel("panel_soft", PixelArt.Hex("3a2b3e"), PixelArt.Hex("1b1420"), PixelArt.Hex("8a6e94"));

            var bar = new PixelCanvas(8, 8);
            bar.Rect(0, 0, 8, 8, PixelArt.Hex("120d18"));
            bar.Rect(1, 1, 6, 6, PixelArt.Hex("35283c"));
            Add(bar, new Vector2(0.5f, 0.5f), "bar_bg");

            var glow = new PixelCanvas(24, 24);
            for (int y = 0; y < 24; y++)
                for (int x = 0; x < 24; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(12f, 12f));
                    float a = Mathf.Clamp01(1f - d / 12f);
                    glow.Set(x, y, new Color32(255, 214, 110, (byte)(a * a * 150f)));
                }
            Add(glow, new Vector2(0.5f, 0.5f), "glow");
        }

        static void MakePanel(string key, Color32 fill, Color32 edge, Color32 rim)
        {
            var c = new PixelCanvas(32, 32);
            c.Rect(0, 0, 32, 32, new Color32(0, 0, 0, 0));
            c.Rect(1, 1, 30, 30, edge);
            c.Rect(3, 3, 26, 26, fill);
            c.FrameRect(3, 3, 26, 26, rim);
            c.FrameRect(4, 4, 24, 24, new Color32(0, 0, 0, 0));
            // clipped corners for a softer pixel silhouette
            for (int i = 0; i < 3; i++)
            {
                c.Set(0 + i, 0, new Color32(0, 0, 0, 0));
                c.Set(0, 0 + i, new Color32(0, 0, 0, 0));
                c.Set(31 - i, 0, new Color32(0, 0, 0, 0));
                c.Set(31, 0 + i, new Color32(0, 0, 0, 0));
                c.Set(0 + i, 31, new Color32(0, 0, 0, 0));
                c.Set(0, 31 - i, new Color32(0, 0, 0, 0));
                c.Set(31 - i, 31, new Color32(0, 0, 0, 0));
                c.Set(31, 31 - i, new Color32(0, 0, 0, 0));
            }

            var tex = c.ToTexture(FilterMode.Point, true);
            var sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f),
                PixelArt.PPU, 0, SpriteMeshType.FullRect, new Vector4(10, 10, 10, 10));
            sprite.name = Prefix + key;
            PixelArt.Sprites[Prefix + key] = sprite;
        }

        static void BuildShadow()
        {
            var c = new PixelCanvas(24, 8);
            c.Ellipse(12f, 4f, 11f, 3.4f, new Color32(18, 12, 24, 110));
            Add(c, new Vector2(0.5f, 0.5f), "shadow");
        }
    }
}
