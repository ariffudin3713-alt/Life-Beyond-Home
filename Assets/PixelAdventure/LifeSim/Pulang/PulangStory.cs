using System;
using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure.LifeSim.Pulang
{
    /// <summary>
    /// Alur utama, urut sesuai Bagian 6–8 dokumen. Tidak ada cabang ending:
    /// setiap jalan berakhir di <see cref="StoryState.EndingPulang"/>.
    /// </summary>
    public enum StoryState
    {
        NameEntry,

        // PROLOG – Kehidupan sebelum merantau
        PrologRumah,          // Adegan 1
        PrologKeputusan,      // Adegan 2
        PrologPersiapan,      // Quest 1: Persiapan Merantau

        // CHAPTER 1 – Awal kehidupan di Jogokerto
        Ch1Tiba,              // Adegan 1: tiba di terminal
        Ch1MengenalKota,      // Quest 2 + Adegan 2 (Pak Hendra) + Adegan 3 (Dimas)
        Ch1Kebutuhan,         // Quest 3
        Ch1PekerjaanPertama,  // Quest 4
        Ch1MembantuNabila,    // Adegan 4 + Quest 5
        Ch1Penutup,           // pesan malam dari Ratna

        // CHAPTER 2 – Mengenal dunia baru
        Ch2MembantuDimas,     // Quest 6
        Ch2MasalahPasar,      // Quest 7
        Ch2MasalahKendaraan,  // Quest 8

        // CHAPTER 3 – Membangun masa depan
        Ch3KesempatanBaru,    // Quest 9 (Pak Budi)
        Ch3MeningkatkanKemampuan, // Quest 10: kursus skill di perpustakaan
        Ch3WajahBaru,         // Quest 11: Melati (Gang Melati) & Citra (Alun-Alun) – episode 1
        Ch3DuniaBisnis,       // Quest 12: belajar usaha dari Pak Darto & Bu Sari
        Ch3MembangunKarier,   // bebas: karier & bisnis (capai level menengah)

        // CHAPTER 4 – Kehidupan dan hubungan
        Ch4TemanSeperjuangan, // Quest 13: menemani Dimas mencari pekerjaan
        Ch4PesanKeluarga,     // Quest 14: surat Pak Hendra untuk Pak Slamet, cerita masa lalu
        Ch4WaktuBersamaTeman, // Quest 15: menghabiskan waktu dengan teman-teman
        Ch4KehidupanHubungan, // bebas: PDKT, pekerjaan, bisnis (capai level profesional)

        // CHAPTER 5 – Kesuksesan
        Ch5KirimanIbu,        // Pesan dari Ibu + Quest 16: kirim oleh-oleh ke desa
        Ch5KerjaBakti,        // Quest 17: kerja bakti Kampung Cempaka
        Ch5Kesuksesan,        // bebas sampai target sukses tercapai

        // CHAPTER 6 – Kabar yang mengubah segalanya
        Ch6KabarDuka,         // telepon keluarga
        Ch6PerjalananTerakhir,// Main quest terakhir

        // BAGIAN 8 – Ending tunggal
        EndingPulang,
        TheEnd,
    }

    public enum StoryEvent
    {
        SceneDone,   // key = id adegan
        Talk,        // key = NpcId
        Arrive,      // key = LocationId
        Travel,      // key = Region tujuan
        Pickup,      // key = id barang
        Buy,         // key = id barang
        Deliver,     // key = id pengiriman
        WorkShift,   // key = id pekerjaan, "*" = pekerjaan apa saja
        Skill,       // key = id pekerjaan yang kursusnya dibeli
        Save,        // key = "*" (menabung di kost)
        GirlTalk,    // key = NpcId teman perempuan (mengobrol, sekali sehari per orang)
        Gift,        // key = NpcId penerima hadiah
        Litter,      // key = "*" (membuang sampah yang dipungut ke tong sampah), sekali per sampah
    }

    /// <summary>Angka yang tidak disebut dokumen. Satu tempat untuk disetel desainer.</summary>
    public static class PulangBalance
    {
        public const int StartMoney = 100000;
        public const int NightStartMinutes = 19 * 60;   // "pada malam hari"

        public const int Ch3ToCh4Level = 5;             // level pekerjaan keterampilan (Montir, Fotografer)
        public const int Ch4ToCh5Level = 10;            // level pekerjaan profesional pertama (Guru)
        public const int SuccessSavings = 2000000;      // target sukses -> memicu Chapter 6
    }

    public enum JobTier { None, Sederhana, Keterampilan, Alam, Profesional }

    [Serializable]
    public class GirlProgress
    {
        public bool met;
        public int closeness;
        public int episode;     // 0..10, episode 10 = kenangan, bukan ending
    }

    [Serializable]
    public class PulangSaveData
    {
        public const string Key = "Pulang.Save.v1";

        public int version = CurrentVersion;

        /// <summary>v2: misi baru Chapter 3–5 disisipkan ke urutan cerita.</summary>
        public const int CurrentVersion = 2;

        /// <summary>Urutan StoryState pada simpanan versi 1 (sebelum misi Chapter 3–5 ditambahkan).</summary>
        static readonly string[] V1States =
        {
            "NameEntry", "PrologRumah", "PrologKeputusan", "PrologPersiapan",
            "Ch1Tiba", "Ch1MengenalKota", "Ch1Kebutuhan", "Ch1PekerjaanPertama", "Ch1MembantuNabila", "Ch1Penutup",
            "Ch2MembantuDimas", "Ch2MasalahPasar", "Ch2MasalahKendaraan",
            "Ch3KesempatanBaru", "Ch3MembangunKarier", "Ch4KehidupanHubungan", "Ch5Kesuksesan",
            "Ch6KabarDuka", "Ch6PerjalananTerakhir", "EndingPulang", "TheEnd",
        };

        /// <summary>
        /// Ubah simpanan lama ke urutan cerita baru. Pemain yang sedang di bagian "bebas"
        /// Chapter 3/4/5 dibawa ke misi baru pertama chapter tersebut agar tidak melewatkannya.
        /// </summary>
        void Migrate()
        {
            if (version >= CurrentVersion) return;
            int old = (int)state;
            string name = old >= 0 && old < V1States.Length ? V1States[old] : "NameEntry";
            var s = (StoryState)Enum.Parse(typeof(StoryState), name);
            StoryState restart = s == StoryState.Ch3MembangunKarier ? StoryState.Ch3MeningkatkanKemampuan
                : s == StoryState.Ch4KehidupanHubungan ? StoryState.Ch4TemanSeperjuangan
                : s == StoryState.Ch5Kesuksesan ? StoryState.Ch5KirimanIbu
                : s;
            state = restart;
            if (restart != s)
            {
                var q = PulangStory.QuestFor(restart);
                objectiveProgress = new int[q != null ? q.Objectives.Length : 0];
                sceneShown = false;
            }
            version = CurrentVersion;
        }
        public string playerName = PulangProfile.DefaultName;

        // Cerita
        public StoryState state = StoryState.NameEntry;
        public int[] objectiveProgress = new int[0];
        public bool sceneShown;

        // Kehidupan (Bagian 5)
        public int day = 1;
        public int minutes = 6 * 60;
        public int money = PulangBalance.StartMoney;
        public int savings;
        public int energy = 100;
        // Kebutuhan tubuh (100 = kenyang / segar, 0 = kelaparan / kehausan). Simpanan lama
        // tanpa field ini mulai dari 100.
        public float hunger = 100f;
        public float thirst = 100f;
        public int level = 1;
        public int exp;
        public string jobMain = "";
        public string jobSide = "";
        public JobTier jobTier = JobTier.None;
        public int reputation;
        public int familyCloseness;           // hubungan dengan Ratna
        public Region region = Region.DesaSukaMakmur;
        public LocationId location = LocationId.RumahZidan;

        public List<string> inventory = new List<string>();
        public string vehicle = "";                            // kendaraan yang sedang dinaiki ("" = jalan kaki)
        public List<string> messages = new List<string>();     // pesan tablet: "NpcId|1 (aku) / 0 (dia)|hari|teks"
        public List<string> calls = new List<string>();        // telepon terakhir: "NpcId|hari"
        public List<string> skills = new List<string>();      // "skill_<jobId>"
        public int onTimeStreak;
        public float posX = -1f, posY = -1f;

        public GirlProgress[] girls = NewGirls();

        // Misi sampingan (lihat PulangSideQuests)
        public List<SideQuestProgress> side = new List<SideQuestProgress>();

        // Pakaian yang sedang dipakai per slot (id barang; "" = bawaan tokoh utama).
        // Simpanan lama hanya punya "outfit"; "hat"/"shoes" yang tidak ada di JSON tetap "".
        public string outfit = "";   // slot BAJU
        public string hat = "";      // slot TOPI
        public string shoes = "";    // slot SEPATU
        public List<string> flags = new List<string>();

        static GirlProgress[] NewGirls()
        {
            var g = new GirlProgress[PulangRomance.Girls.Length];
            for (int i = 0; i < g.Length; i++) g[i] = new GirlProgress();
            return g;
        }

        public bool Has(string flag) => flags.Contains(flag);
        public void Set(string flag) { if (!flags.Contains(flag)) flags.Add(flag); }

        // ------------------------------------------------------------ slot simpanan

        public const int SlotCount = 3;
        const string LastSlotKey = "Pulang.Save.LastSlot";

        /// <summary>Slot yang sedang dimainkan (0..SlotCount-1). Save() menulis ke slot ini.</summary>
        public static int ActiveSlot { get; set; }

        public string savedAt = "";   // waktu simpan terakhir, untuk layar slot

        static string SlotKey(int slot) => Key + ".slot" + slot;

        public void Save()
        {
            savedAt = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            PlayerPrefs.SetString(SlotKey(ActiveSlot), JsonUtility.ToJson(this));
            PlayerPrefs.SetInt(LastSlotKey, ActiveSlot);
            PlayerPrefs.Save();
        }

        public static bool HasSlot(int slot) => PlayerPrefs.HasKey(SlotKey(slot));

        public static bool AnySave()
        {
            for (int i = 0; i < SlotCount; i++) if (HasSlot(i)) return true;
            return false;
        }

        /// <summary>Slot terakhir dimainkan yang masih berisi; -1 bila tidak ada simpanan.</summary>
        public static int LastSlot()
        {
            int last = PlayerPrefs.GetInt(LastSlotKey, -1);
            if (last >= 0 && last < SlotCount && HasSlot(last)) return last;
            for (int i = 0; i < SlotCount; i++) if (HasSlot(i)) return i;
            return -1;
        }

        /// <summary>Baca isi slot tanpa mengubah nama tokoh (untuk pratinjau).</summary>
        public static PulangSaveData Peek(int slot)
        {
            if (!HasSlot(slot)) return null;
            try
            {
                var data = JsonUtility.FromJson<PulangSaveData>(PlayerPrefs.GetString(SlotKey(slot)));
                // Simpanan tanpa field "version" (sangat lama) dianggap versi 1.
                if (data != null && !PlayerPrefs.GetString(SlotKey(slot)).Contains("\"version\"")) data.version = 1;
                data?.Migrate();
                return data;
            }
            catch (Exception) { return null; }
        }

        /// <summary>Muat slot dan jadikan slot aktif.</summary>
        public static PulangSaveData Load(int slot)
        {
            var data = Peek(slot);
            if (data == null) return null;
            ActiveSlot = slot;
            PulangProfile.SetName(data.playerName);
            return data;
        }

        public static void DeleteSlot(int slot)
        {
            PlayerPrefs.DeleteKey(SlotKey(slot));
            if (PlayerPrefs.GetInt(LastSlotKey, -1) == slot) PlayerPrefs.DeleteKey(LastSlotKey);
            PlayerPrefs.Save();
        }

        /// <summary>Hapus slot yang sedang aktif.</summary>
        public static void Delete() => DeleteSlot(ActiveSlot);

        /// <summary>Simpanan versi lama (satu slot) dipindah ke slot 1 bila slot itu kosong.</summary>
        public static void MigrateLegacy()
        {
            if (!PlayerPrefs.HasKey(Key)) return;
            if (!HasSlot(0))
            {
                PlayerPrefs.SetString(SlotKey(0), PlayerPrefs.GetString(Key));
                PlayerPrefs.SetInt(LastSlotKey, 0);
            }
            PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.Save();
        }
    }

    public struct Objective
    {
        public string Text;
        public StoryEvent Event;
        public string Key;
        public int Count;

        public Objective(string text, StoryEvent ev, string key, int count = 1)
        {
            Text = text; Event = ev; Key = key; Count = count;
        }
    }

    public class QuestDef
    {
        public StoryState State;
        public Chapter Chapter;
        public string Title;
        public string EnterScene;                     // diputar saat masuk state
        public bool SceneAtNight;                     // tunggu malam sebelum adegan
        public bool Sequential;                       // tujuan harus berurutan
        public Objective[] Objectives = new Objective[0];
        public Func<PulangSaveData, bool> Gate;       // state bebas: syarat lanjut
        public string GateText;                       // teks jurnal untuk Gate
    }

    public class PulangStory
    {
        public PulangSaveData Data { get; private set; }

        /// <summary>HUD memutar adegan dari <see cref="PulangScript"/> lalu memanggil Notify(SceneDone, id).</summary>
        public event Action<string> PlayScene;
        public event Action<StoryState> StateChanged;
        public event Action<string> ObjectiveCompleted;
        public event Action GameFinished;
        /// <summary>Setiap kejadian (beli, tiba, kerja, hadiah, ...) sebelum diproses misi utama.</summary>
        public event Action<StoryEvent, string> AnyEvent;

        public QuestDef Current => Quests[Data.state];
        public Chapter Chapter => Current.Chapter;

        // ------------------------------------------------------------------
        //  Tabel alur
        // ------------------------------------------------------------------

        static Objective Scene(string id, string text) => new Objective(text, StoryEvent.SceneDone, id);
        static Objective Talk(NpcId n, string text) => new Objective(text, StoryEvent.Talk, n.ToString());
        static Objective Arrive(LocationId l, string text) => new Objective(text, StoryEvent.Arrive, l.ToString());

        static readonly QuestDef[] Table =
        {
            new QuestDef { State = StoryState.NameEntry, Chapter = Chapter.Prolog, Title = "Siapa namamu?" },

            new QuestDef { State = StoryState.PrologRumah, Chapter = Chapter.Prolog, Title = "Adegan 1: Rumah {nama}",
                EnterScene = "prolog_rumah",
                Objectives = new[] { Scene("prolog_rumah", "Bicara dengan Ibu") } },

            new QuestDef { State = StoryState.PrologKeputusan, Chapter = Chapter.Prolog, Title = "Adegan 2: Keputusan merantau",
                EnterScene = "prolog_keputusan",
                Objectives = new[] { Scene("prolog_keputusan", "Sampaikan keputusan kepada Ibu") } },

            new QuestDef { State = StoryState.PrologPersiapan, Chapter = Chapter.Prolog, Title = "Quest 1: Persiapan Merantau", Sequential = true,
                Objectives = new[]
                {
                    new Objective("Mengambil tas", StoryEvent.Pickup, "tas"),
                    new Objective("Mengumpulkan barang pribadi", StoryEvent.Pickup, "barang_pribadi"),
                    Talk(NpcId.Ratna, "Berbicara dengan Ratna"),
                    Scene("prolog_pamit", "Berpamitan"),
                    new Objective("Berangkat ke Kota Jogokerto", StoryEvent.Travel, Region.KotaJogokerto.ToString()),
                } },

            new QuestDef { State = StoryState.Ch1Tiba, Chapter = Chapter.Ch1, Title = "Chapter 1 – Awal Kehidupan di Jogokerto",
                EnterScene = "ch1_tiba",
                Objectives = new[] { Scene("ch1_tiba", "Turun di Terminal Jogokerto") } },

            new QuestDef { State = StoryState.Ch1MengenalKota, Chapter = Chapter.Ch1, Title = "Quest 2: Mengenal Jogokerto",
                Sequential = true,
                Objectives = new[]
                {
                    Arrive(LocationId.TerminalJogokerto, "Berjalan mengelilingi terminal"),
                    Talk(NpcId.PakHendra, "Menemui Pak Hendra"),
                    Scene("ch1_hendra", "Bertanya mengenai tempat tinggal"),
                    Arrive(LocationId.TokoSerbaAda, "Mencari tempat makan (Toko Serba Ada)"),
                    Arrive(LocationId.KostPelangi, "Menemukan Kost Pelangi"),
                    Talk(NpcId.Dimas, "Berkenalan dengan tetangga kost"),
                } },

            new QuestDef { State = StoryState.Ch1Kebutuhan, Chapter = Chapter.Ch1, Title = "Quest 3: Kebutuhan Pertama",
                Sequential = true,
                Objectives = new[]
                {
                    Arrive(LocationId.WarungBuSari, "Pergi ke Warung Bu Sari"),
                    Talk(NpcId.BuSari, "Berbicara dengan Bu Sari"),
                    new Objective("Membeli roti", StoryEvent.Buy, "roti"),
                    Arrive(LocationId.KostPelangi, "Kembali ke kost"),
                } },

            new QuestDef { State = StoryState.Ch1PekerjaanPertama, Chapter = Chapter.Ch1, Title = "Quest 4: Mencari Pekerjaan Pertama",
                Sequential = true,
                Objectives = new[]
                {
                    Scene("q4_dimas_kerja", "Tanya Dimas soal pekerjaan"),
                    new Objective("Selesaikan satu pekerjaan awal", StoryEvent.WorkShift, "*"),
                } },

            new QuestDef { State = StoryState.Ch1MembantuNabila, Chapter = Chapter.Ch1, Title = "Quest 5: Membantu Nabila",
                Sequential = true,
                Objectives = new[]
                {
                    Talk(NpcId.Nabila, "Temui seseorang di Taman Pelangi"),
                    new Objective("Temukan buku catatan kecil", StoryEvent.Pickup, "buku_catatan_nabila"),
                    Talk(NpcId.Nabila, "Kembalikan buku catatan kepada Nabila"),
                } },

            new QuestDef { State = StoryState.Ch1Penutup, Chapter = Chapter.Ch1, Title = "Pesan dari Ibu",
                EnterScene = "ch1_penutup", SceneAtNight = true,
                Objectives = new[] { Scene("ch1_penutup", "Tunggu malam, baca pesan Ibu") } },

            new QuestDef { State = StoryState.Ch2MembantuDimas, Chapter = Chapter.Ch2, Title = "Quest 6: Membantu Dimas",
                Sequential = true,
                Objectives = new[]
                {
                    Talk(NpcId.Dimas, "Dengarkan masalah Dimas"),
                    new Objective("Antar barang titipan Dimas", StoryEvent.Deliver, "paket_dimas", 3),
                } },

            new QuestDef { State = StoryState.Ch2MasalahPasar, Chapter = Chapter.Ch2, Title = "Quest 7: Masalah di Pasar",
                Sequential = true,
                Objectives = new[]
                {
                    Talk(NpcId.BuSari, "Temui Bu Sari di Pasar Ramai Jaya"),
                    new Objective("Antar barang ke pedagang", StoryEvent.Deliver, "barang_pasar", 3),
                } },

            new QuestDef { State = StoryState.Ch2MasalahKendaraan, Chapter = Chapter.Ch2, Title = "Quest 8: Masalah Kendaraan",
                Sequential = true,
                Objectives = new[]
                {
                    Talk(NpcId.PakJoko, "Temui Pak Joko di Bengkel Jaya Motor"),
                    new Objective("Bantu pekerjaan bengkel", StoryEvent.WorkShift, "bengkel"),
                } },

            new QuestDef { State = StoryState.Ch3KesempatanBaru, Chapter = Chapter.Ch3, Title = "Quest 9: Kesempatan Baru",
                Objectives = new[] { Talk(NpcId.PakBudi, "Dengarkan tawaran Pak Budi") } },

            new QuestDef { State = StoryState.Ch3MeningkatkanKemampuan, Chapter = Chapter.Ch3, Title = "Quest 10: Meningkatkan Kemampuan",
                Sequential = true,
                Objectives = new[]
                {
                    Scene("ch3_budi_kursus", "Tanyakan cara naik ke pekerjaan menengah kepada Pak Budi"),
                    Arrive(LocationId.Perpustakaan, "Pergi ke Perpustakaan"),
                    new Objective("Ambil satu kursus keterampilan", StoryEvent.Skill, "*"),
                    new Objective("Selesaikan satu shift kerja", StoryEvent.WorkShift, "*"),
                } },

            new QuestDef { State = StoryState.Ch3WajahBaru, Chapter = Chapter.Ch3, Title = "Quest 11: Wajah-Wajah Baru",
                Objectives = new[]
                {
                    Scene("ch3_melati", "Temui seseorang di Gang Melati"),
                    Scene("ch3_citra", "Temui fotografer muda di Alun-Alun Merdeka"),
                } },

            new QuestDef { State = StoryState.Ch3DuniaBisnis, Chapter = Chapter.Ch3, Title = "Quest 12: Mengenal Dunia Bisnis",
                Sequential = true,
                Objectives = new[]
                {
                    Scene("ch3_darto", "Belajar berdagang dari Pak Darto di Pasar Tradisional"),
                    new Objective("Sisihkan uang ke tabungan sebagai modal usaha", StoryEvent.Save, "*"),
                    Scene("ch3_sari_usaha", "Minta nasihat usaha kepada Bu Sari"),
                } },

            new QuestDef { State = StoryState.Ch3MembangunKarier, Chapter = Chapter.Ch3, Title = "Chapter 3 – Membangun Masa Depan",
                Gate = d => d.level >= PulangBalance.Ch3ToCh4Level,
                GateText = "Capai level " + PulangBalance.Ch3ToCh4Level + " (pekerjaan keterampilan)" },

            new QuestDef { State = StoryState.Ch4TemanSeperjuangan, Chapter = Chapter.Ch4, Title = "Quest 13: Teman Seperjuangan",
                Sequential = true,
                Objectives = new[]
                {
                    Scene("ch4_dimas", "Dengarkan cerita Dimas"),
                    Arrive(LocationId.PTMajuBersama, "Temani Dimas ke PT Maju Bersama"),
                    Scene("ch4_budi", "Tanyakan lowongan untuk Dimas kepada Pak Budi"),
                    Scene("ch4_dimas_selesai", "Sampaikan kabar baik kepada Dimas"),
                } },

            new QuestDef { State = StoryState.Ch4PesanKeluarga, Chapter = Chapter.Ch4, Title = "Quest 14: Pesan Keluarga",
                Sequential = true,
                Objectives = new[]
                {
                    Scene("ch4_hendra", "Temui Pak Hendra di Terminal Jogokerto"),
                    Scene("ch4_slamet", "Antarkan surat Pak Hendra kepada Pak Slamet di Kampung Cempaka"),
                    Scene("ch4_hendra_selesai", "Kembali kepada Pak Hendra"),
                } },

            new QuestDef { State = StoryState.Ch4WaktuBersamaTeman, Chapter = Chapter.Ch4, Title = "Quest 15: Waktu Bersama Teman",
                Objectives = new[]
                {
                    new Objective("Mengobrol dengan teman-temanmu", StoryEvent.GirlTalk, "*", 3),
                    new Objective("Berikan hadiah kepada seorang teman", StoryEvent.Gift, "*"),
                } },

            new QuestDef { State = StoryState.Ch4KehidupanHubungan, Chapter = Chapter.Ch4, Title = "Chapter 4 – Kehidupan dan Hubungan",
                Gate = d => d.level >= PulangBalance.Ch4ToCh5Level,
                GateText = "Capai level " + PulangBalance.Ch4ToCh5Level + " (pekerjaan profesional)" },

            new QuestDef { State = StoryState.Ch5KirimanIbu, Chapter = Chapter.Ch5, Title = "Quest 16: Kiriman untuk Ibu",
                EnterScene = "ch5_pesan_ibu", Sequential = true,
                Objectives = new[]
                {
                    Scene("ch5_pesan_ibu", "Baca pesan dari Ibu"),
                    new Objective("Beli kain batik untuk Ibu di Toko Pakaian", StoryEvent.Buy, "oleh_oleh"),
                    Scene("ch5_hadi", "Kirim paket ke desa lewat Pak Hadi di terminal"),
                } },

            new QuestDef { State = StoryState.Ch5KerjaBakti, Chapter = Chapter.Ch5, Title = "Quest 17: Kerja Bakti",
                Sequential = true,
                Objectives = new[]
                {
                    Scene("ch5_fajar", "Dengarkan ajakan Mas Fajar di Lapangan Kampung"),
                    Arrive(LocationId.GangMelati, "Bersihkan Gang Melati"),
                    Scene("ch5_slamet", "Lapor kepada Pak Slamet di Pos Ronda"),
                } },

            new QuestDef { State = StoryState.Ch5Kesuksesan, Chapter = Chapter.Ch5, Title = "Chapter 5 – Kesuksesan",
                Gate = d => d.jobTier == JobTier.Profesional && d.savings >= PulangBalance.SuccessSavings,
                GateText = "Bekerja profesional dan menabung Rp" + PulangBalance.SuccessSavings.ToString("N0") },

            new QuestDef { State = StoryState.Ch6KabarDuka, Chapter = Chapter.Ch6, Title = "Chapter 6 – Kabar yang Mengubah Segalanya",
                EnterScene = "ch6_telepon", SceneAtNight = true,
                Objectives = new[] { Scene("ch6_telepon", "Angkat telepon") } },

            new QuestDef { State = StoryState.Ch6PerjalananTerakhir, Chapter = Chapter.Ch6, Title = "Quest: Perjalanan Terakhir",
                Sequential = true,
                Objectives = new[]
                {
                    Scene("urusan_terakhir", "Menyelesaikan urusan terakhir di Jogokerto"),
                    new Objective("Menyiapkan barang bawaan", StoryEvent.Pickup, "barang_bawaan"),
                    Scene("pilih_kendaraan", "Memilih kendaraan perjalanan"),
                    Arrive(LocationId.TerminalJogokerto, "Pergi ke Terminal Jogokerto"),
                    new Objective("Perjalanan menuju Desa Suka Makmur", StoryEvent.Travel, Region.DesaSukaMakmur.ToString()),
                    Arrive(LocationId.RumahZidan, "Mengunjungi rumah"),
                } },

            new QuestDef { State = StoryState.EndingPulang, Chapter = Chapter.Ending, Title = "PULANG",
                EnterScene = "ending_pulang",
                Objectives = new[] { Scene("ending_pulang", "") } },

            new QuestDef { State = StoryState.TheEnd, Chapter = Chapter.Ending, Title = "THE END" },
        };

        static readonly Dictionary<StoryState, QuestDef> Quests = BuildIndex();

        /// <summary>Data misi untuk state tertentu (null bila tidak ada).</summary>
        public static QuestDef QuestFor(StoryState s) =>
            Quests.TryGetValue(s, out QuestDef q) ? q : null;

        static Dictionary<StoryState, QuestDef> BuildIndex()
        {
            var map = new Dictionary<StoryState, QuestDef>();
            foreach (var q in Table) map[q.State] = q;
            return map;
        }

        // ------------------------------------------------------------------
        //  Siklus hidup
        // ------------------------------------------------------------------

        public void NewGame()
        {
            PulangSaveData.Delete();
            Data = new PulangSaveData();
            Enter(StoryState.NameEntry);
        }

        public void Resume(PulangSaveData data)
        {
            Data = data;
            PulangProfile.SetName(data.playerName);

            // Keluar di tengah adegan: putar ulang supaya cerita tidak macet.
            var q = Current;
            for (int i = 0; i < q.Objectives.Length; i++)
                if (q.Objectives[i].Event == StoryEvent.SceneDone && q.Objectives[i].Key == q.EnterScene
                    && Data.objectiveProgress[i] < q.Objectives[i].Count)
                    Data.sceneShown = false;

            StateChanged?.Invoke(Data.state);
            TryPlayEnterScene();
        }

        /// <summary>Dipanggil oleh layar nama (NameEntry.Confirmed).</summary>
        public void ConfirmName(string name)
        {
            if (Data.state != StoryState.NameEntry) return;
            PulangProfile.SetName(name);
            Data.playerName = PulangProfile.Name;
            Enter(StoryState.PrologRumah);
        }

        /// <summary>Semua sistem (dialog, peta, toko, kerja) melapor ke sini.</summary>
        public void Notify(StoryEvent ev, string key)
        {
            // Misi sampingan ikut mendengar semua kejadian, apa pun misi utamanya.
            AnyEvent?.Invoke(ev, key);

            var q = Current;
            if (q.Objectives.Length == 0) return;

            for (int i = 0; i < q.Objectives.Length; i++)
            {
                if (Data.objectiveProgress[i] >= q.Objectives[i].Count) continue;

                var o = q.Objectives[i];
                bool match = o.Event == ev && (o.Key == "*" || o.Key == key);
                if (match)
                {
                    Data.objectiveProgress[i]++;
                    if (Data.objectiveProgress[i] >= o.Count)
                        ObjectiveCompleted?.Invoke(PulangProfile.Format(o.Text));
                    break;
                }
                if (q.Sequential) return; // tujuan berikutnya belum boleh
            }

            if (AllDone()) Advance();
            else Data.Save();
        }

        /// <summary>Panggil tiap pergantian jam / hari, setelah naik level, gajian, atau menabung.</summary>
        public void Tick()
        {
            var q = Current;
            if (q.Gate != null && q.Gate(Data))
            {
                Advance();
                return;
            }
            TryPlayEnterScene();
        }

        // ------------------------------------------------------------------
        //  Aturan cerita yang dipakai sistem lain
        // ------------------------------------------------------------------

        /// <summary>Ratna masih bisa ditelepon (Daily Quest "Hubungi Ibu") hanya sebelum kabar duka.</summary>
        public bool CanCallMother => Data.state < StoryState.Ch6KabarDuka;

        public bool IsGirlUnlocked(int girlIndex) => Chapter >= PulangRomance.Girls[girlIndex].Unlocks;

        public bool CanTravelTo(Region target, out string reason)
            => PulangWorld.CanTravelTo(target, Chapter, Data.money, out reason);

        public IEnumerable<string> JournalLines()
        {
            var q = Current;
            yield return PulangProfile.Format(q.Title);
            for (int i = 0; i < q.Objectives.Length; i++)
            {
                var o = q.Objectives[i];
                if (string.IsNullOrEmpty(o.Text)) continue;
                bool done = Data.objectiveProgress[i] >= o.Count;
                string count = o.Count > 1 ? " (" + Data.objectiveProgress[i] + "/" + o.Count + ")" : "";
                yield return (done ? "[x] " : "[ ] ") + PulangProfile.Format(o.Text) + count;
            }
            if (q.GateText != null) yield return "[ ] " + q.GateText;
        }

        // ------------------------------------------------------------------

        bool AllDone()
        {
            var q = Current;
            for (int i = 0; i < q.Objectives.Length; i++)
                if (Data.objectiveProgress[i] < q.Objectives[i].Count) return false;
            return true;
        }

        void Advance()
        {
            if (Data.state == StoryState.TheEnd) return;
            Enter(Data.state + 1); // urutan enum = urutan cerita, tanpa cabang
        }

        void Enter(StoryState s)
        {
            Data.state = s;
            Data.objectiveProgress = new int[Quests[s].Objectives.Length];
            Data.sceneShown = false;

            if (s == StoryState.Ch1Tiba) { Data.region = Region.KotaJogokerto; Data.location = LocationId.TerminalJogokerto; }
            if (s == StoryState.Ch6KabarDuka) Data.Set("ratna_wafat");

            StateChanged?.Invoke(s);

            if (s == StoryState.TheEnd)
            {
                Data.Set("tamat");
                Data.Save();
                GameFinished?.Invoke();
                return;
            }

            Data.Save();
            TryPlayEnterScene();
        }

        void TryPlayEnterScene()
        {
            var q = Current;
            if (q.EnterScene == null || Data.sceneShown) return;
            if (q.SceneAtNight && Data.minutes < PulangBalance.NightStartMinutes) return;

            Data.sceneShown = true;
            PlayScene?.Invoke(q.EnterScene);
        }
    }
}
