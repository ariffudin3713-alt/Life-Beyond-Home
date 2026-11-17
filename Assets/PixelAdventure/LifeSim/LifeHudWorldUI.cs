using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// World-facing HUD pieces: the interactive city loading screen.
    /// (Minimap ada di LifeHudMinimap.cs.)
    /// </summary>
    public partial class LifeHud
    {
        Transform _loadingGroup;
        SpriteRenderer _loadingBg, _loadingPanel, _loadingCity, _loadingTip, _loadingPercent;
        SpriteRenderer _loadingTag, _loadingBarBg, _loadingBarFill;

        float _loadingProgress;
        string _lastLoadingTip;
        int _lastLoadingPercent = -1;

        void BuildWorldUI()
        {
            // ---------------------------------------------------------- loading
            _loadingGroup = Group("Loading");
            _loadingBg = Solid(_loadingGroup, "LoadingBg", new Color(0.03f, 0.04f, 0.08f, 1f), 5000);
            _loadingPanel = Panel(_loadingGroup, "LoadingPanel", "lc_panel_gold", 5001);
            _loadingTag = Text(_loadingGroup, "LoadingTag", "NAIK BUS", 2, Gold,
                new Vector2(0.5f, 0.5f), 5003);
            _loadingCity = Text(_loadingGroup, "LoadingCity", " ", 3, Ink,
                new Vector2(0.5f, 0.5f), 5003);
            _loadingTip = Text(_loadingGroup, "LoadingTip", " ", 1, Soft,
                new Vector2(0.5f, 0.5f), 5003);
            _loadingBarBg = Solid(_loadingGroup, "LoadingBarBg", new Color(0.08f, 0.06f, 0.11f, 1f), 5002);
            _loadingBarFill = Solid(_loadingGroup, "LoadingBarFill", new Color(0.99f, 0.80f, 0.30f, 1f), 5003);
            _loadingPercent = Text(_loadingGroup, "LoadingPercent", "0%", 1, Gold,
                new Vector2(0.5f, 0.5f), 5003);
            _loadingGroup.gameObject.SetActive(false);

            BuildMinimap();
        }

        // ------------------------------------------------------------ loading api

        public void ShowLoading(string cityName, string tagline, string tip)
        {
            SetText(_loadingCity, string.IsNullOrEmpty(cityName) ? "KOTA" : cityName, Ink, 3,
                new Vector2(0.5f, 0.5f));
            SetText(_loadingTag, string.IsNullOrEmpty(tagline) ? "MEMUAT KOTA" : tagline, Gold, 1,
                new Vector2(0.5f, 0.5f));
            SetText(_loadingTip, string.IsNullOrEmpty(tip) ? " " : tip, Soft, 1,
                new Vector2(0.5f, 0.5f));
            _lastLoadingTip = string.IsNullOrEmpty(tip) ? " " : tip;
            _lastLoadingPercent = -1;
            _loadingProgress = 0f;
            SetLoading(0f, tip);
            _loadingGroup.gameObject.SetActive(true);
            SetDim(0f);
            HidePrompt();
        }

        public void SetLoading(float progress, string tip)
        {
            _loadingProgress = Mathf.Clamp01(progress);
            if (!string.IsNullOrEmpty(tip) && tip != _lastLoadingTip)
            {
                _lastLoadingTip = tip;
                SetText(_loadingTip, tip, Soft, 1, new Vector2(0.5f, 0.5f));
            }
            int percent = Mathf.RoundToInt(_loadingProgress * 100f);
            if (percent != _lastLoadingPercent)
            {
                _lastLoadingPercent = percent;
                SetText(_loadingPercent, percent + "%", Gold, 1, new Vector2(0.5f, 0.5f));
            }
            LayoutLoading(_halfW, _halfH);
        }

        public void HideLoading()
        {
            _loadingProgress = 0f;
            if (_loadingGroup != null) _loadingGroup.gameObject.SetActive(false);
        }

        // ------------------------------------------------------------ layout

        void LayoutWorldUI(float halfW, float halfH)
        {
            if (_loadingGroup != null && _loadingGroup.gameObject.activeSelf)
                LayoutLoading(halfW, halfH);

            LayoutMinimap(halfW, halfH);       // arah ke tujuan misi ada di minimap
        }

        void LayoutLoading(float halfW, float halfH)
        {
            Fill(_loadingBg, halfW, halfH);
            float w = Mathf.Min(halfW * 2f - 1.2f, 21f);
            float h = 9.4f;
            SetRect(_loadingPanel, 0f, 0f, w, h);
            _loadingTag.transform.localPosition = new Vector3(0f, h * 0.5f - 0.65f, 0f);
            _loadingCity.transform.localPosition = new Vector3(0f, 1.55f, 0f);
            _loadingBarBg.transform.localPosition = new Vector3(0f, 0.1f, 0f);
            _loadingBarBg.transform.localScale = new Vector3(w - 2.2f, 0.34f, 1f);
            float fillW = (w - 2.32f) * _loadingProgress;
            _loadingBarFill.transform.localPosition =
                new Vector3(-(w - 2.32f) * 0.5f + fillW * 0.5f, 0.1f, 0f);
            _loadingBarFill.transform.localScale = new Vector3(fillW, 0.22f, 1f);
            _loadingPercent.transform.localPosition = new Vector3(0f, -0.75f, 0f);
            _loadingTip.transform.localPosition = new Vector3(0f, -2.35f, 0f);
        }
    }
}
