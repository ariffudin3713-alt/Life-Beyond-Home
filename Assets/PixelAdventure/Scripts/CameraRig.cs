using UnityEngine;

namespace PixelAdventure
{
    /// <summary>Smooth follow camera with look-ahead, level clamping, pixel snapping and screen shake.</summary>
    public class CameraRig : MonoBehaviour
    {
        public Camera Cam;
        public Player Player;
        public Transform Target;
        public float OrthoSize = 7.5f;
        public float LookAhead = 1.25f;

        float _levelWidth = 64f;
        float _levelHeight = 18f;
        float _shakeTime;
        float _shakeDuration;
        float _shakeStrength;
        Vector2 _shakeOffset;
        Vector3 _current;

        public void Bind(Camera cam, Player player, Transform target, int width, int height)
        {
            Cam = cam;
            Player = player;
            Target = target;
            _levelWidth = width;
            _levelHeight = height;
            cam.orthographic = true;
            cam.orthographicSize = OrthoSize;
            var p = cam.transform.position;
            p.z = -10f;
            cam.transform.position = p;
            Snap();
        }

        public void Shake(float strength, float duration = 0.25f)
        {
            _shakeStrength = Mathf.Max(_shakeStrength, strength);
            _shakeDuration = Mathf.Max(_shakeDuration, duration);
            _shakeTime = _shakeDuration;
        }

        public void Snap()
        {
            if (Target == null) return;
            Vector3 desired = DesiredPosition(true);
            _current = desired;
            Apply(desired, Vector2.zero);
        }

        Vector3 DesiredPosition(bool instant)
        {
            if (Target == null) return transform.position;
            float halfH = Cam != null ? Cam.orthographicSize : OrthoSize;
            float halfW = Cam != null ? halfH * Cam.aspect : 13f;

            float minX = halfW;
            float maxX = _levelWidth - halfW;
            if (minX > maxX) { float c = _levelWidth * 0.5f; minX = maxX = c; }

            float minY = halfH;
            float maxY = _levelHeight - halfH;
            if (minY > maxY) { float c = _levelHeight * 0.5f; minY = maxY = c; }

            float look = Player != null ? Player.Facing * LookAhead : 0f;
            float x = Mathf.Clamp(Target.position.x + look, minX, maxX);
            float y = Mathf.Clamp(Target.position.y + 1.35f, minY, maxY);
            return new Vector3(x, y, -10f);
        }

        void LateUpdate()
        {
            if (Target == null) return;
            Vector3 desired = DesiredPosition(false);
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);

            if (_current == Vector3.zero) _current = desired;

            float tx = 1f - Mathf.Exp(-7f * dt);
            float ty = 1f - Mathf.Exp(-5f * dt);
            _current.x = Mathf.Lerp(_current.x, desired.x, tx);
            _current.y = Mathf.Lerp(_current.y, desired.y, ty);
            _current.z = -10f;

            if (_shakeTime > 0f)
            {
                _shakeTime -= dt;
                float t = Mathf.Clamp01(_shakeTime / Mathf.Max(0.001f, _shakeDuration));
                _shakeOffset = Random.insideUnitCircle * (_shakeStrength * t * t);
                if (_shakeTime <= 0f) { _shakeStrength = 0f; _shakeDuration = 0f; _shakeOffset = Vector2.zero; }
            }
            else _shakeOffset = Vector2.zero;

            Apply(_current, _shakeOffset);
        }

        void Apply(Vector3 basePos, Vector2 shake)
        {
            float x = basePos.x + shake.x;
            float y = basePos.y + shake.y;
            if (shake.sqrMagnitude < 0.000001f)
            {
                x = Mathf.Round(x * PixelArt.PPU) / PixelArt.PPU;
                y = Mathf.Round(y * PixelArt.PPU) / PixelArt.PPU;
            }
            transform.position = new Vector3(x, y, -10f);
        }
    }

    /// <summary>Moves a background layer at a fraction of the camera's horizontal movement.</summary>
    public class Parallax : MonoBehaviour
    {
        public Transform CameraTransform;
        public float Factor = 0.35f;
        Vector3 _basePosition;
        float _startCameraX;

        public void Init(Transform cameraTransform, float factor)
        {
            CameraTransform = cameraTransform;
            Factor = factor;
            _basePosition = transform.position;
            _startCameraX = cameraTransform != null ? cameraTransform.position.x : 0f;
        }

        void LateUpdate()
        {
            if (CameraTransform == null) return;
            float delta = CameraTransform.position.x - _startCameraX;
            Vector3 p = _basePosition;
            p.x = _basePosition.x + delta * Factor;
            transform.position = p;
        }
    }
}
