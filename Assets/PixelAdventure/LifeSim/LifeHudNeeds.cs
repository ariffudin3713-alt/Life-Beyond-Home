using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Panel kanan atas: bar LAPAR dan HAUS (menggantikan bar energi), di atas bar level.
    ///   [mangkuk] LAPAR  [==========    ]
    ///   [tetes]   HAUS   [=======       ]
    /// Bar penuh = kenyang / segar. Warna bar berubah merah saat di bawah 25%.
    /// </summary>
    public partial class LifeHud
    {
        static readonly Color HungerFillColor = new Color(0.98f, 0.62f, 0.30f, 1f);
        static readonly Color ThirstFillColor = new Color(0.36f, 0.70f, 1f, 1f);
        static readonly Color NeedLowColor = new Color(0.95f, 0.28f, 0.28f, 1f);
        const float NeedLowRatio = 0.25f;

        SpriteRenderer _hungerIcon, _hungerBarBg, _hungerBarFill, _hungerLabel;
        SpriteRenderer _thirstIcon, _thirstBarBg, _thirstBarFill, _thirstLabel;
        float _hungerRatio = 1f, _thirstRatio = 1f;

        void BuildNeedBars()
        {
            var barBg = new Color(0.09f, 0.07f, 0.12f, 1f);
            _hungerBarBg = Solid(_hudGroup, "HungerBar", barBg, 3998);
            _hungerBarFill = Solid(_hudGroup, "HungerFill", HungerFillColor, 3999);
            _hungerIcon = Make(_hudGroup, PixelArt.Get("lc_icon_hunger"), "HungerIcon", 4001);
            _hungerLabel = Text(_hudGroup, "HungerLabel", "LAPAR", 1, Ink, new Vector2(0f, 0.5f));

            _thirstBarBg = Solid(_hudGroup, "ThirstBar", barBg, 3998);
            _thirstBarFill = Solid(_hudGroup, "ThirstFill", ThirstFillColor, 3999);
            _thirstIcon = Make(_hudGroup, PixelArt.Get("lc_icon_thirst"), "ThirstIcon", 4001);
            _thirstLabel = Text(_hudGroup, "ThirstLabel", "HAUS", 1, Ink, new Vector2(0f, 0.5f));
        }

        /// <summary>Nilai lapar & haus (0..100; 100 = kenyang / segar).</summary>
        public void SetNeeds(float hunger, float thirst)
        {
            _hungerRatio = Mathf.Clamp01(hunger / 100f);
            _thirstRatio = Mathf.Clamp01(thirst / 100f);
            if (_hungerBarFill != null) _hungerBarFill.color = _hungerRatio <= NeedLowRatio ? NeedLowColor : HungerFillColor;
            if (_thirstBarFill != null) _thirstBarFill.color = _thirstRatio <= NeedLowRatio ? NeedLowColor : ThirstFillColor;
        }

        void LayoutNeedBars(float barsLeft, float right, float top)
        {
            if (_hungerBarBg == null) return;
            float iconX = barsLeft + 0.52f;
            float labelX = barsLeft + 1.05f;
            float barL = barsLeft + 3.45f;          // "LAPAR" = 5 huruf, muat sebelum bar
            float barW = right - 0.45f - barL;

            SetRow(left: barL, y: top - 0.62f, width: barW, height: 0.34f,
                icon: _hungerIcon, bg: _hungerBarBg, fill: _hungerBarFill, ratio: _hungerRatio, iconX: iconX);
            _hungerLabel.transform.localPosition = new Vector3(labelX, top - 0.62f, 0f);

            SetRow(left: barL, y: top - 1.36f, width: barW, height: 0.34f,
                icon: _thirstIcon, bg: _thirstBarBg, fill: _thirstBarFill, ratio: _thirstRatio, iconX: iconX);
            _thirstLabel.transform.localPosition = new Vector3(labelX, top - 1.36f, 0f);
        }
    }
}
