using System.Collections.Generic;
using PixelAdventure.LifeSim.Pulang;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>Satu baris inventori (barang sama digabung dengan jumlahnya).</summary>
    public struct InvEntry
    {
        public string Name, Desc, Effect, Action, Icon;
        public int Count;
        public ItemKind Kind;
        public bool Usable;
    }

    /// <summary>
    /// Layar INVENTORI: tab kategori di atas, daftar barang di kiri,
    /// detail barang terpilih (ikon besar, keterangan, kegunaan, aksi) di kanan.
    /// </summary>
    public partial class LifeHud
    {
        const int InvRowsMax = 14;
        const int InvTabsMax = 5;
        const int InvDescLines = 5;
        const float InvRowH = 0.78f;

        static readonly Color InvFood = new Color(0.55f, 0.92f, 0.55f);
        static readonly Color InvGift = new Color(1f, 0.63f, 0.76f);
        static readonly Color InvGear = new Color(0.62f, 0.78f, 1f);
        static readonly Color InvQuest = new Color(1f, 0.84f, 0.40f);

        Transform _invGroup;
        SpriteRenderer _invPanel, _invTitle, _invHeader, _invListPanel, _invDetailPanel, _invHint, _invMessage;
        SpriteRenderer _invTabBar, _invRowHighlight, _invEmpty;
        SpriteRenderer _invBigIcon, _invName, _invKindTag, _invEffect, _invAction, _invMoreUp, _invMoreDown;
        readonly SpriteRenderer[] _invTabs = new SpriteRenderer[InvTabsMax];
        readonly SpriteRenderer[] _invRowIcons = new SpriteRenderer[InvRowsMax];
        readonly SpriteRenderer[] _invRowNames = new SpriteRenderer[InvRowsMax];
        readonly SpriteRenderer[] _invRowCounts = new SpriteRenderer[InvRowsMax];
        readonly SpriteRenderer[] _invDesc = new SpriteRenderer[InvDescLines];

        readonly List<InvEntry> _invEntries = new List<InvEntry>();
        int _invTabCount, _invTabSel, _invSel, _invFirst;
        string _invDescText = "";
        int _invDescChars = -1;
        int _invBuiltFirst = -1, _invBuiltChars = -1;   // teks baris hanya dibuat ulang bila berubah

        void BuildInventory()
        {
            _invGroup = Group("Inventory");
            _invPanel = Panel(_invGroup, "Panel", "lc_panel_gold", 4010);
            _invTitle = Text(_invGroup, "Title", "INVENTORI", 2, Gold, new Vector2(0f, 0.5f), 4013);
            _invHeader = Text(_invGroup, "Header", " ", 1, Ink, new Vector2(1f, 0.5f), 4013);
            _invTabBar = Solid(_invGroup, "TabBar", Gold, 4012);
            for (int i = 0; i < InvTabsMax; i++)
                _invTabs[i] = Text(_invGroup, "Tab" + i, " ", 1, Soft, new Vector2(0.5f, 0.5f), 4013);

            _invListPanel = Panel(_invGroup, "List", "lc_panel_soft", 4011);
            _invDetailPanel = Panel(_invGroup, "Detail", "lc_panel_soft", 4011);
            _invRowHighlight = Solid(_invGroup, "RowHighlight", new Color(1f, 0.84f, 0.40f, 0.22f), 4012);
            for (int i = 0; i < InvRowsMax; i++)
            {
                _invRowIcons[i] = Make(_invGroup, PixelArt.Get("lc_item_food"), "RowIcon" + i, 4013);
                _invRowIcons[i].transform.localScale = Vector3.one * 0.8f;
                _invRowNames[i] = Text(_invGroup, "RowName" + i, " ", 1, Ink, new Vector2(0f, 0.5f), 4013);
                _invRowCounts[i] = Text(_invGroup, "RowCount" + i, " ", 1, Soft, new Vector2(1f, 0.5f), 4013);
            }
            _invEmpty = Text(_invGroup, "Empty", "TIDAK ADA BARANG", 1, Soft, new Vector2(0.5f, 0.5f), 4013);
            _invMoreUp = Text(_invGroup, "MoreUp", "▲", 1, Soft, new Vector2(0.5f, 0.5f), 4013);
            _invMoreDown = Text(_invGroup, "MoreDown", "▼", 1, Soft, new Vector2(0.5f, 0.5f), 4013);

            _invBigIcon = Make(_invGroup, PixelArt.Get("lc_item_food"), "BigIcon", 4013);
            _invBigIcon.transform.localScale = Vector3.one * 2.4f;
            _invName = Text(_invGroup, "Name", " ", 2, Ink, new Vector2(0f, 0.5f), 4013);
            _invKindTag = Text(_invGroup, "Kind", " ", 1, Gold, new Vector2(0f, 0.5f), 4013);
            for (int i = 0; i < InvDescLines; i++)
                _invDesc[i] = Text(_invGroup, "Desc" + i, " ", 1, Ink, new Vector2(0f, 0.5f), 4013);
            _invEffect = Text(_invGroup, "Effect", " ", 1, Gold, new Vector2(0f, 0.5f), 4013);
            _invAction = Text(_invGroup, "Action", " ", 1, Soft, new Vector2(0f, 0.5f), 4013);

            _invMessage = Text(_invGroup, "Message", " ", 1, Pink, new Vector2(0.5f, 0.5f), 4013);
            _invHint = Text(_invGroup, "Hint", "A/D - KATEGORI   W/S - PILIH   E - PAKAI   ESC / I - KEMBALI", 1, Gold,
                new Vector2(0.5f, 0.5f), 4013);
            _invGroup.gameObject.SetActive(false);
        }

        public bool InventoryVisible => _invGroup != null && _invGroup.gameObject.activeSelf;

        static Color KindColor(ItemKind k) =>
            k == ItemKind.Makanan ? InvFood : k == ItemKind.Hadiah ? InvGift : k == ItemKind.Perlengkapan ? InvGear : InvQuest;

        static string KindIcon(ItemKind k) =>
            k == ItemKind.Makanan ? "lc_item_food" : k == ItemKind.Hadiah ? "lc_item_gift"
            : k == ItemKind.Perlengkapan ? "lc_item_gear" : "lc_item_quest";

        public void ShowInventory(string[] tabs, int tab, List<InvEntry> entries, int selected,
            string header, string message)
        {
            _invTabCount = Mathf.Min(tabs.Length, InvTabsMax);
            _invTabSel = tab;
            for (int i = 0; i < InvTabsMax; i++)
            {
                bool show = i < _invTabCount;
                _invTabs[i].gameObject.SetActive(show);
                if (show) SetText(_invTabs[i], tabs[i], i == tab ? Gold : Soft, 1, new Vector2(0.5f, 0.5f));
            }
            SetText(_invHeader, header, Ink, 1, new Vector2(1f, 0.5f));
            SetText(_invMessage, string.IsNullOrEmpty(message) ? " " : message, Pink, 1, new Vector2(0.5f, 0.5f));

            _invEntries.Clear();
            _invEntries.AddRange(entries);
            _invSel = Mathf.Clamp(selected, 0, Mathf.Max(0, entries.Count - 1));

            // Detail barang terpilih.
            bool any = entries.Count > 0;
            _invBigIcon.gameObject.SetActive(any);
            _invName.gameObject.SetActive(any);
            _invKindTag.gameObject.SetActive(any);
            _invEffect.gameObject.SetActive(any);
            _invAction.gameObject.SetActive(any);
            if (any)
            {
                var e = entries[_invSel];
                Color kc = KindColor(e.Kind);
                _invBigIcon.sprite = PixelArt.Get(string.IsNullOrEmpty(e.Icon) ? KindIcon(e.Kind) : e.Icon);
                SetText(_invName, e.Name.ToUpperInvariant() + (e.Count > 1 ? "  x" + e.Count : ""), Ink, 2,
                    new Vector2(0f, 0.5f));
                SetText(_invKindTag, "[" + e.Kind.ToString().ToUpperInvariant() + "]", kc, 1, new Vector2(0f, 0.5f));
                SetText(_invEffect, string.IsNullOrEmpty(e.Effect) ? " " : e.Effect.ToUpperInvariant(), Gold, 1,
                    new Vector2(0f, 0.5f));
                SetText(_invAction, string.IsNullOrEmpty(e.Action) ? " " : e.Action.ToUpperInvariant(),
                    e.Usable ? InvFood : Soft, 1, new Vector2(0f, 0.5f));
                _invDescText = (e.Desc ?? "").ToUpperInvariant();
            }
            else _invDescText = "";
            _invDescChars = -1;   // keterangan dibungkus ulang saat layout
            _invBuiltFirst = -1;  // teks baris dibuat ulang saat layout

            SetDim(0.78f);
            _invGroup.gameObject.SetActive(true);
            HidePrompt();
        }

        public void HideInventory()
        {
            if (_invGroup != null) _invGroup.gameObject.SetActive(false);
            SetDim(0f);
        }

        void LayoutInventory(float halfW, float halfH)
        {
            if (!InventoryVisible) return;
            float w = Mathf.Min(halfW * 2f - 0.8f, 28f);
            float h = halfH * 2f - 0.8f;
            float top = h * 0.5f, left = -w * 0.5f;
            SetRect(_invPanel, 0f, 0f, w, h);

            _invTitle.transform.localPosition = new Vector3(left + 0.7f, top - 0.75f, 0f);
            _invHeader.transform.localPosition = new Vector3(-left - 0.7f, top - 0.75f, 0f);

            // Tab kategori.
            float tabY = top - 1.75f;
            float tabW = (w - 1.4f) / Mathf.Max(1, _invTabCount);
            for (int i = 0; i < _invTabCount; i++)
            {
                float tx = left + 0.7f + tabW * (i + 0.5f);
                _invTabs[i].transform.localPosition = new Vector3(tx, tabY, 0f);
                if (i == _invTabSel) SetSolid(_invTabBar, tx, tabY - 0.38f, tabW * 0.7f, 0.08f);
            }

            // Panel daftar & detail.
            float bodyTop = top - 2.45f;
            float bodyBottom = -top + 1.55f;
            float bodyH = bodyTop - bodyBottom;
            float listW = (w - 1.4f) * 0.48f;
            float detailW = (w - 1.4f) - listW - 0.3f;
            float listCx = left + 0.7f + listW * 0.5f;
            float detailL = left + 0.7f + listW + 0.3f;
            SetRect(_invListPanel, listCx, bodyBottom + bodyH * 0.5f, listW, bodyH);
            SetRect(_invDetailPanel, detailL + detailW * 0.5f, bodyBottom + bodyH * 0.5f, detailW, bodyH);

            // Baris daftar dengan jendela gulir mengikuti pilihan.
            int visibleRows = Mathf.Clamp(Mathf.FloorToInt((bodyH - 0.6f) / InvRowH), 1, InvRowsMax);
            if (_invSel < _invFirst) _invFirst = _invSel;
            if (_invSel >= _invFirst + visibleRows) _invFirst = _invSel - visibleRows + 1;
            _invFirst = Mathf.Clamp(_invFirst, 0, Mathf.Max(0, _invEntries.Count - visibleRows));

            float rowTop = bodyTop - 0.55f;
            float rowL = listCx - listW * 0.5f + 0.35f;
            float rowR = listCx + listW * 0.5f - 0.35f;
            int nameChars = Mathf.Max(6, Mathf.FloorToInt((rowR - rowL - 1.9f) / (6f / 16f)));
            bool rebuildText = _invBuiltFirst != _invFirst || _invBuiltChars != nameChars;
            _invBuiltFirst = _invFirst;
            _invBuiltChars = nameChars;
            for (int i = 0; i < InvRowsMax; i++)
            {
                int idx = _invFirst + i;
                bool show = i < visibleRows && idx < _invEntries.Count;
                _invRowIcons[i].gameObject.SetActive(show);
                _invRowNames[i].gameObject.SetActive(show);
                _invRowCounts[i].gameObject.SetActive(show);
                if (!show) continue;
                var e = _invEntries[idx];
                float y = rowTop - i * InvRowH;
                bool sel = idx == _invSel;
                if (rebuildText)
                {
                    _invRowIcons[i].sprite = PixelArt.Get(string.IsNullOrEmpty(e.Icon) ? KindIcon(e.Kind) : e.Icon);
                    string name = e.Name.ToUpperInvariant();
                    if (name.Length > nameChars) name = name.Substring(0, nameChars - 2) + "..";
                    SetText(_invRowNames[i], (sel ? "> " : "") + name,
                        sel ? Gold : KindColor(e.Kind) * 0.35f + Ink * 0.65f, 1, new Vector2(0f, 0.5f));
                    SetText(_invRowCounts[i], "x" + e.Count, Soft, 1, new Vector2(1f, 0.5f));
                }
                _invRowIcons[i].transform.localPosition = new Vector3(rowL + 0.4f, y, 0f);
                _invRowNames[i].transform.localPosition = new Vector3(rowL + 0.95f, y, 0f);
                _invRowCounts[i].transform.localPosition = new Vector3(rowR, y, 0f);
                if (sel) SetSolid(_invRowHighlight, listCx, y, listW - 0.3f, InvRowH - 0.08f);
            }
            _invRowHighlight.gameObject.SetActive(_invEntries.Count > 0);
            _invEmpty.gameObject.SetActive(_invEntries.Count == 0);
            _invEmpty.transform.localPosition = new Vector3(listCx, rowTop - 0.2f, 0f);
            _invMoreUp.gameObject.SetActive(_invFirst > 0);
            _invMoreDown.gameObject.SetActive(_invFirst + visibleRows < _invEntries.Count);
            _invMoreUp.transform.localPosition = new Vector3(listCx, bodyTop - 0.18f, 0f);
            _invMoreDown.transform.localPosition = new Vector3(listCx, bodyBottom + 0.2f, 0f);

            // Detail.
            float dl = detailL + 0.45f;
            float dTop = bodyTop - 0.35f;
            _invBigIcon.transform.localPosition = new Vector3(dl + 1.2f, dTop - 1.3f, 0f);
            _invName.transform.localPosition = new Vector3(dl + 2.7f, dTop - 0.95f, 0f);
            _invKindTag.transform.localPosition = new Vector3(dl + 2.7f, dTop - 1.8f, 0f);

            int descChars = Mathf.Max(10, Mathf.FloorToInt((detailW - 0.9f) / (6f / 16f)));
            if (descChars != _invDescChars)
            {
                _invDescChars = descChars;
                string[] lines = WrapAll(_invDescText, descChars, InvDescLines);
                for (int i = 0; i < InvDescLines; i++)
                {
                    string l = i < lines.Length && _invDescText.Length > 0 ? lines[i] : " ";
                    SetText(_invDesc[i], l, Ink, 1, new Vector2(0f, 0.5f));
                }
            }
            for (int i = 0; i < InvDescLines; i++)
                _invDesc[i].transform.localPosition = new Vector3(dl, dTop - 3.2f - i * 0.5f, 0f);
            _invEffect.transform.localPosition = new Vector3(dl, dTop - 3.4f - InvDescLines * 0.5f, 0f);
            _invAction.transform.localPosition = new Vector3(dl, bodyBottom + 0.55f, 0f);

            _invMessage.transform.localPosition = new Vector3(0f, -top + 1.05f, 0f);
            _invHint.transform.localPosition = new Vector3(0f, -top + 0.45f, 0f);
        }
    }
}
