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

        enum DecorKind { HouseBlue, HouseGreen, HouseBrown, ClosedShop, Ruin }

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
                    Decor(DecorKind.ClosedShop, 59f, 49f, "Disewakan");
                    Decor(DecorKind.HouseGreen, 5f, 21f);
                    Decor(DecorKind.Ruin, 78.5f, 21f, "Bengkel Tua");
                    Decor(DecorKind.HouseBrown, 90.5f, 21f);
                    Decor(DecorKind.HouseGreen, 10f, 8f);
                    Decor(DecorKind.ClosedShop, 38f, 8f, "Tutup");
                    Decor(DecorKind.HouseBlue, 60f, 8f);
                    Decor(DecorKind.HouseBrown, 72f, 8f);
                    Decor(DecorKind.HouseGreen, 86f, 8f);
                    break;

                case Region.KampungCempaka:
                    Decor(DecorKind.HouseBrown, 14f, 50f);
                    Decor(DecorKind.HouseBlue, 38.5f, 46f);
                    Decor(DecorKind.ClosedShop, 53f, 44f, "Warung Tutup");
                    Decor(DecorKind.HouseGreen, 12f, 22f);
                    Decor(DecorKind.Ruin, 24f, 22f, "Bengkel Tua");
                    break;

                case Region.PasarRamaiJaya:
                    Decor(DecorKind.HouseBlue, 12f, 48f);
                    Decor(DecorKind.ClosedShop, 26f, 48f, "Disewakan");
                    Decor(DecorKind.HouseBrown, 40f, 48f);
                    Decor(DecorKind.ClosedShop, 54f, 48f, "Tutup");
                    Decor(DecorKind.HouseGreen, 68f, 48f);
                    Decor(DecorKind.Ruin, 82f, 48f, "Gudang Tua");
                    break;

                case Region.DesaSukaMakmur:
                    Decor(DecorKind.HouseGreen, 16f, 22f);
                    Decor(DecorKind.HouseBrown, 34f, 22f);
                    Decor(DecorKind.HouseBlue, 42f, 48f);
                    Decor(DecorKind.Ruin, 57f, 48f, "Lumbung Tua");
                    break;

                case Region.PantaiCemara:
                    Decor(DecorKind.ClosedShop, 56f, 16f, "Warung Tutup");
                    Decor(DecorKind.HouseBrown, 88f, 16f);
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
                case DecorKind.Ruin:
                    Building("lc_workshop_ruin", x, y, 1f, 6.3f, 4.6f);
                    label = (plate ?? "Bangunan Tua").ToUpperInvariant();
                    message = "Bangunan ini sudah lama ditinggalkan. Pintunya berkarat dan terkunci.";
                    break;
                default:
                    string sprite = kind == DecorKind.HouseBlue ? "lc_house_blue"
                        : kind == DecorKind.HouseGreen ? "lc_house_green" : "lc_house_brown";
                    Building(sprite, x, y, 1f, 5.8f, 5.0f);
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
