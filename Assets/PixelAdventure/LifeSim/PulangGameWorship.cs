using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Ibadah di Masjid Al-Ikhlas (sholat sesuai waktu: Subuh, Dhuha, Dzuhur, Ashar, Maghrib, Isya)
    /// dan Gereja Kasih Karunia (berdoa; setiap hari ke-7 ada Ibadah Minggu).
    /// Ibadah memakan waktu 20 menit, menenangkan hati (+energi), menambah reputasi & EXP.
    /// Hadiah hanya sekali per waktu sholat / sekali per hari di gereja.
    /// </summary>
    public partial class PulangGame
    {
        string PrayerName()
        {
            int m = D.minutes;
            if (m < 270) return "Isya";
            if (m < 360) return "Subuh";
            if (m < 720) return "Dhuha";
            if (m < 900) return "Dzuhur";
            if (m < 1080) return "Ashar";
            if (m < 1155) return "Maghrib";
            return "Isya";
        }

        string PrayerLabel() => "Sholat " + PrayerName() + (PrayerName() == "Dhuha" ? " (sunnah)" : " berjamaah");

        bool IsSunday => D.day % 7 == 0;

        string ChurchLabel() => IsSunday ? "Ikut Ibadah Minggu" : "Berdoa";

        void Worship(bool mosque)
        {
            string key = mosque ? "sholat_" + PrayerName() : "gereja";
            bool first = !_today.Contains(key);
            _today.Add(key);
            _clock = Mathf.Min(1439f, _clock + 20f);
            GameAudio.Play("page", 0.35f, 0.8f);

            string text;
            if (mosque)
                text = first
                    ? "Kamu berwudhu, lalu menunaikan sholat " + PrayerName() + (PrayerName() == "Dhuha" ? "" : " berjamaah") +
                      ". Selesai berdoa, hati terasa jauh lebih tenang. Semoga Ibu di kampung selalu sehat."
                    : "Kamu sudah sholat " + PrayerName() + " tadi. Kamu duduk sebentar berdzikir sebelum melanjutkan hari.";
            else
                text = first
                    ? (IsSunday
                        ? "Kamu ikut Ibadah Minggu bersama jemaat. Nyanyian pujian dan khotbah hari ini membuat hati terasa hangat."
                        : "Kamu duduk di bangku gereja dan berdoa dengan khusyuk. Suasana yang hening membuat hati terasa damai.")
                    : "Kamu sudah berdoa di sini hari ini. Kamu duduk sejenak menikmati keheningan.";

            Say(" ", text, () =>
            {
                if (!first) return;
                string energy = GainEnergy(mosque ? 10 : 12);
                D.reputation += 1;
                GainExp(3);
                GameAudio.Play("quest_done", 0.35f);
                _hud.ShowToast(("HATI TENANG " + energy + " +1 REPUTASI").ToUpperInvariant());
                _story.Tick();
            });
        }
    }
}
