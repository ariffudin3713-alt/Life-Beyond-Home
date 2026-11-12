using System.Collections;
using UnityEngine;

namespace PixelAdventure
{
    /// <summary>Pixel hero controller: acceleration, coyote time, jump buffer, variable jump height.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Player : MonoBehaviour
    {
        public const int MaxHealth = 3;

        public int Health = MaxHealth;
        public bool IsAlive = true;
        public int Facing = 1;
        public Vector3 RespawnPoint;

        public Vector2 Velocity => _rb != null ? _rb.linearVelocity : Vector2.zero;
        public bool Frozen => _frozen;

        Rigidbody2D _rb;
        BoxCollider2D _collider;
        Transform _visual;
        SpriteRenderer _sr;

        Sprite _idle, _run1, _run2, _jump;

        float _moveInput;
        float _coyote;
        float _jumpBuffer;
        bool _jumpCut;
        bool _grounded;
        bool _wasGrounded;
        float _fallSpeed;
        float _runTime;
        float _squash;
        float _invuln;
        float _hurtTint;
        bool _frozen;
        bool _respawning;

        public float MoveSpeed = 6.8f;
        public float JumpVelocity = 15.2f;

        void Awake()
        {
            gameObject.layer = GameLayers.Player;

            _rb = gameObject.GetComponent<Rigidbody2D>();
            _rb.gravityScale = 4.2f;
            _rb.freezeRotation = true;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _rb.sleepMode = RigidbodySleepMode2D.NeverSleep;

            _collider = gameObject.AddComponent<BoxCollider2D>();
            _collider.size = new Vector2(0.58f, 0.86f);
            _collider.offset = new Vector2(0f, 0.43f);

            var visualGo = new GameObject("Visual");
            _visual = visualGo.transform;
            _visual.SetParent(transform, false);
            _sr = visualGo.AddComponent<SpriteRenderer>();
            _sr.sharedMaterial = PixelArt.SpriteMaterial;
            _sr.sortingOrder = 10;

            _idle = PixelArt.Get("hero_idle");
            _run1 = PixelArt.Get("hero_run1");
            _run2 = PixelArt.Get("hero_run2");
            _jump = PixelArt.Get("hero_jump");
            _sr.sprite = _idle;

            RespawnPoint = transform.position;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            bool canControl = IsAlive && !_frozen && GameManager.I != null &&
                              GameManager.I.State == GameState.Playing;

            if (canControl)
            {
                _moveInput = GameInput.MoveX;
                if (GameInput.JumpPressed) _jumpBuffer = 0.13f;

                if (!GameInput.JumpHeld && !_jumpCut && _rb.linearVelocity.y > 2.5f)
                {
                    _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.y * 0.52f);
                    _jumpCut = true;
                }
                if (Mathf.Abs(_moveInput) > 0.01f) Facing = _moveInput > 0f ? 1 : -1;
            }
            else _moveInput = 0f;

            _jumpBuffer -= dt;
            if (_invuln > 0f) _invuln -= dt;
            if (_hurtTint > 0f) _hurtTint -= dt;
            _squash = Mathf.MoveTowards(_squash, 0f, dt * 3.2f);

            UpdateVisual(dt);
        }

        void FixedUpdate()
        {
            if (_rb == null) return;
            float dt = Time.fixedDeltaTime;
            Vector2 velocity = _rb.linearVelocity;
            _fallSpeed = velocity.y;

            bool groundedNow = IsAlive && !_frozen && velocity.y <= 0.6f &&
                               Physics2D.OverlapBox((Vector2)transform.position + new Vector2(0f, -0.03f),
                                   new Vector2(0.52f, 0.12f), 0f, GameLayers.GroundMask) != null;

            if (groundedNow && !_grounded)
            {
                Land();
            }
            _grounded = groundedNow;
            _wasGrounded = groundedNow;

            if (_grounded)
            {
                _coyote = 0.1f;
                _jumpCut = false;
            }
            else
            {
                _coyote -= dt;
            }

            if (!IsAlive || _frozen)
            {
                _rb.linearVelocity = new Vector2(Mathf.MoveTowards(velocity.x, 0f, 60f * dt), 0f);
                return;
            }

            _rb.gravityScale = velocity.y > 0.1f ? 3.7f : 4.7f;

            float target = _moveInput * MoveSpeed;
            float acceleration = _grounded
                ? (Mathf.Abs(_moveInput) > 0.01f ? 105f : 135f)
                : (Mathf.Abs(_moveInput) > 0.01f ? 58f : 22f);
            float vx = Mathf.MoveTowards(velocity.x, target, acceleration * dt);
            _rb.linearVelocity = new Vector2(vx, velocity.y);

            if (_jumpBuffer > 0f && _coyote > 0f)
            {
                DoJump();
            }
        }

