using System.Collections.Generic;
using PixelAdventure.LifeSim.Pulang;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Membangun lima wilayah PULANG (satu peta 96x72 per wilayah) dan titik interaksi
    /// setiap lokasi resmi dokumen. Hanya satu wilayah aktif pada satu waktu.
    /// </summary>
    public partial class LifeWorld : MonoBehaviour
    {
        public const float Width = LifeArt.MapW;
        public const float Height = LifeArt.MapH;
        const float PlayerRadius = 0.34f;

        enum Kind { House, Shop, Cafe, Stall, Facade, Tower, Spot, Halte, Inn, Cart }

        struct Placement
        {
            public LocationId Id;
            public Kind Kind;
            public Vector2 Pos;
            public Placement(LocationId id, Kind kind, float x, float y) { Id = id; Kind = kind; Pos = new Vector2(x, y); }
        }

        // Koordinat lokasi per wilayah. "Pos" = dasar bangunan; titik interaksi ada di depan pintu.
        static readonly Placement[] Layout =
        {
            // ---------------- Desa Suka Makmur
            new Placement(LocationId.RumahZidan,   Kind.House,  14f, 36f),
            new Placement(LocationId.WarungSenja,  Kind.Stall,  28f, 36f),
            new Placement(LocationId.BalaiDesa,    Kind.Facade, 40f, 36f),
            new Placement(LocationId.PosRondaDesa, Kind.Stall,  62f, 36f),
            new Placement(LocationId.JalanDesa,    Kind.Spot,   30f, 31.5f),
            new Placement(LocationId.LapanganDesa, Kind.Spot,   20f, 46f),
            new Placement(LocationId.Sawah,        Kind.Spot,   62f, 27.5f),
            new Placement(LocationId.Perkebunan,   Kind.Spot,   70f, 44f),
            new Placement(LocationId.TerminalDesa, Kind.Halte,  86f, 34.0f),        // tepi utara jalan desa

            // ---------------- Kota Jogokerto
            new Placement(LocationId.KostPelangi,      Kind.House,  8f, 42f),
            new Placement(LocationId.TokoSerbaAda,     Kind.Shop,  20f, 42f),
            new Placement(LocationId.Minimarket,       Kind.Shop,  32f, 42f),
            new Placement(LocationId.CafeSenja,        Kind.Cafe,  60f, 42f),
            new Placement(LocationId.TokoBungaMelati,  Kind.Shop,  72f, 42f),
            new Placement(LocationId.RumahPakHendra,   Kind.House, 14f, 21f),
            new Placement(LocationId.BengkelJayaMotor, Kind.Facade, 26f, 21f),
            new Placement(LocationId.TokoAnugerah,     Kind.Shop,  38f, 21f),
            new Placement(LocationId.Perpustakaan,     Kind.Facade, 60f, 21f),
            new Placement(LocationId.RumahSakitSehat,  Kind.Facade, 72f, 21f),
            new Placement(LocationId.PTMajuBersama,    Kind.Tower, 84f, 21f),
            new Placement(LocationId.Sekolah,          Kind.Facade, 26f, 5f),
            new Placement(LocationId.PusatKota,        Kind.Spot,  56f, 34f),
            new Placement(LocationId.TamanPelangi,     Kind.Spot,  18f, 52f),
            new Placement(LocationId.AlunAlunMerdeka,  Kind.Spot,  80f, 52f),
            new Placement(LocationId.TerminalJogokerto, Kind.Halte, 40f, 40.2f),  // trotoar utara, di antara Minimarket dan jalan simpang

            // ---------------- Kampung Cempaka
            new Placement(LocationId.PermukimanCempaka,     Kind.House, 12f, 38f),
            new Placement(LocationId.GangMelati,            Kind.Spot,  31f, 46f),
            new Placement(LocationId.PosRondaCempaka,       Kind.Stall, 40f, 36f),
            new Placement(LocationId.TokoKelontong,         Kind.Shop,  58f, 36f),
            new Placement(LocationId.WarungMakanBuSiti,     Kind.Stall, 70f, 36f),
            new Placement(LocationId.TempatBerkumpulPemuda, Kind.Spot,  86f, 38f),
            new Placement(LocationId.LapanganKampung,       Kind.Spot,  71f, 44f),
            new Placement(LocationId.KebunWarga,            Kind.Spot,  66f, 21f),
            new Placement(LocationId.PeternakanKecil,       Kind.Spot,  85f, 21f),
            new Placement(LocationId.TerminalCempaka,       Kind.Halte,  5f, 33.6f),  // tepi utara jalan kampung

            // ---------------- Pasar Ramai Jaya
            new Placement(LocationId.WarungBuSari,       Kind.Stall, 16f, 40f),
            new Placement(LocationId.KiosSayur,          Kind.Stall, 32f, 40f),
            new Placement(LocationId.KiosBuah,           Kind.Stall, 48f, 40f),
            new Placement(LocationId.TokoPakaian,        Kind.Shop,  64f, 40f),
            new Placement(LocationId.TokoKebutuhanRumah, Kind.Shop,  80f, 40f),
            new Placement(LocationId.PasarTradisional,   Kind.Halte,  5f, 34.4f),    // trotoar utara pasar

            // ---------------- Pantai Cemara
            new Placement(LocationId.PantaiCemara, Kind.Spot, 48f, 44f),

            // ---------------- Penginapan (satu per wilayah, di lahan kosong tepi jalan)
            new Placement(LocationId.PenginapanDesa,    Kind.Inn, 75f, 36f),   // utara jalan desa, sebelum terminal
            new Placement(LocationId.PenginapanKota,    Kind.Inn, 83.5f, 41.6f), // deretan utara, di bawah Alun-Alun
            new Placement(LocationId.PenginapanCempaka, Kind.Inn, 36f, 20f),   // selatan jalan kampung
            new Placement(LocationId.PenginapanPasar,   Kind.Inn, 90.5f, 40f), // ujung timur deretan toko pasar
            new Placement(LocationId.PenginapanPantai,  Kind.Inn, 26f, 15.4f), // utara jalan pantai

            // ---------------- Toko khusus (menggantikan beberapa "toko tutup" dekorasi)
            new Placement(LocationId.DealerKendaraan, Kind.Shop, 38f, 8f),     // Kota selatan
            new Placement(LocationId.TokoElektronik,  Kind.Shop, 72f, 8f),     // Kota selatan
            new Placement(LocationId.TokoBuku,        Kind.Shop, 59f, 49f),    // Kota, dekat Alun-Alun
            new Placement(LocationId.TokoPancing,     Kind.Shop, 56f, 16f),    // Pantai, utara jalan

            // ---------------- Tempat ibadah (menggantikan mushola & ruko dekorasi di Kota)
            new Placement(LocationId.MasjidKota, Kind.Facade, 38f, 59f),
            new Placement(LocationId.GerejaKota, Kind.Facade, 59f, 58f),

            // ---------------- Tempat makan khas Indonesia (gerobak / tenda)
            new Placement(LocationId.GerobakBakso,    Kind.Cart, 74f, 56f),    // Alun-Alun Merdeka
            new Placement(LocationId.Angkringan,      Kind.Cart, 78.5f, 43.6f),// Kampung, dekat lapangan
            new Placement(LocationId.WarungPecelLele, Kind.Cart, 54f, 48f),    // Pasar, belakang los
            new Placement(LocationId.KiosEsKelapa,    Kind.Cart, 30f, 30f),    // Pantai, di pasir
        };

        static readonly Dictionary<Region, string> GroundKeys = new Dictionary<Region, string>
        {
            { Region.DesaSukaMakmur, LifeArt.GroundDesa },
            { Region.KotaJogokerto,  LifeArt.GroundKota },
            { Region.KampungCempaka, LifeArt.GroundKampung },
            { Region.PasarRamaiJaya, LifeArt.GroundPasar },
            { Region.PantaiCemara,   LifeArt.GroundPantai },
        };

        public readonly List<Rect> Obstacles = new List<Rect>();
        readonly Dictionary<Region, Transform> _roots = new Dictionary<Region, Transform>();
        readonly Dictionary<Region, List<Rect>> _regionObstacles = new Dictionary<Region, List<Rect>>();
        readonly Dictionary<LocationId, Vector2> _spots = new Dictionary<LocationId, Vector2>();
        readonly Dictionary<Region, List<LocationId>> _locations = new Dictionary<Region, List<LocationId>>();
        Transform _build;
        List<Rect> _buildObstacles;

        public Region CurrentRegion { get; private set; }

        public Transform RegionRoot(Region r) => _roots[r];

        /// <summary>Titik interaksi lokasi (depan pintu / papan nama).</summary>
        public Vector2 Spot(LocationId id) => _spots[id];

        public IReadOnlyList<LocationId> LocationsIn(Region r) => _locations[r];

        /// <summary>Tempat pemain muncul setelah turun dari bus.</summary>
        /// <summary>
        /// Tempat berdiri NPC untuk lokasi yang titiknya ada di tepi/di jalan (terminal, pusat kota):
        /// NPC berdiri di trotoar, berjajar ke arah <c>step</c>, bukan di tengah jalan.
        /// </summary>
        static readonly Dictionary<LocationId, Vector3> SidewalkAnchors = new Dictionary<LocationId, Vector3>
        {
            { LocationId.TerminalJogokerto, new Vector3(34.6f, 38.9f, -1.5f) },
            { LocationId.TerminalDesa,      new Vector3(81.4f, 34.6f, -1.5f) },
            { LocationId.TerminalCempaka,   new Vector3(9.4f, 34.4f, 1.5f) },
            { LocationId.PasarTradisional,  new Vector3(10.2f, 35.4f, 1.5f) },
            { LocationId.PusatKota,         new Vector3(57.2f, 28.9f, 1.5f) },
        };

        public bool TryNpcAnchor(LocationId id, int index, out Vector2 pos)
        {
            pos = default;
            if (!SidewalkAnchors.TryGetValue(id, out Vector3 a)) return false;
            pos = new Vector2(a.x + a.z * index, a.y);
            return true;
        }

        public Vector2 ArrivalSpot(Region r) => Spot(PulangWorld.ArrivalPoint[r]) + new Vector2(0.9f, -0.6f);

        public void Build()
        {
            foreach (Region r in System.Enum.GetValues(typeof(Region)))
            {
                var root = new GameObject("Wilayah_" + r).transform;
                root.SetParent(transform, false);
                _roots[r] = root;
                _build = root;
                _buildRegion = r;
                _buildObstacles = new List<Rect>();
                _regionObstacles[r] = _buildObstacles;
                _locations[r] = new List<LocationId>();

                Make("lc_ground_" + GroundKeys[r], 0f, 0f, "Ground", -3000);
                foreach (var p in Layout)
                    if (PulangWorld.Get(p.Id).Region == r) Place(p, r);
                BuildScenery(r);
                BuildDecor(r);
                BuildCritters(r, root);
                BuildStreet(r, root);
                BuildFill(r);
                BuildIndo(r, root);
                root.gameObject.SetActive(false);
            }
            SetRegion(Region.DesaSukaMakmur);
        }

        public void SetRegion(Region r)
        {
            foreach (var pair in _roots) pair.Value.gameObject.SetActive(pair.Key == r);
            CurrentRegion = r;
            _cullTimer = 0f;   // periksa ulang culling segera setelah pindah wilayah
            Obstacles.Clear();
            Obstacles.AddRange(_regionObstacles[r]);
        }

        // ------------------------------------------------------------------ bangunan

        void Place(Placement p, Region r)
        {
            string name = PulangWorld.Get(p.Id).Name;
            float x = p.Pos.x, y = p.Pos.y;
            Vector2 spot = new Vector2(x, y - 1.1f);
            PlateStyle style = IsPublic(p.Id) ? PlateStyle.Umum : PlateStyle.Toko;

            // Papan nama menempel di dinding bangunan (tinggi & lebar mengikuti sprite),
            // jadi tidak menghalangi jalan, pintu, atau tokoh di depannya.
            // Bangunan dengan fasad khusus (sekolah, rumah sakit, toko baju, dst.).
            bool special = TrySpecialFacade(p.Id, out Facade f);
            if (special)
            {
                Building(f.Sprite, x, y, 1f, f.W, f.H);
                WallPlate(name, x, y, f.Plate, Mathf.Min(6.8f, f.W + 0.6f), style);
                AddBuildingLights(x, y, f.W, f.LightH, f.Door, f.Window);
            }
            else switch (p.Kind)
            {
                case Kind.House:
                    Building("lc_house", x, y, 1f, 6.8f, 5.9f);
                    WallPlate(name, x, y, 3.0f, 6.0f, style);
                    break;
                case Kind.Shop:
                    Building("lc_shop", x, y, 1f, 6.4f, 5.3f);
                    WallPlate(name, x, y, 2.9f, 5.6f, style);
                    break;
                case Kind.Cafe:
                    Building("lc_cafe", x, y, 1f, 7.8f, 5.9f);
                    WallPlate(name, x, y, 3.1f, 6.6f, style);
                    break;
                case Kind.Stall:
                    Building("lc_market_stall", x, y, 1.1f, 6f, 3f);
                    WallPlate(name, x, y, 3.5f, 6.0f, style);
                    break;
                case Kind.Facade:
                    Building("lc_oldtown_facade", x, y, 0.72f, 4.6f, 4.4f);
                    WallPlate(name, x, y, 2.3f, 4.4f, style);
                    break;
                case Kind.Tower:
                    Building("lc_office_tower", x, y, 0.5f, 2.4f, 4.6f);
                    WallPlate(name, x, y, 1.3f, 3.6f, style);
                    break;
                case Kind.Cart:
                {
                    // Gerobak / tenda: gambar sesuai tempat, penghalang rendah, papan nama melayang di atasnya.
                    bool pecel = p.Id == LocationId.WarungPecelLele || p.Id == LocationId.PecelLeleKota;
                    bool warkop = p.Id == LocationId.WarkopDesa;
                    string sprite = p.Id == LocationId.GerobakBakso ? "lc_cart_bakso"
                        : p.Id == LocationId.Angkringan ? "lc_angkringan"
                        : pecel ? "lc_tent_pecel" : warkop ? "lc_warkop" : "lc_kiosk_kelapa";
                    float cw = pecel ? 5.2f : warkop ? 4.0f : p.Id == LocationId.Angkringan ? 4.2f : 3.2f;
                    float topH = pecel ? 3.75f : warkop ? 3.4f : p.Id == LocationId.Angkringan ? 3.5f : 3.1f;
                    Building(sprite, x, y, 1f, cw, 1.1f);
                    WallPlate(name, x, y, topH + 0.45f, 6f, PlateStyle.Toko);
                    spot = new Vector2(x, y - 0.9f);
                    AddLight(x, y + topH - 0.6f, 2.2f, 2.0f, LampWarm, 0.6f);   // lampu warung
                    break;
                }
                case Kind.Inn:
                    Building("lc_inn", x, y, 1f, 7.0f, 6.6f);
                    WallPlate(name, x, y, 4.75f, 6.8f, style);
                    break;
                case Kind.Halte:
                    Make("lc_bus_stop", x, y, "Halte", Order(y));
                    AddObstacle(x - 0.9f, y, 1.8f, 0.6f);
                    WallPlate(name, x, y, 3.25f, 4.8f, PlateStyle.Halte);
                    spot = new Vector2(x + 1.4f, y - 0.3f);
                    break;
                default:
                    // Tempat terbuka: papan bertiang di pinggir, bukan di tengah jalur.
                    spot = new Vector2(x, y - 1f);
                    Vector2 at = SpotSignPos.TryGetValue(p.Id, out Vector2 custom) ? custom : spot + new Vector2(-3f, 0.6f);
                    PostSign(name, at.x, at.y, StreetNames.Contains(p.Id) ? PlateStyle.Jalan : PlateStyle.Tempat);
                    break;
            }

            // Jendela & pintu bangunan menyala di malam hari.
            if (!special) switch (p.Kind)
            {
                case Kind.House: AddBuildingLights(x, y, 6.8f, 5.9f); break;
                case Kind.Shop: AddBuildingLights(x, y, 6.4f, 5.3f, true, 0.4f); break;
                case Kind.Cafe: AddBuildingLights(x, y, 7.8f, 5.9f, true, 0.45f); break;
                case Kind.Inn: AddBuildingLights(x, y, 7.0f, 6.6f, true, 0.42f); break;
                case Kind.Facade: AddBuildingLights(x, y, 4.6f, 4.4f, true, 0.28f); break;
                case Kind.Tower: AddBuildingLights(x, y, 2.4f, 4.6f, false, 0.4f); break;
                case Kind.Stall: AddLight(x, y + 2.6f, 2.0f, 1.8f, LampWarm, 0.5f, 0.15f); break;   // lampu petromaks kios
                case Kind.Halte: AddLight(x, y + 2.9f, 2.0f, 2.0f, LampWarm, 0.55f); break;
            }

            _spots[p.Id] = spot;
            _placeKinds[p.Id] = p.Kind;
            _locations[r].Add(p.Id);
        }

        static bool IsPublic(LocationId id) =>
            id == LocationId.BalaiDesa || id == LocationId.RumahSakitSehat || id == LocationId.Sekolah ||
            id == LocationId.Perpustakaan || id == LocationId.PTMajuBersama || id == LocationId.PosRondaDesa ||
            id == LocationId.PosRondaCempaka;

        /// <summary>Nama jalan/lorong memakai papan hijau khas nama jalan.</summary>
        static readonly HashSet<LocationId> StreetNames = new HashSet<LocationId>
        {
            LocationId.JalanDesa, LocationId.PusatKota, LocationId.GangMelati,
        };

        /// <summary>Letak papan tempat terbuka: di tepi area, jauh dari jalur jalan dan pintu.</summary>
        static readonly Dictionary<LocationId, Vector2> SpotSignPos = new Dictionary<LocationId, Vector2>
        {
            { LocationId.JalanDesa,             new Vector2(5f, 33.4f) },
            { LocationId.LapanganDesa,          new Vector2(34.5f, 47f) },
            { LocationId.Sawah,                 new Vector2(56f, 26.6f) },
            { LocationId.Perkebunan,            new Vector2(60.5f, 47f) },
            { LocationId.PusatKota,             new Vector2(54.6f, 28.3f) },
            { LocationId.TamanPelangi,          new Vector2(5f, 49.5f) },
            { LocationId.AlunAlunMerdeka,       new Vector2(64.6f, 47.2f) },
            { LocationId.GangMelati,            new Vector2(34.3f, 33.6f) },
            { LocationId.TempatBerkumpulPemuda, new Vector2(92.5f, 41f) },
            { LocationId.LapanganKampung,       new Vector2(58.4f, 46.6f) },
            { LocationId.KebunWarga,            new Vector2(56.2f, 21.5f) },
            { LocationId.PeternakanKecil,       new Vector2(93.5f, 21.5f) },
            { LocationId.PantaiCemara,          new Vector2(39.5f, 41.6f) },
        };

        void Building(string sprite, float x, float y, float scale, float w, float h)
        {
            Make(sprite, x, y, sprite, Order(y)).transform.localScale = Vector3.one * scale;
            AddObstacle(x - w * 0.5f, y, w, h);
        }

        void BuildScenery(Region r)
        {
            switch (r)
            {
                case Region.DesaSukaMakmur:
                    Trees(new[] { 4f, 26f, 4f, 58f, 34f, 66f, 46f, 20f, 8f, 12f, 30f, 12f, 40f, 8f, 24f, 22f });
                    Trees(new[] { 66f, 50f, 72f, 56f, 78f, 50f, 84f, 58f, 90f, 50f, 66f, 62f, 76f, 64f, 88f, 64f });
                    Lamps(new[] { 22f, 29.2f, 46f, 29.2f, 74f, 29.2f });
                    Make("lc_bush", 10f, 34f, "Bush", Order(34f));
                    Make("lc_flowers", 18f, 34.5f, "Flowers", Order(34.5f));
                    break;

                case Region.KotaJogokerto:
                    Lamps(new[] { 12f, 27.4f, 30f, 27.4f, 66f, 27.4f, 88f, 27.4f, 16f, 40.6f, 66f, 40.6f });
                    Trees(new[] { 8f, 60f, 28f, 62f, 6f, 52f, 70f, 64f, 90f, 64f, 70f, 52f });
                    Make("lc_fountain", 80f, 58f, "AirMancur", Order(58f));
                    AddObstacle(78.4f, 57.7f, 3.2f, 1.7f);
                    Make("lc_bench", 14f, 56f, "Bangku", Order(56f));
                    Make("lc_bench", 86f, 54f, "BangkuAlun", Order(54f));
                    BuildPark();
                    break;

                case Region.KampungCempaka:
                    Building("lc_house", 22f, 38f, 1f, 6.8f, 5.9f);
                    Building("lc_house", 12f, 14f, 1f, 6.8f, 5.9f);
                    Building("lc_house", 24f, 14f, 1f, 6.8f, 5.9f);
                    Building("lc_market_stall", 85f, 12f, 1f, 6f, 3f);       // kandang
                    Make("lc_bench", 84f, 40f, "BangkuPemuda", Order(40f));
                    Make("lc_bench", 89f, 40f, "BangkuPemuda2", Order(40f));
                    Trees(new[] { 4f, 50f, 40f, 56f, 52f, 64f, 92f, 60f, 4f, 6f, 40f, 8f });
                    Make("lc_flowers", 33f, 56f, "BungaGang", Order(56f));
                    Make("lc_flowers", 29f, 40f, "BungaGang2", Order(40f));
                    Lamps(new[] { 20f, 29.2f, 52f, 29.2f, 80f, 29.2f });
                    break;

                case Region.PasarRamaiJaya:
                    PostSign("Pasar Ramai Jaya", 48f, 22.6f, PlateStyle.Tempat);
                    for (int row = 0; row < 2; row++)
                        for (int col = 0; col < 5; col++)
                            Building("lc_market_stall", 14f + col * 17f, 8f + row * 9f, 1f, 6.4f, 2.8f);
                    Lamps(new[] { 24f, 26.6f, 72f, 26.6f });
                    break;

                case Region.PantaiCemara:
                    AddObstacle(0f, 46f, Width, Height - 46f);              // laut
                    for (int i = 0; i < 8; i++)
                    {
                        float x = 10f + i * 11f;
                        Make("lc_tree", x, 6f, "Cemara" + i, Order(6f));
                        AddObstacle(x - 0.3f, 6f, 0.6f, 0.5f);
                    }
                    float[] palms = { 20f, 30f, 44f, 36f, 64f, 28f, 82f, 38f };
                    for (int i = 0; i < palms.Length; i += 2)
                    {
                        Make("lc_palm", palms[i], palms[i + 1], "Palem" + i, Order(palms[i + 1]));
                        AddObstacle(palms[i] - 0.25f, palms[i + 1], 0.5f, 0.5f);
                    }
                    Make("lc_beach_umbrella", 36f, 24f, "Payung", Order(24f));
                    Make("lc_beach_umbrella", 72f, 22f, "Payung2", Order(22f));
                    Make("lc_bus_stop", 6f, 14.3f, "Halte", Order(14.3f));   // di pasir, tepi utara jalan
                    WallPlate("Halte Pantai", 6f, 14.3f, 3.25f, 4.8f, PlateStyle.Halte);
                    break;
            }
        }

        void Trees(float[] xy)
        {
            for (int i = 0; i + 1 < xy.Length; i += 2)
            {
                Make("lc_tree", xy[i], xy[i + 1], "Pohon", Order(xy[i + 1]));
                AddObstacle(xy[i] - 0.3f, xy[i + 1], 0.6f, 0.5f);
            }
        }

        void Lamps(float[] xy)
        {
            for (int i = 0; i + 1 < xy.Length; i += 2)
            {
                float x = xy[i], y = xy[i + 1];
                Make("lc_lamp", x, y, "Lampu", Order(y));
                AddObstacle(x - 0.16f, y, 0.32f, 0.32f);
                AddLight(x, y + 3.45f, 2.8f, 2.8f, LampWarm, 0.9f);          // kepala lampu
                AddLight(x, y + 0.25f, 3.6f, 1.7f, LampWarm, 0.42f);         // genangan cahaya di tanah
            }
        }

        // ------------------------------------------------------------------ util

        static int Order(float y) => -Mathf.RoundToInt(y * PixelArt.PPU);

        SpriteRenderer Make(string key, float x, float y, string name, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_build, false);
            go.transform.position = new Vector3(x, y, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PixelArt.Get(key);
            sr.sharedMaterial = PixelArt.SpriteMaterial;
            sr.sortingOrder = order;
            if (name != "Ground") RegisterCulled(sr);   // tanah selalu terlihat
            return sr;
        }

        // ------------------------------------------------------------------ culling objek diam

        class Culled
        {
            public SpriteRenderer Sr;
            public Bounds Box;
            public bool HasBox;
        }

        readonly Dictionary<Region, List<Culled>> _culled = new Dictionary<Region, List<Culled>>();
        const float CullInterval = 0.1f;
        float _cullTimer;

        void RegisterCulled(SpriteRenderer sr)
        {
            if (!_culled.TryGetValue(_buildRegion, out List<Culled> list))
                _culled[_buildRegion] = list = new List<Culled>();
            list.Add(new Culled { Sr = sr });
        }

        /// <summary>
        /// Bangunan, pohon, lampu, rambu & papan nama yang tidak masuk layar tidak digambar.
        /// Diperiksa tiap 0,1 detik; batas objek dihitung sekali (objek ini tidak bergerak).
        /// </summary>
        void Update()
        {
            _cullTimer -= Time.unscaledDeltaTime;
            if (_cullTimer > 0f) return;
            _cullTimer = CullInterval;
            if (!_culled.TryGetValue(CurrentRegion, out List<Culled> list)) return;
            for (int i = 0; i < list.Count; i++)
            {
                var c = list[i];
                if (c.Sr == null) continue;
                if (!c.HasBox)
                {
                    if (!c.Sr.enabled) c.Sr.enabled = true;
                    c.Box = c.Sr.bounds;
                    c.HasBox = c.Box.size.sqrMagnitude > 0f;
                    if (!c.HasBox) continue;
                }
                bool visible = LifeView.Sees(c.Box, 2f);
                if (c.Sr.enabled != visible) c.Sr.enabled = visible;
            }
        }

        // ------------------------------------------------------------------ papan nama

        class SignLabel
        {
            public Region Region;
            public Vector2 Center;
            public SpriteRenderer[] Parts;
            public float Alpha = -1f;
        }

        readonly List<SignLabel> _signs = new List<SignLabel>();
        Region _buildRegion;
        const int CharPx = 6;     // lebar satu huruf PixelFont skala 1
        const int LinePx = 9;     // tinggi satu baris termasuk garis tepi

        /// <summary>Papan nama yang menempel di dinding, setinggi <paramref name="height"/> dari dasar bangunan.</summary>
        void WallPlate(string title, float x, float baseY, float height, float maxWidth, PlateStyle style)
        {
            // Urutan gambar sama dengan bangunannya: tokoh di depan bangunan selalu tampil di atas papan.
            CreateSign(title, x, baseY + height, maxWidth, style, Order(baseY) + 1, false);
        }

        /// <summary>Papan bertiang untuk tempat terbuka dan nama jalan.</summary>
        void PostSign(string title, float x, float y, PlateStyle style)
        {
            AddObstacle(x - 0.12f, y, 0.24f, 0.2f);
            CreateSign(title, x, y, 5.2f, style, Order(y), true);
        }

        void CreateSign(string title, float x, float y, float maxWidth, PlateStyle style, int order, bool post)
        {
            int maxChars = Mathf.Max(6, Mathf.FloorToInt((maxWidth * PixelArt.PPU - 8f) / CharPx));
            string[] lines = WrapSign(title.ToUpperInvariant(), maxChars);

            int textW = 0;
            foreach (string l in lines) textW = Mathf.Max(textW, l.Length * CharPx + 2);
            int boardW = textW + 8;
            int boardH = lines.Length * LinePx + 4;

            var root = new GameObject("Papan_" + title);
            root.transform.SetParent(_build, false);
            root.transform.position = new Vector3(x, y, 0f);

            var parts = new List<SpriteRenderer>();
            var board = root.AddComponent<SpriteRenderer>();
            board.sprite = LifeArt.Plate(boardW, boardH, style);
            board.sharedMaterial = PixelArt.SpriteMaterial;
            board.sortingOrder = order;
            parts.Add(board);
            RegisterCulled(board);

            // Tengah papan (relatif ke root): papan bertiang berpivot di kaki tiang.
            float boardCenterY = post ? (LifeArt.PlatePostPx + boardH * 0.5f) / PixelArt.PPU : 0f;
            for (int i = 0; i < lines.Length; i++)
            {
                var t = new GameObject("Teks");
                t.transform.SetParent(root.transform, false);
                float offset = ((lines.Length - 1) * 0.5f - i) * LinePx / PixelArt.PPU;
                t.transform.localPosition = new Vector3(0f, boardCenterY + offset + 0.5f / PixelArt.PPU, 0f);
                var sr = t.AddComponent<SpriteRenderer>();
                sr.sprite = PixelFont.Render(lines[i], LifeArt.PlateText(style), LifeArt.PlateTextOutline(style), 1,
                    new Vector2(0.5f, 0.5f));
                sr.sharedMaterial = PixelArt.SpriteMaterial;
                sr.sortingOrder = order + 1;
                parts.Add(sr);
                RegisterCulled(sr);
            }

            _signs.Add(new SignLabel
            {
                Region = _buildRegion,
                Center = new Vector2(x, y + boardCenterY),
                Parts = parts.ToArray(),
            });
        }

        /// <summary>Bungkus nama panjang menjadi 2–3 baris agar tidak melebihi lebar bangunan.</summary>
        static string[] WrapSign(string text, int maxChars)
        {
            var lines = new List<string>();
            string cur = "";
            foreach (string word in text.Split(' '))
            {
                if (word.Length == 0) continue;
                if (cur.Length == 0) cur = word;
                else if (cur.Length + 1 + word.Length <= maxChars) cur += " " + word;
                else { lines.Add(cur); cur = word; }
            }
            if (cur.Length > 0) lines.Add(cur);
            if (lines.Count == 0) lines.Add(" ");
            return lines.ToArray();
        }

        /// <summary>
        /// Papan samar saat jauh dan jelas saat didekati, supaya peta tidak ramai tulisan.
        /// Panggil setiap frame dengan posisi pemain.
        /// </summary>
        public void UpdateSigns(Vector2 player)
        {
            for (int i = 0; i < _signs.Count; i++)
            {
                var s = _signs[i];
                if (s.Region != CurrentRegion) continue;
                if (!LifeView.Sees(s.Center, 4f, 2f)) continue;   // papan di luar layar: lewati
                float d = Vector2.Distance(player, s.Center);
                float a = Mathf.Lerp(1f, 0.45f, Mathf.InverseLerp(5f, 12f, d));
                if (Mathf.Abs(a - s.Alpha) < 0.02f) continue;
                s.Alpha = a;
                foreach (var part in s.Parts)
                {
                    Color c = part.color;
                    c.a = a;
                    part.color = c;
                }
            }
        }

        void AddObstacle(float x, float y, float w, float h) => _buildObstacles.Add(new Rect(x, y, w, h));

        public bool CanStand(float x, float y)
        {
            if (x < 0.55f || x > Width - 0.55f || y < 0.72f || y > Height - 0.9f) return false;
            for (int i = 0; i < Obstacles.Count; i++)
            {
                Rect r = Obstacles[i];
                if (x > r.xMin - PlayerRadius && x < r.xMax + PlayerRadius &&
                    y > r.yMin - PlayerRadius && y < r.yMax + PlayerRadius)
                    return false;
            }
            return true;
        }

    }
}
