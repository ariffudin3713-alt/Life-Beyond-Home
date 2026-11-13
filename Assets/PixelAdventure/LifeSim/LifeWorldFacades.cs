using System.Collections.Generic;
using PixelAdventure.LifeSim.Pulang;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Fasad khusus per lokasi: gambar, ukuran penghalang (W x H unit), tinggi papan nama,
    /// dan cahaya jendela malam. Lokasi yang tidak tercantum memakai gambar bawaan jenisnya.
    /// </summary>
    public partial class LifeWorld
    {
        struct Facade
        {
            public string Sprite;
            public float W, H, Plate, LightH, Window;
            public bool Door;
        }

        static Facade F(string sprite, float w, float h, float plate, float lightH, float window = 0.38f, bool door = true) =>
            new Facade { Sprite = "lc_" + sprite, W = w, H = h, Plate = plate, LightH = lightH, Window = window, Door = door };

        static readonly Dictionary<LocationId, Facade> Facades = new Dictionary<LocationId, Facade>
        {
            // Bangunan umum
            { LocationId.Sekolah,          F("bld_school",     7.2f, 4.2f, 3.25f, 4.0f, 0.30f) },
            { LocationId.RumahSakitSehat,  F("bld_hospital",   6.0f, 5.0f, 6.35f, 5.6f, 0.42f) },
            { LocationId.Perpustakaan,     F("bld_library",    5.8f, 3.6f, 5.25f, 4.0f, 0.34f) },
            { LocationId.BengkelJayaMotor, F("bld_workshop",   6.0f, 3.6f, 3.2f,  3.6f, 0.36f) },
            { LocationId.BalaiDesa,        F("bld_balai",      6.0f, 3.2f, 5.5f,  2.6f, 0.30f) },
            { LocationId.PTMajuBersama,    F("bld_office",     4.0f, 4.6f, 1.25f, 6.4f, 0.48f, false) },
            { LocationId.KostPelangi,      F("bld_kost",       6.6f, 4.6f, 6.55f, 5.0f, 0.42f) },

            // Toko sesuai jenisnya
            { LocationId.TokoPakaian,      F("bld_clothes",    6.0f, 3.8f, 3.55f, 4.0f, 0.42f) },
            { LocationId.Minimarket,       F("bld_minimarket", 6.0f, 3.8f, 3.3f,  3.8f, 0.48f) },
            { LocationId.TokoBungaMelati,  F("bld_flower",     6.0f, 3.8f, 3.5f,  3.8f, 0.40f) },
            { LocationId.TokoBuku,         F("bld_books",      6.0f, 3.8f, 4.25f, 3.8f, 0.42f) },
            { LocationId.TokoElektronik,   F("bld_electronics",6.0f, 3.8f, 3.1f,  3.8f, 0.50f) },
            { LocationId.DealerKendaraan,  F("bld_dealer",     6.6f, 3.4f, 3.1f,  3.4f, 0.46f) },
            { LocationId.TokoSerbaAda,     F("bld_grocery_a",  6.0f, 3.8f, 3.2f,  3.8f, 0.40f) },
            { LocationId.TokoKebutuhanRumah, F("bld_grocery_a",6.0f, 3.8f, 3.2f,  3.8f, 0.40f) },
            { LocationId.TokoAnugerah,     F("bld_grocery_b",  6.0f, 3.8f, 3.2f,  3.8f, 0.40f) },
            { LocationId.TokoKelontong,    F("bld_grocery_b",  6.0f, 3.8f, 3.2f,  3.8f, 0.40f) },
            { LocationId.TokoPancing,      F("bld_fishing",    6.0f, 3.6f, 3.0f,  3.6f, 0.36f) },

            // Tempat ibadah
            { LocationId.MasjidKota,       F("bld_masjid",     6.8f, 3.6f, 2.5f,  3.4f, 0.40f) },
            { LocationId.GerejaKota,       F("bld_gereja",     5.8f, 3.8f, 2.6f,  3.6f, 0.40f) },
        };

        static bool TrySpecialFacade(LocationId id, out Facade f) => Facades.TryGetValue(id, out f);
    }
}
