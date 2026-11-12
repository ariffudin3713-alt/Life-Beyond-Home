using UnityEngine;

namespace PixelAdventure
{
    /// <summary>Camera-relative HUD drawn with generated sprites, so no Canvas or font asset is needed.</summary>
    public class Hud : MonoBehaviour
    {
        static readonly Color Ink = new Color(0.98f, 0.96f, 0.88f);
        static readonly Color Outline = new Color(0.05f, 0.04f, 0.10f);

        Camera _cam;
        Theme _theme;

        Transform _root;
        Transform _bars;
        SpriteRenderer _dim;
        Transform _titleGroup, _pauseGroup, _overGroup, _clearGroup, _winGroup;

        SpriteRenderer[] _hearts;
        SpriteRenderer _gemIcon, _gemText, _levelText, _toast, _muteText;

        SpriteRenderer _titleName, _titleSub, _titlePrompt, _titleHint, _titleBest;
        SpriteRenderer _pauseName, _pausePrompt, _pauseHint;
        SpriteRenderer _overName, _overPrompt, _overHint;
        SpriteRenderer _clearName, _clearSub, _clearBest, _clearPrompt;
        SpriteRenderer _winName, _winSub, _winBest, _winPrompt;

        float _toastTimer;
        float _time;

        public void Init(Camera cam)
        {
            _cam = cam;

            _root = new GameObject("HUD").transform;
            _root.SetParent(cam.transform, false);
            _root.localPosition = new Vector3(0f, 0f, 1f);

            _dim = MakeSprite(_root, PixelArt.Get("pixel_white"), "Dim", 500);
            _dim.color = new Color(0.02f, 0.02f, 0.06f, 0.74f);

            _bars = new GameObject("Bars").transform;
            _bars.SetParent(_root, false);

            _hearts = new SpriteRenderer[Player.MaxHealth];
            for (int i = 0; i < _hearts.Length; i++)
            {
                _hearts[i] = MakeSprite(_bars, PixelArt.Get("heart_full"), "Heart" + i, 620);
                _hearts[i].transform.localScale = Vector3.one * 1.25f;
            }

            _gemIcon = MakeSprite(_bars, PixelArt.Get("gem_a"), "GemIcon", 620);
            _gemIcon.transform.localScale = Vector3.one * 1.1f;
            _gemText = Text(_bars, "GemText", "0/0", 1, Ink, new Vector2(0f, 0.5f), 621);
            _levelText = Text(_bars, "Level", "LEVEL 1", 1, Ink, new Vector2(0.5f, 0.5f), 621);
            _toast = Text(_bars, "Toast", " ", 2, Ink, new Vector2(0.5f, 0.5f), 630);
            _toast.gameObject.SetActive(false);
            _muteText = Text(_bars, "Muted", "MUTED", 1, Ink, new Vector2(1f, 0.5f), 621);
            _muteText.gameObject.SetActive(false);

            _titleGroup = Group("Title");
            _titleName = Text(_titleGroup, "Name", "PIXEL QUEST", 3, Ink, new Vector2(0.5f, 0.5f));
            _titleSub = Text(_titleGroup, "Sub", "EMERALD GROVE", 2, Ink, new Vector2(0.5f, 0.5f));
            _titlePrompt = Text(_titleGroup, "Prompt", "PRESS ENTER TO START", 2, Ink, new Vector2(0.5f, 0.5f));
            _titleHint = Text(_titleGroup, "Hint", "COLLECT GEMS - REACH THE PORTAL", 1, Ink, new Vector2(0.5f, 0.5f));
            Text(_titleGroup, "Controls1", "A / D OR ARROWS - MOVE", 1, Ink, new Vector2(0.5f, 0.5f));
            Text(_titleGroup, "Controls2", "SPACE OR W - JUMP", 1, Ink, new Vector2(0.5f, 0.5f));
            Text(_titleGroup, "Controls3", "P - PAUSE    M - MUTE", 1, Ink, new Vector2(0.5f, 0.5f));
            _titleBest = Text(_titleGroup, "Best", " ", 1, Ink, new Vector2(0.5f, 0.5f));

            _titleName.transform.localPosition = new Vector3(0f, 2.7f, 0f);
            _titleSub.transform.localPosition = new Vector3(0f, 1.1f, 0f);
            _titlePrompt.transform.localPosition = new Vector3(0f, -1.2f, 0f);
            _titleHint.transform.localPosition = new Vector3(0f, -2.3f, 0f);
            _titleBest.transform.localPosition = new Vector3(0f, -3.15f, 0f);
            _titleGroup.GetChild(4).localPosition = new Vector3(0f, -4.15f, 0f);
            _titleGroup.GetChild(5).localPosition = new Vector3(0f, -4.85f, 0f);
            _titleGroup.GetChild(6).localPosition = new Vector3(0f, -5.55f, 0f);

            _pauseGroup = Group("Pause");
            _pauseName = Text(_pauseGroup, "Name", "PAUSED", 4, Ink, new Vector2(0.5f, 0.5f));
            _pausePrompt = Text(_pauseGroup, "Prompt", "ENTER - RESUME", 2, Ink, new Vector2(0.5f, 0.5f));
            _pauseHint = Text(_pauseGroup, "Hint", "R - RESTART LEVEL    M - MUTE", 1, Ink, new Vector2(0.5f, 0.5f));
            _pauseName.transform.localPosition = new Vector3(0f, 1.1f, 0f);
            _pausePrompt.transform.localPosition = new Vector3(0f, -0.6f, 0f);
            _pauseHint.transform.localPosition = new Vector3(0f, -1.6f, 0f);

            _overGroup = Group("GameOver");
            _overName = Text(_overGroup, "Name", "GAME OVER", 4, Ink, new Vector2(0.5f, 0.5f));
            _overPrompt = Text(_overGroup, "Prompt", "ENTER - TRY AGAIN", 2, Ink, new Vector2(0.5f, 0.5f));
            _overHint = Text(_overGroup, "Hint", "R - RESTART LEVEL", 1, Ink, new Vector2(0.5f, 0.5f));
            _overName.transform.localPosition = new Vector3(0f, 1.1f, 0f);
            _overPrompt.transform.localPosition = new Vector3(0f, -0.8f, 0f);
            _overHint.transform.localPosition = new Vector3(0f, -1.7f, 0f);

            _clearGroup = Group("LevelClear");
            _clearName = Text(_clearGroup, "Name", "LEVEL CLEAR", 3, Ink, new Vector2(0.5f, 0.5f));
            _clearSub = Text(_clearGroup, "Sub", "GEMS 0 / 0", 2, Ink, new Vector2(0.5f, 0.5f));
            _clearBest = Text(_clearGroup, "Best", " ", 2, Ink, new Vector2(0.5f, 0.5f));
            _clearPrompt = Text(_clearGroup, "Prompt", "ENTER - NEXT LEVEL", 2, Ink, new Vector2(0.5f, 0.5f));
            _clearName.transform.localPosition = new Vector3(0f, 2.0f, 0f);
            _clearSub.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            _clearBest.transform.localPosition = new Vector3(0f, -0.55f, 0f);
            _clearPrompt.transform.localPosition = new Vector3(0f, -1.75f, 0f);

            _winGroup = Group("Victory");
            _winName = Text(_winGroup, "Name", "YOU WIN!", 4, Ink, new Vector2(0.5f, 0.5f));
            _winSub = Text(_winGroup, "Sub", "GEMS 0 / 0", 2, Ink, new Vector2(0.5f, 0.5f));
            _winBest = Text(_winGroup, "Best", " ", 2, Ink, new Vector2(0.5f, 0.5f));
            _winPrompt = Text(_winGroup, "Prompt", "ENTER - PLAY AGAIN", 2, Ink, new Vector2(0.5f, 0.5f));
            _winName.transform.localPosition = new Vector3(0f, 2.3f, 0f);
            _winSub.transform.localPosition = new Vector3(0f, 0.65f, 0f);
            _winBest.transform.localPosition = new Vector3(0f, -0.45f, 0f);
            _winPrompt.transform.localPosition = new Vector3(0f, -1.7f, 0f);

            HideOverlays();
            _bars.gameObject.SetActive(false);
        }

