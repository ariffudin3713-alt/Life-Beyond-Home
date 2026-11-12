using System.Collections.Generic;

namespace PixelAdventure.LifeSim.Pulang
{
    public class JobDef
    {
        public string Id, Name;
        public JobTier Tier;
        public int MinLevel, WageMin, WageMax;
        public int Start = -1, End = -1;      // menit; -1 = fleksibel
        public LocationId Place;
        public string NeedItem;               // barang yang harus dimiliki (sepeda, kamera, ...)
        public string Unit = "hari";          // "hari", "pesanan", "proyek", "tangkapan"

        public bool Flexible => Start < 0;
        public bool NeedsSkill => Tier != JobTier.Sederhana;   // "harus membeli skill pekerjaan"

    }

    /// <summary>Kategori barang di inventori.</summary>
    public enum ItemKind { Makanan, Hadiah, Perlengkapan, Misi }

    public class ItemDef
    {
        public string Id, Name;
        public int Price;
        public int Energy;          // makanan/minuman
        public Gift? Gift;          // bisa jadi hadiah
        public LocationId[] SoldAt = new LocationId[0];
        public string Desc = "";    // keterangan di inventori
        public bool Quest;          // barang misi: tidak dijual, tidak bisa dimakan/dibuang

        public ItemKind Kind =>
            Quest ? ItemKind.Misi
            : Energy > 0 ? ItemKind.Makanan
            : Gift != null ? ItemKind.Hadiah
            : ItemKind.Perlengkapan;
    }

    /// <summary>Data pekerjaan dari bagian "RULES PEKERJAAN" dan "Jam masuk kerja".</summary>
    public static class PulangJobs
    {
        static int H(int hour) => hour * 60;

        public static readonly JobDef[] All =
        {
            // 1. Pekerjaan sederhana
            new JobDef { Id = "penjaga_warung", Name = "Penjaga Warung", Tier = JobTier.Sederhana, MinLevel = 1,
                WageMin = 30000, WageMax = 50000, Start = H(7), End = H(15), Place = LocationId.WarungBuSari },
            new JobDef { Id = "kurir_makanan", Name = "Kurir Makanan", Tier = JobTier.Sederhana, MinLevel = 2,
                WageMin = 10000, WageMax = 25000, Start = H(8), End = H(17), Place = LocationId.Minimarket,
                NeedItem = "sepeda", Unit = "pesanan" },
            new JobDef { Id = "pelayan_kafe", Name = "Pelayan Kafe", Tier = JobTier.Sederhana, MinLevel = 2,
                WageMin = 60000, WageMax = 120000, Start = H(10), End = H(18), Place = LocationId.CafeSenja },
            new JobDef { Id = "tukang_kebun", Name = "Tukang Kebun", Tier = JobTier.Sederhana, MinLevel = 1,
                WageMin = 40000, WageMax = 80000, Start = H(6), End = H(12), Place = LocationId.KebunWarga },

            // 2. Pekerjaan keterampilan
            new JobDef { Id = "bengkel", Name = "Montir Motor", Tier = JobTier.Keterampilan, MinLevel = 5,
                WageMin = 75000, WageMax = 200000, Start = H(8), End = H(17), Place = LocationId.BengkelJayaMotor },
            new JobDef { Id = "fotografer", Name = "Fotografer", Tier = JobTier.Keterampilan, MinLevel = 5,
                WageMin = 50000, WageMax = 150000, Place = LocationId.AlunAlunMerdeka,
                NeedItem = "kamera", Unit = "pesanan" },
            new JobDef { Id = "tukang_cukur", Name = "Tukang Cukur", Tier = JobTier.Keterampilan, MinLevel = 5,
                WageMin = 60000, WageMax = 150000, Start = H(9), End = H(18), Place = LocationId.PusatKota },

            // 3. Pekerjaan alam
            new JobDef { Id = "nelayan", Name = "Nelayan", Tier = JobTier.Alam, MinLevel = 2,
                WageMin = 20000, WageMax = 200000, Start = H(5), End = H(11), Place = LocationId.PantaiCemara,
                NeedItem = "alat_pancing", Unit = "tangkapan" },
            new JobDef { Id = "petani", Name = "Petani", Tier = JobTier.Alam, MinLevel = 2,
                WageMin = 40000, WageMax = 80000, Start = H(6), End = H(12), Place = LocationId.KebunWarga },

            // 4. Pekerjaan profesional
            new JobDef { Id = "koki_restoran", Name = "Koki Restoran", Tier = JobTier.Profesional, MinLevel = 8,
                WageMin = 100000, WageMax = 220000, Start = H(10), End = H(18), Place = LocationId.CafeSenja },
            new JobDef { Id = "pegawai_kantor", Name = "Pegawai Kantor", Tier = JobTier.Profesional, MinLevel = 8,
                WageMin = 100000, WageMax = 180000, Start = H(8), End = H(16), Place = LocationId.PTMajuBersama },
            new JobDef { Id = "desainer_grafis", Name = "Desainer Grafis", Tier = JobTier.Profesional, MinLevel = 9,
                WageMin = 100000, WageMax = 300000, Start = H(9), End = H(17), Place = LocationId.PTMajuBersama,
                Unit = "proyek" },
            new JobDef { Id = "guru", Name = "Guru", Tier = JobTier.Profesional, MinLevel = 10,
                WageMin = 150000, WageMax = 250000, Start = H(7), End = H(14), Place = LocationId.Sekolah },
            new JobDef { Id = "perawat", Name = "Perawat", Tier = JobTier.Profesional, MinLevel = 12,
                WageMin = 150000, WageMax = 250000, Start = H(8), End = H(16), Place = LocationId.RumahSakitSehat },
            new JobDef { Id = "polisi", Name = "Polisi", Tier = JobTier.Profesional, MinLevel = 12,
                WageMin = 180000, WageMax = 300000, Start = H(8), End = H(16), Place = LocationId.PusatKota },
            new JobDef { Id = "dokter", Name = "Dokter", Tier = JobTier.Profesional, MinLevel = 18,
                WageMin = 250000, WageMax = 500000, Start = H(8), End = H(16), Place = LocationId.RumahSakitSehat },
        };

