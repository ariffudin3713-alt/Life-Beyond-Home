using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>Simple town NPC: faces the player when nearby, shows a floating name tag.</summary>
    public class LifeNpc : MonoBehaviour
    {
        public string Id;
        public string DisplayName;

        SpriteRenderer _body;
        SpriteRenderer _shadow;
        SpriteRenderer _tag;
        SpriteRenderer _badge;      // "!" misi sampingan baru, "?" tujuan misi sampingan
        string _badgeText;
        Transform _bodyTransform;
        LifeWorld _wanderWorld;
        Vector2 _wanderOrigin;
        Vector2 _wanderTarget;
        float _wanderRadius;
        float _wanderSpeed = 0.8f;
        float _wanderPause;
        bool _hasWanderTarget;

        int _dir;
        float _time;

        static readonly string[] DirNames = { "down", "up", "left", "right" };
        static readonly int[] WalkCycle = { 1, 0, 2, 0 };
        int _frame;
        float _walkAnim;

        public Vector2 Position => transform.position;

        public bool IsVisible => gameObject.activeInHierarchy;

        public static LifeNpc Create(Transform parent, string id, string displayName, float x, float y)
        {
            var go = new GameObject("NPC_" + id);
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(x, y, 0f);
            var npc = go.AddComponent<LifeNpc>();
            npc.Id = id;
            npc.DisplayName = displayName;

            var bodyGo = new GameObject("Body");
            bodyGo.transform.SetParent(go.transform, false);
            npc._bodyTransform = bodyGo.transform;
            npc._body = bodyGo.AddComponent<SpriteRenderer>();
            npc._body.sharedMaterial = PixelArt.SpriteMaterial;
            npc._body.sprite = PixelArt.Get("lc_" + id + "_down_0");
            npc._body.sortingOrder = 0;

            var shadowGo = new GameObject("Shadow");
            shadowGo.transform.SetParent(go.transform, false);
            shadowGo.transform.localPosition = new Vector3(0f, 0.06f, 0f);
            npc._shadow = shadowGo.AddComponent<SpriteRenderer>();
            npc._shadow.sprite = PixelArt.Get("lc_shadow");
            npc._shadow.sharedMaterial = PixelArt.SpriteMaterial;
            npc._shadow.sortingOrder = -1;

            var tagGo = new GameObject("Tag");
            tagGo.transform.SetParent(go.transform, false);
            tagGo.transform.localPosition = new Vector3(0f, 1.86f, 0f);
            npc._tag = tagGo.AddComponent<SpriteRenderer>();
            npc._tag.sprite = PixelFont.Render(displayName,
                new Color(1f, 0.86f, 0.52f), new Color(0.08f, 0.06f, 0.12f), 1,
                new Vector2(0.5f, 0.5f));
            npc._tag.sharedMaterial = PixelArt.SpriteMaterial;
            npc._tag.sortingOrder = 1;

            npc.ApplySprite();
            return npc;
        }

        /// <summary>Enables low-cost wandering for ambient residents only.</summary>
        public void EnableWandering(LifeWorld world, float radius = 2.5f, float speed = 0.8f)
        {
            _wanderWorld = world;
            _wanderOrigin = transform.position;
            _wanderRadius = Mathf.Max(0.5f, radius);
            _wanderSpeed = Mathf.Max(0.2f, speed);
            _wanderPause = Random.Range(0.1f, 1.1f);
            _hasWanderTarget = false;
        }

        public void FaceTowards(Vector2 target)
        {
            Vector2 d = target - (Vector2)transform.position;
            if (d.sqrMagnitude < 0.001f) return;
            if (Mathf.Abs(d.x) > Mathf.Abs(d.y))
                _dir = d.x < 0f ? 2 : 3;
            else
                _dir = d.y < 0f ? 0 : 1;
            ApplySprite();
        }

        /// <summary>Tanda di atas kepala: "!" (kuning) misi baru, "?" (biru) tujuan misi; null = sembunyikan.</summary>
        public void SetBadge(string text)
        {
            if (text == _badgeText) return;
            _badgeText = text;
            if (string.IsNullOrEmpty(text))
            {
                if (_badge != null) _badge.gameObject.SetActive(false);
                return;
            }
            if (_badge == null)
            {
                var go = new GameObject("Badge");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, 2.32f, 0f);
                _badge = go.AddComponent<SpriteRenderer>();
                _badge.sharedMaterial = PixelArt.SpriteMaterial;
                _badge.enabled = _body != null && _body.enabled;
            }
            Color fill = text == "?" ? new Color(0.55f, 0.85f, 1f) : new Color(1f, 0.84f, 0.25f);
            _badge.sprite = PixelFont.Render(text, fill, new Color(0.08f, 0.06f, 0.12f), 2, new Vector2(0.5f, 0.5f));
            _badge.gameObject.SetActive(true);
            ApplySprite();
        }

        public void FaceDown()
        {
            _dir = 0;
            ApplySprite();
        }

        public void Teleport(float x, float y)
        {
            transform.position = new Vector3(x, y, 0f);
            ApplySprite();
        }

        void ApplySprite()
        {
            if (_body == null) return;
            _body.sprite = PixelArt.Get("lc_" + Id + "_" + DirNames[_dir] + "_" + _frame);
            int order = -Mathf.RoundToInt(transform.position.y * PixelArt.PPU);
            _body.sortingOrder = order;
            _shadow.sortingOrder = order - 1;
            _tag.sortingOrder = order + 1;
            if (_badge != null) _badge.sortingOrder = order + 2;
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            _time += dt;

            // Di luar layar: matikan gambar dan lewati animasi/logika jalan-jalan.
            bool visible = LifeView.Sees(transform.position, 1.5f, 2.4f);
            if (_body != null && _body.enabled != visible)
            {
                _body.enabled = visible;
                _shadow.enabled = visible;
                _tag.enabled = visible;
                if (_badge != null) _badge.enabled = visible;
            }
            if (!visible) return;

            if (_bodyTransform != null)
                _bodyTransform.localPosition = new Vector3(0f,
                    Mathf.Sin(_time * 2.1f) * 0.008f, 0f);
            if (_badge != null && _badge.gameObject.activeSelf)
                _badge.transform.localPosition = new Vector3(0f, 2.32f + Mathf.Abs(Mathf.Sin(_time * 3.2f)) * 0.14f, 0f);

            // Keep the population still in menus, pause screens and loading transitions.
            if (_wanderWorld == null || PulangGame.Instance == null ||
                !PulangGame.Instance.CanAmbientMove || !gameObject.activeInHierarchy)
                return;

            if (_wanderPause > 0f)
            {
                _wanderPause = Mathf.Max(0f, _wanderPause - dt);
                return;
            }

            if (!_hasWanderTarget && !ChooseWanderTarget())
            {
                _wanderPause = 0.4f;
                return;
            }

            Vector2 current = transform.position;
            Vector2 delta = _wanderTarget - current;
            // Berhenti saat pemain mendekat (agar mudah diajak bicara), atau saat sampai tujuan.
            bool playerClose = PulangGame.Instance.PlayerNear(current, 2.2f);
            if (delta.sqrMagnitude < 0.04f || playerClose)
            {
                _hasWanderTarget = false;
                _wanderPause = playerClose ? 0.5f : Random.Range(1.5f, 5f);
                if (_frame != 0) { _frame = 0; ApplySprite(); }
                return;
            }

            Vector2 next = Vector2.MoveTowards(current, _wanderTarget, _wanderSpeed * dt);
            if (_wanderWorld.CanWander(next))
            {
                transform.position = new Vector3(next.x, next.y, 0f);
                _walkAnim += dt;
                _frame = WalkCycle[(int)(_walkAnim * 7f) % WalkCycle.Length];
                FaceTowards(_wanderTarget);
            }
            else
            {
                _hasWanderTarget = false;
                _wanderPause = 0.3f;
                if (_frame != 0) { _frame = 0; ApplySprite(); }
            }
        }

        bool ChooseWanderTarget()
        {
            for (int attempt = 0; attempt < 8; attempt++)
            {
                Vector2 candidate = _wanderOrigin + Random.insideUnitCircle * _wanderRadius;
                if (!_wanderWorld.CanWander(candidate)) continue;
                _wanderTarget = candidate;
                _hasWanderTarget = true;
                return true;
            }
            return false;
        }
    }
}
