using System.Collections.Generic;

namespace PixelAdventure.LifeSim.Pulang
{
    /// <summary>Profil kontak di tablet: biodata singkat dan bahan obrolan telepon.</summary>
    public class ContactInfo
    {
        public string Age = "-";
        public string Hobby = "-";
        public string Bio = "";
        public string[] Call = new string[0];     // ucapan NPC saat ditelepon (dipilih bergiliran per hari)
    }

    /// <summary>
    /// Data aplikasi KONTAK, TELEPON, dan PESAN di tablet. {nama} = nama tokoh utama.
    /// </summary>
    public static class PulangContacts
    {
        static ContactInfo C(string age, string hobby, string bio, params string[] call) =>
            new ContactInfo { Age = age, Hobby = hobby, Bio = bio, Call = call };

        public static readonly Dictionary<NpcId, ContactInfo> Info = new Dictionary<NpcId, ContactInfo>
        {
            { NpcId.Ratna, C("48 tahun", "Memasak & merawat kebun",
                "Ibu {nama} di Desa Suka Makmur. Membesarkan {nama} seorang diri dan selalu menunggu kabar dari perantauan.",
                "Jangan lupa makan teratur ya, Nak.", "Ibu sudah masak sayur asem kesukaanmu, sayang kamu jauh.") },
            { NpcId.PakHendra, C("60 tahun", "Bercerita & minum kopi tubruk",
                "Orang pertama yang menyambut {nama} di Jogokerto. Dulu juga perantau dari desa, kini dihormati warga kota.",
                "Kota ini keras, tapi orang jujur selalu menemukan jalannya.", "Mampir ke rumah kalau sempat, kita ngopi.") },
            { NpcId.Dimas, C("22 tahun", "Main gitar & sepak bola",
                "Tetangga kamar di Kost Pelangi dan teman pertama {nama}. Ceria, suka bercanda, dan selalu siap membantu.",
                "Bro! Nanti malam main gitar di teras kost, yuk!", "Aku lagi cari kerja baru nih, doakan ya.") },
            { NpcId.BuSari, C("45 tahun", "Meracik bumbu warung",
                "Pemilik Warung Bu Sari di Pasar Ramai Jaya. Galak di luar, tapi sering memberi makan gratis untuk anak rantau.",
                "Hari ini pasar ramai sekali, warungku sampai kehabisan nasi.", "Kalau lapar, mampir saja. Jangan sungkan.") },
            { NpcId.PakJoko, C("50 tahun", "Mengutak-atik mesin",
                "Pemilik Bengkel Jaya Motor. Mekanik andal yang bisa mendiagnosis motor hanya dari suaranya.",
                "Motor yang dirawat itu awet. Jangan lupa ganti oli!", "Bengkel lagi banyak servis, kalau mau belajar datang saja.") },
            { NpcId.Andi, C("35 tahun", "Bersepeda pagi",
                "Petugas Rumah Sakit Sehat. Tegas soal kesehatan dan selalu mengingatkan warga untuk cukup istirahat.",
                "Jangan begadang terus, kesehatan itu mahal.", "Rumah sakit sedang sibuk, tapi semua terkendali.") },
            { NpcId.PakBudi, C("52 tahun", "Membaca koran bisnis",
                "Pengusaha di Pusat Kota yang suka membuka kesempatan kerja dan kursus bagi anak muda.",
                "Kemampuan itu investasi. Terus belajar, ya.", "Ada peluang usaha baru, nanti kita bicarakan.") },
            { NpcId.Rina, C("24 tahun", "Mengatur acara warga",
                "Aktivis muda Alun-Alun Merdeka. Sering mengajak warga ikut kegiatan sosial di kota.",
                "Minggu depan ada kerja bakti, ikut ya!", "Alun-alun lagi ramai pengunjung, seru deh.") },
            { NpcId.PakDarto, C("55 tahun", "Tawar-menawar",
                "Pedagang senior Pasar Ramai Jaya. Tahu harga semua barang di pasar di luar kepala.",
                "Harga cabai naik lagi, pembeli pada mengeluh.", "Pedagang itu modalnya kepercayaan, ingat itu.") },
            { NpcId.Riko, C("26 tahun", "Meracik kopi",
                "Barista Cafe Senja. Hafal pesanan setiap pelanggan tetap dan senang mencoba resep kopi baru.",
                "Ada menu kopi gula aren baru, cobain deh!", "Cafe lagi ramai anak kuliah, aku sampai kewalahan.") },
            { NpcId.PakSastro, C("62 tahun", "Bertani padi",
                "Petani di sawah Desa Suka Makmur. Sudah menggarap sawah yang sama sejak muda.",
                "Padi sudah mulai menguning, sebentar lagi panen.", "Desa sepi tanpa anak-anak muda seperti kamu.") },
            { NpcId.PakRT, C("54 tahun", "Musyawarah warga",
                "Ketua RT Desa Suka Makmur. Mengenal setiap keluarga di desa, termasuk keluarga {nama}.",
                "Ibumu sehat, jangan khawatir. Warga ikut menjaga.", "Desa sedang memperbaiki jalan, pelan-pelan.") },
            { NpcId.PakHerman, C("47 tahun", "Menata dagangan",
                "Pemilik Toko Anugerah di Jogokerto. Teliti mencatat setiap barang yang keluar masuk.",
                "Stok toko baru datang, lengkap sekarang.", "Kerja yang rapi itu kunci, Nak.") },
            { NpcId.PakSlamet, C("58 tahun", "Main catur di pos ronda",
                "Penjaga Pos Ronda Kampung Cempaka. Selalu tahu kabar terbaru di kampung.",
                "Semalam kampung aman, sambil ronda aku menang catur.", "Ada surat atau titipan? Biar kuterima.") },
            { NpcId.MasFajar, C("25 tahun", "Futsal & karang taruna",
                "Pemuda Kampung Cempaka yang aktif di karang taruna. Penggerak lomba 17-an setiap tahun.",
                "Sore nanti futsal di lapangan, gabung!", "Karang taruna lagi siapkan lomba, butuh bantuan nih.") },
            { NpcId.PakKarman, C("57 tahun", "Merawat ternak",
                "Peternak di Kampung Cempaka. Kambing dan ayamnya terkenal sehat dan gemuk.",
                "Kambingku baru beranak dua, lucu sekali.", "Peternakan butuh tenaga kalau kamu mau kerja.") },
            { NpcId.BuRukmini, C("50 tahun", "Arisan ibu-ibu",
                "Pemilik Toko Kelontong Kampung Cempaka. Ramah dan sering memberi bonus permen untuk anak-anak.",
                "Toko lagi ramai, minyak goreng sampai habis.", "Jangan lupa jaga kesehatan, Nak.") },
            { NpcId.PakHadi, C("49 tahun", "Mengoleksi tiket bus lama",
                "Petugas loket Terminal Jogokerto. Hafal jadwal semua bus antarwilayah.",
                "Bus ke desa berangkat tepat waktu hari ini.", "Kalau mau kirim paket ke kampung, lewat saya saja.") },
            { NpcId.PakRudi, C("46 tahun", "Bangun subuh ke pasar induk",
                "Pedagang sayur di Kios Sayur Pasar Ramai Jaya. Sayurannya selalu segar setiap pagi.",
                "Bayam dan kangkung hari ini segar sekali.", "Pasar itu harus bangun pagi, kalau siang sudah habis.") },
            { NpcId.PakSurya, C("53 tahun", "Merawat tanaman alun-alun",
                "Petugas kebersihan Alun-Alun Merdeka. Bangga kota tetap bersih berkat kerja kerasnya.",
                "Terima kasih sudah ikut buang sampah pada tempatnya.", "Alun-alun bersih, warga senang.") },
            { NpcId.BuSiti, C("44 tahun", "Memasak masakan rumahan",
                "Pemilik Warung Makan Bu Siti di Kampung Cempaka. Sayur lodehnya mengingatkan banyak orang pada rumah.",
                "Hari ini masak lodeh, mampir ya.", "Anak rantau harus makan yang bergizi.") },
            { NpcId.PenjagaPerpustakaan, C("38 tahun", "Menyusun katalog buku",
                "Penjaga Perpustakaan Jogokerto. Bisa menemukan buku apa pun dalam hitungan detik.",
                "Ada buku baru tentang wirausaha, sudah kusisihkan.", "Perpustakaan tutup jam lima, jangan terlambat.") },
            { NpcId.TukangKebun, C("51 tahun", "Menanam bunga",
                "Tukang kebun Taman Pelangi. Setiap bedeng bunga di taman ditanam dengan tangannya sendiri.",
                "Bunga mawar di taman baru mekar.", "Taman itu seperti hidup, harus dirawat setiap hari.") },
            { NpcId.Nabila, C("21 tahun", "Merangkai bunga & menulis catatan",
                "Gadis ceria yang sering terlihat di Taman Pelangi. Ramah, suka membantu, dan selalu membawa buku catatan kecil.",
                "Hai {nama}! Bunga di taman lagi cantik-cantiknya.", "Aku baru menulis puisi, nanti kubacakan ya.") },
            { NpcId.Kirana, C("22 tahun", "Membaca novel",
                "Pengunjung setia Perpustakaan. Tenang, cerdas, dan suka membahas isi buku berjam-jam.",
                "Aku sedang membaca novel bagus, ceritanya tentang merantau.", "Kapan-kapan kita belajar bersama lagi.") },
            { NpcId.Anindya, C("22 tahun", "Mengurus acara & bercanda",
                "Pelayan Cafe Senja yang humoris dan mudah bergaul. Selalu tahu acara seru di kota.",
                "Halo! Cafe lagi ada promo, traktir aku dong. Bercanda!", "Festival kota sebentar lagi, kamu datang kan?") },
            { NpcId.Melati, C("21 tahun", "Merajut & merawat tanaman",
                "Penghuni Gang Melati yang pendiam dan perhatian. Butuh waktu untuk terbuka, tapi tulus.",
                "Eh... {nama}? Terima kasih sudah menelepon.", "Tanaman di depan rumahku mulai berbunga.") },
            { NpcId.Citra, C("23 tahun", "Memotret & menjelajah",
                "Fotografer muda yang suka menjelajah sudut kota. Kreatif dan selalu penuh ide.",
                "Aku baru dapat foto matahari terbenam di pantai!", "Ayo jelajah tempat baru akhir pekan ini.") },
        };

