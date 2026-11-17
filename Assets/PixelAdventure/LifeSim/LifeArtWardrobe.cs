using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Lemari pakaian tokoh utama: tiga slot (TOPI, BAJU, SEPATU) digabung menjadi satu set
    /// sprite "lc_player_{arah}_{frame}".
    /// - BAJU  : badan dari karakter lain di spritesheet (lihat OutfitCanvas di LifeArtSheetChars).
    /// - TOPI  : digambar prosedural di atas rambut (baris gambar 0..4), menyesuaikan lebar kepala.
    /// - SEPATU: kaki (baris gambar 25..27) diwarnai ulang sesuai gaya sepatu.
    ///
    /// Tiap kombinasi disimpan di cache kecil (LRU). Berganti ke kombinasi yang pernah dipakai
    /// tidak membuat tekstur baru, dan tekstur kombinasi lama dihancurkan saat keluar dari cache
    /// (dulu tiap ganti baju membuat 12 tekstur baru yang tidak pernah dibuang).
    /// </summary>
    public static partial class LifeArt
    {
        enum HatShape { Cap, Peci, Caping, Beanie, Bucket }
        enum ShoeShape { Sneaker, Formal, Sandal, Boot }

        struct HatStyle
        {
            public HatShape Shape;
            public Color32 Main, Shade, Light;
            public HatStyle(HatShape s, string main, string shade, string light)
            { Shape = s; Main = PixelArt.Hex(main); Shade = PixelArt.Hex(shade); Light = PixelArt.Hex(light); }
        }

        struct ShoeStyle
        {
            public ShoeShape Shape;
            public Color32 Main, Shade;
            public ShoeStyle(ShoeShape s, string main, string shade)
            { Shape = s; Main = PixelArt.Hex(main); Shade = PixelArt.Hex(shade); }
        }

        /// <summary>Gaya topi; indeks = ItemDef.Hat.</summary>
        static readonly HatStyle[] HatStyles =
        {
            new HatStyle(HatShape.Cap,    "d63a3a", "8e1f2a", "f07a6a"),   // 0 topi baseball merah
            new HatStyle(HatShape.Cap,    "2f4f9e", "1b2d5e", "5a7fd0"),   // 1 topi baseball biru
            new HatStyle(HatShape.Peci,   "24222c", "121118", "4a4656"),   // 2 peci hitam
            new HatStyle(HatShape.Caping, "d9b36a", "a07a3a", "f0d79a"),   // 3 caping bambu
            new HatStyle(HatShape.Beanie, "8a8a96", "5c5c68", "b4b4c0"),   // 4 kupluk rajut
            new HatStyle(HatShape.Bucket, "d8c8a0", "a8966c", "f0e4c4"),   // 5 topi bucket krem
        };

        /// <summary>Gaya sepatu; indeks = ItemDef.Shoes.</summary>
        static readonly ShoeStyle[] ShoeStyles =
        {
            new ShoeStyle(ShoeShape.Sneaker, "d63a3a", "eeeeee"),   // 0 kets merah (sol putih)
            new ShoeStyle(ShoeShape.Formal,  "2a2830", "121118"),   // 1 pantofel hitam
            new ShoeStyle(ShoeShape.Sandal,  "3a7bd5", "1f4f96"),   // 2 sandal jepit biru
            new ShoeStyle(ShoeShape.Boot,    "8a5a32", "5a3a1e"),   // 3 bot kulit coklat
        };

        public static int HatStyleCount => HatStyles.Length;
        public static int ShoeStyleCount => ShoeStyles.Length;

        const int LookCacheSize = 6;    // kombinasi pakaian yang disimpan
        const int IconCacheSize = 64;   // ikon pratinjau inventori

        static readonly Dictionary<long, Sprite[]> LookCache = new Dictionary<long, Sprite[]>();
        static readonly List<long> LookOrder = new List<long>();       // LRU: depan = paling lama
        static readonly List<string> IconOrder = new List<string>();
        static long _appliedLook = long.MinValue;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetWardrobeStatics()
        {
            _sheetPixels = null;
            ResetPlayerLook();
        }

        /// <summary>Lupakan cache pakaian (sprite lama sudah tidak berlaku, mis. sheet dimuat ulang).</summary>
        static void ResetPlayerLook()
        {
            foreach (var set in LookCache.Values) DestroySprites(set);
            LookCache.Clear();
            LookOrder.Clear();
            IconOrder.Clear();
            _appliedLook = long.MinValue;
        }

        static long LookKey(int body, int hat, int shoes) => ((long)(body + 1) << 32) | (uint)(((hat + 1) << 16) | (shoes + 1));

        static int ClampBody(int body) => body < 0 || _sheetPixels == null || body >= _sheetH / SheetFrameH ? -1 : body;
        static int ClampHat(int hat) => hat < 0 || hat >= HatStyles.Length ? -1 : hat;
        static int ClampShoes(int shoes) => shoes < 0 || shoes >= ShoeStyles.Length ? -1 : shoes;

        /// <summary>
        /// Pasang pakaian tokoh utama (nilai -1 = bawaan untuk slot itu). Sprite "lc_player_*"
        /// langsung diganti, jadi tokoh di dunia, potret dialog, dan profil ikut berubah seketika.
        /// Return false bila spritesheet karakter tidak ada.
        /// </summary>
        public static bool ApplyPlayerLook(int body, int hat, int shoes)
        {
            if (_sheetPixels == null) return false;
            body = ClampBody(body); hat = ClampHat(hat); shoes = ClampShoes(shoes);
            long key = LookKey(body, hat, shoes);
            if (key == _appliedLook && LookCache.ContainsKey(key)) return true;

            if (!LookCache.TryGetValue(key, out Sprite[] set) || !SetAlive(set))
            {
                set = new Sprite[12];
                for (int dir = 0; dir < 4; dir++)
                    for (int frame = 0; frame < 3; frame++)
                    {
                        var c = LookCanvas(body, hat, shoes, dir);
                        if (frame == 1) LiftLeg(c, 0, SheetFrameW / 2);
                        else if (frame == 2) LiftLeg(c, SheetFrameW / 2, SheetFrameW);
                        set[dir * 3 + frame] = c.ToSprite(new Vector2(0.5f, 0f));   // tanpa kunci: didaftarkan di bawah
                    }
                LookCache[key] = set;
            }
            Touch(LookOrder, key);
            _appliedLook = key;

            for (int dir = 0; dir < 4; dir++)
                for (int frame = 0; frame < 3; frame++)
                {
                    var s = set[dir * 3 + frame];
                    s.name = Prefix + "player_" + DirNames[dir] + "_" + frame;
                    PixelArt.Sprites[s.name] = s;
                }

            // Buang kombinasi lama (bukan yang sedang dipakai) agar memori tidak terus tumbuh.
            while (LookOrder.Count > LookCacheSize)
            {
                long old = LookOrder[0];
                LookOrder.RemoveAt(0);
                if (LookCache.TryGetValue(old, out Sprite[] oldSet)) DestroySprites(oldSet);
                LookCache.Remove(old);
            }
            return true;
        }

        /// <summary>Versi lama (hanya baju). Topi & sepatu dilepas.</summary>
        public static bool ApplyPlayerOutfit(int bodyRow) => ApplyPlayerLook(bodyRow, -1, -1);

        /// <summary>
        /// Ikon pratinjau: tokoh utama tampak depan memakai kombinasi ini. Kunci sprite dikembalikan
        /// (null bila sheet tidak ada). Dibuat sekali dan disimpan di cache kecil.
        /// </summary>
        public static string LookIcon(int body, int hat, int shoes)
        {
            if (_sheetPixels == null) return null;
            body = ClampBody(body); hat = ClampHat(hat); shoes = ClampShoes(shoes);
            string name = "look_" + body + "_" + hat + "_" + shoes;
            string key = Prefix + name;
            if (!PixelArt.Sprites.TryGetValue(key, out Sprite s) || s == null)
                Add(LookCanvas(body, hat, shoes, 0), new Vector2(0.5f, 0.5f), name);
            Touch(IconOrder, key);
            while (IconOrder.Count > IconCacheSize)
            {
                string old = IconOrder[0];
                IconOrder.RemoveAt(0);
                if (PixelArt.Sprites.TryGetValue(old, out Sprite os)) DestroySprite(os);
                PixelArt.Sprites.Remove(old);
            }
            return key;
        }

        /// <summary>Versi lama (hanya baju).</summary>
        public static string OutfitIcon(int bodyRow) => LookIcon(bodyRow, -1, -1);

        static void Touch<T>(List<T> order, T key)
        {
            int i = order.IndexOf(key);
            if (i >= 0) order.RemoveAt(i);
            order.Add(key);
        }

        static bool SetAlive(Sprite[] set)
        {
            if (set == null) return false;
            foreach (var s in set) if (s == null) return false;   // Unity null: sudah dihancurkan
            return true;
        }

        static void DestroySprites(Sprite[] set)
        {
            if (set == null) return;
            foreach (var s in set) DestroySprite(s);
        }

        static void DestroySprite(Sprite s)
        {
            if (s == null) return;
            var tex = s.texture;
            if (Application.isPlaying) { Object.Destroy(s); if (tex != null) Object.Destroy(tex); }
            else { Object.DestroyImmediate(s); if (tex != null) Object.DestroyImmediate(tex); }
        }

        // ------------------------------------------------------------------ menggambar

        /// <summary>Satu frame diam (arah game <paramref name="dir"/>) dengan semua slot terpasang.</summary>
        static PixelCanvas LookCanvas(int body, int hat, int shoes, int dir)
        {
            int col = SheetColumnForDir[dir];
            var c = OutfitCanvas(body < 0 ? PlayerSheetRow : body, col);
            if (hat >= 0) DrawHat(c, HatStyles[hat], dir);
            if (shoes >= 0) DrawShoes(c, ShoeStyles[shoes]);
            return c;
        }

        // Akses piksel dengan nomor baris gambar (0 = atas), seperti pada sheet.
        static Color32 PixAt(PixelCanvas c, int x, int iy) => c.Get(x, SheetFrameH - 1 - iy);
        static void PixSet(PixelCanvas c, int x, int iy, Color32 p)
        {
            if (x < 0 || x >= SheetFrameW || iy < 0 || iy >= SheetFrameH) return;
            c.Set(x, SheetFrameH - 1 - iy, p);
        }

        static Color32 Darken(Color32 c, float k) =>
            new Color32((byte)(c.r * k), (byte)(c.g * k), (byte)(c.b * k), 255);

        const int HatRows = 5;   // baris gambar 0..4 (rambut bagian atas)

        static void DrawHat(PixelCanvas c, HatStyle st, int dir)
        {
            // Lebar kepala diukur dari baris gambar 3 (bagian rambut terlebar) pada frame ini.
            int L = SheetFrameW, R = -1;
            for (int x = 0; x < SheetFrameW; x++)
                if (PixAt(c, x, 3).a > 0) { L = Mathf.Min(L, x); R = Mathf.Max(R, x); }
            if (R < L) return;
            int mid = (L + R + 1) / 2;
            bool left = dir == 2, right = dir == 3, back = dir == 1;

            // Bentuk topi per baris: [a..b] (b < a = kosong) dan apakah baris itu pinggiran topi.
            var a = new int[HatRows];
            var b = new int[HatRows];
            var brim = new bool[HatRows];
            for (int r = 0; r < HatRows; r++) { a[r] = 0; b[r] = -1; }
            void Row(int r, int from, int to, bool isBrim = false)
            {
                a[r] = Mathf.Max(0, from); b[r] = Mathf.Min(SheetFrameW - 1, to); brim[r] = isBrim;
            }

            switch (st.Shape)
            {
                case HatShape.Cap:
                    Row(0, L + 2, R - 2); Row(1, L + 1, R - 1); Row(2, L, R); Row(3, L, R);
                    if (left) Row(4, L - 3, mid, true);          // lidah topi menghadap ke depan
                    else if (right) Row(4, mid, R + 3, true);
                    else if (!back) Row(4, L, R, true);
                    break;
                case HatShape.Peci:
                    Row(1, L, R); Row(2, L, R); Row(3, L, R); Row(4, L, R);
                    break;
                case HatShape.Caping:
                    Row(0, mid - 1, mid); Row(1, mid - 2, mid + 1); Row(2, mid - 4, mid + 3);
                    Row(3, mid - 6, mid + 5); Row(4, mid - 8, mid + 7, true);
                    break;
                case HatShape.Beanie:
                    Row(0, L + 2, R - 2); Row(1, L + 1, R - 1); Row(2, L, R); Row(3, L, R); Row(4, L, R, true);
                    break;
                case HatShape.Bucket:
                    Row(0, L + 2, R - 2); Row(1, L + 1, R - 1); Row(2, L + 1, R - 1); Row(3, L, R);
                    Row(4, L - 1, R + 1, true);
                    break;
            }

            bool In(int x, int r) => r >= 0 && r < HatRows && x >= a[r] && x <= b[r];

            // Rambut runcing yang mencuat di atas topi dihapus.
            var clear = new Color32(0, 0, 0, 0);
            for (int r = 0; r < 2; r++)
                for (int x = 0; x < SheetFrameW; x++)
                    if (!In(x, r)) PixSet(c, x, r, clear);

            Color32 outline = Darken(st.Main, 0.35f);
            for (int r = 0; r < HatRows; r++)
            {
                for (int x = a[r]; x <= b[r]; x++)
                {
                    // Pinggiran (lidah topi) hanya bertepi di ujung kiri/kanan agar tidak hitam semua.
                    bool edge = !In(x - 1, r) || !In(x + 1, r) || (!brim[r] && !In(x, r - 1));
                    Color32 p;
                    if (edge) p = outline;
                    else if (brim[r]) p = st.Shape == HatShape.Beanie && (x & 1) == 0 ? st.Main : st.Shade;
                    else if (r == 1 && (st.Shape == HatShape.Peci || x <= a[r] + 2)) p = st.Light;   // kilau
                    else if (st.Shape == HatShape.Caping && r == 3) p = st.Shade;                       // anyaman
                    else p = st.Main;
                    PixSet(c, x, r, p);
                }
            }
        }

        static void DrawShoes(PixelCanvas c, ShoeStyle st)
        {
            Color32 skin = SheetPixel(PlayerSheetRow, 0, 7, HeadLastImageRow);
            int top = st.Shape == ShoeShape.Boot ? 25 : 26;
            for (int iy = top; iy < SheetFrameH; iy++)
            {
                for (int x = 0; x < SheetFrameW; x++)
                {
                    if (PixAt(c, x, iy).a == 0) continue;
                    bool runStart = x == 0 || PixAt(c, x - 1, iy).a == 0;
                    bool runEnd = x == SheetFrameW - 1 || PixAt(c, x + 1, iy).a == 0;
                    bool sole = iy == SheetFrameH - 1;
                    Color32 p;
                    switch (st.Shape)
                    {
                        case ShoeShape.Sneaker: p = sole ? st.Shade : st.Main; break;
                        case ShoeShape.Formal:
                            // Ujung sepatu mengilap (satu piksel terang di awal tiap kaki).
                            p = sole ? st.Shade : runStart && !runEnd ? new Color32(110, 106, 124, 255) : st.Main;
                            break;
                        case ShoeShape.Sandal:
                            // Jari kaki terlihat, tali di tengah, sol di bawah.
                            if (sole) p = st.Main;
                            else
                            {
                                int runL = x; while (runL > 0 && PixAt(c, runL - 1, iy).a > 0) runL--;
                                int runR = x; while (runR < SheetFrameW - 1 && PixAt(c, runR + 1, iy).a > 0) runR++;
                                p = x == (runL + runR) / 2 ? st.Shade : skin;
                            }
                            break;
                        default: p = sole ? st.Shade : st.Main; break;   // bot
                    }
                    PixSet(c, x, iy, p);
                }
            }
        }
    }
}
