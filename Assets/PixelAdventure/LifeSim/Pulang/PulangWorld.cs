using System.Collections.Generic;

namespace PixelAdventure.LifeSim.Pulang
{
    // =====================================================================
    //  DATABASE DUNIA PULANG
    //  Hanya wilayah, lokasi, dan tokoh yang tertulis di dokumen Master Story.
    //  Nama ganda di dokumen disatukan:
    //    "Desa Sukamerta"   -> Desa Suka Makmur
    //    "Pantai Sagara"    -> Pantai Cemara
    //    "Jogokarta"        -> Kota Jogokerto
    //    "Terminal Maju Jaya" -> Terminal Jogokerto
    // =====================================================================

    /// <summary>Wilayah yang dihubungkan terminal. Tidak ada kota lain.</summary>
    public enum Region
    {
        DesaSukaMakmur,
        KotaJogokerto,
        KampungCempaka,
        PasarRamaiJaya,
        PantaiCemara,
    }

    public enum LocationId
    {
        // Desa Suka Makmur
        RumahZidan, JalanDesa, Sawah, Perkebunan, BalaiDesa, WarungSenja, PosRondaDesa,
        LapanganDesa, TerminalDesa,

        // Kota Jogokerto
        TerminalJogokerto, PusatKota, AlunAlunMerdeka, TamanPelangi, KostPelangi, TokoSerbaAda,
        BengkelJayaMotor, CafeSenja, RumahSakitSehat, Perpustakaan, PTMajuBersama, TokoAnugerah,
        TokoBungaMelati, Minimarket, RumahPakHendra, Sekolah,

        // Kampung Cempaka
        PermukimanCempaka, GangMelati, PosRondaCempaka, LapanganKampung, TokoKelontong,
        PeternakanKecil, KebunWarga, WarungMakanBuSiti, TempatBerkumpulPemuda, TerminalCempaka,

        // Pasar Ramai Jaya
        PasarTradisional, WarungBuSari, KiosSayur, KiosBuah, TokoPakaian, TokoKebutuhanRumah,

        // Pantai Cemara
        PantaiCemara,

        // Penginapan (satu di setiap wilayah) — ditaruh di akhir agar urutan simpanan lama tetap sama
        PenginapanDesa, PenginapanKota, PenginapanCempaka, PenginapanPasar, PenginapanPantai,
    }

    public enum Chapter { Prolog = 0, Ch1 = 1, Ch2 = 2, Ch3 = 3, Ch4 = 4, Ch5 = 5, Ch6 = 6, Ending = 7 }

    public struct LocationDef
    {
        public LocationId Id;
        public Region Region;
        public string Name;
        public Chapter OpensAt;   // Bagian 6: "Beberapa lokasi hanya terbuka setelah quest tertentu"
        public string Jobs;       // tempat kerja menurut dokumen, kosong bila tidak ada

        public LocationDef(LocationId id, Region region, string name, Chapter opensAt, string jobs = "")
        {
            Id = id; Region = region; Name = name; OpensAt = opensAt; Jobs = jobs;
        }
    }

    public static class PulangWorld
    {
        public static readonly Dictionary<Region, string> RegionNames = new Dictionary<Region, string>
        {
            { Region.DesaSukaMakmur, "Desa Suka Makmur" },
            { Region.KotaJogokerto,  "Kota Jogokerto" },
            { Region.KampungCempaka, "Kampung Cempaka" },
            { Region.PasarRamaiJaya, "Pasar Ramai Jaya" },
            { Region.PantaiCemara,   "Pantai Cemara" },
        };

