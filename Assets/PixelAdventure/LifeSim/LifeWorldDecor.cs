using System.Collections.Generic;
using PixelAdventure.LifeSim.Pulang;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>Bangunan dekorasi yang bisa "dilihat" (E) tetapi tidak bisa dimasuki.</summary>
    public struct DecorBuilding
    {
        public Vector2 Door;
        public string Label;     // untuk prompt, mis. "RUMAH WARGA"
        public string Message;   // ditampilkan saat menekan E
        public bool Bin;         // tong sampah: tempat membuang sampah yang dipungut
    }

    /// <summary>
    /// Bangunan pengisi kota: rumah warga, toko tutup, dan bengkel terbengkalai.
    /// Semuanya penghalang (tidak bisa ditembus) dan tidak bisa dimasuki.
    /// Posisi dipilih di lahan kosong: tidak di jalan/trotoar, jalur pejalan kaki, atau dekat pintu lain.
    /// </summary>
    public partial class LifeWorld
    {
        readonly Dictionary<Region, List<DecorBuilding>> _decor = new Dictionary<Region, List<DecorBuilding>>();

        public IReadOnlyList<DecorBuilding> DecorIn(Region r) =>
            _decor.TryGetValue(r, out var list) ? list : (IReadOnlyList<DecorBuilding>)new DecorBuilding[0];

        enum DecorKind { HouseBlue, HouseGreen, HouseBrown, ClosedShop, Ruin, Ruko, Mushola }

        static readonly string[] HouseLines =
        {
            "Rumah warga. Pintunya tertutup rapat.",
            "Terdengar suara televisi dari dalam rumah.",
            "Ada sandal berjajar di depan pintu. Penghuninya sedang beristirahat.",
            "Rumah ini sepi. Penghuninya mungkin sedang bekerja.",
        };

        void BuildDecor(Region r)
        {
            _decor[r] = new List<DecorBuilding>();
            switch (r)
            {
                case Region.KotaJogokerto:
                    Decor(DecorKind.HouseBlue, 38f, 49f);
                    Decor(DecorKind.HouseGreen, 5f, 21f);
                    Decor(DecorKind.Ruin, 78.5f, 21f, "Bengkel Tua");
                    Decor(DecorKind.HouseBrown, 90.5f, 21f);
                    Decor(DecorKind.HouseGreen, 10f, 8f);
                    Decor(DecorKind.HouseBlue, 60f, 8f);
                    Decor(DecorKind.HouseGreen, 86f, 8f);
                    // Pengisi celah antar bangunan
                    Decor(DecorKind.HouseGreen, 91.5f, 42f);
                    Decor(DecorKind.Ruko, 17.5f, 8f);
                    Decor(DecorKind.HouseBlue, 79f, 8f);
                    Decor(DecorKind.HouseBrown, 92.5f, 8f);
                    break;

                case Region.KampungCempaka:
                    Decor(DecorKind.HouseBrown, 14f, 50f);
                    Decor(DecorKind.HouseBlue, 38.5f, 46f);
                    Decor(DecorKind.ClosedShop, 53f, 44f, "Warung Tutup");
                    Decor(DecorKind.HouseGreen, 12f, 22f);
                    Decor(DecorKind.Ruin, 24f, 22f, "Bengkel Tua");
                    Decor(DecorKind.HouseGreen, 77f, 36f);
                    Decor(DecorKind.Mushola, 23f, 52f);
                    Decor(DecorKind.HouseBrown, 52f, 51f);
                    Decor(DecorKind.HouseBlue, 88f, 48f);
                    Decor(DecorKind.HouseGreen, 52.5f, 21f);
                    break;

                case Region.PasarRamaiJaya:
                    Decor(DecorKind.HouseBlue, 12f, 48f);
                    Decor(DecorKind.ClosedShop, 26f, 48f, "Disewakan");
                    Decor(DecorKind.HouseBrown, 40f, 48f);
                    Decor(DecorKind.HouseGreen, 68f, 48f);
                    Decor(DecorKind.Ruin, 82f, 48f, "Gudang Tua");
                    // Deretan ruko & mushola di selatan jalan (antara los pasar dan trotoar)
                    Decor(DecorKind.Ruko, 12f, 20.5f);
                    Decor(DecorKind.Ruko, 28f, 20.5f);
                    Decor(DecorKind.Mushola, 64f, 20.5f);
                    Decor(DecorKind.Ruko, 80f, 20.5f);
                    // Rumah warga di belakang deretan utara
                    Decor(DecorKind.HouseGreen, 19f, 56f);
                    Decor(DecorKind.HouseBrown, 47f, 56f);
                    Decor(DecorKind.HouseBlue, 75f, 56f);
                    break;

                case Region.DesaSukaMakmur:
                    Decor(DecorKind.HouseGreen, 16f, 22f);
                    Decor(DecorKind.HouseBrown, 34f, 22f);
                    Decor(DecorKind.HouseBlue, 42f, 48f);
                    Decor(DecorKind.Ruin, 57f, 48f, "Lumbung Tua");
                    Decor(DecorKind.HouseBrown, 91f, 40f);
                    Decor(DecorKind.Mushola, 8f, 19.5f);
                    break;

                case Region.PantaiCemara:
                    Decor(DecorKind.HouseBrown, 88f, 16f);
                    Decor(DecorKind.HouseBlue, 42f, 16f);      // rumah nelayan
                    Decor(DecorKind.HouseGreen, 72f, 15.5f);
                    break;
            }
        }

        /// <summary>Kupu-kupu & burung di taman kota, alun-alun, lapangan, dan kebun.</summary>
        void BuildCritters(Region r, Transform root)
        {
            var c = LifeCritters.Attach(root);
            switch (r)
            {
                case Region.KotaJogokerto:
                    c.AddGarden(new Rect(8f, 51f, 20f, 12f), 7, 4);    // Taman Pelangi
                    c.AddGarden(new Rect(64.5f, 51f, 30f, 20f), 8, 9);  // Alun-Alun Merdeka
                    break;
                case Region.KampungCempaka:
                    c.AddGarden(new Rect(61f, 47f, 20f, 14f), 3, 3);   // Lapangan Kampung
                    c.AddGarden(new Rect(59f, 7f, 16f, 12f), 4, 2);    // Kebun Warga
                    break;
                case Region.DesaSukaMakmur:
                    c.AddGarden(new Rect(9f, 49f, 22f, 14f), 3, 3);    // Lapangan Desa
                    c.AddGarden(new Rect(63f, 47f, 28f, 18f), 4, 2);   // Perkebunan
                    break;
                case Region.PasarRamaiJaya:
                    c.AddGarden(new Rect(10f, 44f, 76f, 2f), 0, 3);    // burung mencari remah di belakang los
                    break;
            }
        }

        void Decor(DecorKind kind, float x, float y, string plate = null)
        {
            string label, message;
            switch (kind)
            {
                case DecorKind.ClosedShop:
                    Building("lc_shop_closed", x, y, 1f, 6.0f, 4.4f);
                    label = "TOKO TUTUP";
                    message = plate == "Disewakan"
                        ? "Tokonya tutup. Ada tulisan: DISEWAKAN, HUBUNGI PEMILIK."
                        : "Rolling door-nya terkunci. Toko ini sedang tutup.";
                    break;
                case DecorKind.Ruko:
                    Building("lc_ruko", x, y, 1f, 6.0f, 4.6f);
                    label = "RUKO";
                    message = "Ruko dua lantai. Lantai bawahnya tempat usaha, lantai atas tempat tinggal pemiliknya.";
                    AddBuildingLights(x, y, 6.0f, 5.6f, false, 0.34f);
                    break;
                case DecorKind.Mushola:
                    Building("lc_mushola", x, y, 1f, 5.0f, 3.2f);
                    label = "MUSHOLA";
                    message = "Mushola warga. Terdengar suara mengaji dari dalam.";
                    AddLight(x, y + 0.5f, 1.6f, 1.3f, LampWarm, 0.5f);
                    AddLight(x, y + 4.0f, 2.6f, 2.2f, new UnityEngine.Color(0.6f, 1f, 0.75f), 0.35f);   // kubah berpendar
                    break;
                case DecorKind.Ruin:
                    Building("lc_workshop_ruin", x, y, 1f, 6.3f, 4.6f);
                    label = (plate ?? "Bangunan Tua").ToUpperInvariant();
                    message = "Bangunan ini sudah lama ditinggalkan. Pintunya berkarat dan terkunci.";
                    break;
                default:
                    string sprite = kind == DecorKind.HouseBlue ? "lc_house_blue"
                        : kind == DecorKind.HouseGreen ? "lc_house_green" : "lc_house_brown";
                    Building(sprite, x, y, 1f, 5.8f, 5.0f);
                    // Sebagian rumah warga lampunya menyala di malam hari (tidak semua, agar alami).
                    if (Mathf.Abs((int)(x * 13 + y * 7)) % 3 != 0) AddBuildingLights(x, y, 5.8f, 5.0f, false, 0.36f);
                    label = "RUMAH WARGA";
                    message = HouseLines[Mathf.Abs((int)(x * 7 + y * 3)) % HouseLines.Length];
                    break;
            }
            if (!string.IsNullOrEmpty(plate))
                WallPlate(plate, x, y, kind == DecorKind.Ruin ? 3.0f : 4.25f, 5.6f, PlateStyle.Toko);

            _decor[_buildRegion].Add(new DecorBuilding { Door = new Vector2(x, y - 1.0f), Label = label, Message = message });
        }
    }
}
