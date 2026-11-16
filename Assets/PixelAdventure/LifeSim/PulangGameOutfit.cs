using System;
using System.Collections.Generic;
using PixelAdventure.LifeSim.Pulang;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Pakaian tokoh utama: dibeli di Toko Pakaian (Pasar Ramai Jaya), langsung dipakai,
    /// dan bisa diganti / dilepas kapan saja lewat inventori (tab PAKAIAN) atau menu toko.
    /// Pakaian tersimpan di simpanan (PulangSaveData.outfit).
    /// </summary>
    public partial class PulangGame
    {
        const int OutfitsPerPage = 6;

        /// <summary>Pakaian yang sedang dipakai (null = baju harian).</summary>
        ItemDef WornOutfit
        {
            get
            {
                var it = string.IsNullOrEmpty(D.outfit) ? null : PulangItems.Get(D.outfit);
                return it != null && it.IsOutfit ? it : null;
            }
        }

        /// <summary>Terapkan pakaian simpanan ke sprite tokoh utama (saat mulai/memuat/ganti).</summary>
        void ApplyOutfit()
        {
            if (_story.Data == null) return;
            var worn = WornOutfit;
            if (worn != null && !D.inventory.Contains(worn.Id)) { D.outfit = ""; worn = null; }
            LifeArt.ApplyPlayerOutfit(worn != null ? worn.Outfit : -1);
        }

        /// <summary>Pakai pakaian ini; bila sedang dipakai, lepas dan kembali ke baju harian.</summary>
        string ToggleOutfit(ItemDef it)
        {
            if (D.outfit == it.Id)
            {
                D.outfit = "";
                ApplyOutfit();
                GameAudio.Play("pickup", 0.45f);
                return "KEMBALI MEMAKAI BAJU HARIAN";
            }
            D.outfit = it.Id;
            ApplyOutfit();
            GameAudio.Play("pickup", 0.5f, 1.15f);
            return (it.Name + " dipakai").ToUpperInvariant();
        }

        static List<ItemDef> OutfitsSoldAt(LocationId loc)
        {
            var list = new List<ItemDef>();
            foreach (var it in PulangItems.SoldAt(loc))
                if (it.IsOutfit) list.Add(it);
            return list;
        }

        /// <summary>Koleksi pakaian toko: beli (langsung dipakai) atau pakai yang sudah dimiliki.</summary>
        void OpenOutfitShop(LocationId loc, int page)
        {
            var all = OutfitsSoldAt(loc);
            if (all.Count == 0) return;
            int pages = (all.Count + OutfitsPerPage - 1) / OutfitsPerPage;
            page = ((page % pages) + pages) % pages;

            var labels = new List<string>();
            var actions = new List<Action>();
            for (int i = page * OutfitsPerPage; i < Math.Min(all.Count, (page + 1) * OutfitsPerPage); i++)
            {
                ItemDef it = all[i];
                bool owned = D.inventory.Contains(it.Id);
                if (D.outfit == it.Id) labels.Add(("Lepas: " + it.Name + " (dipakai)").ToUpperInvariant());
                else if (owned) labels.Add(("Pakai: " + it.Name).ToUpperInvariant());
                else labels.Add(("Beli " + it.Name + " - " + Rp(it.Price)).ToUpperInvariant());
                actions.Add(() =>
                {
                    if (D.inventory.Contains(it.Id)) _hud.ShowToast(ToggleOutfit(it));
                    else BuyItem(it.Id, false);
                    OpenOutfitShop(loc, page);
                });
            }
            if (pages > 1)
            {
                int next = page + 1;
                labels.Add(("Model lain (" + (page + 1) + "/" + pages + ")").ToUpperInvariant());
                actions.Add(() => OpenOutfitShop(loc, next));
            }
            labels.Add("SELESAI");
            actions.Add(null);

            var worn = WornOutfit;
            OpenMenu("KOLEKSI PAKAIAN - DIPAKAI: " + (worn != null ? worn.Name : "BAJU HARIAN") + " - " + Rp(D.money),
                labels, actions);
        }
    }
}
