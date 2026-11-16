using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Potret karakter saat percakapan (gaya visual novel / Citampi Stories):
    /// - lawan bicara (NPC) berdiri di kiri, tokoh utama di kanan, di atas kotak dialog;
    /// - kotak dialog menutupi bagian kaki sehingga terlihat seperti potret setengah badan;
    /// - yang sedang berbicara terang, yang lain diredupkan; narasi meredupkan keduanya;
    /// - potret baru muncul dengan animasi naik + memudar masuk.
    /// </summary>
    public partial class LifeHud
    {
        const int PortraitNone = -1, PortraitLeft = 0, PortraitRight = 1;

        static readonly Color PortraitDim = new Color(0.46f, 0.46f, 0.56f, 1f);

        SpriteRenderer _portraitL, _portraitR;
        string _portraitKeyL, _portraitKeyR;
        int _portraitSpeaking = PortraitNone;
        float _portraitPopL = 1f, _portraitPopR = 1f;   // 0 = baru muncul, 1 = diam di tempat
        float _portraitTalkL, _portraitTalkR;           // 1 = sedang bicara (dihaluskan untuk transisi terang/redup)

        void BuildPortraits()
        {
            _portraitL = Make(_talkGroup, null, "PortraitL", 4009);
            _portraitR = Make(_talkGroup, null, "PortraitR", 4009);
            ClearDialoguePortraits();
        }

        /// <summary>
        /// Atur potret percakapan. Kunci sprite lengkap (mis. "lc_ratna_down_0") atau null untuk kosong.
        /// speaking: 0 = kiri bicara, 1 = kanan bicara, -1 = narasi / tidak ada yang bicara.
        /// </summary>
        public void SetDialoguePortraits(string leftKey, string rightKey, int speaking)
        {
            if (_portraitL == null) return;
            SetPortrait(_portraitL, ref _portraitKeyL, ref _portraitPopL, leftKey);
            SetPortrait(_portraitR, ref _portraitKeyR, ref _portraitPopR, rightKey);
            _portraitSpeaking = speaking;
        }

        public void ClearDialoguePortraits()
        {
            if (_portraitL == null) return;
            SetPortrait(_portraitL, ref _portraitKeyL, ref _portraitPopL, null);
            SetPortrait(_portraitR, ref _portraitKeyR, ref _portraitPopR, null);
            _portraitSpeaking = PortraitNone;
            _portraitTalkL = _portraitTalkR = 0f;
        }

        static void SetPortrait(SpriteRenderer sr, ref string current, ref float pop, string key)
        {
            if (key == current) return;
            current = key;
            Sprite s = string.IsNullOrEmpty(key) ? null : PixelArt.Get(key);
            sr.sprite = s;
            pop = 0f;
            bool on = s != null;
            sr.gameObject.SetActive(on);
        }

        void LayoutPortraits(float dw, float dcy, float dh)
        {
            if (_portraitL == null || !_talkGroup.gameObject.activeSelf) return;

            float dt = Time.unscaledDeltaTime;
            _portraitPopL = Mathf.MoveTowards(_portraitPopL, 1f, dt * 5f);
            _portraitPopR = Mathf.MoveTowards(_portraitPopR, 1f, dt * 5f);
            _portraitTalkL = Mathf.MoveTowards(_portraitTalkL, _portraitSpeaking == PortraitLeft ? 1f : 0f, dt * 8f);
            _portraitTalkR = Mathf.MoveTowards(_portraitTalkR, _portraitSpeaking == PortraitRight ? 1f : 0f, dt * 8f);

            // Ukuran potret mengikuti tinggi layar, dibulatkan ke kelipatan piksel agar tetap tajam.
            const float spritePxH = 28f;
            float wantH = Mathf.Clamp(_halfH * 1.15f, 4.2f, 8.5f);
            float pxScale = Mathf.Max(1f, Mathf.Round(wantH / (spritePxH / PixelArt.PPU)));
            float h = pxScale * spritePxH / PixelArt.PPU;
            float w = h * 16f / 28f;

            float boxTop = dcy + dh * 0.5f;
            float baseY = boxTop - h * 0.40f;           // ±60% atas (kepala & badan) terlihat di atas kotak
            float margin = Mathf.Min(1.6f, dw * 0.08f);
            float lx = -dw * 0.5f + margin + w * 0.5f;
            float rx = dw * 0.5f - margin - w * 0.5f;

            PlacePortrait(_portraitL, lx, baseY, pxScale, _portraitPopL, _portraitTalkL);
            PlacePortrait(_portraitR, rx, baseY, pxScale, _portraitPopR, _portraitTalkR);
        }

        static void PlacePortrait(SpriteRenderer sr, float x, float y, float scale,
            float pop, float talk)
        {
            if (sr.sprite == null) return;

            float e = 1f - (1f - pop) * (1f - pop) * (1f - pop);    // ease-out kubik
            float rise = (1f - e) * 1.2f;
            // Yang bicara sedikit lebih tinggi; yang diam sedikit turun & redup.
            float lift = Mathf.Lerp(-0.12f, 0.08f, talk);

            sr.transform.localPosition = new Vector3(x, y - rise + lift, 0f);
            sr.transform.localScale = new Vector3(scale, scale, 1f);
            Color c = Color.Lerp(PortraitDim, Color.white, talk);
            c.a = e;
            sr.color = c;

        }
    }
}
