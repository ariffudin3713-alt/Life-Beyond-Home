using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>Satu langkah misi di jurnal. State: 0 = selesai, 1 = sedang dikerjakan, 2 = berikutnya.</summary>
    public struct JournalStep
    {
        public string Text;
        public int State;
    }

    public struct JournalStat
    {
        public string Label, Value;
    }

    /// <summary>Satu baris hubungan: nama, keterangan kanan, dan bilah kedekatan 0..1.</summary>
    public struct JournalBond
    {
        public string Name, Detail;
        public float Fill;
        public Color Color;
    }

    public class JournalView
    {
        public string Chapter = "", Title = "", Task = "", Clock = "";
        public readonly List<JournalStep> Steps = new List<JournalStep>();
        public readonly List<JournalStat> Stats = new List<JournalStat>();
        public readonly List<JournalBond> Bonds = new List<JournalBond>();
    }

    /// <summary>
    /// JURNAL (tombol J): panel emas dua kolom.
    /// - Kiri  "MISI": label bab, judul misi, tujuan saat ini, dan daftar langkah bertanda
    ///   (✓ selesai, ▶ sedang dikerjakan, - berikutnya).
    /// - Kanan "CATATAN DIRI": level, energi, uang, tabungan, pekerjaan; lalu "HUBUNGAN" dengan bilah kedekatan.
    /// Semua teks dibungkus/dipotong sesuai lebar kolom, dan berhenti rapi bila tinggi layar tidak cukup.
    /// </summary>
    public partial class LifeHud
    {
        static readonly Color JDone = new Color(0.55f, 0.92f, 0.55f);
        static readonly Color JRule = new Color(1f, 1f, 1f, 0.09f);
        static readonly Color JPill = new Color(0.42f, 0.18f, 0.30f, 1f);

        Transform _journalGroup;
        SpriteRenderer _journalHint;
        UiPool _jui;
        JournalView _journal = new JournalView();

        void BuildJournal()
        {
            _journalGroup = Group("Journal");
            _jui = new UiPool(_journalGroup, 4010);
            _journalHint = Text(_journalGroup, "Hint", "J / ESC - TUTUP", 1, Gold, new Vector2(0.5f, 0.5f), 4016);
            _journalGroup.gameObject.SetActive(false);
        }

        public void ShowJournal(JournalView view)
        {
            _journal = view ?? new JournalView();
            SetDim(0.72f);
            _journalGroup.gameObject.SetActive(true);
            HidePrompt();
        }

        public void HideJournal()
        {
            if (_journalGroup != null) _journalGroup.gameObject.SetActive(false);
            SetDim(0f);
        }

        void LayoutJournal(float halfW, float halfH)
        {
            if (_journalGroup == null || !_journalGroup.gameObject.activeSelf) return;
            var ui = _jui;
            var v = _journal;
            ui.Begin();

            float w = Mathf.Min(halfW * 2f - 0.8f, 28f);
            float h = Mathf.Min(halfH * 2f - 0.8f, 15.6f);
            float top = h * 0.5f, left = -w * 0.5f;
            ui.Panel("lc_panel_gold", 0f, 0f, w, h, 0);
            CloseAt(w * 0.5f, h * 0.5f);

            // ---- kepala
            ui.Text("JURNAL PERJALANAN", left + 0.7f, top - 0.75f, Gold, 2);
            ui.Text(Safe(v.Clock), -left - 1.1f, top - 0.75f, Soft, 1, 1f);

            float bodyTop = top - 1.45f, bodyBottom = -top + 1.0f, bodyH = bodyTop - bodyBottom;
            float inner = w - 1.4f;
            float lw = inner * 0.56f, rw = inner - lw - 0.3f;
            float lcx = left + 0.7f + lw * 0.5f, rcx = left + 0.7f + lw + 0.3f + rw * 0.5f;
            float midY = bodyBottom + bodyH * 0.5f;
            ui.Panel("lc_panel_soft", lcx, midY, lw, bodyH, 1);
            ui.Panel("lc_panel_soft", rcx, midY, rw, bodyH, 1);

            LayoutJournalMission(ui, v, lcx - lw * 0.5f + 0.5f, lw - 1.0f, bodyTop, bodyBottom);
            LayoutJournalSelf(ui, v, rcx - rw * 0.5f + 0.5f, rw - 1.0f, bodyTop, bodyBottom);

            _journalHint.transform.localPosition = new Vector3(0f, -top + 0.5f, 0f);
            ui.End();
        }

        void LayoutJournalMission(UiPool ui, JournalView v, float x0, float maxW, float bodyTop, float bodyBottom)
        {
            float y = bodyTop - 0.6f;
            // label bab dalam "pil"
            string chapter = Safe(v.Chapter);
            float pw = chapter.Length * UiCharW + 0.5f;
            ui.Solid(x0 + pw * 0.5f, y, pw, 0.56f, JPill, 2);
            ui.Text(chapter, x0 + 0.25f, y, Pink);
            y -= 0.85f;

            // judul misi: besar bila muat, kecil & dibungkus bila panjang
            string title = Safe(v.Title);
            if (title.Length * UiCharW * 2f <= maxW)
            {
                ui.Text(title, x0, y, Gold, 2);
                y -= 0.95f;
            }
            else
            {
                foreach (string l in WrapAll(title, CharsFor(maxW), 2)) { ui.Text(l, x0, y, Gold); y -= UiLine; }
                y -= 0.25f;
            }

            ui.Text("TUJUAN SAAT INI", x0, y, Soft);
            y -= UiLine + 0.05f;
            foreach (string l in WrapAll(Safe(v.Task), CharsFor(maxW), 3)) { ui.Text(l, x0, y, Ink); y -= UiLine; }

            y -= 0.12f;
            ui.Solid(x0 + maxW * 0.5f, y, maxW, 0.06f, JRule, 2);
            y -= 0.42f;
            ui.Text("LANGKAH MISI", x0, y, Soft);
            y -= UiLine + 0.08f;

            float textX = x0 + 0.6f;
            int chars = CharsFor(maxW - 0.6f);
            float floor = bodyBottom + 0.45f;
            for (int s = 0; s < v.Steps.Count; s++)
            {
                var step = v.Steps[s];
                string mark = step.State == 0 ? "✓" : step.State == 1 ? "▶" : "-";
                Color markC = step.State == 0 ? JDone : step.State == 1 ? Gold : Soft;
                Color textC = step.State == 1 ? Ink : Soft;
                string[] lines = WrapAll(Safe(step.Text), chars, 3);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (y < floor) { ui.Text("...", textX, y + UiLine * 0.4f, Soft); return; }
                    if (i == 0) ui.Text(mark, x0 + 0.1f, y, markC);
                    ui.Text(lines[i], textX, y, textC);
                    y -= UiLine - 0.02f;
                }
                y -= 0.1f;
            }
            if (v.Steps.Count == 0) ui.Text("TIDAK ADA LANGKAH KHUSUS", textX, y, Soft);
        }

        void LayoutJournalSelf(UiPool ui, JournalView v, float x0, float maxW, float bodyTop, float bodyBottom)
        {
            float right = x0 + maxW;
            float floor = bodyBottom + 0.35f;
            float y = bodyTop - 0.6f;
            ui.Text("CATATAN DIRI", x0, y, Gold);
            y -= 0.62f;
            foreach (var st in v.Stats)
            {
                if (y < floor) return;
                string label = Safe(st.Label);
                int room = CharsFor(maxW) - label.Length - 2;
                ui.Text(label, x0, y, Soft);
                ui.Text(Fit(st.Value, room), right, y, Ink, 1, 1f);
                ui.Solid(x0 + maxW * 0.5f, y - 0.27f, maxW, 0.04f, JRule, 2);
                y -= UiLine + 0.06f;
            }

            y -= 0.25f;
            if (y < floor) return;
            ui.Text("HUBUNGAN", x0, y, Gold);
            y -= 0.62f;
            if (v.Bonds.Count == 0) { ui.Text("BELUM ADA TEMAN DEKAT", x0, y, Soft); return; }
            foreach (var b in v.Bonds)
            {
                if (y - 0.42f < floor) { ui.Text("...", x0, y, Soft); return; }
                string name = Safe(b.Name);
                int room = CharsFor(maxW) - name.Length - 2;
                ui.Text(name, x0, y, Ink);
                ui.Text(Fit(b.Detail, room), right, y, Soft, 1, 1f);
                y -= 0.42f;
                ui.Bar(x0, y, maxW, 0.22f, b.Fill, b.Color);
                y -= 0.55f;
            }
        }
    }
}