        public void SetTheme(Theme theme)
        {
            _theme = theme;
            Color accent = theme != null ? theme.Accent : Ink;
            _titleSub.color = accent;
            _pausePrompt.color = Ink;
            _clearSub.color = accent;
            _winSub.color = accent;
            _muteText.color = accent;
        }

        public void SetLevel(int index, LevelDef level)
        {
            SetText(_levelText, "LEVEL " + (index + 1) + " - " + level.Theme.Title,
                Ink, 1, new Vector2(0.5f, 0.5f));
        }

        public void SetHealth(int health)
        {
            for (int i = 0; i < _hearts.Length; i++)
            {
                _hearts[i].sprite = PixelArt.Get(i < health ? "heart_full" : "heart_empty");
            }
        }

        public void SetGems(int gems, int total)
        {
            SetText(_gemText, gems + "/" + total, Ink, 1, new Vector2(0f, 0.5f));
        }

        public void SetMuted(bool muted)
        {
            _muteText.gameObject.SetActive(muted);
        }

        public void ShowTitle(int bestRun, int totalGems)
        {
            Overlay(_titleGroup, false);
            _dim.color = new Color(0.02f, 0.02f, 0.06f, 0.62f);
            Color accent = _theme != null ? _theme.Accent : Ink;
            if (_theme != null) _titleSub.color = accent;
            SetText(_titleBest, bestRun > 0 ? "BEST RUN  " + bestRun + " / " + totalGems : " ",
                accent, 1, new Vector2(0.5f, 0.5f));
        }

