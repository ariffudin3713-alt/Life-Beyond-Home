using System.Collections.Generic;
using PixelAdventure.LifeSim.Pulang;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Inventori (tombol I / aplikasi tablet) dan barang misi.
    ///
    /// Barang misi dan fungsinya:
    /// - Tas Ransel & Barang Pribadi : dipungut di rumah (Quest 1), syarat berangkat merantau.
    /// - Kunci Kost Pelangi          : didapat saat menemukan Kost Pelangi; syarat tidur/menabung di kost.
    /// - Buku Catatan Kecil          : dipungut di Taman Pelangi, diserahkan kepada Nabila (Quest 5).
    /// - Paket Titipan Dimas (x3)    : diberikan saat Quest 6 mengantar; berkurang satu per antaran.
    /// - Barang Dagangan (x3)        : sama, untuk Quest 7 (Bu Sari).
    /// - Barang Bawaan               : dipungut di kost (Chapter 6), dibawa pulang ke desa.
    /// </summary>
    public partial class PulangGame
    {
        static readonly ItemKind?[] InventoryTabs =
            { null, ItemKind.Makanan, ItemKind.Hadiah, ItemKind.Perlengkapan, ItemKind.Pakaian, ItemKind.Kendaraan, ItemKind.Misi };
        static readonly string[] InventoryTabNames = { "SEMUA", "MAKANAN", "HADIAH", "ALAT", "PAKAIAN", "KENDARAAN", "MISI" };

        int _invTab, _invIndex;
        string _invMessage;
        bool _returnToTablet;

        // ================================================================== barang misi

        /// <summary>Samakan barang misi di inventori dengan keadaan cerita (juga memperbaiki simpanan lama).</summary>
        void SyncQuestItems()
        {
            if (_story.Data == null || D.state == StoryState.NameEntry) return;
            PulangSkills.MigrateLegacy(D);   // skill lama "skill_pekerjaan" -> skill berlevel
            var q = _story.Current;

            // Paket antaran: sebanyak penerima yang belum menerima, selama tugas antar sedang berjalan.
            foreach (var pair in DeliveryTargets)
            {
                int need = 0;
                foreach (int i in PendingObjectiveIndices())
                {
                    var o = q.Objectives[i];
                    if (o.Event != StoryEvent.Deliver || o.Key != pair.Key) continue;
                    foreach (var t in pair.Value)
                        if (!D.Has("antar:" + pair.Key + ":" + t)) need++;
                }
                SetItemCount(pair.Key, need);
            }

            // Kunci kost: sejak tujuan "Menemukan Kost Pelangi" selesai.
            if (D.state > StoryState.Ch1MengenalKota || ObjectiveDone(StoryState.Ch1MengenalKota, StoryEvent.Arrive,
                    LocationId.KostPelangi.ToString()))
                SetItemCount("kunci_kost", 1);

            // Buku Nabila: ada sejak dipungut sampai diserahkan.
            bool bookHeld = D.state == StoryState.Ch1MembantuNabila &&
                            ObjectiveDone(StoryState.Ch1MembantuNabila, StoryEvent.Pickup, "buku_catatan_nabila");
            SetItemCount("buku_catatan_nabila", bookHeld && !ObjectiveDoneAt(StoryState.Ch1MembantuNabila, 2) ? 1 : 0);

            // Barang bawaan: dibawa sejak dipungut sampai tiba di rumah.
            bool bawaan = D.state == StoryState.Ch6PerjalananTerakhir &&
                          ObjectiveDone(StoryState.Ch6PerjalananTerakhir, StoryEvent.Pickup, "barang_bawaan");
            SetItemCount("barang_bawaan", bawaan ? 1 : 0);

            // Surat Pak Hendra: sejak diterima sampai diserahkan kepada Pak Slamet.
            bool letter = D.state == StoryState.Ch4PesanKeluarga &&
                          ObjectiveDoneAt(StoryState.Ch4PesanKeluarga, 0) &&
                          !ObjectiveDoneAt(StoryState.Ch4PesanKeluarga, 1);
            SetItemCount("surat_hendra", letter ? 1 : 0);

            // Kain batik: hilang setelah dikirim lewat Pak Hadi (dibeli sendiri di Toko Pakaian).
            if (D.state > StoryState.Ch5KirimanIbu) SetItemCount("oleh_oleh", 0);

            // Tas & barang pribadi tetap disimpan sebagai kenangan dari rumah (hanya untuk simpanan baru).
            _questItemsBaseline = true;
        }

        bool ObjectiveDone(StoryState state, StoryEvent ev, string key)
        {
            if (D.state != state) return D.state > state;
            var q = _story.Current;
            for (int i = 0; i < q.Objectives.Length; i++)
                if (q.Objectives[i].Event == ev && q.Objectives[i].Key == key)
                    return D.objectiveProgress[i] >= q.Objectives[i].Count;
            return false;
        }

        bool ObjectiveDoneAt(StoryState state, int index)
        {
            if (D.state != state) return D.state > state;
            var q = _story.Current;
            return index < q.Objectives.Length && D.objectiveProgress[index] >= q.Objectives[index].Count;
        }

        void SetItemCount(string id, int count)
        {
            int have = 0;
            foreach (string s in D.inventory) if (s == id) have++;
            int before = have;
            while (have < count) { D.inventory.Add(id); have++; }
            while (have > count) { D.inventory.Remove(id); have--; }

            // Pemberitahuan saat barang misi bertambah (tidak saat memuat simpanan).
            var it = PulangItems.Get(id);
            if (_questItemsBaseline && it != null && it.Quest && count > before)
            {
                GameAudio.Play("pickup", 0.55f);
                _hud.ShowToast(("BARANG MISI DIDAPAT: " + it.Name + (count > 1 ? " x" + count : "") + " - LIHAT INVENTORI (I)").ToUpperInvariant());
            }
        }

        /// <summary>False sampai sinkronisasi pertama setelah memulai/memuat game (agar tidak ada notifikasi palsu).</summary>
        bool _questItemsBaseline;

        /// <summary>Ikon khusus barang misi (tas, kunci, buku, paket, surat, ...).</summary>
        static string ItemIcon(ItemDef it) =>
            it.Quest && PixelArt.Sprites.ContainsKey("lc_qi_" + it.Id) ? "lc_qi_" + it.Id : null;

        /// <summary>Nama misi yang sedang memakai barang ini (kosong bila hanya kenang-kenangan).</summary>
        string QuestTitleFor(string id)
        {
            if (_story.Data == null || id == "tas" || id == "barang_pribadi" || id == "kunci_kost") return null;
            string side = SideQuestTitleForItem(id);
            if (side != null) return "Sampingan: " + side;
            return _story.Current.Title;
        }

        // ================================================================== layar inventori

        void OpenInventory(bool fromTablet)
        {
            _returnToTablet = fromTablet;
            _invMessage = null;
            _invIndex = 0;
            _state = State.Inventory;
            _hud.SetPrompt(null);
            RefreshInventory();
            _inputLock = 0.2f;
        }

        /// <summary>Barang dalam tab aktif, dikelompokkan (barang sama = satu baris dengan jumlah).</summary>
        List<KeyValuePair<ItemDef, int>> InventoryRows()
        {
            var rows = new List<KeyValuePair<ItemDef, int>>();
            ItemKind? filter = InventoryTabs[_invTab];
            foreach (string id in D.inventory)
            {
                var it = PulangItems.Get(id);
                if (it == null || (filter != null && it.Kind != filter.Value)) continue;
                int idx = rows.FindIndex(p => p.Key.Id == it.Id);
                if (idx < 0) rows.Add(new KeyValuePair<ItemDef, int>(it, 1));
                else rows[idx] = new KeyValuePair<ItemDef, int>(it, rows[idx].Value + 1);
            }
            // Barang misi dulu, lalu makanan, hadiah, perlengkapan.
            rows.Sort((a, b) => KindOrder(a.Key.Kind).CompareTo(KindOrder(b.Key.Kind)));
            return rows;
        }

        static int KindOrder(ItemKind k) =>
            k == ItemKind.Misi ? 0 : k == ItemKind.Kendaraan ? 1 : k == ItemKind.Pakaian ? 2 : k == ItemKind.Makanan ? 3 : k == ItemKind.Hadiah ? 4 : 5;

        void RefreshInventory()
        {
            var rows = InventoryRows();
            _invIndex = Mathf.Clamp(_invIndex, 0, Mathf.Max(0, rows.Count - 1));
            var entries = new List<InvEntry>();
            foreach (var r in rows) entries.Add(EntryFor(r.Key, r.Value));

            string header = Rp(D.money) + "   ENERGI " + D.energy + "/100";
            // Tab MISI menampilkan jumlah barang misi yang dibawa, mis. "MISI (3)".
            int quest = 0;
            foreach (string id in D.inventory) { var qi = PulangItems.Get(id); if (qi != null && qi.Quest) quest++; }
            var tabs = (string[])InventoryTabNames.Clone();
            if (quest > 0) tabs[tabs.Length - 1] = "MISI (" + quest + ")";
            _hud.ShowInventory(tabs, _invTab, entries, _invIndex, header, _invMessage);
        }

        InvEntry EntryFor(ItemDef it, int count)
        {
            var e = new InvEntry { Name = it.Name, Count = count, Kind = it.Kind, Desc = it.Desc,
                Icon = ItemIcon(it) ?? (it.Id == "kunci_kost" ? "lc_item_key" : null) };
            if (it.Kind == ItemKind.Misi)
            {
                string title = QuestTitleFor(it.Id);
                if (!string.IsNullOrEmpty(title)) e.Desc = "DIPAKAI UNTUK MISI \"" + title + "\". " + it.Desc;
            }
            switch (it.Kind)
            {
                case ItemKind.Makanan:
                    e.Effect = "+" + it.Energy + " ENERGI" + (it.Gift != null ? "   BISA JADI HADIAH" : "");
                    e.Action = "E - MAKAN";
                    e.Usable = true;
                    break;
                case ItemKind.Hadiah:
                    e.Effect = "BERIKAN SAAT MENGOBROL DENGAN TEMAN";
                    e.Action = " ";
                    break;
                case ItemKind.Perlengkapan:
                    e.Effect = "SYARAT PEKERJAAN - TIDAK HABIS DIPAKAI";
                    e.Action = " ";
                    break;
                case ItemKind.Pakaian:
                {
                    bool worn = D.outfit == it.Id;
                    e.Icon = LifeArt.OutfitIcon(it.Outfit);
                    e.Effect = worn ? "SEDANG DIPAKAI" : "BELI LAGI MODEL LAIN DI TOKO PAKAIAN";
                    e.Action = worn ? "E - LEPAS (BAJU HARIAN)" : "E - PAKAI";
                    e.Usable = true;
                    break;
                }
                case ItemKind.Kendaraan:
                {
                    bool riding = D.vehicle == it.Id;
                    e.Icon = "lc_item_vehicle_" + it.Id;
                    e.Effect = "KECEPATAN x" + it.Speed.ToString("0.##") +
                               (it.Engine ? "   BENSIN " + Rp(it.Fuel) + "/PERJALANAN" : "   TANPA BENSIN");
                    e.Action = riding ? "E / V - TURUN & SIMPAN" : "E / V - PANGGIL & NAIKI";
                    e.Usable = true;
                    if (riding) e.Desc = "SEDANG DIKENDARAI. " + it.Desc;
                    break;
                }
                default:
                    e.Effect = QuestUse(it.Id);
                    e.Action = "BARANG MISI - TIDAK BISA DIBUANG";
                    break;
            }
            return e;
        }

        /// <summary>Petunjuk singkat kegunaan barang misi saat ini.</summary>
        string QuestUse(string id)
        {
            if (DeliveryTargets.TryGetValue(id, out NpcId[] targets))
            {
                var left = new List<string>();
                foreach (var t in targets)
                    if (!D.Has("antar:" + id + ":" + t) && _npcs.TryGetValue(t, out LifeNpc npc))
                        left.Add(npc.DisplayName);
                return left.Count > 0 ? "BELUM DIANTAR: " + string.Join(", ", left).ToUpperInvariant() : "SEMUA SUDAH DIANTAR";
            }
            switch (id)
            {
                case "kunci_kost": return "DIPAKAI OTOMATIS DI KOST PELANGI";
                case "surat_hendra": return "BICARA DENGAN PAK SLAMET UNTUK MENYERAHKAN";
                case "oleh_oleh": return "KIRIM LEWAT PAK HADI DI TERMINAL JOGOKERTO";
                case "buku_catatan_nabila": return "BICARA DENGAN NABILA UNTUK MENYERAHKAN";
                case "barang_bawaan": return "BAWA PULANG KE DESA SUKA MAKMUR";
                case "rantang_siti": return "BICARA DENGAN PAK SLAMET UNTUK MENYERAHKAN";
                case "suku_cadang": return "BICARA DENGAN PAK HADI UNTUK MENYERAHKAN";
                case "sayur_pesanan": return "BICARA DENGAN RIKO DI CAFE SENJA";
                case "titipan_rukmini": return "BICARA DENGAN BU SARI UNTUK MENYERAHKAN";
                case "obat_ternak": return "BICARA DENGAN PAK KARMAN UNTUK MENYERAHKAN";
                default: return "KENANGAN DARI RUMAH";
            }
        }

        void UpdateInventory()
        {
            if (_inputLock > 0f) return;
            if (LifeInput.CancelPressed || LifeInput.InventoryPressed)
            {
                _hud.HideInventory();
                CloseSubScreen();
                return;
            }

            var rows = InventoryRows();

            // Kursor: klik tab = ganti kategori; arahkan ke barang = pilih; klik barang terpilih = pakai.
            if (_hud.PointerClick(UiHit.InvTab, out int tab) && tab != _invTab && tab < InventoryTabs.Length)
            {
                _invTab = tab;
                _invIndex = 0;
                _invMessage = null;
                GameAudio.Play("click", 0.35f);
                RefreshInventory();
                return;
            }
            bool useClick = false;
            if (rows.Count > 0)
            {
                int pick = -1;
                if (_hud.PointerClick(UiHit.InvRow, out int clickRow)) { useClick = clickRow == _invIndex; pick = clickRow; }
                else if (_hud.PointerHover(UiHit.InvRow, out int hoverRow)) pick = hoverRow;
                float wheel = LifeInput.ScrollWheel;
                if (pick < 0 && wheel != 0f) pick = Mathf.Clamp(_invIndex + (wheel > 0f ? -1 : 1), 0, rows.Count - 1);
                if (pick >= 0 && pick < rows.Count && pick != _invIndex)
                {
                    _invIndex = pick;
                    _invMessage = null;
                    GameAudio.Play("click", 0.25f);
                    RefreshInventory();
                    return;
                }
            }

            if (LifeInput.LeftPressed || LifeInput.RightPressed)
            {
                int dir = LifeInput.LeftPressed ? -1 : 1;
                _invTab = (_invTab + dir + InventoryTabs.Length) % InventoryTabs.Length;
                _invIndex = 0;
                _invMessage = null;
                GameAudio.Play("click", 0.35f);
                RefreshInventory();
                return;
            }
            if (rows.Count > 0 && (LifeInput.UpPressed || LifeInput.DownPressed))
            {
                int dir = LifeInput.UpPressed ? -1 : 1;
                _invIndex = (_invIndex + dir + rows.Count) % rows.Count;
                _invMessage = null;
                GameAudio.Play("click", 0.35f);
                RefreshInventory();
                return;
            }
            if ((LifeInput.InteractPressed || LifeInput.VehiclePressed || useClick) && rows.Count > 0)
            {
                var it = rows[_invIndex].Key;
                if (it.Kind == ItemKind.Kendaraan)
                {
                    UseVehicleFromBag(it);
                    return;
                }
                if (!LifeInput.InteractPressed && !useClick) return;
                if (it.Kind == ItemKind.Makanan)
                {
                    EatFromBag(it.Id);
                    _invMessage = ("MAKAN " + it.Name + " - ENERGI " + D.energy + "/100").ToUpperInvariant();
                }
                else if (it.Kind == ItemKind.Pakaian)
                {
                    _invMessage = ToggleOutfit(it);
                }
                else
                {
                    GameAudio.Play("error", 0.4f);
                    _invMessage = it.Kind == ItemKind.Misi ? "BARANG INI DIPAKAI OTOMATIS SAAT MISI"
                        : it.Kind == ItemKind.Hadiah ? "BERIKAN SAAT MENGOBROL DENGAN TEMANMU"
                        : "DIPAKAI OTOMATIS SAAT BEKERJA";
                }
                RefreshInventory();
            }
        }

        /// <summary>
        /// Kendaraan di inventori: turun & simpan bila sedang dinaiki, atau panggil lalu langsung
        /// kembali ke permainan agar pemain melihat kendaraannya muncul.
        /// </summary>
        void UseVehicleFromBag(ItemDef it)
        {
            if (D.vehicle == it.Id)
            {
                ParkVehicle();
                _invMessage = (it.Name + " disimpan ke inventori").ToUpperInvariant();
                RefreshInventory();
                return;
            }
            if (!CanSummonVehicle(out string reason))
            {
                GameAudio.Play("error", 0.4f);
                _invMessage = reason;
                RefreshInventory();
                return;
            }
            _returnToTablet = false;
            _hud.HideInventory();
            CloseSubScreen();
            SummonVehicle(it);
        }

        /// <summary>Tutup layar (tugas/jurnal/inventori): kembali ke tablet bila dibuka dari sana.</summary>
        void CloseSubScreen()
        {
            _inputLock = 0.15f;
            if (_returnToTablet) { _returnToTablet = false; OpenTablet(); return; }
            _state = State.Playing;
        }
    }
}