        public static JobDef Get(string id)
        {
            foreach (var j in All) if (j.Id == id) return j;
            return null;
        }

        /// <summary>Quest 4 / Chapter 1: hanya pekerjaan awal yang ditawarkan Dimas.</summary>
        public static bool IsStarterJob(string id) =>
            id == "penjaga_warung" || id == "tukang_kebun" || id == "pelayan_kafe" || id == "kurir_makanan";

        public static int ExpForNextLevel(int level) => 50 * level;

        // Aturan keterlambatan (dokumen): menit -> potongan upah
        public static float LatePenalty(int lateMinutes, out string note)
        {
            note = null;
            if (lateMinutes <= 0) return 0f;
            if (lateMinutes <= 10) { note = "Teguran ringan karena terlambat."; return 0f; }
            if (lateMinutes <= 30) { note = "Terlambat: bonus dikurangi."; return 0.1f; }
            if (lateMinutes <= 60) { note = "Peringatan HRD: terlambat lebih dari 30 menit."; return 0.2f; }
            note = "Catatan pelanggaran: terlambat lebih dari 1 jam.";
            return 0.3f;
        }

        // Reward kehadiran (dokumen): 5 / 10 / 20 hari tepat waktu
        public static int AttendanceBonus(int onTimeStreak, out string note)
        {
            note = null;
            if (onTimeStreak == 5) { note = "5 hari tepat waktu: bonus kecil."; return 10000; }
            if (onTimeStreak == 10) { note = "10 hari tepat waktu: bonus tambahan."; return 25000; }
            if (onTimeStreak == 20) { note = "20 hari tepat waktu: penghargaan karyawan disiplin!"; return 50000; }
            return 0;
        }
    }

