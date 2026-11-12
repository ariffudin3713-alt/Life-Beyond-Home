using System.Collections.Generic;
using PixelAdventure.LifeSim.Pulang;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Halaman utama (home page):
    /// - LANJUTKAN      : memuat slot yang terakhir dimainkan.
    /// - GAME BARU      : pilih slot (kosong atau ditimpa dengan konfirmasi), lalu mulai cerita.
    /// - SLOT SIMPANAN  : lihat 3 slot, muat salah satunya, atau hapus (X / Delete).
    /// </summary>
    public partial class PulangGame
    {
        enum HomePage { Menu, Slots }
        enum SlotMode { NewGame, Load }

        HomePage _homePage;
        SlotMode _slotMode;
        int _homeIndex, _slotIndex;
        bool _confirmOverwrite, _confirmDelete;
        string _slotMessage;

        // ------------------------------------------------------------------ masuk

        void ShowTitle()
        {
            PulangSaveData.MigrateLegacy();
            _state = State.Title;
            _hud.SetGameplayVisible(false);
            _hud.HideEnding();
            _hud.HidePause();
            _hud.HideDialogue();
            _hud.HideMenu();
            _hud.ResetTitleTexts();
            _homePage = HomePage.Menu;
            _homeIndex = PulangSaveData.AnySave() ? 0 : 1;
            RefreshHome();
            _inputLock = 0.3f;
            GameAudio.PlayMusic(3);
        }

        /// <summary>Dari layar jeda: simpan lalu kembali ke halaman utama.</summary>
        void ReturnToHome()
        {
            if (_story.Data != null && D.state != StoryState.NameEntry) D.Save();
            ShowTitle();
        }

        // ------------------------------------------------------------------ tampilan

        void RefreshHome()
        {
            if (_homePage == HomePage.Menu)
            {
                int last = PulangSaveData.LastSlot();
                string cont = "BELUM ADA SIMPANAN";
                if (last >= 0)
                {
                    var d = PulangSaveData.Peek(last);
                    cont = d != null ? "SLOT " + (last + 1) + " - " + Describe(d) : "SLOT " + (last + 1);
                }
                var options = new List<HomeOption>
                {
                    new HomeOption("LANJUTKAN", cont, last >= 0),
                    new HomeOption("GAME BARU", "MULAI CERITA DI SLOT PILIHAN"),
                    new HomeOption("SLOT SIMPANAN", "MUAT ATAU HAPUS SIMPANAN"),
                };
                _hud.ShowHome(options, _homeIndex);
                return;
            }

            var slots = new List<SlotView>();
            for (int i = 0; i < PulangSaveData.SlotCount; i++) slots.Add(SlotViewFor(i));

            string title = _slotMode == SlotMode.NewGame ? "GAME BARU - PILIH SLOT" : "SLOT SIMPANAN";
            string hint = _confirmOverwrite || _confirmDelete
                ? "E - YA    ESC - TIDAK"
                : _slotMode == SlotMode.NewGame
                    ? "W/S - PILIH    E - MULAI DI SLOT INI    ESC - KEMBALI"
                    : "W/S - PILIH    E - MUAT    X / DEL - HAPUS    ESC - KEMBALI";
            _hud.ShowSlots(title, slots, _slotIndex, _slotMessage, hint);
        }

        static SlotView SlotViewFor(int slot)
        {
            var d = PulangSaveData.Peek(slot);
            if (d == null) return new SlotView { Empty = true };

            string name = string.IsNullOrEmpty(d.playerName) ? PulangProfile.DefaultName : d.playerName;
            var q = PulangStory.QuestFor(d.state);
            string chapter = q == null ? "" : q.Chapter == Chapter.Prolog ? "PROLOG"
                : q.Chapter == Chapter.Ending ? "ENDING" : "CHAPTER " + (int)q.Chapter;
            string quest = q == null ? "" : q.Title.Replace(PulangProfile.Token, name);
            if (d.state == StoryState.NameEntry) quest = "Belum memilih nama";

            int hh = d.minutes / 60, mm = d.minutes % 60;
            return new SlotView
            {
                Empty = false,
                Line1 = name + "  -  " + chapter,
                Line2 = Trim(quest, 44),
                Line3 = "HARI " + d.day + "  " + hh.ToString("00") + ":" + mm.ToString("00") +
                        "  " + Rp(d.money) + "  LV " + d.level +
                        (string.IsNullOrEmpty(d.savedAt) ? "" : "   DISIMPAN " + d.savedAt),
            };
        }

        static string Describe(PulangSaveData d)
        {
            string name = string.IsNullOrEmpty(d.playerName) ? PulangProfile.DefaultName : d.playerName;
            return (name + " - HARI " + d.day).ToUpperInvariant();
        }

        static string Trim(string s, int max) => s.Length <= max ? s : s.Substring(0, max - 3) + "...";

        // ------------------------------------------------------------------ input

        void UpdateTitle()
        {
            if (_inputLock > 0f) return;
            if (_homePage == HomePage.Menu) UpdateHomeMenu();
            else UpdateSlotScreen();
        }

        void UpdateHomeMenu()
        {
            if (LifeInput.UpPressed || LifeInput.DownPressed)
            {
                int dir = LifeInput.UpPressed ? -1 : 1;
                _homeIndex = (_homeIndex + dir + 3) % 3;
                GameAudio.Play("click", 0.4f);
                RefreshHome();
                return;
            }
            if (!LifeInput.InteractPressed) return;

            switch (_homeIndex)
            {
                case 0: // LANJUTKAN
                    int last = PulangSaveData.LastSlot();
                    if (last < 0) { GameAudio.Play("error", 0.5f); return; }
                    LoadSlot(last);
                    break;
                case 1: // GAME BARU
                    OpenSlots(SlotMode.NewGame);
                    break;
                case 2: // SLOT SIMPANAN
                    OpenSlots(SlotMode.Load);
                    break;
            }
        }

        void OpenSlots(SlotMode mode)
        {
            _slotMode = mode;
            _homePage = HomePage.Slots;
            _confirmOverwrite = _confirmDelete = false;
            _slotMessage = null;
            // Game baru: arahkan ke slot kosong pertama; muat: ke slot terakhir dimainkan.
            _slotIndex = 0;
            if (mode == SlotMode.NewGame)
            {
                for (int i = 0; i < PulangSaveData.SlotCount; i++)
                    if (!PulangSaveData.HasSlot(i)) { _slotIndex = i; break; }
            }
            else
            {
                int last = PulangSaveData.LastSlot();
                if (last >= 0) _slotIndex = last;
            }
            GameAudio.Play("click", 0.4f);
            RefreshHome();
            _inputLock = 0.15f;
        }

        void UpdateSlotScreen()
        {
            // ---- konfirmasi timpa / hapus
            if (_confirmOverwrite || _confirmDelete)
            {
                if (LifeInput.InteractPressed)
                {
                    if (_confirmOverwrite)
                    {
                        _confirmOverwrite = false;
                        BeginNewGameInSlot(_slotIndex);
                        return;
                    }
                    PulangSaveData.DeleteSlot(_slotIndex);
                    _confirmDelete = false;
                    _slotMessage = "SLOT " + (_slotIndex + 1) + " DIHAPUS";
                    GameAudio.Play("error", 0.4f);
                    RefreshHome();
                }
                else if (LifeInput.CancelPressed)
                {
                    _confirmOverwrite = _confirmDelete = false;
                    _slotMessage = null;
                    RefreshHome();
                }
                return;
            }

            if (LifeInput.CancelPressed)
            {
                _homePage = HomePage.Menu;
                GameAudio.Play("click", 0.4f);
                RefreshHome();
                _inputLock = 0.15f;
                return;
            }

            if (LifeInput.UpPressed || LifeInput.DownPressed)
            {
                int dir = LifeInput.UpPressed ? -1 : 1;
                _slotIndex = (_slotIndex + dir + PulangSaveData.SlotCount) % PulangSaveData.SlotCount;
                _slotMessage = null;
                GameAudio.Play("click", 0.4f);
                RefreshHome();
                return;
            }

            bool filled = PulangSaveData.HasSlot(_slotIndex);

            if (_slotMode == SlotMode.Load && LifeInput.DeletePressed)
            {
                if (!filled) { GameAudio.Play("error", 0.4f); return; }
                _confirmDelete = true;
                _slotMessage = "HAPUS SLOT " + (_slotIndex + 1) + "? SIMPANAN TIDAK BISA DIKEMBALIKAN";
                GameAudio.Play("error", 0.5f);
                RefreshHome();
                return;
            }

            if (!LifeInput.InteractPressed) return;

            if (_slotMode == SlotMode.NewGame)
            {
                if (filled)
                {
                    _confirmOverwrite = true;
                    _slotMessage = "SLOT " + (_slotIndex + 1) + " SUDAH TERISI. TIMPA DENGAN GAME BARU?";
                    GameAudio.Play("error", 0.5f);
                    RefreshHome();
                    return;
                }
                BeginNewGameInSlot(_slotIndex);
            }
            else
            {
                if (!filled)
                {
                    _slotMessage = "SLOT KOSONG - PILIH GAME BARU UNTUK MEMAKAINYA";
                    GameAudio.Play("error", 0.4f);
                    RefreshHome();
                    return;
                }
                LoadSlot(_slotIndex);
            }
        }

        // ------------------------------------------------------------------ aksi

        void BeginNewGameInSlot(int slot)
        {
            PulangSaveData.ActiveSlot = slot;
            _hud.HideHome();
            GameAudio.Play("win", 0.5f);
            StartNewGame();
        }

        void LoadSlot(int slot)
        {
            var data = PulangSaveData.Load(slot);
            if (data == null)
            {
                GameAudio.Play("error", 0.5f);
                _slotMessage = "SIMPANAN RUSAK / TIDAK BISA DIBACA";
                RefreshHome();
                return;
            }
            _hud.HideHome();
            GameAudio.Play("win", 0.5f);
            ContinueGame(data);
        }
    }
}
