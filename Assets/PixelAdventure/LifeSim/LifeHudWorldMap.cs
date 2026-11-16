using System.Collections.Generic;
using PixelAdventure.LifeSim.Pulang;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>Satu wilayah di jalur peta (urut dari ujung kiri ke ujung kanan).</summary>
    public struct WorldMapRegion
    {
        public string Name;
        public bool Here, Locked, Quest;
    }

    /// <summary>Satu tempat (yang sudah terbuka) di peta wilayah yang dipilih.</summary>
    public struct WorldMapPlace
    {
        public string Name;
        public Vector2 Position;
        public bool Goal, Terminal;
        public int Meters;   // jarak dari pemain (-1 bila di wilayah lain)
    }

    /// <summary>Isi layar PETA. Dibuat ulang oleh PulangGame setiap kali pilihan berganti.</summary>
    public class WorldMapView
    {
        public readonly List<WorldMapRegion> Regions = new List<WorldMapRegion>();
        public int Here, Selected;
        public Region Region;                         // wilayah yang dipilih (gaya peta)
        public string RegionName = "";
        public readonly List<WorldMapPlace> Places = new List<WorldMapPlace>();
        public int SelectedPlace = -1;
        public int LockedPlaces;
        public readonly List<Vector2> Goals = new List<Vector2>();     // tujuan misi (wilayah saat ini)
        public readonly List<Vector2> Jobs = new List<Vector2>();      // tempat kerja yang sedang buka
        public readonly List<Vector2> Friends = new List<Vector2>();   // teman di wilayah ini
        public string RouteHead = "";
        public bool RouteOk = true;
        public readonly List<string> Route = new List<string>();
        public bool ShowPlayer;
        public Vector2 Player;
        public int PlayerDir;
        public string ExitLeft, ExitRight;
        public bool ExitLeftOpen, ExitRightOpen;
    }

    /// <summary>
    /// Layar PETA (tombol F / ketuk minimap / aplikasi PETA di tablet), tersusun rapi:
    /// - kepala : "PETA" dan posisi pemain saat ini;
    /// - atas   : JALUR WILAYAH — kelima wilayah berurutan kiri ke kanan seperti di dunia game;
    ///            "KAMU DI SINI", terkunci, bintang = ada misi, jalur ke wilayah pilihan disorot emas;
    /// - kiri   : peta besar wilayah pilihan (jalan, laut, tempat, teman, kerja, tujuan misi, pemain,
    ///            panah jalan keluar). Tempat yang dipilih diberi cincin + label nama;
    /// - kanan  : RUTE (arah, wilayah yang dilewati, waktu, bus), daftar TEMPAT yang bisa dipilih
    ///            (W/S / ketuk, bergulir otomatis), dan legenda warna.
    /// </summary>
    public partial class LifeHud
    {
        static readonly Color WmCard = new Color(0.10f, 0.08f, 0.14f, 1f);
        static readonly Color WmNodeBg = new Color(0.16f, 0.13f, 0.22f, 1f);
        static readonly Color WmNodeLocked = new Color(0.11f, 0.10f, 0.14f, 1f);
        static readonly Color WmLink = new Color(0.40f, 0.36f, 0.48f, 1f);
        static readonly Color WmRule = new Color(1f, 0.84f, 0.40f, 0.35f);
        static readonly Color WmTerminal = new Color(0.45f, 0.80f, 1f, 1f);
        static readonly Color WmPlace = new Color(0.95f, 0.95f, 1f, 1f);
        static readonly Color WmLocked = new Color(0.55f, 0.52f, 0.60f, 1f);
        static readonly Color WmSelRow = new Color(1f, 0.84f, 0.40f, 0.16f);
        static readonly Color WmLabelBg = new Color(0.06f, 0.05f, 0.10f, 0.9f);

        const float WmPad = 0.35f;
        static readonly string[] WmLegendLabels = { "KAMU", "MISI", "KERJA", "TEMAN", "TERMINAL", "TEMPAT" };

        Transform _wmGroup;
        UiPool _wui;
        WorldMapView _wm = new WorldMapView();
        readonly List<Rect> _wmNodeRects = new List<Rect>();
        readonly List<Rect> _wmPlaceRects = new List<Rect>();
        readonly List<int> _wmPlaceRectIndex = new List<int>();
        readonly List<Vector2> _wmPlaceDots = new List<Vector2>();

        void BuildWorldMap()
        {
            _wmGroup = Group("WorldMap");
            _wui = new UiPool(_wmGroup, 4010);
            _wmGroup.gameObject.SetActive(false);
        }

        public bool WorldMapVisible => _wmGroup != null && _wmGroup.gameObject.activeSelf;

        public void ShowWorldMap(WorldMapView view)
        {
            _wm = view ?? new WorldMapView();
            SetDim(0.8f);
            _wmGroup.gameObject.SetActive(true);
            HidePrompt();
        }

        public void HideWorldMap()
        {
            if (_wmGroup != null) _wmGroup.gameObject.SetActive(false);
            SetDim(0f);
        }

        /// <summary>Kotak wilayah yang diketuk pada frame ini (-1 bila tidak ada).</summary>
        public int WorldMapTappedRegion()
        {
            if (!WorldMapVisible || _cam == null || !LifeInput.PointerTapped) return -1;
            Vector2 p = PointerToHud();
            for (int i = 0; i < _wmNodeRects.Count; i++)
                if (_wmNodeRects[i].Contains(p)) return i;
            return -1;
        }

        /// <summary>Tempat yang diketuk (baris daftar atau titik di peta) pada frame ini (-1 bila tidak ada).</summary>
        public int WorldMapTappedPlace()
        {
            if (!WorldMapVisible || _cam == null || !LifeInput.PointerTapped) return -1;
            Vector2 p = PointerToHud();
            for (int i = 0; i < _wmPlaceRects.Count; i++)
                if (_wmPlaceRects[i].Contains(p)) return _wmPlaceRectIndex[i];
            int best = -1;
            float bestD = 0.55f * 0.55f;
            for (int i = 0; i < _wmPlaceDots.Count; i++)
            {
                float d = (_wmPlaceDots[i] - p).sqrMagnitude;
                if (d < bestD) { bestD = d; best = i; }
            }
            return best;
        }

        // ------------------------------------------------------------------ layout

        void LayoutWorldMap(float halfW, float halfH)
        {
            if (!WorldMapVisible) return;
            var ui = _wui;
            var v = _wm;
            ui.Begin();

            float w = Mathf.Min(halfW * 2f - 0.8f, 30f);
            float h = halfH * 2f - 0.8f;
            float top = h * 0.5f, left = -w * 0.5f;
            float innerL = left + 0.7f, innerW = w - 1.4f, innerR = innerL + innerW;
            ui.Panel("lc_panel_gold", 0f, 0f, w, h, 0);
            CloseAt(w * 0.5f, top);

            // ---------------------------------------------------------- kepala
            ui.Text("PETA", innerL, top - 0.75f, Gold, 2);
            string here = v.Here >= 0 && v.Here < v.Regions.Count ? Safe(v.Regions[v.Here].Name) : "";
            float hereW = here.Length * UiCharW;
            ui.Text(here, innerR - 0.35f, top - 0.75f, Ink, 1, 1f);
            ui.Text("KAMU DI:", innerR - 0.35f - hereW - 0.3f, top - 0.75f, Soft, 1, 1f);
            ui.Solid(innerR - 0.12f, top - 0.75f, 0.2f, 0.2f, Pink, 2);
            ui.Text(CompactHint("A/D PILIH WILAYAH    W/S PILIH TEMPAT    F/ESC TUTUP", CharsFor(w - 1.4f)), 0f, -top + 0.45f, Gold, 1, 0.5f);

            // ---------------------------------------------------------- jalur wilayah
            float stripTop = top - 1.3f, stripH = 2.6f;
            ui.Solid(0f, stripTop - stripH * 0.5f, innerW, stripH, WmCard, 1);
            LayoutWorldStrip(ui, v, innerL + WmPad, innerW - WmPad * 2f, stripTop - 0.25f, stripH - 0.5f);

            // ---------------------------------------------------------- badan
            float bodyTop = stripTop - stripH - 0.3f;
            float bodyBottom = -top + 0.85f;
            float bodyH = bodyTop - bodyBottom;

            // Peta kiri: rasio dunia 4:3, sebesar mungkin; kolom kanan minimal 10 unit.
            const float cardTitleH = 0.72f;
            float aspect = LifeWorld.Width / LifeWorld.Height;
            float mapH = bodyH - cardTitleH - WmPad;
            float mapW = mapH * aspect;
            float maxMapW = innerW - 10f - WmPad * 2f - 0.3f;
            if (mapW > maxMapW) { mapW = Mathf.Max(4f, maxMapW); mapH = mapW / aspect; }
            float cardW = mapW + WmPad * 2f;
            float cardCy = bodyTop - bodyH * 0.5f;
            ui.Solid(innerL + cardW * 0.5f, cardCy, cardW, bodyH, WmCard, 1);

            float mapL = innerL + WmPad;
            float mapT = bodyTop - cardTitleH;
            float mapB = mapT - mapH;
            float titleY = bodyTop - 0.38f;
            ui.Text(Safe(v.RegionName), mapL, titleY, Gold);
            ui.Text(v.Places.Count + " TEMPAT", mapL + mapW, titleY, Soft, 1, 1f);
            LayoutWorldMapArea(ui, v, mapL, mapB, mapW, mapH);

            // Kolom kanan.
            float rl = innerL + cardW + 0.3f;
            float rw = innerR - rl;
            ui.Solid(rl + rw * 0.5f, cardCy, rw, bodyH, WmCard, 1);
            LayoutWorldInfo(ui, v, rl + WmPad, rw - WmPad * 2f, titleY, bodyBottom + 0.3f);

            ui.End();
        }

        void LayoutWorldStrip(UiPool ui, WorldMapView v, float left, float width, float top, float nodeH)
        {
            _wmNodeRects.Clear();
            int n = v.Regions.Count;
            if (n == 0) return;
            const float gap = 0.6f;
            float nodeW = (width - gap * (n - 1)) / n;
            float cy = top - nodeH * 0.5f;
            int from = Mathf.Min(v.Here, v.Selected), to = Mathf.Max(v.Here, v.Selected);
            float pulse = Mathf.Sin(_time * 2.5f) * 0.5f + 0.5f;

            // Jalan penghubung (jalur pemain -> pilihan disorot emas dengan panah arah).
            for (int i = 0; i < n - 1; i++)
            {
                float x0 = left + (i + 1) * nodeW + i * gap;
                bool onRoute = v.Here >= 0 && i >= from && i < to;
                ui.Solid(x0 + gap * 0.5f, cy, gap + 0.1f, onRoute ? 0.16f : 0.08f, onRoute ? Gold : WmLink, 2);
            }

            int chars = CharsFor(nodeW - 0.2f);
            for (int i = 0; i < n; i++)
            {
                var r = v.Regions[i];
                float x = left + i * (nodeW + gap);
                float ncx = x + nodeW * 0.5f;
                bool sel = i == v.Selected;
                _wmNodeRects.Add(new Rect(x, cy - nodeH * 0.5f, nodeW, nodeH));
                Hit(UiHit.MapRegion, i, _wmNodeRects[i]);

                if (sel) ui.Solid(ncx, cy, nodeW + 0.16f, nodeH + 0.16f, Gold, 2);
                else if (r.Here) ui.Solid(ncx, cy, nodeW + 0.1f, nodeH + 0.1f, Pink, 2);
                ui.Solid(ncx, cy, nodeW, nodeH, r.Locked ? WmNodeLocked : WmNodeBg, 3);

                // Nama (maks. 2 baris) rata tengah di bagian atas kotak, keterangan di dasar kotak.
                string[] lines = Wrap(Safe(r.Name), chars);
                int count = Mathf.Min(2, lines.Length);
                Color nameC = r.Locked ? WmLocked : sel ? Gold : Ink;
                float nameTop = cy + nodeH * 0.5f - 0.4f;
                float shift = count == 1 ? -0.29f : 0f;   // satu baris: turunkan sedikit agar seimbang
                for (int l = 0; l < count; l++)
                    ui.Text(lines[l], ncx, nameTop + shift - l * 0.58f, nameC, 1, 0.5f, 5);

                string tag; Color tagC;
                if (r.Here) { tag = "KAMU DI SINI"; tagC = Pink; }
                else if (r.Locked) { tag = "TERKUNCI"; tagC = WmLocked; }
                else if (v.Here >= 0)
                {
                    int steps = Mathf.Abs(i - v.Here);
                    tag = i < v.Here ? "◀ " + steps + " WILAYAH" : steps + " WILAYAH ▶";
                    tagC = sel ? Gold : Soft;
                }
                else { tag = " "; tagC = Soft; }
                if (tag.Length > chars) tag = r.Here ? "KAMU" : tag.Replace(" WILAYAH", "");
                float tagY = cy - nodeH * 0.5f + 0.32f;
                ui.Solid(ncx, tagY + 0.27f, nodeW - 0.4f, 0.04f, new Color(1f, 1f, 1f, 0.08f), 4);
                ui.Text(tag, ncx, tagY, tagC, 1, 0.5f, 5);

                if (r.Quest)
                {
                    var s = ui.Sprite(PixelArt.Get("lc_icon_star"), x + nodeW - 0.28f, cy + nodeH * 0.5f - 0.28f,
                        0.5f + 0.08f * pulse, 6);
                    s.transform.localRotation = Quaternion.identity;
                }
            }
        }

        void LayoutWorldMapArea(UiPool ui, WorldMapView v, float mapL, float mapB, float mapW, float mapH)
        {
            float cx = mapL + mapW * 0.5f, cy = mapB + mapH * 0.5f;
            float mapT = mapB + mapH, mapR = mapL + mapW;
            float pulse = Mathf.Sin(_time * 2.5f) * 0.5f + 0.5f;
            ui.Solid(cx, cy, mapW + 0.16f, mapH + 0.16f, MiniOutline, 2);

            Color ground = new Color(0.4f, 0.6f, 0.35f, 1f);
            if (MiniStyles.TryGetValue(v.Region, out MiniStyle s))
            {
                ground = s.Ground;
                ui.Solid(cx, cy, mapW, mapH, ground, 3);
                if (s.SeaFromY >= 0f)
                {
                    float seaB = mapB + mapH * (s.SeaFromY / LifeWorld.Height);
                    ui.Solid(cx, (seaB + mapT) * 0.5f, mapW, mapT - seaB, new Color(0.30f, 0.62f, 0.85f), 4);
                }
                foreach (var r in s.Roads)
                {
                    Vector2 a = MapToMini(new Vector2(r.xMin, r.yMin), mapL, mapB, mapW, mapH);
                    Vector2 b = MapToMini(new Vector2(r.xMax, r.yMax), mapL, mapB, mapW, mapH);
                    ui.Solid((a.x + b.x) * 0.5f, (a.y + b.y) * 0.5f, Mathf.Max(0.12f, b.x - a.x), Mathf.Max(0.12f, b.y - a.y), s.Road, 5);
                }
            }
            else ui.Solid(cx, cy, mapW, mapH, ground, 3);

            // Jalan keluar ke wilayah sebelah: panah di tepi + papan nama di sudut bawah peta.
            float roadY = PulangWorld.EdgeRoadY.TryGetValue(v.Region, out float ry) ? ry : LifeWorld.Height * 0.5f;
            float ay = mapB + mapH * Mathf.Clamp01(roadY / LifeWorld.Height);
            Sprite arrow = PixelArt.Get("lc_icon_direction");
            for (int side = 0; side < 2; side++)
            {
                string name = side == 0 ? v.ExitLeft : v.ExitRight;
                if (string.IsNullOrEmpty(name)) continue;
                bool open = side == 0 ? v.ExitLeftOpen : v.ExitRightOpen;
                Color tint = open ? Gold : WmLocked;
                float nudge = open ? pulse * 0.12f : 0f;
                float ax = side == 0 ? mapL + 0.35f - nudge : mapR - 0.35f + nudge;
                float angle = side == 0 ? 90f : -90f;
                var o = ui.Sprite(arrow, ax, ay, 0.62f, 8, MiniOutline);
                o.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
                var a = ui.Sprite(arrow, ax, ay, 0.48f, 9, tint);
                a.transform.localRotation = Quaternion.Euler(0f, 0f, angle);

                string label = side == 0 ? "◀ KE " + name : "KE " + name + " ▶";
                if (!open) label += " (TERKUNCI)";
                float lw = label.Length * UiCharW + 0.3f;
                float lx = side == 0 ? mapL + 0.12f + lw * 0.5f : mapR - 0.12f - lw * 0.5f;
                float ly = mapB + 0.37f;
                ui.Solid(lx, ly, lw, 0.5f, WmLabelBg, 8);
                ui.Text(label, lx, ly, tint, 1, 0.5f, 9);
            }

            // Tempat. Yang dipilih diberi cincin & label nama (diletakkan agar tetap di dalam peta).
            _wmPlaceDots.Clear();
            for (int i = 0; i < v.Places.Count; i++)
            {
                var p = v.Places[i];
                Vector2 m = MapToMini(p.Position, mapL, mapB, mapW, mapH);
                _wmPlaceDots.Add(m);
                Color c = p.Goal ? Gold : p.Terminal ? WmTerminal : WmPlace;
                float size = p.Goal ? 0.38f + 0.08f * pulse : 0.28f;
                ui.Solid(m.x, m.y, size + 0.1f, size + 0.1f, MiniOutline, 6);
                ui.Solid(m.x, m.y, size, size, c, 7);
            }
            foreach (var j in v.Jobs)
            {
                Vector2 m = MapToMini(j, mapL, mapB, mapW, mapH);
                var st = ui.Sprite(PixelArt.Get("lc_icon_star"), m.x, m.y, 0.62f, 8);
                st.transform.localRotation = Quaternion.identity;
            }
            foreach (var f in v.Friends)
            {
                Vector2 m = MapToMini(f, mapL, mapB, mapW, mapH);
                ui.Solid(m.x, m.y, 0.36f, 0.36f, MiniOutline, 6);
                ui.Solid(m.x, m.y, 0.26f, 0.26f, LegendGirl, 7);
            }
            foreach (var g in v.Goals)
            {
                Vector2 m = MapToMini(g, mapL, mapB, mapW, mapH);
                float size = 0.38f + 0.1f * pulse;
                ui.Solid(m.x, m.y, size + 0.1f, size + 0.1f, MiniOutline, 6);
                ui.Solid(m.x, m.y, size, size, LegendGoal, 7);
            }

            // Pemain.
            if (v.ShowPlayer)
            {
                Vector2 pl = MapToMini(v.Player, mapL, mapB, mapW, mapH);
                float angle = v.PlayerDir == 1 ? 0f : v.PlayerDir == 2 ? 90f : v.PlayerDir == 3 ? -90f : 180f;
                var o = ui.Sprite(arrow, pl.x, pl.y, 0.85f, 10, MiniOutline);
                o.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
                var a = ui.Sprite(arrow, pl.x, pl.y, 0.66f, 11, Color.white);
                a.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            }

            // Tempat terpilih: cincin berdenyut + label di atas (atau di bawah bila terlalu dekat tepi atas).
            if (v.SelectedPlace >= 0 && v.SelectedPlace < v.Places.Count)
            {
                var p = v.Places[v.SelectedPlace];
                Vector2 m = _wmPlaceDots[v.SelectedPlace];
                float ring = 0.62f + 0.12f * pulse;
                ui.Solid(m.x, m.y, ring + 0.1f, ring + 0.1f, Gold, 12);
                ui.Solid(m.x, m.y, ring - 0.06f, ring - 0.06f, MiniOutline, 12);
                ui.Solid(m.x, m.y, 0.3f, 0.3f, p.Goal ? Gold : p.Terminal ? WmTerminal : WmPlace, 13);

                string label = Safe(p.Name) + (p.Meters >= 0 ? "  " + p.Meters + "M" : "");
                int maxChars = CharsFor(mapW - 0.6f);
                if (label.Length > maxChars) label = label.Substring(0, maxChars - 1) + ".";
                float lw = label.Length * UiCharW + 0.4f, lh = 0.55f;
                float ly = m.y + 0.75f;
                if (ly + lh * 0.5f > mapT - 0.05f) ly = m.y - 0.75f;
                float lx = Mathf.Clamp(m.x, mapL + lw * 0.5f + 0.05f, mapR - lw * 0.5f - 0.05f);
                ui.Solid(lx, ly, lw + 0.1f, lh + 0.1f, Gold, 13);
                ui.Solid(lx, ly, lw, lh, WmLabelBg, 14);
                ui.Text(label, lx, ly, Ink, 1, 0.5f, 15);
            }
        }

        void LayoutWorldInfo(UiPool ui, WorldMapView v, float left, float width, float top, float bottom)
        {
            int chars = CharsFor(width);
            float y = top;

            // ---------------- legenda (dasar kolom; turun ke baris kedua bila tidak muat)
            int legRows = 1;
            float measure = 0f;
            foreach (string label in WmLegendLabels)
            {
                float itemW = 0.45f + label.Length * UiCharW + 0.5f;
                if (measure > 0f && measure + itemW - 0.5f > width) { legRows = 2; measure = 0f; }
                measure += itemW;
            }
            float lx = left;
            float ly = bottom + 0.2f + (legRows - 1) * 0.5f;
            float legendTop = ly + 0.38f;
            for (int i = 0; i < WmLegendLabels.Length; i++)
            {
                string label = WmLegendLabels[i];
                float itemW = 0.45f + label.Length * UiCharW + 0.5f;
                if (lx > left && lx + itemW - 0.5f > left + width) { lx = left; ly -= 0.5f; }
                if (i == 0)
                {
                    var a = ui.Sprite(PixelArt.Get("lc_icon_direction"), lx + 0.13f, ly, 0.42f, 4, Color.white);
                    a.transform.localRotation = Quaternion.identity;
                }
                else if (i == 2)
                {
                    var st = ui.Sprite(PixelArt.Get("lc_icon_star"), lx + 0.13f, ly, 0.45f, 4);
                    st.transform.localRotation = Quaternion.identity;
                }
                else
                {
                    Color c = i == 1 ? LegendGoal : i == 3 ? LegendGirl : i == 4 ? WmTerminal : WmPlace;
                    ui.Solid(lx + 0.13f, ly, 0.34f, 0.34f, MiniOutline, 3);
                    ui.Solid(lx + 0.13f, ly, 0.24f, 0.24f, c, 4);
                }
                ui.Text(label, lx + 0.45f, ly, Soft);
                lx += itemW;
            }
            ui.Solid(left + width * 0.5f, legendTop, width, 0.04f, new Color(1f, 1f, 1f, 0.08f), 2);

            // ---------------- RUTE
            ui.Text("RUTE", left, y, Gold);
            ui.Solid(left + width * 0.5f, y - 0.32f, width, 0.05f, WmRule, 2);
            y -= 0.78f;
            foreach (string line in Wrap(Safe(v.RouteHead), chars))
            {
                ui.Text(line, left, y, v.RouteOk ? Gold : Pink);
                y -= UiLine;
            }
            y -= 0.08f;
            float routeLimit = legendTop + 2.6f;   // sisakan ruang untuk daftar tempat
            foreach (string item in v.Route)
            {
                string[] lines = Wrap(Safe(item), chars - 2);
                for (int l = 0; l < lines.Length && y > routeLimit; l++)
                {
                    if (l == 0) ui.Solid(left + 0.1f, y, 0.14f, 0.14f, Soft, 3);
                    ui.Text(lines[l], left + 0.4f, y, Ink);
                    y -= UiLine;
                }
            }

            // ---------------- TEMPAT (bergulir mengikuti pilihan)
            y -= 0.3f;
            float headY = y;
            ui.Text("TEMPAT", left, headY, Gold);
            ui.Solid(left + width * 0.5f, headY - 0.32f, width, 0.05f, WmRule, 2);
            y -= 0.8f;

            _wmPlaceRects.Clear();
            _wmPlaceRectIndex.Clear();
            const float row = 0.62f;
            float listBottom = legendTop + 0.3f + (v.LockedPlaces > 0 ? row : 0f);
            int fit = Mathf.Max(1, Mathf.FloorToInt((y - listBottom) / row) + 1);
            int count = v.Places.Count;
            int first = 0;
            if (count > fit)
            {
                int sel = Mathf.Max(0, v.SelectedPlace);
                first = Mathf.Clamp(sel - fit / 2, 0, count - fit);
            }
            // Posisi gulir di kanan judul, mis. "4-12 / 25".
            string range = count > fit ? (first + 1) + "-" + Mathf.Min(count, first + fit) + " / " + count : "W/S PILIH";
            ui.Text(range, left + width, headY, Soft, 1, 1f);
            int nameChars = CharsFor(width - 0.9f) - (v.Places.Count > 0 && v.Places[0].Meters >= 0 ? 5 : 0);
            for (int k = 0; k < fit && first + k < count; k++)
            {
                int i = first + k;
                var p = v.Places[i];
                float ry = y - k * row;
                bool sel = i == v.SelectedPlace;
                if (sel)
                {
                    ui.Solid(left + width * 0.5f, ry, width + 0.2f, row - 0.04f, WmSelRow, 2);
                    ui.Solid(left - 0.05f, ry, 0.08f, row - 0.04f, Gold, 3);
                }
                Color c = p.Goal ? Gold : p.Terminal ? WmTerminal : Ink;
                ui.Solid(left + 0.2f, ry, 0.3f, 0.3f, MiniOutline, 3);
                ui.Solid(left + 0.2f, ry, 0.2f, 0.2f, p.Goal ? Gold : p.Terminal ? WmTerminal : WmPlace, 4);
                string name = (p.Goal ? "★ " : "") + Safe(p.Name);
                if (name.Length > nameChars) name = name.Substring(0, Mathf.Max(1, nameChars - 1)) + ".";
                ui.Text(name, left + 0.6f, ry, sel ? Gold : c);
                if (p.Meters >= 0) ui.Text(p.Meters + "M", left + width, ry, Soft, 1, 1f);
                _wmPlaceRects.Add(new Rect(left - 0.1f, ry - row * 0.5f, width + 0.2f, row));
                Hit(UiHit.MapPlace, i, _wmPlaceRects[_wmPlaceRects.Count - 1]);
                _wmPlaceRectIndex.Add(i);
            }
            if (count == 0) ui.Text("BELUM ADA TEMPAT TERBUKA", left, y, WmLocked);
            if (v.LockedPlaces > 0)
                ui.Text("+" + v.LockedPlaces + " TEMPAT BELUM TERBUKA", left, legendTop + 0.45f, WmLocked);
        }
    }
}
