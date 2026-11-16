using System;

namespace PixelAdventure.LifeSim.Pulang
{
    /// <summary>
    /// Satu langkah misi sampingan.
    /// - Talk   : bicara dengan NPC (Key = NpcId). Bisa menyerahkan barang (Consume x ConsumeCount)
    ///            dan/atau menerima barang (Give). Reply = jawaban NPC itu.
    /// - lainnya: kejadian biasa (Buy, Arrive, WorkShift, Litter, Save, Gift, ...) sebanyak Count.
    /// </summary>
    public struct SideStep
    {
        public string Text;
        public StoryEvent Event;
        public string Key;
        public int Count;
        public string Consume;
        public int ConsumeCount;
        public string Give;
        public string Reply;

        public static SideStep Talk(NpcId npc, string text, string reply = null,
            string consume = null, int consumeCount = 1, string give = null) =>
            new SideStep
            {
                Text = text, Event = StoryEvent.Talk, Key = npc.ToString(), Count = 1,
                Consume = consume, ConsumeCount = consumeCount, Give = give, Reply = reply,
            };

        public static SideStep Do(StoryEvent ev, string key, string text, int count = 1) =>
            new SideStep { Text = text, Event = ev, Key = key, Count = count };
    }

    public class SideQuestDef
    {
        public string Id, Title;
        public NpcId Giver;
        public Chapter MinChapter = Chapter.Ch1;
        public string[] Offer;        // permintaan pemberi misi ("{nama}" = tokoh utama)
        public string Thanks;         // ucapan saat misi diselesaikan
        public string GiveOnAccept;   // barang misi yang diberikan saat menerima
        public SideStep[] Steps;
        public int Money, Exp, Rep;
    }

    [Serializable]
    public class SideQuestProgress
    {
        public string id;
        public int step;
        public int count;
        public bool done;
    }

