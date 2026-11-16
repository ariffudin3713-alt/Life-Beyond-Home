using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Ikon khusus tiap barang misi (16x16, pivot tengah) + efek penanda barang misi di dunia:
    /// cahaya emas di tanah, pilar cahaya, dan gelembung "!".
    /// </summary>
    public static partial class LifeArt
    {
        static void BuildQuestItemArt()
        {
            Color32 ink = IconInk;
            Color32 gold = PixelArt.Hex("ffd35c");

            var c = Icon();                                                   // tas ransel
            Outlined(c, 3, 1, 10, 12, PixelArt.Hex("3b6fb6"), ink);
            Outlined(c, 4, 2, 8, 5, PixelArt.Hex("2f5a96"), ink);
            c.Rect(6, 13, 4, 2, ink); c.Rect(7, 13, 2, 1, PixelArt.Hex("3b6fb6"));
            c.Rect(7, 4, 2, 1, gold);
            Add(c, Center, "qi_tas");

            c = Icon();                                                       // barang pribadi: kardus berselotip
            Outlined(c, 1, 1, 14, 10, PixelArt.Hex("c8a46a"), ink);
            c.Rect(2, 9, 12, 1, PixelArt.Hex("a8844a"));
            c.Rect(7, 1, 2, 10, PixelArt.Hex("e8d8a8"));
            Outlined(c, 4, 11, 6, 3, PixelArt.Hex("f4efe0"), ink);             // baju terlipat di atas
            Add(c, Center, "qi_barang_pribadi");

            c = Icon();                                                       // kunci kost bergantungan
            c.Disc(5f, 10f, 3.5f, gold); c.Disc(5f, 10f, 1.4f, new Color32(0, 0, 0, 0));
            c.Rect(7, 9, 7, 2, gold); c.Rect(11, 6, 2, 3, gold); c.Rect(13, 7, 1, 2, gold);
            Outlined(c, 1, 1, 6, 5, PixelArt.Hex("ff7aa0"), ink);              // gantungan
            c.Line(4, 6, 5, 7, ink);
            Add(c, Center, "qi_kunci_kost");

            c = Icon();                                                       // buku catatan kecil
            Outlined(c, 3, 1, 10, 13, PixelArt.Hex("e46f9a"), ink);
            c.Rect(4, 2, 1, 11, PixelArt.Hex("b84f78"));
            c.Rect(6, 9, 5, 1, PixelArt.Hex("fff1f6")); c.Rect(6, 7, 5, 1, PixelArt.Hex("fff1f6"));
            c.Disc(11f, 12f, 1.3f, gold);
            Add(c, Center, "qi_buku_catatan_nabila");

            c = Icon();                                                       // paket titipan
            Outlined(c, 2, 1, 12, 11, PixelArt.Hex("d8b07a"), ink);
            c.Rect(7, 1, 2, 11, PixelArt.Hex("8a6a44")); c.Rect(2, 7, 12, 1, PixelArt.Hex("8a6a44"));
            Outlined(c, 9, 2, 4, 3, PixelArt.Hex("ffffff"), ink);              // label alamat
            c.Disc(5f, 13f, 1.6f, PixelArt.Hex("e0383c")); c.Disc(10f, 13f, 1.6f, PixelArt.Hex("e0383c"));   // pita
            Add(c, Center, "qi_paket_dimas");

            c = Icon();                                                       // barang dagangan: keranjang sayur
            Outlined(c, 2, 1, 12, 7, PixelArt.Hex("b8864a"), ink);
            for (int x = 3; x < 14; x += 2) c.Rect(x, 2, 1, 5, PixelArt.Hex("8a6a3a"));
            c.Disc(5f, 9f, 2.2f, PixelArt.Hex("e0383c")); c.Disc(8f, 10f, 2.2f, PixelArt.Hex("4f9a3c"));
            c.Disc(11f, 9f, 2.2f, PixelArt.Hex("f0963a"));
            c.Line(3, 8, 8, 14, ink); c.Line(8, 14, 13, 8, ink);              // pegangan
            Add(c, Center, "qi_barang_pasar");

            c = Icon();                                                       // surat beramplop
            Outlined(c, 1, 3, 14, 10, PixelArt.Hex("f4efe0"), ink);
            c.Line(1, 12, 8, 7, ink); c.Line(14, 12, 8, 7, ink);
            c.Disc(8f, 7f, 1.6f, PixelArt.Hex("d8303a"));                       // segel lilin
            Add(c, Center, "qi_surat_hendra");

            c = Icon();                                                       // kain batik terlipat
            Outlined(c, 1, 2, 14, 10, PixelArt.Hex("8a4a2a"), ink);
            for (int y = 3; y < 11; y += 3)
                for (int x = 2 + (y % 2); x < 14; x += 3) c.Set(x, y, gold);
            c.Rect(2, 7, 12, 1, PixelArt.Hex("5e2e18"));
            c.Disc(12f, 12.5f, 1.8f, PixelArt.Hex("e0383c"));                   // pita hadiah
            Add(c, Center, "qi_oleh_oleh");

            c = Icon();                                                       // barang bawaan: koper
            Outlined(c, 2, 1, 12, 10, PixelArt.Hex("8a5a3c"), ink);
            c.Rect(3, 6, 10, 1, PixelArt.Hex("6b4126"));
            Outlined(c, 6, 11, 4, 3, PixelArt.Hex("6b737c"), ink);
            c.Rect(7, 11, 2, 2, new Color32(0, 0, 0, 0));
            c.Rect(4, 2, 1, 8, gold); c.Rect(11, 2, 1, 8, gold);
            Add(c, Center, "qi_barang_bawaan");

            // ---- efek penanda di dunia
            c = new PixelCanvas(40, 16);                                      // cahaya emas di tanah
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 40; x++)
                {
                    float dx = (x - 19.5f) / 20f, dy = (y - 7.5f) / 8f;
                    float d = dx * dx + dy * dy;
                    if (d < 1f) c.Set(x, y, new Color32(255, 214, 92, (byte)(150 * (1f - d))));
                }
            Add(c, Center, "quest_glow");

            c = new PixelCanvas(14, 56);                                      // pilar cahaya
            for (int y = 0; y < 56; y++)
                for (int x = 0; x < 14; x++)
                {
                    float fx = 1f - Mathf.Abs(x - 6.5f) / 7f;
                    float fy = 1f - y / 56f;
                    c.Set(x, y, new Color32(255, 230, 140, (byte)(110 * fx * fx * fy)));
                }
            Add(c, new Vector2(0.5f, 0f), "quest_beam");

            c = new PixelCanvas(11, 13);                                      // gelembung "!"
            c.Disc(5f, 7f, 5f, IconInk); c.Disc(5f, 7f, 4f, gold);
            c.Triangle(new Vector2(3f, 3f), new Vector2(7f, 3f), new Vector2(5f, 0f), IconInk);
            c.Rect(4, 3, 3, 1, gold);
            c.Rect(5, 7, 1, 4, IconInk); c.Set(5, 5, IconInk);
            Add(c, new Vector2(0.5f, 0f), "quest_bang");
        }
    }
}
