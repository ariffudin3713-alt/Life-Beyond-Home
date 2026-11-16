using PixelAdventure.LifeSim.Pulang;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Layar mini-game kerja: judul pekerjaan + ikon, tugas ronde ini, batas waktu, area permainan
    /// (bar tepat waktu / bar tekan berulang / pilihan / panah kombo / bidikan), umpan balik besar,
    /// dan status (tepat, kombo).
    /// </summary>
    public partial class LifeHud
    {
        const int WgOptions = 4, WgArrows = 8;

        Transform _wg;
        WorkGame _wgGame;
        SpriteRenderer _wgPanel, _wgTitle, _wgInstr, _wgInstr2, _wgIcon, _wgRound, _wgTask, _wgStatus, _wgFeedback, _wgHint;
        string _wgInstrFull = "";
        int _wgInstrChars = -1;
        float _wgWidth = 20f;
        SpriteRenderer _wgTimerBg, _wgTimerFill, _wgBarBg, _wgZone, _wgMarker, _wgFill, _wgMashText;
        SpriteRenderer _wgOptHighlight, _wgField, _wgTarget, _wgCross;
        readonly SpriteRenderer[] _wgOpts = new SpriteRenderer[WgOptions];
        readonly SpriteRenderer[] _wgArrows = new SpriteRenderer[WgArrows];
        readonly SpriteRenderer[] _wgPadBg = new SpriteRenderer[4];     // tombol panah yang bisa diklik (kombo)
        readonly SpriteRenderer[] _wgPadIcon = new SpriteRenderer[4];
        string _wgTaskText, _wgRoundText, _wgStatusText, _wgFeedbackText;
        readonly string[] _wgOptText = new string[WgOptions];
        int _wgOptSel = -1;
        static readonly string[] ArrowSprites = { "lc_arrow_up", "lc_arrow_right", "lc_arrow_down", "lc_arrow_left" };

        void BuildWorkGame()
        {
            _wg = Group("WorkGame");
            _wgPanel = Panel(_wg, "Panel", "lc_panel_gold", 4010);
            _wgTitle = Text(_wg, "Title", " ", 2, Gold, new Vector2(0.5f, 0.5f), 4012);
            _wgInstr = Text(_wg, "Instr", " ", 1, Soft, new Vector2(0.5f, 0.5f), 4012);
            _wgInstr2 = Text(_wg, "Instr2", " ", 1, Soft, new Vector2(0.5f, 0.5f), 4012);
            _wgIcon = Make(_wg, PixelArt.Get("lc_item_gear"), "Icon", 4012);
            _wgIcon.transform.localScale = Vector3.one * 3f;
            _wgRound = Text(_wg, "Round", " ", 1, Gold, new Vector2(0f, 0.5f), 4012);
            _wgTask = Text(_wg, "Task", " ", 1, Ink, new Vector2(0f, 0.5f), 4012);
            _wgTimerBg = Solid(_wg, "TimerBg", new Color(0.08f, 0.06f, 0.11f, 1f), 4011);
            _wgTimerFill = Solid(_wg, "TimerFill", Gold, 4012);

            _wgBarBg = Solid(_wg, "BarBg", new Color(0.08f, 0.06f, 0.11f, 1f), 4011);
            _wgZone = Solid(_wg, "Zone", new Color(0.38f, 0.86f, 0.45f, 0.95f), 4012);
            _wgMarker = Solid(_wg, "Marker", new Color(1f, 0.95f, 0.72f, 1f), 4013);
            _wgFill = Solid(_wg, "Fill", new Color(0.38f, 0.86f, 0.45f, 0.95f), 4012);
            _wgMashText = Text(_wg, "Mash", "TEKAN E / SPASI / KLIK BERULANG-ULANG!", 1, Gold, new Vector2(0.5f, 0.5f), 4012);

            _wgOptHighlight = Solid(_wg, "OptHi", new Color(1f, 0.84f, 0.40f, 0.25f), 4011);
            for (int i = 0; i < WgOptions; i++)
                _wgOpts[i] = Text(_wg, "Opt" + i, " ", 1, Ink, new Vector2(0f, 0.5f), 4012);
            for (int i = 0; i < WgArrows; i++)
                _wgArrows[i] = Make(_wg, PixelArt.Get(ArrowSprites[0]), "Arrow" + i, 4012);
            for (int i = 0; i < 4; i++)
            {
                _wgPadBg[i] = Solid(_wg, "PadBg" + i, new Color(0.20f, 0.16f, 0.28f, 1f), 4011);
                _wgPadIcon[i] = Make(_wg, PixelArt.Get(ArrowSprites[i]), "PadIcon" + i, 4012);
            }

            _wgField = Solid(_wg, "Field", new Color(0.30f, 0.52f, 0.36f, 1f), 4011);
            _wgTarget = Make(_wg, PixelArt.Get("lc_warga2_down_0"), "Target", 4012);
            _wgCross = Make(_wg, PixelArt.Get("lc_crosshair"), "Cross", 4013);

            _wgFeedback = Text(_wg, "Feedback", " ", 2, Gold, new Vector2(0.5f, 0.5f), 4014);
            _wgStatus = Text(_wg, "Status", " ", 1, Soft, new Vector2(0.5f, 0.5f), 4012);
            _wgHint = Text(_wg, "Hint", " ", 1, Gold, new Vector2(0.5f, 0.5f), 4012);
        }

        public void ShowWorkGame(string title, string instruction, string icon, WorkGame game, string targetSprite)
        {
            if (_wg == null) BuildWorkGame();
            _wgGame = game;
            SetText(_wgTitle, title, Gold, 2, new Vector2(0.5f, 0.5f));
            _wgInstrFull = instruction ?? "";
            _wgInstrChars = -1;   // dibungkus sesuai lebar panel saat layout
            _wgIcon.sprite = PixelArt.Get(icon);
            string hint = game == WorkGame.Choice ? "W/S / KURSOR - PILIH    E / KLIK - PILIH INI"
                : game == WorkGame.Combo ? "TEKAN PANAH / WASD / KLIK TOMBOL SESUAI URUTAN"
                : game == WorkGame.Mash ? "E / SPASI / KLIK - TEKAN SECEPATNYA"
                : game == WorkGame.Aim ? "WASD / MOUSE - GERAKKAN    E / KLIK - AKSI"
                : "E / SPASI / KLIK - TEKAN SAAT DI ZONA HIJAU";
            SetHint(_wgHint, hint, Gold);
            if (!string.IsNullOrEmpty(targetSprite)) _wgTarget.sprite = PixelArt.Get(targetSprite);

            bool timing = game == WorkGame.Timing, mash = game == WorkGame.Mash, choice = game == WorkGame.Choice;
            bool combo = game == WorkGame.Combo, aim = game == WorkGame.Aim;
            _wgBarBg.gameObject.SetActive(timing || mash);
            _wgZone.gameObject.SetActive(timing);
            _wgMarker.gameObject.SetActive(timing);
            _wgFill.gameObject.SetActive(mash);
            _wgMashText.gameObject.SetActive(mash);
            _wgOptHighlight.gameObject.SetActive(choice);
            foreach (var o in _wgOpts) o.gameObject.SetActive(choice);
            foreach (var a in _wgArrows) a.gameObject.SetActive(false);
            for (int i = 0; i < 4; i++) { _wgPadBg[i].gameObject.SetActive(combo); _wgPadIcon[i].gameObject.SetActive(combo); }
            _wgField.gameObject.SetActive(aim);
            _wgTarget.gameObject.SetActive(aim);
            _wgCross.gameObject.SetActive(aim);

            _wgTaskText = _wgRoundText = _wgStatusText = _wgFeedbackText = null;
            for (int i = 0; i < WgOptions; i++) _wgOptText[i] = null;
            _wgOptSel = -1;
            WorkFeedback(" ", true, 0f);
            SetDim(0.55f);
            _wg.gameObject.SetActive(true);
            HidePrompt();
        }

        public void HideWorkGame()
        {
            if (_wg != null && _wg.gameObject.activeSelf) { _wg.gameObject.SetActive(false); SetDim(0f); }
        }

        bool WgVisible => _wg != null && _wg.gameObject.activeSelf;

        public void WorkTask(string round, string task)
        {
            if (round != _wgRoundText) { _wgRoundText = round; SetText(_wgRound, round, Gold, 1, new Vector2(0f, 0.5f)); }
            if (task != _wgTaskText) { _wgTaskText = task; SetHint(_wgTask, task, Ink, 0f); }
        }

        public void WorkStatus(string status)
        {
            if (status == _wgStatusText) return;
            _wgStatusText = status;
            SetHint(_wgStatus, status, Soft);
        }

        /// <summary>Umpan balik besar ("PAS!", "MELESET!") yang memudar.</summary>
        public void WorkFeedback(string text, bool good, float alpha)
        {
            if (text != _wgFeedbackText)
            {
                _wgFeedbackText = text;
                // Huruf besar bila muat; kalimat panjang memakai huruf biasa agar tidak keluar panel.
                int scale = (text ?? "").Length * UiCharW * 2f <= _wgWidth - 1f ? 2 : 1;
                SetText(_wgFeedback, Fit(text, CharsFor(_wgWidth - 1f, scale)), good ? new Color(0.55f, 0.95f, 0.55f) : new Color(1f, 0.5f, 0.45f), scale, new Vector2(0.5f, 0.5f));
            }
            SetAlpha(_wgFeedback, alpha);
            _wgFeedback.transform.localScale = Vector3.one * (1f + (1f - alpha) * 0.15f);
        }

        float _wgTimer01 = -1f, _wgMarker01, _wgZone01, _wgZoneHalf, _wgFill01;
        int[] _wgSeq;
        int _wgSeqPos;
        bool _wgSeqFailed;
        Vector2 _wgCross01, _wgTarget01;

        public void WorkTimer(float frac) => _wgTimer01 = frac;
        public void WorkTiming(float marker, float zone, float half) { _wgMarker01 = marker; _wgZone01 = zone; _wgZoneHalf = half; }
        public void WorkGauge(float fill) => _wgFill01 = Mathf.Clamp01(fill);

        public void WorkChoices(string[] options, int selected)
        {
            for (int i = 0; i < WgOptions; i++)
            {
                bool show = options != null && i < options.Length;
                _wgOpts[i].gameObject.SetActive(show);
                if (!show) continue;
                string t = (i == selected ? "> " : "  ") + (i + 1) + ". " + options[i];
                if (t != _wgOptText[i] || (i == selected) != (i == _wgOptSel))
                {
                    _wgOptText[i] = t;
                    SetHint(_wgOpts[i], t, i == selected ? Gold : Ink, 0f);
                }
            }
            _wgOptSel = selected;
        }

        public void WorkCombo(int[] seq, int pos, bool failed) { _wgSeq = seq; _wgSeqPos = pos; _wgSeqFailed = failed; }
        public void WorkAim(Vector2 cross01, Vector2 target01) { _wgCross01 = cross01; _wgTarget01 = target01; }

        void LayoutWorkGame(float halfW, float halfH)
        {
            if (!WgVisible) return;
            float w = Mathf.Min(20f, halfW * 2f - 0.8f), h = Mathf.Min(11f, halfH * 2f - 0.6f);
            float top = h * 0.5f, left = -w * 0.5f;
            SetRect(_wgPanel, 0f, 0f, w, h);
            CloseAt(w * 0.5f, h * 0.5f);   // X = berhenti kerja (sama dengan ESC)
            _wgWidth = w;
            _wgTitle.transform.localPosition = new Vector3(0f, top - 0.8f, 0f);

            // Instruksi: dibungkus maks. 2 baris sesuai lebar panel (dulu satu baris panjang keluar panel).
            int instrChars = CharsFor(w - 1f);
            if (instrChars != _wgInstrChars)
            {
                _wgInstrChars = instrChars;
                string[] il = Wrap(Safe(_wgInstrFull), instrChars);
                SetText(_wgInstr, il[0], Soft, 1, new Vector2(0.5f, 0.5f));
                SetText(_wgInstr2, il.Length > 1 ? il[1] : " ", Soft, 1, new Vector2(0.5f, 0.5f));
            }
            _wgInstr.transform.localPosition = new Vector3(0f, top - 1.45f, 0f);
            _wgInstr2.transform.localPosition = new Vector3(0f, top - 2.07f, 0f);

            _wgIcon.transform.localPosition = new Vector3(left + 2f, top - 3.2f, 0f);
            _wgRound.transform.localPosition = new Vector3(left + 3.8f, top - 2.75f, 0f);
            _wgTask.transform.localPosition = new Vector3(left + 3.8f, top - 3.4f, 0f);
            float textRoom = w - 3.8f - 0.6f;   // dari kolom teks sampai tepi kanan panel
            FitHint(_wgTask, textRoom);
            FitHint(_wgStatus, w - 1f);
            for (int i = 0; i < WgOptions; i++) FitHint(_wgOpts[i], textRoom);

            float innerW = w - 2.4f;
            bool timer = _wgTimer01 >= 0f;
            _wgTimerBg.gameObject.SetActive(timer);
            _wgTimerFill.gameObject.SetActive(timer);
            if (timer)
            {
                float ty = top - 4.25f, tw = innerW;
                SetSolid(_wgTimerBg, 0f, ty, tw, 0.2f);
                SetSolid(_wgTimerFill, -tw * 0.5f + tw * _wgTimer01 * 0.5f, ty, tw * _wgTimer01, 0.2f);
                _wgTimerFill.color = Color.Lerp(new Color(1f, 0.4f, 0.35f), Gold, Mathf.Clamp01(_wgTimer01 * 2f));
            }

            float cy = top - 6.1f;   // pusat area permainan
            switch (_wgGame)
            {
                case WorkGame.Timing:
                {
                    float bw = innerW - 2f;
                    SetSolid(_wgBarBg, 0f, cy, bw, 0.7f);
                    SetSolid(_wgZone, -bw * 0.5f + _wgZone01 * bw, cy, _wgZoneHalf * 2f * bw, 0.7f);
                    SetSolid(_wgMarker, -bw * 0.5f + _wgMarker01 * bw, cy, 0.12f, 1.1f);
                    break;
                }
                case WorkGame.Mash:
                {
                    float bw = innerW - 2f;
                    SetSolid(_wgBarBg, 0f, cy, bw, 0.8f);
                    SetSolid(_wgFill, -bw * 0.5f + bw * _wgFill01 * 0.5f, cy, bw * _wgFill01, 0.8f);
                    _wgMashText.transform.localPosition = new Vector3(0f, cy - 0.9f, 0f);
                    break;
                }
                case WorkGame.Choice:
                {
                    float rowY = cy + 1.1f, x0 = left + 3.8f;
                    for (int i = 0; i < WgOptions; i++)
                    {
                        _wgOpts[i].transform.localPosition = new Vector3(x0, rowY - i * 0.72f, 0f);
                        if (_wgOpts[i].gameObject.activeSelf)
                            Hit(UiHit.WorkChoice, i, RectAt(0f, rowY - i * 0.72f, innerW - 2.2f, 0.66f));
                    }
                    if (_wgOptSel >= 0) SetSolid(_wgOptHighlight, 0f, rowY - _wgOptSel * 0.72f, innerW - 2.2f, 0.6f);
                    break;
                }
                case WorkGame.Combo:
                {
                    int n = _wgSeq == null ? 0 : Mathf.Min(_wgSeq.Length, WgArrows);
                    float gap = 1.45f, x0 = -(n - 1) * gap * 0.5f;
                    for (int i = 0; i < WgArrows; i++)
                    {
                        bool show = i < n;
                        _wgArrows[i].gameObject.SetActive(show);
                        if (!show) continue;
                        var a = _wgArrows[i];
                        a.sprite = PixelArt.Get(ArrowSprites[_wgSeq[i]]);
                        bool done = i < _wgSeqPos, current = i == _wgSeqPos;
                        a.color = _wgSeqFailed && current ? new Color(1f, 0.45f, 0.4f)
                            : done ? new Color(0.5f, 0.95f, 0.55f) : current ? Color.white : new Color(0.7f, 0.68f, 0.78f);
                        a.transform.localScale = Vector3.one * (current ? 1.35f + Mathf.Sin(Time.unscaledTime * 10f) * 0.06f : 1.05f);
                        a.transform.localPosition = new Vector3(x0 + i * gap, cy + 0.55f, 0f);
                    }

                    // Tombol panah yang bisa diklik (urutan sama dengan WorkGame: atas, kanan, bawah, kiri),
                    // disusun berjajar: ◀ ▲ ▼ ▶.
                    int[] order = { 3, 0, 2, 1 };
                    const float pad = 1.05f, padGap = 0.3f;
                    float px0 = -(pad + padGap) * 1.5f, py = cy - 1.25f;
                    for (int k = 0; k < 4; k++)
                    {
                        int d = order[k];
                        float px = px0 + k * (pad + padGap);
                        SetSolid(_wgPadBg[d], px, py, pad, pad);
                        _wgPadIcon[d].transform.localPosition = new Vector3(px, py, 0f);
                        _wgPadIcon[d].transform.localScale = Vector3.one * 0.9f;
                        Hit(UiHit.WorkArrow, d, RectAt(px, py, pad, pad));
                    }
                    break;
                }
                case WorkGame.Aim:
                {
                    float fw = innerW - 1f, fh = 3.2f;
                    SetSolid(_wgField, 0f, cy, fw, fh);
                    Hit(UiHit.WorkAim, 0, RectAt(0f, cy, fw, fh), 1, false);   // bidikan mengikuti mouse
                    // Sprite berpivot bawah: geser turun setengah tinggi agar tengahnya tepat di titik sasaran.
                    const float ts = 1.1f;
                    float half = _wgTarget.sprite != null ? _wgTarget.sprite.bounds.size.y * 0.5f * ts : 0.5f;
                    _wgTarget.transform.localPosition = new Vector3(-fw * 0.5f + _wgTarget01.x * fw, cy - fh * 0.5f + _wgTarget01.y * fh - half, 0f);
                    _wgTarget.transform.localScale = Vector3.one * ts;
                    _wgTarget.flipX = Mathf.Sin(Time.unscaledTime * 0.7f) < 0f;
                    _wgCross.transform.localPosition = new Vector3(-fw * 0.5f + _wgCross01.x * fw, cy - fh * 0.5f + _wgCross01.y * fh, 0f);
                    break;
                }
            }

            _wgFeedback.transform.localPosition = new Vector3(0f, -top + 2.35f, 0f);
            _wgStatus.transform.localPosition = new Vector3(0f, -top + 1.35f, 0f);
            _wgHint.transform.localPosition = new Vector3(0f, -top + 0.6f, 0f);
            FitHint(_wgHint, w - 1f);
        }
    }
}
