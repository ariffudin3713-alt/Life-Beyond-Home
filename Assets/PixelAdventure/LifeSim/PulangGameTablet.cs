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
        enum TabletAction { Profile, Tasks, Inventory, Journal, Contacts, Guide, Map, Save, Graphics, Sound, Home, Close }

        static readonly TabletAction[] TabletOrder =
        {
            TabletAction.Profile, TabletAction.Tasks, TabletAction.Inventory, TabletAction.Journal, TabletAction.Contacts,
            TabletAction.Map, TabletAction.Guide, TabletAction.Save, TabletAction.Graphics, TabletAction.Sound,
            TabletAction.Home, TabletAction.Close,
        };

        const int TabletGridCols = 6;   // sama dengan LifeHud.TabletCols

        int _tabletIndex;
        string _tabletMessage;

        void OpenTablet()
        {
            _state = State.Tablet;
            _hud.SetPrompt(null);
            _tabletMessage = null;
            _tabletIndex = Mathf.Clamp(_tabletIndex, 0, TabletOrder.Length - 1);
            GameAudio.Play("tablet_on", 0.45f);
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
            string left = "HARI " + D.day + "  " + GameCalendar.CompactDate(D.day) + "  " + hh.ToString("00") + ":" + mm.ToString("00") + "  " +
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
                    return App("TAS", "app_bag", "c8794a", "INVENTORI: " + count + " BARANG - MAKANAN, HADIAH, BARANG MISI  (I)");
                case TabletAction.Journal:
                    return App("JURNAL", "app_journal", "4c6bb0", "CATATAN PERJALANAN, STATUS & HUBUNGAN  (J)");
                case TabletAction.Guide:
                    return App("PANDUAN", "app_guide", "a8754a", "BUKU CARA BERMAIN: KONTROL, MISI, KERJA, TEMPAT, TEMAN  (G)");
                case TabletAction.Contacts:
                    return App("KONTAK", "app_contacts", "c0566e", KnownContacts().Count + " ORANG DIKENAL - PROFIL, TELEPON & PESAN (A/D GANTI TAB)");
                case TabletAction.Map:
                    return App("PETA", "app_map", "4f9a6a", "SEMUA WILAYAH, ARAH KE WILAYAH LAIN & DAFTAR TEMPAT  (F)");
                case TabletAction.Save:
                    return App("SIMPAN", "app_save", "3b6fb6", "PILIH SLOT UNTUK MENYIMPAN (AKTIF: SLOT " + (PulangSaveData.ActiveSlot + 1) + ")");
                case TabletAction.Graphics:
                    return App("GRAFIK", "app_display", "3f8f8a", "LAYAR, TAMPILAN, PERFORMA & SUARA - " + LifeGraphics.FpsLabel);
                case TabletAction.Sound:
                    return App(GameAudio.Muted ? "SENYAP" : "SUARA", "app_sound", "8a63ad",
                        GameAudio.Muted ? "SUARA MATI - BUKA UNTUK MENYALAKAN  (M)" : "SUARA NYALA - BUKA UNTUK MEMATIKAN  (M)");
                case TabletAction.Home:
                    return App("MENU", "app_home", "b8503f", "SIMPAN LALU KEMBALI KE MENU UTAMA");
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
            int cols = TabletGridCols;
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

            // Kursor: arahkan = pilih aplikasi, klik = buka.
            if (_hud.PointerHover(UiHit.TabletApp, out int hover) && hover != _tabletIndex && hover < n)
            {
                _tabletIndex = hover;
                _tabletMessage = null;
                GameAudio.Play("click", 0.25f);
                RefreshTablet();
            }
            bool clicked = _hud.PointerClick(UiHit.TabletApp, out int pick) && pick < n;
            if (clicked) _tabletIndex = pick;

            if (!LifeInput.InteractPressed && !clicked) return;
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
                case TabletAction.Contacts:
                    _hud.HideTablet();
                    _contactMsg = null;
                    OpenContacts(ContactTab.Profil);
                    break;
                case TabletAction.Map:
                    _hud.HideTablet();
                    OpenWorldMap(true);
                    break;
                case TabletAction.Save:
                    _hud.HideTablet();
                    OpenSaveSlotMenu();
                    break;
                case TabletAction.Graphics:
                    _hud.HideTablet();
                    OpenSettingsMenu(() => OpenTabletWith(null));
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
            GameAudio.Play("tablet_off", 0.4f);
            _state = State.Playing;
            _returnToTablet = false;
            _inputLock = 0.15f;
        }
    }
}