        public static readonly LocationDef[] Locations =
        {
            // ---- Desa Suka Makmur: hanya Prolog & Ending (lihat CanTravelTo) ----
            new LocationDef(LocationId.RumahZidan,   Region.DesaSukaMakmur, "Rumah Zidan & Ratna", Chapter.Prolog),
            new LocationDef(LocationId.JalanDesa,    Region.DesaSukaMakmur, "Jalan Desa",          Chapter.Prolog),
            new LocationDef(LocationId.Sawah,        Region.DesaSukaMakmur, "Sawah",               Chapter.Prolog, "Petani"),
            new LocationDef(LocationId.Perkebunan,   Region.DesaSukaMakmur, "Perkebunan",          Chapter.Prolog, "Pekerja kebun"),
            new LocationDef(LocationId.BalaiDesa,    Region.DesaSukaMakmur, "Balai Desa",          Chapter.Prolog),
            new LocationDef(LocationId.WarungSenja,  Region.DesaSukaMakmur, "Warung Senja",        Chapter.Prolog, "Pelayan warung"),
            new LocationDef(LocationId.PosRondaDesa, Region.DesaSukaMakmur, "Pos Ronda",           Chapter.Prolog),
            new LocationDef(LocationId.LapanganDesa, Region.DesaSukaMakmur, "Lapangan Desa",       Chapter.Prolog),
            new LocationDef(LocationId.TerminalDesa, Region.DesaSukaMakmur, "Terminal Desa Suka Makmur", Chapter.Prolog),

            // ---- Kota Jogokerto ----
            new LocationDef(LocationId.TerminalJogokerto, Region.KotaJogokerto, "Terminal Jogokerto", Chapter.Ch1, "Petugas loket, Penjaga warung"),
            new LocationDef(LocationId.KostPelangi,       Region.KotaJogokerto, "Kost Pelangi",       Chapter.Ch1),
            new LocationDef(LocationId.PusatKota,         Region.KotaJogokerto, "Pusat Kota",         Chapter.Ch1),
            new LocationDef(LocationId.TamanPelangi,      Region.KotaJogokerto, "Taman Pelangi",      Chapter.Ch1),
            new LocationDef(LocationId.AlunAlunMerdeka,   Region.KotaJogokerto, "Alun-Alun Merdeka",  Chapter.Ch1, "Pedagang makanan, Petugas kebersihan"),
            new LocationDef(LocationId.TokoSerbaAda,      Region.KotaJogokerto, "Toko Serba Ada",     Chapter.Ch1),
            new LocationDef(LocationId.Minimarket,        Region.KotaJogokerto, "Minimarket",         Chapter.Ch1, "Kasir, Pegawai toko"),
            new LocationDef(LocationId.RumahPakHendra,    Region.KotaJogokerto, "Rumah Pak Hendra",   Chapter.Ch1),
            new LocationDef(LocationId.BengkelJayaMotor,  Region.KotaJogokerto, "Bengkel Jaya Motor", Chapter.Ch1, "Montir motor"),
            new LocationDef(LocationId.CafeSenja,         Region.KotaJogokerto, "Cafe Senja",         Chapter.Ch2, "Barista, Pelayan kafe"),
            new LocationDef(LocationId.Perpustakaan,      Region.KotaJogokerto, "Perpustakaan",       Chapter.Ch2),
            new LocationDef(LocationId.TokoAnugerah,      Region.KotaJogokerto, "Toko Anugerah",      Chapter.Ch2, "Kasir, Penjaga toko"),
            new LocationDef(LocationId.RumahSakitSehat,   Region.KotaJogokerto, "Rumah Sakit Sehat",  Chapter.Ch3, "Petugas administrasi, Dokter, Perawat"),
            new LocationDef(LocationId.TokoBungaMelati,   Region.KotaJogokerto, "Toko Bunga Melati",  Chapter.Ch3),
            new LocationDef(LocationId.PTMajuBersama,     Region.KotaJogokerto, "PT Maju Bersama",    Chapter.Ch3, "Pegawai kantor, Admin, Staf, Kasir"),
            new LocationDef(LocationId.Sekolah,           Region.KotaJogokerto, "Sekolah",            Chapter.Ch3, "Guru"),

            // ---- Kampung Cempaka ----
            new LocationDef(LocationId.TerminalCempaka,       Region.KampungCempaka, "Terminal Kampung Cempaka", Chapter.Ch1),
            new LocationDef(LocationId.PermukimanCempaka,     Region.KampungCempaka, "Permukiman Penduduk",      Chapter.Ch1),
            new LocationDef(LocationId.PosRondaCempaka,       Region.KampungCempaka, "Pos Ronda",                Chapter.Ch1),
            new LocationDef(LocationId.LapanganKampung,       Region.KampungCempaka, "Lapangan Kampung",         Chapter.Ch1),
            new LocationDef(LocationId.TokoKelontong,         Region.KampungCempaka, "Toko Kelontong",           Chapter.Ch1, "Penjaga toko"),
            new LocationDef(LocationId.WarungMakanBuSiti,     Region.KampungCempaka, "Warung Makan Bu Siti",     Chapter.Ch1, "Pelayan"),
            new LocationDef(LocationId.TempatBerkumpulPemuda, Region.KampungCempaka, "Tempat Berkumpul Pemuda",  Chapter.Ch1),
            new LocationDef(LocationId.KebunWarga,            Region.KampungCempaka, "Kebun Warga",              Chapter.Ch1, "Tukang kebun"),
            new LocationDef(LocationId.PeternakanKecil,       Region.KampungCempaka, "Peternakan Kecil",         Chapter.Ch2, "Peternak"),
            new LocationDef(LocationId.GangMelati,            Region.KampungCempaka, "Gang Melati",              Chapter.Ch3),

            // ---- Pasar Ramai Jaya ----
            new LocationDef(LocationId.PasarTradisional,   Region.PasarRamaiJaya, "Pasar Tradisional",    Chapter.Ch1, "Pedagang, Penjaga kios"),
            new LocationDef(LocationId.WarungBuSari,       Region.PasarRamaiJaya, "Warung Bu Sari",       Chapter.Ch1, "Penjaga warung"),
            new LocationDef(LocationId.KiosSayur,          Region.PasarRamaiJaya, "Kios Sayuran",         Chapter.Ch1),
            new LocationDef(LocationId.KiosBuah,           Region.PasarRamaiJaya, "Kios Buah",            Chapter.Ch1),
            new LocationDef(LocationId.TokoPakaian,        Region.PasarRamaiJaya, "Toko Pakaian",         Chapter.Ch1, "Pelayan toko"),
            new LocationDef(LocationId.TokoKebutuhanRumah, Region.PasarRamaiJaya, "Toko Kebutuhan Rumah", Chapter.Ch1),

            // ---- Pantai Cemara ----
            new LocationDef(LocationId.PantaiCemara, Region.PantaiCemara, "Pantai Cemara", Chapter.Ch2, "Nelayan"),

            // ---- Penginapan: tempat tidur berbayar di setiap wilayah ----
            new LocationDef(LocationId.PenginapanDesa,    Region.DesaSukaMakmur, "Penginapan Desa Asri",     Chapter.Prolog),
            new LocationDef(LocationId.PenginapanKota,    Region.KotaJogokerto,  "Penginapan Sejahtera",     Chapter.Ch1),
            new LocationDef(LocationId.PenginapanCempaka, Region.KampungCempaka, "Penginapan Cempaka",       Chapter.Ch1),
            new LocationDef(LocationId.PenginapanPasar,   Region.PasarRamaiJaya, "Losmen Ramai Jaya",        Chapter.Ch1),
            new LocationDef(LocationId.PenginapanPantai,  Region.PantaiCemara,   "Penginapan Pantai Cemara", Chapter.Ch2),
        };

