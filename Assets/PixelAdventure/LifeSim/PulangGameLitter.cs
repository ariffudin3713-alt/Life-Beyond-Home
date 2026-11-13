using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// "Buang sampah pada tempatnya": sampah berserakan bisa dipungut (maks. 5 di tangan) lalu
    /// dibuang ke tong sampah tiga warna. Setiap sampah yang dibuang menambah reputasi & EXP.
    /// Sampah yang sudah dipungut muncul lagi keesokan harinya.
    /// </summary>
    public partial class PulangGame
    {
        const int LitterMax = 5;
        int _litterCarried;
        readonly Dictionary<GameObject, int> _litterPickedDay = new Dictionary<GameObject, int>();

        void RefreshLitter(GameObject go)
        {
            if (go.activeSelf) return;
            if (_litterPickedDay.TryGetValue(go, out int day) && day == D.day) return;
            go.SetActive(true);
        }

        void PickLitter(GameObject go)
        {
            if (_litterCarried >= LitterMax)
            {
                Say(" ", "Tanganmu sudah penuh sampah. Buang dulu ke tong sampah terdekat.", null);
                return;
            }
            go.SetActive(false);
            _litterPickedDay[go] = D.day;
            _litterCarried++;
            GameAudio.Play("pickup", 0.45f, 1.15f);
            _hud.ShowToast("SAMPAH DIPUNGUT (" + _litterCarried + "/" + LitterMax + ")");
        }

        void InspectDecor(DecorBuilding d)
        {
            if (!d.Bin || _litterCarried == 0) { Say(" ", d.Message, null); return; }

            int n = _litterCarried;
            _litterCarried = 0;
            D.reputation += n;
            GainExp(n * 2);
            GameAudio.Play("quest_done", 0.4f);
            _hud.ShowToast("+" + n + " REPUTASI - TERIMA KASIH SUDAH MENJAGA KEBERSIHAN!");
        }
    }
}
