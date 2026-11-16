using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Satu topik buku panduan. Setiap entri ditulis "JUDUL|keterangan".
    /// Table = true: entri ditata sebagai tabel dua kolom (mis. TOMBOL | FUNGSI).
    /// </summary>
    public struct GuidePage
    {
        public string Title, Icon;
        public bool Table;
        public string[] Entries;
    }

    /// <summary>
    /// Buku panduan cara bermain:
    /// - tombol kecil di pojok kanan bawah (ikon buku + "G - PANDUAN"), bisa diklik;
    /// - buku terbuka dua halaman. Huruf diperbesar 1,25x dengan tinta gelap di atas kertas terang;
    ///   isi topik yang tidak muat otomatis bersambung ke halaman berikutnya ("LANJUTAN").
    /// </summary>
    public partial class LifeHud
    {
        const int GuideTextMax = 48;
        const float GuideTextScale = 1.25f;                      // pembesaran huruf di halaman
        const float GuideCharW = PixelFont.CharW * GuideTextScale;      // lebar satu huruf
        const float GuideLine = 0.76f;                           // jarak antar baris
        const float GuideEntryGap = 0.36f;                       // jarak antar poin
        const float GuidePad = 0.7f;                             // margin dalam halaman
        const float GuideHeaderH = 1.55f;                        // judul + garis

        static readonly Color PageBg = new Color(0.99f, 0.96f, 0.88f);
        static readonly Color PageInk = new Color(0.13f, 0.09f, 0.07f);
        static readonly Color PageKey = new Color(0.62f, 0.15f, 0.09f);
        static readonly Color PageTitle = new Color(0.48f, 0.16f, 0.10f);
        static readonly Color PageRule = new Color(0.70f, 0.40f, 0.15f, 0.7f);
        static readonly Color PageSep = new Color(0.55f, 0.40f, 0.25f, 0.22f);

        enum FlowKind { Text, Mark }

        struct FlowItem
        {
            public FlowKind Kind;
            public string Text;
            public Color Color;
            public float X, Y, W, H;    // relatif terhadap kiri-atas area isi (Y ke bawah = positif)
        }

        class FlowPage
        {
            public string Title, Icon;
            public readonly List<FlowItem> Items = new List<FlowItem>();
        }

        // ---- tombol pojok
        SpriteRenderer _guideBtnPanel, _guideBtnIcon, _guideBtnText;
        Rect _guideBtnRect;

        // ---- buku
        Transform _guideGroup;
        SpriteRenderer _guideCover, _guideSpine, _guideHint, _guidePageNo;
        readonly SpriteRenderer[] _guidePage = new SpriteRenderer[2];
        readonly SpriteRenderer[] _guideIcon = new SpriteRenderer[2];
        readonly SpriteRenderer[] _guideTitle = new SpriteRenderer[2];
        readonly SpriteRenderer[] _guideRule = new SpriteRenderer[2];
        readonly SpriteRenderer[,] _guideTexts = new SpriteRenderer[2, GuideTextMax];
        readonly SpriteRenderer[,] _guideMarks = new SpriteRenderer[2, GuideTextMax];

        GuidePage[] _guideTopics = new GuidePage[0];
        readonly List<FlowPage> _guideFlow = new List<FlowPage>();
        int _guideSpreadIndex;
        float _guideFlowW = -1f, _guideFlowH = -1f;
        int _guideShownSpread = -1;

        void BuildGuide()
        {
            _guideBtnPanel = Panel(_hudGroup, "GuideButton", "lc_panel_soft", 3990);
            _guideBtnPanel.color = new Color(1f, 1f, 1f, 0.92f);
            _guideBtnIcon = Make(_hudGroup, PixelArt.Get("lc_app_guide"), "GuideIcon", 4001);
            _guideBtnText = Text(_hudGroup, "GuideText", "G - PANDUAN", 1, Ink, new Vector2(0f, 0.5f), 4001);

            _guideGroup = Group("Guide");
            _guideCover = Panel(_guideGroup, "Cover", "lc_panel", 4010);
            _guideCover.color = new Color(0.62f, 0.36f, 0.22f, 1f);
            for (int p = 0; p < 2; p++)
            {
                _guidePage[p] = Solid(_guideGroup, "Page" + p, PageBg, 4011);
                _guideIcon[p] = Make(_guideGroup, PixelArt.Get("lc_app_guide"), "PageIcon" + p, 4013);
                _guideTitle[p] = Make(_guideGroup, null, "PageTitle" + p, 4013);
                _guideRule[p] = Solid(_guideGroup, "Rule" + p, PageRule, 4012);
                for (int i = 0; i < GuideTextMax; i++)
                {
                    _guideMarks[p, i] = Solid(_guideGroup, "Mark" + p + "_" + i, PageSep, 4012);
                    _guideTexts[p, i] = Make(_guideGroup, null, "Text" + p + "_" + i, 4013);
                    _guideTexts[p, i].transform.localScale = Vector3.one * GuideTextScale;
                }
            }
            _guideSpine = Solid(_guideGroup, "Spine", new Color(0.45f, 0.24f, 0.14f, 1f), 4012);
            _guidePageNo = Text(_guideGroup, "PageNo", " ", 1, Gold, new Vector2(0.5f, 0.5f), 4013);
            _guideHint = Text(_guideGroup, "Hint", "A/D / KLIK HALAMAN - BALIK    ESC / G - TUTUP", 1, Gold,
                new Vector2(0.5f, 0.5f), 4013);
            _guideGroup.gameObject.SetActive(false);
        }

        public bool GuideVisible => _guideGroup != null && _guideGroup.gameObject.activeSelf;

        public bool GuideButtonClicked()
        {
            if (_cam == null || _hudGroup == null || !_hudGroup.gameObject.activeSelf || !LifeInput.PointerTapped) return false;
            return _guideBtnRect.Contains(PointerToHud());
        }

        /// <summary>Buka buku dengan semua topik; halaman disusun otomatis sesuai ukuran layar.</summary>
        public void ShowGuide(GuidePage[] topics)
        {
            _guideTopics = topics ?? new GuidePage[0];
            _guideSpreadIndex = 0;
            _guideFlowW = -1f;      // susun ulang
            _guideShownSpread = -1;
            SetDim(0.75f);
            _guideGroup.gameObject.SetActive(true);
            HidePrompt();
        }

        /// <summary>Balik halaman (-1 / +1). True bila halaman berubah.</summary>
        public bool GuideFlip(int dir)
        {
            int count = Mathf.Max(1, (_guideFlow.Count + 1) / 2);
            int next = Mathf.Clamp(_guideSpreadIndex + dir, 0, count - 1);
            if (next == _guideSpreadIndex) return false;
            _guideSpreadIndex = next;
            return true;
        }

        public void HideGuide()
        {
            if (_guideGroup != null) _guideGroup.gameObject.SetActive(false);
            SetDim(0f);
        }

        static Sprite PageSprite(string text, Color color, int scale = 1) =>
            PixelFont.Render(string.IsNullOrEmpty(text) ? " " : text, color, PageBg, scale, new Vector2(0f, 0.5f));

        // ------------------------------------------------------------------ layout

        void LayoutGuideButton(float halfW, float halfH)
        {
            if (_guideBtnPanel == null) return;
            float w = 4.6f, h = 1.05f;
            float right = halfW - 0.28f, bottom = -halfH + 0.28f;
            float cx = right - w * 0.5f, cy = bottom + h * 0.5f;
            SetRect(_guideBtnPanel, cx, cy, w, h);
            _guideBtnIcon.transform.localPosition = new Vector3(right - w + 0.65f, cy, 0f);
            _guideBtnIcon.transform.localScale = Vector3.one * (1.15f + 0.06f * Mathf.Sin(_time * 3f));
            _guideBtnText.transform.localPosition = new Vector3(right - w + 1.3f, cy, 0f);
            _guideBtnRect = new Rect(right - w, bottom, w, h);
            if (PlayHitsOn) Hit(UiHit.GuideButton, 0, _guideBtnRect, 0);
        }

        void LayoutGuide(float halfW, float halfH)
        {
            if (!GuideVisible) return;
            float w = Mathf.Min(halfW * 2f - 0.8f, 28f);
            float h = Mathf.Min(halfH * 2f - 0.8f, 15f);
            SetRect(_guideCover, 0f, 0f, w, h);
            CloseAt(w * 0.5f, h * 0.5f, 2);

            float pageTop = h * 0.5f - 0.5f, pageBottom = -h * 0.5f + 1.25f;
            float pageH = pageTop - pageBottom;
            float spineW = 0.5f, margin = 0.5f;
            float pageW = (w - margin * 2f - spineW) * 0.5f;
            SetSolid(_guideSpine, 0f, (pageTop + pageBottom) * 0.5f, spineW, pageH + 0.2f);
            _guideHint.transform.localPosition = new Vector3(0f, -h * 0.5f + 0.38f, 0f);
            SetHint(_guideHint, "A/D / KLIK HALAMAN - BALIK    ESC / G - TUTUP", Gold);
            FitHint(_guideHint, w - 1f);

            // Susun alur halaman bila ukuran berubah (atau buku baru dibuka).
            if (!Mathf.Approximately(pageW, _guideFlowW) || !Mathf.Approximately(pageH, _guideFlowH))
            {
                _guideFlowW = pageW;
                _guideFlowH = pageH;
                BuildFlow(pageW - GuidePad * 2f, pageH - GuidePad * 2f - GuideHeaderH);
                int count = Mathf.Max(1, (_guideFlow.Count + 1) / 2);
                _guideSpreadIndex = Mathf.Clamp(_guideSpreadIndex, 0, count - 1);
                _guideShownSpread = -1;
            }

            for (int p = 0; p < 2; p++)
            {
                float pcx = p == 0 ? -spineW * 0.5f - pageW * 0.5f : spineW * 0.5f + pageW * 0.5f;
                SetSolid(_guidePage[p], pcx, (pageTop + pageBottom) * 0.5f, pageW, pageH);
                Hit(UiHit.GuidePage, p, RectAt(pcx, (pageTop + pageBottom) * 0.5f, pageW, pageH), 1, false);
            }
            if (_guideShownSpread == _guideSpreadIndex) return;
            _guideShownSpread = _guideSpreadIndex;

            int spreads = Mathf.Max(1, (_guideFlow.Count + 1) / 2);
            SetText(_guidePageNo, "HALAMAN " + (_guideSpreadIndex + 1) + " / " + spreads, Gold, 1, new Vector2(0.5f, 0.5f));
            _guidePageNo.transform.localPosition = new Vector3(0f, -h * 0.5f + 0.85f, 0f);

            for (int p = 0; p < 2; p++)
            {
                float pl = (p == 0 ? -spineW * 0.5f - pageW : spineW * 0.5f);
                int fi = _guideSpreadIndex * 2 + p;
                RenderFlowPage(p, fi < _guideFlow.Count ? _guideFlow[fi] : null, pl, pageW, pageTop);
            }
        }

        // ------------------------------------------------------------------ alur halaman

        /// <summary>Susun semua topik menjadi halaman; topik yang panjang bersambung ke halaman berikutnya.</summary>
        void BuildFlow(float contentW, float availH)
        {
            _guideFlow.Clear();
            foreach (var topic in _guideTopics)
            {
                var page = NewFlowPage(topic, false);
                float y = 0f;
                if (topic.Entries == null) { _guideFlow.Add(page); continue; }

                foreach (string entry in topic.Entries)
                {
                    var items = new List<FlowItem>();
                    float height = topic.Table ? TableEntry(entry, contentW, items) : PointEntry(entry, contentW, items);
                    if (y + height > availH && page.Items.Count > 0)
                    {
                        _guideFlow.Add(page);
                        page = NewFlowPage(topic, true);
                        y = 0f;
                    }
                    foreach (var it in items)
                    {
                        var moved = it;
                        moved.Y += y;
                        page.Items.Add(moved);
                    }
                    y += height;
                }
                _guideFlow.Add(page);
            }
        }

        static FlowPage NewFlowPage(GuidePage topic, bool continued) => new FlowPage
        {
            Title = Safe(topic.Title) + (continued ? " (LANJUTAN)" : ""),
            Icon = string.IsNullOrEmpty(topic.Icon) ? "lc_app_guide" : topic.Icon,
        };

        /// <summary>Baris tabel: tombol (kiri, warna) | fungsi (kanan, dibungkus di kolomnya).</summary>
        static float TableEntry(string entry, float contentW, List<FlowItem> items)
        {
            SplitEntry(entry, out string key, out string desc);
            float keyW = Mathf.Min(4.8f, contentW * 0.38f);
            int keyChars = Mathf.Max(3, Mathf.FloorToInt(keyW / GuideCharW) - 1);
            int descChars = Mathf.Max(5, Mathf.FloorToInt((contentW - keyW) / GuideCharW));
            string k = Safe(key);
            if (k.Length > keyChars) k = k.Substring(0, keyChars);
            string[] d = WrapAll(Safe(desc), descChars, 3);
            float y = GuideLine * 0.5f;
            items.Add(new FlowItem { Kind = FlowKind.Text, Text = k, Color = PageKey, X = 0f, Y = y });
            for (int i = 0; i < d.Length; i++)
                items.Add(new FlowItem { Kind = FlowKind.Text, Text = d[i], Color = PageInk, X = keyW, Y = y + i * GuideLine });
            float h = d.Length * GuideLine + 0.16f;
            items.Add(new FlowItem { Kind = FlowKind.Mark, Color = PageSep, X = contentW * 0.5f, Y = h - 0.08f, W = contentW, H = 0.03f });
            return h;
        }

        /// <summary>Poin: kotak + judul poin (warna), keterangan menjorok di bawahnya.</summary>
        static float PointEntry(string entry, float contentW, List<FlowItem> items)
        {
            SplitEntry(entry, out string key, out string desc);
            const float indent = 0.55f;
            int descChars = Mathf.Max(6, Mathf.FloorToInt((contentW - indent) / GuideCharW));
            float y = GuideLine * 0.5f;
            if (!string.IsNullOrEmpty(key))
            {
                items.Add(new FlowItem { Kind = FlowKind.Mark, Color = PageKey, X = 0.14f, Y = y, W = 0.22f, H = 0.22f });
                items.Add(new FlowItem { Kind = FlowKind.Text, Text = Safe(key), Color = PageKey, X = indent, Y = y });
                y += GuideLine;
            }
            foreach (string l in WrapAll(Safe(desc), descChars, 4))
            {
                items.Add(new FlowItem { Kind = FlowKind.Text, Text = l, Color = PageInk, X = indent, Y = y });
                y += GuideLine;
            }
            return y - GuideLine * 0.5f + GuideEntryGap;
        }

        static void SplitEntry(string entry, out string key, out string desc)
        {
            int bar = entry.IndexOf('|');
            key = bar >= 0 ? entry.Substring(0, bar).Trim() : "";
            desc = bar >= 0 ? entry.Substring(bar + 1).Trim() : entry.Trim();
        }

        void RenderFlowPage(int p, FlowPage page, float pl, float pageW, float pageTop)
        {
            bool has = page != null;
            _guideIcon[p].gameObject.SetActive(has);
            _guideTitle[p].gameObject.SetActive(has);
            _guideRule[p].gameObject.SetActive(has);
            int used = 0;
            if (has)
            {
                float contentL = pl + GuidePad, contentW = pageW - GuidePad * 2f;
                float ty = pageTop - GuidePad - 0.3f;
                _guideIcon[p].sprite = PixelArt.Get(page.Icon);
                _guideIcon[p].transform.localScale = Vector3.one * 1.3f;
                _guideIcon[p].transform.localPosition = new Vector3(contentL + 0.5f, ty, 0f);
                float titleRoom = contentW - 1.35f;
                bool big = page.Title.Length * PixelFont.CharW * 2f <= titleRoom;
                _guideTitle[p].sprite = PageSprite(page.Title, PageTitle, big ? 2 : 1);
                _guideTitle[p].transform.localScale = Vector3.one * (big ? 1f : GuideTextScale);
                _guideTitle[p].transform.localPosition = new Vector3(contentL + 1.35f, ty, 0f);
                SetSolid(_guideRule[p], contentL + contentW * 0.5f, ty - 0.75f, contentW, 0.07f);

                float top = pageTop - GuidePad - GuideHeaderH;
                foreach (var it in page.Items)
                {
                    if (used >= GuideTextMax) break;
                    var t = _guideTexts[p, used];
                    var m = _guideMarks[p, used];
                    if (it.Kind == FlowKind.Text)
                    {
                        m.gameObject.SetActive(false);
                        t.gameObject.SetActive(true);
                        t.sprite = PageSprite(it.Text, it.Color);
                        t.transform.localPosition = new Vector3(contentL + it.X, top - it.Y, 0f);
                    }
                    else
                    {
                        t.gameObject.SetActive(false);
                        m.gameObject.SetActive(true);
                        m.color = it.Color;
                        SetSolid(m, contentL + it.X, top - it.Y, it.W, it.H);
                    }
                    used++;
                }
            }
            for (int i = used; i < GuideTextMax; i++)
            {
                _guideTexts[p, i].gameObject.SetActive(false);
                _guideMarks[p, i].gameObject.SetActive(false);
            }
        }
    }
}
