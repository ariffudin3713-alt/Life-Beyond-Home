using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Lalu lintas satu wilayah: kendaraan di lajur jalan raya, pejalan kaki di trotoar,
    /// dan lampu lalu lintas. Tanpa physics; hanya bergerak saat permainan berjalan.
    ///
    /// Aturan kendaraan:
    /// - lajur kiri (ke timur di bagian utara jalan, ke barat di bagian selatan);
    /// - menjaga jarak dengan kendaraan di depannya;
    /// - berhenti di garis henti saat lampu kuning/merah;
    /// - mengerem bila pemain berdiri di depannya.
    /// </summary>
    public sealed class LifeTraffic : MonoBehaviour
    {
        public enum LightState { Green, Yellow, Red }

        // Siklus lampu untuk arah jalan utama (horizontal).
        const float GreenTime = 9f, YellowTime = 2.2f, RedTime = 7f;
        const float SpawnMargin = 5f;

        struct Lane
        {
            public float Y;
            public int Dir;       // +1 ke timur, -1 ke barat
        }

        class Vehicle
        {
            public SpriteRenderer Sr, Head;
            public int Lane;
            public float X, Speed, MaxSpeed, HalfLen;
            public float Blocked, HornCd;   // klakson saat dihalangi pemain
            public bool FacesRight;
        }

        class Walker
        {
            public Transform T;
            public SpriteRenderer Body, Shadow;
            public string Id;
            public Vector2 A, B;
            public bool ToB;
            public float Speed, Pause, Anim;
        }

        readonly List<Lane> _lanes = new List<Lane>();
        readonly List<Vehicle> _vehicles = new List<Vehicle>();
        readonly List<Walker> _walkers = new List<Walker>();
        readonly List<SpriteRenderer> _lights = new List<SpriteRenderer>();
        float _stopEast = float.NaN, _stopWest = float.NaN;
        float _lightClock;
        LightState _lightState = LightState.Green;
        Sprite _red, _yellow, _green;

        public static LifeTraffic Attach(Transform regionRoot)
        {
            var t = regionRoot.gameObject.AddComponent<LifeTraffic>();
            t._red = PixelArt.Get("lc_traffic_light_red");
            t._yellow = PixelArt.Get("lc_traffic_light_yellow");
            t._green = PixelArt.Get("lc_traffic_light_green");
            return t;
        }

        // ------------------------------------------------------------------ penyusunan

        public int AddLane(float y, int dir)
        {
            _lanes.Add(new Lane { Y = y, Dir = dir >= 0 ? 1 : -1 });
            return _lanes.Count - 1;
        }

        /// <param name="facesRight">Arah hadap gambar asli (motor = kanan, mobil/angkot/bus = kiri).</param>
        public void AddVehicle(int lane, string sprite, bool facesRight, float speed, float startX)
        {
            var go = new GameObject("Kendaraan_" + sprite);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PixelArt.Get(sprite);
            sr.sharedMaterial = PixelArt.SpriteMaterial;
            var v = new Vehicle
            {
                Sr = sr, Lane = lane, X = startX, MaxSpeed = speed, Speed = speed,
                HalfLen = sr.sprite != null ? sr.sprite.bounds.size.x * 0.5f : 2f,
                FacesRight = facesRight,
            };
            // Lampu depan (cahaya kuning pucat di depan kendaraan).
            var headGo = new GameObject("LampuDepan");
            headGo.transform.SetParent(go.transform, false);
            v.Head = headGo.AddComponent<SpriteRenderer>();
            v.Head.sprite = PixelArt.Get("lc_glow");
            v.Head.sharedMaterial = PixelArt.SpriteMaterial;
            v.Head.sortingOrder = LifeWorld.LightOrder + 1;
            headGo.transform.localScale = new Vector3(2.4f, 0.9f, 1f);
            v.Head.enabled = false;
            _vehicles.Add(v);
            PlaceVehicle(v);
        }

        /// <summary>Pejalan kaki bolak-balik antara a dan b (garis lurus di trotoar).</summary>
        public void AddWalker(string castId, Vector2 a, Vector2 b, float speed, float startT)
        {
            // Selalu acak: posisi awal, arah, dan kecepatan pejalan kaki berbeda setiap kali dimainkan.
            startT = Random.value;
            speed *= Random.Range(0.8f, 1.2f);
            AddWalkerAt(castId, a, b, speed, startT);
        }

        void AddWalkerAt(string castId, Vector2 a, Vector2 b, float speed, float startT)
        {
            var go = new GameObject("Warga_" + castId);
            go.transform.SetParent(transform, false);
            go.transform.position = Vector2.Lerp(a, b, Mathf.Clamp01(startT));

            var body = new GameObject("Body").AddComponent<SpriteRenderer>();
            body.transform.SetParent(go.transform, false);
            body.sharedMaterial = PixelArt.SpriteMaterial;
            var shadow = new GameObject("Shadow").AddComponent<SpriteRenderer>();
            shadow.transform.SetParent(go.transform, false);
            shadow.transform.localPosition = new Vector3(0f, 0.06f, 0f);
            shadow.sprite = PixelArt.Get("lc_shadow");
            shadow.sharedMaterial = PixelArt.SpriteMaterial;

            var w = new Walker
            {
                T = go.transform, Body = body, Shadow = shadow, Id = castId,
                A = a, B = b, ToB = startT < 0.5f, Speed = speed,
                Pause = Random.Range(0f, 1.5f), Anim = Random.Range(0f, 1f),
            };
            _walkers.Add(w);
            ApplyWalker(w, w.ToB ? b - a : a - b, false);
        }

        public void AddTrafficLight(SpriteRenderer sr) => _lights.Add(sr);

        /// <summary>Garis henti (posisi bumper depan) untuk arah timur dan barat.</summary>
        public void SetStopLines(float eastStopX, float westStopX)
        {
            _stopEast = eastStopX;
            _stopWest = westStopX;
        }

        // ------------------------------------------------------------------ loop

        void Update()
        {
            var game = PulangGame.Instance;
            if (game == null || !game.CanAmbientMove) return;
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            Vector2 player = game.PlayerFeet;

            UpdateLights(dt);
            for (int i = 0; i < _vehicles.Count; i++) UpdateVehicle(_vehicles[i], dt, player);
            for (int i = 0; i < _walkers.Count; i++) UpdateWalker(_walkers[i], dt, player);
        }

        void UpdateLights(float dt)
        {
            if (_lights.Count == 0) return;
            _lightClock = (_lightClock + dt) % (GreenTime + YellowTime + RedTime);
            LightState s = _lightClock < GreenTime ? LightState.Green
                : _lightClock < GreenTime + YellowTime ? LightState.Yellow : LightState.Red;
            if (s == _lightState && _lights[0].sprite != null) return;
            _lightState = s;
            Sprite sp = s == LightState.Green ? _green : s == LightState.Yellow ? _yellow : _red;
            foreach (var l in _lights) l.sprite = sp;
        }

        void UpdateVehicle(Vehicle v, float dt, Vector2 player)
        {
            var lane = _lanes[v.Lane];
            int dir = lane.Dir;
            float front = v.X + dir * v.HalfLen;
            float target = v.MaxSpeed;
            bool playerBlocks = false;

            // 1) jaga jarak dengan kendaraan di depan pada lajur yang sama
            float gap = float.MaxValue;
            for (int i = 0; i < _vehicles.Count; i++)
            {
                var o = _vehicles[i];
                if (o == v || o.Lane != v.Lane) continue;
                float d = (o.X - v.X) * dir;
                if (d <= 0f) continue;
                gap = Mathf.Min(gap, d - v.HalfLen - o.HalfLen);
            }
            if (gap < 0.9f) target = 0f;
            else if (gap < 4f) target = Mathf.Min(target, v.MaxSpeed * (gap - 0.9f) / 3.1f);

            // 2) pemain di depan kendaraan: rem
            if (player.y > lane.Y - 0.6f && player.y < lane.Y + 1.7f)
            {
                float d = (player.x - front) * dir;
                if (d > -v.HalfLen && d < 1.6f) { target = 0f; playerBlocks = true; }
                else if (d >= 1.6f && d < 4.5f) target = Mathf.Min(target, v.MaxSpeed * (d - 1.6f) / 2.9f);
            }

            // 3) lampu lalu lintas
            float stop = dir > 0 ? _stopEast : _stopWest;
            if (_lights.Count > 0 && !float.IsNaN(stop) && _lightState != LightState.Green)
            {
                float toLine = (stop - front) * dir;
                // Kuning & sudah terlalu dekat garis: terus jalan (tidak sempat berhenti).
                bool commit = _lightState == LightState.Yellow && toLine < 1.2f;
                if (!commit && toLine > -0.1f && toLine < 6f)
                    target = Mathf.Min(target, toLine < 0.25f ? 0f : v.MaxSpeed * toLine / 6f);
            }

            // Diklakson bila pemain menghalangi lebih dari 1,2 detik (jeda 5 detik antar klakson, hanya bila terlihat).
            v.HornCd = Mathf.Max(0f, v.HornCd - dt);
            if (playerBlocks) v.Blocked += dt; else v.Blocked = 0f;
            if (v.Blocked > 1.2f && v.HornCd <= 0f && v.Sr.enabled)
            {
                GameAudio.Play("horn", 0.35f, Random.Range(0.95f, 1.05f));
                v.HornCd = 5f;
            }

            float accel = target < v.Speed ? 16f : 5f;
            v.Speed = Mathf.MoveTowards(v.Speed, target, accel * dt);
            v.X += dir * v.Speed * dt;

            // 4) keluar peta -> muncul lagi di sisi awal bila lajur di sana kosong
            float exitX = dir > 0 ? LifeWorld.Width + SpawnMargin : -SpawnMargin;
            if ((v.X - exitX) * dir > v.HalfLen)
            {
                float spawnX = dir > 0 ? -SpawnMargin - v.HalfLen : LifeWorld.Width + SpawnMargin + v.HalfLen;
                if (LaneClearAt(v, spawnX)) { v.X = spawnX; v.Speed = v.MaxSpeed; }
                else v.X = exitX + dir * (v.HalfLen + 0.01f);   // tunggu di luar peta
            }
            PlaceVehicle(v);
        }

        bool LaneClearAt(Vehicle self, float x)
        {
            for (int i = 0; i < _vehicles.Count; i++)
            {
                var o = _vehicles[i];
                if (o == self || o.Lane != self.Lane) continue;
                if (Mathf.Abs(o.X - x) < o.HalfLen + self.HalfLen + 2.5f) return false;
            }
            return true;
        }

        void PlaceVehicle(Vehicle v)
        {
            var lane = _lanes[v.Lane];
            // Simulasi tetap jalan (arus lalu lintas benar), tapi gambar di luar layar dimatikan.
            bool visible = LifeView.Sees(new Vector2(v.X, lane.Y), v.HalfLen + 1f, 3f);
            if (v.Sr.enabled != visible) { v.Sr.enabled = visible; if (!visible) v.Head.enabled = false; }
            if (!visible) return;
            v.Sr.transform.position = new Vector3(Mathf.Round(v.X * 16f) / 16f, lane.Y, 0f);
            v.Sr.flipX = v.FacesRight ? lane.Dir < 0 : lane.Dir > 0;
            v.Sr.sortingOrder = -Mathf.RoundToInt(lane.Y * PixelArt.PPU);

            // Lampu depan menyala di malam hari (digambar di atas lapisan gelap).
            float night = LifeWorld.NightLevel;
            bool head = night > 0.05f;
            if (v.Head.enabled != head) v.Head.enabled = head;
            if (head)
            {
                v.Head.transform.localPosition = new Vector3(lane.Dir * (v.HalfLen + 0.9f), 0.45f, 0f);
                v.Head.color = new Color(1f, 0.95f, 0.75f, 0.7f * night);
            }
        }

        // ------------------------------------------------------------------ pejalan kaki

        void UpdateWalker(Walker w, float dt, Vector2 player)
        {
            Vector2 pos = w.T.position;
            Vector2 goal = w.ToB ? w.B : w.A;
            Vector2 delta = goal - pos;

            if (w.Pause > 0f)
            {
                w.Pause -= dt;
                ApplyWalker(w, delta, false);
                return;
            }

            if (delta.sqrMagnitude < 0.01f)
            {
                w.ToB = !w.ToB;
                w.Pause = Random.Range(0.6f, 2.6f);
                return;
            }

            // Pemain menghalangi di depan: berhenti sebentar.
            Vector2 dirN = delta.normalized;
            Vector2 toPlayer = player - pos;
            if (toPlayer.sqrMagnitude < 0.9f && Vector2.Dot(toPlayer, dirN) > 0.1f)
            {
                ApplyWalker(w, delta, false);
                return;
            }

            Vector2 next = Vector2.MoveTowards(pos, goal, w.Speed * dt);
            w.T.position = new Vector3(next.x, next.y, 0f);
            w.Anim += dt;
            ApplyWalker(w, delta, true);
        }

        static readonly int[] WalkCycle = { 0, 1, 0, 2 };
        static readonly string[] DirNames = { "down", "up", "left", "right" };

        void ApplyWalker(Walker w, Vector2 heading, bool moving)
        {
            // Di luar layar: matikan gambar, lewati ganti sprite & urutan gambar.
            bool visible = LifeView.Sees(w.T.position, 1.5f, 2.4f);
            if (w.Body.enabled != visible) { w.Body.enabled = visible; w.Shadow.enabled = visible; }
            if (!visible) return;

            int d;
            if (Mathf.Abs(heading.x) >= Mathf.Abs(heading.y)) d = heading.x < 0f ? 2 : 3;
            else d = heading.y < 0f ? 0 : 1;
            int frame = moving ? WalkCycle[(int)(w.Anim * 7f) % WalkCycle.Length] : 0;
            w.Body.sprite = PixelArt.Get("lc_" + w.Id + "_" + DirNames[d] + "_" + frame);
            int order = -Mathf.RoundToInt(w.T.position.y * PixelArt.PPU);
            w.Body.sortingOrder = order;
            w.Shadow.sortingOrder = order - 1;
        }
    }
}
