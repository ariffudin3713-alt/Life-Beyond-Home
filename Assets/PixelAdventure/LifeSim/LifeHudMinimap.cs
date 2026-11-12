using System.Collections.Generic;
using PixelAdventure.LifeSim.Pulang;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Minimap kecil di kiri bawah yang jelas:
    /// - warna tanah per wilayah, jalan sesuai bentuk asli peta, laut di Pantai Cemara;
    /// - kotak putih = area yang sedang terlihat di layar;
    /// - pemain = panah besar bergaris tepi gelap;
    /// - setiap penanda punya garis tepi; tujuan misi lebih besar dan berdenyut;
    /// - legenda warna di bawah peta.
    /// </summary>
    public partial class LifeHud
    {
        const int MiniMax = 32;
        const int MiniRoadMax = 6;

        static readonly Color MiniOutline = new Color(0.06f, 0.05f, 0.10f, 1f);
        static readonly Color LegendGoal = new Color(1f, 0.84f, 0.3f, 1f);
        static readonly Color LegendGirl = new Color(1f, 0.55f, 0.75f, 1f);
        static readonly Color LegendPlace = new Color(0.92f, 0.92f, 0.98f, 1f);

        Transform _minimapGroup;
        SpriteRenderer _miniPanel, _miniTitle, _miniNorth, _miniGround, _miniSea, _miniFrame;
        SpriteRenderer _miniPlayer, _miniPlayerOutline;
        readonly SpriteRenderer[] _miniRoads = new SpriteRenderer[MiniRoadMax];
        readonly SpriteRenderer[] _miniView = new SpriteRenderer[4];
        readonly SpriteRenderer[] _miniDots = new SpriteRenderer[MiniMax];
        readonly SpriteRenderer[] _miniDotOutlines = new SpriteRenderer[MiniMax];
        readonly Vector2[] _miniDotPositions = new Vector2[MiniMax];
        readonly Color[] _miniDotColors = new Color[MiniMax];
        readonly bool[] _miniDotStars = new bool[MiniMax];
        readonly bool[] _miniDotImportant = new bool[MiniMax];
        SpriteRenderer[] _legendSwatches, _legendLabels;
        int _miniDotCount;
        Vector2 _miniPlayerPos;
        int _miniPlayerDir;
        string _lastMiniTitle;
        Region _miniRegion = (Region)(-1);

        /// <summary>Batas minimap (koordinat HUD) untuk penunjuk misi agar tidak menimpanya.</summary>
        float _miniEdgeRight = -999f, _miniEdgeTop = -999f;

        struct MiniStyle
        {
            public Color Ground, Road;
            public Rect[] Roads;      // dalam unit dunia
            public float SeaFromY;    // < 0 = tanpa laut
        }

        // Jalan mengikuti LifeArtTown (jaringan jalan tiap wilayah).
        static readonly Dictionary<Region, MiniStyle> MiniStyles = new Dictionary<Region, MiniStyle>
        {
            { Region.DesaSukaMakmur, new MiniStyle {
                Ground = new Color(0.36f, 0.62f, 0.32f), Road = new Color(0.78f, 0.64f, 0.42f), SeaFromY = -1f,
                Roads = new[] { new Rect(0, 30, 96, 3), new Rect(50, 0, 3, 72) } } },
            { Region.KotaJogokerto, new MiniStyle {
                Ground = new Color(0.62f, 0.60f, 0.56f), Road = new Color(0.25f, 0.26f, 0.30f), SeaFromY = -1f,
                Roads = new[] { new Rect(0, 30, 96, 8), new Rect(44, 0, 8, 72) } } },
            { Region.KampungCempaka, new MiniStyle {
                Ground = new Color(0.40f, 0.64f, 0.34f), Road = new Color(0.78f, 0.64f, 0.42f), SeaFromY = -1f,
                Roads = new[] { new Rect(0, 30, 96, 3), new Rect(46, 0, 3, 72), new Rect(30, 33, 2, 28) } } },
            { Region.PasarRamaiJaya, new MiniStyle {
                Ground = new Color(0.58f, 0.56f, 0.52f), Road = new Color(0.25f, 0.26f, 0.30f), SeaFromY = -1f,
                Roads = new[] { new Rect(0, 30, 96, 4) } } },
            { Region.PantaiCemara, new MiniStyle {
                Ground = new Color(0.86f, 0.76f, 0.52f), Road = new Color(0.25f, 0.26f, 0.30f), SeaFromY = 46f,
                Roads = new[] { new Rect(0, 10, 96, 4) } } },
        };

        void BuildMinimap()
        {
            _minimapGroup = Group("Minimap");
            _miniPanel = Panel(_minimapGroup, "MiniPanel", "lc_panel_soft", 3990);
            _miniPanel.color = new Color(1f, 1f, 1f, 0.96f);
            _miniTitle = Text(_minimapGroup, "MiniTitle", " ", 1, Ink, new Vector2(0f, 0.5f), 4000);
            _miniNorth = Text(_minimapGroup, "MiniNorth", "▲ U", 1, Gold, new Vector2(1f, 0.5f), 4000);

            _miniFrame = Solid(_minimapGroup, "MiniFrame", MiniOutline, 3991);
            _miniGround = Solid(_minimapGroup, "MiniGround", Color.gray, 3992);
            _miniSea = Solid(_minimapGroup, "MiniSea", new Color(0.30f, 0.62f, 0.85f), 3993);
            for (int i = 0; i < MiniRoadMax; i++)
                _miniRoads[i] = Solid(_minimapGroup, "MiniRoad" + i, Color.gray, 3994);
            for (int i = 0; i < 4; i++)
                _miniView[i] = Solid(_minimapGroup, "MiniView" + i, new Color(1f, 1f, 1f, 0.55f), 3995);

            for (int i = 0; i < MiniMax; i++)
            {
                _miniDotOutlines[i] = Solid(_minimapGroup, "MiniDotOutline" + i, MiniOutline, 3996);
                _miniDots[i] = Solid(_minimapGroup, "MiniDot" + i, Color.white, 3997);
                _miniDots[i].gameObject.SetActive(false);
                _miniDotOutlines[i].gameObject.SetActive(false);
            }

            _miniPlayerOutline = Make(_minimapGroup, PixelArt.Get("lc_icon_direction"), "MiniPlayerOutline", 3998);
            _miniPlayerOutline.color = MiniOutline;
            _miniPlayer = Make(_minimapGroup, PixelArt.Get("lc_icon_direction"), "MiniPlayer", 3999);

            // Legenda: dua baris.
            string[] names = { "MISI", "KERJA", "TEMAN", "TEMPAT" };
            Color[] colors = { LegendGoal, Color.white, LegendGirl, LegendPlace };
            _legendSwatches = new SpriteRenderer[names.Length];
            _legendLabels = new SpriteRenderer[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                bool star = i == 1;
                _legendSwatches[i] = star
                    ? Make(_minimapGroup, PixelArt.Get("lc_icon_star"), "LegendStar", 3999)
                    : Solid(_minimapGroup, "LegendSwatch" + i, colors[i], 3999);
                _legendSwatches[i].transform.localScale = Vector3.one * (star ? 0.45f : 0.24f);
                _legendLabels[i] = Text(_minimapGroup, "Legend" + i, names[i], 1, Soft, new Vector2(0f, 0.5f), 4000);
            }

            _minimapGroup.gameObject.SetActive(false);
        }

        // ------------------------------------------------------------ api

        public void SetMinimapVisible(bool visible)
        {
            if (_minimapGroup != null) _minimapGroup.gameObject.SetActive(visible);
        }

        public void RefreshMinimap(Region region, Vector2 playerPosition, int direction,
            List<MapMarker> markers, string title)
        {
            _miniPlayerPos = playerPosition;
            _miniPlayerDir = direction;
            if (region != _miniRegion) ApplyMiniRegion(region);
            if (!string.IsNullOrEmpty(title) && title != _lastMiniTitle)
            {
                _lastMiniTitle = title;
                SetText(_miniTitle, title, Ink, 1, new Vector2(0f, 0.5f));
            }
            _miniDotCount = 0;
            if (markers == null) return;
            for (int i = 0; i < markers.Count && _miniDotCount < MiniMax; i++)
            {
                _miniDotPositions[_miniDotCount] = markers[i].Position;
                _miniDotColors[_miniDotCount] = markers[i].Color;
                _miniDotStars[_miniDotCount] = markers[i].Star;
                _miniDotImportant[_miniDotCount] = markers[i].Important;
                _miniDotCount++;
            }
        }

        void ApplyMiniRegion(Region r)
        {
            _miniRegion = r;
            if (!MiniStyles.TryGetValue(r, out MiniStyle s)) return;
            _miniGround.color = s.Ground;
            for (int i = 0; i < MiniRoadMax; i++)
            {
                bool show = i < s.Roads.Length;
                _miniRoads[i].gameObject.SetActive(show);
                if (show) _miniRoads[i].color = s.Road;
            }
            _miniSea.gameObject.SetActive(s.SeaFromY >= 0f);
        }

        // ------------------------------------------------------------ layout

        void LayoutMinimap(float halfW, float halfH)
        {
            if (_minimapGroup == null || !_minimapGroup.gameObject.activeSelf) return;

            // Kecil, di pojok kiri bawah layar.
            float mapW = Mathf.Min(4.8f, halfW * 0.4f);
            float mapH = mapW * (LifeWorld.Height / LifeWorld.Width);
            float w = mapW + 0.6f;
            float titleH = 0.6f, legendH = 1.0f;
            float h = titleH + mapH + legendH + 0.3f;
            float left = -halfW + 0.28f;
            float bottom = -halfH + 0.28f;
            float top = bottom + h;
            float cx = left + w * 0.5f;
            float cy = bottom + h * 0.5f;
            _miniEdgeRight = left + w;
            _miniEdgeTop = top;

            SetRect(_miniPanel, cx, cy, w, h);
            _miniTitle.transform.localPosition = new Vector3(cx - w * 0.5f + 0.32f, top - 0.33f, 0f);
            _miniNorth.transform.localPosition = new Vector3(cx + w * 0.5f - 0.32f, top - 0.33f, 0f);

            float mapL = cx - mapW * 0.5f;
            float mapT = top - titleH;
            float mapB = mapT - mapH;
            SetSolid(_miniFrame, cx, mapB + mapH * 0.5f, mapW + 0.12f, mapH + 0.12f);
            SetSolid(_miniGround, cx, mapB + mapH * 0.5f, mapW, mapH);

            // Jalan & laut.
            if (MiniStyles.TryGetValue(_miniRegion, out MiniStyle s))
            {
                for (int i = 0; i < s.Roads.Length && i < MiniRoadMax; i++)
                {
                    Rect r = s.Roads[i];
                    Vector2 a = MapToMini(new Vector2(r.xMin, r.yMin), mapL, mapB, mapW, mapH);
                    Vector2 b = MapToMini(new Vector2(r.xMax, r.yMax), mapL, mapB, mapW, mapH);
                    float rw = Mathf.Max(0.1f, b.x - a.x), rh = Mathf.Max(0.1f, b.y - a.y);
                    SetSolid(_miniRoads[i], (a.x + b.x) * 0.5f, (a.y + b.y) * 0.5f, rw, rh);
                }
                if (s.SeaFromY >= 0f)
                {
                    float seaB = mapB + mapH * (s.SeaFromY / LifeWorld.Height);
                    SetSolid(_miniSea, cx, (seaB + mapT) * 0.5f, mapW, mapT - seaB);
                }
            }

            // Kotak area layar (yang sedang terlihat kamera).
            Rect view = LifeView.Rect;
            Vector2 v0 = MapToMini(view.min, mapL, mapB, mapW, mapH);
            Vector2 v1 = MapToMini(view.max, mapL, mapB, mapW, mapH);
            const float line = 0.05f;
            SetSolid(_miniView[0], (v0.x + v1.x) * 0.5f, v1.y, v1.x - v0.x, line);
            SetSolid(_miniView[1], (v0.x + v1.x) * 0.5f, v0.y, v1.x - v0.x, line);
            SetSolid(_miniView[2], v0.x, (v0.y + v1.y) * 0.5f, line, v1.y - v0.y);
            SetSolid(_miniView[3], v1.x, (v0.y + v1.y) * 0.5f, line, v1.y - v0.y);

            // Penanda.
            float pulse = 1f + 0.25f * (Mathf.Sin(_time * 5f) * 0.5f + 0.5f);
            Sprite starSprite = PixelArt.Get("lc_icon_star");
            for (int i = 0; i < MiniMax; i++)
            {
                bool show = i < _miniDotCount;
                if (_miniDots[i].gameObject.activeSelf != show)
                {
                    _miniDots[i].gameObject.SetActive(show);
                    _miniDotOutlines[i].gameObject.SetActive(show);
                }
                if (!show) continue;
                Vector2 p = MapToMini(_miniDotPositions[i], mapL, mapB, mapW, mapH);
                bool star = _miniDotStars[i];
                bool important = _miniDotImportant[i];
                var dot = _miniDots[i];
                var outline = _miniDotOutlines[i];

                if (star)
                {
                    if (dot.sprite != starSprite) dot.sprite = starSprite;
                    dot.color = Color.white;
                    dot.transform.localScale = Vector3.one * 0.5f;
                    dot.sortingOrder = 3998;
                    outline.gameObject.SetActive(false);
                }
                else
                {
                    if (dot.sprite != PixelArt.SolidUnit) dot.sprite = PixelArt.SolidUnit;
                    float size = important ? 0.26f * pulse : 0.17f;
                    dot.color = _miniDotColors[i];
                    dot.transform.localScale = Vector3.one * size;
                    dot.sortingOrder = important ? 3998 : 3997;
                    outline.gameObject.SetActive(true);
                    outline.transform.localScale = Vector3.one * (size + 0.08f);
                    outline.transform.localPosition = new Vector3(p.x, p.y, 0f);
                    outline.sortingOrder = dot.sortingOrder - 1;
                }
                dot.transform.localPosition = new Vector3(p.x, p.y, 0f);
            }

            // Pemain: panah besar menunjuk arah hadap, dengan garis tepi gelap.
            Vector2 pl = MapToMini(_miniPlayerPos, mapL, mapB, mapW, mapH);
            float angle = _miniPlayerDir == 1 ? 0f : _miniPlayerDir == 2 ? 90f : _miniPlayerDir == 3 ? -90f : 180f;
            _miniPlayer.transform.localPosition = new Vector3(pl.x, pl.y, 0f);
            _miniPlayer.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            _miniPlayer.transform.localScale = Vector3.one * 0.42f;
            _miniPlayerOutline.transform.localPosition = new Vector3(pl.x, pl.y, 0f);
            _miniPlayerOutline.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            _miniPlayerOutline.transform.localScale = Vector3.one * 0.55f;

            // Legenda (2 baris x 2 kolom).
            float legTop = mapB - 0.33f;
            float colW = mapW * 0.5f;
            for (int i = 0; i < _legendLabels.Length; i++)
            {
                int col = i % 2, row = i / 2;
                float x = mapL + col * colW + 0.08f;
                float y = legTop - row * 0.45f;
                _legendSwatches[i].transform.localPosition = new Vector3(x, y, 0f);
                _legendLabels[i].transform.localPosition = new Vector3(x + 0.3f, y, 0f);
            }
        }

        static Vector2 MapToMini(Vector2 world, float left, float bottom, float width, float height)
        {
            float x = left + Mathf.Clamp01(world.x / LifeWorld.Width) * width;
            float y = bottom + Mathf.Clamp01(world.y / LifeWorld.Height) * height;
            return new Vector2(x, y);
        }
    }
}
