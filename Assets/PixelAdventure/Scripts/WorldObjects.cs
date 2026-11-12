using UnityEngine;

namespace PixelAdventure
{
    /// <summary>Bobbing collectible gem.</summary>
    public class GemPickup : MonoBehaviour
    {
        SpriteRenderer _sr;
        Vector3 _base;
        float _time;
        bool _taken;

        void Awake()
        {
            gameObject.layer = GameLayers.Pickup;

            _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sprite = PixelArt.Get("gem_a");
            _sr.sharedMaterial = PixelArt.SpriteMaterial;
            _sr.sortingOrder = 8;

            var col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.32f;

            SpriteAnim.Add(gameObject, _sr,
                new[] { PixelArt.Get("gem_a"), PixelArt.Get("gem_b") }, 4f);
        }

        void Start()
        {
            _base = transform.position;
        }

        void Update()
        {
            _time += Time.deltaTime;
            transform.position = _base +
                new Vector3(0f, Mathf.Sin(_time * 2.8f + _base.x * 0.7f) * 0.08f, 0f);
        }

        void OnTriggerEnter2D(Collider2D other) { TryCollect(other); }
        void OnTriggerStay2D(Collider2D other) { TryCollect(other); }

        void TryCollect(Collider2D other)
        {
            if (_taken) return;
            if (other.GetComponentInParent<Player>() == null) return;
            if (GameManager.I == null || GameManager.I.State != GameState.Playing) return;

            _taken = true;
            GameManager.I.CollectGem(transform.position);
            Destroy(gameObject);
        }
    }

    /// <summary>Level exit portal. Leads to the next level (or the ending).</summary>
    public class GoalPortal : MonoBehaviour
    {
        SpriteRenderer _glow;
        float _time;
        bool _used;

        void Awake()
        {
            gameObject.layer = GameLayers.Pickup;

            var sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = PixelArt.Get("portal_a");
            sr.sharedMaterial = PixelArt.SpriteMaterial;
            sr.sortingOrder = 9;
            SpriteAnim.Add(gameObject, sr,
                new[] { PixelArt.Get("portal_a"), PixelArt.Get("portal_b") }, 5f);

            var col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.95f, 1.9f);
            col.offset = new Vector2(0f, 1f);

            var glowGo = new GameObject("Glow");
            glowGo.transform.SetParent(transform, false);
            glowGo.transform.localPosition = new Vector3(0f, 1.25f, 0f);
            glowGo.transform.localScale = Vector3.one * 1.35f;

            _glow = glowGo.AddComponent<SpriteRenderer>();
            _glow.sprite = PixelArt.Get("portal_a");
            _glow.sharedMaterial = PixelArt.SpriteMaterial;
            _glow.sortingOrder = 8;
            _glow.color = new Color(0.55f, 1f, 0.95f, 0.16f);
        }

        void Update()
        {
            _time += Time.deltaTime;
            float s = 1.3f + Mathf.Sin(_time * 2.2f) * 0.08f;
            _glow.transform.localScale = new Vector3(s, s, 1f);
        }

        void OnTriggerEnter2D(Collider2D other) { TryEnter(other); }
        void OnTriggerStay2D(Collider2D other) { TryEnter(other); }

