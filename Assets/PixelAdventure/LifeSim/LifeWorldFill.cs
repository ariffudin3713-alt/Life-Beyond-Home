using System.Collections.Generic;
using PixelAdventure.LifeSim.Pulang;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Mengisi lahan kosong di setiap wilayah dengan bangunan baru (apotek, bank, kantor pos,
    /// rumah makan Padang, puskesmas, rumah joglo, mercusuar, dst.) dan hiasan (pot tanaman,
    /// hidran, papan reklame, parkiran motor, jerami, orang-orangan sawah, perahu, kolam ikan, pohon).
    ///
    /// Semua bangunan bisa dilihat (E - LIHAT ...) tetapi tidak bisa dimasuki. Posisinya dicari
    /// otomatis di area yang diberikan: tidak menimpa bangunan/pohon/lampu lain, jalan & trotoar,
    /// pintu & titik interaksi, atau tanah khusus (lapangan, sawah, kebun, taman, los pasar).
    /// Bila tidak muat, bangunan itu dilewati.
    /// Dipanggil setelah jalan (BuildStreet) dan sebelum suasana (BuildIndo).
    /// </summary>
    public partial class LifeWorld
    {
        struct FillDef
        {
            public string Sprite, Label, Plate, Message;
            public float W, H;          // ukuran gambar (unit)
            public float Fw, Fh;        // alas penghalang (unit)
            public float PlateH;        // tinggi tengah papan nama dari dasar
            public bool Lights;         // jendela menyala di malam hari
        }

        static FillDef FD(string sprite, int pxW, int pxH, float fw, float fh, string label, string plate, float plateH,
            bool lights, string message) => new FillDef
        {
            Sprite = "lc_" + sprite, W = pxW / 16f, H = pxH / 16f, Fw = fw, Fh = fh, Label = label, Plate = plate,
            PlateH = plateH, Lights = lights, Message = message,
        };

        static readonly Dictionary<string, FillDef> FillDefs = new Dictionary<string, FillDef>
        {
            { "apotek",  FD("bld_apotek", 64, 80, 3.8f, 3.6f, "APOTEK", "Apotek", 2.8f, true,
                "Apotek Sehat Selalu. Apotekernya ramah menjelaskan aturan minum obat. Buka sampai jam 9 malam.") },
            { "barber",  FD("bld_barber", 64, 76, 3.8f, 3.4f, "PANGKAS RAMBUT", "Pangkas Rambut", 2.8f, true,
                "Pangkas rambut langganan bapak-bapak kampung. Terdengar obrolan soal sepak bola dari dalam.") },
            { "laundry", FD("bld_laundry", 64, 76, 3.8f, 3.4f, "LAUNDRY", "Laundry", 3.15f, true,
                "Laundry kiloan. \"Cuci + setrika Rp7.000/kg, sehari jadi.\" Wangi pelembut pakaian tercium dari pintu.") },
            { "warnet",  FD("bld_warnet", 64, 76, 3.8f, 3.4f, "WARNET", "Warnet", 2.85f, true,
                "Warnet & game online. Anak-anak sekolah berteriak seru dari balik bilik komputer.") },
            { "bakery",  FD("bld_bakery", 64, 80, 3.8f, 3.4f, "TOKO ROTI", "Toko Roti", 3.15f, true,
                "Toko Roti Harum. Aroma roti sobek yang baru keluar dari oven membuat perutmu keroncongan.") },
            { "bank",    FD("bld_bank", 72, 88, 4.3f, 3.8f, "BANK", "Bank Jogokerto", 3.4f, true,
                "Bank Rakyat Jogokerto. Ada mesin ATM di sampingnya. Antrean nasabah mengular di akhir bulan.") },
            { "post",    FD("bld_post", 72, 80, 4.3f, 3.6f, "KANTOR POS", "Kantor Pos", 3.05f, true,
                "Kantor Pos. Kotak surat oranye di depannya masih sering dipakai warga untuk mengirim surat ke kampung halaman.") },
            { "police",  FD("bld_police", 48, 64, 2.8f, 2.8f, "POS POLISI", "Pos Polisi", 2.05f, true,
                "Pos polisi lalu lintas. \"Utamakan keselamatan, pakai helm dan patuhi rambu!\"") },
            { "atm",     FD("bld_atm", 40, 56, 2.3f, 2.4f, "ANJUNGAN ATM", "ATM", 2.1f, true,
                "Anjungan ATM 24 jam. Lampunya selalu menyala, bahkan tengah malam.") },
            { "ojek",    FD("bld_ojek", 48, 52, 2.8f, 0.6f, "PANGKALAN OJEK", "Ojek", 2.6f, false,
                "Pangkalan ojek. Para tukang ojek berjaket hijau menunggu penumpang sambil main catur.") },
            { "joglo",   FD("bld_joglo", 96, 80, 5.6f, 3.6f, "RUMAH JOGLO", null, 0f, true,
                "Rumah joglo berumur puluhan tahun. Tiang-tiang kayu jatinya diukir halus oleh kakek pemiliknya.") },
            { "lumbung", FD("bld_lumbung", 64, 80, 3.6f, 2.6f, "LUMBUNG PADI", null, 0f, false,
                "Lumbung padi warga. Hasil panen disimpan di atas panggung agar tidak dimakan tikus.") },
            { "puskesmas", FD("bld_puskesmas", 96, 80, 5.8f, 3.6f, "PUSKESMAS", "Puskesmas", 2.7f, true,
                "Puskesmas pembantu. Setiap Selasa ada posyandu untuk menimbang balita dan memeriksa lansia.") },
            { "sd",      FD("bld_sd", 112, 80, 6.8f, 3.4f, "SD NEGERI", "SD Negeri 1", 2.5f, false,
                "SD Negeri 1. Dari jendela terdengar anak-anak menghafal perkalian bersama-sama.") },
            { "kandang", FD("bld_kandang", 80, 64, 5.0f, 2.2f, "KANDANG SAPI", null, 0f, false,
                "Kandang sapi. Seekor sapi menoleh sambil mengunyah jerami. \"Mooo...\"") },
            { "watertower", FD("bld_watertower", 48, 112, 2.6f, 1.0f, "MENARA AIR", null, 0f, false,
                "Menara air warga. Airnya dipompa dari sumur dalam dan dialirkan ke rumah-rumah lewat pipa.") },
            { "padang",  FD("bld_padang", 96, 88, 5.8f, 3.6f, "RUMAH MAKAN PADANG", "RM Padang", 2.06f, true,
                "Rumah Makan Padang. Tumpukan piring rendang, ayam pop, dan gulai tersusun rapi di etalase.") },
            { "goldshop", FD("bld_goldshop", 80, 80, 4.8f, 3.6f, "TOKO EMAS", "Toko Emas", 2.75f, true,
                "Toko Emas Cahaya. Kalung dan gelang berkilauan di balik kaca. Harganya membuatmu menelan ludah.") },
            { "warehouse", FD("bld_warehouse", 96, 76, 5.8f, 3.4f, "GUDANG BERAS", "Gudang Beras", 2.5f, false,
                "Gudang beras pasar. Kuli angkut hilir-mudik memanggul karung 50 kilo.") },
            { "seafood", FD("bld_seafood", 96, 80, 5.8f, 2.6f, "RESTO SEAFOOD", "Seafood", 2.4f, true,
                "Resto seafood panggung. Ikan bakar dan cumi saus padang jadi menu andalannya.") },
            { "lighthouse", FD("bld_lighthouse", 48, 128, 2.4f, 1.0f, "MERCUSUAR", null, 0f, false,
                "Mercusuar Pantai Cemara. Lampunya berputar setiap malam menuntun perahu nelayan pulang.") },
            { "lifeguard", FD("bld_lifeguard", 48, 80, 2.6f, 0.6f, "POS PENJAGA PANTAI", null, 0f, false,
                "Pos penjaga pantai. \"BERENANGLAH DI ANTARA BENDERA KUNING. JANGAN BERENANG SAAT OMBAK BESAR!\"") },
            { "tani",    FD("bld_tani", 64, 76, 3.8f, 3.4f, "TOKO TANI", "Toko Tani", 3.0f, true,
                "Toko Tani Makmur: pupuk, bibit, cangkul, dan obat hama. Pak tani berdiskusi soal musim tanam.") },
        };

        List<Rect> _avoid = new List<Rect>();

        

        void BuildFill(Region r)
        {
            SetClearZones(r);
            _avoid = new List<Rect>();
            switch (r)
            {
                case Region.KotaJogokerto:
                    _avoid.Add(A(4, 47, 28, 22));            // Taman Pelangi
                    _avoid.Add(A(66, 47, 28, 23));           // Alun-Alun Merdeka
                    // Celah di deretan selatan jalan (dasar y 21)
                    Fill("apotek", A(15, 21, 10, 0));
                    Fill("barber", A(27, 21, 10, 0));
                    Fill("laundry", A(61, 21, 10, 0));
                    // Deretan paling selatan (dasar y 8)
                    Fill("post", A(0, 8, 8, 0));
                    Fill("police", A(29, 8, 7, 0));
                    Fill("bank", A(61, 8, 10, 0));
                    // Deretan utara jalan (dasar y 42)
                    Fill("bakery", A(10, 42, 8, 0));
                    Fill("warnet", A(22, 42, 8, 0));
                    Fill("atm", A(63, 42, 7, 0));
                    Fill("ojek", A(74, 42, 7, 0));
                    // Hiasan kota
                    Ornaments("lc_planter", 1.5f, 1.6f, 1.0f, 0.4f, 9, 8f, A(2, 40.35f, 92, 0.3f));
                    Ornaments("lc_hydrant", 0.8f, 1.1f, 0.4f, 0.3f, 3, 25f, A(4, 40.35f, 90, 0.3f));
                    InspectFill("lc_parked_motors", 4.5f, 1.5f, 0.5f, "PARKIRAN MOTOR", ParkLine, A(2, 40.35f, 92, 0.3f));
                    InspectFill("lc_billboard", 4f, 4.5f, 0.4f, "PAPAN REKLAME", BillboardLine, A(54, 62, 12, 9), A(33, 63, 10, 8));
                    InspectFill("lc_billboard", 4f, 4.5f, 0.4f, "PAPAN REKLAME", BillboardLine2, A(86, 1, 9, 6), A(1, 1, 8, 6));
                    // Pohon peneduh & bangku di jalur pejalan kaki antara dua deretan selatan
                    Greenery("lc_tree", 3f, 4.1f, 7, A(2, 14, 92, 0.8f));
                    Ornaments("lc_bench", 2.5f, 1.75f, 2.2f, 0.4f, 4, 16f, A(4, 14.4f, 88, 0.8f));
                    break;

                case Region.DesaSukaMakmur:
                    _avoid.Add(A(8, 48, 24, 16));            // lapangan desa
                    _avoid.Add(A(58, 4, 34, 22));            // sawah
                    _avoid.Add(A(62, 46, 30, 20));           // perkebunan
                    Fill("joglo", A(16, 36, 10, 0));
                    Fill("lumbung", A(43, 36, 7, 0));
                    Fill("tani", A(53, 36, 6, 0));
                    Fill("sd", A(12, 3, 34, 8));
                    Fill("puskesmas", A(2, 2, 46, 16));
                    Fill("joglo", A(18, 13, 28, 8));
                    Fill("kandang", A(34, 12, 14, 10));
                    Fill("watertower", A(2, 3, 12, 12));
                    Fill("joglo", A(34, 54, 14, 14));
                    Fill("lumbung", A(53.5f, 53, 8, 15));
                    InspectFill("lc_fishpond", 4f, 2.3f, 1.8f, "KOLAM IKAN", PondLine, A(34, 39, 14, 7), A(2, 39, 6, 22), A(34, 54, 14, 14));
                    InspectFill("lc_haystack", 2f, 1.6f, 0.6f, "TUMPUKAN JERAMI", HayLine, A(53.5f, 4, 4.5f, 22));
                    InspectFill("lc_haystack", 2f, 1.6f, 0.6f, "TUMPUKAN JERAMI", HayLine, A(53.5f, 4, 4.5f, 22));
                    Scarecrow(68f, 15f); Scarecrow(84f, 20.5f); Scarecrow(78f, 8f);
                    Greenery("lc_tree", 3f, 4.1f, 6, A(2, 2, 46, 26), A(34, 40, 14, 30), A(53.5f, 48, 8, 22));
                    Greenery("lc_bush", 1.9f, 1.3f, 5, A(2, 2, 46, 26), A(2, 40, 6, 30));
                    break;

                case Region.KampungCempaka:
                    _avoid.Add(A(60, 46, 22, 16));           // lapangan kampung
                    _avoid.Add(A(58, 6, 18, 14));            // kebun warga
                    _avoid.Add(A(78, 6, 14, 14));            // peternakan
                    Fill("puskesmas", A(29, 2, 16, 11));
                    Fill("watertower", A(49.5f, 2, 8, 13));
                    FillHouse(DecorKind.HouseGreen, A(2, 59.5f, 26, 6));
                    FillHouse(DecorKind.HouseBlue, A(2, 59.5f, 26, 6));
                    FillHouse(DecorKind.HouseBrown, A(33, 57, 12, 10));
                    FillHouse(DecorKind.HouseGreen, A(49.5f, 56, 9, 12));
                    FillHouse(DecorKind.HouseBlue, A(83, 55, 12, 12));
                    Fill("warnet", A(2, 2, 26, 3));
                    InspectFill("lc_parked_motors", 4.5f, 1.5f, 0.5f, "PARKIRAN MOTOR", ParkLine, A(50, 34.4f, 44, 0.8f));
                    Ornaments("lc_planter", 1.5f, 1.6f, 1.0f, 0.4f, 5, 10f, A(2, 34.4f, 90, 0.8f));
                    Greenery("lc_tree", 3f, 4.1f, 5, A(2, 40, 26, 30), A(33, 40, 12, 30), A(83, 42, 12, 28), A(2, 2, 42, 26));
                    Greenery("lc_flowers", 1.6f, 1.1f, 5, A(2, 40, 26, 30), A(50, 40, 8, 30));
                    break;

                case Region.PasarRamaiJaya:
                    _avoid.Add(A(8, 4, 82, 16));             // los pasar
                    Fill("padang", A(19, 40, 10, 0));
                    Fill("goldshop", A(35, 40, 10, 0));
                    Fill("apotek", A(51, 40, 10, 0));
                    Fill("bakery", A(67, 40, 10, 0));
                    Fill("warehouse", A(35, 20.5f, 10, 0));
                    Fill("post", A(52, 20.5f, 9, 0));
                    Fill("barber", A(69, 20.5f, 8, 0));
                    InspectFill("lc_parked_motors", 4.5f, 1.5f, 0.5f, "PARKIRAN MOTOR", ParkLine, A(4, 37.35f, 90, 0.6f));
                    InspectFill("lc_parked_motors", 4.5f, 1.5f, 0.5f, "PARKIRAN MOTOR", ParkLine, A(50, 37.35f, 44, 0.6f));
                    InspectFill("lc_billboard", 4f, 4.5f, 0.4f, "PAPAN REKLAME", BillboardLine, A(1, 50, 8, 14), A(88, 50, 7, 14));
                    Ornaments("lc_hydrant", 0.8f, 1.1f, 0.4f, 0.3f, 3, 28f, A(4, 37.35f, 90, 0.6f));
                    // Permukiman di belakang pasar
                    FillHouse(DecorKind.HouseBrown, A(2, 62, 92, 5));
                    FillHouse(DecorKind.HouseGreen, A(30, 62, 64, 5));
                    FillHouse(DecorKind.HouseBlue, A(60, 62, 34, 5));
                    Greenery("lc_tree", 3f, 4.1f, 6, A(2, 56, 92, 14));
                    break;

                case Region.PantaiCemara:
                    Fill("seafood", A(2, 17, 19, 8));
                    Fill("lighthouse", A(84, 35, 10, 3));
                    Fill("lifeguard", A(56, 34, 24, 6));
                    Fill("lifeguard", A(8, 34, 24, 6));
                    InspectFill("lc_boat", 4f, 1.9f, 0.5f, "PERAHU NELAYAN", BoatLine, A(12, 41.5f, 30, 2.5f));
                    InspectFill("lc_boat", 4f, 1.9f, 0.5f, "PERAHU NELAYAN", BoatLine, A(54, 41.5f, 28, 2.5f));
                    InspectFill("lc_beach_umbrella", 3f, 3f, 0.4f, "PAYUNG PANTAI", UmbrellaLine, A(46, 24, 16, 10));
                    InspectFill("lc_beach_umbrella", 3f, 3f, 0.4f, "PAYUNG PANTAI", UmbrellaLine, A(78, 20, 14, 10));
                    Greenery("lc_palm", 4.5f, 7f, 4, A(2, 16, 92, 20));
                    break;
            }
        }

        const string ParkLine = "Parkiran motor berjajar rapi. Tukang parkir berompi oranye meniup peluit: \"Priiit, terus... terus... stop!\"";
        const string BillboardLine = "Papan reklame: \"MINUM SUSU SEGAR SETIAP HARI - SEHAT, KUAT, CERDAS!\"";
        const string BillboardLine2 = "Papan reklame: \"SELAMAT DATANG DI KOTA JOGOKERTO - KOTA BERSIH, WARGA RAMAH.\"";
        const string PondLine = "Kolam ikan warga. Ikan mas dan nila berebut pelet yang ditaburkan anak-anak.";
        const string HayLine = "Tumpukan jerami sisa panen. Nanti dipakai untuk pakan sapi dan pupuk kompos.";
        const string BoatLine = "Perahu nelayan bercadik ditarik ke pasir. Jaringnya dijemur menunggu melaut nanti malam.";
        const string UmbrellaLine = "Payung pantai warna-warni. Sewa Rp10.000 sepuasnya, kata penjaganya.";
        const string ScarecrowLine = "Orang-orangan sawah bercaping. Burung pipit tetap saja berani mencuri padi.";

        // ------------------------------------------------------------------ penempatan

        /// <summary>Bangunan pengisi di tempat kosong pertama pada area (h = 0 berarti satu baris dasar).</summary>
        bool Fill(string key, params Rect[] areas)
        {
            var d = FillDefs[key];
            if (!FindFill(d.W, d.H, d.Fw, d.Fh, true, out Vector2 p, areas, 0.1f))
            {
#if UNITY_EDITOR
                Debug.Log("[LifeWorldFill] " + _buildRegion + ": tidak ada lahan untuk " + key);
#endif
                return false;
            }
            float x = p.x, y = p.y;
            Building(d.Sprite, x, y, 1f, d.Fw, d.Fh);
            if (!string.IsNullOrEmpty(d.Plate))
                WallPlate(d.Plate, x, y, d.PlateH, Mathf.Max(3.2f, d.W - 0.4f), PlateStyle.Toko);
            if (d.Lights) AddBuildingLights(x, y, d.W, d.H * 0.8f, true, 0.36f);
            if (key == "lighthouse")
            {
                AddLight(x, y + 6.4f, 3.4f, 3.4f, LampWarm, 0.95f);              // lampu mercusuar
                AddLight(x, y + 6.4f, 7f, 2.2f, LampWarm, 0.35f);
            }
            _decor[_buildRegion].Add(new DecorBuilding { Door = new Vector2(x, y - 0.9f), Label = d.Label, Message = d.Message });
            return true;
        }

        /// <summary>Rumah warga tambahan (gambar & keterangan sama dengan rumah dekorasi lain).</summary>
        bool FillHouse(DecorKind kind, params Rect[] areas)
        {
            if (!FindFill(6.2f, 5.6f, 5.8f, 5.0f, true, out Vector2 p, areas, 0.25f))
            {
#if UNITY_EDITOR
                Debug.Log("[LifeWorldFill] " + _buildRegion + ": tidak ada lahan untuk rumah " + kind);
#endif
                return false;
            }
            Decor(kind, p.x, p.y);
            return true;
        }

        /// <summary>Hiasan yang bisa dilihat (papan reklame, parkiran motor, perahu, ...).</summary>
        bool InspectFill(string sprite, float sw, float sh, float fh, string label, string message, params Rect[] areas)
        {
            float fw = Mathf.Min(sw, 4.2f);
            if (!FindFill(sw, sh, fw, fh, false, out Vector2 p, areas, 0.25f)) return false;
            Prop(sprite, p.x, p.y, true, fw, fh);
            _decor[_buildRegion].Add(new DecorBuilding { Door = new Vector2(p.x, p.y - 0.55f), Label = label, Message = message });
            return true;
        }

        /// <summary>Hiasan kecil berjajar (pot tanaman, hidran) dengan jarak minimal <paramref name="gap"/>.</summary>
        void Ornaments(string sprite, float sw, float sh, float fw, float fh, int count, float gap, Rect area)
        {
            float x = area.xMin;
            for (int i = 0; i < count && x < area.xMax; i++)
            {
                var sub = new Rect(x, area.y, area.xMax - x, area.height);
                if (!FindFill(sw, sh, fw, fh, false, out Vector2 p, new[] { sub }, 0.25f)) return;
                Prop(sprite, p.x, p.y, true, fw, fh);
                x = p.x + gap;
            }
        }

        /// <summary>Pohon / semak / bunga tersebar merata di beberapa area.</summary>
        void Greenery(string sprite, float sw, float sh, int count, params Rect[] areas)
        {
            int placed = 0;
            for (int k = 0; k < 6 && placed < count; k++)
                foreach (var a in areas)
                {
                    if (placed >= count) return;
                    // titik mulai bergeser tiap putaran agar tidak menumpuk di satu sudut
                    float sx = a.xMin + a.width * ((k * 0.37f + 0.11f) % 1f);
                    float sy = a.yMin + a.height * ((k * 0.53f + 0.2f) % 1f);
                    var sub = new Rect(sx, sy, a.xMax - sx, a.yMax - sy);
                    if (sub.width < sw || sub.height < 0.5f) continue;
                    if (!FindFill(sw, sh, 0.6f, 0.5f, false, out Vector2 p, new[] { sub }, 1f)) continue;
                    Prop(sprite, p.x, p.y, true, 0.6f, 0.5f);
                    placed++;
                }
        }

        void Scarecrow(float x, float y)
        {
            Prop("lc_scarecrow", x, y, true, 0.4f, 0.3f);
            _decor[_buildRegion].Add(new DecorBuilding { Door = new Vector2(x, y - 0.5f), Label = "ORANG-ORANGAN SAWAH", Message = ScarecrowLine });
        }

        /// <summary>
        /// Cari tempat untuk benda selebar sw x sh (alas fw x fh, dasar di x,y). Sama seperti Find/Fits,
        /// ditambah: tidak di tanah khusus (_avoid), halaman depan bangunan tetap lapang,
        /// dan lampu/pohon kecil DI DEPAN bangunan diperbolehkan (lampu jalan di depan toko).
        /// </summary>
        bool FindFill(float sw, float sh, float fw, float fh, bool building, out Vector2 pos, Rect[] areas, float step)
        {
            foreach (var a in areas)
            {
                // Satu baris (tinggi area ~0): langkah halus agar pas di celah sempit; area luas: langkah 0,5.
                float st = a.height < 0.05f ? step : Mathf.Max(step, 0.5f);
                for (float y = a.yMin; y <= a.yMax + 0.001f; y += st)
                    for (float x = a.xMin + fw * 0.5f; x <= a.xMax - fw * 0.5f + 0.001f; x += st)
                        if (FitsFill(x, y, sw, sh, fw, fh, building)) { pos = new Vector2(x, y); return true; }
            }
            pos = default;
            return false;
        }

        bool FitsFill(float x, float y, float sw, float sh, float fw, float fh, bool building)
        {
            var foot = new Rect(x - fw * 0.5f, y, fw, fh);
            var pad = new Rect(foot.x - 0.8f, foot.y - 0.8f, foot.width + 1.6f, foot.height + 1.6f);
            var padClear = new Rect(foot.x - 0.25f, foot.y - 0.25f, foot.width + 0.5f, foot.height + 0.5f);
            var sprite = new Rect(x - sw * 0.5f, y, sw, sh);
            if (pad.xMin < 1f || pad.xMax > Width - 1f || pad.yMin < 0.8f || sprite.yMax > Height - 0.5f) return false;

            foreach (var o in _buildObstacles)
            {
                if (pad.Overlaps(o) || sprite.Overlaps(o)) return false;
                // Mahkota pohon / kepala lampu di BELAKANG benda ini jangan tertutup.
                if (o.width <= 0.7f && o.y >= y - 0.1f && sprite.Overlaps(new Rect(o.x - 1.3f, o.y, o.width + 2.6f, 3.4f))) return false;
            }
            foreach (var k in _keepClear) if (padClear.Overlaps(k)) return false;
            foreach (var av in _avoid) if (sprite.Overlaps(av) || foot.Overlaps(av)) return false;

            if (building)
            {
                // Halaman depan (1.4 unit) bebas bangunan besar agar pintu tidak tertutup.
                var yard = new Rect(foot.x, y - 1.4f, foot.width, 1.4f);
                foreach (var o in _buildObstacles)
                    if (o.width > 0.7f && yard.Overlaps(o)) return false;
            }

            foreach (var loc in _locations[_buildRegion])
                if (NearRect(sprite, _spots[loc], 1.9f) || NearRect(pad, _spots[loc], 1.9f)) return false;
            foreach (var d in _decor[_buildRegion])
                if (NearRect(pad, d.Door, 1.4f)) return false;
            foreach (var an in SidewalkAnchors.Values)
                if (NearRect(pad, new Vector2(an.x, an.y), 1.6f)) return false;
            return true;
        }
    }
}
