using System;
using System.Collections.Generic;
using PixelAdventure.LifeSim.Pulang;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Tablet > SIMPAN: pilih slot tujuan (1–3). Slot yang berisi permainan LAIN meminta konfirmasi
    /// sebelum ditimpa. Setelah disimpan, slot itu menjadi slot aktif (simpan otomatis berikutnya
    /// juga ke sana). ESC / X kembali ke tablet.
    /// </summary>
    public partial class PulangGame
    {
        void OpenSaveSlotMenu()
        {
            var labels = new List<string>();
            var actions = new List<Action>();
            for (int i = 0; i < PulangSaveData.SlotCount; i++)
            {
                int slot = i;
                labels.Add(SaveSlotLabel(slot));
                actions.Add(() => ChooseSaveSlot(slot));
            }
            labels.Add("BATAL");
            actions.Add(() => OpenTabletWith(null));
            OpenMenu("SIMPAN KE SLOT MANA?", labels, actions, true, () => OpenTabletWith(null));
            _menuSelected = PulangSaveData.ActiveSlot;
            _hud.SetMenuSelection(_menuSelected);
        }

        /// <summary>Mis. "SLOT 2   ZIDAN - CHAPTER 3, HARI 12   13/11/2026 17:20   [AKTIF]".</summary>
        static string SaveSlotLabel(int slot)
        {
            string head = "SLOT " + (slot + 1) + "   ";
            string active = slot == PulangSaveData.ActiveSlot ? "   [AKTIF]" : "";
            var d = PulangSaveData.Peek(slot);
            if (d == null) return head + "- KOSONG -" + active;
            string name = string.IsNullOrEmpty(d.playerName) ? PulangProfile.DefaultName : d.playerName;
            var q = PulangStory.QuestFor(d.state);
            string chapter = q == null ? "" : ", " + ChapterLabel(q.Chapter);
            string saved = string.IsNullOrEmpty(d.savedAt) ? "" : "   " + d.savedAt;
            return (head + name + chapter + ", HARI " + d.day + saved + active).ToUpperInvariant();
        }

        void ChooseSaveSlot(int slot)
        {
            var d = PulangSaveData.Peek(slot);
            // Menimpa permainan lain (bukan slot yang sedang dimainkan): tanya dulu.
            if (d != null && slot != PulangSaveData.ActiveSlot)
            {
                string who = string.IsNullOrEmpty(d.playerName) ? PulangProfile.DefaultName : d.playerName;
                OpenMenu("TIMPA SLOT " + (slot + 1) + " (" + who.ToUpperInvariant() + ", HARI " + d.day + ")?",
                    new List<string> { "YA, TIMPA DENGAN PERMAINAN INI", "TIDAK, PILIH SLOT LAIN" },
                    new List<Action> { () => SaveToSlot(slot), OpenSaveSlotMenu },
                    true, OpenSaveSlotMenu);
                _menuSelected = 1;   // pilihan aman lebih dulu
                _hud.SetMenuSelection(1);
                return;
            }
            SaveToSlot(slot);
        }

        void SaveToSlot(int slot)
        {
            PulangSaveData.ActiveSlot = slot;
            D.Save();
            GameAudio.Play("checkpoint", 0.5f);
            OpenTabletWith("TERSIMPAN DI SLOT " + (slot + 1) + " - " + D.savedAt);
        }

        /// <summary>Buka tablet dengan pesan (null = tanpa pesan), selektor tetap pada aplikasi terakhir.</summary>
        void OpenTabletWith(string message)
        {
            OpenTablet();
            _tabletMessage = message;
            RefreshTablet();
        }
    }
}
