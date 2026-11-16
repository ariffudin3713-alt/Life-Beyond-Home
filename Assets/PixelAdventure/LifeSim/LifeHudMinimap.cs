using System.Collections.Generic;
using PixelAdventure.LifeSim.Pulang;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Minimap kecil di kiri bawah, tersusun dari atas ke bawah:
    /// 1) judul wilayah (ketuk minimap / tekan F = buka peta besar);
    /// 2) peta: warna tanah per wilayah, jalan sesuai bentuk asli, laut di Pantai Cemara,
    ///    kotak putih = area layar, pemain = panah bergaris tepi, tujuan misi berdenyut,
    ///    panah di tepi kiri/kanan = jalan keluar ke wilayah sebelah;
    /// 3) baris "◀ WILAYAH KIRI ... WILAYAH KANAN ▶";
    /// 4) garis pemisah, baris tujuan misi + jarak, dan petunjuk "F: BUKA PETA" (legenda warna ada di layar PETA).
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

        // Jalan keluar ke wilayah sebelah: panah di tepi peta + nama wilayah di bawah peta.
        static readonly Color MiniExitOpen = new Color(1f, 0.84f, 0.40f, 1f);
        static readonly Color MiniExitLocked = new Color(0.55f, 0.52f, 0.60f, 1f);
        const float MiniExitRowH = 0.58f;
        readonly SpriteRenderer[] _miniExitArrow = new SpriteRenderer[2];
        readonly SpriteRenderer[] _miniExitArrowOutline = new SpriteRenderer[2];
        readonly SpriteRenderer[] _miniExitText = new SpriteRenderer[2];
        readonly string[] _miniExitName = new string[2];
        readonly bool[] _miniExitOpen = new bool[2];
        readonly string[] _miniExitBuilt = new string[2];
        SpriteRenderer _miniDivider;
        Rect _miniRect;

        // Penunjuk arah misi di minimap: garis putus-putus pemain -> tujuan terdekat,
        // panah kecil di samping pemain, dan baris "tujuan + jarak" di bawah legenda.
        const int MiniGuideDots = 28;
        const float MiniGuideRowH = 0.58f;
        readonly SpriteRenderer[] _miniGuideDots = new SpriteRenderer[MiniGuideDots];
        SpriteRenderer _miniGuideArrow, _miniGuideArrowOutline, _miniGuideIcon, _miniGuideText;
        string _miniGuideCache;
        int _miniDotCount;
        Vector2 _miniPlayerPos;
        int _miniPlayerDir;
        string _lastMiniTitle;
        int _miniTitleChars = -1;
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
            _miniNorth = Text(_minimapGroup, "MiniMapKey", "F: BUKA PETA", 1, Gold, new Vector2(0.5f, 0.5f), 4000);
            _miniDivider = Solid(_minimapGroup, "MiniDivider", new Color(1f, 1f, 1f, 0.10f), 3991);

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

            for (int i = 0; i < MiniGuideDots; i++)
            {
                _miniGuideDots[i] = Solid(_minimapGroup, "MiniGuideDot" + i, LegendGoal, 3995);
                _miniGuideDots[i].gameObject.SetActive(false);
            }
            _miniGuideArrowOutline = Make(_minimapGroup, PixelArt.Get("lc_icon_direction"), "MiniGuideArrowOutline", 3998);
            _miniGuideArrowOutline.color = MiniOutline;
            _miniGuideArrow = Make(_minimapGroup, PixelArt.Get("lc_icon_direction"), "MiniGuideArrow", 3999);
            _miniGuideIcon = Make(_minimapGroup, PixelArt.Get("lc_icon_direction"), "MiniGuideIcon", 3999);
            _miniGuideIcon.transform.localScale = Vector3.one * 0.32f;
            _miniGuideText = Text(_minimapGroup, "MiniGuideText", " ", 1, LegendGoal, new Vector2(0f, 0.5f), 4000);
            _miniGuideArrow.gameObject.SetActive(false);
            _miniGuideArrowOutline.gameObject.SetActive(false);
            _miniGuideIcon.gameObject.SetActive(false);
            _miniGuideText.gameObject.SetActive(false);

            for (int i = 0; i < 2; i++)
            {
                _miniExitArrowOutline[i] = Make(_minimapGroup, PixelArt.Get("lc_icon_direction"), "MiniExitOutline" + i, 3998);
                _miniExitArrowOutline[i].color = MiniOutline;
                _miniExitArrow[i] = Make(_minimapGroup, PixelArt.Get("lc_icon_direction"), "MiniExit" + i, 3999);
                _miniExitText[i] = Text(_minimapGroup, "MiniExitText" + i, " ", 1, MiniExitOpen,
                    new Vector2(i == 0 ? 0f : 1f, 0.5f), 4000);
                Show(_miniExitArrow[i], false);
                Show(_miniExitArrowOutline[i], false);
                Show(_miniExitText[i], false);
            }

            _miniPlayerOutline = Make(_minimapGroup, PixelArt.Get("lc_icon_direction"), "MiniPlayerOutline", 3998);
            _miniPlayerOutline.color = MiniOutline;
            _miniPlayer = Make(_minimapGroup, PixelArt.Get("lc_icon_direction"), "MiniPlayer", 3999);

            // Legenda warna ada di layar PETA (F) agar minimap tetap ringkas.

            _minimapGroup.gameObject.SetActive(false);
        }

        // ------------------------------------------------------------ api

        public void SetMinimapVisible(bool visible)
        {
            if (_minimapGroup != null) _minimapGroup.gameObject.SetActive(visible);
            if (!visible) { _miniEdgeRight = -999f; _miniEdgeTop = -999f; }   // batas lama tidak dipakai lagi
        }

        /// <summary>Minimap diketuk / diklik pada frame ini (membuka peta besar).</summary>
        public bool MinimapClicked()
        {
            if (_cam == null || _minimapGroup == null || !_minimapGroup.gameObject.activeInHierarchy || !LifeInput.PointerTapped)
                return false;
            return _miniRect.Contains(PointerToHud());
        }

        /// <summary>Wilayah di balik ujung kiri / kanan peta (null = tidak ada jalan keluar).</summary>
        public void SetMinimapExits(string left, bool leftOpen, string right, bool rightOpen)
        {
            _miniExitName[0] = left; _miniExitOpen[0] = leftOpen;
            _miniExitName[1] = right; _miniExitOpen[1] = rightOpen;
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
                _miniTitleChars = -1;   // dipotong sesuai lebar saat layout
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

            // Kecil, di pojok kiri bawah layar. Susunan (atas -> bawah), semua rata pada tepi peta:
            //   judul wilayah | peta | ◀ KIRI ..... KANAN ▶ | garis | tujuan misi | F: BUKA PETA
            const float pad = 0.3f, titleH = 0.66f, rowH = 0.58f;
            float mapW = Mathf.Min(5.4f, halfW * 0.4f);
            float mapH = mapW * (LifeWorld.Height / LifeWorld.Width);
            float w = mapW + pad * 2f;

            // Baris jalan keluar: satu baris bila kedua nama muat, dua baris bila tidak.
            bool exitL = !string.IsNullOrEmpty(_miniExitName[0]), exitR = !string.IsNullOrEmpty(_miniExitName[1]);
            int exitChars = (exitL ? _miniExitName[0].Length + 1 : 0) + (exitR ? _miniExitName[1].Length + 1 : 0);
            int exitRows = !exitL && !exitR ? 0 : (exitL && exitR && (exitChars + 1) * UiCharW > mapW) ? 2 : 1;

            int guide = NearestQuestGoal(out float guideDist);
            bool guideOn = guide >= 0 && guideDist > 1.6f;
            int footRows = guideOn ? 2 : 1;
            float h = titleH + mapH + 0.06f + exitRows * MiniExitRowH + 0.1f + footRows * rowH + 0.14f;
            float left = -halfW + 0.28f;
            float bottom = -halfH + 0.28f;
            float top = bottom + h;
            float cx = left + w * 0.5f;
            float cy = bottom + h * 0.5f;
            _miniEdgeRight = left + w;
            _miniEdgeTop = top;
            _miniRect = new Rect(left, bottom, w, h);
            if (PlayHitsOn) Hit(UiHit.Minimap, 0, _miniRect, 0);

            SetRect(_miniPanel, cx, cy, w, h);
            float titleY = top - titleH * 0.5f - 0.02f;
            _miniTitle.transform.localPosition = new Vector3(left + pad, titleY, 0f);
            int titleChars = CharsFor(mapW);
            if (titleChars != _miniTitleChars && !string.IsNullOrEmpty(_lastMiniTitle))
            {
                _miniTitleChars = titleChars;
                string t = _lastMiniTitle;
                int sp = t.IndexOf(' ');
                if (t.Length > titleChars && sp > 0) t = t.Substring(sp + 1);   // "PASAR RAMAI JAYA" -> "RAMAI JAYA"
                if (t.Length > titleChars) t = t.Substring(0, Mathf.Max(1, titleChars - 1)).TrimEnd() + ".";
                SetText(_miniTitle, t, Ink, 1, new Vector2(0f, 0.5f));
            }

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

            // Jalan keluar: panah di tepi peta (setinggi jalan utama) + nama wilayah di bawah peta.
            float roadY = PulangWorld.EdgeRoadY.TryGetValue(_miniRegion, out float ry) ? ry : LifeWorld.Height * 0.5f;
            float arrowY = mapB + mapH * Mathf.Clamp01(roadY / LifeWorld.Height);
            float exitTop = mapB - 0.06f - MiniExitRowH * 0.5f;
            float nudge = (Mathf.Sin(_time * 2.5f) * 0.5f + 0.5f) * 0.06f;
            for (int i = 0; i < 2; i++)
            {
                bool on = i == 0 ? exitL : exitR;
                Show(_miniExitArrow[i], on);
                Show(_miniExitArrowOutline[i], on);
                Show(_miniExitText[i], on);
                if (!on) continue;
                Color tint = _miniExitOpen[i] ? MiniExitOpen : MiniExitLocked;
                float side = i == 0 ? -1f : 1f;
                float ax = (i == 0 ? mapL + 0.2f : mapL + mapW - 0.2f) + side * (_miniExitOpen[i] ? nudge : 0f);
                float exitAngle = i == 0 ? 90f : -90f;
                _miniExitArrow[i].color = tint;
                _miniExitArrow[i].transform.localPosition = new Vector3(ax, arrowY, 0f);
                _miniExitArrow[i].transform.localRotation = Quaternion.Euler(0f, 0f, exitAngle);
                _miniExitArrow[i].transform.localScale = Vector3.one * 0.3f;
                _miniExitArrowOutline[i].transform.localPosition = new Vector3(ax, arrowY, 0f);
                _miniExitArrowOutline[i].transform.localRotation = Quaternion.Euler(0f, 0f, exitAngle);
                _miniExitArrowOutline[i].transform.localScale = Vector3.one * 0.4f;

                string label = i == 0 ? "◀" + _miniExitName[0] : _miniExitName[1] + "▶";
                string key = label + (_miniExitOpen[i] ? "1" : "0");
                if (key != _miniExitBuilt[i])
                {
                    _miniExitBuilt[i] = key;
                    SetText(_miniExitText[i], label, tint, 1, new Vector2(i == 0 ? 0f : 1f, 0.5f));
                }
                float ey = exitRows == 2 && i == 1 ? exitTop - MiniExitRowH : exitTop;
                _miniExitText[i].transform.localPosition = new Vector3(i == 0 ? mapL : mapL + mapW, ey, 0f);
            }

            // Garis pemisah, lalu baris bawah: tujuan misi (bila ada) dan petunjuk membuka peta.
            float footTop = mapB - 0.06f - exitRows * MiniExitRowH;
            SetSolid(_miniDivider, cx, footTop - 0.04f, mapW, 0.04f);
            float guideRowY = footTop - 0.1f - rowH * 0.5f;
            float hintRowY = guideRowY - (guideOn ? rowH : 0f);
            _miniNorth.transform.localPosition = new Vector3(cx, hintRowY, 0f);
            SetAlpha(_miniNorth, 0.85f);   // tetap (tidak berkedip)

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

            LayoutMiniGuide(guideOn ? guide : -1, guideDist, pl, mapL, mapB, mapW, mapH, guideRowY);
        }

        void LayoutMiniGuide(int goal, float dist, Vector2 pl, float mapL, float mapB, float mapW, float mapH, float rowY)
        {
            bool on = goal >= 0;
            Show(_miniGuideArrow, on);
            Show(_miniGuideArrowOutline, on);
            Show(_miniGuideIcon, on);
            Show(_miniGuideText, on);
            if (!on)
            {
                for (int i = 0; i < MiniGuideDots; i++) Show(_miniGuideDots[i], false);
                return;
            }

            QuestGoal g = _questGoals[goal];
            Color tint = g.IsJob ? JobArrowTint : LegendGoal;
            Vector2 target = MapToMini(g.Position, mapL, mapB, mapW, mapH);
            Vector2 delta = target - pl;
            float len = delta.magnitude;
            Vector2 dir = len > 0.0001f
                ? delta / len
                : (g.Position - _miniPlayerPos).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

            // Garis putus-putus yang "berjalan" dari pemain ke tujuan.
            const float spacing = 0.2f, startGap = 0.3f;
            float march = Mathf.Repeat(_time * 0.6f, spacing);
            for (int i = 0; i < MiniGuideDots; i++)
            {
                float t = startGap + march + i * spacing;
                bool show = t < len - 0.12f;
                Show(_miniGuideDots[i], show);
                if (!show) continue;
                Vector2 p = pl + dir * t;
                var dot = _miniGuideDots[i];
                dot.color = tint;
                dot.transform.localPosition = new Vector3(p.x, p.y, 0f);
                dot.transform.localScale = Vector3.one * 0.07f;
            }

            // Panah kecil di samping pemain, berdenyut ke arah tujuan.
            float bob = (Mathf.Sin(_time * 5f) * 0.5f + 0.5f) * 0.06f;
            Vector2 a = pl + dir * (0.42f + bob);
            _miniGuideArrow.color = tint;
            _miniGuideArrow.transform.localPosition = new Vector3(a.x, a.y, 0f);
            _miniGuideArrow.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            _miniGuideArrow.transform.localScale = Vector3.one * 0.3f;
            _miniGuideArrowOutline.transform.localPosition = new Vector3(a.x, a.y, 0f);
            _miniGuideArrowOutline.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            _miniGuideArrowOutline.transform.localScale = Vector3.one * 0.4f;

            // Baris di bawah legenda: ikon arah + nama tujuan + jarak.
            _miniGuideIcon.color = tint;
            _miniGuideIcon.transform.localPosition = new Vector3(mapL + 0.12f, rowY, 0f);
            _miniGuideIcon.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            string meters = " " + Mathf.RoundToInt(dist) + "M";
            int maxChars = Mathf.Max(4, Mathf.FloorToInt((mapW - 0.35f) / PixelFont.CharW));
            string label = string.IsNullOrEmpty(g.Label) ? "TUJUAN" : g.Label;
            int room = maxChars - meters.Length;
            if (label.Length > room) label = label.Substring(0, Mathf.Max(1, room - 1)) + ".";
            string text = label + meters;
            string key = (g.IsJob ? "J" : "Q") + text;
            if (key != _miniGuideCache)
            {
                _miniGuideCache = key;
                SetText(_miniGuideText, text, tint, 1, new Vector2(0f, 0.5f));
            }
            _miniGuideText.transform.localPosition = new Vector3(mapL + 0.38f, rowY, 0f);
        }

        static Vector2 MapToMini(Vector2 world, float left, float bottom, float width, float height)
        {
            float x = left + Mathf.Clamp01(world.x / LifeWorld.Width) * width;
            float y = bottom + Mathf.Clamp01(world.y / LifeWorld.Height) * height;
            return new Vector2(x, y);
        }
    }
}
