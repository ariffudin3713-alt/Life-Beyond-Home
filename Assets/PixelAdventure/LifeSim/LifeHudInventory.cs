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
    /// Layar INVENTORI (panel emas):
    /// - kepala: judul + uang & energi;
    /// - tab kategori berbentuk "pil" yang lebarnya dibagi rata;
    /// - kiri: daftar barang (ikon, nama, jumlah, garis warna kategori) dengan gulir otomatis;
    /// - kanan: ikon besar berbingkai, nama, label kategori, keterangan, kegunaan, dan tombol aksi.
    /// Tata letak dihitung ulang tiap frame sehingga teks selalu muat di layar mana pun.
    /// </summary>
    public partial class LifeHud
    {
        static readonly Color InvFood = new Color(0.55f, 0.92f, 0.55f);
        static readonly Color InvGift = new Color(1f, 0.63f, 0.76f);
        static readonly Color InvGear = new Color(0.62f, 0.78f, 1f);
        static readonly Color InvQuest = new Color(1f, 0.84f, 0.40f);
        static readonly Color InvRide = new Color(1f, 0.66f, 0.38f);
        static readonly Color InvCloth = new Color(0.78f, 0.62f, 1f);

        static readonly Color InvTabOn = new Color(0.52f, 0.38f, 0.14f, 1f);
        static readonly Color InvTabOff = new Color(0.16f, 0.12f, 0.20f, 1f);
        static readonly Color InvRowSel = new Color(1f, 0.84f, 0.40f, 0.20f);
        static readonly Color InvFrame = new Color(0.10f, 0.08f, 0.14f, 1f);
        static readonly Color InvBtnOn = new Color(0.20f, 0.42f, 0.26f, 1f);
        static readonly Color InvBtnOff = new Color(0.20f, 0.17f, 0.24f, 1f);

        const float InvRowH = 0.82f;

        Transform _invGroup;
        UiPool _iui;
        readonly List<InvEntry> _invEntries = new List<InvEntry>();
        string[] _invTabNames = new string[0];
        int _invTabSel, _invSel, _invFirst;
        string _invHeaderText = "", _invMessageText = "";

        void BuildInventory()
        {
            _invGroup = Group("Inventory");
            _iui = new UiPool(_invGroup, 4010);
            _invGroup.gameObject.SetActive(false);
        }

        public bool InventoryVisible => _invGroup != null && _invGroup.gameObject.activeSelf;

        static Color KindColor(ItemKind k) =>
            k == ItemKind.Makanan ? InvFood : k == ItemKind.Hadiah ? InvGift : k == ItemKind.Perlengkapan ? InvGear
            : k == ItemKind.Kendaraan ? InvRide : k == ItemKind.Pakaian ? InvCloth : InvQuest;

        static string KindIcon(ItemKind k) =>
            k == ItemKind.Makanan ? "lc_item_food" : k == ItemKind.Hadiah ? "lc_item_gift"
            : k == ItemKind.Perlengkapan ? "lc_item_gear" : k == ItemKind.Kendaraan ? "lc_item_vehicle_motor" : "lc_item_quest";

        static string KindName(ItemKind k) =>
            k == ItemKind.Misi ? "BARANG MISI - PENTING" : k == ItemKind.Perlengkapan ? "PERLENGKAPAN" : k.ToString().ToUpperInvariant();

        public void ShowInventory(string[] tabs, int tab, List<InvEntry> entries, int selected,
            string header, string message)
        {
            _invTabNames = tabs ?? new string[0];
            _invTabSel = tab;
            _invHeaderText = header ?? "";
            _invMessageText = message ?? "";
            _invEntries.Clear();
            _invEntries.AddRange(entries);
            _invSel = Mathf.Clamp(selected, 0, Mathf.Max(0, entries.Count - 1));
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
            var ui = _iui;
            ui.Begin();

            float w = Mathf.Min(halfW * 2f - 0.8f, 28f);
            float h = Mathf.Min(halfH * 2f - 0.8f, 15.6f);
            float top = h * 0.5f, left = -w * 0.5f;
            ui.Panel("lc_panel_gold", 0f, 0f, w, h, 0);
            CloseAt(w * 0.5f, h * 0.5f);

            // ---- kepala
            ui.Text("INVENTORI", left + 0.7f, top - 0.75f, Gold, 2);
            ui.Text(Fit(_invHeaderText, CharsFor(w * 0.5f - 1.4f)), -left - 1.1f, top - 0.75f, Ink, 1, 1f);

            // ---- tab kategori
            int n = Mathf.Max(1, _invTabNames.Length);
            float tabY = top - 1.7f;
            float tabW = (w - 1.4f) / n;
            for (int i = 0; i < _invTabNames.Length; i++)
            {
                bool sel = i == _invTabSel;
                float tx = left + 0.7f + tabW * (i + 0.5f);
                ui.Solid(tx, tabY, tabW - 0.16f, 0.62f, sel ? InvTabOn : InvTabOff, 2);
                if (sel) ui.Solid(tx, tabY - 0.31f, tabW - 0.16f, 0.07f, Gold, 3);
                ui.Text(Fit(_invTabNames[i], CharsFor(tabW - 0.4f)), tx, tabY, sel ? Gold : Soft, 1, 0.5f);
                Hit(UiHit.InvTab, i, RectAt(tx, tabY, tabW - 0.16f, 0.62f));
            }

            // ---- badan
            float bodyTop = top - 2.3f, bodyBottom = -top + 1.35f, bodyH = bodyTop - bodyBottom;
            float inner = w - 1.4f;
            float listW = inner * 0.46f, detailW = inner - listW - 0.3f;
            float listCx = left + 0.7f + listW * 0.5f;
            float detailL = left + 0.7f + listW + 0.3f;
            float midY = bodyBottom + bodyH * 0.5f;
            ui.Panel("lc_panel_soft", listCx, midY, listW, bodyH, 1);
            ui.Panel("lc_panel_soft", detailL + detailW * 0.5f, midY, detailW, bodyH, 1);

            LayoutInvList(ui, listCx, listW, bodyTop, bodyBottom);
            LayoutInvDetail(ui, detailL + 0.5f, detailW - 1.0f, bodyTop, bodyBottom);

            // ---- pesan & petunjuk
            if (!string.IsNullOrEmpty(_invMessageText))
                ui.Text(Fit(_invMessageText, CharsFor(w - 1.4f)), 0f, -top + 0.95f, Pink, 1, 0.5f);
            ui.Text(CompactHint("A/D - KATEGORI   W/S - PILIH   E / KLIK - PAKAI   ESC / I - KEMBALI", CharsFor(w - 1.4f)),
                0f, -top + 0.45f, Gold, 1, 0.5f);
            ui.End();
        }

        void LayoutInvList(UiPool ui, float cx, float lw, float bodyTop, float bodyBottom)
        {
            float l = cx - lw * 0.5f + 0.3f, r = cx + lw * 0.5f - 0.3f;
            if (_invEntries.Count == 0)
            {
                ui.Text("TIDAK ADA BARANG", cx, bodyTop - 0.8f, Soft, 1, 0.5f);
                ui.Text("DI KATEGORI INI", cx, bodyTop - 1.3f, Soft, 1, 0.5f);
                return;
            }

            float rowsTop = bodyTop - 0.6f;
            int visible = Mathf.Max(1, Mathf.FloorToInt((rowsTop - bodyBottom - 0.2f) / InvRowH) + 1);
            if (_invSel < _invFirst) _invFirst = _invSel;
            if (_invSel >= _invFirst + visible) _invFirst = _invSel - visible + 1;
            _invFirst = Mathf.Clamp(_invFirst, 0, Mathf.Max(0, _invEntries.Count - visible));

            int nameChars = CharsFor(r - l - 2.2f);
            for (int i = 0; i < visible; i++)
            {
                int idx = _invFirst + i;
                if (idx >= _invEntries.Count) break;
                var e = _invEntries[idx];
                float y = rowsTop - i * InvRowH;
                bool sel = idx == _invSel;
                Color kc = KindColor(e.Kind);
                if (sel) ui.Solid(cx, y, lw - 0.3f, InvRowH - 0.08f, InvRowSel, 2);
                ui.Solid(l + 0.06f, y, 0.12f, InvRowH - 0.22f, kc, 3);                // garis warna kategori
                ui.Sprite(PixelArt.Get(string.IsNullOrEmpty(e.Icon) ? KindIcon(e.Kind) : e.Icon), l + 0.65f, y, 0.72f);
                string name = (e.Kind == ItemKind.Misi ? "! " : "") + e.Name;
                ui.Text(Fit(name, nameChars), l + 1.25f, y, sel ? Gold : Ink);
                ui.Text("X" + e.Count, r, y, Soft, 1, 1f);
                Hit(UiHit.InvRow, idx, RectAt(cx, y, lw - 0.3f, InvRowH - 0.04f));
            }
            if (_invFirst > 0) ui.Text("▲", cx, bodyTop - 0.18f, Soft, 1, 0.5f);
            if (_invFirst + visible < _invEntries.Count) ui.Text("▼", cx, bodyBottom + 0.2f, Soft, 1, 0.5f);
        }

        void LayoutInvDetail(UiPool ui, float x0, float dw, float bodyTop, float bodyBottom)
        {
            if (_invEntries.Count == 0) return;
            var e = _invEntries[_invSel];
            Color kc = KindColor(e.Kind);

            // ikon besar dalam bingkai
            float iconC = bodyTop - 1.5f;
            ui.Solid(x0 + 1.2f, iconC, 2.4f, 2.4f, InvFrame, 2);
            ui.Solid(x0 + 1.2f, iconC - 1.16f, 2.4f, 0.08f, kc, 3);
            ui.Sprite(PixelArt.Get(string.IsNullOrEmpty(e.Icon) ? KindIcon(e.Kind) : e.Icon), x0 + 1.2f, iconC, 2.0f, 4);

            // nama & kategori
            float nx = x0 + 2.8f, nw = dw - 2.8f;
            string name = Safe(e.Name) + (e.Count > 1 ? "  X" + e.Count : "");
            float ny = bodyTop - 0.85f;
            if (name.Length * UiCharW * 2f <= nw) { ui.Text(name, nx, ny, Ink, 2); ny -= 0.9f; }
            else
            {
                foreach (string l in WrapAll(name, CharsFor(nw), 2)) { ui.Text(l, nx, ny, Ink); ny -= UiLine; }
                ny -= 0.15f;
            }
            ui.Text(Fit("[" + KindName(e.Kind) + "]", CharsFor(nw)), nx, ny, kc);

            float y = bodyTop - 3.05f;
            ui.Solid(x0 + dw * 0.5f, y, dw, 0.06f, JRule, 2);

            // bawah: tombol aksi + kegunaan (dipesan dulu supaya keterangan tidak menimpanya)
            float btnY = bodyBottom + 0.55f;
            string[] effect = WrapAll(Safe(e.Effect), CharsFor(dw), 2);
            bool hasEffect = !string.IsNullOrEmpty(e.Effect) && e.Effect.Trim().Length > 0;
            float effTop = btnY + 0.75f + (hasEffect ? effect.Length * UiLine + 0.45f : 0f);

            y -= 0.45f;
            ui.Text("KETERANGAN", x0, y, Soft);
            y -= UiLine + 0.05f;
            int descMax = Mathf.Max(1, Mathf.FloorToInt((y - effTop) / UiLine) + 1);
            foreach (string l in WrapAll(Safe(e.Desc), CharsFor(dw), descMax)) { ui.Text(l, x0, y, Ink); y -= UiLine; }

            if (hasEffect)
            {
                float ey = effTop - 0.1f;
                ui.Text("KEGUNAAN", x0, ey, Soft);
                ey -= UiLine;
                foreach (string l in effect) { ui.Text(l, x0, ey, Gold); ey -= UiLine; }
            }

            bool hasAction = !string.IsNullOrEmpty(e.Action) && e.Action.Trim().Length > 0;
            if (hasAction)
            {
                ui.Solid(x0 + dw * 0.5f, btnY, dw, 0.66f, e.Usable ? InvBtnOn : InvBtnOff, 2);
                ui.Text(Fit(e.Action, CharsFor(dw - 0.4f)), x0 + dw * 0.5f, btnY, e.Usable ? Ink : Soft, 1, 0.5f);
            }
        }
    }
}
