using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Hewan taman (hanya hiasan, tanpa physics):
    /// - kupu-kupu beterbangan berkelok di atas taman dengan warna acak;
    /// - burung gereja melompat & mematuk di tanah, terbang menjauh bila pemain mendekat,
    ///   lalu kembali beberapa detik kemudian.
    /// Muncul di siang hari saja; berhenti saat menu/dialog; tidak digambar di luar layar.
    /// </summary>
    public sealed class LifeCritters : MonoBehaviour
    {
        static readonly Color[] WingColors =
        {
            new Color(1f, 0.85f, 0.35f), new Color(1f, 0.6f, 0.8f), new Color(0.6f, 0.8f, 1f),
            new Color(1f, 1f, 1f), new Color(1f, 0.65f, 0.35f), new Color(0.75f, 0.6f, 1f),
        };

        class Butterfly
        {
            public SpriteRenderer Sr;
            public Rect Area;
            public Vector2 Pos, Target;
            public float Anim, Speed;
        }

        enum BirdState { Ground, Flying, Away }

        class Bird
        {
            public SpriteRenderer Sr;
            public Rect Area;
            public Vector2 Pos, Vel;
            public BirdState State;
            public float Timer, Anim;
            public bool FacingRight;
        }

        readonly List<Butterfly> _butterflies = new List<Butterfly>();
        readonly List<Bird> _birds = new List<Bird>();

        bool _hiddenForLow;

        public static LifeCritters Attach(Transform regionRoot) => regionRoot.gameObject.AddComponent<LifeCritters>();

        /// <summary>Tambah kupu-kupu dan burung di sebuah area taman (koordinat dunia).</summary>
        public void AddGarden(Rect area, int butterflies, int birds)
        {
            for (int i = 0; i < butterflies; i++)
            {
                var b = new Butterfly { Area = area, Speed = Random.Range(0.8f, 1.4f), Anim = Random.value };
                b.Pos = RandomIn(area);
                b.Target = RandomIn(area);
                b.Sr = MakeSprite("KupuKupu", "lc_butterfly_0", 0.9f);
                b.Sr.color = WingColors[Random.Range(0, WingColors.Length)];
                _butterflies.Add(b);
            }
            for (int i = 0; i < birds; i++)
            {
                var b = new Bird { Area = area, State = BirdState.Ground, Timer = Random.Range(0.5f, 2f), Anim = Random.value };
                b.Pos = RandomIn(area);
                b.FacingRight = Random.value > 0.5f;
                b.Sr = MakeSprite("Burung", "lc_bird_0", 1f);
                _birds.Add(b);
            }
        }

        SpriteRenderer MakeSprite(string name, string sprite, float scale)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.localScale = Vector3.one * scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PixelArt.Get(sprite);
            sr.sharedMaterial = PixelArt.SpriteMaterial;
            return sr;
        }

        static Vector2 RandomIn(Rect r) => new Vector2(Random.Range(r.xMin, r.xMax), Random.Range(r.yMin, r.yMax));

        void Update()
        {
            // Mode hemat efek: kupu-kupu & burung disembunyikan dan tidak diperbarui.
            bool low = LifeGraphics.LowEffects;
            if (low != _hiddenForLow)
            {
                _hiddenForLow = low;
                foreach (var b in _butterflies) b.Sr.gameObject.SetActive(!low);
                foreach (var b in _birds) b.Sr.gameObject.SetActive(!low);
            }
            if (low) return;

            var game = PulangGame.Instance;
            bool day = LifeWorld.NightLevel < 0.5f;
            bool moving = game != null && game.CanAmbientMove;
            float dt = moving ? Mathf.Min(Time.unscaledDeltaTime, 0.05f) : 0f;
            Vector2 player = game != null ? game.PlayerFeet : new Vector2(-999f, -999f);

            foreach (var b in _butterflies) UpdateButterfly(b, dt, day);
            foreach (var b in _birds) UpdateBird(b, dt, day, player);
        }

        void UpdateButterfly(Butterfly b, float dt, bool day)
        {
            bool visible = day && LifeView.Sees(b.Pos, 1f, 1f);
            if (b.Sr.enabled != visible) b.Sr.enabled = visible;
            if (dt <= 0f) return;

            // Terbang berkelok menuju titik acak; ganti tujuan saat dekat.
            Vector2 to = b.Target - b.Pos;
            if (to.sqrMagnitude < 0.2f) b.Target = RandomIn(b.Area);
            Vector2 dir = to.normalized;
            Vector2 wiggle = new Vector2(-dir.y, dir.x) * Mathf.Sin(Time.unscaledTime * 6f + b.Anim * 10f) * 0.8f;
            b.Pos += (dir + wiggle) * b.Speed * dt;
            b.Anim += dt * 10f;
            if (!visible) return;

            float hover = Mathf.Sin(Time.unscaledTime * 3f + b.Anim) * 0.15f + 1.1f;   // melayang di atas tanah
            b.Sr.transform.position = new Vector3(b.Pos.x, b.Pos.y + hover, 0f);
            b.Sr.sprite = PixelArt.Get(((int)b.Anim) % 2 == 0 ? "lc_butterfly_0" : "lc_butterfly_1");
            b.Sr.sortingOrder = -Mathf.RoundToInt(b.Pos.y * PixelArt.PPU) + 4;
        }

        void UpdateBird(Bird b, float dt, bool day, Vector2 player)
        {
            bool visible = day && b.State != BirdState.Away && LifeView.Sees(b.Pos, 1f, 1f);
            if (b.Sr.enabled != visible) b.Sr.enabled = visible;
            if (dt <= 0f) return;
            b.Timer -= dt;
            b.Anim += dt;

            switch (b.State)
            {
                case BirdState.Ground:
                    // Pemain mendekat: terbang menjauh.
                    if ((player - b.Pos).sqrMagnitude < 2.4f * 2.4f)
                    {
                        b.State = BirdState.Flying;
                        b.FacingRight = b.Pos.x >= player.x;
                        b.Vel = new Vector2(b.FacingRight ? 4.5f : -4.5f, 3.2f);
                        b.Timer = 2.2f;
                        if (visible) PixelAdventure.GameAudio.Play("flap", 0.25f, Random.Range(0.9f, 1.15f));
                        break;
                    }
                    // Melompat kecil ke arah acak atau mematuk tanah.
                    if (b.Timer <= 0f)
                    {
                        b.Timer = Random.Range(0.4f, 1.6f);
                        if (Random.value < 0.5f)
                        {
                            Vector2 hop = new Vector2(Random.Range(-0.6f, 0.6f), Random.Range(-0.3f, 0.3f));
                            Vector2 np = b.Pos + hop;
                            if (b.Area.Contains(np)) { b.Pos = np; b.FacingRight = hop.x >= 0f; }
                        }
                    }
                    break;

                case BirdState.Flying:
                    b.Pos += b.Vel * dt;
                    if (b.Timer <= 0f) { b.State = BirdState.Away; b.Timer = Random.Range(6f, 12f); }
                    break;

                case BirdState.Away:
                    // Kembali mendarat di tempat acak yang jauh dari pemain.
                    if (b.Timer <= 0f)
                    {
                        Vector2 p = RandomIn(b.Area);
                        if ((p - player).sqrMagnitude > 5f * 5f) { b.Pos = p; b.State = BirdState.Ground; b.Timer = 1f; }
                        else b.Timer = 1f;
                    }
                    break;
            }
            if (!b.Sr.enabled) return;

            float y = b.State == BirdState.Flying ? b.Pos.y + (2.2f - b.Timer) * 0.8f : b.Pos.y;
            b.Sr.transform.position = new Vector3(b.Pos.x, y, 0f);
            b.Sr.flipX = !b.FacingRight;
            string sprite = b.State == BirdState.Flying
                ? (((int)(b.Anim * 14f)) % 2 == 0 ? "lc_bird_fly_0" : "lc_bird_fly_1")
                : (Mathf.Sin(b.Anim * 3f + b.Pos.x) > 0.6f ? "lc_bird_1" : "lc_bird_0");   // sesekali mematuk
            b.Sr.sprite = PixelArt.Get(sprite);
            b.Sr.sortingOrder = b.State == BirdState.Flying ? 2900 : -Mathf.RoundToInt(b.Pos.y * PixelArt.PPU) + 1;
        }
    }
}
