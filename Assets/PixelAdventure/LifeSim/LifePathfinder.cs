using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Pencari jalan A* di atas grid 0,5 unit untuk kontrol ketuk/klik.
    /// Memakai LifeWorld.CanStand (batas peta, bangunan, perabot, dinding ruangan), lalu
    /// merapikan jalur dengan "string pulling" agar karakter berjalan lurus bila bisa.
    /// </summary>
    public static class LifePathfinder
    {
        const float Cell = 0.5f;
        const int MaxNodes = 9000;

        struct Node
        {
            public int Parent;
            public float G;
            public bool Closed;
        }

        /// <summary>Jalur dari start ke goal (tanpa titik start). Kosong bila tidak ada jalan.</summary>
        public static List<Vector2> Find(LifeWorld world, Vector2 start, Vector2 goal)
        {
            var result = new List<Vector2>();
            int w = Mathf.CeilToInt(LifeWorld.Width / Cell), h = Mathf.CeilToInt(LifeWorld.Height / Cell);

            Vector2Int s = ToCell(start), g = ToCell(goal);
            // Tujuan di dalam penghalang (mis. mengetuk bangunan): cari sel terdekat yang bisa diinjak.
            if (!Standable(world, g) && !NearestStandable(world, g, 6, out g)) return result;
            if (!Standable(world, s)) NearestStandable(world, s, 2, out s);

            var nodes = new Dictionary<int, Node>();
            var open = new List<KeyValuePair<float, int>>();   // (f, key) — heap sederhana
            int Key(Vector2Int c) => c.y * w + c.x;
            int startKey = Key(s), goalKey = Key(g);
            nodes[startKey] = new Node { Parent = -1, G = 0f };
            Push(open, Heuristic(s, g), startKey);

            int expanded = 0;
            bool found = false;
            while (open.Count > 0 && expanded < MaxNodes)
            {
                int cur = Pop(open);
                var cn = nodes[cur];
                if (cn.Closed) continue;
                cn.Closed = true;
                nodes[cur] = cn;
                expanded++;
                if (cur == goalKey) { found = true; break; }

                var cc = new Vector2Int(cur % w, cur / w);
                for (int dx = -1; dx <= 1; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        var nc = new Vector2Int(cc.x + dx, cc.y + dy);
                        if (nc.x < 0 || nc.y < 0 || nc.x >= w || nc.y >= h) continue;
                        if (!Standable(world, nc)) continue;
                        // Diagonal: jangan memotong sudut penghalang.
                        if (dx != 0 && dy != 0 &&
                            (!Standable(world, new Vector2Int(cc.x + dx, cc.y)) || !Standable(world, new Vector2Int(cc.x, cc.y + dy))))
                            continue;
                        int nk = Key(nc);
                        float ng = cn.G + (dx != 0 && dy != 0 ? 1.4142f : 1f);
                        if (nodes.TryGetValue(nk, out Node existing) && (existing.Closed || existing.G <= ng)) continue;
                        nodes[nk] = new Node { Parent = cur, G = ng };
                        Push(open, ng + Heuristic(nc, g), nk);
                    }
            }
            if (!found) return result;

            // Susun jalur dari tujuan ke awal.
            var cells = new List<Vector2>();
            for (int k = goalKey; k != -1 && k != startKey; k = nodes[k].Parent)
                cells.Add(ToWorld(new Vector2Int(k % w, k / w)));
            cells.Reverse();
            if (cells.Count == 0) return result;
            // Titik akhir = posisi ketukan asli bila bisa diinjak (lebih presisi daripada tengah sel).
            if (world.CanStand(goal.x, goal.y) && (ToCell(goal) == g)) cells[cells.Count - 1] = goal;

            // String pulling: lompati titik selama garis lurus bisa dilewati.
            Vector2 from = start;
            int i = 0;
            while (i < cells.Count)
            {
                int far = i;
                for (int j = cells.Count - 1; j > i; j--)
                    if (Clear(world, from, cells[j])) { far = j; break; }
                result.Add(cells[far]);
                from = cells[far];
                i = far + 1;
            }
            return result;
        }

        static Vector2Int ToCell(Vector2 p) => new Vector2Int(Mathf.FloorToInt(p.x / Cell), Mathf.FloorToInt(p.y / Cell));
        static Vector2 ToWorld(Vector2Int c) => new Vector2((c.x + 0.5f) * Cell, (c.y + 0.5f) * Cell);
        static bool Standable(LifeWorld world, Vector2Int c) { var p = ToWorld(c); return world.CanStand(p.x, p.y); }

        static float Heuristic(Vector2Int a, Vector2Int b)
        {
            int dx = Mathf.Abs(a.x - b.x), dy = Mathf.Abs(a.y - b.y);
            return (dx + dy) + (1.4142f - 2f) * Mathf.Min(dx, dy);
        }

        static bool NearestStandable(LifeWorld world, Vector2Int c, int radius, out Vector2Int found)
        {
            for (int r = 1; r <= radius * 2; r++)
                for (int dx = -r; dx <= r; dx++)
                    for (int dy = -r; dy <= r; dy++)
                    {
                        if (Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) != r) continue;
                        var n = new Vector2Int(c.x + dx, c.y + dy);
                        if (Standable(world, n)) { found = n; return true; }
                    }
            found = c;
            return false;
        }

        /// <summary>Garis lurus a→b bisa dilalui (dicek tiap 0,2 unit).</summary>
        static bool Clear(LifeWorld world, Vector2 a, Vector2 b)
        {
            float len = Vector2.Distance(a, b);
            int steps = Mathf.Max(1, Mathf.CeilToInt(len / 0.2f));
            for (int k = 1; k <= steps; k++)
            {
                Vector2 p = Vector2.Lerp(a, b, k / (float)steps);
                if (!world.CanStand(p.x, p.y)) return false;
            }
            return true;
        }

        // ---- antrian prioritas biner sederhana
        static void Push(List<KeyValuePair<float, int>> heap, float f, int key)
        {
            heap.Add(new KeyValuePair<float, int>(f, key));
            int i = heap.Count - 1;
            while (i > 0)
            {
                int p = (i - 1) / 2;
                if (heap[p].Key <= heap[i].Key) break;
                (heap[p], heap[i]) = (heap[i], heap[p]);
                i = p;
            }
        }

        static int Pop(List<KeyValuePair<float, int>> heap)
        {
            int top = heap[0].Value;
            int last = heap.Count - 1;
            heap[0] = heap[last];
            heap.RemoveAt(last);
            int i = 0;
            while (true)
            {
                int l = i * 2 + 1, r = l + 1, m = i;
                if (l < heap.Count && heap[l].Key < heap[m].Key) m = l;
                if (r < heap.Count && heap[r].Key < heap[m].Key) m = r;
                if (m == i) break;
                (heap[m], heap[i]) = (heap[i], heap[m]);
                i = m;
            }
            return top;
        }
    }
}
