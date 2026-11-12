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
        SpriteRenderer _dayText, _timeText, _moneyText, _questText, _questText2, _phaseText, _hungerText;
        SpriteRenderer _coinIcon, _clockIcon, _boltIcon, _heartIcon;
        SpriteRenderer _energyBar, _energyFill, _affBar, _affFill, _affNameText;
        SpriteRenderer _seedIcon, _flowerIcon, _seedText, _flowerText;
        SpriteRenderer _toast;

        SpriteRenderer _promptPanel, _promptText;

        SpriteRenderer _talkPanel, _talkSpeaker, _talkHint;
        readonly SpriteRenderer[] _talkLines = new SpriteRenderer[3];

        string _lastDay = "", _lastTime = "", _lastMoney = "", _lastQuest = "", _lastPhase = "";
        string _lastAffName = "";
        int _lastEnergy = -1, _lastAffection = -1;
        float _energyRatio, _affectionRatio;
        float _halfW, _halfH;

        float _toastTimer;
        float _time;

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
            BuildMenu();
            BuildTitle();
            BuildHome();
            BuildPause();
            BuildWork();
            BuildEnding();
            BuildJournal();
            BuildWorldUI();
            BuildQuestCompass();
            BuildTasks();
            BuildInventory();
            BuildTablet();
            BuildProfile();
            BuildGuide();

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
            _talkHint = Text(_talkGroup, "Hint", "E - LANJUT", 1, Soft, new Vector2(1f, 0.5f), 4012);
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
            string moneyText = money.ToString();
            string phase = PhaseFor(minutes);

            if (dayText != _lastDay)
            {
                _lastDay = dayText;
                SetText(_dayText, dayText, Ink, 1, new Vector2(0f, 0.5f));
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
                SetText(_phaseText, phase, Soft, 1, new Vector2(0f, 0.5f));
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

        /// <summary>Lebar panel status kiri atas (unit) — teks misi dibungkus agar muat di dalamnya.</summary>
        const float StatsPanelW = 12.4f;
        int _questLineCount = 1;

        /// <summary>Bungkus teks misi per kata menjadi maks. 2 baris selebar panel; sisanya diberi "...".</summary>
        void SetQuestLines(string quest)
        {
            int maxChars = Mathf.FloorToInt((StatsPanelW - 0.45f - 0.4f) / (6f / 16f));   // ±28 huruf
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
            SetText(_hungerText, string.IsNullOrEmpty(text) ? " " : text, Soft, 1, new Vector2(1f, 0.5f));
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
            SetText(_toast, text, Gold, 2, new Vector2(0.5f, 0.5f));
            _toast.gameObject.SetActive(true);
            _toastTimer = 2.4f;
        }

        public void ShowDialogue(string speaker, string body)
        {
            SetText(_talkSpeaker, string.IsNullOrEmpty(speaker) ? " " : speaker, Gold, 1,
                new Vector2(0f, 0.5f));
            string[] lines = Wrap(body, 62);
            for (int i = 0; i < _talkLines.Length; i++)
            {
                string line = i < lines.Length ? lines[i] : " ";
                SetText(_talkLines[i], string.IsNullOrEmpty(line) ? " " : line, Ink, 1,
                    new Vector2(0f, 0.5f));
            }
            _talkGroup.gameObject.SetActive(true);
            HidePrompt();
        }

        public void HideDialogue()
        {
            if (_talkGroup != null) _talkGroup.gameObject.SetActive(false);
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

            float pulse = 0.55f + 0.45f * (Mathf.Sin(_time * 4.2f) * 0.5f + 0.5f);
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
            float halfH = _cam.orthographicSize;
            float halfW = halfH * _cam.aspect;
            _halfW = halfW;
            _halfH = halfH;

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
            float top = halfH - 0.28f;
            float left = -halfW + 0.28f;
            SetRect(_statsPanel, left + StatsPanelW * 0.5f, top - 1.55f, StatsPanelW, 3.1f);

            _dayText.transform.localPosition = new Vector3(left + 0.45f, top - 0.55f, 0f);
            _clockIcon.transform.localPosition = new Vector3(left + 4.3f, top - 0.55f, 0f);
            _timeText.transform.localPosition = new Vector3(left + 4.72f, top - 0.55f, 0f);
            _coinIcon.transform.localPosition = new Vector3(left + 8.05f, top - 0.55f, 0f);
            _moneyText.transform.localPosition = new Vector3(left + 8.45f, top - 0.55f, 0f);
            // Teks misi: 1 baris di tengah, atau 2 baris bila panjang (tetap di dalam panel).
            bool twoLines = _questLineCount > 1;
            _questText.transform.localPosition = new Vector3(left + 0.45f, top - (twoLines ? 1.27f : 1.55f), 0f);
            _questText2.transform.localPosition = new Vector3(left + 0.45f, top - 1.82f, 0f);
            _phaseText.transform.localPosition = new Vector3(left + 5.15f, top - 2.45f, 0f);
            _hungerText.transform.localPosition = new Vector3(left + 12.15f, top - 2.45f, 0f);

            _seedIcon.transform.localPosition = new Vector3(left + 0.62f, top - 2.45f, 0f);
            _seedText.transform.localPosition = new Vector3(left + 1.02f, top - 2.45f, 0f);
            _flowerIcon.transform.localPosition = new Vector3(left + 2.55f, top - 2.45f, 0f);
            _flowerText.transform.localPosition = new Vector3(left + 2.95f, top - 2.45f, 0f);

            // ---------------- top right bars
            float right = halfW - 0.28f;
            float barsLeft = right - 8.4f;
            SetRect(_barsPanel, right - 4.2f, top - 1.38f, 8.4f, 2.76f);

            float barX = barsLeft + 1.28f;
            float barW = 6.62f;
            SetRow(left: barX, y: top - 0.62f, width: barW, height: 0.34f,
                icon: _boltIcon, bg: _energyBar, fill: _energyFill, ratio: _energyRatio,
                iconX: barsLeft + 0.52f);
            SetRow(left: barX, y: top - 1.50f, width: barW, height: 0.34f,
                icon: _heartIcon, bg: _affBar, fill: _affFill, ratio: _affectionRatio,
                iconX: barsLeft + 0.52f);
            _affNameText.transform.localPosition =
                new Vector3(right - 0.55f, top - 2.12f, 0f);

            // ---------------- toast
            _toast.transform.localPosition = new Vector3(0f, halfH * 0.42f, 0f);

            // ---------------- prompt
            if (_promptText.sprite != null)
            {
                float pw = _promptText.sprite.bounds.size.x + 0.9f;
                SetRect(_promptPanel, 0f, -halfH + 0.62f, Mathf.Max(2.2f, pw), 0.68f);
                _promptText.transform.localPosition = new Vector3(0f, -halfH + 0.62f, 0f);
            }

            // ---------------- dialogue
            float dw = Mathf.Min(halfW * 2f - 1.0f, 27f);
            float dh = 3.55f;
            float dcy = -halfH + dh * 0.5f + 0.30f;
            SetRect(_talkPanel, 0f, dcy, dw, dh);
            float dlx = -dw * 0.5f + 0.55f;
            _talkSpeaker.transform.localPosition =
                new Vector3(dlx, dcy + dh * 0.5f - 0.45f, 0f);
            for (int i = 0; i < _talkLines.Length; i++)
                _talkLines[i].transform.localPosition =
                    new Vector3(dlx, dcy + 0.52f - i * 0.72f, 0f);
            _talkHint.transform.localPosition =
                new Vector3(dw * 0.5f - 0.55f, dcy - dh * 0.5f + 0.34f, 0f);

            LayoutMenu(halfW, halfH);
            LayoutWork(halfW, halfH);
            LayoutJournal(halfW, halfH);
            LayoutHome(halfW, halfH);
            LayoutTasks(halfW, halfH);
            LayoutInventory(halfW, halfH);
            LayoutTablet(halfW, halfH);
            LayoutProfile(halfW, halfH);
            LayoutGuideButton(halfW, halfH);
            LayoutGuide(halfW, halfH);
            LayoutWorldUI(halfW, halfH);
            LayoutOverlays(halfW, halfH);
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