        void DoJump()
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, JumpVelocity);
            _grounded = false;
            _coyote = 0f;
            _jumpBuffer = 0f;
            _jumpCut = false;
            _squash = -0.18f;
            GameAudio.Play("jump", 0.8f);
            FxManager.Dust(transform.position + new Vector3(0f, 0.08f, 0f),
                new Color(0.95f, 0.93f, 0.82f, 0.9f), 5, Mathf.PI * 0.5f);
        }

        void Land()
        {
            if (_fallSpeed < -7f)
            {
                _squash = 0.24f;
                GameAudio.Play("land", 0.55f);
                FxManager.Dust(transform.position + new Vector3(0f, 0.06f, 0f),
                    new Color(0.92f, 0.9f, 0.78f, 0.9f), 7, Mathf.PI * 0.5f);
                GameManager.I?.CameraRig?.Shake(0.045f, 0.12f);
            }
            else if (_fallSpeed < -3f)
            {
                _squash = 0.12f;
            }
        }

        void UpdateVisual(float dt)
        {
            if (!IsAlive)
            {
                if (_visual != null) _visual.gameObject.SetActive(false);
                return;
            }

            if (!_visual.gameObject.activeSelf) _visual.gameObject.SetActive(true);

            Vector2 v = _rb != null ? _rb.linearVelocity : Vector2.zero;
            Sprite frame;
            if (!_grounded)
            {
                frame = _jump;
            }
            else if (Mathf.Abs(v.x) > 0.6f && Mathf.Abs(_moveInput) > 0.01f)
            {
                _runTime += dt * Mathf.Lerp(8f, 14f, Mathf.Abs(v.x) / MoveSpeed);
                frame = ((int)_runTime & 1) == 0 ? _run1 : _run2;
            }
            else
            {
                frame = _idle;
            }

            _sr.sprite = frame;
            _sr.flipX = Facing < 0;
            _visual.localPosition = new Vector3(0f,
                _grounded && Mathf.Abs(v.x) < 0.3f ? Mathf.Sin(Time.time * 2.6f) * 0.015f : 0f, 0f);
            _visual.localScale = new Vector3(1f + _squash * 0.35f, 1f - _squash * 0.45f, 1f);

            Color tint = _hurtTint > 0f
                ? Color.Lerp(Color.white, new Color(1f, 0.38f, 0.38f), Mathf.Clamp01(_hurtTint / 0.25f))
                : Color.white;
            _sr.color = tint;

            _sr.enabled = _invuln <= 0f || (Mathf.FloorToInt(_invuln * 18f) & 1) == 0;
        }

        // ---------------------------------------------------------------- interactions

        void OnTriggerEnter2D(Collider2D other) { HandleTrigger(other); }
        void OnTriggerStay2D(Collider2D other) { HandleTrigger(other); }

        void HandleTrigger(Collider2D other)
        {
            if (!IsAlive || _frozen || _respawning) return;
            if (GameManager.I == null || GameManager.I.State != GameState.Playing) return;

            EnemyBase enemy = other.GetComponentInParent<EnemyBase>();
            if (enemy != null)
            {
                bool fromAbove = _rb.linearVelocity.y < -0.5f &&
                                 transform.position.y >= other.bounds.max.y - 0.35f;
                if (fromAbove && enemy.CanStomp)
                {
                    if (enemy.Stomp())
                    {
                        Bounce(11.8f);
                        GameAudio.Play("stomp", 0.9f);
                        FxManager.Burst(enemy.transform.position + new Vector3(0f, 0.35f, 0f),
                            new Color(0.55f, 0.95f, 0.5f), 10, 2f, 5f, 0.4f, 0.12f);
                        GameManager.I.CameraRig?.Shake(0.08f, 0.12f);
                    }
                }
                else TakeHit(enemy.transform.position);
                return;
            }

            Hazard hazard = other.GetComponentInParent<Hazard>();
            if (hazard != null) TakeHit(hazard.transform.position);
        }

        public void TakeHit(Vector2 source)
        {
            if (!IsAlive || _frozen || _invuln > 0f || _respawning) return;
            if (GameManager.I == null || GameManager.I.State != GameState.Playing) return;

            Health--;
            GameManager.I.OnPlayerHealthChanged(Health);
            _invuln = 1.35f;
            _hurtTint = 0.25f;
            GameAudio.Play("hurt", 0.9f);
            GameManager.I.CameraRig?.Shake(0.28f, 0.25f);
            FxManager.Burst(transform.position + new Vector3(0f, 0.55f, 0f),
                new Color(1f, 0.35f, 0.35f), 12, 2.5f, 6.5f, 0.45f, 0.12f);

            Vector2 dir = (Vector2)transform.position - source;
            if (dir.sqrMagnitude < 0.01f) dir = new Vector2(-Facing, 0.5f);
            dir.y = 0f;
            dir = dir.normalized;
            if (Mathf.Abs(dir.x) < 0.1f) dir.x = -Facing;
            _rb.linearVelocity = new Vector2(dir.x * 7.5f, 9.2f);
            _grounded = false;
            _jumpCut = true;

            if (Health <= 0) Die();
        }

        public void Fell()
        {
            if (!IsAlive || _frozen || _respawning) return;
            if (GameManager.I == null || GameManager.I.State != GameState.Playing) return;

            Health--;
            GameManager.I.OnPlayerHealthChanged(Health);
            GameAudio.Play("hurt", 0.9f);
            GameManager.I.CameraRig?.Shake(0.2f, 0.2f);
            if (Health <= 0) { Die(); return; }
            StartCoroutine(RespawnRoutine());
        }

        IEnumerator RespawnRoutine()
        {
            _respawning = true;
            SetFrozen(true);
            _visual.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.55f);

            Teleport(RespawnPoint);
            SetFrozen(false);
            _invuln = 1.6f;
            _sr.enabled = true;
            _visual.gameObject.SetActive(true);
            _respawning = false;
            GameManager.I?.CameraRig?.Snap();
            GameAudio.Play("checkpoint", 0.5f);
            FxManager.Burst(RespawnPoint + new Vector3(0f, 0.6f, 0f),
                GameManager.I != null ? GameManager.I.CurrentTheme.Accent : Color.white,
                12, 1.5f, 4f, 0.5f, 0.1f, 6f);
        }

        public void Die()
        {
            if (!IsAlive) return;
            IsAlive = false;
            _rb.linearVelocity = Vector2.zero;
            _rb.gravityScale = 0f;
            _collider.enabled = false;
            _visual.gameObject.SetActive(false);
            FxManager.Burst(transform.position + new Vector3(0f, 0.55f, 0f),
                new Color(1f, 0.4f, 0.4f), 22, 2.5f, 8f, 0.85f, 0.14f, 9f);
            FxManager.Burst(transform.position + new Vector3(0f, 0.55f, 0f),
                new Color(0.4f, 0.7f, 1f), 14, 2f, 7f, 0.8f, 0.12f, 7f);
            GameManager.I?.CameraRig?.Shake(0.45f, 0.4f);
            GameManager.I?.OnPlayerDied();
        }

        public void Bounce(float velocityY)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, velocityY);
            _grounded = false;
            _jumpCut = true;
            _squash = -0.15f;
        }

        public void Launch(float velocityY)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x * 0.7f, velocityY);
            _grounded = false;
            _jumpCut = true;
            _coyote = 0f;
            _squash = -0.25f;
            FxManager.Dust(transform.position + new Vector3(0f, 0.1f, 0f),
                new Color(1f, 0.8f, 0.5f), 9, Mathf.PI * 0.5f);
        }

        public void Teleport(Vector3 position)
        {
            transform.position = position;
            if (_rb != null)
            {
                _rb.position = position;
                _rb.linearVelocity = Vector2.zero;
            }
            _grounded = false;
            _wasGrounded = false;
        }

        public void SetFrozen(bool frozen)
        {
            _frozen = frozen;
            if (_rb == null) return;
            if (frozen)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.gravityScale = 0f;
            }
            else if (IsAlive)
            {
                _rb.gravityScale = 4.2f;
            }
        }

        public void Heal(int amount)
        {
            Health = Mathf.Clamp(Health + amount, 0, MaxHealth);
            GameManager.I?.OnPlayerHealthChanged(Health);
        }
    }
}
