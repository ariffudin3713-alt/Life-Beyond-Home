namespace PixelAdventure.LifeSim.Pulang
{
    /// <summary>
    /// Adegan Prolog–Chapter 6 yang dipicu dengan berbicara ke NPC.
    /// Efek pilihan: "keluarga+N", "rep+N", "dekat:Nabila+N", "buy:roti", "flag:nama".
    /// </summary>
    public static partial class PulangScript
    {
        const string Hendra = "Pak Hendra", Dimas = "Dimas", Sari = "Bu Sari", Nabila = "Nabila";
        const string Joko = "Pak Joko", Budi = "Pak Budi";

        static SceneDef[] ChapterScenes() => new[]
        {
            // ---------------- PROLOG – Quest 1
            new SceneDef
            {
                Id = "prolog_bicara",
                Lines = new[]
                {
                    L(Ratna, "Tasmu sudah siap, Nak?"),
                    L(P, "Sudah, Bu. Barang-barang " + P + " juga sudah dikumpulkan."),
                    L(Ratna, "Gunakan uangmu dengan bijak. Jangan terlalu memaksakan diri."),
                },
            },
            new SceneDef
            {
                Id = "prolog_pamit",
                Lines = new[]
                {
                    L(P, "Bu, " + P + " berangkat sekarang."),
                    L(Ratna, "Hati-hati, Nak. Ibu tunggu kabarmu."),
                    L(Ratna, "Kalau suatu saat kamu pergi jauh, jangan lupa pulang."),
                    L(P, "Tenang saja, Bu. " + P + " pasti pulang."),
                    L(N, "Pergilah ke Terminal Desa Suka Makmur dan naik bus ke Kota Jogokerto."),
                },
            },

            // ---------------- CHAPTER 1
            new SceneDef
            {
                Id = "ch1_hendra",
                Lines = new[]
                {
                    L(Hendra, "Kamu orang baru di sini?"),
                    L(P, "Iya, Pak. Saya baru sampai dari desa."),
                    L(Hendra, "Namamu siapa?"),
                    L(P, "Nama saya " + P + "."),
                    L(Hendra, "Kalau begitu, selamat datang di Jogokerto."),
                },
                Choices = new[]
                {
                    new Choice("Saya sedang mencari tempat tinggal.", "",
                        L(Hendra, "Kamu bisa mencoba Kost Pelangi. Tidak terlalu jauh dari sini.")),
                    new Choice("Saya sedang mencari pekerjaan.", "",
                        L(Hendra, "Kalau begitu, kamu perlu mengenal lingkungan kota dahulu.")),
                    new Choice("Saya ingin mengetahui tempat-tempat di kota.", "",
                        L(Hendra, "Bisa. Nanti saya tunjukkan beberapa tempat penting.")),
                },
                After = new[]
                {
                    L(Hendra, "Kost Pelangi ada di sisi utara jalan. Toko Serba Ada juga di sana kalau kamu lapar."),
                },
            },
            new SceneDef
            {
                Id = "ch1_dimas",
                Lines = new[]
                {
                    L(Dimas, "Hei, kamu penghuni baru?"),
                    L(P, "Iya. Kamu siapa?"),
                    L(Dimas, "Namaku Dimas. Aku juga tinggal di sekitar sini."),
                    L(P, "Kenalkan, aku " + P + "."),
                    L(Dimas, "Kalau butuh bantuan, bilang saja."),
                },
                Choices = new[]
                {
                    new Choice("Terima kasih, aku masih belum mengenal kota ini.", "",
                        L(Dimas, "Santai saja. Nanti aku bantu.")),
                    new Choice("Kamu sudah lama tinggal di sini?", "",
                        L(Dimas, "Sudah lumayan lama. Aku sudah hafal sebagian besar tempat di kota.")),
                    new Choice("Aku ingin segera mencari pekerjaan.", "",
                        L(Dimas, "Bagus. Banyak kesempatan di sini, asal kamu mau berusaha.")),
                },
                After = new[] { L(N, "Dimas menjadi teman pertamamu di Jogokerto.") },
            },
            new SceneDef
            {
                Id = "ch1_busari",
                Lines = new[]
                {
                    L(Sari, "Eh, Nak. Sepertinya wajah baru."),
                    L(P, "Iya, Bu. Saya baru datang ke Jogokerto."),
                    L(Sari, "Sudah makan?"),
                    L(P, "Belum, Bu."),
                    L(Sari, "Kalau begitu, pilih saja makanan yang kamu mau."),
                },
                Choices = new[]
                {
                    new Choice("Saya mau membeli roti.", "buy:roti",
                        L(Sari, "Baik, tunggu sebentar.")),
                    new Choice("Ada makanan yang murah, Bu?", "flag:info_harga",
                        L(Sari, "Ada. Kamu bisa memilih makanan sesuai uangmu.")),
                    new Choice("Saya hanya ingin bertanya tentang kota.", "flag:info_pasar",
                        L(Sari, "Kalau mau tahu kehidupan di sini, sering-seringlah berkeliling.")),
                },
            },
            new SceneDef
            {
                Id = "q4_dimas_kerja",
                Lines = new[]
                {
                    L(P, "Aku tidak bisa terus menggunakan uang yang kubawa."),
                    L(Dimas, "Kamu sudah memikirkan pekerjaan?"),
                    L(P, "Belum. Aku masih mencari informasi."),
                    L(Dimas, "Aku tahu beberapa tempat yang membutuhkan bantuan."),
                    L(Dimas, "Penjaga warung di Warung Bu Sari, Pasar Ramai Jaya. " +
                             "Atau tukang kebun di Kebun Warga, Kampung Cempaka."),
                    L(Dimas, "Dua-duanya bisa langsung kamu lamar. Naik bus saja dari terminal."),
                    L(Dimas, "Minimarket juga cari kurir, tapi kamu harus punya sepeda dulu."),
                    L(Dimas, "Cafe Senja nanti juga butuh pelayan, tapi kafenya belum buka."),
                    L(N, "Lamar salah satu pekerjaan di tempatnya, lalu selesaikan satu shift. " +
                         "Ikuti panah misi ke tempat kerja."),
                },
            },
            new SceneDef
            {
                Id = "ch1_nabila",
                Lines = new[]
                {
                    L(Nabila, "Hai! Kamu yang baru datang ke Jogokerto itu, ya?"),
                    L(P, "Iya. Kok kamu tahu?"),
                    L(Nabila, "Katanya ada pendatang baru di sekitar kost."),
                    L(P, "Oh, begitu."),
                    L(Nabila, "Namaku Nabila. Kamu?"),
                    L(P, P + "."),
                    L(Nabila, "Senang kenalan denganmu!"),
                },
                Choices = new[]
                {
                    new Choice("Aku juga senang kenalan denganmu.", "dekat:Nabila+2",
                        L(Nabila, "Hehe, kamu ramah juga ternyata.")),
                    new Choice("Kamu memang sering berada di taman?", "flag:info_nabila",
                        L(Nabila, "Iya! Aku suka suasana taman di sini.")),
                    new Choice("Aku masih harus mengurus pekerjaanku.", "",
                        L(Nabila, "Oh, baiklah. Semangat, ya!")),
                },
                After = new[]
                {
                    L(Nabila, P + ", boleh minta bantuan?"),
                    L(P, "Boleh. Ada apa?"),
                    L(Nabila, "Aku sedang mencari barang yang tertinggal di sekitar taman. Sebuah buku catatan kecil."),
                },
            },
            new SceneDef
            {
                Id = "ch1_nabila_selesai",
                Lines = new[]
                {
                    L(P, "Ini buku catatanmu, Nabila."),
                    L(Nabila, "Terima kasih! Kamu baik sekali."),
                },
            },

            // ---------------- CHAPTER 2
            new SceneDef
            {
                Id = "ch2_dimas",
                Lines = new[]
                {
                    L(Dimas, P + ", aku punya masalah kecil."),
                    L(P, "Ada apa?"),
                    L(Dimas, "Ada beberapa barang yang harus dikirim, tapi aku belum sempat mengurusnya."),
                },
                Choices = new[]
                {
                    new Choice("Aku bisa membantu.", "rep+2", L(Dimas, "Serius? Terima kasih!")),
                    new Choice("Apa pekerjaannya sulit?", "", L(Dimas, "Tidak terlalu. Hanya perlu mengantar beberapa barang.")),
                    new Choice("Aku akan membantu kalau pekerjaanku selesai.", "", L(Dimas, "Baik. Kabari aku nanti.")),
                },
                After = new[] { L(Dimas, "Antar ke Pak Herman di Toko Anugerah, Bu Rukmini di Kampung Cempaka, dan Pak Hadi di terminal.") },
            },
            new SceneDef
            {
                Id = "ch2_busari",
                Lines = new[]
                {
                    L(Sari, P + ", pasar sedang ramai sekali hari ini."),
                    L(P, "Memangnya ada masalah, Bu?"),
                    L(Sari, "Ada beberapa barang yang belum sampai."),
                },
                Choices = new[]
                {
                    new Choice("Membantu tanpa meminta tambahan upah.", "rep+2",
                        L(Sari, "Terima kasih, Nak. Ibu sangat terbantu.")),
                    new Choice("Menanyakan upah terlebih dahulu.", "flag:upah_pasar",
                        L(Sari, "Tentu saja. Ibu akan memberikan bayaran sesuai pekerjaan.")),
                    new Choice("Menyarankan meminta bantuan NPC lain.", "",
                        L(Sari, "Itu juga ide bagus.")),
                },
                After = new[] { L(Sari, "Antarkan ke Pak Darto, Pak Rudi, dan Bu Siti, ya.") },
            },
            new SceneDef
            {
                Id = "ch2_joko",
                Lines = new[]
                {
                    L(Joko, "Kamu bisa bantu saya sebentar?"),
                    L(P, "Ada apa, Pak?"),
                    L(Joko, "Ada beberapa pekerjaan bengkel yang harus diselesaikan."),
                },
                Choices = new[]
                {
                    new Choice("Saya mau belajar, Pak.", "", L(Joko, "Bagus. Kalau mau belajar, kamu harus teliti.")),
                    new Choice("Saya ingin membantu pekerjaan yang mudah dahulu.", "", L(Joko, "Tidak masalah. Kita mulai dari dasar.")),
                    new Choice("Saya ingin tahu pekerjaan apa saja di bengkel.", "",
                        L(Joko, "Banyak. Ada yang sederhana dan ada yang membutuhkan keahlian.")),
                },
            },

            // ---------------- CHAPTER 3
            new SceneDef
            {
                Id = "ch3_budi",
                Lines = new[]
                {
                    L(Budi, P + ", saya mendengar kamu cukup rajin bekerja."),
                    L(P, "Terima kasih, Pak."),
                    L(Budi, "Saya punya informasi pekerjaan yang mungkin cocok untukmu."),
                },
                Choices = new[]
                {
                    new Choice("Saya tertarik, Pak.", "", L(Budi, "Bagus. Saya akan menjelaskan persyaratannya.")),
                    new Choice("Apa saya harus mempunyai pengalaman?", "", L(Budi, "Beberapa pekerjaan membutuhkan pengalaman.")),
                    new Choice("Saya ingin meningkatkan kemampuan dahulu.", "", L(Budi, "Itu keputusan yang baik.")),
                },
                After = new[]
                {
                    L(Budi, "Pekerjaan dibagi tiga tingkatan: sederhana, menengah, dan profesional."),
                    L(Budi, "Setiap tingkat membutuhkan level, pengalaman, dan skill. Skill bisa dipelajari di perpustakaan."),
                },
            },

            // ---------------- CHAPTER 6
            new SceneDef
            {
                Id = "urusan_terakhir",
                Lines = new[]
                {
                    L(P, "Dimas, aku harus pulang ke Desa Suka Makmur."),
                    L(Dimas, "Aku sudah dengar. Turut berduka, " + P + "."),
                    L(Dimas, "Urusan di sini biar aku yang bantu. Pulanglah."),
                },
            },
            new SceneDef
            {
                Id = "pilih_kendaraan",
                Lines = new[] { L(N, "Bagaimana kamu akan pulang?") },
                Choices = new[]
                {
                    new Choice("Naik bus dari Terminal Jogokerto.", "",
                        L(P, "Aku harus segera ke terminal.")),
                },
            },
        };

        /// <summary>Kalimat Ratna saat ditelepon (Daily Quest "Hubungi Ibu").</summary>
        public static readonly string[] RatnaPhone =
        {
            "Syukurlah. Jangan lupa makan dan istirahat.",
            "Gunakan uangmu dengan bijak. Jangan terlalu memaksakan diri.",
            "Ibu tidak meminta kamu menjadi orang kaya, Nak. Ibu cuma ingin kamu bahagia.",
            "Ibu tunggu, Nak.",
            "Ibu juga merindukanmu.",
        };
    }
}