        /// <summary>Tarif semalam di penginapan (energi pulih penuh, bangun pukul 06.00).</summary>
        public static readonly Dictionary<LocationId, int> InnPrice = new Dictionary<LocationId, int>
        {
            { LocationId.PenginapanDesa,    40000 },
            { LocationId.PenginapanKota,    75000 },
            { LocationId.PenginapanCempaka, 50000 },
            { LocationId.PenginapanPasar,   60000 },
            { LocationId.PenginapanPantai,  90000 },
        };

        public static bool IsInn(LocationId id) => InnPrice.ContainsKey(id);

        /// <summary>Tarif bus dari halaman "Sistem pilihan Maps" (dari Terminal Jogokerto).</summary>
        public static readonly Dictionary<Region, int> BusFare = new Dictionary<Region, int>
        {
            { Region.DesaSukaMakmur, 5000 },
            { Region.KampungCempaka, 4000 },
            { Region.PasarRamaiJaya, 4000 },
            { Region.PantaiCemara,   4000 },
            { Region.KotaJogokerto,  4000 }, // arah balik; dokumen tidak menyebut, disamakan
        };

        public static readonly Dictionary<Region, LocationId> ArrivalPoint = new Dictionary<Region, LocationId>
        {
            { Region.DesaSukaMakmur, LocationId.TerminalDesa },
            { Region.KotaJogokerto,  LocationId.TerminalJogokerto },
            { Region.KampungCempaka, LocationId.TerminalCempaka },
            { Region.PasarRamaiJaya, LocationId.PasarTradisional },
            { Region.PantaiCemara,   LocationId.PantaiCemara },
        };

