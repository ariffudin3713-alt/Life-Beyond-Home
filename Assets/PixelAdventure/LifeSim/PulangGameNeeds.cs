using PixelAdventure.LifeSim.Pulang;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Kebutuhan tubuh: LAPAR dan HAUS (0..100; 100 = kenyang / segar).
    /// - Berkurang seiring jam permainan (haus lebih cepat daripada lapar).
    /// - Makanan mengisi LAPAR, minuman (ItemDef.Drink) mengisi HAUS; makanan berkuah
    ///   (ItemDef.Thirst) sedikit mengisi HAUS juga.
    /// - Bila salah satu habis, energi terkuras perlahan sampai pemain makan / minum.
    /// Ditampilkan di panel kanan atas HUD (menggantikan bar energi).
    /// </summary>
    public partial class PulangGame
    {
        const float HungerPerMinute = 0.08f;   // 100 -> 0 dalam ±21 jam permainan
        const float ThirstPerMinute = 0.12f;   // 100 -> 0 dalam ±14 jam permainan
        const float NeedWarnAt = 25f;          // peringatan "mulai lapar / haus"
        const float StarveEnergyEvery = 5f;    // menit permainan per -1 energi saat lapar/haus habis

        float _starveTimer;

        /// <summary>Kurangi lapar & haus sebanyak <paramref name="minutes"/> menit permainan.</summary>
        void TickNeeds(float minutes)
        {
            if (_story.Data == null || minutes <= 0f) return;
            float hungerBefore = D.hunger, thirstBefore = D.thirst;
            D.hunger = Mathf.Clamp(D.hunger - minutes * HungerPerMinute, 0f, 100f);
            D.thirst = Mathf.Clamp(D.thirst - minutes * ThirstPerMinute, 0f, 100f);

            if (hungerBefore > NeedWarnAt && D.hunger <= NeedWarnAt)
                _hud.ShowToast("PERUTMU MULAI LAPAR - MAKAN BEKAL ATAU MAMPIR KE WARUNG");
            else if (thirstBefore > NeedWarnAt && D.thirst <= NeedWarnAt)
                _hud.ShowToast("KAMU MULAI HAUS - BELI MINUMAN DI WARUNG");

            if (D.hunger <= 0f || D.thirst <= 0f)
            {
                _starveTimer += minutes;
                while (_starveTimer >= StarveEnergyEvery)
                {
                    _starveTimer -= StarveEnergyEvery;
                    if (D.energy > 0) D.energy--;
                }
                if ((hungerBefore > 0f && D.hunger <= 0f) || (thirstBefore > 0f && D.thirst <= 0f))
                {
                    GameAudio.Play("error", 0.4f);
                    _hud.ShowToast(D.hunger <= 0f ? "KAMU KELAPARAN - ENERGI TERUS BERKURANG"
                                                  : "KAMU KEHAUSAN - ENERGI TERUS BERKURANG");
                }
            }
            else _starveTimer = 0f;
        }

        /// <summary>Isi lapar / haus dari makanan atau minuman; teks mis. "LAPAR 80/100".</summary>
        string FeedNeeds(ItemDef it)
        {
            if (it == null || it.Energy <= 0) return "";
            string text = "";
            if (it.Drink)
            {
                D.thirst = Mathf.Min(100f, D.thirst + 30f + it.Energy * 2f);
                text = "HAUS " + NeedValue(D.thirst) + "/100";
            }
            else
            {
                D.hunger = Mathf.Min(100f, D.hunger + it.Energy * 2f);
                text = "LAPAR " + NeedValue(D.hunger) + "/100";
                if (it.Thirst > 0)
                {
                    D.thirst = Mathf.Min(100f, D.thirst + it.Thirst);
                    text += "  HAUS " + NeedValue(D.thirst) + "/100";
                }
            }
            return text;
        }

        static int NeedValue(float v) => Mathf.Clamp(Mathf.CeilToInt(v), 0, 100);

        /// <summary>Teks kegunaan di inventori, mis. "+30 LAPAR" / "+46 HAUS".</summary>
        static string NeedEffect(ItemDef it)
        {
            if (it.Drink) return "+" + Mathf.RoundToInt(30f + it.Energy * 2f) + " HAUS";
            string s = "+" + it.Energy * 2 + " LAPAR";
            if (it.Thirst > 0) s += "  +" + it.Thirst + " HAUS";
            return s;
        }
    }
}
