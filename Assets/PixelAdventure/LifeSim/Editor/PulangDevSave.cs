using PixelAdventure.LifeSim.Pulang;
using UnityEditor;
using UnityEngine;

namespace PixelAdventure.LifeSim.EditorTools
{
    /// <summary>
    /// Alat uji (hanya di Unity Editor): membuat simpanan siap-uji di SLOT 2.
    /// Menu: PULANG > Buat Save Slot 2 (Arif - Sebelum Kabar Duka)
    ///
    /// Isi simpanan:
    /// - nama "Arif", uang Rp999.999.999;
    /// - cerita di "Chapter 5 – Kesuksesan", tahap terakhir SEBELUM kabar duka (Ibu masih hidup,
    ///   bisa ditelepon). Semua tempat di kota terbuka. Desa Suka Makmur baru terbuka lagi di Chapter 6.
    /// - tabungan sengaja Rp500.000 DI BAWAH target sukses: cerita tidak langsung berlanjut.
    ///   Menabung Rp500.000 lagi di kost = target tercapai -> Chapter 6 (kabar duka) -> menuju ENDING;
    /// - level 20, pekerjaan utama Dokter (profesional) + sampingan Fotografer;
    /// - semua skill kursus di level maksimal;
    /// - semua tokoh dikenal (muncul di KONTAK), kelima teman perempuan sudah bertemu;
    /// - semua kendaraan, kamera, alat pancing, kunci kost, serta bekal makanan & hadiah.
    /// Disusun memakai kelas simpanan game sendiri, jadi formatnya pasti sama dengan simpanan biasa.
    /// </summary>
    public static class PulangDevSave
    {
        const int Slot = 1;   // indeks 0..2 -> SLOT 2 di layar

        const string AutoFlag = "Pulang.DevSave.Slot2.AutoCreated.v1";

        /// <summary>
        /// Otomatis sekali: begitu Unity selesai memuat ulang skrip, SLOT 2 yang masih KOSONG langsung diisi
        /// (tanpa perlu membuka menu). Slot yang sudah berisi tidak pernah ditimpa di sini.
        /// </summary>
        [InitializeOnLoadMethod]
        static void AutoCreateOnce()
        {
            if (EditorPrefs.GetBool(AutoFlag, false)) return;
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;   // coba lagi saat tidak sedang Play
                EditorPrefs.SetBool(AutoFlag, true);
                if (PulangSaveData.HasSlot(Slot)) return;
                WriteSave();
                Debug.Log("[PULANG] SLOT 2 dibuat otomatis: Arif, Rp999.999.999, Chapter 5 - Kesuksesan (sebelum kabar duka).");
            };
        }

        [MenuItem("PULANG/Buat Save Slot 2 (Arif - Sebelum Kabar Duka)")]
        static void CreateEndingSave()
        {
            if (PulangSaveData.HasSlot(Slot) &&
                !EditorUtility.DisplayDialog("Timpa SLOT 2?",
                    "SLOT 2 sudah berisi simpanan. Timpa dengan simpanan Arif (sebelum kabar duka)?", "Timpa", "Batal"))
                return;
            WriteSave();
            Debug.Log("[PULANG] SLOT 2 dibuat: Arif, Rp999.999.999, Chapter 5 - Kesuksesan (sebelum kabar duka).");
            EditorUtility.DisplayDialog("Selesai",
                "SLOT 2 berisi simpanan Arif:\n- Uang Rp999.999.999\n- Chapter 5: Kesuksesan (Ibu masih ada)\n" +
                "- Semua tempat kota, skill, kontak, dan kendaraan terbuka\n" +
                "- Menabung Rp500.000 lagi di kost = lanjut ke Chapter 6 lalu ending\n\n" +
                "Mainkan game, pilih SLOT SIMPANAN > SLOT 2.", "OK");
        }

        static void WriteSave()
        {
            var d = new PulangSaveData
            {
                playerName = "Arif",

                // Cerita: tahap terakhir sebelum ending.
                state = StoryState.Ch5Kesuksesan,
                sceneShown = false,

                // Kehidupan
                day = 60,
                minutes = 8 * 60,
                money = 999999999,
                savings = PulangBalance.SuccessSavings - 500000,   // di bawah target: Chapter 6 tidak langsung terpicu
                energy = 100,
                level = 20,
                exp = 0,
                jobMain = "dokter",
                jobSide = "fotografer",
                jobTier = JobTier.Profesional,
                reputation = 100,
                familyCloseness = 100,
                region = Region.KotaJogokerto,
                location = LocationId.KostPelangi,
                posX = -1f,
                posY = -1f,   // muncul di titik kedatangan Kota Jogokerto
                vehicle = "",
            };
            var quest = PulangStory.QuestFor(d.state);
            d.objectiveProgress = new int[quest != null ? quest.Objectives.Length : 0];

            // Semua skill kursus di level maksimal.
            foreach (var s in PulangSkills.All) PulangSkills.SetLevel(d, s.Id, s.MaxLevel);

            // Semua tokoh sudah dikenal (kontak, telepon, pesan).
            foreach (var c in PulangCast.All)
            {
                d.Set("kenal:" + c.Id);
                d.Set("kenalhari:" + c.Id + ":1");
            }

            // Kelima teman perempuan sudah bertemu (kedekatan & episode dimulai dari awal).
            foreach (var g in d.girls) g.met = true;

            // Barang: semua kendaraan & perlengkapan kerja, kunci kost, bekal, dan hadiah.
            string[] items =
            {
                "sepeda", "skuter", "motor", "mobil", "kamera", "alat_pancing", "kunci_kost",
                "nasi_goreng", "nasi_goreng", "nasi_goreng", "roti", "roti", "kopi", "kopi",
                "cokelat", "cokelat", "bunga", "bunga", "buku",
            };
            foreach (string id in items)
                if (PulangItems.Get(id) != null) d.inventory.Add(id);
                else Debug.LogWarning("[PULANG] Barang tidak dikenal, dilewati: " + id);

            int previous = PulangSaveData.ActiveSlot;
            PulangSaveData.ActiveSlot = Slot;
            d.Save();
            PulangSaveData.ActiveSlot = previous;
        }
    }
}
