using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure
{
    /// <summary>Lightweight pooled pixel particles. Everything is a 1x1 sprite, tinted per particle.</summary>
    public class FxManager : MonoBehaviour
    {
        public static FxManager Instance { get; private set; }

        class P
        {
            public Transform Tr;
            public SpriteRenderer Sr;
            public Vector2 Vel;
            public float Life;
            public float MaxLife;
            public float Gravity;
            public float Size;
            public float Spin;
            public Color Color;
            public bool Active;
        }

        readonly List<P> _pool = new List<P>();
        int _cursor;
        Sprite _sprite;

        public static void Ensure(GameObject host)
        {
            if (Instance != null) return;
            var go = new GameObject("Pixel FX");
            go.transform.SetParent(host.transform, false);
            Instance = go.AddComponent<FxManager>();
        }

        void Awake()
        {
            Instance = this;
            _sprite = PixelArt.Get("pixel_white");
            for (int i = 0; i < 280; i++)
            {
                var go = new GameObject("p" + i);
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _sprite;
                sr.sharedMaterial = PixelArt.SpriteMaterial;
                sr.sortingOrder = 40;
                go.SetActive(false);
                _pool.Add(new P { Tr = go.transform, Sr = sr, Active = false });
            }
        }

        P Take()
        {
            for (int i = 0; i < _pool.Count; i++)
            {
                int index = (_cursor + i) % _pool.Count;
                if (_pool[index].Active) continue;
                _cursor = (index + 1) % _pool.Count;
                return _pool[index];
            }
            var p = _pool[_cursor];
            _cursor = (_cursor + 1) % _pool.Count;
            return p;
        }

        public static void Emit(Vector3 position, Color color, Vector2 velocity, float life = 0.5f,
            float size = 0.12f, float gravity = 0f, float spin = 0f)
        {
            if (Instance == null) return;
            P p = Instance.Take();
            p.Active = true;
            p.Tr.gameObject.SetActive(true);
            p.Tr.position = position;
            p.Vel = velocity;
            p.Life = life;
            p.MaxLife = Mathf.Max(0.01f, life);
            p.Gravity = gravity;
            p.Size = size;
            p.Spin = spin;
            p.Color = color;
            p.Tr.localScale = Vector3.one * size;
            p.Tr.localRotation = Quaternion.identity;
            p.Sr.color = color;
            p.Sr.enabled = true;
        }

        public static void Burst(Vector3 position, Color color, int count, float speedMin = 2f,
            float speedMax = 6f, float life = 0.5f, float size = 0.12f, float gravity = 12f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = Random.value * Mathf.PI * 2f;
                float speed = Random.Range(speedMin, speedMax);
                Vector2 vel = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed + Vector2.up * 1.5f;
                Emit(position, color, vel, life * Random.Range(0.65f, 1.25f),
                    size * Random.Range(0.7f, 1.3f), gravity);
            }
        }

        public static void Dust(Vector3 position, Color color, int count = 5, float direction = 0f)
        {
            for (int i = 0; i < count; i++)
            {
                float dir = direction + Random.Range(-0.9f, 0.9f);
                Vector2 vel = new Vector2(Mathf.Cos(dir) * Random.Range(1f, 2.6f),
                    Random.Range(1.2f, 3.2f));
                Emit(position, color, vel, Random.Range(0.25f, 0.45f),
                    Random.Range(0.08f, 0.16f), 18f);
            }
        }

        public static void ClearAll()
        {
            if (Instance == null) return;
            foreach (P p in Instance._pool)
            {
                p.Active = false;
                p.Tr.gameObject.SetActive(false);
            }
        }

        void Update()
        {
            float dt = Time.deltaTime;
            for (int i = 0; i < _pool.Count; i++)
            {
                P p = _pool[i];
                if (!p.Active) continue;
                p.Life -= dt;
                if (p.Life <= 0f)
                {
                    p.Active = false;
                    p.Tr.gameObject.SetActive(false);
                    continue;
                }

                p.Vel.y -= p.Gravity * dt;
                p.Tr.position += (Vector3)(p.Vel * dt);
                p.Tr.Rotate(0f, 0f, p.Spin * dt);

                float t = Mathf.Clamp01(p.Life / p.MaxLife);
                Color c = p.Color;
                c.a *= t * t;
                p.Sr.color = c;
                p.Tr.localScale = Vector3.one * p.Size * Mathf.Lerp(0.35f, 1f, t);
            }
        }
    }

    /// <summary>Simple frame animation for generated sprites.</summary>
    public class SpriteAnim : MonoBehaviour
    {
        public SpriteRenderer Renderer;
        public Sprite[] Frames;
        public float Fps = 6f;
        public bool Loop = true;
        public bool Playing = true;
        float _time;
        int _frame;

        public static SpriteAnim Add(GameObject go, SpriteRenderer sr, Sprite[] frames, float fps, bool loop = true)
        {
            var anim = go.AddComponent<SpriteAnim>();
            anim.Renderer = sr;
            anim.Frames = frames;
            anim.Fps = fps;
            anim.Loop = loop;
            if (frames != null && frames.Length > 0) sr.sprite = frames[0];
            return anim;
        }

        void Update()
        {
            if (!Playing || Frames == null || Frames.Length < 2) return;
            _time += Time.deltaTime * Fps;
            int frame = (int)_time;
            if (Loop) frame %= Frames.Length;
            else frame = Mathf.Min(frame, Frames.Length - 1);
            if (frame != _frame)
            {
                _frame = frame;
                Renderer.sprite = Frames[frame];
            }
        }
    }

    public class Twinkle : MonoBehaviour
    {
        public SpriteRenderer Renderer;
        public float Speed = 1.7f;
        public float Phase;
        public float MinAlpha = 0.25f;
        public float MaxAlpha = 1f;

        void Update()
        {
            if (Renderer == null) return;
            float t = (Mathf.Sin(Time.time * Speed + Phase) * 0.5f + 0.5f);
            Color c = Renderer.color;
            c.a = Mathf.Lerp(MinAlpha, MaxAlpha, t);
            Renderer.color = c;
        }
    }

    public class CloudDrift : MonoBehaviour
    {
        public float Speed = 0.35f;
        public float MinX = -10f;
        public float MaxX = 80f;

        void Update()
        {
            Vector3 p = transform.position;
            p.x += Speed * Time.deltaTime;
            if (p.x > MaxX) p.x = MinX;
            transform.position = p;
        }
    }

    public class AmbientMotes : MonoBehaviour
    {
        public Color ColorA = Color.white;
        public Color ColorB = Color.white;
        public float Rate = 0.12f;
        public float Rise = 0.3f;
        public float Drift = 0.4f;
        public float SizeMin = 0.06f;
        public float SizeMax = 0.16f;
        public float LifeMin = 2.5f;
        public float LifeMax = 5f;
        Camera _camera;
        float _accum;

        void Start()
        {
            _camera = Camera.main;
        }

        void Update()
        {
            if (_camera == null) { _camera = Camera.main; if (_camera == null) return; }
            _accum += Time.deltaTime;
            float interval = 1f / Mathf.Max(0.01f, Rate);
            while (_accum >= interval)
            {
                _accum -= interval;
                float halfH = _camera.orthographicSize;
                float halfW = halfH * _camera.aspect;
                Vector3 cam = _camera.transform.position;
                Vector3 pos = new Vector3(
                    cam.x + Random.Range(-halfW * 1.15f, halfW * 1.15f),
                    cam.y + Random.Range(-halfH * 1.05f, halfH * 1.05f),
                    0f);
                Color color = Color.Lerp(ColorA, ColorB, Random.value);
                Vector2 vel = new Vector2(Random.Range(-Drift, Drift), Rise * Random.Range(0.6f, 1.4f));
                float life = Random.Range(LifeMin, LifeMax);
                float size = Random.Range(SizeMin, SizeMax);
                FxManager.Emit(pos, color, vel, life, size, 0f);
            }
        }
    }
}
