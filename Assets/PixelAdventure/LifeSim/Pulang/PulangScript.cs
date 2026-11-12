using System.Collections.Generic;

namespace PixelAdventure.LifeSim.Pulang
{
    public struct Line
    {
        public string Speaker;   // "{nama}" untuk tokoh utama, "" untuk narasi
        public string Text;

        public Line(string speaker, string text) { Speaker = speaker; Text = text; }
    }

    public struct Choice
    {
        public string Label;
        public Line[] Reply;
        public string Effect;    // contoh "keluarga+2", "flag:tanya_merantau"; dibaca oleh runner

        public Choice(string label, string effect, params Line[] reply)
        {
            Label = label; Effect = effect; Reply = reply;
        }
    }

    public class SceneDef
    {
        public string Id;
        public Line[] Lines = new Line[0];
        public Choice[] Choices = new Choice[0]; // muncul setelah Lines
        public Line[] After = new Line[0];       // setelah pilihan, sama untuk semua
    }

    /// <summary>
    /// Naskah adegan utama, disalin dari dokumen. Nama tokoh utama selalu "{nama}".
    /// Runner/HUD: tampilkan PulangProfile.Format(speaker/text), lalu Notify(SceneDone, Id).
    /// </summary>
    public static partial class PulangScript
    {
        const string P = "{nama}";
        const string Ratna = "Ratna";
        const string Keluarga = "Keluarga";
        const string N = "";

        static Line L(string s, string t) => new Line(s, t);