    public static class PulangItems
    {
        public static readonly ItemDef[] All =
        {
            new ItemDef { Id = "roti", Name = "Roti", Price = 5000, Energy = 15, Gift = Pulang.Gift.Roti,
                SoldAt = new[] { LocationId.WarungBuSari, LocationId.Minimarket, LocationId.TokoKelontong },
                Desc = "Roti manis untuk bekal. Bisa dimakan kapan saja atau diberikan sebagai hadiah." },
            new ItemDef { Id = "es_teh", Name = "Es Teh", Price = 5000, Energy = 8,
                SoldAt = new[] { LocationId.WarungSenja, LocationId.WarungBuSari, LocationId.WarungMakanBuSiti },
                Desc = "Segelas es teh manis yang menyegarkan." },
            new ItemDef { Id = "nasi_goreng", Name = "Nasi Goreng", Price = 15000, Energy = 30,
                SoldAt = new[] { LocationId.WarungMakanBuSiti, LocationId.WarungBuSari },
                Desc = "Sepiring nasi goreng hangat. Mengenyangkan." },
            new ItemDef { Id = "kopi", Name = "Kopi", Price = 8000, Energy = 10, Gift = Pulang.Gift.Kopi,
                SoldAt = new[] { LocationId.CafeSenja, LocationId.Minimarket },
                Desc = "Kopi hangat penambah semangat. Bisa juga jadi hadiah." },
            new ItemDef { Id = "cokelat", Name = "Cokelat", Price = 10000, Gift = Pulang.Gift.Cokelat,
                SoldAt = new[] { LocationId.Minimarket, LocationId.TokoSerbaAda, LocationId.TokoKelontong },
                Desc = "Sebatang cokelat. Hadiah manis untuk orang terdekat." },
            new ItemDef { Id = "buku", Name = "Buku", Price = 25000, Gift = Pulang.Gift.Buku,
                SoldAt = new[] { LocationId.TokoSerbaAda, LocationId.TokoAnugerah },
                Desc = "Buku cerita yang bagus. Cocok untuk yang suka membaca." },
            new ItemDef { Id = "bunga", Name = "Bunga", Price = 15000, Gift = Pulang.Gift.Bunga,
                SoldAt = new[] { LocationId.TokoBungaMelati },
                Desc = "Seikat bunga segar dari Toko Bunga Melati." },
            // Perlengkapan kerja (harga belum ada di dokumen)
            new ItemDef { Id = "sepeda", Name = "Sepeda", Price = 300000, SoldAt = new[] { LocationId.TokoSerbaAda },
                Desc = "Syarat pekerjaan Kurir Makanan di Minimarket." },
            new ItemDef { Id = "kamera", Name = "Kamera", Price = 500000, SoldAt = new[] { LocationId.TokoAnugerah },
                Desc = "Syarat pekerjaan Fotografer di Alun-Alun Merdeka." },
            new ItemDef { Id = "alat_pancing", Name = "Alat Pancing", Price = 100000,
                SoldAt = new[] { LocationId.TokoKebutuhanRumah },
                Desc = "Syarat pekerjaan Nelayan di Pantai Cemara." },

            // ---- Barang misi (didapat dari cerita, dipakai untuk menyelesaikan misi)
            new ItemDef { Id = "tas", Name = "Tas Ransel", Quest = true,
                Desc = "Tas untuk merantau. Wajib dibawa sebelum berangkat ke Jogokerto." },
            new ItemDef { Id = "barang_pribadi", Name = "Barang Pribadi", Quest = true,
                Desc = "Pakaian dan barang kenangan dari rumah. Wajib dibawa sebelum berangkat." },
            new ItemDef { Id = "kunci_kost", Name = "Kunci Kost Pelangi", Quest = true,
                Desc = "Kunci kamarmu. Dipakai untuk tidur, menabung, dan menelepon Ibu di kost." },
            new ItemDef { Id = "buku_catatan_nabila", Name = "Buku Catatan Kecil", Quest = true,
                Desc = "Buku catatan milik Nabila yang tertinggal. Kembalikan kepadanya di Taman Pelangi." },
            new ItemDef { Id = "paket_dimas", Name = "Paket Titipan Dimas", Quest = true,
                Desc = "Antar ke Pak Herman, Bu Rukmini, dan Pak Hadi. Satu paket untuk satu orang." },
            new ItemDef { Id = "barang_pasar", Name = "Barang Dagangan", Quest = true,
                Desc = "Titipan Bu Sari. Antar ke Pak Darto, Pak Rudi, dan Bu Siti." },
            new ItemDef { Id = "surat_hendra", Name = "Surat Pak Hendra", Quest = true,
                Desc = "Surat dari Pak Hendra untuk Pak Slamet di Pos Ronda Kampung Cempaka." },
            new ItemDef { Id = "oleh_oleh", Name = "Kain Batik untuk Ibu", Price = 60000, Quest = true,
                SoldAt = new[] { LocationId.TokoPakaian },
                Desc = "Kain batik cantik untuk dikirim kepada Ibu lewat Pak Hadi di terminal." },
            new ItemDef { Id = "barang_bawaan", Name = "Barang Bawaan", Quest = true,
                Desc = "Semua barangmu dari kost. Wajib dibawa untuk pulang ke Desa Suka Makmur." },
        };

