using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    public partial class LifeHud
    {
        // ---------------------------------------------------------------- menu fields

        SpriteRenderer _menuPanel, _menuTitle, _menuHint;
        readonly SpriteRenderer[] _menuOptions = new SpriteRenderer[9];
        readonly List<string> _menuOptionList = new List<string>();
        int _menuCount;
        int _menuSelected;

        // ---------------------------------------------------------------- title fields

        SpriteRenderer _titlePanel, _titleName, _titleSub, _titleInfo, _titlePrompt;
        SpriteRenderer _titleHint1, _titleHint2, _titleHint3, _titleHint4;

        // ---------------------------------------------------------------- pause fields

        SpriteRenderer _pausePanel, _pauseTitle, _pausePrompt, _pauseHint, _pauseMute;

        // ---------------------------------------------------------------- work fields

        SpriteRenderer _workPanel, _workTitle, _workOrder, _workHits, _workPrompt;
        SpriteRenderer _workBarBg, _workZone, _workMarker;
        float _workMarker01, _workZoneCenter, _workZoneHalf;
        int _workServed, _workHitsCount, _workTotal = 3;
        string _lastWorkOrder = "", _lastWorkHits = "";

        // ---------------------------------------------------------------- ending fields

        SpriteRenderer _endPanel, _endTitle, _endSub, _endLine1, _endLine2, _endLine3, _endPrompt;

        // ---------------------------------------------------------------- journal fields

        Transform _journalGroup;
        SpriteRenderer _journalPanel, _journalHeader, _journalChapter, _journalTitle;
        SpriteRenderer _journalObjectiveA, _journalObjectiveB, _journalHint;
        readonly SpriteRenderer[] _journalDetails = new SpriteRenderer[18];

        // ---------------------------------------------------------------- menu

        /// <summary>Simbol bintang di awal label menandai pilihan yang melanjutkan misi.</summary>
        public const char QuestMark = '★';
        static readonly Color QuestColor = new Color(1f, 0.62f, 0.30f);

        void BuildMenu()
        {
            _menuGroup = Group("Menu");
            _menuPanel = Panel(_menuGroup, "Panel", "lc_panel", 4010);
            _menuTitle = Text(_menuGroup, "Title", " ", 1, Gold, new Vector2(0f, 0.5f), 4012);
            for (int i = 0; i < _menuOptions.Length; i++)
                _menuOptions[i] = Text(_menuGroup, "Option" + i, " ", 1, Ink,
                    new Vector2(0f, 0.5f), 4012);
            _menuHint = Text(_menuGroup, "Hint", "E - OK   ESC - BATAL", 1, Soft,
                new Vector2(0.5f, 0.5f), 4012);
            _menuGroup.gameObject.SetActive(false);
        }

        public void ShowMenu(string title, List<string> options, int selected)
        {
            _menuOptionList.Clear();
            if (options != null) _menuOptionList.AddRange(options);
            _menuCount = Mathf.Min(_menuOptionList.Count, _menuOptions.Length);
            _menuSelected = Mathf.Clamp(selected, 0, Mathf.Max(0, _menuCount - 1));
            SetText(_menuTitle, title, Gold, 1, new Vector2(0f, 0.5f));
            ApplyMenu();
            _menuGroup.gameObject.SetActive(true);
            HidePrompt();
        }

        public void SetMenuSelection(int selected)
        {
            if (_menuCount <= 0) return;
            _menuSelected = Mathf.Clamp(selected, 0, _menuCount - 1);
            ApplyMenu();
        }

        void ApplyMenu()
        {
            for (int i = 0; i < _menuOptions.Length; i++)
            {
                bool show = i < _menuCount;
                _menuOptions[i].gameObject.SetActive(show);
                if (!show) continue;
                string label = (i == _menuSelected ? "> " : "  ") + _menuOptionList[i];
                bool quest = _menuOptionList[i].Length > 0 && _menuOptionList[i][0] == QuestMark;
                Color color = i == _menuSelected ? Gold : (quest ? QuestColor : Ink);
                SetText(_menuOptions[i], label, color, 1, new Vector2(0f, 0.5f));
            }
        }

        public void HideMenu()
        {
            if (_menuGroup != null) _menuGroup.gameObject.SetActive(false);
            _menuOptionList.Clear();
            _menuCount = 0;
        }

        void LayoutMenu(float halfW, float halfH)
        {
            if (_menuGroup == null || !_menuGroup.gameObject.activeSelf) return;
            // Panel pilihan berada di tengah layar; lebarnya mengikuti teks terpanjang.
            float widest = _menuTitle.sprite != null ? _menuTitle.sprite.bounds.size.x : 0f;
            for (int i = 0; i < _menuCount; i++)
                if (_menuOptions[i].sprite != null)
                    widest = Mathf.Max(widest, _menuOptions[i].sprite.bounds.size.x);
            float w = Mathf.Min(Mathf.Max(10.8f, widest + 1.3f), halfW * 2f - 1f);
            float h = 1.25f + _menuCount * 0.72f + 0.62f;
            float cx = 0f;
            float cy = 0f;
            SetRect(_menuPanel, cx, cy, w, h);
            float lx = cx - w * 0.5f + 0.55f;
            _menuTitle.transform.localPosition = new Vector3(lx, cy + h * 0.5f - 0.5f, 0f);
            for (int i = 0; i < _menuOptions.Length; i++)
                _menuOptions[i].transform.localPosition =
                    new Vector3(lx, cy + h * 0.5f - 1.18f - i * 0.72f, 0f);
            _menuHint.transform.localPosition = new Vector3(cx, cy - h * 0.5f + 0.36f, 0f);
        }

        // ---------------------------------------------------------------- title

        void BuildTitle()
        {
            _titleGroup = Group("Title");
            _titlePanel = Panel(_titleGroup, "Panel", "lc_panel_gold", 4000);
            _titlePanel.color = new Color(1f, 1f, 1f, 0.97f);
            SetRect(_titlePanel, 0f, -0.1f, 20f, 10.6f);

            _titleName = Text(_titleGroup, "Name", "PULANG", 4, Ink, new Vector2(0.5f, 0.5f), 4005);
            _titleSub = Text(_titleGroup, "Sub", "LIFE BEYOND HOME", 1, Pink, new Vector2(0.5f, 0.5f), 4005);
            _titleInfo = Text(_titleGroup, "Info", " ", 1, Soft, new Vector2(0.5f, 0.5f), 4005);
            _titlePrompt = Text(_titleGroup, "Prompt", "ENTER - MULAI", 2, Gold, new Vector2(0.5f, 0.5f), 4006);
            _titleHint1 = Text(_titleGroup, "Hint1", "WASD - JALAN   SHIFT - LARI", 1, Soft, new Vector2(0.5f, 0.5f), 4005);
            _titleHint2 = Text(_titleGroup, "Hint2", "E - BICARA  J - JURNAL  T - TUGAS", 1, Soft, new Vector2(0.5f, 0.5f), 4005);
            _titleHint3 = Text(_titleGroup, "Hint3", "TAB - TABLET  I - INVENTORI  P - JEDA", 1, Soft, new Vector2(0.5f, 0.5f), 4005);
            _titleHint4 = Text(_titleGroup, "Hint4", "N - MULAI BARU", 1, Soft, new Vector2(0.5f, 0.5f), 4005);

            _titleName.transform.localPosition = new Vector3(0f, 3.35f, 0f);
            _titleSub.transform.localPosition = new Vector3(0f, 2.05f, 0f);
            _titleInfo.transform.localPosition = new Vector3(0f, 0.95f, 0f);
            _titlePrompt.transform.localPosition = new Vector3(0f, -0.35f, 0f);
            _titleHint1.transform.localPosition = new Vector3(0f, -1.75f, 0f);
            _titleHint2.transform.localPosition = new Vector3(0f, -2.35f, 0f);
            _titleHint3.transform.localPosition = new Vector3(0f, -2.95f, 0f);
            _titleHint4.transform.localPosition = new Vector3(0f, -3.55f, 0f);
            _titleGroup.gameObject.SetActive(false);
        }

        public void ShowTitle(bool hasSave, string info)
        {
            SetText(_titlePrompt, hasSave ? "ENTER - LANJUTKAN" : "ENTER - MULAI", Gold, 2,
                new Vector2(0.5f, 0.5f));
            SetTitleInfo(info);
            SetDim(0.66f);
            _titleGroup.gameObject.SetActive(true);
        }

        /// <summary>Lets the title screen show hints such as the new-game confirmation.</summary>
        public void SetTitleInfo(string info)
        {
            SetText(_titleInfo, string.IsNullOrEmpty(info) ? " " : info, Soft, 1,
                new Vector2(0.5f, 0.5f));
        }

        public void HideTitle()
        {
            if (_titleGroup != null) _titleGroup.gameObject.SetActive(false);
            if (_homeGroup != null) _homeGroup.gameObject.SetActive(false);
            SetDim(0f);
        }

        // ---------------------------------------------------------------- pause

        void BuildPause()
        {
            _pauseGroup = Group("Pause");
            _pausePanel = Panel(_pauseGroup, "Panel", "lc_panel", 4010);
            SetRect(_pausePanel, 0f, 0f, 15f, 7.2f);
            _pauseTitle = Text(_pauseGroup, "Title", "JEDA", 3, Ink, new Vector2(0.5f, 0.5f), 4012);
            _pausePrompt = Text(_pauseGroup, "Prompt", "P / ESC - LANJUT", 1, Gold, new Vector2(0.5f, 0.5f), 4013);
            _pauseHint = Text(_pauseGroup, "Hint", "H - SIMPAN & KE MENU UTAMA", 1, Soft, new Vector2(0.5f, 0.5f), 4012);
            _pauseMute = Text(_pauseGroup, "Mute", "M - SUARA", 1, Soft, new Vector2(0.5f, 0.5f), 4012);

            _pauseTitle.transform.localPosition = new Vector3(0f, 2.2f, 0f);
            _pausePrompt.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            _pauseHint.transform.localPosition = new Vector3(0f, -0.85f, 0f);
            _pauseMute.transform.localPosition = new Vector3(0f, -1.65f, 0f);
            _pauseGroup.gameObject.SetActive(false);
        }

        public void ShowPause(bool muted)
        {
            SetText(_pauseMute, "M - SUARA: " + (muted ? "MATI" : "NYALA"), Soft, 1,
                new Vector2(0.5f, 0.5f));
            SetDim(0.62f);
            _pauseGroup.gameObject.SetActive(true);
        }

        public void HidePause()
        {
            if (_pauseGroup != null) _pauseGroup.gameObject.SetActive(false);
            SetDim(0f);
        }

        // ---------------------------------------------------------------- work minigame

        void BuildWork()
        {
            _workGroup = Group("Work");
            _workPanel = Panel(_workGroup, "Panel", "lc_panel_gold", 4010);
            SetRect(_workPanel, 0f, 0f, 13.2f, 6.6f);
            _workTitle = Text(_workGroup, "Title", "LAYANI PESANAN", 2, Gold, new Vector2(0.5f, 0.5f), 4012);
            _workOrder = Text(_workGroup, "Order", "PESANAN 1 / 3", 1, Ink, new Vector2(0.5f, 0.5f), 4012);
            _workHits = Text(_workGroup, "Hits", "TEPAT 0", 1, Soft, new Vector2(0.5f, 0.5f), 4012);
            _workPrompt = Text(_workGroup, "Prompt", "E / SPASI - SERVE", 1, Gold, new Vector2(0.5f, 0.5f), 4012);

            _workBarBg = Solid(_workGroup, "BarBg", new Color(0.08f, 0.06f, 0.11f, 1f), 4011);
            _workZone = Solid(_workGroup, "Zone", new Color(0.38f, 0.86f, 0.45f, 0.95f), 4012);
            _workMarker = Solid(_workGroup, "Marker", new Color(1f, 0.95f, 0.72f, 1f), 4013);

            _workTitle.transform.localPosition = new Vector3(0f, 2.45f, 0f);
            _workOrder.transform.localPosition = new Vector3(0f, 1.55f, 0f);
            _workHits.transform.localPosition = new Vector3(0f, -0.55f, 0f);
            _workPrompt.transform.localPosition = new Vector3(0f, -2.45f, 0f);
            _workGroup.gameObject.SetActive(false);
        }

        public void ShowWork(int total)
        {
            _workTotal = Mathf.Max(1, total);
            _workServed = 0;
            _workHitsCount = 0;
            _lastWorkOrder = "";
            _lastWorkHits = "";
            _workMarker01 = 0f;
            _workZoneCenter = 0.5f;
            _workZoneHalf = 0.12f;
            _workGroup.gameObject.SetActive(true);
            UpdateWork("PESANAN 1 / " + _workTotal, "TEPAT 0");
            HidePrompt();
        }

        public void UpdateWork(float marker01, float zoneCenter, float zoneHalf,
            int served, int hits)
        {
            _workMarker01 = Mathf.Clamp01(marker01);
            _workZoneCenter = Mathf.Clamp01(zoneCenter);
            _workZoneHalf = Mathf.Clamp(zoneHalf, 0.02f, 0.5f);
            _workServed = served;
            _workHitsCount = hits;
            string order = served >= _workTotal
                ? "SELESAI"
                : "PESANAN " + (served + 1) + " / " + _workTotal;
            string hitText = "TEPAT " + hits + " / " + _workTotal;
            UpdateWork(order, hitText);
        }

        void UpdateWork(string order, string hits)
        {
            if (order != _lastWorkOrder)
            {
                _lastWorkOrder = order;
                SetText(_workOrder, order, Ink, 1, new Vector2(0.5f, 0.5f));
            }
            if (hits != _lastWorkHits)
            {
                _lastWorkHits = hits;
                SetText(_workHits, hits, Soft, 1, new Vector2(0.5f, 0.5f));
            }
        }

        public void HideWork()
        {
            if (_workGroup != null) _workGroup.gameObject.SetActive(false);
        }

        void LayoutWork(float halfW, float halfH)
        {
            if (_workGroup == null || !_workGroup.gameObject.activeSelf) return;
            float barLeft = -3.6f;
            float barW = 7.2f;
            float barY = 0.6f;
            SetSolid(_workBarBg, barLeft + barW * 0.5f, barY, barW, 0.56f);
            float zoneW = _workZoneHalf * 2f * barW;
            SetSolid(_workZone, barLeft + _workZoneCenter * barW, barY, zoneW, 0.56f);
            SetSolid(_workMarker, barLeft + _workMarker01 * barW, barY, 0.10f, 0.9f);
        }

        // ---------------------------------------------------------------- ending

        void BuildEnding()
        {
            _endGroup = Group("Ending");
            _endPanel = Panel(_endGroup, "Panel", "lc_panel_gold", 4010);
            SetRect(_endPanel, 0f, 0f, 20f, 10.4f);
            _endTitle = Text(_endGroup, "Title", "PULANG", 3, Gold, new Vector2(0.5f, 0.5f), 4012);
            _endSub = Text(_endGroup, "Sub", "THE END", 1, Pink, new Vector2(0.5f, 0.5f), 4012);
            _endLine1 = Text(_endGroup, "Line1", " ", 1, Ink, new Vector2(0.5f, 0.5f), 4012);
            _endLine2 = Text(_endGroup, "Line2", " ", 2, Pink, new Vector2(0.5f, 0.5f), 4012);
            _endLine3 = Text(_endGroup, "Line3", "TERIMA KASIH SUDAH BERMAIN", 1, Soft, new Vector2(0.5f, 0.5f), 4012);
            _endPrompt = Text(_endGroup, "Prompt", "ENTER - KEMBALI KE JUDUL", 1, Gold, new Vector2(0.5f, 0.5f), 4013);

            _endTitle.transform.localPosition = new Vector3(0f, 3.0f, 0f);
            _endSub.transform.localPosition = new Vector3(0f, 1.7f, 0f);
            _endLine1.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            _endLine2.transform.localPosition = new Vector3(0f, -0.3f, 0f);
            _endLine3.transform.localPosition = new Vector3(0f, -1.6f, 0f);
            _endPrompt.transform.localPosition = new Vector3(0f, -3.55f, 0f);
            _endGroup.gameObject.SetActive(false);
        }

        public void HideEnding()
        {
            if (_endGroup != null) _endGroup.gameObject.SetActive(false);
            SetDim(0f);
        }

        /// <summary>Kartu ending tunggal PULANG. Tidak ada ending lain.</summary>
        public void ShowPulangEnding(string playerName, int day)
        {
            SetText(_endTitle, "PULANG", Gold, 3, new Vector2(0.5f, 0.5f));
            SetText(_endSub, "THE END", Pink, 1, new Vector2(0.5f, 0.5f));
            SetText(_endLine1, "PERJALANAN " + playerName + " - " + day + " HARI DI PERANTAUAN",
                Ink, 1, new Vector2(0.5f, 0.5f));
            SetText(_endLine2, "SEJAUH APA PUN KAMU PERGI,", Pink, 1, new Vector2(0.5f, 0.5f));
            SetText(_endLine3, "JANGAN PERNAH MELUPAKAN RUMAH.", Soft, 1, new Vector2(0.5f, 0.5f));
            _endGroup.gameObject.SetActive(true);
            SetDim(0.92f);
        }

        // ---------------------------------------------------------------- name entry

        /// <summary>Layar "SIAPA NAMAMU?" memakai kartu judul.</summary>
        public void ShowNameEntry(string display, bool typedSomething, bool caret)
        {
            if (_homeGroup != null) _homeGroup.gameObject.SetActive(false);
            SetText(_titleName, "SIAPA NAMAMU?", Ink, 2, new Vector2(0.5f, 0.5f));
            SetText(_titleSub, "KETIK NAMA TOKOH UTAMA", Pink, 1, new Vector2(0.5f, 0.5f));
            string shown = display + (caret ? "_" : " ");
            SetText(_titleInfo, " ", Soft, 1, new Vector2(0.5f, 0.5f));
            SetText(_titlePrompt, shown, typedSomething ? Gold : Soft, 2, new Vector2(0.5f, 0.5f));
            SetText(_titleHint1, "ENTER - MULAI CERITA", Soft, 1, new Vector2(0.5f, 0.5f));
            SetText(_titleHint2, "BACKSPACE - HAPUS", Soft, 1, new Vector2(0.5f, 0.5f));
            SetText(_titleHint3, "ESC - PAKAI NAMA ZIDAN", Soft, 1, new Vector2(0.5f, 0.5f));
            SetText(_titleHint4, "MAKSIMAL 12 HURUF", Soft, 1, new Vector2(0.5f, 0.5f));
            SetDim(0.66f);
            _titleGroup.gameObject.SetActive(true);
        }

        /// <summary>Kembalikan teks kartu judul setelah layar nama.</summary>
        public void ResetTitleTexts()
        {
            SetText(_titleName, "PULANG", Ink, 4, new Vector2(0.5f, 0.5f));
            SetText(_titleSub, "LIFE BEYOND HOME", Pink, 1, new Vector2(0.5f, 0.5f));
            SetText(_titleHint1, "WASD - JALAN   SHIFT - LARI", Soft, 1, new Vector2(0.5f, 0.5f));
            SetText(_titleHint2, "E - BICARA  J - JURNAL  T - TUGAS", Soft, 1, new Vector2(0.5f, 0.5f));
            SetText(_titleHint3, "TAB - TABLET  I - INVENTORI  P - JEDA", Soft, 1, new Vector2(0.5f, 0.5f));
            SetText(_titleHint4, "N - MULAI BARU", Soft, 1, new Vector2(0.5f, 0.5f));
        }

        // ---------------------------------------------------------------- journal

        void BuildJournal()
        {
            _journalGroup = Group("Journal");
            _journalPanel = Panel(_journalGroup, "Panel", "lc_panel_gold", 4010);
            _journalHeader = Text(_journalGroup, "Header", "JURNAL - CATATAN PERJALANAN", 1,
                Soft, new Vector2(0.5f, 0.5f), 4012);
            _journalChapter = Text(_journalGroup, "Chapter", "BAB 1", 1, Pink,
                new Vector2(0f, 0.5f), 4012);
            _journalTitle = Text(_journalGroup, "Title", " ", 2, Gold,
                new Vector2(0f, 0.5f), 4012);
            _journalObjectiveA = Text(_journalGroup, "ObjectiveA", " ", 1, Ink,
                new Vector2(0f, 0.5f), 4012);
            _journalObjectiveB = Text(_journalGroup, "ObjectiveB", " ", 1, Ink,
                new Vector2(0f, 0.5f), 4012);
            for (int i = 0; i < _journalDetails.Length; i++)
                _journalDetails[i] = Text(_journalGroup, "Detail" + i, " ", 1, Ink,
                    new Vector2(0f, 0.5f), 4012);
            _journalHint = Text(_journalGroup, "Hint", "J / ESC - TUTUP", 1, Gold,
                new Vector2(0.5f, 0.5f), 4012);
            _journalGroup.gameObject.SetActive(false);
        }

        public void ShowJournal(string chapter, string title, string hint,
            List<string> details)
        {
            SetText(_journalHeader, "JURNAL - CATATAN PERJALANAN", Soft, 1,
                new Vector2(0.5f, 0.5f));
            SetText(_journalChapter, chapter, Pink, 1, new Vector2(0f, 0.5f));
            SetText(_journalTitle, title, Gold, 2, new Vector2(0f, 0.5f));

            string[] wrapped = Wrap("TUGAS: " + hint, 52);
            SetText(_journalObjectiveA, wrapped.Length > 0 ? wrapped[0] : " ", Ink, 1,
                new Vector2(0f, 0.5f));
            SetText(_journalObjectiveB, wrapped.Length > 1 ? wrapped[1] : " ", Soft, 1,
                new Vector2(0f, 0.5f));

            int count = details == null ? 0 : Mathf.Min(details.Count, _journalDetails.Length);
            for (int i = 0; i < _journalDetails.Length; i++)
            {
                string line = i < count ? details[i] : " ";
                SetText(_journalDetails[i], string.IsNullOrEmpty(line) ? " " : line,
                    i < 2 ? Ink : Soft, 1, new Vector2(0f, 0.5f));
            }

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

            float w = Mathf.Min(halfW * 2f - 1.2f, 23f);
            float h = Mathf.Min(halfH * 2f - 1.1f, 13.4f);
            float cx = 0f;
            float cy = 0f;
            SetRect(_journalPanel, cx, cy, w, h);

            float lx = cx - w * 0.5f + 0.75f;
            float top = cy + h * 0.5f;
            _journalHeader.transform.localPosition = new Vector3(cx, top - 0.52f, 0f);
            _journalChapter.transform.localPosition = new Vector3(lx, top - 1.18f, 0f);
            _journalTitle.transform.localPosition = new Vector3(lx, top - 1.98f, 0f);
            _journalObjectiveA.transform.localPosition = new Vector3(lx, top - 3.02f, 0f);
            _journalObjectiveB.transform.localPosition = new Vector3(lx, top - 3.60f, 0f);

            for (int i = 0; i < _journalDetails.Length; i++)
                _journalDetails[i].transform.localPosition =
                    new Vector3(lx, top - 4.05f - i * 0.44f, 0f);

            _journalHint.transform.localPosition = new Vector3(cx, cy - h * 0.5f + 0.42f, 0f);
        }

        /// <summary>
        /// The title and ending cards have a fixed 20-unit width. On narrow
        /// windows (4:3, 5:4, square) they are scaled down as a group instead of
        /// spilling past the screen edges.
        /// </summary>
        void LayoutOverlays(float halfW, float halfH)
        {
            float fit = Mathf.Min(1f, (halfW * 2f - 1f) / 20f);
            if (_titleGroup != null)
                _titleGroup.localScale = new Vector3(fit, fit, 1f);
            if (_endGroup != null)
                _endGroup.localScale = new Vector3(fit, fit, 1f);
        }
    }
}
