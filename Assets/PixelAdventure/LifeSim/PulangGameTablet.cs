using System.Collections.Generic;
using PixelAdventure.LifeSim.Pulang;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Tablet (tombol TAB): shortcut ke Tugas, Inventori, Jurnal, Telepon Ibu, Simpan,
    /// Suara, Menu Utama. Layar yang dibuka dari tablet kembali ke tablet saat ditutup.
    /// </summary>
    public partial class PulangGame
    {
        enum TabletAction { Profile, Tasks, Inventory, Journal, Phone, Guide, Save, Sound, Home, Close }

        static readonly TabletAction[] TabletOrder =
        {
            TabletAction.Profile, TabletAction.Tasks, TabletAction.Inventory, TabletAction.Journal, TabletAction.Phone,
            TabletAction.Guide, TabletAction.Save, TabletAction.Sound, TabletAction.Home, TabletAction.Close,
        };

        int _tabletIndex;
        string _tabletMessage;

        void OpenTablet()
        {
            _state = State.Tablet;
            _hud.SetPrompt(null);
            _tabletMessage = null;
            GameAudio.Play("click", 0.4f);
            RefreshTablet();
            _inputLock = 0.2f;
        }

        bool CanPhoneNow(out string reason)
        {
            reason = null;
            if (!_story.CanCallMother) { reason = "TIDAK ADA YANG BISA DITELEPON"; return false; }
            if (_story.Chapter < Chapter.Ch1) { reason = "IBU ADA DI RUMAH BERSAMAMU"; return false; }
            if (_today.Contains("telepon_ibu")) { reason = "SUDAH MENELEPON IBU HARI INI"; return false; }
            return true;
        }

        void RefreshTablet()
        {
            var apps = new List<TabletApp>();
            foreach (var a in TabletOrder) apps.Add(AppFor(a));

            int hh = D.minutes / 60, mm = D.minutes % 60;
            string left = "HARI " + D.day + "  " + hh.ToString("00") + ":" + mm.ToString("00") + "  " +
                          LifeHud.PhaseFor(D.minutes);
            string right = "ENERGI " + D.energy + "%   " + Rp(D.money);
            _hud.ShowTablet("TABLET " + PulangProfile.Name, left, right, apps, _tabletIndex, _tabletMessage);
        }

        TabletApp AppFor(TabletAction a)
        {
            switch (a)
            {
                case TabletAction.Profile:
                    return App("PROFIL", "app_profile", "e09a3a", "LEVEL " + D.level + " - EXP, PEKERJAAN, SKILL, REPUTASI & TEMAN");
                case TabletAction.Tasks:
                    return App("TUGAS", "app_tasks", "4fae62", "DAFTAR MISI: BISA, SUDAH, BELUM DIKERJAKAN  (T)");
                case TabletAction.Inventory:
                    int count = D.inventory.Count;
                    return App("INVENTORI", "app_bag", "c8794a", count + " BARANG DI TAS - MAKANAN, HADIAH, BARANG MISI  (I)");
                case TabletAction.Journal:
                    return App("JURNAL", "app_journal", "4c6bb0", "CATATAN PERJALANAN, STATUS & HUBUNGAN  (J)");
                case TabletAction.Guide:
                    return App("PANDUAN", "app_guide", "a8754a", "BUKU CARA BERMAIN: KONTROL, MISI, KERJA, TEMPAT, TEMAN  (G)");
                case TabletAction.Phone:
                    bool ok = CanPhoneNow(out string why);
                    return App("TELEPON IBU", "app_phone", "2e8f62", ok ? "HUBUNGI IBU (+5 HUBUNGAN KELUARGA, SEKALI SEHARI)" : why, ok);
                case TabletAction.Save:
                    return App("SIMPAN", "app_save", "3b6fb6", "SIMPAN PERMAINAN KE SLOT " + (PulangSaveData.ActiveSlot + 1));
                case TabletAction.Sound:
                    return App(GameAudio.Muted ? "SUARA MATI" : "SUARA ON", "app_sound", "8a63ad",
                        "NYALAKAN / MATIKAN SUARA  (M)");
                case TabletAction.Home:
                    return App("MENU UTAMA", "app_home", "b8503f", "SIMPAN LALU KEMBALI KE MENU UTAMA");
                default:
                    return App("TUTUP", "app_close", "5a5550", "KEMBALI BERMAIN  (TAB)");
            }
        }

        static TabletApp App(string label, string icon, string tileHex, string desc, bool enabled = true) =>
            new TabletApp
            {
                Label = label, Icon = "lc_" + icon, Desc = desc, Enabled = enabled,
                Tile = (Color)PixelArt.Hex(tileHex),
            };

        void UpdateTablet()
        {
            if (_inputLock > 0f) return;
            if (LifeInput.TabletPressed || LifeInput.CancelPressed)
            {
                CloseTablet();
                return;
            }

            int n = TabletOrder.Length;
            int cols = 5;
            int move = 0;
            if (LifeInput.LeftPressed) move = -1;
            else if (LifeInput.RightPressed) move = 1;
            else if (LifeInput.UpPressed) move = -cols;
            else if (LifeInput.DownPressed) move = cols;
            if (move != 0)
            {
                _tabletIndex = (_tabletIndex + move + n) % n;
                _tabletMessage = null;
                GameAudio.Play("click", 0.35f);
                RefreshTablet();
                return;
            }

            if (!LifeInput.InteractPressed) return;
            switch (TabletOrder[_tabletIndex])
            {
                case TabletAction.Profile:
                    _hud.HideTablet();
                    OpenProfile(true);
                    break;
                case TabletAction.Tasks:
                    _hud.HideTablet();
                    OpenTasks();
                    _returnToTablet = true;
                    break;
                case TabletAction.Inventory:
                    _hud.HideTablet();
                    OpenInventory(true);
                    break;
                case TabletAction.Journal:
                    _hud.HideTablet();
                    OpenJournal();
                    _returnToTablet = true;
                    break;
                case TabletAction.Guide:
                    _hud.HideTablet();
                    OpenGuide(true);
                    break;
                case TabletAction.Phone:
                    if (!CanPhoneNow(out string why))
                    {
                        GameAudio.Play("error", 0.4f);
                        _tabletMessage = why;
                        RefreshTablet();
                        return;
                    }
                    CloseTablet();
                    CallMother();
                    break;
                case TabletAction.Save:
                    D.Save();
                    GameAudio.Play("checkpoint", 0.5f);
                    _tabletMessage = "TERSIMPAN DI SLOT " + (PulangSaveData.ActiveSlot + 1) + " - " + D.savedAt;
                    RefreshTablet();
                    break;
                case TabletAction.Sound:
                    GameAudio.ToggleMute();
                    RefreshTablet();
                    break;
                case TabletAction.Home:
                    _hud.HideTablet();
                    ReturnToHome();
                    break;
                default:
                    CloseTablet();
                    break;
            }
        }

        void CloseTablet()
        {
            _hud.HideTablet();
            _state = State.Playing;
            _returnToTablet = false;
            _inputLock = 0.15f;
        }
    }
}
