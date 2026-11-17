using System;
using System.Collections.Generic;
using PixelAdventure.LifeSim.Pulang;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Pakaian tokoh utama dalam tiga slot: TOPI, BAJU, SEPATU.
    /// - Dibeli di Toko Pakaian (Pasar Ramai Jaya) dan langsung dipakai.
    /// - Bisa diganti / dilepas kapan saja dan di mana saja lewat inventori (tombol I, ikon tas
    ///   di layar, atau aplikasi tablet) - tab PAKAIAN. Sprite tokoh berubah seketika.
    /// - Tersimpan di PulangSaveData (outfit / hat / shoes) dan langsung disimpan setiap berganti,
    ///   jadi tidak kembali ke baju harian bila game ditutup lalu dimuat ulang.
    /// </summary>
    public partial class PulangGame
    {
        const int OutfitsPerPage = 6;

        static readonly WearSlot[] WearSlots = { WearSlot.Topi, WearSlot.Baju, WearSlot.Sepatu };

        static string SlotName(WearSlot s) =>
            s == WearSlot.Topi ? "TOPI" : s == WearSlot.Sepatu ? "SEPATU" : "BAJU";

        static string DefaultWearName(WearSlot s) =>
            s == WearSlot.Topi ? "TANPA TOPI" : s == WearSlot.Sepatu ? "SEPATU HARIAN" : "BAJU HARIAN";

        /// <summary>Id barang yang dipakai di slot ini ("" = bawaan).</summary>
        string WornId(WearSlot s) =>
            (s == WearSlot.Topi ? D.hat : s == WearSlot.Sepatu ? D.shoes : D.outfit) ?? "";

        void SetWornId(WearSlot s, string id)
        {
            if (s == WearSlot.Topi) D.hat = id;
            else if (s == WearSlot.Sepatu) D.shoes = id;
            else D.outfit = id;
        }

        /// <summary>Barang yang dipakai di slot ini (null = bawaan / tidak valid).</summary>
        ItemDef Worn(WearSlot s)
        {
            string id = WornId(s);
            var it = string.IsNullOrEmpty(id) ? null : PulangItems.Get(id);
            return it != null && it.IsOutfit && it.Slot == s ? it : null;
        }

        bool IsWorn(ItemDef it) => it != null && it.IsOutfit && WornId(it.Slot) == it.Id;

        static int StyleOf(ItemDef it) =>
            it == null ? -1 : it.Slot == WearSlot.Topi ? it.Hat : it.Slot == WearSlot.Sepatu ? it.Shoes : it.Outfit;

        /// <summary>
        /// Terapkan pakaian simpanan ke sprite tokoh utama (saat mulai/memuat/ganti).
        /// Barang yang tidak dimiliki lagi atau id yang tidak dikenal (simpanan rusak) dilepas.
        /// </summary>
        void ApplyOutfit()
        {
            if (_story.Data == null) return;
            foreach (var s in WearSlots)
            {
                string id = WornId(s);
                if (string.IsNullOrEmpty(id)) { SetWornId(s, ""); continue; }
                var it = Worn(s);
                if (it == null || D.inventory == null || !D.inventory.Contains(it.Id)) SetWornId(s, "");
            }
            LifeArt.ApplyPlayerLook(StyleOf(Worn(WearSlot.Baju)), StyleOf(Worn(WearSlot.Topi)),
                StyleOf(Worn(WearSlot.Sepatu)));
        }

        /// <summary>Ikon pratinjau: tokoh memakai pakaian sekarang, dengan slot barang ini diganti barang ini.</summary>
        string PreviewIcon(ItemDef it)
        {
            int body = StyleOf(Worn(WearSlot.Baju)), hat = StyleOf(Worn(WearSlot.Topi)), shoes = StyleOf(Worn(WearSlot.Sepatu));
            if (it.Slot == WearSlot.Topi) hat = it.Hat;
            else if (it.Slot == WearSlot.Sepatu) shoes = it.Shoes;
            else body = it.Outfit;
            return LifeArt.LookIcon(body, hat, shoes);
        }

        /// <summary>Pakai barang ini di slotnya; bila sedang dipakai, lepas (kembali ke bawaan slot itu).</summary>
        string ToggleOutfit(ItemDef it)
        {
            if (it == null || !it.IsOutfit) return " ";
            string msg;
            if (IsWorn(it))
            {
                SetWornId(it.Slot, "");
                GameAudio.Play("pickup", 0.45f);
                msg = SlotName(it.Slot) + " DILEPAS - " + DefaultWearName(it.Slot);
            }
            else
            {
                SetWornId(it.Slot, it.Id);
                GameAudio.Play("pickup", 0.5f, 1.15f);
                msg = (it.Name + " dipakai").ToUpperInvariant();
            }
            ApplyOutfit();
            SaveWardrobe();
            return msg;
        }

        /// <summary>Simpan segera agar pakaian tidak hilang bila game ditutup sebelum simpan otomatis.</summary>
        void SaveWardrobe()
        {
            if (_story.Data != null && D.state != StoryState.NameEntry) D.Save();
        }

        /// <summary>Ringkasan pakaian terpasang, mis. "TOPI: PECI HITAM  BAJU: BAJU HARIAN  SEPATU: ...".</summary>
        string WornSummary()
        {
            var parts = new List<string>();
            foreach (var s in WearSlots)
            {
                var it = Worn(s);
                parts.Add(SlotName(s) + ": " + (it != null ? it.Name.ToUpperInvariant() : DefaultWearName(s)));
            }
            return string.Join("   ", parts);
        }

        static List<ItemDef> OutfitsSoldAt(LocationId loc)
        {
            var list = new List<ItemDef>();
            foreach (var it in PulangItems.SoldAt(loc))
                if (it.IsOutfit) list.Add(it);
            return list;
        }

        static List<ItemDef> OutfitsSoldAt(LocationId loc, WearSlot slot)
        {
            var list = new List<ItemDef>();
            foreach (var it in OutfitsSoldAt(loc))
                if (it.Slot == slot) list.Add(it);
            return list;
        }

        /// <summary>Toko pakaian: pilih kategori (baju / topi / sepatu) bila ada lebih dari satu.</summary>
        void OpenClothesShop(LocationId loc)
        {
            var labels = new List<string>();
            var actions = new List<Action>();
            foreach (var s in WearSlots)
            {
                int n = OutfitsSoldAt(loc, s).Count;
                if (n == 0) continue;
                WearSlot slot = s;
                labels.Add(SlotName(s) + " (" + n + " MODEL)");
                actions.Add(() => OpenOutfitShop(loc, slot, 0));
            }
            if (labels.Count == 0) return;
            if (labels.Count == 1) { actions[0](); return; }
            labels.Add("SELESAI");
            actions.Add(null);
            OpenMenu("TOKO PAKAIAN - " + Rp(D.money), labels, actions);
        }

        /// <summary>Koleksi satu slot: beli (langsung dipakai) atau pakai/lepas yang sudah dimiliki.</summary>
        void OpenOutfitShop(LocationId loc, WearSlot slot, int page)
        {
            var all = OutfitsSoldAt(loc, slot);
            if (all.Count == 0) return;
            int pages = (all.Count + OutfitsPerPage - 1) / OutfitsPerPage;
            page = ((page % pages) + pages) % pages;

            var labels = new List<string>();
            var actions = new List<Action>();
            for (int i = page * OutfitsPerPage; i < Math.Min(all.Count, (page + 1) * OutfitsPerPage); i++)
            {
                ItemDef it = all[i];
                bool owned = D.inventory.Contains(it.Id);
                if (IsWorn(it)) labels.Add(("Lepas: " + it.Name + " (dipakai)").ToUpperInvariant());
                else if (owned) labels.Add(("Pakai: " + it.Name).ToUpperInvariant());
                else labels.Add(("Beli " + it.Name + " - " + Rp(it.Price)).ToUpperInvariant());
                actions.Add(() =>
                {
                    if (D.inventory.Contains(it.Id)) _hud.ShowToast(ToggleOutfit(it));
                    else BuyItem(it.Id, false);
                    OpenOutfitShop(loc, slot, page);
                });
            }
            if (pages > 1)
            {
                int next = page + 1;
                labels.Add(("Model lain (" + (page + 1) + "/" + pages + ")").ToUpperInvariant());
                actions.Add(() => OpenOutfitShop(loc, slot, next));
            }
            int categories = 0;
            foreach (var s in WearSlots) if (OutfitsSoldAt(loc, s).Count > 0) categories++;
            if (categories > 1) { labels.Add("KEMBALI"); actions.Add(() => OpenClothesShop(loc)); }
            else { labels.Add("SELESAI"); actions.Add(null); }

            var worn = Worn(slot);
            OpenMenu(SlotName(slot) + " - DIPAKAI: " + (worn != null ? worn.Name.ToUpperInvariant() : DefaultWearName(slot)) +
                     " - " + Rp(D.money), labels, actions);
        }
    }
}
