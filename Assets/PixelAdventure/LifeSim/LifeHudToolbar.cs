using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Bilah tombol kecil di kanan bawah (di atas tombol PANDUAN) agar semua fitur utama bisa
    /// dibuka dengan kursor / sentuhan: TAS, TUGAS, JURNAL, PETA, TABLET, JEDA.
    /// Arahkan kursor ke tombol = keterangan + tombol keyboardnya muncul di atas bilah.
    /// Urutan indeks = <see cref="ToolbarLabels"/> (dipakai PulangGame untuk menjalankan aksinya).
    /// </summary>
    public partial class LifeHud
    {
        public static readonly string[] ToolbarLabels = { "TAS (I)", "TUGAS (T)", "JURNAL (J)", "PETA (F)", "TABLET (TAB)", "JEDA (ESC)" };
        static readonly string[] ToolbarIcons = { "lc_app_bag", "lc_app_tasks", "lc_app_journal", "lc_app_map", "lc_app_tablet", "lc_app_pause" };

        const float ToolBtn = 1.15f, ToolGap = 0.14f;

        readonly SpriteRenderer[] _tbBg = new SpriteRenderer[6];
        readonly SpriteRenderer[] _tbIcon = new SpriteRenderer[6];
        SpriteRenderer _tbTipBg, _tbTip;
        int _tbTipShown = -1;

        void BuildToolbar()
        {
            for (int i = 0; i < ToolbarIcons.Length; i++)
            {
                _tbBg[i] = Panel(_hudGroup, "ToolBtn" + i, "lc_panel_soft", 3990);
                _tbBg[i].color = new Color(1f, 1f, 1f, 0.92f);
                _tbIcon[i] = Make(_hudGroup, PixelArt.Get(ToolbarIcons[i]), "ToolIcon" + i, 4001);
            }
            _tbTipBg = Solid(_hudGroup, "ToolTipBg", new Color(0.06f, 0.05f, 0.10f, 0.92f), 4002);
            _tbTip = Text(_hudGroup, "ToolTip", " ", 1, Gold, new Vector2(0.5f, 0.5f), 4003);
            _tbTipBg.gameObject.SetActive(false);
            _tbTip.gameObject.SetActive(false);
        }

        bool _playInput;
        float _toolEdgeLeft = 999f, _toolEdgeTop = -999f;
        float ToolbarWidth => ToolbarIcons.Length * ToolBtn + (ToolbarIcons.Length - 1) * ToolGap;

        /// <summary>
        /// Diatur PulangGame tiap frame: true hanya saat bermain bebas (bukan di menu, dialog, atau layar lain).
        /// Bilah tombol hanya tampil saat itu, dan tombol HUD lain (panduan, minimap) hanya bisa diklik saat itu.
        /// </summary>
        public void SetPlayInput(bool on) => _playInput = on;

        /// <summary>Zona klik HUD bermain (panduan, minimap, panel status) aktif?</summary>
        bool PlayHitsOn => _playInput && _hudGroup != null && _hudGroup.gameObject.activeSelf;

        void LayoutToolbar(float halfW, float halfH)
        {
            if (_tbBg[0] == null) return;
            bool visible = PlayHitsOn;
            // Batas pojok kanan bawah (tombol PANDUAN + bilah) agar penunjuk misi & prompt tidak menimpanya.
            _toolEdgeLeft = halfW - 0.28f - Mathf.Max(4.6f, visible ? ToolbarWidth : 0f);
            _toolEdgeTop = -halfH + 0.28f + 1.05f + (visible ? 0.18f + ToolBtn : 0f);
            for (int i = 0; i < _tbBg.Length; i++)
            {
                if (_tbBg[i].gameObject.activeSelf == visible) continue;
                _tbBg[i].gameObject.SetActive(visible);
                _tbIcon[i].gameObject.SetActive(visible);
            }
            if (!visible)
            {
                if (_tbTip.gameObject.activeSelf) { _tbTip.gameObject.SetActive(false); _tbTipBg.gameObject.SetActive(false); }
                return;
            }
            int n = ToolbarIcons.Length;
            float right = halfW - 0.28f;
            float cy = -halfH + 0.28f + 1.05f + 0.18f + ToolBtn * 0.5f;   // tepat di atas tombol PANDUAN
            float x0 = right - n * ToolBtn - (n - 1) * ToolGap + ToolBtn * 0.5f;

            bool hover = PointerOver(UiHit.Toolbar, out int over);
            for (int i = 0; i < n; i++)
            {
                float cx = x0 + i * (ToolBtn + ToolGap);
                bool on = hover && over == i;
                SetRect(_tbBg[i], cx, cy, ToolBtn, ToolBtn);
                _tbIcon[i].transform.localPosition = new Vector3(cx, cy, 0f);
                _tbIcon[i].transform.localScale = Vector3.one * (on ? 0.82f : 0.7f);
                Hit(UiHit.Toolbar, i, RectAt(cx, cy, ToolBtn, ToolBtn), 0);
            }

            // Keterangan tombol yang sedang ditunjuk.
            bool tip = hover && over >= 0 && over < n;
            if (_tbTip.gameObject.activeSelf != tip) { _tbTip.gameObject.SetActive(tip); _tbTipBg.gameObject.SetActive(tip); }
            if (!tip) return;
            if (over != _tbTipShown)
            {
                _tbTipShown = over;
                SetText(_tbTip, ToolbarLabels[over], Gold, 1, new Vector2(0.5f, 0.5f));
            }
            float tw = ToolbarLabels[over].Length * UiCharW + 0.4f;
            float tx = Mathf.Min(x0 + over * (ToolBtn + ToolGap), right - tw * 0.5f);
            float ty = cy + ToolBtn * 0.5f + 0.42f;
            SetSolid(_tbTipBg, tx, ty, tw, 0.52f);
            _tbTip.transform.localPosition = new Vector3(tx, ty, 0f);
        }
    }
}
