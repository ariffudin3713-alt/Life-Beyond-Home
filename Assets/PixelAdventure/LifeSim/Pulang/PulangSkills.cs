using System.Collections.Generic;

namespace PixelAdventure.LifeSim.Pulang
{
    public enum SkillGroup { Keterampilan, Alam, Profesional }

    /// <summary>Satu kursus di Perpustakaan. Tiap level dibeli terpisah; harga level n = BasePrice x n.</summary>
    public class SkillDef
    {
        public string Id, Name, Desc;
        public SkillGroup Group;
        public int MaxLevel = 1;
        public int BasePrice;
        public int MinPlayerLevel = 1;     // level karakter minimal untuk ikut kursus
    }

    /// <summary>
    /// Kursus & persyaratan skill dari dokumen ("RULES PEKERJAAN" / persyaratan kerja):
    /// Montir = skill mekanik level 2, Tukang Cukur = ketelitian level 2, Desainer = desain level 5,
    /// Koki = memasak level 5, Guru = pendidikan & mengajar, Perawat = pelatihan keperawatan,
    /// Dokter = pendidikan kedokteran, Pelayan Kafe = bisa berkomunikasi dengan pelanggan.
    /// "SETIAP PERSYARATAN KERJA: HARUS MEMBELI SKILL PEKERJAAN."
    /// Disimpan di PulangSaveData.skills sebagai "idSkill:level".
    /// </summary>
    public static class PulangSkills
    {
        public static readonly SkillDef[] All =
        {
            // ---- Keterampilan
            new SkillDef { Id = "komunikasi", Name = "Komunikasi Pelanggan", Group = SkillGroup.Keterampilan,
                MaxLevel = 1, BasePrice = 15000, MinPlayerLevel = 1,
                Desc = "Melayani pelanggan dengan ramah. Syarat Pelayan Kafe." },
            new SkillDef { Id = "mekanik", Name = "Mekanik", Group = SkillGroup.Keterampilan,
                MaxLevel = 2, BasePrice = 25000, MinPlayerLevel = 2,
                Desc = "Memperbaiki mesin motor. Level 2 untuk Montir Motor." },
            new SkillDef { Id = "ketelitian", Name = "Ketelitian", Group = SkillGroup.Keterampilan,
                MaxLevel = 2, BasePrice = 25000, MinPlayerLevel = 2,
                Desc = "Bekerja rapi dan teliti. Level 2 untuk Tukang Cukur." },
            new SkillDef { Id = "fotografi", Name = "Fotografi", Group = SkillGroup.Keterampilan,
                MaxLevel = 1, BasePrice = 30000, MinPlayerLevel = 3,
                Desc = "Teknik memotret. Syarat Fotografer (juga butuh kamera)." },

            // ---- Alam & petualangan
            new SkillDef { Id = "memancing", Name = "Memancing", Group = SkillGroup.Alam,
                MaxLevel = 1, BasePrice = 15000, MinPlayerLevel = 1,
                Desc = "Teknik memancing di laut. Syarat Nelayan (juga butuh alat pancing)." },
            new SkillDef { Id = "bertani", Name = "Bertani", Group = SkillGroup.Alam,
                MaxLevel = 1, BasePrice = 15000, MinPlayerLevel = 1,
                Desc = "Merawat tanaman dan lahan. Syarat Petani." },

            // ---- Profesional
            new SkillDef { Id = "memasak", Name = "Memasak", Group = SkillGroup.Profesional,
                MaxLevel = 5, BasePrice = 18000, MinPlayerLevel = 3,
                Desc = "Dari dasar sampai menu restoran. Level 5 untuk Koki Restoran." },
            new SkillDef { Id = "desain", Name = "Desain Grafis", Group = SkillGroup.Profesional,
                MaxLevel = 5, BasePrice = 22000, MinPlayerLevel = 4,
                Desc = "Membuat poster dan logo. Level 5 untuk Desainer Grafis." },
            new SkillDef { Id = "administrasi", Name = "Administrasi Perkantoran", Group = SkillGroup.Profesional,
                MaxLevel = 1, BasePrice = 45000, MinPlayerLevel = 5,
                Desc = "Surat-menyurat dan arsip. Syarat Pegawai Kantor." },
            new SkillDef { Id = "mengajar", Name = "Pendidikan & Mengajar", Group = SkillGroup.Profesional,
                MaxLevel = 1, BasePrice = 75000, MinPlayerLevel = 7,
                Desc = "Pendidikan dan kemampuan mengajar. Syarat Guru." },
            new SkillDef { Id = "kepolisian", Name = "Pelatihan Kepolisian", Group = SkillGroup.Profesional,
                MaxLevel = 1, BasePrice = 110000, MinPlayerLevel = 9,
                Desc = "Disiplin dan pelayanan masyarakat. Syarat Polisi." },
            new SkillDef { Id = "keperawatan", Name = "Pelatihan Keperawatan", Group = SkillGroup.Profesional,
                MaxLevel = 1, BasePrice = 120000, MinPlayerLevel = 9,
                Desc = "Merawat pasien. Syarat Perawat." },
            new SkillDef { Id = "kedokteran", Name = "Pendidikan Kedokteran", Group = SkillGroup.Profesional,
                MaxLevel = 3, BasePrice = 150000, MinPlayerLevel = 12,
                Desc = "Tiga tahap pendidikan dokter. Tahap 3 untuk Dokter." },
        };

