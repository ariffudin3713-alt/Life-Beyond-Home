using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Camera-relative HUD made entirely of generated sprites: clock/money/energy/affection,
    /// interaction prompts, dialogue box, menus, work minigame and title/ending overlays.
    /// </summary>
    public partial class LifeHud : MonoBehaviour
    {
        public static readonly Color Ink = new Color(0.98f, 0.96f, 0.88f);
        public static readonly Color Soft = new Color(0.78f, 0.75f, 0.86f);
        public static readonly Color Gold = new Color(1f, 0.84f, 0.40f);
        public static readonly Color Pink = new Color(1f, 0.63f, 0.76f);
        static readonly Color OutlineColor = new Color(0.06f, 0.05f, 0.10f);

        Camera _cam;
        Transform _root;
        Transform _hudGroup, _promptGroup, _talkGroup, _menuGroup, _titleGroup, _pauseGroup,
            _workGroup, _endGroup, _starGroup;

        SpriteRenderer _nightDim, _dim, _fade;
        float _nightAmount;

        const int StarCount = 46;
        readonly SpriteRenderer[] _stars = new SpriteRenderer[StarCount];
        readonly Vector2[] _starSpots = new Vector2[StarCount];
        readonly float[] _starPhases = new float[StarCount];

        SpriteRenderer _statsPanel, _barsPanel;
        Transform _statsCorner, _barsCorner;
        /// <summary>Skala panel status kiri atas & bar kanan atas (1 = ukuran asli).</summary>
        const float CornerHudScale = 0.75f;
        SpriteRenderer _statsRule1, _statsRule2, _questLabel, _dateText;
        int _dateDay = -1, _dateChars = -1;   // tanggal kalender terakhir yang digambar
        SpriteRenderer _dayText, _timeText, _moneyText, _questText, _questText2, _phaseText, _hungerText;
        SpriteRenderer _coinIcon, _clockIcon, _boltIcon, _heartIcon;
        SpriteRenderer _energyBar, _energyFill, _affBar, _affFill, _affNameText;
        SpriteRenderer _seedIcon, _flowerIcon, _seedText, _flowerText;
        SpriteRenderer _toast;

        SpriteRenderer _promptPanel, _promptText;

        SpriteRenderer _talkPanel, _talkSpeaker, _talkHint;
        readonly SpriteRenderer[] _talkLines = new SpriteRenderer[4];

        string _lastDay = "", _lastTime = "", _lastMoney = "", _lastQuest = "", _lastPhase = "";
        string _lastAffName = "";
        int _lastEnergy = -1, _lastAffection = -1;
        float _energyRatio, _affectionRatio;
        float _halfW, _halfH;

        float _toastTimer;
        float _time;
        float _hudScale = 1f;   // skala akar HUD (zoom kamera x ukuran UI)

        /// <summary>Titik dunia -> koordinat lokal HUD.</summary>
        Vector2 WorldToHud(Vector2 world) => ((Vector2)world - (Vector2)_root.position) / _hudScale;

        /// <summary>Posisi pointer (layar) -> koordinat lokal HUD.</summary>
        Vector2 PointerToHud() => WorldToHud(_cam.ScreenToWorldPoint(LifeInput.PointerPosition));

        public void Init(Camera cam)
        {
            _cam = cam;
            _root = new GameObject("LifeHUD").transform;
            _root.SetParent(cam.transform, false);
            _root.localPosition = new Vector3(0f, 0f, 1f);

            BuildStars();

            _nightDim = Solid(_root, "NightTint", new Color(0f, 0f, 0f, 0f), 3000);
            _dim = Solid(_root, "Dim", new Color(0.02f, 0.02f, 0.06f, 0f), 3900);
            _fade = Solid(_root, "Fade", new Color(0f, 0f, 0f, 0f), 4900);

            BuildPlayHud();
            BuildPrompt();
            BuildDialogue();
            BuildPortraits();
            BuildMenu();
            BuildTitle();
            BuildHome();
            BuildPause();
            BuildWork();
            BuildWorkGame();
            BuildEnding();
            BuildJournal();
            BuildWorldUI();
            BuildTasks();
            BuildInventory();
            BuildTablet();
            BuildProfile();
            BuildGuide();
            BuildContacts();
            BuildWorldMap();
            BuildSettings();
            BuildAmount();
            BuildCloseButton();
            BuildToolbar();
            BuildPointer();

            HidePrompt();
            HideDialogue();
            HideMenu();
            HideTitle();
            HideHome();
            HidePause();
            HideWork();
            HideEnding();
            HideJournal();
            HideTasks();
            HideInventory();
            HideTablet();
            HideProfile();
            HideGuide();
            SetDim(0f);
        }

        void BuildStars()
        {
            _starGroup = Group("Stars");
            for (int i = 0; i < StarCount; i++)
            {
                _starSpots[i] = new Vector2(
                    ((i * 73 + 19) % 97) / 97f,
                    ((i * 47 + 31) % 89) / 89f);
                _starPhases[i] = ((i * 31) % 100) / 100f * Mathf.PI * 2f;
                var star = Solid(_starGroup, "Star" + i, new Color(1f, 0.97f, 0.86f, 0f), 3002);
                float size = 0.045f + (i % 4) * 0.018f;
                star.transform.localScale = new Vector3(size, size, 1f);
                _stars[i] = star;
            }
            _starGroup.gameObject.SetActive(false);
        }

        // ---------------------------------------------------------------- construction

        void BuildPlayHud()
        {
            _hudGroup = Group("Play");

            _statsPanel = Panel(_hudGroup, "StatsPanel", "lc_panel_soft", 3990);
            _statsPanel.color = new Color(1f, 1f, 1f, 0.92f);
            _barsPanel = Panel(_hudGroup, "BarsPanel", "lc_panel_soft", 3990);
            _barsPanel.color = new Color(1f, 1f, 1f, 0.92f);

            _dayText = Text(_hudGroup, "Day", "HARI 1", 1, Ink, new Vector2(0f, 0.5f));
            _timeText = Text(_hudGroup, "Time", "08:00", 1, Ink, new Vector2(0f, 0.5f));
            _moneyText = Text(_hudGroup, "Money", "150", 1, Gold, new Vector2(0f, 0.5f));
            _questText2 = Text(_hudGroup, "Quest2", " ", 1, Gold, new Vector2(0f, 0.5f));
            _questText = Text(_hudGroup, "Quest", " ", 1, Gold,
                new Vector2(0f, 0.5f));
            _phaseText = Text(_hudGroup, "Phase", "PAGI", 1, Soft, new Vector2(0f, 0.5f));
            _hungerText = Text(_hudGroup, "Hunger", " ", 1, Soft,
                new Vector2(1f, 0.5f));

            _coinIcon = Make(_hudGroup, PixelArt.Get("lc_icon_coin"), "Coin", 4001);
            _coinIcon.transform.localScale = Vector3.one * 1.15f;
            _clockIcon = Make(_hudGroup, PixelArt.Get("lc_icon_clock"), "Clock", 4001);
            _clockIcon.transform.localScale = Vector3.one * 1.05f;

            _energyBar = Solid(_hudGroup, "EnergyBar", new Color(0.09f, 0.07f, 0.12f, 1f), 3998);
            _energyFill = Solid(_hudGroup, "EnergyFill", new Color(0.99f, 0.80f, 0.30f, 1f), 3999);
            _affBar = Solid(_hudGroup, "AffBar", new Color(0.09f, 0.07f, 0.12f, 1f), 3998);
            _affFill = Solid(_hudGroup, "AffFill", new Color(1f, 0.40f, 0.58f, 1f), 3999);
            _boltIcon = Make(_hudGroup, PixelArt.Get("lc_icon_bolt"), "Bolt", 4001);
            _boltIcon.transform.localScale = Vector3.one * 1.05f;
            // Bar energi diganti LAPAR & HAUS (energi tetap terlihat di inventori).
            _boltIcon.gameObject.SetActive(false);
            _energyBar.gameObject.SetActive(false);
            _energyFill.gameObject.SetActive(false);
            BuildNeedBars();
            _heartIcon = Make(_hudGroup, PixelArt.Get("lc_icon_heart"), "Heart", 4001);
            _heartIcon.transform.localScale = Vector3.one * 1.0f;
            _affNameText = Text(_hudGroup, "AffName", "LV 1", 1, Pink,
                new Vector2(1f, 0.5f));

            _seedIcon = Make(_hudGroup, PixelArt.Get("lc_icon_seed"), "Seed", 4001);
            _seedIcon.transform.localScale = Vector3.one * 0.95f;
            _flowerIcon = Make(_hudGroup, PixelArt.Get("lc_icon_flower"), "Flower", 4001);
            _flowerIcon.transform.localScale = Vector3.one * 0.95f;
            _seedText = Text(_hudGroup, "SeedText", "0", 1, Ink, new Vector2(0f, 0.5f));
            _flowerText = Text(_hudGroup, "FlowerText", "0", 1, Ink, new Vector2(0f, 0.5f));

            // Ikon benih & bunga (sistem kebun lama) tidak dipakai di PULANG.
            _seedIcon.gameObject.SetActive(false);
            _flowerIcon.gameObject.SetActive(false);
            _seedText.gameObject.SetActive(false);
            _flowerText.gameObject.SetActive(false);

            // Panel kiri atas: garis pemisah antar bagian + label "MISI SAAT INI".
            _statsRule1 = Solid(_hudGroup, "StatsRule1", new Color(1f, 1f, 1f, 0.12f), 3995);
            _statsRule2 = Solid(_hudGroup, "StatsRule2", new Color(1f, 1f, 1f, 0.12f), 3995);
            _questLabel = Text(_hudGroup, "QuestLabel", "MISI SAAT INI", 1, Soft, new Vector2(0f, 0.5f));
            // Baris tanggal kalender di bawah HARI / jam: "SENIN, 5 JANUARI 2026".
            _dateText = Text(_hudGroup, "Date", " ", 1, Soft, new Vector2(0f, 0.5f));

            // Panel kiri atas & kanan atas digambar di grup pojok yang diperkecil (CornerHudScale).
            _statsCorner = new GameObject("StatsCorner").transform;
            _statsCorner.SetParent(_hudGroup, false);
            foreach (var sr in new[] { _statsPanel, _dayText, _timeText, _moneyText, _questText, _questText2,
                         _phaseText, _hungerText, _coinIcon, _clockIcon, _seedIcon, _flowerIcon, _seedText,
                         _flowerText, _statsRule1, _statsRule2, _questLabel, _dateText })
                sr.transform.SetParent(_statsCorner, false);
            _barsCorner = new GameObject("BarsCorner").transform;
            _barsCorner.SetParent(_hudGroup, false);
            foreach (var sr in new[] { _barsPanel, _energyBar, _energyFill, _boltIcon, _affBar, _affFill, _heartIcon,
                         _affNameText, _hungerIcon, _hungerBarBg, _hungerBarFill, _hungerLabel,
                         _thirstIcon, _thirstBarBg, _thirstBarFill, _thirstLabel })
                sr.transform.SetParent(_barsCorner, false);

            _toast = Text(_hudGroup, "Toast", " ", 2, Gold, new Vector2(0.5f, 0.5f), 4020);
            _toast.gameObject.SetActive(false);
            _hudGroup.gameObject.SetActive(false);
        }

        void BuildPrompt()
        {
            _promptGroup = Group("Prompt");
            _promptPanel = Panel(_promptGroup, "Panel", "lc_panel", 4005);
            _promptText = Text(_promptGroup, "Text", "E - BICARA", 1, Ink, new Vector2(0.5f, 0.5f), 4006);
            _promptGroup.gameObject.SetActive(false);
        }

        void BuildDialogue()
        {
            _talkGroup = Group("Dialogue");
            _talkPanel = Panel(_talkGroup, "Panel", "lc_panel_gold", 4010);
            _talkSpeaker = Text(_talkGroup, "Speaker", " ", 1, Gold, new Vector2(0f, 0.5f), 4012);
            for (int i = 0; i < _talkLines.Length; i++)
                _talkLines[i] = Text(_talkGroup, "Line" + i, " ", 1, Ink, new Vector2(0f, 0.5f), 4012);
            _talkHint = Text(_talkGroup, "Hint", "E / KLIK - LANJUT", 1, Soft, new Vector2(1f, 0.5f), 4012);
            _talkGroup.gameObject.SetActive(false);
        }

        // ---------------------------------------------------------------- public hud api

        public void SetGameplayVisible(bool visible)
        {
            if (_hudGroup != null) _hudGroup.gameObject.SetActive(visible);
            SetMinimapVisible(visible);
            if (!visible) HidePrompt();
        }

        public void Refresh(int day, int minutes, int money, int energy, int affection,
            string affectionName, string quest)
        {
            string dayText = "HARI " + day;
            int hh = minutes / 60;
            int mm = minutes % 60;
            string timeText = (hh < 10 ? "0" : "") + hh + ":" + (mm < 10 ? "0" : "") + mm;
            string moneyText = "RP" + money.ToString("N0", System.Globalization.CultureInfo.InvariantCulture).Replace(",", ".");
            string phase = PhaseFor(minutes);

            if (dayText != _lastDay)
            {
                _lastDay = dayText;
                SetText(_dayText, dayText, Ink, 1, new Vector2(0f, 0.5f));
            }
            if (day != _dateDay)
            {
                _dateDay = day;
                ApplyDate();
            }
            if (timeText != _lastTime)
            {
                _lastTime = timeText;
                SetText(_timeText, timeText, Ink, 1, new Vector2(0f, 0.5f));
            }
            if (moneyText != _lastMoney)
            {
                _lastMoney = moneyText;
                SetText(_moneyText, moneyText, Gold, 1, new Vector2(0f, 0.5f));
            }
            if (quest != _lastQuest)
            {
                _lastQuest = quest;
                SetQuestLines(quest);
            }
            if (phase != _lastPhase)
            {
                _lastPhase = phase;
                SetText(_phaseText, phase, Soft, 1, new Vector2(1f, 0.5f));
            }
            if (energy != _lastEnergy)
            {
                _lastEnergy = energy;
                _energyRatio = Mathf.Clamp01(energy / 100f);
            }
            if (affection != _lastAffection)
            {
                _lastAffection = affection;
                _affectionRatio = Mathf.Clamp01(affection / 100f);
            }
            string affName = (string.IsNullOrEmpty(affectionName) ? "CINTA" : affectionName) +
                             " " + affection + "%";
            if (affName != _lastAffName)
            {
                _lastAffName = affName;
                SetText(_affNameText, affName, Pink, 1, new Vector2(1f, 0.5f));
            }
        }

        string _questLabelText = "MISI SAAT INI", _questLabelShown;

        /// <summary>Judul bagian misi di panel kiri atas (mis. "MISI DARI BU SARI - OBAT UNTUK IBU").</summary>
        public void SetQuestLabel(string label)
        {
            _questLabelText = string.IsNullOrEmpty(label) ? "MISI SAAT INI" : label.ToUpperInvariant();
            ApplyQuestLabel();
        }

        void ApplyQuestLabel()
        {
            if (_questLabel == null) return;
            int max = Mathf.Max(8, Mathf.FloorToInt((_statsW - 1f) / PixelFont.CharW));
            string t = _questLabelText;
            if (t.Length > max) t = t.Substring(0, max - 1).TrimEnd() + ".";
            if (t == _questLabelShown) return;
            _questLabelShown = t;
            // Misi sampingan: hijau (warna penanda misi sampingan); misi utama: warna label biasa.
            bool side = _questLabelText != "MISI SAAT INI";
            SetText(_questLabel, t, side ? new Color(0.55f, 0.95f, 0.65f, 1f) : Soft, 1, new Vector2(0f, 0.5f));
        }

        void ApplyDate()
        {
            if (_dateText == null || _dateDay < 1) return;
            int chars = _dateChars > 0 ? _dateChars : 99;
            SetText(_dateText, GameCalendar.Fit(_dateDay, chars), Soft, 1, new Vector2(0f, 0.5f));
        }

        /// <summary>Lebar panel status kiri atas (unit) — teks misi dibungkus agar muat di dalamnya.</summary>
        const float StatsPanelMaxW = 13.4f;
        float _statsW = 12.4f;          // lebar panel saat ini (menyempit di layar sempit)
        int _questLineCount = 1;

        /// <summary>Bungkus teks misi per kata menjadi maks. 2 baris selebar panel; sisanya diberi "...".</summary>
        void SetQuestLines(string quest)
        {
            int maxChars = Mathf.FloorToInt((_statsW - 0.5f - 0.5f) / PixelFont.CharW);   // ±33 huruf
            var lines = new List<string>();
            string cur = "";
            foreach (string raw in (quest ?? " ").Split(' '))
            {
                if (raw.Length == 0) continue;
                string word = raw.Length > maxChars ? raw.Substring(0, maxChars) : raw;
                if (cur.Length == 0) cur = word;
                else if (cur.Length + 1 + word.Length <= maxChars) cur += " " + word;
                else { lines.Add(cur); cur = word; }
            }
            if (cur.Length > 0) lines.Add(cur);
            if (lines.Count == 0) lines.Add(" ");
            if (lines.Count > 2)
            {
                string second = lines[1];
                if (second.Length > maxChars - 3) second = second.Substring(0, maxChars - 3);
                lines[1] = second + "...";
                lines.RemoveRange(2, lines.Count - 2);
            }
            _questLineCount = lines.Count;
            SetText(_questText, lines[0], Gold, 1, new Vector2(0f, 0.5f));
            SetText(_questText2, lines.Count > 1 ? lines[1] : " ", Gold, 1, new Vector2(0f, 0.5f));
            _questText2.gameObject.SetActive(lines.Count > 1);
        }

        string _lastPrompt;

        string _lastSubline;

        /// <summary>Baris kecil kanan bawah panel status (mis. tabungan).</summary>
        public void SetSubline(string text)
        {
            if (text == _lastSubline) return;
            _lastSubline = text;
            ApplySubline();
        }

        bool _sublineShort;

        void ApplySubline()
        {
            string text = string.IsNullOrEmpty(_lastSubline) ? " " : _lastSubline;
            if (_sublineShort) text = text.Replace("TABUNGAN ", "TAB. ");
            SetText(_hungerText, text, Soft, 1, new Vector2(0f, 0.5f));   // rata kiri, di bawah uang
        }

        public void SetPrompt(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                HidePrompt();
                return;
            }
            if (text == _lastPrompt)
            {
                if (!_promptGroup.gameObject.activeSelf) _promptGroup.gameObject.SetActive(true);
                return;
            }
            _lastPrompt = text;
            SetText(_promptText, text, Ink, 1, new Vector2(0.5f, 0.5f));
            _promptGroup.gameObject.SetActive(true);
        }

        public void HidePrompt()
        {
            _lastPrompt = null;
            if (_promptGroup != null) _promptGroup.gameObject.SetActive(false);
        }

        public void ShowToast(string text)
        {
            // Huruf besar bila muat di layar; pesan panjang memakai huruf biasa (dipotong rapi bila perlu).
            string t = string.IsNullOrEmpty(text) ? " " : text;
            float room = Mathf.Max(6f, _halfW * 2f - 1.5f);
            int scale = t.Length * UiCharW * 2f <= room ? 2 : 1;
            SetText(_toast, scale == 2 ? t : CompactHint(t, CharsFor(room)), Gold, scale, new Vector2(0.5f, 0.5f));
            _toast.gameObject.SetActive(true);
            _toastTimer = 2.4f;
        }

        public void ShowDialogue(string speaker, string body)
        {
            SetText(_talkSpeaker, string.IsNullOrEmpty(speaker) ? " " : speaker, Gold, 1,
                new Vector2(0f, 0.5f));
            _talkBody = body ?? "";
            _talkChars = CharsFor(Mathf.Min(_halfW * 2f - 1.0f, 27f) - 1.1f);
            ApplyDialogueText();
            _talkGroup.gameObject.SetActive(true);
            HidePrompt();
        }

        string _talkBody = "";
        int _talkChars = 62, _talkLineCount;

        /// <summary>Bungkus isi dialog per kata sesuai lebar kotak (maks. 4 baris).</summary>
        void ApplyDialogueText()
        {
            var lines = new List<string>();
            string cur = "";
            foreach (string word in _talkBody.Split(' '))
            {
                if (word.Length == 0) continue;
                string w = word.Length > _talkChars ? word.Substring(0, _talkChars) : word;
                if (cur.Length == 0) cur = w;
                else if (cur.Length + 1 + w.Length <= _talkChars) cur += " " + w;
                else { lines.Add(cur); cur = w; }
            }
            if (cur.Length > 0) lines.Add(cur);
            int max = _talkLines.Length;
            if (lines.Count > max)
            {
                string last = lines[max - 1];
                lines[max - 1] = (last.Length + 3 > _talkChars ? last.Substring(0, Mathf.Max(1, _talkChars - 3)) : last) + "...";
                lines.RemoveRange(max, lines.Count - max);
            }
            _talkLineCount = lines.Count;
            for (int i = 0; i < _talkLines.Length; i++)
            {
                string line = i < lines.Count ? lines[i] : " ";
                SetText(_talkLines[i], line, Ink, 1, new Vector2(0f, 0.5f));
            }
        }

        public void HideDialogue()
        {
            if (_talkGroup != null) _talkGroup.gameObject.SetActive(false);
            ClearDialoguePortraits();
        }

        public void SetNight(Color color, float amount)
        {
            _nightAmount = Mathf.Clamp01(amount);
            if (_nightDim != null)
            {
                _nightDim.color = color;
                _nightDim.gameObject.SetActive(color.a > 0.005f);
            }
            if (_starGroup != null)
                _starGroup.gameObject.SetActive(_nightAmount > 0.04f);
        }

        /// <summary>Uppercase time-of-day label used by the HUD and the journal.</summary>
        public static string PhaseFor(int minutes)
        {
            int m = ((minutes % 1440) + 1440) % 1440;
            if (m < 5 * 60) return "DINI HARI";
            if (m < 11 * 60) return "PAGI";
            if (m < 15 * 60) return "SIANG";
            if (m < 18 * 60) return "SORE";
            if (m < 20 * 60) return "SENJA";
            return "MALAM";
        }

        public void SetFade(float alpha)
        {
            if (_fade == null) return;
            Color c = _fade.color;
            c.a = Mathf.Clamp01(alpha);
            _fade.color = c;
            _fade.gameObject.SetActive(c.a > 0.001f);
        }

        internal void SetDim(float alpha)
        {
            Color c = _dim.color;
            c.a = Mathf.Clamp01(alpha);
            _dim.color = c;
            _dim.gameObject.SetActive(c.a > 0.001f);
        }

        // ---------------------------------------------------------------- runtime

        void Update()
        {
            if (_cam == null) return;
            _time += Time.unscaledDeltaTime;
            Layout();

            // Denyut lembut dan lambat untuk teks petunjuk (tidak berkedip tajam agar mata tidak lelah).
            float pulse = 0.78f + 0.22f * (Mathf.Sin(_time * 2.4f) * 0.5f + 0.5f);
            if (_talkGroup.gameObject.activeSelf) SetAlpha(_talkHint, pulse);
            if (_menuGroup.gameObject.activeSelf) SetAlpha(_menuHint, pulse);
            if (_promptGroup.gameObject.activeSelf) SetAlpha(_promptText, 0.85f + 0.15f * pulse);
            if (_titlePrompt != null && _titleGroup.gameObject.activeSelf) SetAlpha(_titlePrompt, pulse);
            if (_pausePrompt != null && _pauseGroup.gameObject.activeSelf) SetAlpha(_pausePrompt, pulse);
            if (_workPrompt != null && _workGroup.gameObject.activeSelf) SetAlpha(_workPrompt, pulse);
            if (_endPrompt != null && _endGroup.gameObject.activeSelf) SetAlpha(_endPrompt, pulse);
            if (_journalHint != null && _journalGroup != null && _journalGroup.gameObject.activeSelf)
                SetAlpha(_journalHint, pulse);
            if (TasksVisible) SetAlpha(_taskHint, pulse);

            if (_starGroup != null && _starGroup.gameObject.activeSelf)
            {
                for (int i = 0; i < _stars.Length; i++)
                {
                    if (_stars[i] == null) continue;
                    float twinkle = 0.55f + 0.45f *
                        (Mathf.Sin(_time * 1.6f + _starPhases[i]) * 0.5f + 0.5f);
                    Color c = _stars[i].color;
                    c.a = _nightAmount * (0.35f + 0.65f * twinkle);
                    _stars[i].color = c;
                }
            }

            if (_toastTimer > 0f)
            {
                _toastTimer -= Time.unscaledDeltaTime;
                SetAlpha(_toast, Mathf.Clamp01(_toastTimer / 0.5f));
                if (_toastTimer <= 0f) _toast.gameObject.SetActive(false);
            }
        }

        void Layout()
        {
            // HUD mengikuti layar, bukan zoom kamera: skala akar = (zoom / ukuran dasar) x ukuran UI.
            // Area tata letak (dalam unit HUD) = ukuran dasar / ukuran UI, jadi UI lebih besar = ruang lebih sempit.
            float scale = _cam.orthographicSize / LifeGraphics.BaseCameraSize * LifeGraphics.UiScale;
            if (!Mathf.Approximately(_root.localScale.x, scale)) _root.localScale = new Vector3(scale, scale, 1f);
            _hudScale = scale;
            float halfH = _cam.orthographicSize / scale;
            float halfW = halfH * _cam.aspect;
            _halfW = halfW;
            _halfH = halfH;
            BeginPointerFrame();

            Fill(_nightDim, halfW, halfH);
            Fill(_dim, halfW, halfH);
            Fill(_fade, halfW, halfH);

            for (int i = 0; i < _stars.Length; i++)
            {
                if (_stars[i] == null) continue;
                float sx = Mathf.Lerp(-halfW + 0.3f, halfW - 0.3f, _starSpots[i].x);
                float sy = Mathf.Lerp(halfH * 0.28f, halfH - 0.18f, _starSpots[i].y);
                _stars[i].transform.localPosition = new Vector3(sx, sy, 0f);
            }

            // ---------------- top left stats
            // Panel status tiga bagian, dipisah garis tipis:
            //   1) HARI n   [jam] 08:30 ........ PAGI
            //   2) MISI SAAT INI / teks misi (maks. 2 baris)
            //   3) [koin] RP245.000 ...... TABUNGAN RP120.000
            // Kedua panel atas ada di grup pojok yang diperkecil; koordinat di bawah ini lokal terhadap pojoknya.
            float cs = CornerHudScale;
            float cornerTop = halfH - 0.28f, cornerLeft = -halfW + 0.28f, cornerRight = halfW - 0.28f;
            _statsCorner.localPosition = new Vector3(cornerLeft, cornerTop, 0f);
            _barsCorner.localPosition = new Vector3(cornerRight, cornerTop, 0f);
            if (!Mathf.Approximately(_statsCorner.localScale.x, cs))
            {
                _statsCorner.localScale = new Vector3(cs, cs, 1f);
                _barsCorner.localScale = new Vector3(cs, cs, 1f);
            }
            float top = 0f;
            float left = 0f;
            float statsW = Mathf.Clamp((halfW * 2f - 0.56f - 0.4f) / cs - 8.4f, 10.5f, StatsPanelMaxW);
            if (Mathf.Abs(statsW - _statsW) > 0.01f) { _statsW = statsW; SetQuestLines(_lastQuest); ApplyQuestLabel(); }
            // Jarak antarbaris >= 0,62 (tinggi huruf 0,56) agar tidak ada baris yang menempel.
            const float statsH = 5.37f;   // + baris tanggal & baris tabungan
            const float dRow = 0.62f;     // baris tanggal menggeser bagian bawah panel
            float padL = left + 0.5f, padR = left + _statsW - 0.5f;
            SetRect(_statsPanel, left + _statsW * 0.5f, top - statsH * 0.5f, _statsW, statsH);

            float row1 = top - 0.6f;
            _dayText.transform.localPosition = new Vector3(padL, row1, 0f);
            // Jam mengikuti lebar teks hari ("HARI 9" / "HARI 120") agar tidak pernah menempel.
            float dayW = _dayText.sprite != null ? _dayText.sprite.bounds.size.x : 3f;
            float clockX = padL + Mathf.Max(3.35f, dayW + 0.55f);
            _clockIcon.transform.localPosition = new Vector3(clockX, row1, 0f);
            _timeText.transform.localPosition = new Vector3(clockX + 0.6f, row1, 0f);
            _phaseText.transform.localPosition = new Vector3(padR, row1, 0f);
            // Baris 2: tanggal kalender (dipendekkan bila panel sempit).
            _dateText.transform.localPosition = new Vector3(padL, row1 - dRow, 0f);
            int dateChars = Mathf.FloorToInt((padR - padL) / PixelFont.CharW);
            if (dateChars != _dateChars) { _dateChars = dateChars; ApplyDate(); }
            SetSolid(_statsRule1, left + _statsW * 0.5f, top - 1.1f - dRow, _statsW - 0.7f, 0.05f);

            _questLabel.transform.localPosition = new Vector3(padL, top - 1.52f - dRow, 0f);
            _questText.transform.localPosition = new Vector3(padL, top - 2.14f - dRow, 0f);
            _questText2.transform.localPosition = new Vector3(padL, top - 2.76f - dRow, 0f);
            SetSolid(_statsRule2, left + _statsW * 0.5f, top - 3.2f - dRow, _statsW - 0.7f, 0.05f);

            float row3 = top - 3.62f - dRow;
            _coinIcon.transform.localPosition = new Vector3(padL + 0.3f, row3, 0f);
            _moneyText.transform.localPosition = new Vector3(padL + 0.75f, row3, 0f);
            // Tabungan di baris sendiri, tepat di bawah uang & sejajar dengan teks uang (tidak berdempetan).
            float row4 = row3 - 0.64f;
            _hungerText.transform.localPosition = new Vector3(padL + 0.75f, row4, 0f);
            int subChars = Mathf.FloorToInt((padR - padL - 0.75f) / PixelFont.CharW);
            bool shortSub = (_lastSubline ?? "").Length > subChars;   // layar sangat sempit: "TABUNGAN" -> "TAB."
            if (shortSub != _sublineShort) { _sublineShort = shortSub; ApplySubline(); }

            _seedIcon.transform.localPosition = new Vector3(left + 0.62f, top - 2.45f, 0f);
            _seedText.transform.localPosition = new Vector3(left + 1.02f, top - 2.45f, 0f);
            _flowerIcon.transform.localPosition = new Vector3(left + 2.55f, top - 2.45f, 0f);
            _flowerText.transform.localPosition = new Vector3(left + 2.95f, top - 2.45f, 0f);

            // ---------------- top right bars
            float right = 0f;
            float barsLeft = right - 8.4f;
            const float barsH = 3.5f;
            SetRect(_barsPanel, right - 4.2f, top - barsH * 0.5f, 8.4f, barsH);

            float barX = barsLeft + 1.28f;
            float barW = 6.62f;
            LayoutNeedBars(barsLeft, right, top);   // baris 1-2: LAPAR, HAUS
            SetRow(left: barX, y: top - 2.12f, width: barW, height: 0.34f,
                icon: _heartIcon, bg: _affBar, fill: _affFill, ratio: _affectionRatio,
                iconX: barsLeft + 0.52f);
            _affNameText.transform.localPosition =
                new Vector3(right - 0.55f, top - 2.82f, 0f);

            // Panel status tidak bisa diklik, tetapi klik di atasnya tidak boleh menjadi "jalan ke sini".
            if (PlayHitsOn)
            {
                Hit(UiHit.Blocker, 0, new Rect(cornerLeft, cornerTop - statsH * cs, _statsW * cs, statsH * cs), 0, false);
                Hit(UiHit.Blocker, 1, new Rect(cornerRight - 8.4f * cs, cornerTop - barsH * cs, 8.4f * cs, barsH * cs), 0, false);
            }

            // ---------------- toast
            _toast.transform.localPosition = new Vector3(0f, halfH * 0.42f, 0f);

            // ---------------- prompt
            if (_promptText.sprite != null)
            {
                float pw = Mathf.Max(2.2f, _promptText.sprite.bounds.size.x + 0.9f);
                // Di tengah bawah bila muat di antara minimap (kiri) dan tombol kanan bawah;
                // bila tidak, naik ke atas keduanya agar tidak menumpuk.
                float gapL = Mathf.Max(-halfW + 0.28f, _miniEdgeRight) + 0.2f;
                float gapR = Mathf.Min(halfW - 0.28f, _toolEdgeLeft) - 0.2f;
                float py = -halfH + 0.62f;
                if (pw * 0.5f > Mathf.Min(-gapL, gapR))
                    py = Mathf.Max(_miniEdgeTop, _toolEdgeTop, -halfH + 0.28f) + 0.55f;
                SetRect(_promptPanel, 0f, py, pw, 0.68f);
                _promptText.transform.localPosition = new Vector3(0f, py, 0f);
            }

            // ---------------- dialogue
            // Teks dibungkus sesuai lebar kotak yang sebenarnya (tidak pernah keluar kotak);
            // bila perlu 4 baris, kotak meninggi sedikit agar tidak ada kalimat yang terpotong.
            float dw = Mathf.Min(halfW * 2f - 1.0f, 27f);
            int talkChars = CharsFor(dw - 1.1f);
            if (talkChars != _talkChars) { _talkChars = talkChars; ApplyDialogueText(); }
            bool four = _talkLineCount > 3;
            float lineStep = four ? 0.64f : 0.72f;
            float dh = four ? 3.55f + lineStep : 3.55f;
            float dcy = -halfH + dh * 0.5f + 0.30f;
            SetRect(_talkPanel, 0f, dcy, dw, dh);
            float dlx = -dw * 0.5f + 0.55f;
            _talkSpeaker.transform.localPosition =
                new Vector3(dlx, dcy + dh * 0.5f - 0.45f, 0f);
            float firstLine = dcy + dh * 0.5f - 1.255f;
            for (int i = 0; i < _talkLines.Length; i++)
                _talkLines[i].transform.localPosition =
                    new Vector3(dlx, firstLine - i * lineStep, 0f);
            _talkHint.transform.localPosition =
                new Vector3(dw * 0.5f - 0.55f, dcy - dh * 0.5f + 0.34f, 0f);
            LayoutPortraits(dw, dcy, dh);

            LayoutMenu(halfW, halfH);
            LayoutWork(halfW, halfH);
            LayoutJournal(halfW, halfH);
            LayoutHome(halfW, halfH);
            LayoutTasks(halfW, halfH);
            LayoutInventory(halfW, halfH);
            LayoutTablet(halfW, halfH);
            LayoutProfile(halfW, halfH);
            LayoutContacts(halfW, halfH);
            LayoutWorldMap(halfW, halfH);
            LayoutSettings(halfW, halfH);
            LayoutAmount(halfW, halfH);
            LayoutGuideButton(halfW, halfH);
            LayoutToolbar(halfW, halfH);
            LayoutGuide(halfW, halfH);
            LayoutWorldUI(halfW, halfH);
            LayoutOverlays(halfW, halfH);
            if (_pauseGroup != null && _pauseGroup.gameObject.activeSelf)
            {
                CloseAt(7.5f, 3.6f, 3);
                // Baris jeda bisa diklik: 0 = lanjut, 1 = simpan & menu utama, 2 = suara.
                Hit(UiHit.Pause, 0, RectAt(0f, 0.55f, 11f, 0.75f), 3);
                Hit(UiHit.Pause, 1, RectAt(0f, -0.85f, 11f, 0.75f), 3);
                Hit(UiHit.Pause, 2, RectAt(0f, -1.65f, 11f, 0.75f), 3);
            }
            LayoutCloseButton();
            EndPointerFrame();
        }

        void SetRow(float left, float y, float width, float height,
            SpriteRenderer icon, SpriteRenderer bg, SpriteRenderer fill, float ratio, float iconX)
        {
            icon.transform.localPosition = new Vector3(iconX, y, 0f);
            SetSolid(bg, left + width * 0.5f, y, width, height);
            float inner = Mathf.Max(0f, width - 0.12f);
            float fillW = inner * Mathf.Clamp01(ratio);
            SetSolid(fill, left + 0.06f + fillW * 0.5f, y, fillW, height - 0.12f);
        }

        // ---------------------------------------------------------------- helpers

        Transform Group(string name)
        {
            var t = new GameObject(name).transform;
            t.SetParent(_root, false);
            t.gameObject.SetActive(false);
            return t;
        }

        internal static SpriteRenderer Make(Transform parent, Sprite sprite, string name, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sharedMaterial = PixelArt.SpriteMaterial;
            sr.sortingOrder = order;
            return sr;
        }

        internal static SpriteRenderer Panel(Transform parent, string name, string spriteKey, int order)
        {
            var sr = Make(parent, PixelArt.Get(spriteKey), name, order);
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = new Vector2(2f, 2f);
            return sr;
        }

        internal static SpriteRenderer Solid(Transform parent, string name, Color color, int order)
        {
            var sr = Make(parent, PixelArt.SolidUnit, name, order);
            sr.color = color;
            return sr;
        }

        internal static SpriteRenderer Text(Transform parent, string name, string text, int scale,
            Color color, Vector2 pivot, int order = 4010)
        {
            var sr = Make(parent, PixelFont.Render(text, color, OutlineColor, scale, pivot),
                name, order);
            return sr;
        }

        internal static void SetText(SpriteRenderer sr, string text, Color color, int scale,
            Vector2 pivot)
        {
            if (sr == null) return;
            sr.sprite = PixelFont.Render(text, color, OutlineColor, scale, pivot);
        }

        internal static void SetAlpha(SpriteRenderer sr, float alpha)
        {
            if (sr == null) return;
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }

        internal static void SetRect(SpriteRenderer sr, float cx, float cy, float w, float h)
        {
            if (sr == null) return;
            sr.transform.localPosition = new Vector3(cx, cy, 0f);
            sr.size = new Vector2(w, h);
        }

        internal static void SetSolid(SpriteRenderer sr, float cx, float cy, float w, float h)
        {
            if (sr == null) return;
            sr.transform.localPosition = new Vector3(cx, cy, 0f);
            sr.transform.localScale = new Vector3(Mathf.Max(0f, w), Mathf.Max(0f, h), 1f);
        }

        internal static void Fill(SpriteRenderer sr, float halfW, float halfH)
        {
            if (sr == null) return;
            sr.transform.localPosition = new Vector3(0f, 0f, 0f);
            sr.transform.localScale = new Vector3(halfW * 2f, halfH * 2f, 1f);
        }

        public static string[] Wrap(string text, int maxChars)
        {
            var lines = new List<string>();
            if (string.IsNullOrEmpty(text)) return new[] { " " };
            string[] words = text.Split(' ');
            string cur = "";
            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];
                if (word.Length == 0) continue;
                if (cur.Length == 0) cur = word;
                else if (cur.Length + 1 + word.Length <= maxChars) cur += " " + word;
                else
                {
                    lines.Add(cur);
                    cur = word;
                }
            }
            if (cur.Length > 0) lines.Add(cur);
            if (lines.Count == 0) lines.Add(" ");
            if (lines.Count > 3)
            {
                lines[2] = lines[2] + "...";
                lines.RemoveRange(3, lines.Count - 3);
            }
            return lines.ToArray();
        }
    }
}