    /// <summary>
    /// Misi sampingan: bisa diambil kapan saja sejak tiba di kota, tidak mengunci atau
    /// menunggu misi utama. NPC pemberi misi ditandai "!" di atas kepalanya.
    /// </summary>
    public static class PulangSideQuests
    {
        public static readonly SideQuestDef[] All =
        {
            new SideQuestDef
            {
                Id = "sq_rantang", Title = "Rantang untuk Pos Ronda", Giver = NpcId.BuSiti,
                Offer = new[]
                {
                    "{nama}, kebetulan sekali. Ibu sudah masak untuk Pak Slamet yang jaga pos ronda nanti malam.",
                    "Bisa tolong antarkan rantang ini ke Pos Ronda? Kaki Ibu sedang pegal.",
                },
                GiveOnAccept = "rantang_siti",
                Steps = new[]
                {
                    SideStep.Talk(NpcId.PakSlamet, "Antar rantang Bu Siti kepada Pak Slamet",
                        "Wah, masakan Bu Siti! Malam ini ronda jadi semangat. Sampaikan terima kasih saya, ya.",
                        consume: "rantang_siti"),
                    SideStep.Talk(NpcId.BuSiti, "Kembali kepada Bu Siti"),
                },
                Thanks = "Sudah sampai? Terima kasih, Nak. Ini sedikit uang jajan untukmu.",
                Money = 30000, Exp = 40, Rep = 2,
            },

            new SideQuestDef
            {
                Id = "sq_kopi_ronda", Title = "Kopi Teman Ronda", Giver = NpcId.PakSlamet,
                Offer = new[]
                {
                    "Malam-malam jaga pos itu paling enak ditemani kopi.",
                    "Kalau kamu lewat warung atau kafe, belikan dua gelas kopi untuk kami, ya. Nanti saya ganti.",
                },
                Steps = new[]
                {
                    SideStep.Do(StoryEvent.Buy, "kopi", "Beli 2 kopi (Cafe Senja, Minimarket, atau Angkringan)", 2),
                    SideStep.Talk(NpcId.PakSlamet, "Berikan 2 kopi kepada Pak Slamet", consume: "kopi", consumeCount: 2),
                },
                Thanks = "Mantap! Ini uangnya, lebihnya untukmu. Hati-hati kalau pulang malam.",
                Money = 35000, Exp = 50, Rep = 3,
            },

            new SideQuestDef
            {
                Id = "sq_bersih_alun", Title = "Alun-Alun Bersih", Giver = NpcId.PakSurya,
                Offer = new[]
                {
                    "Setiap hari ada saja orang yang membuang sampah sembarangan.",
                    "Kalau kamu mau membantu, pungut lima sampah lalu buang ke tong sampah. Di wilayah mana saja boleh.",
                },
                Steps = new[]
                {
                    SideStep.Do(StoryEvent.Litter, "*", "Pungut sampah dan buang ke tong sampah", 5),
                    SideStep.Talk(NpcId.PakSurya, "Lapor kepada Pak Surya di Alun-Alun Merdeka"),
                },
                Thanks = "Kota ini butuh lebih banyak orang sepertimu. Terimalah ini sebagai tanda terima kasih.",
                Money = 40000, Exp = 60, Rep = 5,
            },

            new SideQuestDef
            {
                Id = "sq_buku_baru", Title = "Koleksi Buku Baru", Giver = NpcId.PenjagaPerpustakaan,
                Offer = new[]
                {
                    "Rak buku cerita kami sudah lama tidak bertambah.",
                    "Tolong belikan satu buku di Toko Buku Pelita. Perpustakaan akan mengganti lebih dari harganya.",
                },
                Steps = new[]
                {
                    SideStep.Do(StoryEvent.Buy, "buku", "Beli buku di Toko Buku Pelita"),
                    SideStep.Talk(NpcId.PenjagaPerpustakaan, "Serahkan buku ke Penjaga Perpustakaan", consume: "buku"),
                },
                Thanks = "Buku yang bagus! Anak-anak pasti senang. Ini penggantinya, ditambah sedikit terima kasih.",
                Money = 45000, Exp = 50, Rep = 2,
            },

            new SideQuestDef
            {
                Id = "sq_bunga_taman", Title = "Taman Berbunga", Giver = NpcId.TukangKebun,
                Offer = new[]
                {
                    "Bedeng bunga di Taman Pelangi masih kosong sebagian.",
                    "Bisa belikan dua ikat bunga di Toko Bunga Melati? Nanti saya tanam di sini.",
                },
                Steps = new[]
                {
                    SideStep.Do(StoryEvent.Buy, "bunga", "Beli 2 ikat bunga di Toko Bunga Melati", 2),
                    SideStep.Talk(NpcId.TukangKebun, "Berikan bunga kepada Tukang Kebun", consume: "bunga", consumeCount: 2),
                },
                Thanks = "Lihat, tamannya jadi lebih cerah! Ini untukmu, terima kasih sudah repot-repot.",
                Money = 50000, Exp = 50, Rep = 3,
            },

            new SideQuestDef
            {
                Id = "sq_suku_cadang", Title = "Suku Cadang untuk Terminal", Giver = NpcId.PakJoko,
                Offer = new[]
                {
                    "Pak Hadi di terminal memesan suku cadang untuk bus antarwilayah.",
                    "Bengkel sedang ramai. Tolong antarkan kotak ini kepadanya.",
                },
                GiveOnAccept = "suku_cadang",
                Steps = new[]
                {
                    SideStep.Talk(NpcId.PakHadi, "Antar suku cadang kepada Pak Hadi di Terminal Jogokerto",
                        "Ini dia yang ditunggu! Bus jurusan pantai bisa jalan lagi besok.", consume: "suku_cadang"),
                    SideStep.Talk(NpcId.PakJoko, "Kembali ke Bengkel Jaya Motor"),
                },
                Thanks = "Cepat juga kamu. Ini upah antarnya. Kalau mau belajar mesin, mampir saja.",
                Money = 40000, Exp = 50, Rep = 2,
            },

            new SideQuestDef
            {
                Id = "sq_sayur_kafe", Title = "Pesanan Sayur Cafe Senja", Giver = NpcId.Riko,
                Offer = new[]
                {
                    "Gawat, sayuran untuk menu salad hari ini belum datang.",
                    "Pak Rudi di Kios Sayuran sudah menyiapkan pesanannya. Bisa tolong ambilkan?",
                },
                Steps = new[]
                {
                    SideStep.Talk(NpcId.PakRudi, "Ambil pesanan sayur dari Pak Rudi",
                        "Pesanan Cafe Senja, ya? Ini, masih segar. Hati-hati membawanya.", give: "sayur_pesanan"),
                    SideStep.Talk(NpcId.Riko, "Antar sayur ke Riko di Cafe Senja", consume: "sayur_pesanan"),
                },
                Thanks = "Penyelamat! Menu salad bisa dijual lagi. Ini upahmu, dan kapan-kapan kopi gratis.",
                Money = 45000, Exp = 50, Rep = 2,
            },

            new SideQuestDef
            {
                Id = "sq_kabar_pantai", Title = "Kabar dari Pantai", Giver = NpcId.PakHadi, MinChapter = Chapter.Ch1,
                Offer = new[]
                {
                    "Banyak penumpang bertanya soal Pantai Cemara, tapi saya belum sempat ke sana.",
                    "Kalau kamu jalan-jalan ke pantai, ceritakan suasananya. Akan saya tulis di papan informasi.",
                },
                Steps = new[]
                {
                    SideStep.Do(StoryEvent.Arrive, LocationId.PantaiCemara.ToString(), "Kunjungi Pantai Cemara"),
                    SideStep.Talk(NpcId.PakHadi, "Ceritakan suasana pantai kepada Pak Hadi"),
                },
                Thanks = "Pasir putih dan es kelapa, ya? Bagus untuk papan info. Ini ongkos jalan-jalanmu.",
                Money = 30000, Exp = 60, Rep = 2,
            },

            new SideQuestDef
            {
                Id = "sq_roti_ternak", Title = "Bekal Pekerja Peternakan", Giver = NpcId.PakKarman,
                Offer = new[]
                {
                    "Pekerja di peternakan belum sempat sarapan dari subuh.",
                    "Tolong belikan tiga roti di warung atau toko kelontong, nanti saya ganti.",
                },
                Steps = new[]
                {
                    SideStep.Do(StoryEvent.Buy, "roti", "Beli 3 roti (Warung Bu Sari, Minimarket, Toko Kelontong)", 3),
                    SideStep.Talk(NpcId.PakKarman, "Berikan roti kepada Pak Karman", consume: "roti", consumeCount: 3),
                },
                Thanks = "Terima kasih, Nak. Pekerja jadi bertenaga lagi. Ini uangnya, lebihnya untukmu.",
                Money = 45000, Exp = 50, Rep = 3,
            },

            new SideQuestDef
            {
                Id = "sq_titipan_pasar", Title = "Titipan ke Pasar", Giver = NpcId.BuRukmini,
                Offer = new[]
                {
                    "Bu Sari di pasar memesan bumbu dapur dari toko saya.",
                    "Bisa antarkan bungkusan ini ke Warung Bu Sari di Pasar Ramai Jaya?",
                },
                GiveOnAccept = "titipan_rukmini",
                Steps = new[]
                {
                    SideStep.Talk(NpcId.BuSari, "Antar titipan Bu Rukmini kepada Bu Sari",
                        "Bumbu dari Bu Rukmini memang paling lengkap. Terima kasih, ya.", consume: "titipan_rukmini"),
                    SideStep.Talk(NpcId.BuRukmini, "Kembali ke Toko Kelontong"),
                },
                Thanks = "Sudah sampai? Syukurlah. Ini ongkos antarnya, Nak.",
                Money = 40000, Exp = 50, Rep = 2,
            },

            new SideQuestDef
            {
                Id = "sq_lomba", Title = "Panitia Lomba Kampung", Giver = NpcId.MasFajar,
                Offer = new[]
                {
                    "Kampung mau bikin lomba tarik tambang dan balap karung!",
                    "Bantu aku ajak orang kota ikut, ya. Dimas dan Rina pasti mau kalau kamu yang ajak.",
                },
                Steps = new[]
                {
                    SideStep.Talk(NpcId.Dimas, "Ajak Dimas ikut lomba", "Lomba balap karung? Siap, aku ikut!"),
                    SideStep.Talk(NpcId.Rina, "Ajak Rina ikut lomba", "Seru! Nanti aku ajak teman-teman kota juga."),
                    SideStep.Talk(NpcId.MasFajar, "Kabari Mas Fajar di Lapangan Kampung"),
                },
                Thanks = "Wah, pesertanya bakal ramai! Ini uang terima kasih dari kas pemuda.",
                Money = 35000, Exp = 70, Rep = 4,
            },

            new SideQuestDef
            {
                Id = "sq_kerja_keras", Title = "Etos Kerja", Giver = NpcId.PakBudi,
                Offer = new[]
                {
                    "Orang yang rajin bekerja selalu dicari perusahaan.",
                    "Buktikan dirimu: selesaikan tiga shift kerja, pekerjaan apa saja. Setelah itu temui saya lagi.",
                },
                Steps = new[]
                {
                    SideStep.Do(StoryEvent.WorkShift, "*", "Selesaikan 3 shift kerja (pekerjaan apa saja)", 3),
                    SideStep.Talk(NpcId.PakBudi, "Temui Pak Budi di Pusat Kota"),
                },
                Thanks = "Bagus sekali. Catatan kerjamu akan saya ingat. Ini bonus dari saya.",
                Money = 75000, Exp = 120, Rep = 3,
            },

            new SideQuestDef
            {
                Id = "sq_survei_acara", Title = "Survei Tempat Acara", Giver = NpcId.Rina,
                Offer = new[]
                {
                    "Kota mau mengadakan pentas seni, tapi kami belum memilih tempatnya.",
                    "Bisa tolong lihat Taman Pelangi dan Gang Melati? Ceritakan mana yang lebih cocok.",
                },
                Steps = new[]
                {
                    SideStep.Do(StoryEvent.Arrive, LocationId.TamanPelangi.ToString(), "Lihat Taman Pelangi"),
                    SideStep.Do(StoryEvent.Arrive, LocationId.GangMelati.ToString(), "Lihat Gang Melati (Kampung Cempaka)"),
                    SideStep.Talk(NpcId.Rina, "Laporkan kepada Rina di Alun-Alun Merdeka"),
                },
                Thanks = "Taman Pelangi untuk panggung, Gang Melati untuk bazar. Ide bagus! Ini honor survei.",
                Money = 35000, Exp = 60, Rep = 3,
            },

            new SideQuestDef
            {
                Id = "sq_obat_ternak", Title = "Obat untuk Peternakan", Giver = NpcId.Andi,
                Offer = new[]
                {
                    "Pak Karman di Kampung Cempaka minta vitamin untuk hewan ternaknya lewat rumah sakit.",
                    "Saya tidak bisa meninggalkan jaga. Tolong antarkan paket ini, ya.",
                },
                GiveOnAccept = "obat_ternak",
                Steps = new[]
                {
                    SideStep.Talk(NpcId.PakKarman, "Antar vitamin ternak kepada Pak Karman",
                        "Akhirnya datang! Kambing-kambing saya bisa sehat lagi.", consume: "obat_ternak"),
                    SideStep.Talk(NpcId.Andi, "Kembali ke Rumah Sakit Sehat"),
                },
                Thanks = "Terima kasih bantuannya. Ini sedikit uang dari bagian administrasi.",
                Money = 40000, Exp = 50, Rep = 3,
            },

            new SideQuestDef
            {
                Id = "sq_belajar_menabung", Title = "Belajar Menabung", Giver = NpcId.Dimas,
                Offer = new[]
                {
                    "Bro, aku baru sadar gajiku habis tanpa terasa.",
                    "Ayo sama-sama belajar menabung. Sisihkan uangmu ke tabungan di kost, lalu kita saling cerita.",
                },
                Steps = new[]
                {
                    SideStep.Do(StoryEvent.Save, "*", "Menabung di Kost Pelangi"),
                    SideStep.Talk(NpcId.Dimas, "Ceritakan kepada Dimas"),
                },
                Thanks = "Keren! Aku juga mulai menabung. Nih, aku traktir uang jajan sebagai hadiah semangat.",
                Money = 20000, Exp = 40, Rep = 1,
            },

            new SideQuestDef
            {
                Id = "sq_gaya_baru", Title = "Gaya Baru di Kota", Giver = NpcId.BuSari, MinChapter = Chapter.Ch1,
                Offer = new[]
                {
                    "{nama}, bajumu itu-itu saja dari kampung. Sekali-sekali ganti, dong.",
                    "Di Toko Pakaian dekat sini banyak baju bagus. Coba beli satu, lalu tunjukkan ke Ibu.",
                },
                Steps = new[]
                {
                    SideStep.Do(StoryEvent.Buy, "#pakaian", "Beli pakaian baru di Toko Pakaian (Pasar Ramai Jaya)"),
                    SideStep.Talk(NpcId.BuSari, "Tunjukkan pakaian baru kepada Bu Sari"),
                },
                Thanks = "Nah, begitu kan lebih gagah! Ini, Ibu tambahi untuk beli baju lagi.",
                Money = 25000, Exp = 40, Rep = 1,
            },
        };

        public static SideQuestDef Get(string id)
        {
            foreach (var q in All) if (q.Id == id) return q;
            return null;
        }
    }
}
