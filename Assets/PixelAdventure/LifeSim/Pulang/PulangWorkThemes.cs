using System.Collections.Generic;

namespace PixelAdventure.LifeSim.Pulang
{
    /// <summary>Jenis mini-game kerja.</summary>
    public enum WorkGame
    {
        Timing,   // tekan saat penanda di zona hijau (cukur, nelayan: zona ikut bergerak)
        Choice,   // pilih jawaban/barang yang benar sebelum waktu habis (warung, kafe, kantor, guru, dokter)
        Combo,    // tekan urutan panah dengan benar (kurir, montir, koki, desainer, perawat)
        Mash,     // tekan E berulang-ulang sampai bar penuh (petani, tukang kebun)
        Aim,      // gerakkan bidikan ke sasaran yang bergerak lalu tekan E (fotografer, polisi)
    }

    public struct WorkQuestion
    {
        public string Ask;
        public string[] Options;   // Options[0] = jawaban benar (diacak saat tampil)
        public WorkQuestion(string ask, params string[] options) { Ask = ask; Options = options; }
    }

    /// <summary>Tema mini-game satu pekerjaan: judul, cara main, tugas per ronde, kata-kata hasil.</summary>
    public class WorkTheme
    {
        public string Title, Instruction, RoundName, Icon;
        public WorkGame Game;
        public int Rounds;
        public string[] Tasks = new string[0];
        public WorkQuestion[] Questions = new WorkQuestion[0];
        public string[] HitLines = { "BAGUS!" }, MissLines = { "MELESET!" };
        public string Result = "{0} dari {1} tugas selesai dengan baik.";
        public string Target;            // Aim: sprite sasaran
        public bool MovingZone;          // Timing: zona bergerak (ikan melawan)
        public float ZoneHalf = 0.12f;   // Timing: lebar setengah zona
    }

