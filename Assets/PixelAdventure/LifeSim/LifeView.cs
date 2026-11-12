using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Area pandang kamera (dunia), dihitung sekali per frame. Dipakai untuk melewati
    /// gambar & logika objek yang tidak terlihat di layar (culling manual).
    /// </summary>
    public static class LifeView
    {
        static int _frame = -1;
        static Rect _rect;
        static Camera _cam;

        /// <summary>Kamera yang dipakai; diset sekali saat boot (fallback Camera.main).</summary>
        public static void SetCamera(Camera cam) => _cam = cam;

        /// <summary>Persegi area pandang kamera dalam koordinat dunia.</summary>
        public static Rect Rect
        {
            get
            {
                if (_frame != Time.frameCount) Recompute();
                return _rect;
            }
        }

        static void Recompute()
        {
            _frame = Time.frameCount;
            if (_cam == null) _cam = Camera.main;
            if (_cam == null) { _rect = new Rect(-1e6f, -1e6f, 2e6f, 2e6f); return; }
            float halfH = _cam.orthographicSize;
            float halfW = halfH * _cam.aspect;
            Vector3 p = _cam.transform.position;
            _rect = new Rect(p.x - halfW, p.y - halfH, halfW * 2f, halfH * 2f);
        }

        /// <summary>Titik (kaki objek) terlihat, dengan margin agar objek tinggi tidak "muncul tiba-tiba".</summary>
        /// <param name="halfWidth">Setengah lebar objek + margin.</param>
        /// <param name="height">Tinggi objek di atas kakinya (bisa terlihat walau kaki di bawah layar).</param>
        public static bool Sees(Vector2 p, float halfWidth = 2f, float height = 4f)
        {
            Rect r = Rect;
            return p.x > r.xMin - halfWidth && p.x < r.xMax + halfWidth &&
                   p.y > r.yMin - height && p.y < r.yMax + 1f;
        }

        /// <summary>Kotak dunia bersinggungan dengan area pandang (+ margin).</summary>
        public static bool Sees(Bounds b, float margin = 1f)
        {
            Rect r = Rect;
            return b.max.x > r.xMin - margin && b.min.x < r.xMax + margin &&
                   b.max.y > r.yMin - margin && b.min.y < r.yMax + margin;
        }
    }
}
