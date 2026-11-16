using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>Satu baris pengaturan. Count &gt; 1 = jumlah pilihan (titik penanda di bawah nilai).</summary>
    public struct SettingRow
    {
        public string Section, Label, Value, Help;
        public int Count, Index;
    }

    /// <summary>
    /// Layar PENGATURAN GRAFIK (aplikasi GRAFIK di tablet / PENGATURAN di menu utama), tersusun rapi:
    /// - judul + perangkat (mis. "PC 1920X1080");
    /// - baris dikelompokkan per bagian (LAYAR, TAMPILAN, PERFORMA &amp; SUARA): label di kiri,
    ///   nilai di kanan dalam kotak "◀ NILAI ▶" + titik penanda pilihan;
    /// - kotak keterangan untuk baris yang dipilih;
    /// - dua tombol: SETEL OTOMATIS dan KEMBALI.
    /// Indeks pilihan: 0..baris-1 = baris, baris = SETEL OTOMATIS, baris+1 = KEMBALI.
    /// </summary>
    public partial class LifeHud
    {
        static readonly Color SetCard = new Color(0.10f, 0.08f, 0.14f, 1f);
        static readonly Color SetPill = new Color(0.06f, 0.05f, 0.09f, 1f);
        static readonly Color SetSelBg = new Color(1f, 0.84f, 0.40f, 0.16f);
        static readonly Color SetDotOff = new Color(0.30f, 0.27f, 0.38f, 1f);
        static readonly Color SetButton = new Color(0.20f, 0.16f, 0.28f, 1f);

        public const string SettingsAutoHelp = "Pilih nilai terbaik untuk perangkat ini secara otomatis.";
        public const string SettingsBackHelp = "Semua perubahan langsung tersimpan. Tutup pengaturan.";

        Transform _setGroup;
        UiPool _sui;
        readonly List<SettingRow> _setRows = new List<SettingRow>();
        readonly List<Rect> _setRowRects = new List<Rect>();
        readonly List<Rect> _setLeftRects = new List<Rect>();
        Rect _setAutoRect, _setBackRect;
        int _setSel;
        string _setDevice = "";

        void BuildSettings()
        {
            _setGroup = Group("Settings");
            _sui = new UiPool(_setGroup, 4010);
            _setGroup.gameObject.SetActive(false);
        }

        public bool SettingsVisible => _setGroup != null && _setGroup.gameObject.activeSelf;

        public void ShowSettings(List<SettingRow> rows, int selected, string device)
        {
            _setRows.Clear();
            if (rows != null) _setRows.AddRange(rows);
            _setSel = selected;
            _setDevice = device ?? "";
            SetDim(0.8f);
            _setGroup.gameObject.SetActive(true);
            HidePrompt();
        }

        public void HideSettings()
        {
            if (_setGroup != null) _setGroup.gameObject.SetActive(false);
            SetDim(0f);
        }

        /// <summary>
        /// Baris / tombol yang diketuk pada frame ini (-1 bila tidak ada).
        /// <paramref name="step"/> = -1 bila yang diketuk panah kiri "◀", selain itu +1.
        /// </summary>
        public int SettingsTapped(out int step)
        {
            step = 1;
            if (!SettingsVisible || _cam == null || !LifeInput.PointerTapped) return -1;
            Vector2 p = PointerToHud();
            for (int i = 0; i < _setRowRects.Count; i++)
            {
                if (!_setRowRects[i].Contains(p)) continue;
                if (_setLeftRects[i].Contains(p)) step = -1;
                return i;
            }
            if (_setAutoRect.Contains(p)) return _setRows.Count;
            if (_setBackRect.Contains(p)) return _setRows.Count + 1;
            return -1;
        }

        void LayoutSettings(float halfW, float halfH)
        {
            if (!SettingsVisible) return;
            var ui = _sui;
            ui.Begin();
            _setRowRects.Clear();
            _setLeftRects.Clear();

            // Hitung tinggi isi lebih dulu agar panel pas (dan rapat bila layar pendek).
            int sections = 0;
            string last = null;
            foreach (var r in _setRows) if (r.Section != last) { sections++; last = r.Section; }
            float w = Mathf.Min(halfW * 2f - 1f, 21f);
            float innerW = w - 1.2f;

            // Kotak keterangan setinggi keterangan terpanjang (maks. 3 baris), agar tidak ada kalimat
            // yang terpotong dan panel tidak berubah tinggi saat pilihan berpindah.
            int helpChars = CharsFor(innerW - 0.7f);
            int helpLines = Mathf.Max(Wrap(Safe(SettingsAutoHelp), helpChars).Length, Wrap(Safe(SettingsBackHelp), helpChars).Length);
            foreach (var r in _setRows) helpLines = Mathf.Max(helpLines, Wrap(Safe(r.Help), helpChars).Length);
            helpLines = Mathf.Clamp(helpLines, 1, 3);

            const float headH = 1.35f, sectionH = 0.62f, sectionGap = 0.22f, buttonsH = 1.25f, hintH = 0.75f;
            float helpH = 0.45f + helpLines * UiLine;   // 2 baris = 1,45 (seperti sebelumnya)
            float rowH = 0.86f;
            float fixedH = headH + sections * sectionH + (sections - 1) * sectionGap + helpH + buttonsH + hintH;
            float maxH = halfH * 2f - 0.8f;
            if (fixedH + _setRows.Count * rowH > maxH)
                rowH = Mathf.Max(0.6f, (maxH - fixedH) / Mathf.Max(1, _setRows.Count));
            float h = Mathf.Min(maxH, fixedH + _setRows.Count * rowH);
            float top = h * 0.5f, left = -w * 0.5f;
            float innerL = left + 0.6f, innerR = innerL + innerW;

            ui.Panel("lc_panel_gold", 0f, 0f, w, h, 0);
            CloseAt(w * 0.5f, top, 3);
            ui.Text("PENGATURAN", innerL, top - 0.72f, Gold, 2);
            ui.Text(Safe(_setDevice), innerR, top - 0.72f, Soft, 1, 1f);

            // ---------------------------------------------------------- baris
            float pillW = Mathf.Min(7.8f, innerW * 0.5f);
            float pillL = innerR - 0.25f - pillW;
            int valueChars = CharsFor(pillW - 1.3f);
            int labelChars = CharsFor(pillL - innerL - 0.9f);
            float y = top - headH;
            last = null;
            for (int i = 0; i < _setRows.Count; i++)
            {
                var r = _setRows[i];
                if (r.Section != last)
                {
                    if (last != null) y -= sectionGap;
                    last = r.Section;
                    ui.Text(Safe(r.Section), innerL, y - 0.26f, Gold);
                    ui.Solid(innerL + innerW * 0.5f, y - 0.55f, innerW, 0.05f, new Color(1f, 0.84f, 0.40f, 0.35f), 2);
                    y -= sectionH;
                }

                float cy = y - rowH * 0.5f;
                bool sel = i == _setSel;
                var rowRect = new Rect(innerL, cy - rowH * 0.5f, innerW, rowH);
                _setRowRects.Add(rowRect);
                Hit(UiHit.SettingsRow, i, rowRect, 3);
                _setLeftRects.Add(new Rect(pillL - 0.2f, rowRect.y, 1.1f, rowH));
                if (sel)
                {
                    ui.Solid(innerL + innerW * 0.5f, cy, innerW, rowH - 0.08f, SetSelBg, 1);
                    ui.Solid(innerL + 0.05f, cy, 0.1f, rowH - 0.08f, Gold, 2);
                }

                string label = Safe(r.Label);
                if (label.Length > labelChars) label = label.Substring(0, labelChars - 1) + ".";
                ui.Text(label, innerL + 0.45f, cy, sel ? Gold : Ink);

                // Kotak nilai: ◀ NILAI ▶ dan titik penanda pilihan.
                float pcx = pillL + pillW * 0.5f;
                float pillH = rowH - 0.2f;
                ui.Solid(pcx, cy, pillW, pillH, sel ? Gold : SetDotOff, 2);
                ui.Solid(pcx, cy, pillW - 0.1f, pillH - 0.1f, SetPill, 3);
                Color arrowC = sel ? Gold : Soft;
                ui.Text("◀", pillL + 0.3f, cy, arrowC, 1, 0.5f);
                ui.Text("▶", pillL + pillW - 0.3f, cy, arrowC, 1, 0.5f);

                bool dots = r.Count > 1 && pillH >= 0.62f;
                string value = Safe(r.Value);
                if (value.Length > valueChars) value = value.Substring(0, valueChars - 1) + ".";
                ui.Text(value, pcx, cy + (dots ? 0.1f : 0f), sel ? Ink : Soft, 1, 0.5f);
                if (dots)
                {
                    const float dot = 0.11f, dotGap = 0.2f;
                    float dx0 = pcx - (r.Count - 1) * dotGap * 0.5f;
                    for (int d = 0; d < r.Count; d++)
                        ui.Solid(dx0 + d * dotGap, cy - pillH * 0.5f + 0.15f, dot, dot,
                            d == r.Index ? Gold : SetDotOff, 4);
                }
                y -= rowH;
            }

            // ---------------------------------------------------------- keterangan
            y -= 0.2f;
            string help = _setSel < _setRows.Count && _setSel >= 0 ? _setRows[_setSel].Help
                : _setSel == _setRows.Count ? SettingsAutoHelp : SettingsBackHelp;
            float helpCardH = helpH - 0.25f;
            ui.Solid(innerL + innerW * 0.5f, y - helpCardH * 0.5f, innerW, helpCardH, SetCard, 1);
            string[] lines = Wrap(Safe(help), helpChars);
            for (int l = 0; l < lines.Length && l < helpLines; l++)
                ui.Text(lines[l], innerL + 0.35f, y - 0.38f - l * UiLine, Soft);
            y -= helpH;

            // ---------------------------------------------------------- tombol
            float bw = (innerW - 0.4f) * 0.5f, bh = 0.9f;
            float by = y - bh * 0.5f;
            _setAutoRect = SettingsButton(ui, innerL + bw * 0.5f, by, bw, bh, "SETEL OTOMATIS", _setSel == _setRows.Count);
            _setBackRect = SettingsButton(ui, innerR - bw * 0.5f, by, bw, bh, "KEMBALI", _setSel == _setRows.Count + 1);
            Hit(UiHit.SettingsRow, _setRows.Count, _setAutoRect, 3);
            Hit(UiHit.SettingsRow, _setRows.Count + 1, _setBackRect, 3);

            ui.Text(CompactHint("W/S PILIH   A/D / KLIK UBAH   ESC KEMBALI", CharsFor(innerW)), 0f, -top + 0.42f, Gold, 1, 0.5f);
            ui.End();
        }

        static Rect SettingsButton(UiPool ui, float cx, float cy, float w, float h, string label, bool selected)
        {
            ui.Solid(cx, cy, w, h, selected ? Gold : SetDotOff, 2);
            ui.Solid(cx, cy, w - 0.12f, h - 0.12f, selected ? new Color(0.36f, 0.26f, 0.12f, 1f) : SetButton, 3);
            ui.Text(label, cx, cy, selected ? Gold : Ink, 1, 0.5f);
            return new Rect(cx - w * 0.5f, cy - h * 0.5f, w, h);
        }
    }
}
