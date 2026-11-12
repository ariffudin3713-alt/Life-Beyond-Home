using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>Ikon 16x16 untuk aplikasi tablet dan kategori barang inventori.</summary>
    public static partial class LifeArt
    {
        static readonly Color32 IconInk = new Color32(0x24, 0x1a, 0x2b, 255);

        static void BuildTabletArt()
        {
            // ---------------- aplikasi tablet
            var c = Icon();                                   // TUGAS: papan klip
            Outlined(c, 3, 1, 10, 13, PixelArt.Hex("f4efe0"), IconInk);
            c.Rect(6, 13, 4, 2, PixelArt.Hex("8a9098"));
            for (int y = 4; y <= 10; y += 3) { c.Rect(5, y, 2, 2, PixelArt.Hex("4fae62")); c.Rect(8, y, 4, 1, IconInk); }
            Add(c, Center, "app_tasks");

            c = Icon();                                       // INVENTORI: tas ransel
            Outlined(c, 3, 1, 10, 11, PixelArt.Hex("c8794a"), IconInk);
            c.Rect(5, 12, 6, 2, IconInk); c.Rect(6, 12, 4, 1, PixelArt.Hex("c8794a"));
            Outlined(c, 5, 3, 6, 4, PixelArt.Hex("a85e36"), IconInk);
            c.Set(8, 5, PixelArt.Hex("ffd35c"));
            Add(c, Center, "app_bag");

            c = Icon();                                       // JURNAL: buku
            Outlined(c, 2, 2, 12, 11, PixelArt.Hex("6f8fd6"), IconInk);
            c.Rect(8, 3, 1, 9, IconInk);
            c.Rect(4, 9, 3, 1, PixelArt.Hex("e9e2c9")); c.Rect(10, 9, 3, 1, PixelArt.Hex("e9e2c9"));
            c.Rect(4, 7, 3, 1, PixelArt.Hex("e9e2c9")); c.Rect(10, 7, 3, 1, PixelArt.Hex("e9e2c9"));
            Add(c, Center, "app_journal");

            c = Icon();                                       // TELEPON
            Outlined(c, 4, 1, 8, 14, PixelArt.Hex("3a3f52"), IconInk);
            c.Rect(5, 4, 6, 8, PixelArt.Hex("7fd486"));
            c.Rect(7, 2, 2, 1, PixelArt.Hex("8a9098"));
            c.Set(8, 13, PixelArt.Hex("e9e2c9"));
            Add(c, Center, "app_phone");

            c = Icon();                                       // SIMPAN: disket
            Outlined(c, 2, 2, 12, 12, PixelArt.Hex("3b6fb6"), IconInk);
            c.Rect(5, 9, 6, 4, PixelArt.Hex("dcdde2"));
            c.Rect(9, 10, 1, 2, IconInk);
            c.Rect(4, 3, 8, 4, PixelArt.Hex("e9e2c9"));
            Add(c, Center, "app_save");

            c = Icon();                                       // SUARA: speaker
            c.Rect(3, 6, 3, 4, PixelArt.Hex("e9e2c9"));
            c.Triangle(new Vector2(5f, 8f), new Vector2(10f, 13f), new Vector2(10f, 3f), PixelArt.Hex("e9e2c9"));
            c.Rect(12, 6, 1, 4, PixelArt.Hex("ffd35c")); c.Rect(14, 5, 1, 6, PixelArt.Hex("ffd35c"));
            Add(c, Center, "app_sound");

            c = Icon();                                       // MENU UTAMA: rumah
            c.Triangle(new Vector2(8f, 15f), new Vector2(1f, 8f), new Vector2(15f, 8f), PixelArt.Hex("d8584a"));
            Outlined(c, 3, 1, 10, 8, PixelArt.Hex("f0dcb8"), IconInk);
            c.Rect(7, 1, 3, 5, PixelArt.Hex("7b4a2a"));
            Add(c, Center, "app_home");

            c = Icon();                                       // PROFIL: kartu identitas
            Outlined(c, 1, 3, 14, 10, PixelArt.Hex("e9e2c9"), IconInk);
            c.Disc(5f, 9f, 2.2f, PixelArt.Hex("f2c49b"));                 // kepala
            c.Rect(3, 4, 5, 3, PixelArt.Hex("4fae62"));                   // badan
            c.Rect(9, 9, 4, 1, IconInk); c.Rect(9, 7, 4, 1, IconInk); c.Rect(9, 5, 3, 1, IconInk);
            Add(c, Center, "app_profile");

            c = Icon();                                       // PANDUAN: buku terbuka
            c.Triangle(new Vector2(8f, 3f), new Vector2(1f, 5f), new Vector2(1f, 13f), PixelArt.Hex("c8794a"));
            Outlined(c, 1, 4, 7, 10, PixelArt.Hex("f4efe0"), IconInk);
            Outlined(c, 8, 4, 7, 10, PixelArt.Hex("f4efe0"), IconInk);
            c.Rect(7, 3, 2, 11, PixelArt.Hex("b8503f"));                  // punggung buku
            for (int y = 6; y <= 11; y += 2) { c.Rect(2, y, 4, 1, PixelArt.Hex("8a9098")); c.Rect(10, y, 4, 1, PixelArt.Hex("8a9098")); }
            c.Rect(11, 12, 2, 4, PixelArt.Hex("e8b23a"));                 // pita pembatas
            Add(c, Center, "app_guide");

            c = Icon();                                       // TUTUP: silang
            c.Line(3, 3, 12, 12, PixelArt.Hex("ff6b6b"), 2);
            c.Line(3, 12, 12, 3, PixelArt.Hex("ff6b6b"), 2);
            Add(c, Center, "app_close");

            // ---------------- kategori barang
            c = Icon();                                       // makanan: roti
            c.Ellipse(8f, 7f, 6f, 4f, PixelArt.Hex("8a5a2c"));
            c.Ellipse(8f, 7.5f, 5f, 3f, PixelArt.Hex("e0a65a"));
            c.Rect(5, 8, 1, 2, PixelArt.Hex("fff1d6")); c.Rect(8, 8, 1, 2, PixelArt.Hex("fff1d6"));
            c.Rect(11, 8, 1, 2, PixelArt.Hex("fff1d6"));
            Add(c, Center, "item_food");

            c = Icon();                                       // hadiah: kotak berpita
            Outlined(c, 2, 1, 12, 10, PixelArt.Hex("ff9fc0"), IconInk);
            c.Rect(7, 1, 2, 10, PixelArt.Hex("ffd35c"));
            c.Rect(2, 7, 12, 2, PixelArt.Hex("ffd35c"));
            c.Disc(6f, 12.5f, 2f, PixelArt.Hex("ffd35c")); c.Disc(10f, 12.5f, 2f, PixelArt.Hex("ffd35c"));
            Add(c, Center, "item_gift");

            c = Icon();                                       // perlengkapan: kunci pas
            c.Line(4, 4, 11, 11, PixelArt.Hex("aab2b4"), 3);
            c.Disc(12f, 12f, 3f, PixelArt.Hex("aab2b4")); c.Disc(13f, 13f, 1.4f, new Color32(0, 0, 0, 0));
            c.Disc(4f, 4f, 2.4f, PixelArt.Hex("8a9098"));
            Add(c, Center, "item_gear");

            c = Icon();                                       // misi: paket berbintang
            Outlined(c, 2, 2, 12, 10, PixelArt.Hex("c8a46a"), IconInk);
            c.Rect(2, 9, 12, 1, PixelArt.Hex("8a6a44"));
            c.Rect(7, 2, 2, 10, PixelArt.Hex("8a6a44"));
            c.Rect(11, 12, 3, 3, PixelArt.Hex("ffd35c"));
            Add(c, Center, "item_quest");

            c = Icon();                                       // kunci
            c.Disc(5f, 10f, 3.5f, PixelArt.Hex("ffd35c")); c.Disc(5f, 10f, 1.4f, new Color32(0, 0, 0, 0));
            c.Rect(7, 9, 7, 2, PixelArt.Hex("ffd35c"));
            c.Rect(11, 6, 2, 3, PixelArt.Hex("ffd35c")); c.Rect(13, 7, 1, 2, PixelArt.Hex("ffd35c"));
            Add(c, Center, "item_key");
        }

        static readonly Vector2 Center = new Vector2(0.5f, 0.5f);

        static PixelCanvas Icon() => new PixelCanvas(16, 16);
    }
}