        public static readonly Dictionary<string, SceneDef> Scenes = Build(
            new SceneDef
            {
                Id = "prolog_rumah",
                Lines = new[]
                {
                    L(N, "Desa Suka Makmur. Pagi hari."),
                    L(Ratna, P + ", kamu sudah bangun, Nak?"),
                    L(P, "Iya, Bu. Memangnya ada apa?"),
                    L(Ratna, "Ibu cuma ingin mengajakmu sarapan."),
                    L(P, "Sebentar lagi, Bu."),
                    L(Ratna, "Kamu sekarang sudah besar. Ibu cuma berharap kamu bisa mendapatkan kehidupan yang lebih baik."),
                    L(P, "Iya, Bu. " + P + " juga ingin sukses."),
                    L(Ratna, "Kalau suatu saat kamu pergi jauh, jangan lupa pulang."),
                    L(P, "Tenang saja, Bu. " + P + " pasti pulang."),
                },
                Choices = new[]
                {
                    new Choice(P + " ingin membuat Ibu bangga.", "keluarga+2",
                        L(Ratna, "Ibu tidak meminta kamu menjadi orang kaya, Nak. Ibu cuma ingin kamu bahagia.")),
                    new Choice(P + " ingin mencoba kehidupan di kota.", "flag:dialog_merantau",
                        L(Ratna, "Kalau memang itu keinginanmu, Ibu akan mendukung.")),
                    new Choice(P + " masih belum tahu apa yang harus dilakukan.", "flag:dialog_tambahan",
                        L(Ratna, "Tidak apa-apa. Semua orang punya waktunya masing-masing.")),
                },
            },
            new SceneDef
            {
                Id = "prolog_keputusan",
                Lines = new[]
                {
                    L(N, "Beberapa hari kemudian, " + P + " mulai memikirkan masa depannya."),
                    L(P, "Bu, " + P + " ingin mencoba merantau ke Jogokerto."),
                    L(Ratna, "Jogokerto? Jauh juga dari sini."),
                    L(P, "Iya, Bu. " + P + " ingin mencari pekerjaan dan membangun kehidupan sendiri."),
                    L(Ratna, "Kalau itu keputusanmu, Ibu tidak akan melarang."),
                    L(P, P + " janji akan sering menghubungi Ibu."),
                    L(Ratna, "Ibu tunggu kabarmu, Nak."),
                },
                Choices = new[]
                {
                    new Choice(P + " akan segera mencari pekerjaan.", "",
                        L(Ratna, "Semangat, Nak. Jangan lupa menjaga kesehatan.")),
                    new Choice(P + " ingin mengumpulkan uang terlebih dahulu.", "",
                        L(Ratna, "Gunakan uangmu dengan bijak. Jangan terlalu memaksakan diri.")),
                    new Choice(P + " ingin belajar hidup mandiri.", "",
                        L(Ratna, "Ibu bangga kamu mau mencoba.")),
                },
            },
            new SceneDef
            {
                Id = "ch1_tiba",
                Lines = new[]
                {
                    L(N, "Terminal Jogokerto. " + P + " turun dan melihat suasana kota yang ramai."),
                    L(P, "Akhirnya sampai juga di Jogokerto."),
                    L(P, "Mulai sekarang aku harus bisa mengurus semuanya sendiri."),
                },
            },
            new SceneDef
            {
                Id = "ch1_penutup",
                Lines = new[]
                {
                    L(N, "Malam hari. Sebuah pesan masuk dari Ibu."),
                    L(Ratna, "Bagaimana keadaanmu di sana, Nak?"),
                    L(P, "Baik, Bu. " + P + " sudah mulai mengenal kota."),
                    L(Ratna, "Syukurlah. Jangan lupa makan dan istirahat."),
                    L(P, "Iya, Bu. Nanti kalau sudah sukses, " + P + " pulang."),
                    L(N, "CHAPTER 1 COMPLETED – KEHIDUPAN BARU"),
                },
            },
            new SceneDef
            {
                Id = "ch5_pesan_ibu",
                Lines = new[]
                {
                    L(Ratna, P + ", bagaimana pekerjaanmu sekarang?"),
                    L(P, "Baik, Bu. Sekarang " + P + " sudah mendapatkan pekerjaan yang lebih bagus."),
                    L(Ratna, "Alhamdulillah. Ibu ikut senang."),
                    L(P, "Nanti kalau semuanya sudah selesai, " + P + " pulang."),
                    L(Ratna, "Ibu tunggu, Nak."),
                },
                Choices = new[]
                {
                    new Choice(P + " akan segera pulang setelah urusannya selesai.", "keluarga+1",
                        L(Ratna, "Iya, Nak. Jangan terlalu memaksakan diri.")),
                    new Choice(P + " ingin mencapai kesuksesan lebih dahulu.", "",
                        L(Ratna, "Ibu mengerti. Tetapi jangan lupa menjaga dirimu.")),
                    new Choice(P + " merindukan rumah.", "keluarga+2",
                        L(Ratna, "Ibu juga merindukanmu.")),
                },
            },
            new SceneDef
            {
                Id = "ch6_telepon",
                Lines = new[]
                {
                    L(N, "Malam hari. Ponsel " + P + " berdering. Nama yang muncul: keluarga dari Desa Suka Makmur."),
                    L(Keluarga, P + "..."),
                    L(P, "Iya, ada apa?"),
                    L(Keluarga, "Kamu bisa pulang sekarang?"),
                    L(P, "Kenapa? Ada sesuatu?"),
                    L(Keluarga, "Ini tentang ibumu."),
                    L(P, "Ibu kenapa?"),
                    L(Keluarga, "Maaf, " + P + ". Ibumu sudah meninggal dunia."),
                    L(N, P + " terdiam."),
                    L(P, "Enggak... kapan?"),
                    L(Keluarga, "Beberapa waktu lalu. Kami sudah berusaha menghubungimu."),
                    L(P, "Kenapa aku baru tahu sekarang?"),
                    L(Keluarga, "Kami berharap kamu segera pulang."),
                },
                // Semua pilihan menuju quest yang sama (Perjalanan Terakhir).
                Choices = new[]
                {
                    new Choice("Aku akan pulang sekarang.", "",
                        L(Keluarga, "Baik, " + P + ". Kami tunggu.")),
                    new Choice("Aku ingin tahu semuanya terlebih dahulu.", "flag:dialog_tambahan_duka",
                        L(Keluarga, "Kami akan menjelaskan semuanya saat kamu sampai di rumah.")),
                    new Choice("Aku ingin segera melihat rumah.", "flag:dialog_emosional",
                        L(Keluarga, "Rumah masih sama, " + P + ". Pulanglah.")),
                },
            },
            new SceneDef
            {
                Id = "ending_pulang",
                Lines = new[]
                {
                    L(N, "Desa Suka Makmur. Sore menjelang malam."),
                    L(N, "Suasana desa masih sama seperti dahulu."),
                    L(P, "Sudah lama sekali aku tidak melewati jalan ini."),
                    L(P, "Bu... " + P + " pulang."),
                    L(N, "Tidak ada jawaban."),
                    L(N, P + " menemukan barang peninggalan ibunya dan sebuah surat."),
                    L(Ratna, "Untuk " + P + ", anak Ibu."),
                    L(Ratna, "Kalau suatu hari kamu membaca surat ini, berarti kamu sudah tumbuh menjadi seseorang yang lebih dewasa."),
                    L(Ratna, "Ibu selalu berharap kamu mendapatkan kehidupan yang baik."),
                    L(Ratna, "Jangan pernah merasa harus menjadi orang hebat hanya untuk membuat Ibu bangga."),
                    L(Ratna, "Ibu sudah bangga sejak kamu berusaha menjalani hidupmu sendiri."),
                    L(Ratna, "Ibu hanya ingin kamu ingat satu hal."),
                    L(Ratna, "Sejauh apa pun kamu pergi, jangan pernah melupakan rumah."),
                    L(Ratna, "Ibu selalu menyayangimu. Dari Ibu."),
                    L(P, "Bu... " + P + " sudah berhasil."),
                    L(P, P + " sudah mendapatkan pekerjaan yang dulu " + P + " impikan."),
                    L(P, P + " juga sudah punya banyak teman."),
                    L(P, "Tapi kenapa saat " + P + " akhirnya pulang, Ibu sudah tidak ada?"),
                    L(P, "Maaf, Bu."),
                    L(P, "Maaf karena " + P + " terlalu sibuk mengejar masa depan."),
                    L(P, P + " pikir masih ada banyak waktu."),
                    L(P, "Ternyata " + P + " salah."),
                    L(P, P + " sudah pulang, Bu."),
                    L(N, P + " duduk di depan rumah. Kamera perlahan menjauh. Layar perlahan gelap."),
                    L(N, "PULANG"),
                    L(N, "Pada akhirnya, perjalanan terjauh bukanlah perjalanan menuju kesuksesan, " +
                         "melainkan perjalanan kembali ke tempat yang selalu menunggu kita."),
                    L(N, "THE END"),
                },
            }
        );

        static Dictionary<string, SceneDef> Build(params SceneDef[] scenes)
        {
            var map = new Dictionary<string, SceneDef>();
            foreach (var s in scenes) map[s.Id] = s;
            foreach (var s in ChapterScenes()) map[s.Id] = s;
            foreach (var s in MidChapterScenes()) map[s.Id] = s;
            return map;
        }
    }
}
