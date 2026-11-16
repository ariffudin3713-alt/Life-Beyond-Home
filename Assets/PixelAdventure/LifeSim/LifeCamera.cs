using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>Follow camera clamped to the town map, with pixel snapping.</summary>
    public class LifeCamera : MonoBehaviour
    {
        public const float MapWidth = LifeWorld.Width;
        public const float MapHeight = LifeWorld.Height;

        Camera _cam;
        Transform _target;
        Vector3 _current;

        public void Init(Camera cam, Transform target)
        {
            _cam = cam;
            _target = target;
            cam.orthographic = true;
            cam.orthographicSize = LifeGraphics.CameraSize;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.10f, 0.12f, 0.20f);
            Snap();
        }

        public void Snap()
        {
            if (_target == null || _cam == null) return;
            _current = Desired();
            Apply(_current);
        }

        /// <summary>Bila diisi, kamera diam di titik ini (mis. tengah ruangan dalam bangunan).</summary>
        public Vector2? Fixed;

        Vector3 Desired()
        {
            if (Fixed.HasValue) return new Vector3(Fixed.Value.x, Fixed.Value.y, -10f);

            float halfH = _cam != null ? _cam.orthographicSize : 8f;
            float halfW = _cam != null ? halfH * _cam.aspect : 14f;

            float minX = halfW, maxX = MapWidth - halfW;
            if (minX > maxX) { float c = MapWidth * 0.5f; minX = maxX = c; }
            float minY = halfH, maxY = MapHeight - halfH;
            if (minY > maxY) { float c = MapHeight * 0.5f; minY = maxY = c; }

            Vector2 t = _target != null ? (Vector2)_target.position : Vector2.zero;
            float x = Mathf.Clamp(t.x, minX, maxX);
            float y = Mathf.Clamp(t.y + 1.2f, minY, maxY);
            return new Vector3(x, y, -10f);
        }

        void LateUpdate()
        {
            if (_target == null || _cam == null) return;
            if (!Mathf.Approximately(_cam.orthographicSize, LifeGraphics.CameraSize))
                _cam.orthographicSize = LifeGraphics.CameraSize;   // zoom diubah di pengaturan
            Vector3 desired = Desired();
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            float k = 1f - Mathf.Exp(-8f * dt);
            _current.x = Mathf.Lerp(_current.x, desired.x, k);
            _current.y = Mathf.Lerp(_current.y, desired.y, k);
            _current.z = -10f;
            Apply(_current);
        }

        void Apply(Vector3 p)
        {
            p.x = Mathf.Round(p.x * PixelArt.PPU) / PixelArt.PPU;
            p.y = Mathf.Round(p.y * PixelArt.PPU) / PixelArt.PPU;
            transform.position = p;
        }
    }
}
