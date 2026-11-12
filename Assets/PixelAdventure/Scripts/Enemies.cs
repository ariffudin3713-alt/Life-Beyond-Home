using System.Collections;
using UnityEngine;

namespace PixelAdventure
{
    /// <summary>Shared enemy behaviour: stomp death, poof animation and trigger hitbox plumbing.</summary>
    public class EnemyBase : MonoBehaviour
    {
        public bool CanStomp = true;

        protected SpriteRenderer Sr;
        protected Transform Visual;
        protected bool Dead;

        protected virtual void Awake()
        {
            gameObject.layer = GameLayers.Enemy;
        }

        public virtual bool Stomp()
        {
            if (Dead) return false;
            Dead = true;
            foreach (Collider2D c in GetComponentsInChildren<Collider2D>()) c.enabled = false;
            StartCoroutine(DeathRoutine());
            return true;
        }

        IEnumerator DeathRoutine()
        {
            Vector3 baseScale = Visual != null ? Visual.localScale : transform.localScale;
            FxManager.Burst(transform.position + new Vector3(0f, 0.42f, 0f),
                new Color(0.93f, 0.96f, 0.82f), 14, 2f, 6f, 0.45f, 0.13f, 12f);

            float t = 0f;
            while (t < 0.26f)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / 0.26f);
                if (Visual != null)
                    Visual.localScale = new Vector3(baseScale.x * (1f + k * 0.7f),
                        baseScale.y * (1f - k), 1f);
                yield return null;
            }
            Destroy(gameObject);
        }

        protected void MakeVisual(string spriteKey, int order)
        {
            Visual = new GameObject("Visual").transform;
            Visual.SetParent(transform, false);
            Sr = Visual.gameObject.AddComponent<SpriteRenderer>();
            Sr.sprite = PixelArt.Get(spriteKey);
            Sr.sharedMaterial = PixelArt.SpriteMaterial;
            Sr.sortingOrder = order;
        }

        protected static void AddHitbox(Transform parent, Vector2 size, Vector2 offset)
        {
            var go = new GameObject("Hitbox");
            go.transform.SetParent(parent, false);
            go.layer = GameLayers.Enemy;
            var box = go.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = size;
            box.offset = offset;
        }
    }

    /// <summary>Ground patroller that turns around at walls and platform edges.</summary>
    public class Slime : EnemyBase
    {
        public float Speed = 1.9f;

        int _dir = 1;
        float _time;
        Vector3 _baseScale;

        protected override void Awake()
        {
            base.Awake();
            MakeVisual("slime", 8);
            _baseScale = Visual.localScale;

            AddHitbox(transform, new Vector2(0.72f, 0.52f), new Vector2(0f, 0.33f));

            var bodyGo = new GameObject("Body");
            bodyGo.transform.SetParent(transform, false);
            bodyGo.layer = GameLayers.EnemyBody;
            var body = bodyGo.AddComponent<BoxCollider2D>();
            body.size = new Vector2(0.68f, 0.5f);
            body.offset = new Vector2(0f, 0.31f);

            _dir = Random.value < 0.5f ? -1 : 1;
        }

        void Update()
        {
            if (Dead) return;
            float dt = Time.deltaTime;
            Vector2 pos = transform.position;

            Vector2 eye = pos + new Vector2(_dir * 0.42f, 0.32f);
            bool groundAhead = Physics2D.Raycast(eye, Vector2.down, 0.9f,
                GameLayers.GroundMask).collider != null;
            bool wall = Physics2D.Raycast(pos + new Vector2(0f, 0.3f), new Vector2(_dir, 0f),
                0.58f, GameLayers.GroundMask).collider != null;

            if (!groundAhead || wall) _dir = -_dir;

            pos.x += _dir * Speed * dt;
            transform.position = pos;

            _time += dt;
            Sr.flipX = _dir < 0;
            float bob = Mathf.Sin(_time * 7f);
            Visual.localScale = new Vector3(_baseScale.x * (1f + bob * 0.03f),
                _baseScale.y * (1f - bob * 0.05f), 1f);
            Visual.localPosition = new Vector3(0f, Mathf.Abs(Mathf.Sin(_time * 5f)) * 0.04f, 0f);
        }
    }

    /// <summary>Flying sine-wave enemy. Stompable when hit from above.</summary>
    public class Bat : EnemyBase
    {
        public float Range = 2.3f;
        public float Speed = 1.35f;
        public float BobHeight = 0.55f;

        float _time;
        Vector3 _origin;
        Vector3 _baseScale;

        protected override void Awake()
        {
            base.Awake();
            MakeVisual("bat_a", 9);
            _baseScale = Visual.localScale;
            SpriteAnim.Add(gameObject, Sr,
                new[] { PixelArt.Get("bat_a"), PixelArt.Get("bat_b") }, 8f);
            AddHitbox(transform, new Vector2(0.6f, 0.42f), Vector2.zero);
            _time = Random.value * 6.28f;
        }

        void Start()
        {
            _origin = transform.position;
        }

        void Update()
        {
            if (Dead) return;
            float dt = Time.deltaTime;
            _time += dt * Speed;

            float x = _origin.x + Mathf.Sin(_time) * Range;
            float y = _origin.y + Mathf.Sin(_time * 1.9f + 0.7f) * BobHeight;
            transform.position = new Vector3(x, y, _origin.z);

            Sr.flipX = Mathf.Cos(_time) < 0f;
            Visual.localScale = new Vector3(_baseScale.x,
                _baseScale.y * (1f + Mathf.Sin(_time * 6f) * 0.03f), 1f);
        }
    }

    /// <summary>Marker component for anything that damages the player on touch.</summary>
    public class Hazard : MonoBehaviour
    {
        void Awake()
        {
            gameObject.layer = GameLayers.Hazard;
        }
    }

    public static class EnemyFactory
    {
        public static void SpawnSlime(Vector3 position, Transform parent)
        {
            var go = new GameObject("Slime");
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.AddComponent<Slime>();
        }

        public static void SpawnBat(Vector3 position, Transform parent)
        {
            var go = new GameObject("Bat");
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.AddComponent<Bat>();
        }

        public static void SpawnSpike(Vector3 position, Transform parent)
        {
            var go = new GameObject("Spike");
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.layer = GameLayers.Hazard;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PixelArt.Get("spike");
            sr.sharedMaterial = PixelArt.SpriteMaterial;
            sr.sortingOrder = 6;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.92f, 0.38f);
            col.offset = new Vector2(0f, 0.2f);

            go.AddComponent<Hazard>();
        }
    }
}