        public void ShowPause()
        {
            Overlay(_pauseGroup, true);
            _dim.color = new Color(0.02f, 0.02f, 0.06f, 0.70f);
        }

        public void ShowGameOver()
        {
            Overlay(_overGroup, true);
            _dim.color = new Color(0.02f, 0.02f, 0.06f, 0.76f);
        }

        public void ShowLevelClear(int gems, int total, int best, bool newBest)
        {
            Overlay(_clearGroup, true);
            _dim.color = new Color(0.02f, 0.02f, 0.06f, 0.72f);
            Color accent = _theme != null ? _theme.Accent : Ink;
            SetText(_clearSub, "GEMS  " + gems + " / " + total, accent, 2, new Vector2(0.5f, 0.5f));
            string line = newBest
                ? "NEW BEST!  " + best + " / " + total
                : best > 0 ? "BEST  " + best + " / " + total : " ";
            SetText(_clearBest, line, newBest ? accent : Ink, 2, new Vector2(0.5f, 0.5f));
        }

        public void ShowVictory(int gems, int total, int best, bool newBest)
        {
            Overlay(_winGroup, false);
            _dim.color = new Color(0.02f, 0.02f, 0.06f, 0.78f);
            Color accent = _theme != null ? _theme.Accent : Ink;
            SetText(_winSub, "GEMS  " + gems + " / " + total, accent, 2, new Vector2(0.5f, 0.5f));
            string line = newBest
                ? "NEW BEST!  " + best + " / " + total
                : best > 0 ? "BEST RUN  " + best + " / " + total : " ";
            SetText(_winBest, line, newBest ? accent : Ink, 2, new Vector2(0.5f, 0.5f));
        }

        public void HideOverlays()
        {
            _titleGroup.gameObject.SetActive(false);
            _pauseGroup.gameObject.SetActive(false);
            _overGroup.gameObject.SetActive(false);
            _clearGroup.gameObject.SetActive(false);
            _winGroup.gameObject.SetActive(false);
            _dim.gameObject.SetActive(false);
            _bars.gameObject.SetActive(true);
        }

        public void ShowToast(string text)
        {
            Color accent = _theme != null ? _theme.Accent : Ink;
            SetText(_toast, text, accent, 2, new Vector2(0.5f, 0.5f));
            _toast.gameObject.SetActive(true);
            _toastTimer = 2f;
        }