    public static class PulangWorkThemes
    {
        static readonly Dictionary<string, WorkTheme> Themes = new Dictionary<string, WorkTheme>
        {
            // ------------------------------------------------------------ PILIHAN
            { "penjaga_warung", new WorkTheme
            {
                Title = "JAGA WARUNG", Icon = "lc_job_penjaga_warung", Game = WorkGame.Choice, Rounds = 5, RoundName = "PEMBELI",
                Instruction = "AMBILKAN BARANG YANG DIMINTA PEMBELI",
                HitLines = new[] { "PEMBELI SENANG!", "MAKASIH, MAS!", "PAS!" }, MissLines = new[] { "LHO, BUKAN ITU!", "SALAH AMBIL!" },
                Result = "{0} dari {1} pembeli mendapat barang yang tepat.",
                Questions = new[]
                {
                    new WorkQuestion("MINTA GULA PASIR SEKILO", "GULA PASIR", "GARAM", "TEPUNG TERIGU", "BERAS"),
                    new WorkQuestion("BELI MINYAK GORENG SATU LITER", "MINYAK GORENG", "KECAP MANIS", "SAUS SAMBAL", "SIRUP"),
                    new WorkQuestion("ADA MI INSTAN RASA SOTO?", "MI INSTAN SOTO", "MI GORENG", "BIHUN", "SOUN"),
                    new WorkQuestion("TELUR AYAM SETENGAH KILO", "TELUR AYAM", "TELUR ASIN", "TAHU", "TEMPE"),
                    new WorkQuestion("SABUN CUCI PIRING SATU", "SABUN CUCI PIRING", "SAMPO", "PASTA GIGI", "DETERJEN"),
                    new WorkQuestion("KOPI SACHET DUA RENCENG", "KOPI SACHET", "TEH CELUP", "SUSU KENTAL", "GULA MERAH"),
                    new WorkQuestion("KOREK API, MAU BAKAR SATE", "KOREK API", "LILIN", "BATERAI", "OBAT NYAMUK"),
                    new WorkQuestion("GAS ELPIJI MELON SATU", "GAS ELPIJI 3 KG", "GALON AIR", "MINYAK TANAH", "ARANG"),
                }
            } },
            { "pelayan_kafe", new WorkTheme
            {
                Title = "LAYANI TAMU KAFE", Icon = "lc_job_pelayan_kafe", Game = WorkGame.Choice, Rounds = 5, RoundName = "PESANAN",
                Instruction = "ANTARKAN MENU YANG SESUAI PESANAN TAMU",
                HitLines = new[] { "PESANAN BENAR!", "TAMU TERSENYUM!", "CEPAT & TEPAT!" }, MissLines = new[] { "SALAH MEJA/MENU!", "TAMU KECEWA..." },
                Result = "{0} dari {1} pesanan diantar dengan benar.",
                Questions = new[]
                {
                    new WorkQuestion("MEJA 2: ES KOPI SUSU GULA AREN", "ES KOPI SUSU", "TEH TARIK", "JUS ALPUKAT", "AIR MINERAL"),
                    new WorkQuestion("MEJA 5: CAPPUCCINO PANAS", "CAPPUCCINO", "ES TEH MANIS", "LATTE DINGIN", "COKELAT PANAS"),
                    new WorkQuestion("MEJA 1: ROTI BAKAR COKELAT", "ROTI BAKAR COKELAT", "KENTANG GORENG", "PISANG GORENG", "NASI GORENG"),
                    new WorkQuestion("MEJA 4: MATCHA LATTE DINGIN", "MATCHA LATTE", "ES JERUK", "KOPI TUBRUK", "SODA GEMBIRA"),
                    new WorkQuestion("MEJA 3: PISANG GORENG KEJU", "PISANG GORENG KEJU", "ROTI BAKAR", "DONAT", "CIRENG"),
                    new WorkQuestion("MEJA 6: KOPI TUBRUK TANPA GULA", "KOPI TUBRUK", "CAPPUCCINO", "ES KOPI SUSU", "TEH TAWAR"),
                }
            } },
            { "pegawai_kantor", new WorkTheme
            {
                Title = "SUSUN BERKAS KANTOR", Icon = "lc_job_pegawai_kantor", Game = WorkGame.Choice, Rounds = 5, RoundName = "BERKAS",
                Instruction = "MASUKKAN BERKAS KE MAP BAGIAN YANG BENAR",
                HitLines = new[] { "RAPI!", "ATASAN PUAS!", "TEPAT!" }, MissLines = new[] { "SALAH MAP!", "BERKAS NYASAR!" },
                Result = "{0} dari {1} berkas tersusun di map yang benar.",
                Questions = new[]
                {
                    new WorkQuestion("TAGIHAN LISTRIK BULAN INI", "KEUANGAN", "PERSONALIA", "PEMASARAN", "GUDANG"),
                    new WorkQuestion("LAMARAN KERJA CALON KARYAWAN", "PERSONALIA", "KEUANGAN", "GUDANG", "PEMASARAN"),
                    new WorkQuestion("BROSUR PROMOSI PRODUK BARU", "PEMASARAN", "PERSONALIA", "KEUANGAN", "HUKUM"),
                    new WorkQuestion("SURAT PERJANJIAN KERJA SAMA", "HUKUM", "GUDANG", "PEMASARAN", "PERSONALIA"),
                    new WorkQuestion("DAFTAR STOK BARANG MASUK", "GUDANG", "HUKUM", "KEUANGAN", "PERSONALIA"),
                    new WorkQuestion("SLIP GAJI KARYAWAN", "KEUANGAN", "PEMASARAN", "GUDANG", "HUKUM"),
                    new WorkQuestion("DATA CUTI KARYAWAN", "PERSONALIA", "HUKUM", "PEMASARAN", "KEUANGAN"),
                }
            } },
            { "guru", new WorkTheme
            {
                Title = "MENGAJAR DI KELAS", Icon = "lc_job_guru", Game = WorkGame.Choice, Rounds = 5, RoundName = "MURID BERTANYA",
                Instruction = "JAWAB PERTANYAAN MURID DENGAN BENAR",
                HitLines = new[] { "MURID PAHAM!", "KELAS BERTEPUK TANGAN!", "BENAR!" }, MissLines = new[] { "MURID BINGUNG...", "JAWABAN SALAH!" },
                Result = "{0} dari {1} pertanyaan murid terjawab dengan benar.",
                Questions = new[]
                {
                    new WorkQuestion("7 X 8 = ?", "56", "54", "48", "63"),
                    new WorkQuestion("IBU KOTA JAWA TENGAH?", "SEMARANG", "SURABAYA", "BANDUNG", "YOGYAKARTA"),
                    new WorkQuestion("LAWAN KATA DARI RAJIN?", "MALAS", "PINTAR", "TEKUN", "CEPAT"),
                    new WorkQuestion("125 + 75 = ?", "200", "190", "210", "175"),
                    new WorkQuestion("HEWAN PEMAKAN TUMBUHAN DISEBUT?", "HERBIVORA", "KARNIVORA", "OMNIVORA", "INSEKTIVORA"),
                    new WorkQuestion("PROKLAMASI KEMERDEKAAN RI TAHUN?", "1945", "1928", "1965", "1998"),
                    new WorkQuestion("PLANET TERDEKAT DARI MATAHARI?", "MERKURIUS", "VENUS", "BUMI", "MARS"),
                    new WorkQuestion("1 JAM = BERAPA MENIT?", "60", "100", "30", "24"),
                    new WorkQuestion("LAGU KEBANGSAAN INDONESIA?", "INDONESIA RAYA", "BAGIMU NEGERI", "SATU NUSA", "HARI MERDEKA"),
                }
            } },
            { "dokter", new WorkTheme
            {
                Title = "PRAKTIK DOKTER", Icon = "lc_job_dokter", Game = WorkGame.Choice, Rounds = 5, RoundName = "PASIEN",
                Instruction = "PERIKSA KELUHAN, PILIH PENANGANAN YANG TEPAT",
                HitLines = new[] { "PASIEN MEMBAIK!", "DIAGNOSIS TEPAT!" }, MissLines = new[] { "PENANGANAN KELIRU!", "PASIEN BELUM SEMBUH..." },
                Result = "{0} dari {1} pasien ditangani dengan tepat.",
                Questions = new[]
                {
                    new WorkQuestion("DEMAM TINGGI, BINTIK MERAH DI KULIT", "CEK DEMAM BERDARAH", "OBAT BATUK", "PERBAN LUKA", "TETES MATA"),
                    new WorkQuestion("BATUK BERDAHAK SUDAH 3 HARI", "OBAT BATUK + ISTIRAHAT", "OPERASI", "PASANG GIPS", "OBAT MAAG"),
                    new WorkQuestion("LUTUT LECET JATUH DARI MOTOR", "BERSIHKAN + PERBAN", "OBAT MAAG", "TETES MATA", "OBAT BATUK"),
                    new WorkQuestion("PERUT PERIH, SERING TELAT MAKAN", "OBAT MAAG + MAKAN TERATUR", "OBAT BATUK", "PERBAN", "SALEP KULIT"),
                    new WorkQuestion("GATAL DAN RUAM MERAH DI KULIT", "SALEP ANTIALERGI", "OBAT MAAG", "OBAT BATUK", "PASANG GIPS"),
                    new WorkQuestion("TANGAN BENGKAK SETELAH JATUH", "RONTGEN + GIPS", "OBAT BATUK", "VITAMIN", "TETES MATA"),
                    new WorkQuestion("MATA MERAH DAN BERAIR", "TETES MATA", "OBAT MAAG", "PASANG GIPS", "OBAT BATUK"),
                }
            } },

            // ------------------------------------------------------------ KOMBO PANAH
            { "kurir_makanan", new WorkTheme
            {
                Title = "ANTAR PESANAN", Icon = "lc_job_kurir_makanan", Game = WorkGame.Combo, Rounds = 3, RoundName = "PESANAN",
                Instruction = "IKUTI ARAH BELOKAN JALAN SAMPAI TUJUAN",
                HitLines = new[] { "PESANAN TERANTAR!", "BINTANG 5!" }, MissLines = new[] { "SALAH BELOK!", "NYASAR..." },
                Result = "{0} dari {1} pesanan sampai tepat waktu.",
                Tasks = new[] { "ANTAR NASI PADANG KE GANG MAWAR NO. 5", "ANTAR MARTABAK KE KOST PELANGI", "ANTAR KOPI KE KANTOR PT MAJU BERSAMA", "ANTAR SATE KE PERUMAHAN ASRI" },
            } },
            { "bengkel", new WorkTheme
            {
                Title = "SERVIS MOTOR", Icon = "lc_job_bengkel", Game = WorkGame.Combo, Rounds = 4, RoundName = "MOTOR",
                Instruction = "KERJAKAN LANGKAH SERVIS SESUAI URUTAN",
                HitLines = new[] { "MESIN HALUS!", "BERES, MOTOR SIAP!" }, MissLines = new[] { "BAUT SELIP!", "SALAH URUTAN!" },
                Result = "{0} dari {1} motor selesai diservis dengan baik.",
                Tasks = new[] { "GANTI OLI MESIN", "SETEL RANTAI MOTOR", "GANTI KAMPAS REM", "TAMBAL BAN BOCOR", "BERSIHKAN KARBURATOR" },
            } },
            { "koki_restoran", new WorkTheme
            {
                Title = "MASAK DI DAPUR", Icon = "lc_job_koki_restoran", Game = WorkGame.Combo, Rounds = 4, RoundName = "MASAKAN",
                Instruction = "IKUTI LANGKAH RESEP: POTONG, TUMIS, ADUK, SAJIKAN",
                HitLines = new[] { "LEZAT!", "PELANGGAN MINTA TAMBAH!" }, MissLines = new[] { "GOSONG!", "KEASINAN!" },
                Result = "{0} dari {1} masakan keluar dengan rasa sempurna.",
                Tasks = new[] { "NASI GORENG SPESIAL", "SOTO AYAM", "AYAM GEPREK SAMBAL BAWANG", "MI GORENG JAWA", "GADO-GADO" },
            } },
            { "desainer_grafis", new WorkTheme
            {
                Title = "KERJAKAN DESAIN", Icon = "lc_job_desainer_grafis", Game = WorkGame.Combo, Rounds = 4, RoundName = "PROYEK",
                Instruction = "GAMBAR GARIS DESAIN MENGIKUTI ARAH PANAH",
                HitLines = new[] { "KLIEN SUKA!", "DESAIN KEREN!" }, MissLines = new[] { "GARIS MENCONG!", "REVISI LAGI..." },
                Result = "{0} dari {1} desain disetujui klien tanpa revisi.",
                Tasks = new[] { "LOGO WARUNG KOPI", "POSTER KONSER KAMPUS", "SPANDUK 17 AGUSTUS", "KEMASAN KERIPIK PEDAS", "MENU KAFE SENJA" },
            } },
            { "perawat", new WorkTheme
            {
                Title = "TUGAS JAGA PERAWAT", Icon = "lc_job_perawat", Game = WorkGame.Combo, Rounds = 4, RoundName = "PASIEN",
                Instruction = "LAKUKAN PROSEDUR PERAWATAN SESUAI URUTAN",
                HitLines = new[] { "PROSEDUR TEPAT!", "PASIEN NYAMAN!" }, MissLines = new[] { "PROSEDUR TERLEWAT!", "ULANGI DARI AWAL!" },
                Result = "{0} dari {1} pasien dirawat sesuai prosedur.",
                Tasks = new[] { "UKUR TENSI PASIEN", "PASANG INFUS", "GANTI PERBAN LUKA", "BERI OBAT SESUAI DOSIS", "CEK SUHU TUBUH" },
            } },

            // ------------------------------------------------------------ TEKAN BERULANG
            { "petani", new WorkTheme
            {
                Title = "BERTANI DI SAWAH", Icon = "lc_job_petani", Game = WorkGame.Mash, Rounds = 3, RoundName = "PEKERJAAN",
                Instruction = "TEKAN E / SPASI BERULANG-ULANG SEBELUM WAKTU HABIS",
                HitLines = new[] { "SELESAI!", "HASILNYA BAGUS!" }, MissLines = new[] { "BELUM SELESAI...", "KEHABISAN WAKTU!" },
                Result = "{0} dari {1} pekerjaan sawah selesai tepat waktu.",
                Tasks = new[] { "CANGKUL TANAH SAWAH", "TANAM BIBIT PADI", "PANEN SAYURAN", "ANGKUT KARUNG GABAH" },
            } },
            { "tukang_kebun", new WorkTheme
            {
                Title = "MERAWAT KEBUN", Icon = "lc_job_tukang_kebun", Game = WorkGame.Mash, Rounds = 3, RoundName = "PEKERJAAN",
                Instruction = "TEKAN E / SPASI BERULANG-ULANG SEBELUM WAKTU HABIS",
                HitLines = new[] { "KEBUN RAPI!", "SELESAI!" }, MissLines = new[] { "BELUM BERES...", "KEHABISAN WAKTU!" },
                Result = "{0} dari {1} pekerjaan kebun selesai dengan rapi.",
                Tasks = new[] { "CABUT RUMPUT LIAR", "SIRAM SEMUA TANAMAN", "PANGKAS PAGAR TANAMAN", "SAPU DAUN KERING" },
            } },

            // ------------------------------------------------------------ TEPAT WAKTU
            { "tukang_cukur", new WorkTheme
            {
                Title = "CUKUR RAMBUT", Icon = "lc_job_tukang_cukur", Game = WorkGame.Timing, Rounds = 4, RoundName = "BAGIAN",
                Instruction = "GUNTING TEPAT SAAT PENANDA DI ZONA HIJAU",
                ZoneHalf = 0.1f,
                HitLines = new[] { "RAPI!", "GANTENG!", "PELANGGAN PUAS!" }, MissLines = new[] { "KEPENDEKAN!", "MIRING!" },
                Result = "{0} dari {1} potongan rambut rapi.",
                Tasks = new[] { "RAPIKAN BAGIAN SAMPING", "CUKUR BAGIAN BELAKANG", "RAPIKAN PONI", "CUKUR JENGGOT TIPIS", "BENTUK GAYA UNDERCUT" },
            } },
            { "nelayan", new WorkTheme
            {
                Title = "MEMANCING DI LAUT", Icon = "lc_job_nelayan", Game = WorkGame.Timing, Rounds = 4, RoundName = "TARIKAN",
                Instruction = "TARIK JORAN SAAT PENANDA MENGENAI IKAN (ZONA BERGERAK)",
                MovingZone = true, ZoneHalf = 0.11f,
                HitLines = new[] { "DAPAT!", "IKAN BESAR!" }, MissLines = new[] { "IKANNYA LEPAS!", "UMPAN HILANG..." },
                Result = "{0} dari {1} tarikan berhasil mendapat ikan.",
                Tasks = new[] { "IKAN KAKAP MERAH", "IKAN TONGKOL", "CUMI-CUMI", "IKAN BAWAL", "IKAN KERAPU" },
            } },

            // ------------------------------------------------------------ BIDIK
            { "fotografer", new WorkTheme
            {
                Title = "SESI FOTO", Icon = "lc_job_fotografer", Game = WorkGame.Aim, Rounds = 4, RoundName = "FOTO",
                Instruction = "ARAHKAN KAMERA KE OBJEK (WASD) LALU JEPRET (E)",
                Target = "lc_warga2_down_0",
                HitLines = new[] { "JEPRET! FOTONYA BAGUS!", "FOKUS SEMPURNA!" }, MissLines = new[] { "BURAM!", "OBJEKNYA KEPOTONG!" },
                Result = "{0} dari {1} foto layak dicetak.",
                Tasks = new[] { "FOTO PENGANTIN DI ALUN-ALUN", "FOTO WISUDA", "FOTO KELUARGA", "FOTO ANAK BERMAIN", "FOTO PRODUK UMKM" },
            } },
            { "polisi", new WorkTheme
            {
                Title = "PATROLI LALU LINTAS", Icon = "lc_job_polisi", Game = WorkGame.Aim, Rounds = 4, RoundName = "PELANGGAR",
                Instruction = "KEJAR PELANGGAR (WASD) LALU HENTIKAN (E)",
                Target = "lc_vehicle_motorcycle",
                HitLines = new[] { "PELANGGAR DIHENTIKAN!", "TILANG!" }, MissLines = new[] { "LOLOS!", "SALAH ORANG!" },
                Result = "{0} dari {1} pelanggar berhasil dihentikan.",
                Tasks = new[] { "PEMOTOR TANPA HELM", "MENEROBOS LAMPU MERAH", "MELAWAN ARUS", "BONCENG TIGA", "KEBUT-KEBUTAN" },
            } },
        };

        /// <summary>Tema untuk pekerjaan (atau "bantu_bengkel"); yang tidak terdaftar memakai tema umum.</summary>
        public static WorkTheme For(string jobId)
        {
            if (jobId == "bantu_bengkel")
            {
                var b = Themes["bengkel"];
                return new WorkTheme
                {
                    Title = "BANTU PAK JOKO", Icon = b.Icon, Game = b.Game, Rounds = 3, RoundName = b.RoundName,
                    Instruction = b.Instruction, HitLines = b.HitLines, MissLines = b.MissLines, Result = b.Result, Tasks = b.Tasks,
                };
            }
            if (jobId != null && Themes.TryGetValue(jobId, out var t)) return t;
            return new WorkTheme
            {
                Title = "BEKERJA", Icon = "lc_item_gear", Game = WorkGame.Timing, Rounds = 3, RoundName = "TUGAS",
                Instruction = "TEKAN SAAT PENANDA DI ZONA HIJAU",
                Tasks = new[] { "SELESAIKAN TUGAS" },
            };
        }
    }
}
