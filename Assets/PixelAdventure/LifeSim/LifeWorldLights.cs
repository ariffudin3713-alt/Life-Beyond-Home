using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Cahaya malam. Lapisan gelap malam ada di HUD (urutan gambar 3000), jadi cahaya digambar
    /// DI ATASNYA (urutan 3100) agar benar-benar menembus kegelapan: kepala lampu jalan + genangan
    /// cahaya di tanah, jendela & pintu bangunan, lampu warung/gerobak, dan lampu depan kendaraan.
    /// Kekuatan cahaya mengikuti tingkat malam (0 = siang, 1 = malam pekat).
    /// </summary>
    public partial class LifeWorld
    {
        public const int LightOrder = 3100;
        public static readonly Color LampWarm = new Color(1f, 0.86f, 0.55f);
        static readonly Color WindowWarm = new Color(1f, 0.74f, 0.40f);
        static readonly Color DoorWarm = new Color(1f, 0.82f, 0.50f);

        /// <summary>Tingkat malam saat ini (dipakai kendaraan, hewan, dll).</summary>
        public static float NightLevel { get; private set; }

        struct NightLight
        {
            public SpriteRenderer Sr;
            public float Base;       // kecerahan penuh saat malam pekat
            public float Flicker;    // 0 = stabil; > 0 = berkedip lembut (lampu minyak/senthir)
        }

        readonly List<NightLight> _nightLights = new List<NightLight>();

        /// <summary>Tambah satu sumber cahaya (sprite glow) di posisi dunia.</summary>
        SpriteRenderer AddLight(float x, float y, float sx, float sy, Color color, float baseAlpha, float flicker = 0f)
        {
            var sr = Make("lc_glow", x, y, "Cahaya", LightOrder);
            sr.color = new Color(color.r, color.g, color.b, 0f);
            sr.transform.localScale = new Vector3(sx, sy, 1f);
            sr.gameObject.SetActive(false);
            _nightLights.Add(new NightLight { Sr = sr, Base = baseAlpha, Flicker = flicker });
            return sr;
        }

        /// <summary>Cahaya bangunan: jendela berpendar & pintu terang.</summary>
        void AddBuildingLights(float x, float y, float w, float h, bool door = true, float windowAlpha = 0.32f)
        {
            AddLight(x, y + h * 0.55f, w / 1.5f * 0.95f, h / 1.5f * 0.55f, WindowWarm, windowAlpha);
            if (door) AddLight(x, y + 0.45f, 1.5f, 1.25f, DoorWarm, 0.5f);
        }

        public void SetNight(float amount)
        {
            NightLevel = Mathf.Clamp01(amount);
            bool on = NightLevel > 0.02f;
            float t = Time.unscaledTime;
            for (int i = 0; i < _nightLights.Count; i++)
            {
                var l = _nightLights[i];
                if (l.Sr == null) continue;
                if (l.Sr.gameObject.activeSelf != on) l.Sr.gameObject.SetActive(on);
                if (!on) continue;
                float a = l.Base * NightLevel;
                if (l.Flicker > 0f) a *= 1f - l.Flicker * (0.5f + 0.5f * Mathf.Sin(t * 9f + i * 1.7f));
                Color c = l.Sr.color;
                c.a = a;
                l.Sr.color = c;
            }
        }
    }
}