        void Update()
        {
            if (_cam == null) return;
            float dt = Time.unscaledDeltaTime;
            _time += dt;

            Layout();

            if (_toastTimer > 0f)
            {
                _toastTimer -= dt;
                if (_toastTimer <= 0f) _toast.gameObject.SetActive(false);
            }

            float pulse = 0.55f + 0.45f * (Mathf.Sin(_time * 3.4f) * 0.5f + 0.5f);
            if (_titleGroup.gameObject.activeSelf) SetAlpha(_titlePrompt, pulse);
            if (_overGroup.gameObject.activeSelf) SetAlpha(_overPrompt, pulse);
            if (_clearGroup.gameObject.activeSelf) SetAlpha(_clearPrompt, pulse);
            if (_winGroup.gameObject.activeSelf) SetAlpha(_winPrompt, pulse);
            if (_pauseGroup.gameObject.activeSelf) SetAlpha(_pausePrompt, pulse);
        }

        void Layout()
        {
            float halfH = _cam.orthographicSize;
            float halfW = halfH * _cam.aspect;
            Sprite dimSprite = _dim.sprite;
            Vector2 unit = dimSprite != null ? dimSprite.bounds.size : Vector2.one;
            if (unit.x <= 0f || unit.y <= 0f) unit = Vector2.one;
            _dim.transform.localScale = new Vector3((halfW * 2f) / unit.x,
                (halfH * 2f) / unit.y, 1f);
            _dim.transform.localPosition = new Vector3(0f, 0f, -0.1f);

            float top = halfH - 0.72f;
            float left = -halfW + 0.95f;
            for (int i = 0; i < _hearts.Length; i++)
            {
                _hearts[i].transform.localPosition =
                    new Vector3(left + i * 0.92f, top, 0f);
            }

            float right = halfW - 0.8f;
            float textWidth = _gemText.sprite != null ? _gemText.sprite.bounds.size.x : 1f;
            float gemX = right - textWidth;
            _gemText.transform.localPosition = new Vector3(gemX, top, 0f);
            _gemIcon.transform.localPosition = new Vector3(gemX - 0.5f, top, 0f);

            _levelText.transform.localPosition = new Vector3(0f, top, 0f);
            _toast.transform.localPosition = new Vector3(0f, -halfH + 0.9f, 0f);
            _muteText.transform.localPosition = new Vector3(halfW - 0.7f, -halfH + 0.85f, 0f);
        }

        // ------------------------------------------------------------------ helpers

        Transform Group(string name)
        {
            var t = new GameObject(name).transform;
            t.SetParent(_root, false);
            t.gameObject.SetActive(false);
            return t;
        }

        static SpriteRenderer MakeSprite(Transform parent, Sprite sprite, string name, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sharedMaterial = PixelArt.SpriteMaterial;
            sr.sortingOrder = order;
            return sr;
        }

        static SpriteRenderer Text(Transform parent, string name, string text, int scale,
            Color color, Vector2 pivot, int order = 610)
        {
            var sr = MakeSprite(parent, PixelFont.Render(text, color, Outline, scale, pivot),
                name, order);
            return sr;
        }

        static void SetText(SpriteRenderer sr, string text, Color color, int scale, Vector2 pivot)
        {
            if (sr == null) return;
            sr.sprite = PixelFont.Render(text, color, Outline, scale, pivot);
        }

        static void SetAlpha(SpriteRenderer sr, float alpha)
        {
            if (sr == null) return;
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }

        void Overlay(Transform group, bool showBars)
        {
            _titleGroup.gameObject.SetActive(group == _titleGroup);
            _pauseGroup.gameObject.SetActive(group == _pauseGroup);
            _overGroup.gameObject.SetActive(group == _overGroup);
            _clearGroup.gameObject.SetActive(group == _clearGroup);
            _winGroup.gameObject.SetActive(group == _winGroup);
            _dim.gameObject.SetActive(true);
            _bars.gameObject.SetActive(showBars);
        }
    }
}
