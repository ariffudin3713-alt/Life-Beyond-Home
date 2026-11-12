using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>Satu pilihan di halaman utama.</summary>
    public struct HomeOption
    {
        public string Label, Sub;
        public bool Enabled;
        public HomeOption(string label, string sub, bool enabled = true) { Label = label; Sub = sub; Enabled = enabled; }
    }

    /// <summary>Isi satu kartu slot simpanan.</summary>
    public struct SlotView
    {
        public bool Empty;
        public string Line1, Line2, Line3;
    }

    /// <summary>
    /// Halaman utama (home page): LANJUTKAN, GAME BARU, SLOT SIMPANAN,
    /// dan layar pilih slot (3 slot) untuk game baru / memuat / menghapus.
    /// </summary>
    public partial class LifeHud
    {
        const int HomeMaxOptions = 4;
        const int SlotCards = 3;
        static readonly Color HomeDisabled = new Color(0.45f, 0.43f, 0.50f);

        Transform _homeGroup, _homeMenuGroup, _homeSlotGroup;
        SpriteRenderer _homePanel, _homeName, _homeSub, _homeHint, _homeHighlight;
        readonly SpriteRenderer[] _homeOptions = new SpriteRenderer[HomeMaxOptions];
        readonly SpriteRenderer[] _homeOptionSubs = new SpriteRenderer[HomeMaxOptions];
        int _homeOptionCount, _homeSelected;

        SpriteRenderer _slotTitle, _slotMessage;
        readonly SpriteRenderer[] _slotPanels = new SpriteRenderer[SlotCards];
        readonly SpriteRenderer[] _slotLabels = new SpriteRenderer[SlotCards];
        readonly SpriteRenderer[] _slotLine1 = new SpriteRenderer[SlotCards];
        readonly SpriteRenderer[] _slotLine2 = new SpriteRenderer[SlotCards];
        readonly SpriteRenderer[] _slotLine3 = new SpriteRenderer[SlotCards];
        int _slotSelected;

        void BuildHome()
        {
            _homeGroup = Group("Home");
            _homePanel = Panel(_homeGroup, "Panel", "lc_panel_gold", 4000);
            _homePanel.color = new Color(1f, 1f, 1f, 0.97f);
            _homeName = Text(_homeGroup, "Name", "PULANG", 4, Ink, new Vector2(0.5f, 0.5f), 4005);
            _homeSub = Text(_homeGroup, "Sub", "LIFE BEYOND HOME", 1, Pink, new Vector2(0.5f, 0.5f), 4005);
            _homeHint = Text(_homeGroup, "Hint", " ", 1, Soft, new Vector2(0.5f, 0.5f), 4005);

            // ---- menu utama
            _homeMenuGroup = new GameObject("Menu").transform;
            _homeMenuGroup.SetParent(_homeGroup, false);
            _homeHighlight = Solid(_homeMenuGroup, "Highlight", new Color(1f, 0.84f, 0.40f, 0.16f), 4002);
            for (int i = 0; i < HomeMaxOptions; i++)
            {
                _homeOptions[i] = Text(_homeMenuGroup, "Option" + i, " ", 2, Ink, new Vector2(0.5f, 0.5f), 4005);
                _homeOptionSubs[i] = Text(_homeMenuGroup, "OptionSub" + i, " ", 1, Soft, new Vector2(0.5f, 0.5f), 4005);
            }

            // ---- layar slot
            _homeSlotGroup = new GameObject("Slots").transform;
            _homeSlotGroup.SetParent(_homeGroup, false);
            _slotTitle = Text(_homeSlotGroup, "SlotTitle", " ", 2, Gold, new Vector2(0.5f, 0.5f), 4005);
            _slotMessage = Text(_homeSlotGroup, "SlotMessage", " ", 1, Pink, new Vector2(0.5f, 0.5f), 4006);
            for (int i = 0; i < SlotCards; i++)
            {
                _slotPanels[i] = Panel(_homeSlotGroup, "SlotPanel" + i, "lc_panel_soft", 4002);
                _slotLabels[i] = Text(_homeSlotGroup, "SlotLabel" + i, "SLOT " + (i + 1), 2, Gold,
                    new Vector2(0f, 0.5f), 4005);
                _slotLine1[i] = Text(_homeSlotGroup, "SlotLine1_" + i, " ", 1, Ink, new Vector2(0f, 0.5f), 4005);
                _slotLine2[i] = Text(_homeSlotGroup, "SlotLine2_" + i, " ", 1, Soft, new Vector2(0f, 0.5f), 4005);
                _slotLine3[i] = Text(_homeSlotGroup, "SlotLine3_" + i, " ", 1, Soft, new Vector2(0f, 0.5f), 4005);
            }

            _homeGroup.gameObject.SetActive(false);
        }

        public bool HomeVisible => _homeGroup != null && _homeGroup.gameObject.activeSelf;

        // ------------------------------------------------------------ menu utama

        public void ShowHome(List<HomeOption> options, int selected)
        {
            _homeOptionCount = Mathf.Min(options.Count, HomeMaxOptions);
            _homeSelected = Mathf.Clamp(selected, 0, Mathf.Max(0, _homeOptionCount - 1));
            for (int i = 0; i < HomeMaxOptions; i++)
            {
                bool show = i < _homeOptionCount;
                _homeOptions[i].gameObject.SetActive(show);
                _homeOptionSubs[i].gameObject.SetActive(show);
                if (!show) continue;
                var o = options[i];
                bool sel = i == _homeSelected;
                Color c = !o.Enabled ? HomeDisabled : sel ? Gold : Ink;
                SetText(_homeOptions[i], (sel ? "> " : "") + o.Label + (sel ? " <" : ""), c, 2, new Vector2(0.5f, 0.5f));
                SetText(_homeOptionSubs[i], string.IsNullOrEmpty(o.Sub) ? " " : o.Sub,
                    o.Enabled ? Soft : HomeDisabled, 1, new Vector2(0.5f, 0.5f));
            }
            SetText(_homeHint, "W/S - PILIH    E / ENTER - OK    M - SUARA", Soft, 1, new Vector2(0.5f, 0.5f));
            _homeMenuGroup.gameObject.SetActive(true);
            _homeSlotGroup.gameObject.SetActive(false);
            OpenHomeGroup();
        }

        // ------------------------------------------------------------ layar slot

        public void ShowSlots(string title, List<SlotView> slots, int selected, string message, string hint)
        {
            _slotSelected = Mathf.Clamp(selected, 0, SlotCards - 1);
            SetText(_slotTitle, title, Gold, 2, new Vector2(0.5f, 0.5f));
            SetText(_slotMessage, string.IsNullOrEmpty(message) ? " " : message, Pink, 1, new Vector2(0.5f, 0.5f));
            for (int i = 0; i < SlotCards; i++)
            {
                var s = i < slots.Count ? slots[i] : new SlotView { Empty = true };
                bool sel = i == _slotSelected;
                _slotPanels[i].sprite = PixelArt.Get(sel ? "lc_panel_gold" : "lc_panel_soft");
                SetText(_slotLabels[i], (sel ? "> " : "") + "SLOT " + (i + 1), sel ? Gold : Soft, 2,
                    new Vector2(0f, 0.5f));
                if (s.Empty)
                {
                    SetText(_slotLine1[i], "- KOSONG -", HomeDisabled, 1, new Vector2(0f, 0.5f));
                    SetText(_slotLine2[i], " ", Soft, 1, new Vector2(0f, 0.5f));
                    SetText(_slotLine3[i], " ", Soft, 1, new Vector2(0f, 0.5f));
                }
                else
                {
                    SetText(_slotLine1[i], Safe(s.Line1), sel ? Gold : Ink, 1, new Vector2(0f, 0.5f));
                    SetText(_slotLine2[i], Safe(s.Line2), Ink, 1, new Vector2(0f, 0.5f));
                    SetText(_slotLine3[i], Safe(s.Line3), Soft, 1, new Vector2(0f, 0.5f));
                }
            }
            SetText(_homeHint, hint, Soft, 1, new Vector2(0.5f, 0.5f));
            _homeMenuGroup.gameObject.SetActive(false);
            _homeSlotGroup.gameObject.SetActive(true);
            OpenHomeGroup();
        }

        static string Safe(string s) =>
            string.IsNullOrEmpty(s) ? " " : s.ToUpperInvariant().Replace('–', '-').Replace('—', '-');

        void OpenHomeGroup()
        {
            if (_titleGroup != null) _titleGroup.gameObject.SetActive(false);
            SetDim(0.66f);
            _homeGroup.gameObject.SetActive(true);
            HidePrompt();
        }

        public void HideHome()
        {
            if (_homeGroup != null) _homeGroup.gameObject.SetActive(false);
            SetDim(0f);
        }

        // ------------------------------------------------------------ layout

        void LayoutHome(float halfW, float halfH)
        {
            if (!HomeVisible) return;
            float w = Mathf.Min(halfW * 2f - 1.0f, 24f);
            float h = Mathf.Min(halfH * 2f - 0.8f, 15f);
            float top = h * 0.5f;
            SetRect(_homePanel, 0f, 0f, w, h);
            _homeHint.transform.localPosition = new Vector3(0f, -top + 0.5f, 0f);

            if (_homeMenuGroup.gameObject.activeSelf)
            {
                _homeName.gameObject.SetActive(true);
                _homeSub.gameObject.SetActive(true);
                _homeName.transform.localPosition = new Vector3(0f, top - 2.0f, 0f);
                _homeSub.transform.localPosition = new Vector3(0f, top - 3.3f, 0f);

                float startY = top - 5.0f;
                float step = 1.75f;
                for (int i = 0; i < _homeOptionCount; i++)
                {
                    float y = startY - i * step;
                    _homeOptions[i].transform.localPosition = new Vector3(0f, y, 0f);
                    _homeOptionSubs[i].transform.localPosition = new Vector3(0f, y - 0.68f, 0f);
                    if (i == _homeSelected)
                        SetSolid(_homeHighlight, 0f, y - 0.3f, Mathf.Min(w - 2f, 16f), 1.5f);
                }
                float pulse = 0.12f + 0.08f * (Mathf.Sin(_time * 4f) * 0.5f + 0.5f);
                SetAlpha(_homeHighlight, pulse);
            }
            else
            {
                // Layar slot: judul kecil di atas, tiga kartu bertumpuk.
                _homeName.gameObject.SetActive(false);
                _homeSub.gameObject.SetActive(false);
                _slotTitle.transform.localPosition = new Vector3(0f, top - 0.95f, 0f);

                float cw = Mathf.Min(w - 1.6f, 20f);
                float ch = Mathf.Min(2.75f, (h - 4.4f - 0.6f) / SlotCards);
                float gap = 0.3f;
                float firstCy = top - 1.85f - ch * 0.5f;
                for (int i = 0; i < SlotCards; i++)
                {
                    float cy = firstCy - i * (ch + gap);
                    SetRect(_slotPanels[i], 0f, cy, cw, ch);
                    float lx = -cw * 0.5f + 0.5f;
                    float ix = lx + 4.4f;                  // kolom isi di kanan label SLOT
                    _slotLabels[i].transform.localPosition = new Vector3(lx, cy, 0f);
                    _slotLine1[i].transform.localPosition = new Vector3(ix, cy + ch * 0.25f, 0f);
                    _slotLine2[i].transform.localPosition = new Vector3(ix, cy, 0f);
                    _slotLine3[i].transform.localPosition = new Vector3(ix, cy - ch * 0.25f, 0f);
                }
                float lastBottom = firstCy - (SlotCards - 1) * (ch + gap) - ch * 0.5f;
                _slotMessage.transform.localPosition = new Vector3(0f, (lastBottom + (-top + 0.5f)) * 0.5f, 0f);
            }
        }
    }
}
