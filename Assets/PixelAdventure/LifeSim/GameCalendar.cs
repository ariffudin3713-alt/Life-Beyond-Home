using System;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Kalender game: HARI 1 = SENIN, 5 JANUARI 2026.
    /// Dipilih hari Senin agar cocok dengan aturan lama "D.day % 7 == 0 = hari Minggu"
    /// (HARI 7 = MINGGU, 11 JANUARI 2026). Lewat 31 Desember 2026 kalender lanjut ke 2027.
    /// </summary>
    public static class GameCalendar
    {
        public static readonly DateTime Start = new DateTime(2026, 1, 5);

        static readonly string[] DayNames = { "MINGGU", "SENIN", "SELASA", "RABU", "KAMIS", "JUMAT", "SABTU" };
        static readonly string[] DayShort = { "MIN", "SEN", "SEL", "RAB", "KAM", "JUM", "SAB" };
        static readonly string[] Months =
        {
            "JANUARI", "FEBRUARI", "MARET", "APRIL", "MEI", "JUNI",
            "JULI", "AGUSTUS", "SEPTEMBER", "OKTOBER", "NOVEMBER", "DESEMBER",
        };
        static readonly string[] MonthShort =
            { "JAN", "FEB", "MAR", "APR", "MEI", "JUN", "JUL", "AGU", "SEP", "OKT", "NOV", "DES" };

        /// <summary>Tanggal kalender untuk hari ke-<paramref name="day"/> (mulai 1).</summary>
        public static DateTime DateFor(int day) => Start.AddDays(Math.Max(1, day) - 1);

        /// <summary>"SENIN"</summary>
        public static string DayName(int day) => DayNames[(int)DateFor(day).DayOfWeek];

        /// <summary>"5 JANUARI"</summary>
        public static string DayMonth(int day)
        {
            var d = DateFor(day);
            return d.Day + " " + Months[d.Month - 1];
        }

        /// <summary>"SENIN, 5 JANUARI 2026"</summary>
        public static string LongDate(int day)
        {
            var d = DateFor(day);
            return DayNames[(int)d.DayOfWeek] + ", " + d.Day + " " + Months[d.Month - 1] + " " + d.Year;
        }

        /// <summary>"SEN, 5 JAN 2026"</summary>
        public static string ShortDate(int day)
        {
            var d = DateFor(day);
            return DayShort[(int)d.DayOfWeek] + ", " + d.Day + " " + MonthShort[d.Month - 1] + " " + d.Year;
        }

        /// <summary>"SEN 5 JAN"</summary>
        public static string CompactDate(int day)
        {
            var d = DateFor(day);
            return DayShort[(int)d.DayOfWeek] + " " + d.Day + " " + MonthShort[d.Month - 1];
        }

        /// <summary>Tanggal terpanjang yang muat dalam <paramref name="maxChars"/> huruf.</summary>
        public static string Fit(int day, int maxChars)
        {
            string s = LongDate(day);
            if (s.Length <= maxChars) return s;
            s = ShortDate(day);
            return s.Length <= maxChars ? s : CompactDate(day);
        }
    }
}