        /// <summary>Skill (dan levelnya) yang dibutuhkan setiap pekerjaan.</summary>
        static readonly Dictionary<string, KeyValuePair<string, int>> JobNeeds = new Dictionary<string, KeyValuePair<string, int>>
        {
            { "pelayan_kafe",    Need("komunikasi", 1) },
            { "bengkel",         Need("mekanik", 2) },
            { "tukang_cukur",    Need("ketelitian", 2) },
            { "fotografer",      Need("fotografi", 1) },
            { "nelayan",         Need("memancing", 1) },
            { "petani",          Need("bertani", 1) },
            { "koki_restoran",   Need("memasak", 5) },
            { "desainer_grafis", Need("desain", 5) },
            { "pegawai_kantor",  Need("administrasi", 1) },
            { "guru",            Need("mengajar", 1) },
            { "polisi",          Need("kepolisian", 1) },
            { "perawat",         Need("keperawatan", 1) },
            { "dokter",          Need("kedokteran", 3) },
        };

        static KeyValuePair<string, int> Need(string id, int level) => new KeyValuePair<string, int>(id, level);

        public static SkillDef Get(string id)
        {
            foreach (var s in All) if (s.Id == id) return s;
            return null;
        }

        public static int PriceFor(SkillDef s, int nextLevel) => s.BasePrice * nextLevel;

        public static int Level(PulangSaveData d, string id)
        {
            string prefix = id + ":";
            foreach (string e in d.skills)
                if (e.StartsWith(prefix) && int.TryParse(e.Substring(prefix.Length), out int lv)) return lv;
            return 0;
        }

        public static void SetLevel(PulangSaveData d, string id, int level)
        {
            string prefix = id + ":";
            d.skills.RemoveAll(e => e.StartsWith(prefix));
            if (level > 0) d.skills.Add(prefix + level);
        }

        /// <summary>Skill yang dibutuhkan pekerjaan ini (false bila tidak butuh skill).</summary>
        public static bool NeedFor(string jobId, out SkillDef skill, out int level)
        {
            skill = null; level = 0;
            if (!JobNeeds.TryGetValue(jobId, out var n)) return false;
            skill = Get(n.Key);
            level = n.Value;
            return skill != null;
        }

        /// <summary>Syarat skill pekerjaan terpenuhi? reason berisi penjelasan bila belum.</summary>
        public static bool MeetsJob(PulangSaveData d, string jobId, out string reason)
        {
            reason = null;
            if (!NeedFor(jobId, out SkillDef s, out int need)) return true;
            int have = Level(d, s.Id);
            if (have >= need) return true;
            reason = "Butuh skill " + s.Name + (s.MaxLevel > 1 ? " level " + need : "") +
                     (have > 0 ? " (sekarang level " + have + ")" : "") + ". Ikuti kursus di Perpustakaan.";
            return false;
        }

        /// <summary>Simpanan lama ("skill_idPekerjaan") diubah ke skill berlevel yang setara.</summary>
        public static void MigrateLegacy(PulangSaveData d)
        {
            var legacy = d.skills.FindAll(e => e.StartsWith("skill_"));
            foreach (string e in legacy)
            {
                d.skills.Remove(e);
                if (NeedFor(e.Substring(6), out SkillDef s, out int need) && Level(d, s.Id) < need)
                    SetLevel(d, s.Id, need);
            }
        }

        /// <summary>Teks ringkas semua skill yang dimiliki, mis. "Mekanik 2, Memasak 3".</summary>
        public static string Summary(PulangSaveData d)
        {
            var parts = new List<string>();
            foreach (var s in All)
            {
                int lv = Level(d, s.Id);
                if (lv > 0) parts.Add(s.MaxLevel > 1 ? s.Name + " " + lv : s.Name);
            }
            return parts.Count == 0 ? "-" : string.Join(", ", parts);
        }
    }
}
