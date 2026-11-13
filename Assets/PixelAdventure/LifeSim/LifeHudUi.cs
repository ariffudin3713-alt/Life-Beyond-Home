using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    public partial class LifeHud
    {
        /// <summary>Lebar satu huruf PixelFont skala 1 (unit).</summary>
        const float UiCharW = 6f / 16f;
        /// <summary>Jarak baris teks skala 1 (unit).</summary>
        const float UiLine = 0.5f;

        /// <summary>Jumlah huruf skala <paramref name="scale"/> yang muat di lebar <paramref name="width"/>.</summary>
        static int CharsFor(float width, int scale = 1) => Mathf.Max(1, Mathf.FloorToInt(width / (UiCharW * scale)));

        /// <summary>
        /// Penggambar UI "langsung" untuk layar yang tata letaknya dihitung ulang tiap frame
        /// (jurnal, kontak): teks, kotak warna, panel, dan gambar diambil dari kolam objek
        /// lalu yang tidak terpakai disembunyikan. Teks dirender lewat cache PixelFont.
        /// </summary>
        sealed class UiPool
        {
            readonly Transform _parent;
            readonly int _order;
            readonly List<SpriteRenderer> _texts = new List<SpriteRenderer>();
            readonly List<SpriteRenderer> _solids = new List<SpriteRenderer>();
            readonly List<SpriteRenderer> _panels = new List<SpriteRenderer>();
            readonly List<SpriteRenderer> _sprites = new List<SpriteRenderer>();
            int _t, _s, _p, _i;

            public UiPool(Transform parent, int baseOrder)
            {
                _parent = parent;
                _order = baseOrder;
            }

            public void Begin() { _t = _s = _p = _i = 0; }

            public void End()
            {
                Hide(_texts, _t); Hide(_solids, _s); Hide(_panels, _p); Hide(_sprites, _i);
            }

            static void Hide(List<SpriteRenderer> list, int used)
            {
                for (int i = used; i < list.Count; i++)
                    if (list[i].gameObject.activeSelf) list[i].gameObject.SetActive(false);
            }

            SpriteRenderer Next(List<SpriteRenderer> list, ref int index, string name, Sprite sprite)
            {
                SpriteRenderer sr;
                if (index < list.Count) sr = list[index];
                else
                {
                    sr = Make(_parent, sprite, name + list.Count, _order);
                    list.Add(sr);
                }
                index++;
                if (!sr.gameObject.activeSelf) sr.gameObject.SetActive(true);
                return sr;
            }

            /// <summary>Teks; pivotX 0 = rata kiri, 0.5 = tengah, 1 = rata kanan (tegak di tengah y).</summary>
            public SpriteRenderer Text(string text, float x, float y, Color color, int scale = 1, float pivotX = 0f, int layer = 4)
            {
                var sr = Next(_texts, ref _t, "T", null);
                sr.sprite = PixelFont.Render(string.IsNullOrEmpty(text) ? " " : text, color, OutlineColor, scale,
                    new Vector2(pivotX, 0.5f));
                sr.sortingOrder = _order + layer;
                sr.transform.localPosition = new Vector3(x, y, 0f);
                sr.transform.localScale = Vector3.one;
                sr.color = Color.white;
                return sr;
            }

            /// <summary>Kotak warna polos dengan pusat (cx, cy).</summary>
            public SpriteRenderer Solid(float cx, float cy, float w, float h, Color color, int layer = 2)
            {
                var sr = Next(_solids, ref _s, "S", PixelArt.SolidUnit);
                sr.color = color;
                sr.sortingOrder = _order + layer;
                SetSolid(sr, cx, cy, w, h);
                return sr;
            }

            /// <summary>Panel berbingkai (9-slice) dengan pusat (cx, cy).</summary>
            public SpriteRenderer Panel(string key, float cx, float cy, float w, float h, int layer = 1)
            {
                var sr = Next(_panels, ref _p, "P", PixelArt.Get(key));
                sr.drawMode = SpriteDrawMode.Sliced;
                sr.sprite = PixelArt.Get(key);
                sr.sortingOrder = _order + layer;
                SetRect(sr, cx, cy, w, h);
                return sr;
            }

            /// <summary>Gambar (ikon, potret) di (x, y) sesuai pivot gambarnya.</summary>
            public SpriteRenderer Sprite(Sprite sprite, float x, float y, float scale, int layer = 3, Color? tint = null)
            {
                var sr = Next(_sprites, ref _i, "I", sprite);
                sr.sprite = sprite;
                sr.sortingOrder = _order + layer;
                sr.color = tint ?? Color.white;
                sr.transform.localPosition = new Vector3(x, y, 0f);
                sr.transform.localScale = Vector3.one * scale;
                return sr;
            }

            /// <summary>Bilah kemajuan: latar gelap + isi berwarna (0..1).</summary>
            public void Bar(float left, float cy, float w, float h, float fill, Color color)
            {
                Solid(left + w * 0.5f, cy, w, h, new Color(0.08f, 0.06f, 0.11f, 1f), 2);
                float f = Mathf.Clamp01(fill);
                if (f > 0.001f) Solid(left + w * f * 0.5f, cy, w * f, h - 0.08f, color, 3);
            }
        }
    }
}