        void TryEnter(Collider2D other)
        {
            if (_used) return;
            if (other.GetComponentInParent<Player>() == null) return;
            if (GameManager.I == null || GameManager.I.State != GameState.Playing) return;

            _used = true;
            GameManager.I.ReachGoal();
        }
    }

    /// <summary>Bouncy spring hidden on the ground; launches the player upward.</summary>
    public class Spring : MonoBehaviour
    {
        float _cooldown;
        float _squash;

        void Awake()
        {
            gameObject.layer = GameLayers.Pickup;

            var sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = PixelArt.Get("spring");
            sr.sharedMaterial = PixelArt.SpriteMaterial;
            sr.sortingOrder = 7;

            var col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.9f, 0.34f);
            col.offset = new Vector2(0f, 0.17f);
        }

        void Update()
        {
            float dt = Time.deltaTime;
            if (_cooldown > 0f) _cooldown -= dt;
            _squash = Mathf.MoveTowards(_squash, 0f, dt * 4f);
            transform.localScale = new Vector3(1f + _squash * 0.22f, 1f - _squash * 0.55f, 1f);
        }

        void OnTriggerEnter2D(Collider2D other) { TryLaunch(other); }
        void OnTriggerStay2D(Collider2D other) { TryLaunch(other); }

        void TryLaunch(Collider2D other)
        {
            if (_cooldown > 0f) return;

            Player player = other.GetComponentInParent<Player>();
            if (player == null || !player.IsAlive) return;
            if (GameManager.I == null || GameManager.I.State != GameState.Playing) return;
            if (player.Velocity.y > 0.8f) return;
            if (player.transform.position.y < transform.position.y - 0.3f) return;

            _cooldown = 0.35f;
            _squash = 0.5f;

            player.Launch(21.5f);
            GameAudio.Play("spring", 0.85f);
            FxManager.Dust(transform.position + new Vector3(0f, 0.12f, 0f),
                new Color(1f, 0.85f, 0.55f), 8, Mathf.PI * 0.5f);
            GameManager.I.CameraRig?.Shake(0.05f, 0.1f);
        }
    }

    /// <summary>Shrine that activates a new respawn point.</summary>
    public class Checkpoint : MonoBehaviour
    {
        SpriteRenderer _sr;
        SpriteRenderer _glow;
        bool _active;
        float _time;

        void Awake()
        {
            gameObject.layer = GameLayers.Pickup;
            transform.localScale = Vector3.one * 1.6f;

            _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sprite = PixelArt.Get("shrine");
            _sr.sharedMaterial = PixelArt.SpriteMaterial;
            _sr.sortingOrder = 7;

            var col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.8f, 1.45f);
            col.offset = new Vector2(0f, 0.72f);

            var glowGo = new GameObject("Glow");
            glowGo.transform.SetParent(transform, false);
            glowGo.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            glowGo.transform.localScale = Vector3.one * 1.18f;
            _glow = glowGo.AddComponent<SpriteRenderer>();
            _glow.sprite = PixelArt.Get("shrine");
            _glow.sharedMaterial = PixelArt.SpriteMaterial;
            _glow.sortingOrder = 6;
            _glow.color = new Color(0.55f, 1f, 0.95f, 0.10f);
        }

        void Update()
        {
            _time += Time.deltaTime;
            float a = _active ? 0.28f : 0.10f;
            Color c = _glow.color;
            c.a = a + Mathf.Sin(_time * 2.4f) * 0.05f;
            _glow.color = c;
        }

        void OnTriggerEnter2D(Collider2D other) { TryActivate(other); }
        void OnTriggerStay2D(Collider2D other) { TryActivate(other); }

        void TryActivate(Collider2D other)
        {
            if (_active) return;
            if (other.GetComponentInParent<Player>() == null) return;
            if (GameManager.I == null || GameManager.I.State != GameState.Playing) return;

            _active = true;

            Color accent = GameManager.I.CurrentTheme.Accent;
            _sr.color = Color.Lerp(Color.white, accent, 0.45f);
            _glow.color = new Color(accent.r, accent.g, accent.b, 0.32f);

            GameAudio.Play("checkpoint", 0.9f);
            FxManager.Burst(transform.position + new Vector3(0f, 1.2f, 0f), accent,
                16, 1.2f, 3.5f, 0.6f, 0.1f, 4f);
            GameManager.I.SetCheckpoint(new Vector3(transform.position.x + 1.15f,
                transform.position.y + 0.05f, 0f));
        }
    }

    public static class WorldFactory
    {
        public static void SpawnGem(Vector3 position, Transform parent)
        {
            var go = new GameObject("Gem");
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.AddComponent<GemPickup>();
        }

        public static void SpawnPortal(Vector3 position, Transform parent)
        {
            var go = new GameObject("Goal");
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.AddComponent<GoalPortal>();
        }

        public static void SpawnSpring(Vector3 position, Transform parent)
        {
            var go = new GameObject("Spring");
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.AddComponent<Spring>();
        }

        public static void SpawnCheckpoint(Vector3 position, Transform parent)
        {
            var go = new GameObject("Checkpoint");
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.AddComponent<Checkpoint>();
        }
    }
}