        static Dictionary<LocationId, LocationDef> _byId;

        public static LocationDef Get(LocationId id)
        {
            if (_byId == null)
            {
                _byId = new Dictionary<LocationId, LocationDef>();
                foreach (var l in Locations) _byId[l.Id] = l;
            }
            return _byId[id];
        }

        public static bool IsOpen(LocationId id, Chapter current) => current >= Get(id).OpensAt;

        /// <summary>
        /// Aturan perjalanan. Desa Suka Makmur sengaja dikunci sejak berangkat merantau
        /// sampai kabar duka (Chapter 6): ini inti cerita, pulang terus "ditunda".
        /// </summary>
        public static bool CanTravelTo(Region target, Chapter current, int money, out string reason)
        {
            reason = null;
            if (target == Region.DesaSukaMakmur)
            {
                if (current >= Chapter.Ch1 && current < Chapter.Ch6)
                {
                    reason = PulangProfile.Format("{nama}: \"Nanti kalau sudah sukses, aku pulang.\"");
                    return false;
                }
                if (current >= Chapter.Ch6) return true; // perjalanan terakhir tidak boleh gagal karena uang
            }
            if (Get(ArrivalPoint[target]).OpensAt > current)
            {
                reason = "Tempat ini belum bisa dikunjungi.";
                return false;
            }
            if (money < BusFare[target])
            {
                reason = "Uangmu tidak cukup untuk membeli tiket."; // dokumen: tidak bisa pindah jika uang kurang
                return false;
            }
            return true;
        }
    }

    // =====================================================================
    //  TOKOH
    // =====================================================================

    public enum NpcId
    {
        // Keluarga
        Ratna, NpcKeluarga,

        // NPC pendukung (Bagian 3)
        PakHendra, Dimas, BuSari, PakJoko, Andi, PakBudi, Rina, PakDarto, Riko,

        // NPC random per wilayah (bagian Pekerjaan / NPC Random)
        PakSastro, PakRT, PakHerman, PakSlamet, MasFajar, PakKarman, BuRukmini,
        PakHadi, PakRudi, PakSurya, BuSiti, PenjagaPerpustakaan, TukangKebun,

        // Lima karakter perempuan
        Nabila, Kirana, Anindya, Melati, Citra,
    }

    public struct NpcDef
    {
        public NpcId Id;
        public string Name;
        public string Role;
        public LocationId Home;

        public NpcDef(NpcId id, string name, string role, LocationId home)
        {
            Id = id; Name = name; Role = role; Home = home;
        }
    }

