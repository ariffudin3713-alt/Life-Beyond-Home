using System.Collections.Generic;
using PixelAdventure.LifeSim.Pulang;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Suasana khas Indonesia di setiap wilayah: papan "DILARANG BUANG SAMPAH SEMBARANGAN",
    /// tong sampah tiga warna, spanduk kebersihan, bendera merah putih & umbul-umbul, gapura
    /// "SELAMAT DATANG", becak, gerobak sayur, jemuran, pohon pisang, kucing oren, ayam kampung,
    /// warung pecel lele di kota & warkop di desa, serta sampah berserakan yang bisa dipungut.
    ///
    /// Posisi dipilih otomatis: setiap benda mencari tempat kosong di area yang diinginkan,
    /// tidak di jalan / jalur pejalan kaki, tidak menutup pintu atau celah antar bangunan,
    /// dan tidak menimpa pohon/lampu/rambu. Bila tidak ada tempat, benda itu dilewati.
    /// </summary>
    public partial class LifeWorld
    {
        readonly Dictionary<Region, List<GameObject>> _litter = new Dictionary<Region, List<GameObject>>();
        List<Rect> _keepClear = new List<Rect>();     // jalan + jalur pejalan kaki wilayah yang sedang dibangun
        List<Rect> _roads = new List<Rect>();         // hanya badan jalan (sampah tidak ditaruh di sini)

        static readonly string[] CatLines =
        {
            "Kucing oren sedang tidur siang. Katanya kucing oren paling bandel se-kampung.",
            "Seekor kucing oren menatapmu, lalu kembali menjilati kakinya.",
        };

        /// <summary>Sampah yang bisa dipungut di wilayah ini (objek nonaktif = sudah dipungut).</summary>
        public IReadOnlyList<GameObject> LitterIn(Region r) =>
            _litter.TryGetValue(r, out var list) ? list : (IReadOnlyList<GameObject>)new GameObject[0];

        void BuildIndo(Region r, Transform root)
        {
            _litter[r] = new List<GameObject>();
            SetClearZones(r);
            var chickens = new List<Rect>();

            switch (r)
            {
                case Region.KotaJogokerto:
                    StallAt(LocationId.PecelLeleKota, Kind.Cart, 5.6f, 3.9f, 5.2f, 1.1f,
                        A(4, 46, 30, 6), A(56, 44, 10, 10), A(2, 41, 94, 26));
                    Inspect("lc_sign_litter", 3.5f, 2.4f, 0.4f, "PAPAN LARANGAN", LitterSign, A(4, 45.5f, 30, 4), A(4, 41, 36, 6));
                    Inspect("lc_sign_litter", 3.5f, 2.4f, 0.4f, "PAPAN LARANGAN", LitterSign, A(66, 44, 28, 6));
                    Inspect("lc_banner_clean", 4.5f, 2.5f, 0.3f, "SPANDUK", "Spanduk RT: \"JAGA KEBERSIHAN, KOTA BERSIH WARGA SEHAT.\"", A(30, 41, 36, 8), A(2, 41, 94, 10));
                    Bins(A(4, 40.3f, 36, 0.4f));
                    Bins(A(56, 40.3f, 36, 0.4f));
                    Bins(A(4, 26.7f, 36, 0.5f), A(56, 26.7f, 36, 0.5f));
                    Bins(A(8, 46, 24, 5));
                    Bins(A(65, 50.6f, 12, 2), A(83, 50.6f, 11, 2));
                    Inspect("lc_flag_pole", 1.2f, 4f, 0.4f, "BENDERA", FlagLine, A(79.2f, 67.5f, 1.6f, 2.5f), A(66, 64, 26, 6));
                    Umbul(A(4, 40.3f, 36, 0.4f), 5, 7f);
                    Umbul(A(56, 40.3f, 38, 0.4f), 5, 7f);
                    Inspect("lc_becak", 2.8f, 2.3f, 0.5f, "BECAK", BecakLine, A(4, 40.3f, 36, 1.2f), A(56, 40.3f, 36, 1.2f));
                    Prop("lc_cat_sleep", 14.3f, 56.05f, false);                         // di bangku taman
                    OccupySeatNear(new Vector2(14.6f, 56.6f), "kucing");               // kursi kanan dipakai kucing
                    Pet("lc_cat_sit", A(30, 41, 12, 8), A(2, 41, 94, 26));
                    Litter(9, 101, A(0, 28.1f, 96, 1.7f), A(0, 38.2f, 96, 1.7f), A(6, 50, 24, 14), A(65, 51, 29, 19));
                    break;

                case Region.PasarRamaiJaya:
                    Inspect("lc_sign_litter", 3.5f, 2.4f, 0.4f, "PAPAN LARANGAN", LitterSign, A(30, 21, 40, 5), A(4, 21, 90, 5));
                    Inspect("lc_banner_clean", 4.5f, 2.5f, 0.3f, "SPANDUK", "Spanduk pengelola pasar: \"JAGA KEBERSIHAN PASAR KITA.\"", A(30, 37.3f, 40, 2), A(2, 37.3f, 92, 3));
                    Bins(A(4, 37.3f, 40, 0.5f));
                    Bins(A(50, 37.3f, 44, 0.5f));
                    Bins(A(4, 25.6f, 40, 1f));
                    Bins(A(50, 25.6f, 44, 1f));
                    Umbul(A(4, 37.3f, 90, 0.5f), 6, 12f);
                    Inspect("lc_becak", 2.8f, 2.3f, 0.5f, "BECAK", BecakLine, A(4, 37.3f, 90, 1f));
                    Inspect("lc_becak", 2.8f, 2.3f, 0.5f, "BECAK", BecakLine, A(50, 37.3f, 44, 1f), A(4, 21, 90, 5));
                    Inspect("lc_cart_sayur", 3f, 2f, 0.5f, "GEROBAK SAYUR", SayurLine, A(4, 21, 90, 5));
                    Pet("lc_cat_sit", A(8, 5, 80, 20));
                    Litter(11, 202, A(8, 5, 80, 20), A(0, 34.1f, 96, 2.8f), A(0, 27.1f, 96, 2.8f));
                    chickens.Add(new Rect(8f, 44f, 30f, 2f));
                    break;

                case Region.PantaiCemara:
                    Inspect("lc_sign_litter", 3.5f, 2.4f, 0.4f, "PAPAN LARANGAN",
                        "DILARANG BUANG SAMPAH DI PANTAI! Sampah plastik bisa terbawa ombak dan membahayakan penyu & ikan.",
                        A(30, 17, 30, 6), A(4, 16, 88, 10));
                    Bins(A(30, 20, 20, 8));
                    Bins(A(60, 18, 26, 8));
                    Umbul(A(4, 15.5f, 88, 0.6f), 5, 14f);
                    Inspect("lc_flag_pole", 1.2f, 4f, 0.4f, "BENDERA", FlagLine, A(44, 38, 10, 4), A(4, 16, 88, 24));
                    Pet("lc_cat_sleep", A(20, 16, 50, 10));
                    Litter(10, 303, A(4, 16, 88, 28));
                    break;

                case Region.DesaSukaMakmur:
                    StallAt(LocationId.WarkopDesa, Kind.Cart, 4.4f, 3.6f, 4f, 1.1f,
                        A(56, 34.2f, 16, 4), A(2, 34.2f, 92, 8), A(2, 22, 92, 5));
                    Gapura(51.5f, 25f, 8f, "Gapura desa: SELAMAT DATANG DI DESA SUKA MAKMUR. Dicat merah putih oleh warga setiap Agustus.");
                    Inspect("lc_sign_litter", 3.5f, 2.4f, 0.4f, "PAPAN LARANGAN", LitterSign, A(4, 34.2f, 40, 5), A(2, 22, 92, 6));
                    Bins(A(30, 34.2f, 40, 4));
                    Inspect("lc_flag_pole", 1.2f, 4f, 0.4f, "BENDERA", FlagLine, A(34, 34.2f, 14, 3), A(2, 34.2f, 92, 6));
                    Umbul(A(4, 34.2f, 90, 0.6f), 5, 14f);
                    Scatter("lc_banana", 2.2f, 2.9f, 0.5f, 5, A(2, 2, 46, 24), A(56, 2, 38, 24), A(2, 40, 92, 28));
                    Inspect("lc_jemuran", 3.6f, 2f, 0.3f, "JEMURAN", JemuranLine, A(4, 14, 44, 12), A(2, 2, 92, 26));
                    Pet("lc_cat_sleep", A(2, 34.2f, 92, 8));
                    Litter(6, 404, A(0, 33.4f, 96, 4), A(2, 2, 46, 26));
                    chickens.Add(new Rect(6f, 16f, 40f, 10f));
                    chickens.Add(new Rect(56f, 36f, 30f, 8f));
                    break;

                case Region.KampungCempaka:
                    Gapura(47.5f, 36f, 60f, "Gapura kampung: SELAMAT DATANG DI KAMPUNG CEMPAKA. Di sebelahnya tertulis: \"TAMU WAJIB LAPOR 1x24 JAM\".");
                    Inspect("lc_sign_litter", 3.5f, 2.4f, 0.4f, "PAPAN LARANGAN", LitterSign, A(56, 42, 30, 4), A(2, 34.2f, 92, 10));
                    Inspect("lc_banner_clean", 4.5f, 2.5f, 0.3f, "SPANDUK", "Spanduk karang taruna: \"KERJA BAKTI SETIAP MINGGU PAGI. JAGA KEBERSIHAN KAMPUNG!\"", A(50, 34.2f, 44, 6), A(2, 34.2f, 92, 10));
                    Bins(A(56, 42, 30, 4));
                    Bins(A(2, 34.2f, 40, 4));
                    Inspect("lc_flag_pole", 1.2f, 4f, 0.4f, "BENDERA", FlagLine, A(36, 34.2f, 8, 4), A(2, 34.2f, 92, 8));
                    Umbul(A(4, 34.2f, 90, 0.6f), 6, 12f);
                    Inspect("lc_cart_sayur", 3f, 2f, 0.5f, "GEROBAK SAYUR", SayurLine, A(50, 34.2f, 44, 4), A(2, 34.2f, 92, 8));
                    Scatter("lc_banana", 2.2f, 2.9f, 0.5f, 4, A(2, 2, 42, 26), A(2, 40, 26, 28), A(50, 40, 44, 28));
                    Inspect("lc_jemuran", 3.6f, 2f, 0.3f, "JEMURAN", JemuranLine, A(2, 40, 26, 24), A(50, 40, 44, 28));
                    Inspect("lc_jemuran", 3.6f, 2f, 0.3f, "JEMURAN", JemuranLine, A(2, 2, 42, 26));
                    Pet("lc_cat_sit", A(2, 34.2f, 92, 8));
                    Pet("lc_cat_sleep", A(50, 40, 44, 20));
                    Litter(7, 505, A(0, 33.4f, 96, 4), A(60, 46, 22, 16));
                    chickens.Add(new Rect(79f, 7f, 12f, 12f));        // peternakan kecil
                    chickens.Add(new Rect(50f, 22f, 40f, 6f));
                    break;
            }

            if (chickens.Count > 0)
                LifeChickens.Attach(root, _buildObstacles, chickens, r == Region.PasarRamaiJaya ? 2 : 4);
        }

        const string LitterSign =
            "DILARANG BUANG SAMPAH SEMBARANGAN! Pelanggar dikenakan denda. Pungut sampah yang kamu temukan lalu buang ke tong sampah.";
        const string FlagLine = "Sang Merah Putih berkibar gagah. Warga memasangnya menyambut 17 Agustus.";
        const string BecakLine = "Becak sedang mangkal menunggu penumpang. Pengayuhnya tidur siang di jok depan.";
        const string SayurLine = "Gerobak sayur keliling. Tiap pagi ibu-ibu berkerumun di sini sambil bertukar kabar.";
        const string JemuranLine = "Jemuran warga. Semoga tidak turun hujan sore ini.";
        const string BinLine =
            "Tong sampah tiga warna: HIJAU untuk sampah organik, KUNING untuk anorganik, MERAH untuk sampah B3. Buanglah sampah pada tempatnya!";

        // ------------------------------------------------------------------ penempatan

        static Rect A(float x, float y, float w, float h) => new Rect(x, y, w, h);

        void SetClearZones(Region r)
        {
            _keepClear = new List<Rect>();
            _roads = new List<Rect>();
            _roadsByRegion[r] = _roads;
            switch (r)
            {
                case Region.KotaJogokerto:
                    _roads.Add(A(0, 30, 96, 8)); _roads.Add(A(44, 0, 8, 72));
                    _keepClear.Add(A(0, 27.9f, 96, 12.2f)); _keepClear.Add(A(41.7f, 0, 12.6f, 72));
                    break;
                case Region.PasarRamaiJaya:
                    _roads.Add(A(0, 30, 96, 4));
                    _keepClear.Add(A(0, 26.9f, 96, 10.2f));
                    break;
                case Region.PantaiCemara:
                    _roads.Add(A(0, 10, 96, 4));
                    _keepClear.Add(A(0, 9.8f, 96, 4.4f)); _keepClear.Add(A(0, 14.5f, 96, 0.8f)); _keepClear.Add(A(0, 32.6f, 96, 0.8f));
                    break;
                case Region.DesaSukaMakmur:
                    _roads.Add(A(0, 30, 96, 3)); _roads.Add(A(50, 0, 3, 72));
                    _keepClear.Add(A(0, 29.1f, 96, 4.9f)); _keepClear.Add(A(49.8f, 0, 3.4f, 72));
                    break;
                case Region.KampungCempaka:
                    _roads.Add(A(0, 30, 96, 3)); _roads.Add(A(46, 0, 3, 72)); _roads.Add(A(30, 33, 2, 28));
                    _keepClear.Add(A(0, 29.8f, 96, 4.2f)); _keepClear.Add(A(45.8f, 0, 3.4f, 72)); _keepClear.Add(A(29.6f, 32.8f, 2.8f, 28.6f));
                    break;
            }
        }

        /// <summary>
        /// Apakah benda dengan gambar selebar sw x sh dan alas fw x fh (dasar di x,y) muat di sini?
        /// Alas diberi jarak 0.8 dari penghalang lain (celah tetap bisa dilewati pemain).
        /// </summary>
        bool Fits(float x, float y, float sw, float sh, float fw, float fh, float clearPad = 0.25f)
        {
            var foot = new Rect(x - fw * 0.5f, y, fw, fh);
            var pad = new Rect(foot.x - 0.8f, foot.y - 0.8f, foot.width + 1.6f, foot.height + 1.6f);
            var padClear = new Rect(foot.x - clearPad, foot.y - clearPad, foot.width + clearPad * 2f, foot.height + clearPad * 2f);
            var sprite = new Rect(x - sw * 0.5f, y, sw, sh);
            if (pad.xMin < 1f || pad.xMax > Width - 1f || pad.yMin < 1f || sprite.yMax > Height - 0.5f) return false;

            foreach (var o in _buildObstacles)
            {
                if (pad.Overlaps(o) || sprite.Overlaps(o)) return false;
                // Benda kecil (pohon, lampu, rambu): hindari juga mahkota/kepalanya.
                if (o.width <= 0.7f && sprite.Overlaps(new Rect(o.x - 1.3f, o.y, o.width + 2.6f, 3.4f))) return false;
            }
            foreach (var k in _keepClear) if (padClear.Overlaps(k)) return false;

            // Jangan menutup pintu / titik interaksi.
            foreach (var loc in _locations[_buildRegion])
                if (NearRect(sprite, _spots[loc], 1.9f) || NearRect(pad, _spots[loc], 1.9f)) return false;
            foreach (var d in _decor[_buildRegion])
                if (NearRect(pad, d.Door, 1.4f)) return false;
            foreach (var a in SidewalkAnchors.Values)
                if (NearRect(pad, new Vector2(a.x, a.y), 1.6f)) return false;
            return true;
        }

        static bool NearRect(Rect r, Vector2 p, float d) =>
            p.x > r.xMin - d && p.x < r.xMax + d && p.y > r.yMin - d && p.y < r.yMax + d;

        /// <summary>Cari tempat kosong pertama di area-area yang diberikan (berurutan).</summary>
        bool Find(float sw, float sh, float fw, float fh, out Vector2 pos, Rect[] areas, float clearPad = 0.25f, float step = 0.5f)
        {
            foreach (var a in areas)
                for (float y = a.yMin; y <= a.yMax + 0.001f; y += step)
                    for (float x = a.xMin + fw * 0.5f; x <= a.xMax - fw * 0.5f + 0.001f; x += step)
                        if (Fits(x, y, sw, sh, fw, fh, clearPad)) { pos = new Vector2(x, y); return true; }
            pos = default;
            return false;
        }

        SpriteRenderer Prop(string sprite, float x, float y, bool solid, float fw = 0f, float fh = 0f)
        {
            var sr = Make(sprite, x, y, sprite, Order(y));
            if (solid) AddObstacle(x - fw * 0.5f, y, fw, fh);
            if (sprite == "lc_bench") RegisterBench(x, y);   // bangku hiasan trotoar juga bisa diduduki
            return sr;
        }

        /// <summary>Benda yang bisa dilihat (E - LIHAT ...).</summary>
        bool Inspect(string sprite, float sw, float sh, float fh, string label, string message, params Rect[] areas)
        {
            float fw = Mathf.Min(sw, 3.2f);
            if (!Find(sw, sh, fw, fh, out Vector2 p, areas)) return false;
            Prop(sprite, p.x, p.y, true, fw, fh);
            _decor[_buildRegion].Add(new DecorBuilding { Door = new Vector2(p.x, p.y - 0.55f), Label = label, Message = message });
            return true;
        }

        void Bins(params Rect[] areas)
        {
            if (!Find(2.4f, 1.3f, 2.2f, 0.4f, out Vector2 p, areas)) return;
            Prop("lc_bins3", p.x, p.y, true, 2.2f, 0.4f);
            _decor[_buildRegion].Add(new DecorBuilding { Door = new Vector2(p.x, p.y - 0.55f), Label = "TONG SAMPAH", Message = BinLine, Bin = true });
        }

        /// <summary>Umbul-umbul berjajar dengan jarak minimal <paramref name="gap"/>.</summary>
        void Umbul(Rect area, int count, float gap)
        {
            float x = area.xMin;
            for (int i = 0; i < count && x < area.xMax; i++)
            {
                if (!Find(1.1f, 4.4f, 0.3f, 0.25f, out Vector2 p, new[] { new Rect(x, area.y, area.xMax - x, area.height) })) return;
                Prop("lc_umbul_" + (i % 2), p.x, p.y, true, 0.3f, 0.25f);
                x = p.x + gap;
            }
        }

        void Scatter(string sprite, float sw, float sh, float fh, int count, params Rect[] areas)
        {
            int placed = 0;
            foreach (var a in areas)
            {
                // beberapa titik mulai berbeda di tiap area agar tidak menumpuk di satu sudut
                for (int k = 0; k < 4 && placed < count; k++)
                {
                    float sx = a.xMin + a.width * (k * 0.27f % 1f);
                    var sub = new Rect(sx, a.y, a.xMax - sx, a.height);
                    if (Find(sw, sh, 0.5f, fh, out Vector2 p, new[] { sub }, 0.25f, 1f))
                    {
                        Prop(sprite, p.x, p.y, true, 0.5f, fh);
                        placed++;
                    }
                }
                if (placed >= count) return;
            }
        }

        /// <summary>Kucing: tidak menghalangi, tapi bisa dilihat.</summary>
        void Pet(string sprite, params Rect[] areas)
        {
            if (!Find(1f, 0.8f, 0.6f, 0.3f, out Vector2 p, areas)) return;
            Prop(sprite, p.x, p.y, false);
            _decor[_buildRegion].Add(new DecorBuilding
            {
                Door = new Vector2(p.x, p.y - 0.3f), Label = "KUCING",
                Message = CatLines[Mathf.Abs((int)(p.x * 3 + p.y)) % CatLines.Length],
            });
        }

        /// <summary>Warung/gerobak sungguhan (punya menu) yang posisinya dicari otomatis.</summary>
        void StallAt(LocationId id, Kind kind, float sw, float sh, float fw, float fh, params Rect[] areas)
        {
            if (!Find(sw, sh + 0.6f, fw, fh, out Vector2 p, areas)) p = areas[0].center;
            Place(new Placement(id, kind, p.x, p.y), _buildRegion);
        }

        /// <summary>Gapura melintang di atas jalan tegak (tiang di kiri-kanan jalan).</summary>
        void Gapura(float cx, float yFrom, float yTo, string message)
        {
            float dir = yTo >= yFrom ? 1f : -1f;
            for (float y = yFrom; dir > 0 ? y <= yTo : y >= yTo; y += dir * 0.5f)
            {
                if (!Fits(cx - 2.45f, y, 0.8f, 4.6f, 0.6f, 0.4f, 0.2f) || !Fits(cx + 2.45f, y, 0.8f, 4.6f, 0.6f, 0.4f, 0.2f)) continue;
                Make("lc_gapura", cx, y, "Gapura", Order(y));
                AddObstacle(cx - 2.75f, y, 0.6f, 0.4f);
                AddObstacle(cx + 2.15f, y, 0.6f, 0.4f);
                _decor[_buildRegion].Add(new DecorBuilding { Door = new Vector2(cx - 2.45f, y - 0.5f), Label = "GAPURA", Message = message });
                return;
            }
        }

        /// <summary>Sampah berserakan (botol, bungkus, kaleng, kertas) di titik acak yang bisa dicapai.</summary>
        void Litter(int count, int seed, params Rect[] areas)
        {
            var rng = new System.Random(seed);
            int tries = 0;
            while (_litter[_buildRegion].Count < count && tries++ < 400)
            {
                Rect a = areas[rng.Next(areas.Length)];
                float x = a.xMin + (float)rng.NextDouble() * a.width;
                float y = a.yMin + (float)rng.NextDouble() * a.height;
                if (!FreeGround(x, y)) continue;
                bool tooClose = false;
                foreach (var o in _litter[_buildRegion])
                    if (((Vector2)o.transform.position - new Vector2(x, y)).sqrMagnitude < 9f) { tooClose = true; break; }
                if (tooClose) continue;
                var sr = Make("lc_litter_" + rng.Next(4), x, y, "Sampah", Order(y) + 1);
                sr.flipX = rng.Next(2) == 0;
                _litter[_buildRegion].Add(sr.gameObject);
            }
        }

        bool FreeGround(float x, float y)
        {
            if (x < 1.5f || x > Width - 1.5f || y < 1.5f || y > Height - 1.5f) return false;
            foreach (var rd in _roads) if (rd.Contains(new Vector2(x, y))) return false;
            foreach (var o in _buildObstacles)
                if (x > o.xMin - 0.6f && x < o.xMax + 0.6f && y > o.yMin - 0.6f && y < o.yMax + 0.6f) return false;
            return true;
        }
    }

    /// <summary>Ayam kampung yang berkeliaran, sesekali mematuk tanah. Tidur (hilang) saat malam.</summary>
    public sealed class LifeChickens : MonoBehaviour
    {
        class Hen
        {
            public SpriteRenderer Sr;
            public Rect Area;
            public Vector2 Pos, Target;
            public float Wait, Peck;
        }

        readonly List<Hen> _hens = new List<Hen>();
        List<Rect> _obstacles;

        public static LifeChickens Attach(Transform root, List<Rect> obstacles, List<Rect> areas, int perArea)
        {
            var c = new GameObject("AyamKampung").AddComponent<LifeChickens>();
            c.transform.SetParent(root, false);
            c._obstacles = obstacles;
            int n = 0;
            foreach (var a in areas)
                for (int i = 0; i < perArea; i++)
                {
                    var go = new GameObject("Ayam" + n++);
                    go.transform.SetParent(c.transform, false);
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = PixelArt.Get("lc_chicken_0");
                    sr.sharedMaterial = PixelArt.SpriteMaterial;
                    var h = new Hen { Sr = sr, Area = a, Wait = Random.Range(0f, 2f) };
                    h.Pos = h.Target = c.RandomPoint(a, a.center);
                    c._hens.Add(h);
                }
            return c;
        }

        Vector2 RandomPoint(Rect a, Vector2 fallback)
        {
            for (int t = 0; t < 12; t++)
            {
                var p = new Vector2(Random.Range(a.xMin, a.xMax), Random.Range(a.yMin, a.yMax));
                bool ok = true;
                foreach (var o in _obstacles)
                    if (p.x > o.xMin - 0.3f && p.x < o.xMax + 0.3f && p.y > o.yMin - 0.3f && p.y < o.yMax + 0.3f) { ok = false; break; }
                if (ok) return p;
            }
            return fallback;
        }

        void Update()
        {
            bool night = LifeWorld.NightLevel > 0.6f;
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            foreach (var h in _hens)
            {
                if (h.Sr.enabled == night) h.Sr.enabled = !night;
                if (night) continue;

                if (h.Wait > 0f)
                {
                    h.Wait -= dt;
                    h.Peck += dt;
                    h.Sr.sprite = PixelArt.Get((int)(h.Peck * 4f) % 2 == 0 ? "lc_chicken_1" : "lc_chicken_0");
                    if (h.Wait <= 0f) h.Target = RandomPoint(new Rect(h.Pos.x - 3f, h.Pos.y - 2f, 6f, 4f), h.Pos);
                }
                else
                {
                    if (!h.Area.Contains(h.Target)) h.Target = RandomPoint(h.Area, h.Pos);
                    Vector2 d = h.Target - h.Pos;
                    if (d.magnitude < 0.05f) { h.Wait = Random.Range(1f, 3.5f); h.Peck = 0f; }
                    else
                    {
                        h.Pos += d.normalized * Mathf.Min(d.magnitude, 1.4f * dt);
                        h.Sr.flipX = d.x < 0f;
                        h.Sr.sprite = PixelArt.Get("lc_chicken_0");
                    }
                }
                h.Sr.transform.position = new Vector3(h.Pos.x, h.Pos.y + Mathf.Abs(Mathf.Sin(Time.time * 12f)) * (h.Wait > 0f ? 0f : 0.04f), 0f);
                h.Sr.sortingOrder = -Mathf.RoundToInt(h.Pos.y * PixelArt.PPU);
            }
        }
    }
}
