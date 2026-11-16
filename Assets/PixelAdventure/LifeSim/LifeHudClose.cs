using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Tombol X merah di pojok kanan atas setiap layar yang bisa ditutup (menu pilihan, jeda,
    /// jurnal, tugas, inventori, tablet, profil, panduan, slot simpanan).
    /// Setiap Layout* layar memanggil <see cref="CloseAt"/> dengan pojok kanan atas panelnya;
    /// PulangGame menentukan kapan tombol boleh tampil dan memperlakukan kliknya sama dengan ESC.
    /// </summary>
    public partial class LifeHud
    {
        const float CloseSize = 0.9f;      // sisi tombol (unit)
        const float CloseHit = 1.3f;       // area ketuk sedikit lebih besar agar mudah di layar sentuh

        Transform _closeGroup;
        SpriteRenderer _closeBtn;
        Rect _closeRect;
        Vector2? _closeAt;
        bool _closeAllowed;
        int _closePriority;

        void BuildCloseButton()
        {
            MakeCloseSprite("btn_close", PixelArt.Hex("d8384a"), PixelArt.Hex("8a1e1a"));
            MakeCloseSprite("btn_close_hover", PixelArt.Hex("f05a5a"), PixelArt.Hex("a8322c"));
            _closeGroup = new GameObject("CloseButton").transform;
            _closeGroup.SetParent(_root, false);
            _closeBtn = Make(_closeGroup, PixelArt.Get("lc_btn_close"), "X", 4880);
            _closeGroup.gameObject.SetActive(false);
        }

        static void MakeCloseSprite(string key, Color32 fill, Color32 shade)
        {
            var c = new PixelCanvas(16, 16);
            Color32 ink = PixelArt.Hex("241a2b"), white = PixelArt.Hex("fff6e8");
            c.Rect(1, 0, 14, 16, ink); c.Rect(0, 1, 16, 14, ink);          // sudut membulat
            c.Rect(1, 1, 14, 14, shade);
            c.Rect(1, 3, 14, 12, fill);
            c.Rect(2, 13, 12, 1, Color32.Lerp(fill, white, 0.4f));        // kilap atas
            for (int i = 0; i < 7; i++)                                     // huruf X tebal
            {
                c.Rect(4 + i, 4 + i, 2, 2, white);
                c.Rect(10 - i, 4 + i, 2, 2, white);
            }
            var tex = c.ToTexture(FilterMode.Point, true);
            var s = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
            s.name = "lc_" + key;
            PixelArt.Sprites["lc_" + key] = s;
        }

        /// <summary>Boleh tampil? (diatur PulangGame tiap frame sesuai layar yang terbuka).</summary>
        public void SetCloseAllowed(bool allowed) => _closeAllowed = allowed;

        /// <summary>Daftarkan pojok kanan atas panel yang sedang tampil (panggilan terakhir yang dipakai).</summary>
        /// <param name="priority">Layar di lapisan atas (menu pilihan, jeda) menang atas panel di belakangnya.</param>
        void CloseAt(float right, float top, int priority = 1)
        {
            if (_closeAt.HasValue && priority < _closePriority) return;
            _closeAt = new Vector2(right, top);
            _closePriority = priority;
        }

        void LayoutCloseButton()
        {
            bool show = _closeAllowed && _closeAt.HasValue;
            if (_closeGroup.gameObject.activeSelf != show) _closeGroup.gameObject.SetActive(show);
            if (show)
            {
                // Menempel di pojok panel, sedikit keluar dari bingkainya (tidak menutupi isi).
                Vector2 p = _closeAt.Value + new Vector2(-0.12f, -0.12f);
                _closeBtn.transform.localPosition = new Vector3(p.x, p.y, 0f);
                _closeRect = new Rect(p.x - CloseHit * 0.5f, p.y - CloseHit * 0.5f, CloseHit, CloseHit);
                Hit(UiHit.Close, 0, _closeRect, 9);
                bool hover = _cam != null && _closeRect.Contains(PointerLocal());
                float pop = hover ? 1.12f : 1f;
                _closeBtn.sprite = PixelArt.Get(hover ? "lc_btn_close_hover" : "lc_btn_close");
                _closeBtn.transform.localScale = Vector3.one * (CloseSize * pop);
            }
            _closePriority = 0;
            _closeAt = null;   // didaftarkan ulang oleh layar yang tampil pada layout berikutnya
        }

        /// <summary>Tombol X diketuk / diklik pada frame ini.</summary>
        public bool CloseButtonClicked()
        {
            if (_cam == null || _closeGroup == null || !_closeGroup.gameObject.activeSelf || !LifeInput.PointerTapped)
                return false;
            return _closeRect.Contains(PointerLocal());
        }
    }
}
