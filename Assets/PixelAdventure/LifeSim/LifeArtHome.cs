using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>Gambar latar halaman utama: langit senja, matahari, awan, siluet kota, bukit.</summary>
    public static partial class LifeArt
    {
        static void BuildHomeArt()
        {
            // Langit senja bergradasi (dibentangkan memenuhi layar).
            var sky = new PixelCanvas(4, 96);
            Color32 top = PixelArt.Hex("1d2a5c"), mid = PixelArt.Hex("8a4f8f"), low = PixelArt.Hex("f39a6b"), hor = PixelArt.Hex("ffd59a");
            for (int y = 0; y < 96; y++)
            {
                float t = y / 95f;
                Color32 c = t < 0.25f ? Color32.Lerp(hor, low, t / 0.25f)
                          : t < 0.6f ? Color32.Lerp(low, mid, (t - 0.25f) / 0.35f)
                          : Color32.Lerp(mid, top, (t - 0.6f) / 0.4f);
                sky.Rect(0, y, 4, 1, c);
            }
            Add(sky, new Vector2(0.5f, 0f), "home_sky");

            // Matahari terbenam dengan cahaya lembut.
            var sun = new PixelCanvas(64, 64);
            sun.Disc(32f, 32f, 31f, new Color32(255, 214, 140, 60));
            sun.Disc(32f, 32f, 24f, new Color32(255, 200, 120, 110));
            sun.Disc(32f, 32f, 17f, PixelArt.Hex("ffe08a"));
            sun.Disc(32f, 32f, 13f, PixelArt.Hex("fff1c0"));
            Add(sun, new Vector2(0.5f, 0.5f), "home_sun");

            // Awan.
            var cloud = new PixelCanvas(56, 20);
            Color32 cw = new Color32(255, 236, 226, 230), cs = new Color32(230, 190, 200, 230);
            cloud.Ellipse(16f, 8f, 13f, 7f, cs); cloud.Ellipse(32f, 10f, 16f, 9f, cs); cloud.Ellipse(44f, 7f, 11f, 6f, cs);
            cloud.Ellipse(16f, 9f, 12f, 6f, cw); cloud.Ellipse(32f, 11f, 15f, 8f, cw); cloud.Ellipse(44f, 8f, 10f, 5f, cw);
            Add(cloud, new Vector2(0.5f, 0.5f), "home_cloud");

            // Siluet kota (gedung, menara, rumah) dengan jendela menyala; bisa diulang menyamping.
            var city = new PixelCanvas(384, 104);
            Color32 far = PixelArt.Hex("4a3560"), win = PixelArt.Hex("ffd98a");
            int x = 0, k = 0;
            while (x < 384)
            {
                int w = 18 + (int)(Hash(k, 3) % 26);
                int h = 30 + (int)(Hash(k, 7) % 66);
                city.Rect(x, 0, Mathf.Min(w, 384 - x), h, far);
                if (k % 5 == 2) city.Rect(x + w / 2 - 1, h, 2, 8, far);              // antena
                for (int wy = 8; wy < h - 6; wy += 9)
                    for (int wx = x + 4; wx < x + w - 4 && wx < 382; wx += 7)
                        if (Hash(wx, wy) % 3 == 0) city.Rect(wx, wy, 3, 4, win);
                x += w + 2 + (int)(Hash(k, 11) % 6);
                k++;
            }
            Add(city, new Vector2(0f, 0f), "home_city");

            // Bukit hijau gelap di depan kota.
            var hills = new PixelCanvas(384, 48);
            for (int px = 0; px < 384; px++)
            {
                float hgt = 22f + 10f * Mathf.Sin(px / 384f * Mathf.PI * 4f) + 6f * Mathf.Sin(px / 384f * Mathf.PI * 10f + 1f);
                hills.Rect(px, 0, 1, Mathf.RoundToInt(hgt), PixelArt.Hex("2f4a3a"));
                hills.Rect(px, 0, 1, Mathf.RoundToInt(hgt * 0.55f), PixelArt.Hex("253c2f"));
            }
            for (int t = 0; t < 30; t++)                                                  // pohon kecil
            {
                int tx = (int)(Hash(t, 21) % 380);
                float hgt = 22f + 10f * Mathf.Sin(tx / 384f * Mathf.PI * 4f) + 6f * Mathf.Sin(tx / 384f * Mathf.PI * 10f + 1f);
                hills.Disc(tx, hgt + 3f, 4f, PixelArt.Hex("1f3428"));
            }
            Add(hills, new Vector2(0f, 0f), "home_hills");
        }
    }
}