        public static ItemDef Get(string id)
        {
            foreach (var i in All) if (i.Id == id) return i;
            return null;
        }

        public static IEnumerable<ItemDef> SoldAt(LocationId loc)
        {
            foreach (var i in All)
                foreach (var l in i.SoldAt)
                    if (l == loc) { yield return i; break; }
        }
    }

    /// <summary>Kunci sprite dan sapaan NPC (dialog "NPC Random" di dokumen).</summary>
    public static class PulangNpcLines
    {
        public static readonly Dictionary<NpcId, string> Sprite = new Dictionary<NpcId, string>
        {
            { NpcId.Ratna, "ratna" }, { NpcId.NpcKeluarga, "keluarga" },
            { NpcId.PakHendra, "hendra" }, { NpcId.Dimas, "dimas" }, { NpcId.BuSari, "busari" },
            { NpcId.PakJoko, "joko" }, { NpcId.Andi, "andi" }, { NpcId.PakBudi, "budi" },
            { NpcId.Rina, "rina" }, { NpcId.PakDarto, "darto" }, { NpcId.Riko, "riko" },
            { NpcId.PakSastro, "sastro" }, { NpcId.PakRT, "rt" }, { NpcId.PakHerman, "herman" },
            { NpcId.PakSlamet, "slamet" }, { NpcId.MasFajar, "fajar" }, { NpcId.PakKarman, "karman" },
            { NpcId.BuRukmini, "rukmini" }, { NpcId.PakHadi, "hadi" }, { NpcId.PakRudi, "rudi" },
            { NpcId.PakSurya, "surya" }, { NpcId.BuSiti, "siti" },
            { NpcId.PenjagaPerpustakaan, "pustakawan" }, { NpcId.TukangKebun, "tukangkebun" },
            { NpcId.Nabila, "nabila" }, { NpcId.Kirana, "kirana" }, { NpcId.Anindya, "anindya" },
            { NpcId.Melati, "melati" }, { NpcId.Citra, "citra" },
        };

