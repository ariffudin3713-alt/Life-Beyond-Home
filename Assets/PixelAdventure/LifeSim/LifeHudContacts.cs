using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>Satu baris daftar kontak.</summary>
    public struct ContactRow
    {
        public string Name, Role, Status, Portrait;
    }

    /// <summary>Satu gelembung obrolan.</summary>
    public struct ChatLine
    {
        public bool Mine;
        public string Text;
    }

    /// <summary>Satu pilihan di jendela pilihan kontak (telepon / pesan).</summary>
    public struct PickOption
    {
        public string Label, Sub;
        public bool Enabled;
    }

    /// <summary>Jendela pilihan di atas layar kontak: judul, potret, catatan, dan daftar pilihan bernomor.</summary>
    public class ContactPicker
    {
        public string Title = "", Note = "", Portrait;
        public readonly List<PickOption> Options = new List<PickOption>();
        public int Selected;
    }

    /// <summary>Isi panel kanan layar kontak (sama untuk tab PROFIL, TELEPON, PESAN; isinya berbeda).</summary>
    public class ContactDetail
    {
        public string Name = "", Role = "", Relation = "", Portrait;
        public bool HasBond;
        public float Bond;
        public string BondLabel = "";
        public readonly List<JournalStat> Rows = new List<JournalStat>();
        public string AboutTitle = "", About = "";
        public readonly List<ChatLine> Chat = new List<ChatLine>();
        public bool ShowChat;
        public string Action = "";
        public bool ActionEnabled;
    }

    /// <summary>
    /// Layar KONTAK di tablet (tab PROFIL / TELEPON / PESAN), bergaya sama dengan inventori:
    /// kiri daftar orang yang sudah dikenal (potret, nama, peran, status), kanan kartu detail
    /// berpotret besar, label hubungan, bilah kedekatan, lalu isi tab dan tombol aksi di bawah.
    /// </summary>
    public partial class LifeHud
    {
        static readonly Color ChatMine = new Color(0.22f, 0.40f, 0.28f, 1f);
        static readonly Color ChatTheirs = new Color(0.24f, 0.21f, 0.33f, 1f);
        const float ContactRowH = 1.4f;     // tinggi baris daftar kontak
        const float CLine = 0.62f;           // jarak baris teks di layar kontak (huruf + bingkai 0,56 + celah)
        const float CRowGap = 0.78f;         // jarak antar baris keterangan (label | isi)

        Transform _contactGroup;
        UiPool _cui;
        string[] _contactTabs = new string[0];
        int _contactTab, _contactSel, _contactFirst;
        readonly List<ContactRow> _contactRows = new List<ContactRow>();
        ContactDetail _contactDetail = new ContactDetail();
        string _contactHeader = "", _contactMessage = "", _contactHint = "";

        void BuildContacts()
        {
            _contactGroup = Group("Contacts");
            _cui = new UiPool(_contactGroup, 4010);
            _contactGroup.gameObject.SetActive(false);
        }

        public bool ContactsVisible => _contactGroup != null && _contactGroup.gameObject.activeSelf;

        public void ShowContacts(string[] tabs, int tab, List<ContactRow> rows, int selected, ContactDetail detail,
            string header, string message, string hint, ContactPicker picker = null)
        {
            _contactPicker = picker;
            _contactTabs = tabs ?? new string[0];
            _contactTab = tab;
            _contactRows.Clear();
            _contactRows.AddRange(rows);
            _contactSel = Mathf.Clamp(selected, 0, Mathf.Max(0, rows.Count - 1));
            _contactDetail = detail ?? new ContactDetail();
            _contactHeader = header ?? "";
            _contactMessage = message ?? "";
            _contactHint = hint ?? "";
            SetDim(0.78f);
            _contactGroup.gameObject.SetActive(true);
            HidePrompt();
        }

        public void HideContacts()
        {
            if (_contactGroup != null) _contactGroup.gameObject.SetActive(false);
            SetDim(0f);
        }

        void LayoutContacts(float halfW, float halfH)
        {
            if (!ContactsVisible) return;
            var ui = _cui;
            ui.Begin();

            float w = Mathf.Min(halfW * 2f - 0.8f, 28f);
            float h = Mathf.Min(halfH * 2f - 0.8f, 15.6f);
            float top = h * 0.5f, left = -w * 0.5f;
            ui.Panel("lc_panel_gold", 0f, 0f, w, h, 0);
            CloseAt(w * 0.5f, h * 0.5f);

            ui.Text("KONTAK", left + 0.8f, top - 0.8f, Gold, 2);
            ui.Text(Fit(_contactHeader, CharsFor(w * 0.55f - 1.4f)), -left - 1.2f, top - 0.8f, Soft, 1, 1f);

            // tab
            int n = Mathf.Max(1, _contactTabs.Length);
            float tabY = top - 1.85f, tabW = (w - 1.6f) / n;
            for (int i = 0; i < _contactTabs.Length; i++)
            {
                bool sel = i == _contactTab;
                float tx = left + 0.8f + tabW * (i + 0.5f);
                ui.Solid(tx, tabY, tabW - 0.24f, 0.7f, sel ? InvTabOn : InvTabOff, 2);
                if (sel) ui.Solid(tx, tabY - 0.35f, tabW - 0.24f, 0.08f, Gold, 3);
                ui.Text(Fit(_contactTabs[i], CharsFor(tabW - 0.5f)), tx, tabY, sel ? Gold : Soft, 1, 0.5f);
            }

            float bodyTop = top - 2.55f, bodyBottom = -top + 1.5f, bodyH = bodyTop - bodyBottom;
            float inner = w - 1.6f;
            float listW = inner * 0.38f, detailW = inner - listW - 0.4f;
            float listCx = left + 0.8f + listW * 0.5f;
            float detailL = left + 0.8f + listW + 0.4f;
            float midY = bodyBottom + bodyH * 0.5f;
            ui.Panel("lc_panel_soft", listCx, midY, listW, bodyH, 1);
            ui.Panel("lc_panel_soft", detailL + detailW * 0.5f, midY, detailW, bodyH, 1);

            LayoutContactList(ui, listCx, listW, bodyTop, bodyBottom);
            if (_contactRows.Count > 0) LayoutContactDetail(ui, detailL + 0.65f, detailW - 1.3f, bodyTop, bodyBottom);
            else
            {
                float dcx = detailL + detailW * 0.5f;
                ui.Text("BELUM ADA KONTAK", dcx, midY + 0.35f, Soft, 1, 0.5f);
                ui.Text(Fit("BICARALAH DENGAN WARGA UNTUK BERKENALAN", CharsFor(detailW - 1.2f)), dcx, midY - 0.35f, Soft, 1, 0.5f);
            }

            if (!string.IsNullOrEmpty(_contactMessage))
                ui.Text(Fit(_contactMessage, CharsFor(w - 1.6f)), 0f, -top + 1.0f, Pink, 1, 0.5f);
            ui.Text(Fit(_contactHint, CharsFor(w - 1.6f)), 0f, -top + 0.45f, Gold, 1, 0.5f);
            if (_contactPicker != null) LayoutContactPicker(ui, _contactPicker, w, h);
            ui.End();
        }

        ContactPicker _contactPicker;

        /// <summary>
        /// Jendela pilihan di tengah layar kontak (latar diredupkan):
        /// potret + judul, catatan kecil, pilihan bernomor berbentuk tombol (terpilih = emas,
        /// tidak tersedia = redup) dengan keterangan di bawah label, lalu petunjuk tombol.
        /// </summary>
        void LayoutContactPicker(UiPool ui, ContactPicker p, float w, float h)
        {
            bool anySub = false;
            foreach (var o in p.Options) anySub |= !string.IsNullOrEmpty(o.Sub);
            float rowH = anySub ? 1.2f : 0.85f, rowGap = 0.16f;
            bool hasNote = !string.IsNullOrEmpty(p.Note);

            float pw = Mathf.Min(w - 2.4f, 16f);
            float headH = 1.55f + (hasNote ? 0.62f : 0f);
            float ph = headH + p.Options.Count * (rowH + rowGap) + 1.25f;
            ph = Mathf.Min(ph, h - 0.8f);
            float top = ph * 0.5f, left = -pw * 0.5f;

            ui.Solid(0f, 0f, w - 0.2f, h - 0.2f, new Color(0.02f, 0.02f, 0.06f, 0.62f), 6);
            ui.Panel("lc_panel_gold", 0f, 0f, pw, ph, 7);

            // kepala: potret kecil + judul
            float x0 = left + 0.6f, inner = pw - 1.2f;
            float ty = top - 0.85f;
            float textX = x0;
            if (!string.IsNullOrEmpty(p.Portrait))
            {
                ui.Solid(x0 + 0.45f, ty, 0.9f, 1.1f, InvFrame, 8);
                ui.Sprite(PixelArt.Get(p.Portrait), x0 + 0.45f, ty - 0.5f, 0.58f, 9);
                textX = x0 + 1.25f;
            }
            string title = Safe(p.Title);
            float titleW = x0 + inner - textX;
            if (title.Length * UiCharW * 2f <= titleW) ui.Text(title, textX, ty, Gold, 2, 0f, 9);
            else ui.Text(Fit(title, CharsFor(titleW)), textX, ty, Gold, 1, 0f, 9);
            float y = top - headH + 0.1f;
            if (hasNote) ui.Text(Fit(p.Note, CharsFor(inner)), x0, top - 1.75f, Soft, 1, 0f, 9);
            ui.Solid(0f, y + 0.12f, inner, 0.05f, JRule, 8);

            // pilihan
            y -= rowGap + rowH * 0.5f;
            for (int i = 0; i < p.Options.Count; i++)
            {
                if (y - rowH * 0.5f < -top + 0.8f) break;
                var o = p.Options[i];
                bool sel = i == p.Selected;
                ui.Solid(0f, y, inner, rowH, sel ? InvTabOn : InvTabOff, 8);
                if (sel) ui.Solid(left + 0.6f + 0.05f, y, 0.1f, rowH, Gold, 9);
                Color lc = !o.Enabled ? HomeDisabled : sel ? Gold : Ink;
                float lx = x0 + 0.45f;
                ui.Text((i + 1) + ".", lx, anySub ? y + 0.24f : y, o.Enabled ? Soft : HomeDisabled, 1, 0f, 9);
                float labelX = lx + 0.85f;
                int chars = CharsFor(x0 + inner - 0.3f - labelX);
                if (anySub)
                {
                    ui.Text(Fit(o.Label, chars), labelX, y + 0.24f, lc, 1, 0f, 9);
                    if (!string.IsNullOrEmpty(o.Sub))
                        ui.Text(Fit(o.Sub, chars), labelX, y - 0.3f, o.Enabled ? Soft : HomeDisabled, 1, 0f, 9);
                }
                else ui.Text(Fit(o.Label, chars), labelX, y, lc, 1, 0f, 9);
                y -= rowH + rowGap;
            }

            ui.Text(Fit("W/S - PILIH   E - OK   ESC - BATAL", CharsFor(inner)), 0f, -top + 0.5f, Gold, 1, 0.5f, 9);
        }

        void LayoutContactList(UiPool ui, float cx, float lw, float bodyTop, float bodyBottom)
        {
            float l = cx - lw * 0.5f + 0.35f, r = cx + lw * 0.5f - 0.35f;
            if (_contactRows.Count == 0)
            {
                ui.Text("KOSONG", cx, bodyTop - 0.9f, Soft, 1, 0.5f);
                return;
            }
            float rowsTop = bodyTop - 0.4f - ContactRowH * 0.5f;
            int visible = Mathf.Max(1, Mathf.FloorToInt((rowsTop - bodyBottom - ContactRowH * 0.5f - 0.2f) / ContactRowH) + 1);
            if (_contactSel < _contactFirst) _contactFirst = _contactSel;
            if (_contactSel >= _contactFirst + visible) _contactFirst = _contactSel - visible + 1;
            _contactFirst = Mathf.Clamp(_contactFirst, 0, Mathf.Max(0, _contactRows.Count - visible));

            float textX = l + 1.35f;
            for (int i = 0; i < visible; i++)
            {
                int idx = _contactFirst + i;
                if (idx >= _contactRows.Count) break;
                var c = _contactRows[idx];
                float y = rowsTop - i * ContactRowH;
                bool sel = idx == _contactSel;
                if (sel) ui.Solid(cx, y, lw - 0.3f, ContactRowH - 0.14f, InvRowSel, 2);
                else if (i > 0) ui.Solid(cx, y + ContactRowH * 0.5f, lw - 0.7f, 0.04f, JRule, 2);   // garis pemisah tipis
                // potret kecil dalam bingkai
                ui.Solid(l + 0.55f, y, 0.95f, ContactRowH - 0.34f, InvFrame, 2);
                if (!string.IsNullOrEmpty(c.Portrait))
                    ui.Sprite(PixelArt.Get(c.Portrait), l + 0.55f, y - (ContactRowH - 0.34f) * 0.5f + 0.06f, 0.62f);
                string status = Safe(c.Status).Trim();
                float statusW = status.Length > 0 ? status.Length * UiCharW + 0.4f : 0f;
                ui.Text(Fit(c.Name, CharsFor(r - textX - statusW)), textX, y + 0.27f, sel ? Gold : Ink);
                ui.Text(Fit(c.Role, CharsFor(r - textX)), textX, y - 0.29f, Soft);
                if (status.Length > 0) ui.Text(status, r, y + 0.27f, Pink, 1, 1f);
            }
            if (_contactFirst > 0) ui.Text("▲", cx, bodyTop - 0.2f, Soft, 1, 0.5f);
            if (_contactFirst + visible < _contactRows.Count) ui.Text("▼", cx, bodyBottom + 0.22f, Soft, 1, 0.5f);
        }

        void LayoutContactDetail(UiPool ui, float x0, float dw, float bodyTop, float bodyBottom)
        {
            var d = _contactDetail;
            float right = x0 + dw;

            // ---- potret dalam bingkai
            float fw = 2.4f, fh = 3.2f;
            float fcx = x0 + fw * 0.5f, fcy = bodyTop - 0.5f - fh * 0.5f;
            ui.Solid(fcx, fcy, fw, fh, InvFrame, 2);
            ui.Solid(fcx, fcy - fh * 0.5f + 0.04f, fw, 0.08f, Gold, 3);
            if (!string.IsNullOrEmpty(d.Portrait))
                ui.Sprite(PixelArt.Get(d.Portrait), fcx, fcy - fh * 0.5f + 0.22f, 1.65f, 4);

            // ---- nama, peran, hubungan, kedekatan (di kanan potret)
            float nx = x0 + fw + 0.6f, nw = right - nx;
            float y = bodyTop - 0.9f;
            string name = Safe(d.Name);
            if (name.Length * UiCharW * 2f <= nw) { ui.Text(name, nx, y, Ink, 2); y -= 0.95f; }
            else { ui.Text(Fit(name, CharsFor(nw)), nx, y, Ink); y -= CLine + 0.1f; }
            ui.Text(Fit(d.Role, CharsFor(nw)), nx, y, Gold);
            y -= CLine + 0.12f;
            string rel = Safe(d.Relation);
            float pw = rel.Length * UiCharW + 0.6f;
            ui.Solid(nx + pw * 0.5f, y, pw, 0.6f, JPill, 2);
            ui.Text(rel, nx + 0.3f, y, Pink);
            if (d.HasBond)
            {
                y -= CLine + 0.12f;
                ui.Text(Fit(d.BondLabel, CharsFor(nw)), nx, y, Soft);
                y -= 0.48f;
                ui.Bar(nx, y, nw, 0.24f, d.Bond, new Color(1f, 0.55f, 0.72f));
            }

            float divY = Mathf.Min(fcy - fh * 0.5f - 0.45f, y - 0.5f);
            ui.Solid(x0 + dw * 0.5f, divY, dw, 0.06f, JRule, 2);

            // ---- tombol aksi di bawah
            float btnY = bodyBottom + 0.6f;
            bool hasAction = !string.IsNullOrEmpty(d.Action);
            if (hasAction)
            {
                ui.Solid(x0 + dw * 0.5f, btnY, dw, 0.72f, d.ActionEnabled ? InvBtnOn : InvBtnOff, 2);
                ui.Text(Fit(d.Action, CharsFor(dw - 0.5f)), x0 + dw * 0.5f, btnY, d.ActionEnabled ? Ink : Soft, 1, 0.5f);
            }
            float floor = hasAction ? btnY + 0.75f : bodyBottom + 0.35f;

            if (d.ShowChat) { LayoutChat(ui, x0, dw, divY - 0.3f, floor); return; }

            // ---- baris keterangan: kolom label (rata kiri) | kolom isi (rata kiri, sejajar)
            y = divY - 0.6f;
            int labelLen = 0;
            foreach (var row in d.Rows) labelLen = Mathf.Max(labelLen, Safe(row.Label).Length);
            float valueX = x0 + labelLen * UiCharW + 0.8f;
            int valueChars = CharsFor(right - valueX);
            foreach (var row in d.Rows)
            {
                if (y < floor + 0.3f) return;
                ui.Text(Safe(row.Label), x0, y, Soft);
                ui.Text(Fit(row.Value, valueChars), valueX, y, Ink);
                ui.Solid(x0 + dw * 0.5f, y - CRowGap * 0.5f, dw, 0.04f, JRule, 2);
                y -= CRowGap;
            }

            // ---- paragraf
            if (string.IsNullOrEmpty(d.About)) return;
            y -= 0.2f;
            if (y < floor + 0.4f) return;
            if (!string.IsNullOrEmpty(d.AboutTitle)) { ui.Text(Safe(d.AboutTitle), x0, y, Gold); y -= CLine + 0.12f; }
            int maxLines = Mathf.Max(1, Mathf.FloorToInt((y - floor) / CLine) + 1);
            foreach (string l in WrapAll(Safe(d.About), CharsFor(dw), maxLines)) { ui.Text(l, x0, y, Ink); y -= CLine; }
        }

        /// <summary>Gelembung obrolan: terbaru di bawah, disusun ke atas sampai ruang habis.</summary>
        void LayoutChat(UiPool ui, float x0, float dw, float areaTop, float floor)
        {
            var chat = _contactDetail.Chat;
            if (chat.Count == 0)
            {
                float my = (areaTop + floor) * 0.5f;
                ui.Text("BELUM ADA PESAN", x0 + dw * 0.5f, my + 0.35f, Soft, 1, 0.5f);
                ui.Text(Fit("TEKAN E UNTUK MENGIRIM PESAN PERTAMA", CharsFor(dw)), x0 + dw * 0.5f, my - 0.35f, Soft, 1, 0.5f);
                return;
            }
            const float padX = 0.35f, padY = 0.28f, gap = 0.5f;
            float bubbleMax = dw * 0.72f;
            int chars = CharsFor(bubbleMax - padX * 2f);
            float y = floor + 0.15f;   // dasar gelembung paling baru
            for (int i = chat.Count - 1; i >= 0; i--)
            {
                var m = chat[i];
                string[] lines = WrapAll(Safe(m.Text), chars, 4);
                int longest = 0;
                foreach (string l in lines) longest = Mathf.Max(longest, l.Length);
                float bw = longest * UiCharW + padX * 2f;
                float bh = (lines.Length - 1) * CLine + 0.56f + padY * 2f;
                if (y + bh > areaTop) return;
                Color bg = m.Mine ? ChatMine : ChatTheirs;
                float bx = m.Mine ? x0 + dw - bw : x0;
                ui.Solid(bx + bw * 0.5f, y + bh * 0.5f, bw, bh, bg, 2);
                // ekor gelembung kecil di sisi pengirim
                ui.Solid(m.Mine ? bx + bw - 0.15f : bx + 0.15f, y - 0.07f, 0.3f, 0.16f, bg, 2);
                float ty = y + bh - padY - 0.28f;
                foreach (string l in lines) { ui.Text(l, bx + padX, ty, Ink); ty -= CLine; }
                y += bh + gap;
            }
        }
    }
}
