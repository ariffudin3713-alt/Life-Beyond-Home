using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>Jenis zona yang bisa diklik / disorot kursor.</summary>
    public enum UiHit
    {
        Blocker,        // panel HUD tanpa aksi: hanya mencegah klik tembus menjadi "jalan ke sini"
        Close, Toolbar, GuideButton, Minimap,
        Menu, Pause, TabletApp,
        InvTab, InvRow,
        ContactTab, ContactRow, ContactAction, Picker,
        TaskCard, GuidePage,
        WorkChoice, WorkArrow, WorkAim,
        HomeOption, SlotCard,
        SettingsRow, MapRegion, MapPlace,
        AmountKey, AmountChip, AmountButton,
    }

    /// <summary>
    /// Dukungan kursor untuk semua layar.
    /// Setiap Layout* mendaftarkan kotak tombolnya lewat <see cref="Hit"/> (koordinat HUD) setiap frame.
    /// PulangGame lalu bertanya:
    /// - <see cref="PointerHover"/> : kursor baru bergerak di atas zona itu (dipakai untuk memilih);
    /// - <see cref="PointerClick"/> : zona itu diklik / diketuk pada frame ini (dipakai untuk menjalankan);
    /// - <see cref="PointerOverUi"/> : kursor di atas panel HUD (klik tidak diteruskan ke dunia).
    /// Zona paling atas (lapisan terbesar, lalu yang terakhir didaftarkan) yang menang.
    /// Di PC, zona di bawah kursor diberi bingkai emas tipis sebagai tanda bisa diklik.
    /// </summary>
    public partial class LifeHud
    {
        struct HitZone
        {
            public UiHit Kind;
            public int Index, Layer;
            public Rect Rect;
            public bool Glow;
        }

        List<HitZone> _hitsBuild = new List<HitZone>();
        List<HitZone> _hitsReady = new List<HitZone>();
        readonly SpriteRenderer[] _hoverGlow = new SpriteRenderer[4];
        Vector2 _lastMouse;
        bool _pointerMoved;
        static readonly Color HoverGlow = new Color(1f, 0.84f, 0.40f, 0.9f);

        void BuildPointer()
        {
            for (int i = 0; i < 4; i++)
            {
                _hoverGlow[i] = Solid(_root, "HoverGlow" + i, HoverGlow, 4890);
                _hoverGlow[i].gameObject.SetActive(false);
            }
        }

        /// <summary>Daftarkan zona klik untuk frame ini.</summary>
        /// <param name="layer">Lapisan: 0 = HUD bermain, 1 = layar, 3 = menu / jendela di atas layar, 9 = tombol X.</param>
        /// <param name="glow">False untuk area besar (halaman buku, area bidik) agar tidak diberi bingkai.</param>
        void Hit(UiHit kind, int index, Rect rect, int layer = 1, bool glow = true)
        {
            _hitsBuild.Add(new HitZone { Kind = kind, Index = index, Rect = rect, Layer = layer, Glow = glow });
        }

        static Rect RectAt(float cx, float cy, float w, float h) => new Rect(cx - w * 0.5f, cy - h * 0.5f, w, h);

        void BeginPointerFrame()
        {
            _hitsBuild.Clear();
            Vector2 m = LifeInput.MousePosition;
            _pointerMoved = (m - _lastMouse).sqrMagnitude > 0.25f;
            _lastMouse = m;
        }

        /// <summary>Selesai mendaftarkan zona: pakai daftar baru & gambar bingkai sorotan.</summary>
        void EndPointerFrame()
        {
            var t = _hitsReady; _hitsReady = _hitsBuild; _hitsBuild = t;

            HitZone z = default;
            bool show = !Application.isMobilePlatform && TopHit(out z) && z.Glow;
            for (int i = 0; i < 4; i++)
                if (_hoverGlow[i].gameObject.activeSelf != show) _hoverGlow[i].gameObject.SetActive(show);
            if (!show) return;
            Rect r = z.Rect;
            const float t2 = 0.07f;
            float a = 0.9f;   // bingkai sorotan tetap, tidak berkedip
            var c = HoverGlow; c.a = a;
            SetSolid(_hoverGlow[0], r.center.x, r.yMax, r.width + t2, t2);
            SetSolid(_hoverGlow[1], r.center.x, r.yMin, r.width + t2, t2);
            SetSolid(_hoverGlow[2], r.xMin, r.center.y, t2, r.height + t2);
            SetSolid(_hoverGlow[3], r.xMax, r.center.y, t2, r.height + t2);
            for (int i = 0; i < 4; i++) _hoverGlow[i].color = c;
        }

        bool TopHit(out HitZone zone)
        {
            zone = default;
            if (_cam == null || _hitsReady.Count == 0) return false;
            Vector2 p = PointerToHud();
            bool found = false;
            for (int i = 0; i < _hitsReady.Count; i++)
            {
                var h = _hitsReady[i];
                if (!h.Rect.Contains(p)) continue;
                if (!found || h.Layer >= zone.Layer) { zone = h; found = true; }
            }
            return found;
        }

        /// <summary>Kursor (tanpa melihat gerakan) sedang di atas zona jenis ini.</summary>
        public bool PointerOver(UiHit kind, out int index)
        {
            index = -1;
            if (!TopHit(out HitZone z) || z.Kind != kind) return false;
            index = z.Index;
            return true;
        }

        /// <summary>Kursor baru saja bergerak di atas zona jenis ini (untuk menyorot / memilih).</summary>
        public bool PointerHover(UiHit kind, out int index)
        {
            index = -1;
            return _pointerMoved && PointerOver(kind, out index);
        }

        /// <summary>Zona jenis ini diklik / diketuk pada frame ini.</summary>
        public bool PointerClick(UiHit kind, out int index)
        {
            index = -1;
            return LifeInput.PointerTapped && PointerOver(kind, out index);
        }

        /// <summary>Posisi kursor di dalam zona, dinormalkan 0..1 (untuk area bidik).</summary>
        public bool PointerIn(UiHit kind, out Vector2 normalized)
        {
            normalized = Vector2.zero;
            if (!TopHit(out HitZone z) || z.Kind != kind) return false;
            Vector2 p = PointerToHud();
            normalized = new Vector2(Mathf.InverseLerp(z.Rect.xMin, z.Rect.xMax, p.x), Mathf.InverseLerp(z.Rect.yMin, z.Rect.yMax, p.y));
            return true;
        }

        /// <summary>Kursor bergerak sejak frame sebelumnya.</summary>
        public bool PointerMoved => _pointerMoved;

        /// <summary>Kursor berada di atas panel / tombol HUD mana pun.</summary>
        public bool PointerOverUi => TopHit(out _);
    }
}
