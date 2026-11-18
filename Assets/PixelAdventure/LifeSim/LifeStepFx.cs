using PixelAdventure.LifeSim.Pulang;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>Jenis permukaan tanah di bawah kaki.</summary>
    public enum Surface { Hard, Dirt, Grass, Sand, Floor }

    /// <summary>
    /// Efek berjalan: suara langkah sesuai permukaan (aspal/trotoar, tanah, rumput, pasir, lantai)
    /// dan kepulan debu (saat berlari, atau berjalan di tanah/pasir). Debu memakai kumpulan sprite
    /// yang dipakai ulang, membesar lalu memudar.
    /// </summary>
    public sealed class LifeStepFx : MonoBehaviour
    {
        const int PoolSize = 16;
        static LifeStepFx _instance;

        readonly SpriteRenderer[] _puffs = new SpriteRenderer[PoolSize];
        readonly float[] _age = new float[PoolSize];
        readonly Vector2[] _drift = new Vector2[PoolSize];
        int _next;

        static LifeStepFx Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = new GameObject("EfekLangkah").AddComponent<LifeStepFx>();
                for (int i = 0; i < PoolSize; i++)
                {
                    var go = new GameObject("Debu" + i);
                    go.transform.SetParent(_instance.transform, false);
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = PixelArt.Get("lc_dust");
                    sr.sharedMaterial = PixelArt.SpriteMaterial;
                    go.SetActive(false);
                    _instance._puffs[i] = sr;
                    _instance._age[i] = 99f;
                }
                return _instance;
            }
        }

        /// <summary>Dipanggil setiap kaki menapak.</summary>
        public static void Step(LifeWorld world, Vector2 feet, bool running, int dir)
        {
            Surface s = world != null ? world.SurfaceAt(feet) : Surface.Hard;
            string sfx = s == Surface.Hard ? "step_hard" : s == Surface.Grass ? "step_grass"
                : s == Surface.Sand ? "step_sand" : s == Surface.Floor ? "step_wood" : "step";
            GameAudio.Play(sfx, running ? 0.42f : 0.3f, Random.Range(0.9f, 1.1f) * (running ? 1.08f : 1f));

            bool dusty = s == Surface.Dirt || s == Surface.Sand;
            if (s != Surface.Floor && (running || dusty) && !LifeGraphics.LowEffects)   // mode hemat: tanpa debu
            {
                // Debu sedikit di belakang kaki, melayang ke arah berlawanan langkah.
                Vector2 back = dir == 2 ? Vector2.right : dir == 3 ? Vector2.left : dir == 1 ? Vector2.down : Vector2.up;
                Color tint = s == Surface.Sand ? new Color(0.95f, 0.86f, 0.62f) : s == Surface.Grass ? new Color(0.75f, 0.85f, 0.6f)
                    : s == Surface.Hard ? new Color(0.85f, 0.85f, 0.85f) : new Color(0.82f, 0.7f, 0.52f);
                Instance.Spawn(feet + back * 0.15f, back * 0.5f + Vector2.up * 0.25f, tint, running ? 1f : 0.7f);
            }
        }

        void Spawn(Vector2 pos, Vector2 drift, Color tint, float strength)
        {
            int i = _next;
            _next = (_next + 1) % PoolSize;
            var sr = _puffs[i];
            sr.gameObject.SetActive(true);
            sr.transform.position = new Vector3(pos.x + Random.Range(-0.08f, 0.08f), pos.y + 0.05f, 0f);
            sr.color = new Color(tint.r, tint.g, tint.b, 0.55f * strength);
            sr.sortingOrder = -Mathf.RoundToInt(pos.y * PixelArt.PPU) + 2;
            _age[i] = 0f;
            _drift[i] = drift;
        }

        void Update()
        {
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            for (int i = 0; i < PoolSize; i++)
            {
                if (_age[i] > 0.5f) { if (_puffs[i].gameObject.activeSelf) _puffs[i].gameObject.SetActive(false); continue; }
                _age[i] += dt;
                float k = _age[i] / 0.5f;
                var sr = _puffs[i];
                sr.transform.position += (Vector3)(_drift[i] * dt);
                sr.transform.localScale = Vector3.one * (0.6f + k * 0.9f);
                Color c = sr.color;
                c.a = Mathf.Max(0f, c.a - dt * 1.2f);
                sr.color = c;
            }
        }
    }

    public partial class LifeWorld
    {
        static bool In(Vector2 p, float x, float y, float w, float h) => p.x >= x && p.x < x + w && p.y >= y && p.y < y + h;

        /// <summary>Permukaan di titik ini (mengikuti jaringan jalan tiap wilayah di LifeArtTown).</summary>
        public Surface SurfaceAt(Vector2 p)
        {
            if (InInterior) return Surface.Floor;
            switch (CurrentRegion)
            {
                case Region.KotaJogokerto:
                    // Taman Pelangi & Alun-Alun berumput; sisanya jalan & paving.
                    float tx = (p.x - 18f) / 14f, ty = (p.y - 58f) / 10f;
                    if (tx * tx + ty * ty < 1f) return Surface.Grass;
                    if (In(p, 63.5f, 50f, 32f, 22f)) return Surface.Grass;
                    return Surface.Hard;
                case Region.PasarRamaiJaya:
                    return Surface.Hard;
                case Region.PantaiCemara:
                    if (In(p, 0f, 10f, 96f, 4f)) return Surface.Hard;     // jalan
                    if (In(p, 0f, 4f, 96f, 6f)) return Surface.Grass;     // jalur cemara
                    return Surface.Sand;
                case Region.KampungCempaka:
                    if (In(p, 0f, 30f, 96f, 3f) || In(p, 46f, 0f, 3f, 72f) || In(p, 30f, 33f, 2f, 28f)) return Surface.Dirt;
                    return Surface.Grass;
                default: // Desa
                    if (In(p, 0f, 30f, 96f, 3f) || In(p, 50f, 0f, 3f, 72f)) return Surface.Dirt;
                    return Surface.Grass;
            }
        }
    }
}
