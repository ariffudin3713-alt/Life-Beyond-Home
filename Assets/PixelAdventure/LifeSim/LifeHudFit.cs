using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Agar tulisan tidak pernah keluar dari panel / menumpuk dengan tulisan lain.
    /// Petunjuk tombol ("W/S - PILIH   E / KLIK - OK   ESC - BATAL") diringkas bertahap bila
    /// tidak muat di lebar yang tersedia (layar 4:3, UKURAN UI besar):
    ///   1) jarak antarbagian dirapatkan;
    ///   2) alternatif kursor dibuang ("E / KLIK - OK" -> "E - OK");
    ///   3) bagian tengah dibuang satu per satu (bagian terakhir, biasanya "ESC - KEMBALI", tetap ada);
    ///   4) terakhir baru dipotong.
    /// </summary>
    public partial class LifeHud
    {
        static readonly Regex HintGap = new Regex(" {3,}");
        static readonly Regex HintAlt = new Regex(@" / (KLIK KANAN|KLIK TOMBOL|KLIK SLOT|KLIK HALAMAN|KLIK ◀ ▶|KLIK|KURSOR|RODA MOUSE|RODA|MOUSE|ENTER)");
        static readonly Dictionary<string, string> HintCache = new Dictionary<string, string>();

        /// <summary>Ringkas petunjuk tombol agar muat <paramref name="maxChars"/> huruf.</summary>
        public static string CompactHint(string text, int maxChars)
        {
            text = Safe(text);
            if (text.Length <= maxChars) return text;
            string key = maxChars + "|" + text;
            if (HintCache.TryGetValue(key, out string cached)) return cached;

            string s = HintGap.Replace(text, "  ");
            if (s.Length > maxChars) s = HintAlt.Replace(s, "");
            if (s.Length > maxChars)
            {
                var parts = new List<string>(s.Split(new[] { "  " }, System.StringSplitOptions.RemoveEmptyEntries));
                while (parts.Count > 2 && string.Join("  ", parts).Length > maxChars) parts.RemoveAt(parts.Count - 2);
                s = string.Join("  ", parts);
            }
            if (s.Length > maxChars) s = maxChars < 3 ? " " : s.Substring(0, maxChars - 2).TrimEnd() + "..";
            if (HintCache.Count > 512) HintCache.Clear();   // teks dinamis (jam, uang): jangan tumbuh tanpa batas
            HintCache[key] = s;
            return s;
        }

        // ------------------------------------------------------------------ petunjuk tetap (SpriteRenderer)

        struct HintState { public string Full; public Color Color; public int Chars; public float PivotX; }
        readonly Dictionary<SpriteRenderer, HintState> _hints = new Dictionary<SpriteRenderer, HintState>();

        /// <summary>Atur teks petunjuk (rata tengah); dirender ulang oleh <see cref="FitHint"/> sesuai lebar.</summary>
        void SetHint(SpriteRenderer sr, string full, Color color, float pivotX = 0.5f)
        {
            if (sr == null) return;
            if (_hints.TryGetValue(sr, out HintState h) && h.Full == full && h.Color == color && h.PivotX == pivotX) return;
            _hints[sr] = new HintState { Full = full, Color = color, Chars = -1, PivotX = pivotX };
        }

        /// <summary>Render petunjuk agar muat <paramref name="width"/> unit (hanya bila lebarnya berubah).</summary>
        void FitHint(SpriteRenderer sr, float width)
        {
            if (sr == null || !_hints.TryGetValue(sr, out HintState h)) return;
            int chars = CharsFor(width);
            if (chars == h.Chars) return;
            h.Chars = chars;
            _hints[sr] = h;
            SetText(sr, CompactHint(h.Full, chars), h.Color, 1, new Vector2(h.PivotX, 0.5f));
        }
    }
}