        /// <summary>Kalimat pertama + balasan dari dokumen. Dipakai bila tidak ada adegan cerita.</summary>
        public static readonly Dictionary<NpcId, string[]> Greeting = new Dictionary<NpcId, string[]>
        {
            { NpcId.PakSastro, new[] { "{nama}! Pagi-pagi sudah keluar rumah. Mau ke mana?",
                "Kalau mau bekerja, coba bantu orang-orang di sawah. Lumayan buat menambah uang saku." } },
            { NpcId.PakRT, new[] { "Anak muda sekarang harus rajin. Jangan hanya mengandalkan orang tua.",
                "Kalau ada waktu, bantu kegiatan desa. Siapa tahu kamu mendapatkan kenalan baru." } },
            { NpcId.PakJoko, new[] { "Selamat datang di bengkel saya. Motor warga di sini sering membutuhkan perbaikan.",
                "Kalau memang serius, saya bisa mengajarimu dari dasar." } },
            { NpcId.BuSari, new[] { "{nama}, mau makan dulu? Dari tadi kelihatannya sibuk sekali.",
                "Kalau kamu rajin dan bisa dipercaya, nanti kita bicarakan soal pekerjaan." } },
            { NpcId.PakBudi, new[] { "Selamat pagi! Kamu pendatang baru, ya?",
                "Kalau begitu, coba keliling kota. Banyak tempat usaha yang membutuhkan pegawai." } },
            { NpcId.PakHerman, new[] { "Selamat datang di toko saya. Ada yang bisa dibantu?",
                "Kebetulan saya sedang membutuhkan pegawai. Kalau berminat, datanglah besok pagi." } },
            { NpcId.PakDarto, new[] { "Wah, {nama}! Mau belanja atau sekadar melihat-lihat pasar?",
                "Pasar ini selalu ramai. Kalau kamu rajin, banyak pedagang yang membutuhkan bantuan." } },
            { NpcId.Riko, new[] { "Hei, kamu baru di sini? Saya sering melihat orang baru datang ke kota ini.",
                "Kerja di sini menyenangkan, meskipun kadang pelanggan sangat ramai." } },
            { NpcId.Andi, new[] { "Permisi, jangan berlari di dalam rumah sakit. Banyak pasien sedang beristirahat.",
                "Hampir setiap hari ramai. Banyak warga datang untuk mendapatkan pelayanan kesehatan." } },
            { NpcId.PakSlamet, new[] { "{nama}! Sudah malam, kok masih keluyuran?",
                "Jangan terlalu malam. Istirahat juga penting supaya besok bisa bekerja lagi." } },
            { NpcId.MasFajar, new[] { "Eh, {nama}! Besok ada pertandingan sepak bola di lapangan. Ikut, tidak?",
                "Kalau berubah pikiran, langsung datang saja!" } },
            { NpcId.PakKarman, new[] { "Kalau memelihara hewan itu harus sabar. Tidak bisa hanya memikirkan hasilnya.",
                "Hampir sepuluh tahun. Banyak pengalaman yang saya dapat selama bekerja." } },
            { NpcId.BuRukmini, new[] { "Silakan, Nak. Kalau mencari kebutuhan sehari-hari, semuanya ada di sini.",
                "Kalau kamu mau membantu, saya bisa memberikan pekerjaan paruh waktu." } },
            { NpcId.PakHadi, new[] { "Selamat datang! Mau bepergian ke mana hari ini?",
                "Silakan periksa jadwal di papan informasi." } },
            { NpcId.PakRudi, new[] { "Sayurnya masih segar, Nak! Baru datang dari petani.",
                "Kalau pagi begini memang ramai. Banyak warga datang untuk berbelanja." } },
            { NpcId.PakSurya, new[] { "Tempat umum harus dijaga bersama. Jangan membuang sampah sembarangan.",
                "Saya ingin tempat ini tetap nyaman untuk semua orang." } },
            { NpcId.BuSiti, new[] { "Masih banyak makanan, Nak. Mau pesan apa?",
                "Sudah hampir sepuluh tahun. Banyak pelanggan tetap di sini." } },
            { NpcId.PakHendra, new[] { "Kalau begitu, selamat datang di Jogokerto.",
                "Kamu perlu mengenal lingkungan kota dahulu." } },
            { NpcId.Dimas, new[] { "Kalau butuh bantuan, bilang saja.",
                "Banyak kesempatan di sini, asal kamu mau berusaha." } },
            { NpcId.Rina, new[] { "Hai, {nama}! Kota sedang ramai kegiatan hari ini.",
                "Kalau ada waktu, ikut bantu kegiatan kota, ya." } },
            { NpcId.PenjagaPerpustakaan, new[] { "Selamat datang di perpustakaan. Silakan membaca dengan tenang.",
                "Di sini kamu juga bisa mengikuti kursus keterampilan kerja." } },
            { NpcId.TukangKebun, new[] { "Tanaman di taman kota ini harus disiram setiap pagi.",
                "Kalau mau membantu, kebun warga sering membutuhkan tenaga." } },
        };
    }
}