    public static class PulangCast
    {
        public static readonly NpcDef[] All =
        {
            new NpcDef(NpcId.Ratna,       "Ratna",        "Ibu {nama}",                  LocationId.RumahZidan),
            new NpcDef(NpcId.NpcKeluarga, "Keluarga",     "Penelepon kabar duka (Ch6)",  LocationId.RumahZidan),

            new NpcDef(NpcId.PakHendra, "Pak Hendra", "Memperkenalkan kota",           LocationId.TerminalJogokerto),
            new NpcDef(NpcId.Dimas,     "Dimas",      "Teman pertama di Jogokerto",    LocationId.KostPelangi),
            new NpcDef(NpcId.BuSari,    "Bu Sari",    "Pemilik Warung Bu Sari",        LocationId.WarungBuSari),
            new NpcDef(NpcId.PakJoko,   "Pak Joko",   "Pemilik Bengkel Jaya Motor",    LocationId.BengkelJayaMotor),
            new NpcDef(NpcId.Andi,      "Pak Andi",   "Petugas Rumah Sakit",           LocationId.RumahSakitSehat),
            new NpcDef(NpcId.PakBudi,   "Pak Budi",   "Pekerjaan & pengembangan kemampuan", LocationId.PusatKota),
            new NpcDef(NpcId.Rina,      "Rina",       "Aktivitas & misi kota",         LocationId.AlunAlunMerdeka),
            new NpcDef(NpcId.PakDarto,  "Pak Darto",  "Pedagang pasar",                LocationId.PasarTradisional),
            new NpcDef(NpcId.Riko,      "Mas Riko",   "Pegawai Cafe Senja",            LocationId.CafeSenja),

            new NpcDef(NpcId.PakSastro,  "Pak Sastro",  "Petani",                   LocationId.Sawah),
            new NpcDef(NpcId.PakRT,      "Pak RT",      "Ketua RT",                 LocationId.BalaiDesa),
            new NpcDef(NpcId.PakHerman,  "Pak Herman",  "Pemilik Toko Anugerah",    LocationId.TokoAnugerah),
            new NpcDef(NpcId.PakSlamet,  "Pak Slamet",  "Penjaga Pos Ronda",        LocationId.PosRondaCempaka),
            new NpcDef(NpcId.MasFajar,   "Mas Fajar",   "Pemuda kampung",           LocationId.LapanganKampung),
            new NpcDef(NpcId.PakKarman,  "Pak Karman",  "Peternak",                 LocationId.PeternakanKecil),
            new NpcDef(NpcId.BuRukmini,  "Bu Rukmini",  "Pemilik Toko Kelontong",   LocationId.TokoKelontong),
            new NpcDef(NpcId.PakHadi,    "Pak Hadi",    "Petugas loket terminal",   LocationId.TerminalJogokerto),
            new NpcDef(NpcId.PakRudi,    "Pak Rudi",    "Pedagang sayur",           LocationId.KiosSayur),
            new NpcDef(NpcId.PakSurya,   "Pak Surya",   "Petugas kebersihan",       LocationId.AlunAlunMerdeka),
            new NpcDef(NpcId.BuSiti,     "Bu Siti",     "Pemilik warung makan",     LocationId.WarungMakanBuSiti),
            new NpcDef(NpcId.PenjagaPerpustakaan, "Penjaga Perpustakaan", "Side quest buku", LocationId.Perpustakaan),
            new NpcDef(NpcId.TukangKebun, "Tukang Kebun", "Side quest taman kota", LocationId.TamanPelangi),

            // Lokasi awal = Bagian 2 dokumen
            new NpcDef(NpcId.Nabila,  "Nabila",  "Ceria, ramah, suka membantu",      LocationId.TamanPelangi),
            new NpcDef(NpcId.Kirana,  "Kirana",  "Tenang, suka membaca, cerdas",     LocationId.Perpustakaan),
            new NpcDef(NpcId.Anindya, "Anindya", "Ramah, humoris, mudah bergaul",    LocationId.CafeSenja),
            new NpcDef(NpcId.Melati,  "Melati",  "Pendiam, perhatian, sulit terbuka", LocationId.GangMelati),
            new NpcDef(NpcId.Citra,   "Citra",   "Kreatif, suka menjelajah",         LocationId.AlunAlunMerdeka),
        };
    }

    // =====================================================================
    //  PDKT LIMA KARAKTER — tanpa ending pasangan
    // =====================================================================

