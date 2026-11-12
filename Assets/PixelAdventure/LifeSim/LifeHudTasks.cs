using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>Satu tugas di daftar: teks utama + keterangan (lokasi/alasan).</summary>
    public struct TaskItem
    {
        public string Text, Detail;
        public TaskItem(string text, string detail = null) { Text = text; Detail = detail; }
    }

    /// <summary>
    /// Layar DAFTAR TUGAS (tombol T). Tiga kartu berdampingan:
    /// BISA DIKERJAKAN, SUDAH DIKERJAKAN, BELUM BISA DIKERJAKAN.
    /// Setiap tugas memakai ikon, teks dibungkus per kata, keterangan di baris sendiri,
    /// dan garis pemisah. Kartu yang dipilih (A/D) bisa digulir (W/S / roda mouse).
    /// </summary>
    public partial class LifeHud
    {
        const float TaskCharW = 6f / 16f;     // lebar satu huruf PixelFont skala 1
        const float TaskLineStep = 0.44f;
        const float TaskItemGap = 0.75f;      // jarak antar misi (garis pemisah di tengahnya)
        const float TaskScrollSpeed = 9f;     // satuan per detik saat W/S ditahan

        static readonly Color TaskGreen = new Color(0.50f, 0.92f, 0.55f);
        static readonly Color TaskBlue = new Color(0.62f, 0.78f, 1f);
        static readonly Color TaskRed = new Color(1f, 0.58f, 0.50f);
        static readonly Color TaskGray = new Color(0.66f, 0.64f, 0.72f);

        /// <summary>Satu elemen isi kartu dengan posisi relatif terhadap puncak isi (negatif = ke bawah).</summary>
        struct TaskEntry
        {
            public SpriteRenderer Sprite;
            public float X, Y;
            public bool IsDivider;
            public float Width;
        }

        class TaskCard
        {
            public string Title;
            public char Icon;
            public Color Accent, TextColor;
            public SpriteRenderer Panel, Bar, Header, Count, Empty, Track, Thumb, Focus;
            public SpriteRenderer MoreUp, MoreDown;
            public readonly List<SpriteRenderer> Lines = new List<SpriteRenderer>();
            public readonly List<SpriteRenderer> Icons = new List<SpriteRenderer>();
            public readonly List<SpriteRenderer> Dividers = new List<SpriteRenderer>();
            public readonly List<TaskEntry> Entries = new List<TaskEntry>();
            public readonly List<TaskItem> Items = new List<TaskItem>();
            public int UsedLines, UsedIcons, UsedDividers;
            public float ContentHeight, Scroll, ScrollTarget, ViewHeight;
        }

        Transform _taskGroup;
        SpriteRenderer _taskPanel, _taskTitle, _taskSub, _taskBarBg, _taskBarFill, _taskProgress, _taskHint;
        readonly TaskCard[] _taskCards = new TaskCard[3];
        float _taskRatio;
        float _taskBuiltWidth = -1f, _taskBuiltHeight = -1f;
        int _taskFocus;

        void BuildTasks()
        {
            _taskGroup = Group("Tasks");
            _taskPanel = Panel(_taskGroup, "Panel", "lc_panel_gold", 4010);
            _taskTitle = Text(_taskGroup, "Title", "DAFTAR TUGAS", 2, Gold, new Vector2(0.5f, 0.5f), 4013);
            _taskSub = Text(_taskGroup, "Sub", " ", 1, Pink, new Vector2(0.5f, 0.5f), 4013);
            _taskBarBg = Solid(_taskGroup, "ProgressBg", new Color(0.08f, 0.06f, 0.11f, 1f), 4012);
            _taskBarFill = Solid(_taskGroup, "ProgressFill", TaskGreen, 4013);
            _taskProgress = Text(_taskGroup, "Progress", " ", 1, Ink, new Vector2(0f, 0.5f), 4013);
            _taskHint = Text(_taskGroup, "Hint", "A/D - PILIH KARTU   W/S - GULIR   T/ESC - TUTUP", 1, Gold,
                new Vector2(0.5f, 0.5f), 4013);

            _taskCards[0] = MakeCard("BISA DIKERJAKAN", '▶', TaskGreen, Ink);
            _taskCards[1] = MakeCard("SUDAH DIKERJAKAN", '✓', TaskBlue, TaskGray);
            _taskCards[2] = MakeCard("BELUM BISA", '▣', TaskRed, Soft);
            _taskGroup.gameObject.SetActive(false);
        }

        TaskCard MakeCard(string title, char icon, Color accent, Color textColor)
        {
            var c = new TaskCard { Title = title, Icon = icon, Accent = accent, TextColor = textColor };
            c.Focus = Solid(_taskGroup, "CardFocus" + title, accent, 4011);
            c.Panel = Panel(_taskGroup, "Card" + title, "lc_panel_soft", 4012);
            c.Bar = Solid(_taskGroup, "CardBar" + title,
                new Color(accent.r * 0.45f, accent.g * 0.45f, accent.b * 0.45f, 1f), 4014);
            c.Header = Text(_taskGroup, "CardHeader" + title, icon + " " + title, 1, accent,
                new Vector2(0f, 0.5f), 4015);
            c.Count = Text(_taskGroup, "CardCount" + title, "0", 1, Ink, new Vector2(1f, 0.5f), 4015);
            c.Empty = Text(_taskGroup, "CardEmpty" + title, "TIDAK ADA", 1, TaskGray,
                new Vector2(0.5f, 0.5f), 4013);
            c.Track = Solid(_taskGroup, "CardTrack" + title, new Color(0.08f, 0.06f, 0.11f, 0.9f), 4013);
            c.Thumb = Solid(_taskGroup, "CardThumb" + title, accent, 4014);
            c.MoreUp = Text(_taskGroup, "CardMoreUp" + title, "▲ LAINNYA", 1, TaskGray,
                new Vector2(0.5f, 0.5f), 4015);
            c.MoreDown = Text(_taskGroup, "CardMoreDown" + title, "▼ LAINNYA", 1, TaskGray,
                new Vector2(0.5f, 0.5f), 4015);
            return c;
        }

        public bool TasksVisible => _taskGroup != null && _taskGroup.gameObject.activeSelf;

        public void ShowTasks(string subtitle, int doneCount, int totalCount,
            List<TaskItem> available, List<TaskItem> done, List<TaskItem> locked)
        {
            SetText(_taskSub, string.IsNullOrEmpty(subtitle) ? " " : CleanTaskText(subtitle), Pink, 1,
                new Vector2(0.5f, 0.5f));
            _taskRatio = totalCount > 0 ? Mathf.Clamp01(doneCount / (float)totalCount) : 1f;
            SetText(_taskProgress, totalCount > 0 ? doneCount + "/" + totalCount + " SELESAI" : "BEBAS",
                Ink, 1, new Vector2(0f, 0.5f));

            var lists = new[] { available, done, locked };
            for (int i = 0; i < 3; i++)
            {
                var card = _taskCards[i];
                card.Items.Clear();
                if (lists[i] != null) card.Items.AddRange(lists[i]);
                SetText(card.Count, card.Items.Count.ToString(), Ink, 1, new Vector2(1f, 0.5f));
                card.Scroll = card.ScrollTarget = 0f;
            }
            _taskFocus = 0;
            _taskBuiltWidth = -1f; // isi kartu dibangun ulang saat layout
            SetDim(0.78f);
            _taskGroup.gameObject.SetActive(true);
            HidePrompt();
        }

        public void HideTasks()
        {
            if (_taskGroup != null) _taskGroup.gameObject.SetActive(false);
            SetDim(0f);
        }

        /// <summary>Pindah kartu yang dipilih (-1 kiri, +1 kanan).</summary>
        public void FocusTaskCard(int delta)
        {
            _taskFocus = (_taskFocus + delta + 3) % 3;
        }

        /// <summary>Gulir kartu yang dipilih. amount positif = ke bawah (satuan dunia).</summary>
        public void ScrollTasks(float amount)
        {
            var c = _taskCards[_taskFocus];
            float max = Mathf.Max(0f, c.ContentHeight - c.ViewHeight);
            c.ScrollTarget = Mathf.Clamp(c.ScrollTarget + amount, 0f, max);
        }

        /// <summary>Gulir satu tugas penuh (dipakai roda mouse / tekan sekali).</summary>
        public void ScrollTasksStep(int direction) => ScrollTasks(direction * (TaskLineStep * 3f));

        void LayoutTasks(float halfW, float halfH)
        {
            if (!TasksVisible) return;

            float w = Mathf.Min(halfW * 2f - 0.8f, 30f);
            float h = halfH * 2f - 0.8f;
            float top = h * 0.5f;
            SetRect(_taskPanel, 0f, 0f, w, h);

            _taskTitle.transform.localPosition = new Vector3(0f, top - 0.72f, 0f);
            _taskSub.transform.localPosition = new Vector3(0f, top - 1.45f, 0f);

            // Bar progres misi saat ini.
            float barW = Mathf.Min(9f, w * 0.35f);
            float barL = -barW * 0.5f - 1.2f;
            float barY = top - 2.05f;
            SetSolid(_taskBarBg, barL + barW * 0.5f, barY, barW, 0.30f);
            float fillW = (barW - 0.1f) * _taskRatio;
            SetSolid(_taskBarFill, barL + 0.05f + fillW * 0.5f, barY, fillW, 0.20f);
            _taskProgress.transform.localPosition = new Vector3(barL + barW + 0.35f, barY, 0f);

            _taskHint.transform.localPosition = new Vector3(0f, -top + 0.45f, 0f);

            // Tiga kartu.
            float gap = 0.35f;
            float inner = w - 1.0f;
            float cw = (inner - gap * 2f) / 3f;
            float cardTop = top - 2.55f;
            float cardBottom = -top + 0.95f;
            float ch = cardTop - cardBottom;

            bool rebuild = !Mathf.Approximately(cw, _taskBuiltWidth) || !Mathf.Approximately(ch, _taskBuiltHeight);
            float dt = Time.unscaledDeltaTime;
            for (int i = 0; i < 3; i++)
            {
                var c = _taskCards[i];
                float cx = -inner * 0.5f + cw * 0.5f + i * (cw + gap);
                bool focused = i == _taskFocus;

                // Bingkai warna di belakang kartu yang dipilih.
                c.Focus.gameObject.SetActive(focused);
                if (focused) SetSolid(c.Focus, cx, cardBottom + ch * 0.5f, cw + 0.16f, ch + 0.16f);
                SetRect(c.Panel, cx, cardBottom + ch * 0.5f, cw, ch);
                SetSolid(c.Bar, cx, cardTop - 0.48f, cw - 0.36f, 0.62f);
                c.Header.transform.localPosition = new Vector3(cx - cw * 0.5f + 0.38f, cardTop - 0.48f, 0f);
                c.Count.transform.localPosition = new Vector3(cx + cw * 0.5f - 0.38f, cardTop - 0.48f, 0f);

                // Area isi yang bisa digulir.
                float viewTop = cardTop - 1.05f;
                float viewBottom = cardBottom + 0.30f;
                c.ViewHeight = viewTop - viewBottom;
                if (rebuild) FillCard(c, cx, cw);

                float max = Mathf.Max(0f, c.ContentHeight - c.ViewHeight);
                c.ScrollTarget = Mathf.Clamp(c.ScrollTarget, 0f, max);
                c.Scroll = Mathf.Lerp(c.Scroll, c.ScrollTarget, 1f - Mathf.Exp(-14f * dt));
                if (Mathf.Abs(c.Scroll - c.ScrollTarget) < 0.005f) c.Scroll = c.ScrollTarget;
                // Pixel-snap agar teks tetap tajam saat bergulir.
                float scroll = Mathf.Round(c.Scroll * 16f) / 16f;

                PlaceEntries(c, viewTop, viewBottom, scroll);

                c.Empty.gameObject.SetActive(c.Items.Count == 0);
                c.Empty.transform.localPosition = new Vector3(cx, viewTop - 0.3f, 0f);

                // Scrollbar + penanda "masih ada".
                bool scrollable = max > 0.01f;
                c.Track.gameObject.SetActive(scrollable);
                c.Thumb.gameObject.SetActive(scrollable);
                c.MoreUp.gameObject.SetActive(scrollable && scroll > 0.05f);
                c.MoreDown.gameObject.SetActive(scrollable && scroll < max - 0.05f);
                if (scrollable)
                {
                    float trackX = cx + cw * 0.5f - 0.22f;
                    float viewH = c.ViewHeight;
                    SetSolid(c.Track, trackX, viewBottom + viewH * 0.5f, 0.12f, viewH);
                    float thumbH = Mathf.Max(0.6f, viewH * (viewH / c.ContentHeight));
                    float t = max > 0f ? scroll / max : 0f;
                    float thumbY = viewTop - thumbH * 0.5f - t * (viewH - thumbH);
                    SetSolid(c.Thumb, trackX, thumbY, 0.12f, thumbH);
                    SetAlpha(c.Thumb, focused ? 1f : 0.45f);
                    c.MoreUp.transform.localPosition = new Vector3(cx, viewTop + 0.12f, 0f);
                    c.MoreDown.transform.localPosition = new Vector3(cx, viewBottom - 0.02f, 0f);
                }
            }
            _taskBuiltWidth = cw;
            _taskBuiltHeight = ch;
        }

        /// <summary>Susun seluruh isi kartu (tanpa batas tinggi) ke daftar entri.</summary>
        void FillCard(TaskCard c, float cx, float cw)
        {
            c.UsedLines = 0;
            c.UsedIcons = 0;
            c.UsedDividers = 0;
            c.Entries.Clear();

            float left = cx - cw * 0.5f + 0.38f;
            float textX = left + 0.55f;
            float right = cx + cw * 0.5f - 0.45f;          // sisakan ruang scrollbar
            int chars = Mathf.Max(8, Mathf.FloorToInt((right - textX) / TaskCharW));
            Color detailColor = new Color(c.Accent.r * 0.85f, c.Accent.g * 0.85f, c.Accent.b * 0.85f, 1f);

            float y = -0.25f;   // relatif terhadap puncak area isi
            for (int k = 0; k < c.Items.Count; k++)
            {
                var item = c.Items[k];
                if (k > 0)
                {
                    // Garis pemisah tipis di tengah jarak antar misi.
                    float lineY = y + (TaskLineStep + TaskItemGap) * 0.5f;
                    AddDivider(c, left, right, lineY);
                }

                string[] main = WrapAll(CleanTaskText(item.Text), chars, 3);
                string[] detail = string.IsNullOrEmpty(item.Detail)
                    ? new string[0] : WrapAll(CleanTaskText(item.Detail), chars, 2);

                AddIcon(c, left + 0.12f, y);
                foreach (string line in main) { AddLine(c, textX, y, line, c.TextColor); y -= TaskLineStep; }
                foreach (string line in detail) { AddLine(c, textX, y, line, detailColor); y -= TaskLineStep; }
                y -= TaskItemGap;
            }
            // Tinggi isi = jarak dari puncak sampai baris terakhir + sedikit ruang.
            c.ContentHeight = c.Items.Count == 0 ? 0f : -(y + TaskItemGap) + 0.05f;

            for (int i = c.UsedLines; i < c.Lines.Count; i++) c.Lines[i].gameObject.SetActive(false);
            for (int i = c.UsedIcons; i < c.Icons.Count; i++) c.Icons[i].gameObject.SetActive(false);
            for (int i = c.UsedDividers; i < c.Dividers.Count; i++) c.Dividers[i].gameObject.SetActive(false);
        }

        /// <summary>Tempatkan entri sesuai gulir; yang keluar area isi disembunyikan.</summary>
        void PlaceEntries(TaskCard c, float viewTop, float viewBottom, float scroll)
        {
            const float halfLine = 0.2f;
            foreach (var e in c.Entries)
            {
                float y = viewTop + e.Y + scroll;
                bool visible = y + halfLine <= viewTop + 0.02f && y - halfLine >= viewBottom - 0.02f;
                if (e.Sprite.gameObject.activeSelf != visible) e.Sprite.gameObject.SetActive(visible);
                if (!visible) continue;
                if (e.IsDivider) SetSolid(e.Sprite, e.X, y, e.Width, 0.05f);
                else e.Sprite.transform.localPosition = new Vector3(e.X, y, 0f);
            }
        }

        void AddLine(TaskCard c, float x, float y, string text, Color color)
        {
            if (c.UsedLines >= c.Lines.Count)
                c.Lines.Add(Text(_taskGroup, "TaskLine", " ", 1, Ink, new Vector2(0f, 0.5f), 4013));
            var sr = c.Lines[c.UsedLines++];
            SetText(sr, string.IsNullOrEmpty(text) ? " " : text, color, 1, new Vector2(0f, 0.5f));
            c.Entries.Add(new TaskEntry { Sprite = sr, X = x, Y = y });
        }

        void AddIcon(TaskCard c, float x, float y)
        {
            if (c.UsedIcons >= c.Icons.Count)
                c.Icons.Add(Text(_taskGroup, "TaskIcon", " ", 1, Ink, new Vector2(0f, 0.5f), 4013));
            var sr = c.Icons[c.UsedIcons++];
            SetText(sr, c.Icon.ToString(), c.Accent, 1, new Vector2(0f, 0.5f));
            c.Entries.Add(new TaskEntry { Sprite = sr, X = x, Y = y });
        }

        void AddDivider(TaskCard c, float left, float right, float y)
        {
            if (c.UsedDividers >= c.Dividers.Count)
                c.Dividers.Add(Solid(_taskGroup, "TaskDivider",
                    new Color(c.Accent.r, c.Accent.g, c.Accent.b, 0.22f), 4013));
            var sr = c.Dividers[c.UsedDividers++];
            c.Entries.Add(new TaskEntry
            {
                Sprite = sr, X = (left + right) * 0.5f, Y = y, IsDivider = true, Width = right - left
            });
        }

        /// <summary>Huruf besar; tanda pisah panjang (tidak ada di font piksel) jadi "-".</summary>
        static string CleanTaskText(string text) =>
            text.ToUpperInvariant().Replace('–', '-').Replace('—', '-');

        /// <summary>Bungkus per kata; kata yang terlalu panjang dipotong; maksimal maxLines baris.</summary>
        static string[] WrapAll(string text, int maxChars, int maxLines)
        {
            var lines = new List<string>();
            string cur = "";
            foreach (string raw in text.Split(' '))
            {
                if (raw.Length == 0) continue;
                string word = raw.Length > maxChars ? raw.Substring(0, maxChars) : raw;
                if (cur.Length == 0) cur = word;
                else if (cur.Length + 1 + word.Length <= maxChars) cur += " " + word;
                else { lines.Add(cur); cur = word; }
            }
            if (cur.Length > 0) lines.Add(cur);
            if (lines.Count == 0) lines.Add(" ");
            if (lines.Count > maxLines)
            {
                lines.RemoveRange(maxLines, lines.Count - maxLines);
                string l = lines[maxLines - 1];
                if (l.Length > maxChars - 3) l = l.Substring(0, maxChars - 3);
                lines[maxLines - 1] = l + "...";
            }
            return lines.ToArray();
        }
    }
}
