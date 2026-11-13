using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>Satu aplikasi di layar tablet.</summary>
    public struct TabletApp
    {
        public string Label, Desc, Icon;
        public Color Tile;
        public bool Enabled;
    }

    /// <summary>
    /// TABLET (tombol TAB): perangkat dengan bilah status (hari, jam, energi, uang) dan
    /// grid aplikasi 4x2 sebagai shortcut ke tugas, inventori, jurnal, telepon, simpan, dll.
    /// </summary>
    public partial class LifeHud
    {
        const int TabletAppsMax = 12;
        const int TabletCols = 5;

        Transform _tabGroup;
        SpriteRenderer _tabBezel, _tabScreen, _tabStatusBar, _tabStatusL, _tabStatusR, _tabCamera, _tabHomeBtn;
        SpriteRenderer _tabTitle, _tabDesc, _tabMessage, _tabHint, _tabFrame;
        readonly SpriteRenderer[] _tabTiles = new SpriteRenderer[TabletAppsMax];
        readonly SpriteRenderer[] _tabIcons = new SpriteRenderer[TabletAppsMax];
        readonly SpriteRenderer[] _tabLabels = new SpriteRenderer[TabletAppsMax];
        int _tabCount, _tabSel;

        void BuildTablet()
        {
            _tabGroup = Group("Tablet");
            _tabBezel = Panel(_tabGroup, "Bezel", "lc_panel", 4010);
            _tabBezel.color = new Color(0.55f, 0.55f, 0.62f, 1f);
            _tabScreen = Solid(_tabGroup, "Screen", new Color(0.09f, 0.14f, 0.24f, 1f), 4011);
            _tabStatusBar = Solid(_tabGroup, "StatusBar", new Color(0.05f, 0.08f, 0.15f, 1f), 4012);
            _tabStatusL = Text(_tabGroup, "StatusL", " ", 1, Soft, new Vector2(0f, 0.5f), 4014);
            _tabStatusR = Text(_tabGroup, "StatusR", " ", 1, Soft, new Vector2(1f, 0.5f), 4014);
            _tabCamera = Solid(_tabGroup, "Camera", new Color(0.05f, 0.05f, 0.08f, 1f), 4012);
            _tabHomeBtn = Solid(_tabGroup, "HomeButton", new Color(0.30f, 0.30f, 0.36f, 1f), 4012);
            _tabTitle = Text(_tabGroup, "Title", " ", 2, Ink, new Vector2(0.5f, 0.5f), 4014);
            _tabFrame = Solid(_tabGroup, "Frame", new Color(1f, 0.92f, 0.60f, 1f), 4012);
            for (int i = 0; i < TabletAppsMax; i++)
            {
                _tabTiles[i] = Solid(_tabGroup, "Tile" + i, Color.gray, 4013);
                _tabIcons[i] = Make(_tabGroup, PixelArt.Get("lc_app_close"), "Icon" + i, 4014);
                _tabLabels[i] = Text(_tabGroup, "Label" + i, " ", 1, Ink, new Vector2(0.5f, 0.5f), 4014);
            }
            _tabDesc = Text(_tabGroup, "Desc", " ", 1, Soft, new Vector2(0.5f, 0.5f), 4014);
            _tabMessage = Text(_tabGroup, "Message", " ", 1, Pink, new Vector2(0.5f, 0.5f), 4014);
            _tabHint = Text(_tabGroup, "Hint", "WASD - PILIH   E - BUKA   TAB / ESC - TUTUP", 1, Gold,
                new Vector2(0.5f, 0.5f), 4014);
            _tabGroup.gameObject.SetActive(false);
        }

        public bool TabletVisible => _tabGroup != null && _tabGroup.gameObject.activeSelf;

        public void ShowTablet(string title, string statusLeft, string statusRight,
            List<TabletApp> apps, int selected, string message)
        {
            _tabCount = Mathf.Min(apps.Count, TabletAppsMax);
            _tabSel = Mathf.Clamp(selected, 0, Mathf.Max(0, _tabCount - 1));
            SetText(_tabTitle, title.ToUpperInvariant(), Ink, 2, new Vector2(0.5f, 0.5f));
            SetText(_tabStatusL, statusLeft.ToUpperInvariant(), Soft, 1, new Vector2(0f, 0.5f));
            SetText(_tabStatusR, statusRight.ToUpperInvariant(), Soft, 1, new Vector2(1f, 0.5f));
            for (int i = 0; i < TabletAppsMax; i++)
            {
                bool show = i < _tabCount;
                _tabTiles[i].gameObject.SetActive(show);
                _tabIcons[i].gameObject.SetActive(show);
                _tabLabels[i].gameObject.SetActive(show);
                if (!show) continue;
                var a = apps[i];
                bool sel = i == _tabSel;
                Color tile = a.Enabled ? a.Tile : new Color(0.25f, 0.25f, 0.30f, 1f);
                _tabTiles[i].color = tile;
                _tabIcons[i].sprite = PixelArt.Get(a.Icon);
                _tabIcons[i].color = a.Enabled ? Color.white : new Color(1f, 1f, 1f, 0.4f);
                SetText(_tabLabels[i], a.Label.ToUpperInvariant(), sel ? Gold : a.Enabled ? Ink : Soft, 1,
                    new Vector2(0.5f, 0.5f));
            }
            string desc = _tabCount > 0 ? apps[_tabSel].Desc : " ";
            SetText(_tabDesc, string.IsNullOrEmpty(desc) ? " " : desc.ToUpperInvariant(), Soft, 1, new Vector2(0.5f, 0.5f));
            SetText(_tabMessage, string.IsNullOrEmpty(message) ? " " : message.ToUpperInvariant(), Pink, 1,
                new Vector2(0.5f, 0.5f));
            SetDim(0.6f);
            _tabGroup.gameObject.SetActive(true);
            HidePrompt();
        }

        public void HideTablet()
        {
            if (_tabGroup != null) _tabGroup.gameObject.SetActive(false);
            SetDim(0f);
        }

        void LayoutTablet(float halfW, float halfH)
        {
            if (!TabletVisible) return;
            float w = Mathf.Min(halfW * 2f - 2f, 24f);
            float h = Mathf.Min(halfH * 2f - 1.2f, 14.4f);
            SetRect(_tabBezel, 0f, 0f, w, h);
            CloseAt(w * 0.5f, h * 0.5f);

            float sw = w - 1.3f, sh = h - 1.9f;
            float scy = 0.1f;
            SetSolid(_tabScreen, 0f, scy, sw, sh);
            float sTop = scy + sh * 0.5f, sBottom = scy - sh * 0.5f, sLeft = -sw * 0.5f;
            SetSolid(_tabCamera, 0f, h * 0.5f - 0.42f, 0.22f, 0.22f);
            SetSolid(_tabHomeBtn, 0f, -h * 0.5f + 0.45f, 0.9f, 0.26f);

            SetSolid(_tabStatusBar, 0f, sTop - 0.3f, sw, 0.6f);
            _tabStatusL.transform.localPosition = new Vector3(sLeft + 0.35f, sTop - 0.3f, 0f);
            _tabStatusR.transform.localPosition = new Vector3(-sLeft - 0.35f, sTop - 0.3f, 0f);
            _tabTitle.transform.localPosition = new Vector3(0f, sTop - 1.35f, 0f);

            // Grid aplikasi 4x2 di tengah layar tablet.
            int rows = Mathf.CeilToInt(_tabCount / (float)TabletCols);
            float tile = Mathf.Min(2.1f, (sw - 1.5f) / TabletCols - 0.9f);
            float cellW = (sw - 1.0f) / TabletCols;
            float cellH = tile + 1.0f;
            float gridTop = sTop - 2.35f;
            float bob = Mathf.Sin(_time * 5f) * 0.05f;
            for (int i = 0; i < _tabCount; i++)
            {
                int col = i % TabletCols, row = i / TabletCols;
                float cx = sLeft + 0.5f + cellW * (col + 0.5f);
                float cy = gridTop - tile * 0.5f - row * cellH;
                bool sel = i == _tabSel;
                if (sel)
                {
                    cy += bob;
                    SetSolid(_tabFrame, cx, cy, tile + 0.22f, tile + 0.22f);
                }
                SetSolid(_tabTiles[i], cx, cy, tile, tile);
                _tabIcons[i].transform.localPosition = new Vector3(cx, cy, 0f);
                _tabIcons[i].transform.localScale = Vector3.one * (tile * 0.7f);
                _tabLabels[i].transform.localPosition = new Vector3(cx, cy - tile * 0.5f - 0.38f, 0f);
            }
            _tabFrame.gameObject.SetActive(_tabCount > 0);

            float below = gridTop - rows * cellH;
            _tabDesc.transform.localPosition = new Vector3(0f, Mathf.Max(below - 0.1f, sBottom + 1.35f), 0f);
            _tabMessage.transform.localPosition = new Vector3(0f, sBottom + 0.95f, 0f);
            _tabHint.transform.localPosition = new Vector3(0f, sBottom + 0.4f, 0f);
        }
    }
}
