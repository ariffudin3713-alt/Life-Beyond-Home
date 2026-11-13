using System.Collections.Generic;
using PixelAdventure.LifeSim.Pulang;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>Titik acak untuk NPC: bisa dipijak, tidak di badan jalan, tidak menutupi pintu.</summary>
    public partial class LifeWorld
    {
        readonly Dictionary<Region, List<Rect>> _roadsByRegion = new Dictionary<Region, List<Rect>>();

        /// <summary>Apakah titik ini di badan jalan wilayah yang aktif (tempat kendaraan lewat)?</summary>
        public bool OnRoad(Vector2 p)
        {
            if (InInterior || !_roadsByRegion.TryGetValue(CurrentRegion, out var roads)) return false;
            foreach (var r in roads) if (r.Contains(p)) return true;
            return false;
        }

        bool NearDoor(Vector2 p, float d)
        {
            foreach (var loc in _locations[CurrentRegion])
                if ((_spots[loc] - p).sqrMagnitude < d * d) return true;
            foreach (var dec in DecorIn(CurrentRegion))
                if ((dec.Door - p).sqrMagnitude < d * d) return true;
            return false;
        }

        /// <summary>Titik acak sejauh minR–maxR dari <paramref name="home"/> di wilayah aktif; gagal = home.</summary>
        public Vector2 RandomNpcSpot(Vector2 home, float minR, float maxR, IList<Vector2> avoid = null)
        {
            for (int attempt = 0; attempt < 40; attempt++)
            {
                float ang = Random.Range(0f, Mathf.PI * 2f);
                float dist = Random.Range(minR, maxR);
                Vector2 p = home + new Vector2(Mathf.Cos(ang) * dist, Mathf.Sin(ang) * dist * 0.75f);
                if (!CanStand(p.x, p.y) || OnRoad(p) || NearDoor(p, 1.1f)) continue;
                bool crowded = false;
                if (avoid != null)
                    foreach (var a in avoid) if ((a - p).sqrMagnitude < 1.2f * 1.2f) { crowded = true; break; }
                if (crowded) continue;
                return p;
            }
            return home;
        }

        /// <summary>Kotak bebas dipijak untuk NPC yang berkeliaran (tidak masuk jalan raya).</summary>
        public bool CanWander(Vector2 p) => CanStand(p.x, p.y) && !OnRoad(p);
    }
}
