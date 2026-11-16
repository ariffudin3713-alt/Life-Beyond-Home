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
        public string Saved;    // waktu simpan, ditampilkan rata kanan di baris atas
    }

    /// <summary>
    /// Halaman utama layar penuh:
    /// - latar hidup: langit senja, matahari, awan bergerak, siluet kota & bukit (paralaks),
    ///   jalan dengan marka bergerak, tokoh utama berjalan membawa tas, bus kota sesekali lewat;
    /// - logo judul berbayang + subjudul;
    /// - tombol menu (keyboard W/S + E, atau ketuk/klik langsung);
    /// - layar slot simpanan (3 kartu) di atas latar yang sama.
    /// </summary>
    public partial class LifeHud
    {
        const int HomeMaxOptions = 6;
        const int SlotCards = 3;
        const int HomeClouds = 5;
        const int HomeTiles = 4;
        static readonly Color HomeDisabled = new Color(0.45f, 0.43f, 0.50f);

        Transform _homeGroup, _homeMenuGroup, _homeSlotGroup, _homeBgGroup;

        // ---- latar
        SpriteRenderer _homeSky, _homeSun, _homeRoad, _homeRoadEdge, _homeWalker, _homeBus;
        readonly SpriteRenderer[] _homeCloudsSr = new SpriteRenderer[HomeClouds];
        readonly float[] _homeCloudX = new float[HomeClouds];
        readonly SpriteRenderer[] _homeCity = new SpriteRenderer[HomeTiles];
        readonly SpriteRenderer[] _homeHills = new SpriteRenderer[HomeTiles];
        readonly SpriteRenderer[] _homeDashes = new SpriteRenderer[12];
        float _homeWalkerX = -999f, _homeBusX = 999f, _homeBusWait = 3f;

        // ---- judul & menu
        SpriteRenderer _homePanel, _homeName, _homeNameShadow, _homeSub, _homeHint, _homeVersion, _homeDesc;
        readonly SpriteRenderer[] _homeButtons = new SpriteRenderer[HomeMaxOptions];
        readonly SpriteRenderer[] _homeOptions = new SpriteRenderer[HomeMaxOptions];
        readonly Rect[] _homeButtonRects = new Rect[HomeMaxOptions];
        readonly List<HomeOption> _homeOpts = new List<HomeOption>();
        int _homeOptionCount, _homeSelected;

        // ---- slot
        SpriteRenderer _slotTitle, _slotMessage;
        readonly SpriteRenderer[] _slotPanels = new SpriteRenderer[SlotCards];
        readonly SpriteRenderer[] _slotLabels = new SpriteRenderer[SlotCards];
        readonly SpriteRenderer[] _slotLine1 = new SpriteRenderer[SlotCards];
        readonly SpriteRenderer[] _slotLine2 = new SpriteRenderer[SlotCards];
        readonly SpriteRenderer[] _slotLine3 = new SpriteRenderer[SlotCards];
        readonly SpriteRenderer[] _slotSaved = new SpriteRenderer[SlotCards];
        readonly SlotView[] _slotViews = new SlotView[SlotCards];
        int _slotFitChars = -1;        // teks kartu dibuat ulang hanya bila lebar kartu berubah
        readonly Rect[] _slotRects = new Rect[SlotCards];
        int _slotSelected;

        void BuildHome()
        {
            _homeGroup = Group("Home");

            // ---------------- latar (urutan gambar 3950–3975, di atas dunia & peredup)
            _homeBgGroup = new GameObject("Background").transform;
            _homeBgGroup.SetParent(_homeGroup, false);
            _homeSky = Make(_homeBgGroup, PixelArt.Get("lc_home_sky"), "Sky", 3950);
            _homeSun = Make(_homeBgGroup, PixelArt.Get("lc_home_sun"), "Sun", 3951);
            for (int i = 0; i < HomeClouds; i++)
            {
                _homeCloudsSr[i] = Make(_homeBgGroup, PixelArt.Get("lc_home_cloud"), "Cloud" + i, 3952);
                _homeCloudX[i] = -14f + i * 7.5f;
            }
            for (int i = 0; i < HomeTiles; i++)
            {
                _homeCity[i] = Make(_homeBgGroup, PixelArt.Get("lc_home_city"), "City" + i, 3953);
                _homeHills[i] = Make(_homeBgGroup, PixelArt.Get("lc_home_hills"), "Hills" + i, 3954);
            }
            _homeRoad = Solid(_homeBgGroup, "Road", new Color(0.20f, 0.19f, 0.24f, 1f), 3955);
            _homeRoadEdge = Solid(_homeBgGroup, "RoadEdge", new Color(0.62f, 0.58f, 0.52f, 1f), 3956);
            for (int i = 0; i < _homeDashes.Length; i++)
                _homeDashes[i] = Solid(_homeBgGroup, "Dash" + i, new Color(0.95f, 0.88f, 0.55f, 0.9f), 3956);
            _homeBus = Make(_homeBgGroup, PixelArt.Get("lc_vehicle_bus"), "Bus", 3957);
            _homeWalker = Make(_homeBgGroup, PixelArt.Get("lc_player_right_0"), "Walker", 3958);

            // ---------------- judul
            _homePanel = Panel(_homeGroup, "Panel", "lc_panel_gold", 4000);
            _homePanel.color = new Color(1f, 1f, 1f, 0.95f);
            _homeNameShadow = Text(_homeGroup, "NameShadow", "LIFE BEYOND HOME", 3, new Color(0.10f, 0.06f, 0.14f, 0.85f),
                new Vector2(0.5f, 0.5f), 4004);
            _homeName = Text(_homeGroup, "Name", "LIFE BEYOND HOME", 3, Gold, new Vector2(0.5f, 0.5f), 4005);
            _homeSub = Text(_homeGroup, "Sub", "SEBUAH KISAH TENTANG PULANG", 1, Ink, new Vector2(0.5f, 0.5f), 4005);
            _homeHint = Text(_homeGroup, "Hint", " ", 1, Ink, new Vector2(0.5f, 0.5f), 4005);
            _homeVersion = Text(_homeGroup, "Version", "V1.0", 1, Soft, new Vector2(1f, 0.5f), 4005);

            // ---------------- menu: tombol
            _homeMenuGroup = new GameObject("Menu").transform;
            _homeMenuGroup.SetParent(_homeGroup, false);
            for (int i = 0; i < HomeMaxOptions; i++)
            {
                _homeButtons[i] = Panel(_homeMenuGroup, "Button" + i, "lc_panel_soft", 4002);
                _homeOptions[i] = Text(_homeMenuGroup, "Option" + i, " ", 2, Ink, new Vector2(0.5f, 0.5f), 4005);
            }
            _homeDesc = Text(_homeMenuGroup, "Desc", " ", 1, Ink, new Vector2(0.5f, 0.5f), 4005);

            // ---------------- layar slot
            _homeSlotGroup = new GameObject("Slots").transform;
            _homeSlotGroup.SetParent(_homeGroup, false);
            _slotTitle = Text(_homeSlotGroup, "SlotTitle", " ", 2, Gold, new Vector2(0.5f, 0.5f), 4005);
            _slotMessage = Text(_homeSlotGroup, "SlotMessage", " ", 1, Pink, new Vector2(0.5f, 0.5f), 4006);
            for (int i = 0; i < SlotCards; i++)
            {
                _slotPanels[i] = Panel(_homeSlotGroup, "SlotPanel" + i, "lc_panel_soft", 4002);
                _slotLabels[i] = Text(_homeSlotGroup, "SlotLabel" + i, "SLOT " + (i + 1), 2, Gold, new Vector2(0f, 0.5f), 4005);
                _slotLine1[i] = Text(_homeSlotGroup, "SlotLine1_" + i, " ", 1, Ink, new Vector2(0f, 0.5f), 4005);
                _slotLine2[i] = Text(_homeSlotGroup, "SlotLine2_" + i, " ", 1, Soft, new Vector2(0f, 0.5f), 4005);
                _slotLine3[i] = Text(_homeSlotGroup, "SlotLine3_" + i, " ", 1, Soft, new Vector2(0f, 0.5f), 4005);
                _slotSaved[i] = Text(_homeSlotGroup, "SlotSaved" + i, " ", 1, Soft, new Vector2(1f, 0.5f), 4005);
            }

            _homeGroup.gameObject.SetActive(false);
        }

        public bool HomeVisible => _homeGroup != null && _homeGroup.gameObject.activeSelf;

        // ------------------------------------------------------------ menu utama

        public void ShowHome(List<HomeOption> options, int selected)
        {
            _homeOpts.Clear();
            _homeOpts.AddRange(options);
            _homeOptionCount = Mathf.Min(options.Count, HomeMaxOptions);
            _homeSelected = Mathf.Clamp(selected, 0, Mathf.Max(0, _homeOptionCount - 1));
            for (int i = 0; i < HomeMaxOptions; i++)
            {
                bool show = i < _homeOptionCount;
                _homeButtons[i].gameObject.SetActive(show);
                _homeOptions[i].gameObject.SetActive(show);
                if (!show) continue;
                var o = options[i];
                bool sel = i == _homeSelected;
                _homeButtons[i].sprite = PixelArt.Get(sel ? "lc_panel_gold" : "lc_panel_soft");
                _homeButtons[i].color = o.Enabled ? Color.white : new Color(1f, 1f, 1f, 0.55f);
                Color c = !o.Enabled ? HomeDisabled : sel ? Gold : Ink;
                SetText(_homeOptions[i], o.Label, c, 2, new Vector2(0.5f, 0.5f));
            }
            string desc = _homeOptionCount > 0 ? _homeOpts[_homeSelected].Sub : " ";
            SetText(_homeDesc, string.IsNullOrEmpty(desc) ? " " : desc, Ink, 1, new Vector2(0.5f, 0.5f));
            SetHint(_homeHint, "W/S / KURSOR - PILIH    E / ENTER / KLIK - OK", Ink);
            _homeMenuGroup.gameObject.SetActive(true);
            _homeSlotGroup.gameObject.SetActive(false);
            _homePanel.gameObject.SetActive(false);
            _homeName.gameObject.SetActive(true);
            _homeNameShadow.gameObject.SetActive(true);
            _homeSub.gameObject.SetActive(true);
            OpenHomeGroup();
        }

        /// <summary>Tombol menu yang diketuk/diklik pada frame ini (-1 bila tidak ada).</summary>
        public int HomeTappedOption()
        {
            if (!HomeVisible || !_homeMenuGroup.gameObject.activeSelf || !LifeInput.PointerTapped) return -1;
            Vector2 p = PointerLocal();
            for (int i = 0; i < _homeOptionCount; i++)
                if (_homeButtonRects[i].Contains(p)) return i;
            return -1;
        }

        /// <summary>Kartu slot yang diketuk/diklik pada frame ini (-1 bila tidak ada).</summary>
        public int SlotTappedIndex()
        {
            if (!HomeVisible || !_homeSlotGroup.gameObject.activeSelf || !LifeInput.PointerTapped) return -1;
            Vector2 p = PointerLocal();
            for (int i = 0; i < SlotCards; i++)
                if (_slotRects[i].Contains(p)) return i;
            return -1;
        }

        Vector2 PointerLocal() => PointerToHud();

        // ------------------------------------------------------------ layar slot

        public void ShowSlots(string title, List<SlotView> slots, int selected, string message, string hint)
        {
            _slotSelected = Mathf.Clamp(selected, 0, SlotCards - 1);
            SetText(_slotTitle, title, Gold, 2, new Vector2(0.5f, 0.5f));
            SetText(_slotMessage, string.IsNullOrEmpty(message) ? " " : message, Pink, 1, new Vector2(0.5f, 0.5f));
            for (int i = 0; i < SlotCards; i++)
            {
                _slotViews[i] = i < slots.Count ? slots[i] : new SlotView { Empty = true };
                bool sel = i == _slotSelected;
                _slotPanels[i].sprite = PixelArt.Get(sel ? "lc_panel_gold" : "lc_panel_soft");
                // Slot terpilih ditandai panel emas & label emas (tanpa "> " yang membuat label melebar).
                SetText(_slotLabels[i], "SLOT " + (i + 1), sel ? Gold : Soft, 2, new Vector2(0f, 0.5f));
            }
            _slotFitChars = -1;   // isi kartu ditulis ulang saat layout (menyesuaikan lebar kartu)
            SetHint(_homeHint, hint, Ink);
            _homeMenuGroup.gameObject.SetActive(false);
            _homeSlotGroup.gameObject.SetActive(true);
            _homePanel.gameObject.SetActive(true);
            _homeName.gameObject.SetActive(false);
            _homeNameShadow.gameObject.SetActive(false);
            _homeSub.gameObject.SetActive(false);
            OpenHomeGroup();
        }

        static string Safe(string s) =>
            string.IsNullOrEmpty(s) ? " " : s.ToUpperInvariant().Replace('–', '-').Replace('—', '-');

        /// <summary>Potong teks agar muat <paramref name="max"/> huruf (diakhiri "..").</summary>
        static string Fit(string s, int max)
        {
            s = Safe(s);
            if (max < 3) return " ";
            return s.Length <= max ? s : s.Substring(0, max - 2).TrimEnd() + "..";
        }

        /// <summary>Tulis isi kartu slot dengan lebar maksimal <paramref name="chars"/> huruf per baris.</summary>
        void ApplySlotTexts(int chars)
        {
            for (int i = 0; i < SlotCards; i++)
            {
                var s = _slotViews[i];
                bool sel = i == _slotSelected;
                if (s.Empty)
                {
                    SetText(_slotLine1[i], " ", Soft, 1, new Vector2(0f, 0.5f));
                    SetText(_slotLine2[i], "- SLOT KOSONG -", HomeDisabled, 1, new Vector2(0f, 0.5f));
                    SetText(_slotLine3[i], " ", Soft, 1, new Vector2(0f, 0.5f));
                    SetText(_slotSaved[i], " ", Soft, 1, new Vector2(1f, 0.5f));
                    continue;
                }
                string saved = string.IsNullOrEmpty(s.Saved) ? "" : Safe(s.Saved);
                // Baris atas berbagi tempat dengan waktu simpan di kanan (jarak minimal 2 huruf).
                int line1Max = saved.Length > 0 ? chars - saved.Length - 2 : chars;
                if (line1Max < 10) { saved = ""; line1Max = chars; }   // kartu sempit: waktu simpan disembunyikan
                SetText(_slotLine1[i], Fit(s.Line1, line1Max), sel ? Gold : Ink, 1, new Vector2(0f, 0.5f));
                SetText(_slotLine2[i], Fit(s.Line2, chars), Ink, 1, new Vector2(0f, 0.5f));
                SetText(_slotLine3[i], Fit(s.Line3, chars), Soft, 1, new Vector2(0f, 0.5f));
                SetText(_slotSaved[i], saved.Length > 0 ? saved : " ", Soft, 1, new Vector2(1f, 0.5f));
            }
        }

        void OpenHomeGroup()
        {
            if (_titleGroup != null) _titleGroup.gameObject.SetActive(false);
            SetDim(0f);                       // latar sendiri sudah menutupi seluruh layar
            _homeGroup.gameObject.SetActive(true);
            HidePrompt();
        }

        public void HideHome()
        {
            if (_homeGroup != null) _homeGroup.gameObject.SetActive(false);
            SetDim(0f);
        }

        // ------------------------------------------------------------ layout & animasi

        void LayoutHome(float halfW, float halfH)
        {
            if (!HomeVisible) return;
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            LayoutHomeBackground(halfW, halfH, dt);

            float top = halfH;
            float bob = Mathf.Sin(_time * 1.6f) * 0.08f;
            _homeVersion.transform.localPosition = new Vector3(halfW - 0.4f, -halfH + 0.35f, 0f);
            _homeHint.transform.localPosition = new Vector3(0f, -halfH + 0.35f, 0f);
            FitHint(_homeHint, (halfW - 2.6f) * 2f);   // jangan menabrak label versi di kanan bawah

            if (_homeMenuGroup.gameObject.activeSelf)
            {
                _homeName.transform.localPosition = new Vector3(0f, top - 2.1f + bob, 0f);
                _homeNameShadow.transform.localPosition = new Vector3(0.14f, top - 2.24f + bob, 0f);
                _homeSub.transform.localPosition = new Vector3(0f, top - 3.25f, 0f);

                // Tombol bertumpuk di tengah.
                float bw = Mathf.Min(13f, halfW * 2f - 2f), bh = 1.1f, step = 1.3f;   // "SLOT SIMPANAN" (huruf besar) muat dengan tepi lega
                float startY = top - 4.55f;
                for (int i = 0; i < _homeOptionCount; i++)
                {
                    float y = startY - i * step;
                    bool sel = i == _homeSelected;
                    float grow = sel ? 0.3f + 0.06f * Mathf.Sin(_time * 5f) : 0f;
                    SetRect(_homeButtons[i], 0f, y, bw + grow, bh + grow * 0.3f);
                    _homeOptions[i].transform.localPosition = new Vector3(0f, y, 0f);
                    _homeButtonRects[i] = new Rect(-bw * 0.5f, y - bh * 0.5f, bw, bh);
                    Hit(UiHit.HomeOption, i, _homeButtonRects[i], 2);
                }
                _homeDesc.transform.localPosition = new Vector3(0f, startY - _homeOptionCount * step - 0.15f, 0f);
            }
            else
            {
                // Layar slot: panel di tengah, tiga kartu bertumpuk.
                float w = Mathf.Min(halfW * 2f - 1.0f, 24f);
                float h = Mathf.Min(halfH * 2f - 1.6f, 14.2f);
                float ptop = h * 0.5f + 0.2f;
                SetRect(_homePanel, 0f, 0.2f, w, h);
                CloseAt(w * 0.5f, 0.2f + h * 0.5f);
                _slotTitle.transform.localPosition = new Vector3(0f, ptop - 0.95f, 0f);
                float cw = Mathf.Min(w - 1.6f, 20f);
                float ch = Mathf.Min(2.75f, (h - 4.4f - 0.6f) / SlotCards);
                float gap = 0.3f;
                float firstCy = ptop - 1.85f - ch * 0.5f;
                // Kolom kiri: label "SLOT n" (6 huruf skala 2 = 4,5 unit). Kolom kanan: tiga baris isi.
                float lx = -cw * 0.5f + 0.6f;
                float ix = lx + 5.2f;
                float rx = cw * 0.5f - 0.6f;
                int chars = Mathf.Max(8, Mathf.FloorToInt((rx - ix) / PixelFont.CharW));
                if (chars != _slotFitChars) { _slotFitChars = chars; ApplySlotTexts(chars); }
                float row = Mathf.Min(0.72f, ch * 0.28f);
                for (int i = 0; i < SlotCards; i++)
                {
                    float cy = firstCy - i * (ch + gap);
                    SetRect(_slotPanels[i], 0f, cy, cw, ch);
                    _slotLabels[i].transform.localPosition = new Vector3(lx, cy, 0f);
                    _slotLine1[i].transform.localPosition = new Vector3(ix, cy + row, 0f);
                    _slotSaved[i].transform.localPosition = new Vector3(rx, cy + row, 0f);
                    _slotLine2[i].transform.localPosition = new Vector3(ix, cy, 0f);
                    _slotLine3[i].transform.localPosition = new Vector3(ix, cy - row, 0f);
                    _slotRects[i] = new Rect(-cw * 0.5f, cy - ch * 0.5f, cw, ch);
                    Hit(UiHit.SlotCard, i, _slotRects[i], 2);
                }
                float lastBottom = firstCy - (SlotCards - 1) * (ch + gap) - ch * 0.5f;
                _slotMessage.transform.localPosition = new Vector3(0f, (lastBottom + (-h * 0.5f + 0.2f)) * 0.5f, 0f);
            }
        }

        void LayoutHomeBackground(float halfW, float halfH, float dt)
        {
            float bottom = -halfH;
            float roadH = 1.7f;
            float ground = bottom + roadH;

            // Langit memenuhi layar (sprite 4x96 px = 0,25 x 6 unit).
            _homeSky.transform.localPosition = new Vector3(0f, bottom, 0f);
            _homeSky.transform.localScale = new Vector3(halfW * 2f / 0.25f + 1f, halfH * 2f / 6f + 0.1f, 1f);

            // Matahari terbenam di belakang kota.
            _homeSun.transform.localPosition = new Vector3(halfW * 0.42f, ground + 3.6f, 0f);
            _homeSun.transform.localScale = Vector3.one * (2.4f + 0.05f * Mathf.Sin(_time * 0.8f));

            // Awan bergerak pelan ke kanan.
            for (int i = 0; i < HomeClouds; i++)
            {
                _homeCloudX[i] += dt * (0.25f + i * 0.07f);
                if (_homeCloudX[i] > halfW + 3f) _homeCloudX[i] = -halfW - 3f;
                float y = halfH - 1.4f - (i % 3) * 1.6f - (i % 2) * 0.6f;
                _homeCloudsSr[i].transform.localPosition = new Vector3(_homeCloudX[i], y, 0f);
                _homeCloudsSr[i].transform.localScale = Vector3.one * (1.2f + (i % 3) * 0.35f);
            }

            // Kota & bukit: ubin 24 unit yang bergeser (paralaks).
            const float tileW = 24f;
            float cityOff = -((_time * 0.35f) % tileW);
            float hillOff = -((_time * 0.9f) % tileW);
            for (int i = 0; i < HomeTiles; i++)
            {
                float x0 = -halfW + (i - 1) * tileW;
                _homeCity[i].transform.localPosition = new Vector3(x0 + cityOff + tileW, ground, 0f);
                _homeHills[i].transform.localPosition = new Vector3(x0 + hillOff + tileW, ground - 0.2f, 0f);
            }

            // Jalan + marka bergerak.
            SetSolid(_homeRoad, 0f, bottom + roadH * 0.5f, halfW * 2f + 1f, roadH);
            SetSolid(_homeRoadEdge, 0f, ground, halfW * 2f + 1f, 0.12f);
            float dashGap = 2.6f;
            float dashOff = (_time * 1.6f) % dashGap;
            for (int i = 0; i < _homeDashes.Length; i++)
            {
                float x = -halfW - dashGap + i * dashGap + dashOff;
                SetSolid(_homeDashes[i], x, bottom + roadH * 0.45f, 1.2f, 0.1f);
            }

            // Tokoh utama berjalan ke kanan membawa tas, lalu muncul lagi dari kiri.
            if (_homeWalkerX < -halfW - 3f || _homeWalkerX > halfW + 2f) _homeWalkerX = -halfW - 1.5f;
            _homeWalkerX += dt * 1.5f;
            int frame = ((int)(_time * 7f)) % 4;
            frame = frame == 1 ? 1 : frame == 3 ? 2 : 0;
            _homeWalker.sprite = PixelArt.Get("lc_player_right_" + frame);
            _homeWalker.transform.localPosition = new Vector3(_homeWalkerX, bottom + roadH * 0.65f, 0f);
            _homeWalker.transform.localScale = Vector3.one * 1.3f;

            // Bus sesekali lewat ke kiri (di lajur bawah, di depan tokoh).
            if (_homeBusWait > 0f)
            {
                _homeBusWait -= dt;
                _homeBus.gameObject.SetActive(false);
                _homeBusX = halfW + 5f;
            }
            else
            {
                _homeBus.gameObject.SetActive(true);
                _homeBusX -= dt * 6f;
                _homeBus.transform.localPosition = new Vector3(_homeBusX, bottom + 0.1f, 0f);
                if (_homeBusX < -halfW - 6f) _homeBusWait = 9f + (_time % 5f);
            }
            _homeBus.sortingOrder = 3959;
        }
    }
}