        public static ContactInfo Get(NpcId id) => Info.TryGetValue(id, out ContactInfo c) ? c : new ContactInfo();

        // ------------------------------------------------------------------ pesan

        /// <summary>Pesan yang bisa dikirim pemain (urutan = pilihan di menu).</summary>
        public static readonly string[] Templates =
        {
            "Halo, apa kabar?",
            "Terima kasih untuk bantuannya.",
            "Semangat untuk hari ini, ya!",
            "Ada kabar terbaru?",
            "Selamat malam, istirahat yang cukup ya.",
        };

        static readonly string[][] FriendReplies =
        {
            new[] { "Kabar baik! Kamu sendiri gimana?", "Baik, terima kasih sudah tanya." },
            new[] { "Sama-sama, jangan sungkan.", "Santai saja, kita kan saling bantu." },
            new[] { "Kamu juga, semangat!", "Siap! Terima kasih ya." },
            new[] { "Belum ada yang baru, aman-aman saja.", "Nanti kalau ada kabar kuberitahu." },
            new[] { "Selamat malam juga!", "Iya, kamu juga jangan begadang." },
        };

        static readonly string[][] GirlReplies =
        {
            new[] { "Baik! Senang kamu menghubungiku :)", "Kabarku baik. Kamu jangan lupa makan ya." },
            new[] { "Sama-sama! Aku senang bisa membantu.", "Ah, bukan apa-apa kok." },
            new[] { "Kamu juga ya! Hati-hati di jalan.", "Terima kasih, aku jadi semangat." },
            new[] { "Hari ini cukup menyenangkan. Kapan kita ketemu lagi?", "Aku punya cerita, nanti kuceritakan langsung." },
            new[] { "Selamat malam, mimpi indah.", "Iya, kamu juga istirahat ya." },
        };

        static readonly string[][] MotherReplies =
        {
            new[] { "Ibu baik, Nak. Kamu sehat kan?", "Alhamdulillah sehat. Kamu jaga diri ya." },
            new[] { "Ibu yang terima kasih, Nak.", "Sudah kewajiban Ibu, Nak." },
            new[] { "Kamu juga semangat ya, Nak. Ibu bangga.", "Doa Ibu selalu menyertaimu." },
            new[] { "Di desa baik-baik saja. Pak RT titip salam.", "Panen kali ini lumayan, Nak." },
            new[] { "Selamat malam, Nak. Jangan lupa berdoa.", "Iya, Ibu juga mau tidur." },
        };

        /// <summary>Balasan NPC untuk pesan nomor <paramref name="template"/> (bervariasi menurut hari).</summary>
        public static string Reply(NpcId id, int template, int day, bool girl)
        {
            var pool = id == NpcId.Ratna ? MotherReplies : girl ? GirlReplies : FriendReplies;
            var options = pool[UnityEngine.Mathf.Clamp(template, 0, pool.Length - 1)];
            return options[(day + (int)id) % options.Length];
        }
    }
}
