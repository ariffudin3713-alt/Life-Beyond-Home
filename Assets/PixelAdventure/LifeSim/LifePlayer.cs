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

        /// <summary>Displays the player's saved personal vehicle beneath the avatar.</summary>
        public void SetVehicleVisual(int vehicle)
        {
            if (_body == null) return;
            if (vehicle <= 0)
            {
                if (_vehicleRenderer != null) _vehicleRenderer.gameObject.SetActive(false);
                return;
            }

            if (_vehicleRenderer == null)
            {
                var go = new GameObject("PersonalVehicle");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, -0.08f, 0f);
                go.transform.localScale = Vector3.one * 0.42f;
                _vehicleRenderer = go.AddComponent<SpriteRenderer>();
                _vehicleRenderer.sharedMaterial = PixelArt.SpriteMaterial;
            }

            string sprite = vehicle == 1 ? "lc_vehicle_motorcycle"
                : vehicle == 2 ? "lc_vehicle_angkot" : "lc_vehicle_car";
            _vehicleRenderer.sprite = PixelArt.Get(sprite);
            _vehicleRenderer.gameObject.SetActive(true);
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

        public void Tick(float dt) => Tick(dt, false);

        /// <param name="allowRun">Boleh berlari (mis. energi masih cukup).</param>
        public void Tick(float dt, bool allowRun)
        {
            if (dt <= 0f) return;
            PathArrived = false;
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
                if (_world == null || _world.CanStand(pos.x + delta.x, pos.y))
                    pos.x += delta.x;
                if (_world == null || _world.CanStand(pos.x, pos.y + delta.y))
                    pos.y += delta.y;
                transform.position = new Vector3(pos.x, pos.y, 0f);
            }

            if (moving)
            {
                _anim += dt * (IsRunning ? 14f : 9f);   // langkah lebih cepat saat berlari
                int step = (int)_anim % 4;
                _frame = step == 0 ? 0 : step == 1 ? 1 : step == 2 ? 0 : 2;
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
            _body.sprite = PixelArt.Get("lc_player_" + DirNames[_dir] + "_" + _frame);
            int order = -Mathf.RoundToInt(transform.position.y * PixelArt.PPU);
            _body.sortingOrder = order;
            _shadow.sortingOrder = order - 1;
            _shadow.transform.localPosition = new Vector3(0f, 0.06f, 0f);
            if (_vehicleRenderer != null)
            {
                _vehicleRenderer.sortingOrder = order - 2;
                _vehicleRenderer.flipX = _dir == 2;
            }
        }
    }
}
