using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>Top-down player with four-direction walk animation and tile collision.</summary>
    public class LifePlayer : MonoBehaviour
    {
        public float Speed = 4.6f;

        LifeWorld _world;
        SpriteRenderer _body;
        SpriteRenderer _shadow;
        SpriteRenderer _vehicleRenderer;

        int _dir;
        int _frame;
        float _anim;

        static readonly string[] DirNames = { "down", "up", "left", "right" };

        public Vector2 Feet => transform.position;
        public int Direction => _dir;

        public void SetSpeed(float speed)
        {
            Speed = Mathf.Max(1f, speed);
        }

        public void SetTint(Color color)
        {
            if (_body != null) _body.color = color;
        }

        string _vehicle;   // id kendaraan yang dinaiki (null = jalan kaki)

        /// <summary>Sedang menaiki kendaraan pribadi.</summary>
        public bool Riding => !string.IsNullOrEmpty(_vehicle);

        /// <summary>
        /// Tampilkan kendaraan pribadi yang dinaiki ("sepeda", "skuter", "motor", "mobil"),
        /// atau null/"" untuk berjalan kaki. Sepeda/motor digambar di atas kaki pemain;
        /// mobil menggantikan sosok pemain (sopir terlihat di balik kaca).
        /// </summary>
        public void SetVehicleVisual(string vehicle)
        {
            if (_body == null) return;
            _vehicle = string.IsNullOrEmpty(vehicle) ? null : vehicle;
            if (_vehicle == null)
            {
                if (_vehicleRenderer != null) _vehicleRenderer.gameObject.SetActive(false);
                _body.enabled = true;
                _shadow.transform.localScale = Vector3.one;
                ApplySprite();
                return;
            }

            if (_vehicleRenderer == null)
            {
                var go = new GameObject("PersonalVehicle");
                go.transform.SetParent(transform, false);
                _vehicleRenderer = go.AddComponent<SpriteRenderer>();
                _vehicleRenderer.sharedMaterial = PixelArt.SpriteMaterial;
            }
            _vehicleRenderer.gameObject.SetActive(true);
            bool car = _vehicle == "mobil";
            _body.enabled = !car;
            _shadow.transform.localScale = car ? new Vector3(2.2f, 1.6f, 1f) : new Vector3(1.4f, 1f, 1f);
            ApplySprite();
        }

        public static LifePlayer Create(LifeWorld world, float x, float y)
        {
            var go = new GameObject("Player");
            var player = go.AddComponent<LifePlayer>();
            player._world = world;
            go.transform.position = new Vector3(x, y, 0f);

            player._body = go.AddComponent<SpriteRenderer>();
            player._body.sharedMaterial = PixelArt.SpriteMaterial;
            player._body.sprite = PixelArt.Get("lc_player_down_0");
            player._body.sortingOrder = 0;

            var shadowGo = new GameObject("Shadow");
            shadowGo.transform.SetParent(go.transform, false);
            shadowGo.transform.localPosition = new Vector3(0f, 0.06f, 0f);
            player._shadow = shadowGo.AddComponent<SpriteRenderer>();
            player._shadow.sprite = PixelArt.Get("lc_shadow");
            player._shadow.sharedMaterial = PixelArt.SpriteMaterial;
            player._shadow.sortingOrder = -1;

            return player;
        }

        public void Teleport(float x, float y)
        {
            ClearPath();
            Sit = SitPhase.None;   // pindah tempat (pintu, bus, tidur) = otomatis berdiri
            transform.position = new Vector3(x, y, 0f);
            _anim = 0f;
            _frame = 0;
            ApplySprite();
        }

        /// <summary>Kecepatan lari = kecepatan jalan x RunMultiplier.</summary>
        public const float RunMultiplier = 1.65f;

        /// <summary>Frame terakhir pemain benar-benar berlari (bergerak sambil menahan SHIFT).</summary>
        public bool IsRunning { get; private set; }

        // ---- jalur kontrol ketuk / klik
        readonly System.Collections.Generic.List<Vector2> _path = new System.Collections.Generic.List<Vector2>();
        int _pathIndex;
        bool _pathRun;
        float _stuckTime;
        Vector2 _lastPos;

        /// <summary>Sedang berjalan mengikuti jalur hasil ketukan.</summary>
        public bool HasPath => _pathIndex < _path.Count;

        /// <summary>True satu frame ketika jalur ketukan selesai sampai tujuan.</summary>
        public bool PathArrived { get; private set; }

        /// <summary>Tujuan akhir jalur (untuk penanda).</summary>
        public Vector2 PathGoal => _path.Count > 0 ? _path[_path.Count - 1] : Feet;

        public void SetPath(System.Collections.Generic.List<Vector2> path, bool run)
        {
            _path.Clear();
            if (path != null) _path.AddRange(path);
            _pathIndex = 0;
            _pathRun = run;
            _stuckTime = 0f;
            _lastPos = Feet;
        }

        public void ClearPath()
        {
            _path.Clear();
            _pathIndex = 0;
        }

        // ------------------------------------------------------------------ duduk

        public enum SitPhase { None, Sitting, Seated, Standing }

        /// <summary>Fase duduk. Selain None, input jalan/lari diabaikan.</summary>
        public SitPhase Sit { get; private set; }

        /// <summary>Sedang duduk atau sedang transisi duduk/berdiri (gerak jalan terkunci).</summary>
        public bool SitBusy => Sit != SitPhase.None;

        /// <summary>Sudah duduk penuh (boleh berdiri lagi).</summary>
        public bool IsSeated => Sit == SitPhase.Seated;

        /// <summary>Posisi berdiri yang aman: titik depan bangku saat duduk, selain itu posisi kaki.</summary>
        public Vector2 StandSpot => SitBusy ? _sitExit : Feet;

        const float SitTweenTime = 0.22f;   // detik, transisi duduk / berdiri
        const float SitHop = 0.18f;         // lompatan kecil saat naik / turun bangku
        const int SitCropPx = 5;            // baris kaki yang disembunyikan pada pose duduk

        Vector2 _sitFrom, _sitTo, _sitExit;
        float _sitT;
        int _sitOrder;
        Sprite _sitSprite, _sitSource;

        /// <summary>
        /// Mulai duduk: pemain berbalik menghadap kamera lalu bergeser mulus ke <paramref name="seat"/>.
        /// <paramref name="exit"/> = titik aman untuk berdiri nanti. False bila sedang berkendara / sudah duduk.
        /// </summary>
        public bool SitDown(Vector2 seat, Vector2 exit, int benchOrder)
        {
            if (SitBusy || Riding) return false;
            ClearPath();
            IsRunning = false;
            _sitFrom = Feet;
            _sitTo = seat;
            _sitExit = exit;
            _sitOrder = benchOrder + 1;
            _sitT = 0f;
            _dir = 0;
            _anim = 0f;
            _frame = 0;
            Sit = SitPhase.Sitting;
            ApplySprite();
            return true;
        }

        /// <summary>Berdiri lagi ke titik depan bangku. Hanya dari posisi duduk penuh.</summary>
        public void StandUp()
        {
            if (Sit != SitPhase.Seated) return;
            _sitFrom = Feet;
            _sitTo = _sitExit;
            _sitT = 0f;
            Sit = SitPhase.Standing;
        }

        void TickSit(float dt)
        {
            IsRunning = false;
            if (Sit == SitPhase.Sitting || Sit == SitPhase.Standing)
            {
                _sitT = Mathf.Min(1f, _sitT + dt / SitTweenTime);
                float k = Mathf.SmoothStep(0f, 1f, _sitT);
                Vector2 p = Vector2.Lerp(_sitFrom, _sitTo, k);
                transform.position = new Vector3(p.x, p.y + Mathf.Sin(k * Mathf.PI) * SitHop, 0f);
                if (_sitT >= 1f)
                {
                    transform.position = new Vector3(_sitTo.x, _sitTo.y, 0f);
                    Sit = Sit == SitPhase.Sitting ? SitPhase.Seated : SitPhase.None;
                }
            }
            ApplySprite();
        }

        /// <summary>Pose duduk sedang tampil (separuh akhir transisi duduk / separuh awal berdiri).</summary>
        bool SeatedPose =>
            Sit == SitPhase.Seated ||
            (Sit == SitPhase.Sitting && _sitT >= 0.5f) ||
            (Sit == SitPhase.Standing && _sitT < 0.5f);

        /// <summary>
        /// Pose duduk = sprite menghadap depan dengan baris kaki dipotong (paha tertutup dudukan).
        /// Dibuat dari sprite "lc_player_down_0" yang sedang aktif, jadi otomatis ikut pakaian;
        /// tidak membuat tekstur baru, hanya Sprite yang menunjuk tekstur yang sama.
        /// </summary>
        Sprite SeatedSprite()
        {
            Sprite src = PixelArt.Get("lc_player_down_0");
            if (src == null) return null;
            if (src == _sitSource && _sitSprite != null) return _sitSprite;
            if (_sitSprite != null) Destroy(_sitSprite);
            _sitSource = src;
            Rect r = src.textureRect;
            var crop = new Rect(r.x, r.y + SitCropPx, r.width, r.height - SitCropPx);
            _sitSprite = Sprite.Create(src.texture, crop, new Vector2(0.5f, 0f), src.pixelsPerUnit, 0, SpriteMeshType.FullRect);
            _sitSprite.name = "lc_player_sit";
            return _sitSprite;
        }

        void OnDestroy()
        {
            if (_sitSprite != null) Destroy(_sitSprite);
            ClearRideSprites();
        }

        // ------------------------------------------------------------------ duduk di kendaraan

        const int RideLegRows = 6;   // baris kaki pada sprite pemain (dipotong; kaki digambar ulang)

        SpriteRenderer _limbRenderer;   // kaki & tangan pengendara (di depan kendaraan)
        Sprite _rideSource;
        readonly System.Collections.Generic.Dictionary<int, Sprite> _rideSprites =
            new System.Collections.Generic.Dictionary<int, Sprite>();

        Sprite _colorSource;
        Color32 _pants = new Color32(58, 74, 106, 255), _shoes = new Color32(43, 34, 51, 255),
                _shirt = new Color32(90, 143, 214, 255);

        void ClearRideSprites()
        {
            foreach (var s in _rideSprites.Values) if (s != null) Destroy(s);
            _rideSprites.Clear();
        }

        static int RideView(int dir) => dir == 0 ? 0 : dir == 1 ? 1 : 2;

        /// <summary>
        /// Badan pengendara: sprite arah yang sama, kaki (6 baris bawah) dipotong, dan pivot digeser
        /// sehingga pinggul tepat di jok (naik <c>Hip</c> piksel; tampak samping digeser ke jok).
        /// Tidak membuat tekstur baru, jadi otomatis ikut pakaian yang dipakai.
        /// </summary>
        Sprite RiderSprite(Sprite src, int bob)
        {
            if (src == null || !LifeArt.RideSeat(_vehicle, RideView(_dir), out var seat)) return src;
            int shift = _dir == 2 ? -seat.Shift : seat.Shift;   // menghadap kiri: kendaraan dicerminkan
            int hip = seat.Hip + bob;
            if (src != _rideSource) { ClearRideSprites(); _rideSource = src; }
            int key = hip * 1000 + shift + 500;
            if (_rideSprites.TryGetValue(key, out Sprite s) && s != null) return s;

            Rect r = src.textureRect;
            var crop = new Rect(r.x, r.y + RideLegRows, r.width, r.height - RideLegRows);
            var pivot = new Vector2(0.5f - shift / crop.width, -hip / crop.height);
            s = Sprite.Create(src.texture, crop, pivot, src.pixelsPerUnit, 0, SpriteMeshType.FullRect);
            s.name = "lc_player_ride";
            _rideSprites[key] = s;
            return s;
        }

        /// <summary>Warna celana, sepatu, dan baju dari sprite pemain tampak depan (ikut pakaian).</summary>
        void SampleOutfitColors()
        {
            Sprite src = PixelArt.Get("lc_player_down_0");
            if (src == null || src == _colorSource) return;
            _colorSource = src;
            try
            {
                var tex = src.texture;
                Rect r = src.textureRect;
                Color32[] px = tex.GetPixels32();
                Color32 Pick(int y0, int y1, Color32 fallback)
                {
                    var count = new System.Collections.Generic.Dictionary<uint, int>();
                    Color32 best = fallback; int bestN = 0;
                    for (int y = y0; y <= y1; y++)
                        for (int x = 0; x < (int)r.width; x++)
                        {
                            Color32 c = px[((int)r.y + y) * tex.width + (int)r.x + x];
                            if (c.a < 200) continue;
                            if (c.r + c.g + c.b < 90) continue;   // garis tepi gelap
                            uint k = ((uint)c.r << 16) | ((uint)c.g << 8) | c.b;
                            count.TryGetValue(k, out int n);
                            count[k] = ++n;
                            if (n > bestN) { bestN = n; best = c; }
                        }
                    best.a = 255;
                    return best;
                }
                _shoes = Pick(0, 1, _shoes);
                _pants = Pick(2, RideLegRows - 1, _pants);
                _shirt = Pick(RideLegRows + 1, RideLegRows + 5, _shirt);
            }
            catch (UnityException) { /* tekstur tidak bisa dibaca: pakai warna bawaan */ }
        }

        /// <summary>Kaki & tangan pengendara, di depan kendaraan dan ikut bergoyang bersamanya.</summary>
        void ApplyRiderLimbs(int order, float bob, int phase)
        {
            bool show = Riding && !SitBusy && LifeArt.RideSeat(_vehicle, RideView(_dir), out _);
            if (!show)
            {
                if (_limbRenderer != null) _limbRenderer.enabled = false;
                return;
            }
            if (_limbRenderer == null)
            {
                var go = new GameObject("RiderLimbs");
                go.transform.SetParent(transform, false);
                _limbRenderer = go.AddComponent<SpriteRenderer>();
                _limbRenderer.sharedMaterial = PixelArt.SpriteMaterial;
            }
            SampleOutfitColors();
            _limbRenderer.enabled = true;
            _limbRenderer.sprite = LifeArt.RiderLimbs(_vehicle, RideView(_dir), phase, _pants, _shoes, _shirt);
            _limbRenderer.flipX = _dir == 2;
            _limbRenderer.sortingOrder = order + 2;
            _limbRenderer.color = _body.color;
            _limbRenderer.transform.localPosition = new Vector3(0f, bob, 0f);
        }

        // ------------------------------------------------------------------ jalan

        /// <summary>Jarak maksimal geser samping otomatis saat menabrak sudut (kusen pintu, ujung pagar).</summary>
        const float CornerNudge = 0.4f;

        /// <summary>
        /// "Corner assist": bila gerak lurus terhalang tetapi ada celah dalam jarak <see cref="CornerNudge"/>
        /// ke samping, geser pemain ke arah celah itu. Mencegah tersangkut di kusen pintu / sudut bangunan
        /// ketika posisi pemain meleset sedikit dari lorong.
        /// </summary>
        Vector2 CornerAssist(Vector2 pos, Vector2 delta, float step)
        {
            bool vertical = Mathf.Abs(delta.y) >= Mathf.Abs(delta.x);
            float mainLen = vertical ? Mathf.Abs(delta.y) : Mathf.Abs(delta.x);
            float sideLen = vertical ? Mathf.Abs(delta.x) : Mathf.Abs(delta.y);
            if (mainLen < 0.0001f || sideLen > mainLen * 0.35f) return pos;   // gerak diagonal: geser sumbu biasa sudah cukup

            Vector2 main = vertical ? new Vector2(0f, delta.y) : new Vector2(delta.x, 0f);
            Vector2 side = vertical ? Vector2.right : Vector2.up;
            for (float s = 0.05f; s <= CornerNudge + 0.001f; s += 0.05f)
                for (int sign = -1; sign <= 1; sign += 2)
                {
                    Vector2 probe = pos + side * (s * sign);
                    if (!_world.CanStand(probe.x, probe.y)) continue;
                    Vector2 ahead = probe + main;
                    if (!_world.CanStand(ahead.x, ahead.y)) continue;
                    Vector2 next = pos + side * (Mathf.Min(step, s) * sign);
                    if (_world.CanStand(next.x, next.y)) return next;
                }
            return pos;
        }

        public void Tick(float dt) => Tick(dt, false);

        /// <param name="allowRun">Boleh berlari (mis. energi masih cukup).</param>
        public void Tick(float dt, bool allowRun)
        {
            if (dt <= 0f) return;
            PathArrived = false;
            if (SitBusy) { TickSit(dt); return; }   // duduk: abaikan input jalan / lari
            float ix = LifeInput.X;
            float iy = LifeInput.Y;
            var input = new Vector2(ix, iy);
            if (input.sqrMagnitude > 1f) input.Normalize();
            bool keyboard = input.sqrMagnitude > 0.01f;
            bool tapRun = false;

            if (keyboard) ClearPath();                       // tombol arah membatalkan jalur ketukan
            else if (HasPath)
            {
                Vector2 pos0 = Feet;
                Vector2 to = _path[_pathIndex] - pos0;
                if (to.magnitude < 0.12f)
                {
                    _pathIndex++;
                    if (!HasPath) { PathArrived = true; ClearPath(); }
                }
                if (HasPath)
                {
                    to = _path[_pathIndex] - pos0;
                    input = to.normalized;
                    tapRun = _pathRun;
                    // Tersangkut (tidak bergerak) terlalu lama: batalkan.
                    if ((pos0 - _lastPos).sqrMagnitude < 0.0004f) _stuckTime += dt; else _stuckTime = 0f;
                    _lastPos = pos0;
                    if (_stuckTime > 0.6f) { ClearPath(); input = Vector2.zero; }
                }
                else input = Vector2.zero;
            }

            bool moving = input.sqrMagnitude > 0.01f;
            IsRunning = moving && allowRun && (LifeInput.RunHeld || tapRun);
            float speed = Speed * (IsRunning ? RunMultiplier : 1f);

            if (moving)
            {
                if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
                    _dir = input.x < 0f ? 2 : 3;
                else
                    _dir = input.y < 0f ? 0 : 1;

                Vector2 pos = transform.position;
                Vector2 delta = input * (speed * dt);
                // Jangan melewati titik jalur (hindari bolak-balik di sekitar tujuan).
                if (HasPath)
                {
                    float remain = Vector2.Distance(pos, _path[_pathIndex]);
                    if (delta.magnitude > remain) delta = delta.normalized * remain;
                }
                if (_world != null && !_world.CanStand(pos.x, pos.y))
                {
                    // Jaring pengaman: kaki sudah berada di dalam penghalang (muncul di posisi
                    // terhalang, penghalang baru, dsb.) sehingga semua arah ditolak = macet total.
                    // Pindahkan ke titik bebas terdekat.
                    Vector2? free = _world.NearestStandable(pos);
                    if (free.HasValue) pos = free.Value;
                }
                Vector2 before = pos;
                if (_world == null || _world.CanStand(pos.x + delta.x, pos.y))
                    pos.x += delta.x;
                if (_world == null || _world.CanStand(pos.x, pos.y + delta.y))
                    pos.y += delta.y;
                if (_world != null && pos == before)
                    pos = CornerAssist(pos, delta, speed * dt);
                transform.position = new Vector3(pos.x, pos.y, 0f);
            }

            if (moving)
            {
                _anim += dt * (IsRunning ? 14f : 9f);   // langkah lebih cepat saat berlari
                int step = (int)_anim % 4;
                int prevFrame = _frame;
                _frame = step == 0 ? 0 : step == 1 ? 1 : step == 2 ? 0 : 2;
                // Suara langkah setiap kaki menapak (frame 1 & 2), nada sedikit acak agar alami.
                if (_frame != prevFrame && _frame != 0 && !Riding)
                    LifeStepFx.Step(_world, Feet, IsRunning, _dir);   // suara sesuai permukaan + debu
            }
            else
            {
                _anim = 0f;
                _frame = 0;
            }

            ApplySprite();
        }

        void ApplySprite()
        {
            // Di atas kendaraan kaki tidak melangkah: pakai pose diam.
            bool seated = SitBusy && SeatedPose;
            // Goyang halus 1 piksel saat melaju: badan, kendaraan, dan kaki/tangan bergerak bersama.
            int bobPx = Riding && _anim > 0f && ((int)(_anim * 1.5f) % 2 == 0) ? 1 : 0;
            Sprite pose = seated ? SeatedSprite() : PixelArt.Get("lc_player_" + DirNames[_dir] + "_" + (Riding ? 0 : _frame));
            if (!seated && Riding) pose = RiderSprite(pose, bobPx);
            _body.sprite = pose;
            int order = -Mathf.RoundToInt(transform.position.y * PixelArt.PPU);
            if (SitBusy) order = Mathf.Max(order, _sitOrder);   // duduk: digambar di depan bangku
            _body.sortingOrder = order;
            _shadow.sortingOrder = order - 1;
            _shadow.enabled = !seated;
            _shadow.transform.localPosition = new Vector3(0f, 0.06f, 0f);
            if (_vehicleRenderer != null && Riding)
            {
                string view = _dir == 0 ? "front" : _dir == 1 ? "back" : "side";
                _vehicleRenderer.sprite = PixelArt.Get("lc_ride_" + _vehicle + "_" + view);
                _vehicleRenderer.flipX = _dir == 2;
                // Kendaraan menutupi pinggul pemain (duduk di jok); kaki & tangan digambar di depannya.
                _vehicleRenderer.sortingOrder = order + 1;
                _vehicleRenderer.transform.localPosition = new Vector3(0f, bobPx / PixelArt.PPU, 0f);
            }
            // Sepeda: kaki bergantian mengayuh saat melaju.
            int phase = _vehicle == "sepeda" && _anim > 0f ? (int)(_anim * 0.6f) % 2 : 0;
            ApplyRiderLimbs(order, bobPx / PixelArt.PPU, phase);
        }
    }
}