    public enum Gift { Cokelat, Buku, Bunga, Roti, Kopi }
    public enum GiftTaste { TidakSuka = -1, Biasa = 0, Suka = 1, SangatSuka = 2 }

    public class GirlDef
    {
        public NpcId Id;
        public Chapter Unlocks;            // bab saat ceritanya dibuka (Bab 1-3 dokumen)
        public GiftTaste[] Taste;          // urutan sama dengan enum Gift
        public string[] Episodes;          // 10 episode
    }

    public static class PulangRomance
    {
        public const int EpisodeCount = 10;

        static readonly GiftTaste B = GiftTaste.Biasa, S = GiftTaste.Suka, SS = GiftTaste.SangatSuka;

        public static readonly GirlDef[] Girls =
        {
            new GirlDef { Id = NpcId.Nabila, Unlocks = Chapter.Ch1, Taste = new[] { SS, B, S, SS, S },
                Episodes = new[] { "Pertemuan Pertama", "Mulai Mengenal", "Percakapan Sore", "Membantu di Warung",
                    "Hadiah Kecil", "Jalan Sore", "Cerita Pribadi", "Menghabiskan Waktu Bersama",
                    "Mengungkapkan Perasaan", "Awal Kisah Baru" } },
            new GirlDef { Id = NpcId.Kirana, Unlocks = Chapter.Ch2, Taste = new[] { S, SS, B, B, S },
                Episodes = new[] { "Bertemu di Perpustakaan", "Membaca Bersama", "Membicarakan Impian", "Belajar Bersama",
                    "Hadiah Buku", "Jalan ke Taman", "Cerita Masa Lalu", "Hari yang Menyenangkan",
                    "Perasaan yang Mulai Tumbuh", "Sebuah Janji" } },
            new GirlDef { Id = NpcId.Anindya, Unlocks = Chapter.Ch2, Taste = new[] { B, S, S, S, S },
                Episodes = new[] { "Pertemuan di Lapangan", "Kegiatan Bersama", "Tantangan Kecil", "Membantu Acara Desa",
                    "Hadiah untuk Panitia", "Berbelanja Bersama", "Sisi Lain Anindya", "Hari Festival",
                    "Percakapan Setelah Festival", "Langkah Bersama" } },
            new GirlDef { Id = NpcId.Melati, Unlocks = Chapter.Ch3, Taste = new[] { S, B, SS, S, B },
                Episodes = new[] { "Pertemuan di Taman", "Buku Catatan", "Menemani Melati", "Membantu Menulis Cerita",
                    "Hadiah Buku Harian", "Mengunjungi Rumah Melati", "Rahasia Kecil", "Hari di Taman Bunga",
                    "Isi Buku Catatan", "Cerita yang Belum Selesai" } },
            // Dokumen belum mencantumkan kesukaan Citra: sementara semua "Biasa".
            new GirlDef { Id = NpcId.Citra, Unlocks = Chapter.Ch3, Taste = new[] { B, B, B, B, B },
                Episodes = new[] { "Pertemuan yang Tidak Terduga", "Bercanda Bersama", "Ajakan Jalan-Jalan",
                    "Misi Mencari Jajanan", "Hadiah Kejutan", "Foto Bersama", "Citra yang Sebenarnya",
                    "Hari Spesial", "Pertanyaan Citra", "Kenangan Bersama" } },
        };

        public static int GiftCloseness(GiftTaste t)
        {
            switch (t)
            {
                case GiftTaste.SangatSuka: return 8;
                case GiftTaste.Suka: return 5;
                case GiftTaste.TidakSuka: return -2;
                default: return 2;
            }
        }

        public static string GiftReaction(GiftTaste t)
        {
            switch (t)
            {
                case GiftTaste.SangatSuka:
                case GiftTaste.Suka: return "Wah! Ini kesukaanku. Terima kasih, {nama}!";
                case GiftTaste.TidakSuka: return "Terima kasih. Tapi sebenarnya aku kurang suka barang seperti ini.";
                default: return "Terima kasih, {nama}. Kamu baik sekali.";
            }
        }
    }
}
