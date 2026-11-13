using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Taman Pelangi yang lebih hidup: air mancur bertingkat (beranimasi & bercahaya malam),
    /// area bermain (perosotan, ayunan, jungkat-jungkit) dengan anak-anak yang berlarian,
    /// gazebo, bedeng bunga, bangku & lampu taman. Alun-Alun ikut diberi bunga & bangku.
    /// Koordinat mengikuti jalan setapak di LifeArtTown.ParkPaths.
    /// </summary>
    public partial class LifeWorld
    {
        void BuildPark()
        {
            // Air mancur besar di tengah cincin paving
            Animated(new[] { "lc_fountain_big_0", "lc_fountain_big_1" }, 18f, 57.9f, 4f);
            AddObstacle(16f, 57.9f, 4f, 1.4f);
            AddLight(18f, 59.2f, 3.4f, 2.4f, new Color(0.6f, 0.85f, 1f), 0.4f);

            // Area bermain anak (di atas lantai karet)
            Make("lc_slide", 25f, 54.4f, "Perosotan", Order(54.4f));
            AddObstacle(23.9f, 54.4f, 2.2f, 0.5f);
            Animated(new[] { "lc_swing_0", "lc_swing_1", "lc_swing_2", "lc_swing_1" }, 28f, 55.6f, 3f);
            AddObstacle(26.6f, 55.6f, 2.8f, 0.3f);
            Animated(new[] { "lc_seesaw_0", "lc_seesaw_1" }, 26.5f, 52.9f, 1.3f);
            AddObstacle(25.2f, 52.9f, 2.6f, 0.3f);

            // Gazebo, bunga, bangku, lampu taman
            Make("lc_gazebo", 9.5f, 55.4f, "Gazebo", Order(55.4f));
            AddObstacle(7.8f, 55.4f, 3.4f, 0.6f);
            foreach (var p in new[] { new Vector2(12.3f, 61.6f), new Vector2(23.8f, 61.0f), new Vector2(21.4f, 53.4f) })
            {
                Make("lc_flowerbed", p.x, p.y, "BedengBunga", Order(p.y));
                AddObstacle(p.x - 1.1f, p.y, 2.2f, 0.5f);
            }
            Make("lc_bench", 14.6f, 62.8f, "BangkuTaman", Order(62.8f));
            Make("lc_bench", 21.4f, 62.8f, "BangkuTaman2", Order(62.8f));
            Lamps(new[] { 15.4f, 53.8f, 20.6f, 63.6f });

            // Alun-Alun Merdeka: bedeng bunga & bangku di sekitar air mancurnya
            foreach (var p in new[] { new Vector2(77f, 54.4f), new Vector2(83f, 54.4f), new Vector2(77f, 63.8f), new Vector2(83f, 63.8f) })
            {
                Make("lc_flowerbed", p.x, p.y, "BedengAlun", Order(p.y));
                AddObstacle(p.x - 1.1f, p.y, 2.2f, 0.5f);
            }
            Make("lc_bench", 75.6f, 61.4f, "BangkuAlun2", Order(61.4f));
            Make("lc_bench", 84.4f, 61.4f, "BangkuAlun3", Order(61.4f));

            LifeParkKids.Attach(_build, _buildObstacles, new[]
            {
                new Rect(23.6f, 52.7f, 5.8f, 4.8f),      // area bermain
                new Rect(13f, 55f, 10f, 7f),             // cincin air mancur
                new Rect(13f, 55f, 16f, 7f),
            });
        }

        /// <summary>Sprite diam beranimasi (air mancur, ayunan, jungkat-jungkit).</summary>
        void Animated(string[] frames, float x, float y, float fps)
        {
            var sr = Make(frames[0], x, y, frames[0], Order(y));
            var a = sr.gameObject.AddComponent<LifeFlipbook>();
            a.Frames = frames;
            a.Fps = fps;
        }
    }

    /// <summary>Ganti-ganti sprite berurutan (hanya saat terlihat).</summary>
    public sealed class LifeFlipbook : MonoBehaviour
    {
        public string[] Frames;
        public float Fps = 3f;
        SpriteRenderer _sr;
        int _last = -1;

        void Awake() => _sr = GetComponent<SpriteRenderer>();

        void Update()
        {
            if (_sr == null || !_sr.enabled || Frames == null || Frames.Length == 0) return;
            int i = (int)(Time.time * Fps) % Frames.Length;
            if (i == _last) return;
            _last = i;
            _sr.sprite = PixelArt.Get(Frames[i]);
        }
    }

    /// <summary>Anak-anak berlarian di taman (siang hari saja). Memakai sprite warga yang diperkecil.</summary>
    public sealed class LifeParkKids : MonoBehaviour
    {
        static readonly string[] DirNames = { "down", "up", "left", "right" };
        static readonly int[] WalkCycle = { 1, 0, 2, 0 };

        class Kid
        {
            public Transform T;
            public SpriteRenderer Body;
            public string Id;
            public Rect Area;
            public Vector2 Pos, Target;
            public float Wait, Anim, Speed;
            public int Dir;
        }

        readonly List<Kid> _kids = new List<Kid>();
        List<Rect> _obstacles;

        public static LifeParkKids Attach(Transform parent, List<Rect> obstacles, Rect[] areas)
        {
            var c = new GameObject("AnakAnak").AddComponent<LifeParkKids>();
            c.transform.SetParent(parent, false);
            c._obstacles = obstacles;
            for (int i = 0; i < areas.Length; i++)
            {
                var go = new GameObject("Anak" + i);
                go.transform.SetParent(c.transform, false);
                go.transform.localScale = Vector3.one * 0.72f;
                var body = go.AddComponent<SpriteRenderer>();
                body.sharedMaterial = PixelArt.SpriteMaterial;
                var k = new Kid
                {
                    T = go.transform, Body = body, Id = "warga" + (1 + (i * 2 + 1) % 6), Area = areas[i],
                    Speed = Random.Range(1.4f, 2.1f), Wait = Random.Range(0f, 1.5f),
                };
                k.Pos = k.Target = c.Pick(k.Area, k.Area.center);
                c._kids.Add(k);
            }
            return c;
        }

        Vector2 Pick(Rect a, Vector2 fallback)
        {
            for (int t = 0; t < 14; t++)
            {
                var p = new Vector2(Random.Range(a.xMin, a.xMax), Random.Range(a.yMin, a.yMax));
                bool ok = true;
                foreach (var o in _obstacles)
                    if (p.x > o.xMin - 0.3f && p.x < o.xMax + 0.3f && p.y > o.yMin - 0.25f && p.y < o.yMax + 0.25f) { ok = false; break; }
                if (ok) return p;
            }
            return fallback;
        }

        void Update()
        {
            bool night = LifeWorld.NightLevel > 0.5f;
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            foreach (var k in _kids)
            {
                if (k.Body.enabled == night) k.Body.enabled = !night;
                if (night) continue;

                bool moving = false;
                if (k.Wait > 0f) k.Wait -= dt;
                else
                {
                    Vector2 d = k.Target - k.Pos;
                    if (d.magnitude < 0.08f)
                    {
                        k.Wait = Random.Range(0.4f, 2.5f);
                        k.Target = Pick(k.Area, k.Pos);
                    }
                    else
                    {
                        k.Pos += d.normalized * Mathf.Min(d.magnitude, k.Speed * dt);
                        k.Anim += dt;
                        moving = true;
                        k.Dir = Mathf.Abs(d.x) > Mathf.Abs(d.y) ? (d.x < 0f ? 2 : 3) : (d.y < 0f ? 0 : 1);
                    }
                }
                int frame = moving ? WalkCycle[(int)(k.Anim * 9f) % WalkCycle.Length] : 0;
                k.Body.sprite = PixelArt.Get("lc_" + k.Id + "_" + DirNames[k.Dir] + "_" + frame);
                k.T.position = new Vector3(k.Pos.x, k.Pos.y + (moving ? Mathf.Abs(Mathf.Sin(k.Anim * 14f)) * 0.05f : 0f), 0f);
                k.Body.sortingOrder = -Mathf.RoundToInt(k.Pos.y * PixelArt.PPU);
            }
        }
    }
}
