namespace PixelAdventure.LifeSim.Pulang
{
    /// <summary>
    /// Adegan misi Chapter 3–5 (Quest 10–17). Disusun dari fokus tiap chapter di dokumen:
    /// Ch3 kemampuan, bisnis, cerita Melati & Citra; Ch4 hubungan, misi teman, cerita masa lalu;
    /// Ch5 pesan dari Ibu dan pulang yang terus tertunda. Efek pilihan sama dengan adegan lain.
    /// </summary>
    public static partial class PulangScript
    {
        const string Melati = "Melati", Citra = "Citra", Darto = "Pak Darto", Slamet = "Pak Slamet";
        const string Hadi = "Pak Hadi", Fajar = "Mas Fajar";

        static SceneDef[] MidChapterScenes() => new[]
        {
            // ================================================================ CHAPTER 3
            // ---------------- Quest 10: Meningkatkan Kemampuan
            new SceneDef
            {
                Id = "ch3_budi_kursus",
                Lines = new[]
                {
                    L(Budi, P + ", kalau mau naik ke pekerjaan menengah, kamu harus punya keterampilan."),
                    L(P, "Keterampilan apa yang paling dibutuhkan, Pak?"),
                    L(Budi, "Montir, fotografer, tukang cukur. Semuanya bisa dipelajari lewat kursus di Perpustakaan."),
                },
                Choices = new[]
                {
                    new Choice("Saya akan mulai belajar minggu ini.", "rep+1",
                        L(Budi, "Bagus. Orang yang mau belajar tidak akan tertinggal.")),
                    new Choice("Apakah kursusnya mahal, Pak?", "",
                        L(Budi, "Ada biayanya. Anggap saja tabungan untuk masa depanmu.")),
                    new Choice("Saya ingin tahu pekerjaan mana yang paling cocok.", "flag:info_karier",
                        L(Budi, "Pilih yang sesuai minatmu. Pak Joko di bengkel bisa jadi tempat belajar yang baik.")),
                },
                After = new[] { L(Budi, "Datanglah ke Perpustakaan. Ambil satu kursus, lalu buktikan dengan bekerja.") },
            },

            // ---------------- Quest 11: Wajah-Wajah Baru (Melati & Citra – episode 1)
            new SceneDef
            {
                Id = "ch3_melati",
                Lines = new[]
                {
                    L(N, "Gang Melati. Seorang perempuan sedang menyiram bunga di depan rumahnya."),
                    L(P, "Permisi... bunganya cantik sekali."),
                    L(Melati, "...Terima kasih."),
                    L(P, "Aku tinggal di Kost Pelangi. Namaku " + P + "."),
                    L(Melati, "Melati."),
                    L(Melati, "Maaf, aku tidak terbiasa mengobrol dengan orang baru."),
                },
                Choices = new[]
                {
                    new Choice("Tidak apa-apa. Pelan-pelan saja.", "dekat:Melati+2",
                        L(Melati, "...Kamu orang yang sabar, ya.")),
                    new Choice("Kamu yang merawat semua bunga di gang ini?", "flag:info_melati",
                        L(Melati, "Iya. Bunga tidak pernah banyak bertanya.")),
                    new Choice("Kalau begitu aku pamit dulu.", "",
                        L(Melati, "Iya. Hati-hati di jalan.")),
                },
            },
            new SceneDef
            {
                Id = "ch3_citra",
                Lines = new[]
                {
                    L(Citra, "Eh, jangan bergerak! Sedikit lagi... selesai!"),
                    L(P, "Kamu memotretku?"),
                    L(Citra, "Maaf! Cahaya sore di alun-alun lagi bagus sekali. Aku Citra."),
                    L(P, P + ". Aku belum lama tinggal di Jogokerto."),
                    L(Citra, "Pendatang? Berarti masih banyak tempat yang belum kamu lihat!"),
                },
                Choices = new[]
                {
                    new Choice("Mau jadi pemandu jalanku?", "dekat:Citra+2",
                        L(Citra, "Boleh! Aku tahu sudut-sudut kota yang jarang orang tahu.")),
                    new Choice("Kamu seorang fotografer?", "flag:info_citra",
                        L(Citra, "Belum resmi. Aku sedang mengumpulkan foto untuk pameran kecil.")),
                    new Choice("Aku harus kembali bekerja.", "",
                        L(Citra, "Oke! Lain kali aku tunjukkan hasil fotonya.")),
                },
            },

            // ---------------- Quest 12: Mengenal Dunia Bisnis
            new SceneDef
            {
                Id = "ch3_darto",
                Lines = new[]
                {
                    L(Darto, "Nak " + P + ", kamu kelihatan makin mapan sekarang."),
                    L(P, "Alhamdulillah, Pak. Saya ingin belajar berdagang."),
                    L(Darto, "Usaha itu bukan cuma soal untung. Kamu harus pintar mengatur pemasukan dan pengeluaran."),
                    L(Darto, "Mulailah dengan menyisihkan modal. Sedikit demi sedikit."),
                },
                Choices = new[]
                {
                    new Choice("Berapa modal yang dibutuhkan untuk mulai?", "",
                        L(Darto, "Tidak harus besar. Yang penting rutin menabung.")),
                    new Choice("Bagaimana kalau usahanya rugi?", "flag:info_rugi",
                        L(Darto, "Rugi itu guru. Asal kamu mencatat semuanya, kamu tahu apa yang harus diperbaiki.")),
                    new Choice("Saya ingin membantu dagangan Bapak dulu.", "rep+1",
                        L(Darto, "Boleh. Belajar dari dekat itu paling cepat.")),
                },
                After = new[] { L(Darto, "Pulanglah, simpan sebagian uangmu di tabungan kost. Itu langkah pertama seorang pedagang.") },
            },
            new SceneDef
            {
                Id = "ch3_sari_usaha",
                Lines = new[]
                {
                    L(Sari, "Mau buka usaha? Wah, Ibu senang mendengarnya."),
                    L(Sari, "Dulu warung ini juga dimulai dari satu meja kecil."),
                    L(P, "Apa rahasianya, Bu?"),
                    L(Sari, "Jujur kepada pembeli, dan jangan pakai uang modal untuk jajan."),
                },
                Choices = new[]
                {
                    new Choice("Saya akan mengingatnya, Bu.", "rep+1",
                        L(Sari, "Ibu percaya kamu bisa.")),
                    new Choice("Apa Ibu pernah hampir menyerah?", "flag:info_sari",
                        L(Sari, "Sering, Nak. Tapi setiap ingat anak-anak di rumah, Ibu buka warung lagi.")),
                    new Choice("Nanti saya mampir belanja lagi.", "",
                        L(Sari, "Ibu tunggu, ya.")),
                },
                After = new[]
                {
                    L(N, P + " mulai memikirkan masa depan, bukan hanya kebutuhan sehari-hari."),
                    L(N, "Tingkatkan level untuk membuka pekerjaan menengah."),
                },
            },

            // ================================================================ CHAPTER 4
            // ---------------- Quest 13: Teman Seperjuangan
            new SceneDef
            {
                Id = "ch4_dimas",
                Lines = new[]
                {
                    L(Dimas, P + "... boleh aku cerita sebentar?"),
                    L(P, "Tentu. Ada apa, Dim?"),
                    L(Dimas, "Kontrak kerjaku di gudang sudah habis. Aku harus mencari pekerjaan lagi."),
                    L(Dimas, "Dulu kamu yang baru datang. Sekarang malah aku yang bingung."),
                },
                Choices = new[]
                {
                    new Choice("Ayo, aku temani mencari pekerjaan.", "rep+2",
                        L(Dimas, "Serius? Kamu memang teman yang bisa diandalkan.")),
                    new Choice("Kamu sudah melamar ke mana saja?", "",
                        L(Dimas, "Beberapa tempat, tapi belum ada kabar.")),
                    new Choice("Jangan menyerah, Dim.", "",
                        L(Dimas, "Iya. Terima kasih sudah mau mendengarkan.")),
                },
                After = new[] { L(Dimas, "Katanya PT Maju Bersama sedang membuka lowongan. Temani aku ke sana, ya?") },
            },
            new SceneDef
            {
                Id = "ch4_budi",
                Lines = new[]
                {
                    L(P, "Pak Budi, teman saya sedang mencari pekerjaan. Apa ada lowongan di PT Maju Bersama?"),
                    L(Budi, "Ada. Mereka butuh staf gudang yang rajin."),
                    L(Budi, "Suruh temanmu datang besok pagi membawa surat lamaran."),
                },
                Choices = new[]
                {
                    new Choice("Saya jamin dia pekerja keras, Pak.", "rep+1",
                        L(Budi, "Kalau kamu yang menjamin, saya percaya.")),
                    new Choice("Apa syaratnya, Pak?", "",
                        L(Budi, "Jujur, tepat waktu, dan mau belajar. Sama seperti kamu dulu.")),
                    new Choice("Terima kasih banyak, Pak.", "",
                        L(Budi, "Sama-sama. Sampaikan salam saya.")),
                },
            },
            new SceneDef
            {
                Id = "ch4_dimas_selesai",
                Lines = new[]
                {
                    L(P, "Dim! PT Maju Bersama butuh staf gudang. Datang besok pagi bawa surat lamaran."),
                    L(Dimas, "Yang benar? " + P + ", kamu luar biasa!"),
                    L(Dimas, "Waktu kamu baru datang, aku yang membantumu. Sekarang kamu yang membantuku."),
                    L(P, "Kita kan teman seperjuangan."),
                },
                Choices = new[]
                {
                    new Choice("Semoga lancar besok, Dim.", "rep+2",
                        L(Dimas, "Doakan aku, ya. Aku traktir kalau diterima!")),
                    new Choice("Jangan lupa tidur cepat malam ini.", "",
                        L(Dimas, "Siap, Bos!")),
                    new Choice("Kita harus sama-sama sukses.", "rep+1",
                        L(Dimas, "Setuju. Lalu kita pulang kampung dengan kepala tegak.")),
                },
                After = new[] { L(N, "Persahabatan dengan Dimas semakin erat.") },
            },

            // ---------------- Quest 14: Pesan Keluarga (cerita masa lalu)
            new SceneDef
            {
                Id = "ch4_hendra",
                Lines = new[]
                {
                    L(Hendra, P + ", saya mau minta tolong."),
                    L(Hendra, "Antarkan surat ini kepada Pak Slamet di Pos Ronda Kampung Cempaka."),
                    L(P, "Bapak kenal Pak Slamet?"),
                    L(Hendra, "Kami berasal dari desa yang sama. Desa Suka Makmur."),
                    L(P, "Desa Suka Makmur? Itu desa saya, Pak!"),
                    L(Hendra, "Saya tahu. Sejak pertama bertemu, wajahmu mengingatkan saya pada ibumu."),
                },
                Choices = new[]
                {
                    new Choice("Bapak kenal ibu saya?", "flag:hendra_kenal_ratna",
                        L(Hendra, "Antarkan dulu suratnya. Nanti saya ceritakan.")),
                    new Choice("Baik, Pak. Saya antar sekarang.", "rep+1",
                        L(Hendra, "Terima kasih, Nak.")),
                    new Choice("Kenapa tidak Bapak antar sendiri?", "",
                        L(Hendra, "Ada hal yang lebih mudah ditulis daripada diucapkan.")),
                },
                After = new[] { L(N, "Kamu menerima Surat Pak Hendra.") },
            },
            new SceneDef
            {
                Id = "ch4_slamet",
                Lines = new[]
                {
                    L(P, "Pak Slamet, ini surat dari Pak Hendra."),
                    L(Slamet, "Hendra? Dia masih ingat saya rupanya."),
                    L(N, "Pak Slamet membaca surat itu pelan-pelan."),
                    L(Slamet, "Jadi kamu anaknya Ratna..."),
                    L(Slamet, "Ibumu itu pekerja keras. Ia bekerja di sawah dari subuh sampai magrib supaya kamu bisa sekolah."),
                    L(P, "Ibu tidak pernah cerita..."),
                    L(Slamet, "Ibu mana yang mau anaknya merasa berutang?"),
                },
                Choices = new[]
                {
                    new Choice("Saya ingin membalas semua pengorbanan Ibu.", "keluarga+2",
                        L(Slamet, "Balas dengan sering pulang, Nak. Itu yang paling ia tunggu.")),
                    new Choice("Kenapa Bapak dan Pak Hendra meninggalkan desa?", "flag:masa_lalu",
                        L(Slamet, "Sama sepertimu. Mencari hidup yang lebih baik... dan terlalu lama tidak pulang.")),
                    new Choice("Terima kasih sudah bercerita, Pak.", "",
                        L(Slamet, "Sampaikan salam saya untuk Hendra.")),
                },
            },
            new SceneDef
            {
                Id = "ch4_hendra_selesai",
                Lines = new[]
                {
                    L(Hendra, "Sudah kamu antar? Slamet bilang apa?"),
                    L(P, "Beliau bercerita tentang Ibu. Saya baru tahu banyak hal."),
                    L(Hendra, "Dulu saya juga merantau, dan berjanji akan pulang setelah sukses."),
                    L(Hendra, "Waktu akhirnya saya pulang, orang tua saya sudah tidak ada."),
                    L(Hendra, "Jangan sampai kamu mengalami hal yang sama, Nak."),
                },
                Choices = new[]
                {
                    new Choice("Saya akan menelepon Ibu malam ini.", "keluarga+2",
                        L(Hendra, "Bagus. Suara anaknya adalah obat paling manjur.")),
                    new Choice("Saya pulang setelah sukses, Pak.", "",
                        L(Hendra, "Sukses itu tidak ada habisnya, Nak. Ingat itu.")),
                    new Choice("(Diam dan mengangguk)", "keluarga+1",
                        L(Hendra, "...")),
                },
            },

            // ================================================================ CHAPTER 5
            // ---------------- Quest 16: Kiriman untuk Ibu
            new SceneDef
            {
                Id = "ch5_hadi",
                Lines = new[]
                {
                    L(Hadi, "Mau kirim paket ke mana, Mas?"),
                    L(P, "Ke Desa Suka Makmur, Pak. Untuk ibu saya."),
                    L(Hadi, "Kenapa tidak diantar sendiri? Ibu pasti lebih senang melihat anaknya."),
                    L(P, "Sedang banyak pekerjaan, Pak..."),
                    L(Hadi, "Baik. Paketnya sampai lusa."),
                },
                Choices = new[]
                {
                    new Choice("Titip pesan: " + P + " sayang Ibu.", "keluarga+2",
                        L(Hadi, "Saya tuliskan di kartunya.")),
                    new Choice("Nanti saya pulang sendiri setelah semuanya beres.", "",
                        L(Hadi, "Jangan kelamaan, Mas. Waktu tidak menunggu.")),
                    new Choice("Terima kasih, Pak.", "",
                        L(Hadi, "Sama-sama.")),
                },
                After = new[] { L(N, "Paket untuk Ibu sudah dikirim ke Desa Suka Makmur.") },
            },

            // ---------------- Quest 17: Kerja Bakti
            new SceneDef
            {
                Id = "ch5_fajar",
                Lines = new[]
                {
                    L(Fajar, "Mas " + P + "! Minggu ini warga Kampung Cempaka kerja bakti."),
                    L(Fajar, "Gang Melati banyak sampah sesudah hujan. Mau bantu?"),
                },
                Choices = new[]
                {
                    new Choice("Tentu, aku ikut.", "rep+2",
                        L(Fajar, "Mantap! Warga pasti senang.")),
                    new Choice("Apa yang harus kubawa?", "",
                        L(Fajar, "Cukup tenaga dan semangat. Sapu sudah disiapkan di pos ronda.")),
                    new Choice("Aku bantu sebentar saja, ya.", "",
                        L(Fajar, "Sebentar juga sangat membantu.")),
                },
                After = new[] { L(Fajar, "Bersihkan Gang Melati, lalu lapor ke Pak Slamet di pos ronda.") },
            },
            new SceneDef
            {
                Id = "ch5_slamet",
                Lines = new[]
                {
                    L(Slamet, "Gang Melati sudah bersih. Terima kasih, Nak."),
                    L(Slamet, "Kamu sudah jadi bagian dari kampung ini."),
                    L(Slamet, "Ibumu pasti bangga kalau tahu anaknya dihormati di tanah rantau."),
                },
                Choices = new[]
                {
                    new Choice("Saya ingin Ibu melihatnya sendiri.", "keluarga+1",
                        L(Slamet, "Kalau begitu, ajaklah ia ke sini. Atau kamu yang pulang menemuinya.")),
                    new Choice("Senang bisa membantu, Pak.", "rep+2",
                        L(Slamet, "Kampung ini juga senang punya warga sepertimu.")),
                    new Choice("Semua ini berkat bantuan warga.", "rep+1",
                        L(Slamet, "Rendah hati. Itu juga dari ibumu, saya kira.")),
                },
                After = new[]
                {
                    L(N, P + " sekarang memiliki pekerjaan, tabungan, teman, dan nama baik."),
                    L(N, "Namun ada satu hal yang terus tertunda: pulang ke rumah."),
                },
            },
        };
    }
}
