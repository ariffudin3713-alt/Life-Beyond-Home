using System.Collections.Generic;
using PixelAdventure.LifeSim.Pulang;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>Titik interaksi di dalam ruangan (kasir, kasur, pintu keluar, ...).</summary>
    public struct InteriorSpot
    {
        public Vector2 Pos;
        public string Label;
        public bool Exit;
    }

    /// <summary>
    /// Ruangan dalam bangunan. Saat masuk, wilayah disembunyikan dan sebuah ruangan dibangun
    /// di tengah peta (lantai, dinding, perabot, penjaga). Dinding & perabot menjadi penghalang,
    /// jadi CanStand tetap berlaku. Saat keluar, ruangan dihancurkan dan wilayah dipulihkan.
    /// </summary>
    public partial class LifeWorld
    {
        readonly Dictionary<LocationId, Kind> _placeKinds = new Dictionary<LocationId, Kind>();
        readonly List<InteriorSpot> _interiorSpots = new List<InteriorSpot>();
        Transform _interiorRoot;

        public bool InInterior => _interiorRoot != null;
        public LocationId InteriorLocation { get; private set; }
        public IReadOnlyList<InteriorSpot> InteriorSpots => _interiorSpots;

        /// <summary>Tengah ruangan (untuk kamera).</summary>
        public Vector2 InteriorCenter { get; private set; }

        /// <summary>Area keluar (keset pintu) di dalam ruangan.</summary>
        public Rect InteriorExitZone { get; private set; }

        /// <summary>Bangunan yang bisa dimasuki dan jenis ruangannya.</summary>
        public bool TryInteriorKind(LocationId id, out InteriorKind kind)
        {
            kind = InteriorKind.Office;
            if (!_placeKinds.TryGetValue(id, out Kind k)) return false;
            switch (k)
            {
                case Kind.House:
                    // Hanya rumah sendiri / kost yang bisa dimasuki.
                    if (id != LocationId.RumahZidan && id != LocationId.KostPelangi) return false;
                    kind = InteriorKind.Home; return true;
                case Kind.Shop: kind = InteriorKind.Shop; return true;
                case Kind.Cafe: kind = InteriorKind.Cafe; return true;
                case Kind.Inn: kind = InteriorKind.Inn; return true;
                case Kind.Tower: kind = InteriorKind.Office; return true;
                case Kind.Facade:
                    kind = id == LocationId.Perpustakaan ? InteriorKind.Library
                        : id == LocationId.BengkelJayaMotor ? InteriorKind.Workshop
                        : InteriorKind.Office;
                    return true;
                default: return false;
            }
        }

        public bool IsEnterable(LocationId id) => TryInteriorKind(id, out _);

        // ------------------------------------------------------------------ masuk / keluar

        /// <summary>Bangun ruangan dan kembalikan posisi pemain (di dalam, dekat pintu).</summary>
        public Vector2 EnterInterior(LocationId id)
        {
            if (InInterior) ExitInteriorRoom();
            TryInteriorKind(id, out InteriorKind kind);
            InteriorLocation = id;

            int w = kind == InteriorKind.Home ? 12 : kind == InteriorKind.Shop || kind == InteriorKind.Cafe ? 16 : 14;
            int h = kind == InteriorKind.Home ? 8 : kind == InteriorKind.Shop || kind == InteriorKind.Cafe ? 10 : 9;
            float L = Mathf.Round(Width * 0.5f - w * 0.5f);
            float B = Mathf.Round(Height * 0.5f - h * 0.5f);
            float doorX = L + w * 0.5f;
            InteriorCenter = new Vector2(doorX, B + (h + LifeArt.RoomWallH) * 0.5f - 0.6f);
            InteriorExitZone = new Rect(doorX - 1.2f, B - 1f, 2.4f, 1.75f);

            // Sembunyikan wilayah, bangun ruangan.
            _roots[CurrentRegion].gameObject.SetActive(false);
            _interiorRoot = new GameObject("Interior_" + id).transform;
            _interiorRoot.SetParent(transform, false);
            Obstacles.Clear();
            _interiorSpots.Clear();

            var floor = Furn(LifeArt.RoomSprite(kind, w, h), L, B, -3500);
            floor.name = "Lantai";

            // Dinding ruangan sebagai penghalang (pintu di tengah bawah).
            Obstacles.Add(new Rect(L - 2f, B - 3f, 2.5f, h + 6f));            // kiri
            Obstacles.Add(new Rect(L + w - 0.5f, B - 3f, 2.5f, h + 6f));      // kanan
            Obstacles.Add(new Rect(L - 2f, B + h - 0.15f, w + 4f, 4f));       // belakang
            Obstacles.Add(new Rect(L - 2f, B - 3f, doorX - 1.25f - (L - 2f), 3.15f));   // bawah kiri
            Obstacles.Add(new Rect(doorX + 1.25f, B - 3f, L + w + 2f - (doorX + 1.25f), 3.15f)); // bawah kanan
            Obstacles.Add(new Rect(L - 2f, B - 4f, w + 4f, 3.2f));            // di luar pintu

            Flat("lc_int_doormat", doorX, B + 0.15f);
            _interiorSpots.Add(new InteriorSpot { Pos = new Vector2(doorX, B + 0.9f), Label = "KELUAR", Exit = true });

            BuildRoom(kind, id, L, B, w, h);
            return new Vector2(doorX, B + 1.6f);
        }

        /// <summary>Hancurkan ruangan dan tampilkan lagi wilayah (posisi pemain diatur pemanggil).</summary>
        public void ExitInteriorRoom()
        {
            if (_interiorRoot != null) Destroy(_interiorRoot.gameObject);
            _interiorRoot = null;
            _interiorSpots.Clear();
            SetRegion(CurrentRegion);
        }

        // ------------------------------------------------------------------ isi ruangan

        void BuildRoom(InteriorKind kind, LocationId id, float L, float B, int w, int h)
        {
            string staff = "lc_warga" + (1 + ((int)id % 6)) + "_down_0";
            float wallY = B + h + 0.55f;

            switch (kind)
            {
                case InteriorKind.Home:
                    Flat("lc_int_rug", L + 6f, B + 1.6f);
                    Item("lc_int_bed", L + 2.4f, B + 4.3f, 2f, 2.4f);
                    Item("lc_int_table", L + 7f, B + 3.4f, 2.1f, 0.9f);
                    Item("lc_int_chair", L + 5.6f, B + 3.3f, 0.6f, 0.4f);
                    Item("lc_int_chair", L + 8.4f, B + 3.3f, 0.6f, 0.4f);
                    Item("lc_int_bookshelf", L + 10.2f, B + 5.6f, 3f, 0.8f);
                    Item("lc_int_plant", L + 11.1f, B + 0.8f, 0.6f, 0.4f);
                    Wall("lc_int_window", L + 3.5f, wallY); Wall("lc_int_window", L + 8.5f, wallY);
                    Wall("lc_int_painting", L + 6f, wallY + 0.4f);
                    Spot(L + 2.4f, B + 3.7f, "KASUR");
                    break;

                case InteriorKind.Shop:
                    Item("lc_int_shelf", L + 3.5f, B + 6.2f, 3f, 0.8f);
                    Item("lc_int_shelf", L + 8f, B + 6.2f, 3f, 0.8f);
                    Item("lc_int_shelf", L + 3.5f, B + 3.4f, 3f, 0.8f);
                    Item("lc_int_shelf", L + 8f, B + 3.4f, 3f, 0.8f);
                    Item("lc_int_fridge", L + 13.2f, B + 7.4f, 1.4f, 0.7f);
                    Item("lc_int_fridge", L + 14.6f, B + 7.4f, 1.4f, 0.7f);
                    Item(staff, L + 13.2f, B + 3.7f, 0.6f, 0.4f);
                    Item("lc_int_counter", L + 12.6f, B + 2.6f, 4f, 0.9f);
                    Item("lc_int_plant", L + 1f, B + 0.8f, 0.6f, 0.4f);
                    Item("lc_int_plant", L + 15f, B + 0.8f, 0.6f, 0.4f);
                    Wall("lc_int_window", L + 5f, wallY); Wall("lc_int_window", L + 11f, wallY);
                    Spot(L + 12.6f, B + 1.9f, "KASIR");
                    break;

                case InteriorKind.Cafe:
                    Item(staff, L + 4.4f, B + 7.7f, 0.6f, 0.4f);
                    Item("lc_int_counter", L + 4f, B + 6.6f, 4f, 0.9f);
                    Item("lc_int_fridge", L + 7.2f, B + 8.2f, 1.4f, 0.7f);
                    Item("lc_int_sofa", L + 13.4f, B + 7.6f, 2.7f, 0.8f);
                    foreach (var t in new[] { new Vector2(9f, 4.4f), new Vector2(13f, 4.4f), new Vector2(9f, 1.6f), new Vector2(13f, 1.6f) })
                    {
                        Item("lc_int_table", L + t.x, B + t.y, 2.1f, 0.9f);
                        Item("lc_int_chair", L + t.x - 1.4f, B + t.y - 0.1f, 0.6f, 0.4f);
                        Item("lc_int_chair", L + t.x + 1.4f, B + t.y - 0.1f, 0.6f, 0.4f);
                    }
                    Item("lc_int_plant", L + 1f, B + 0.8f, 0.6f, 0.4f);
                    Wall("lc_int_painting", L + 9f, wallY + 0.4f); Wall("lc_int_painting", L + 13f, wallY + 0.4f);
                    Wall("lc_int_window", L + 11f, wallY);
                    Spot(L + 4f, B + 5.9f, "PESAN DI KASIR");
                    break;

                case InteriorKind.Inn:
                    Flat("lc_int_rug", L + 7f, B + 2f);
                    Item(staff, L + 7.4f, B + 6.5f, 0.6f, 0.4f);
                    Item("lc_int_counter", L + 7f, B + 5.4f, 4f, 0.9f);
                    Item("lc_int_bookshelf", L + 2.2f, B + 6.6f, 3f, 0.8f);
                    Item("lc_int_plant", L + 12.6f, B + 6.8f, 0.6f, 0.4f);
                    Item("lc_int_sofa", L + 2.6f, B + 1.6f, 2.7f, 0.8f);
                    Item("lc_int_sofa", L + 11.4f, B + 1.6f, 2.7f, 0.8f);
                    Wall("lc_int_painting", L + 7f, wallY + 0.4f);
                    Wall("lc_int_window", L + 3f, wallY); Wall("lc_int_window", L + 11f, wallY);
                    Spot(L + 7f, B + 4.7f, "RESEPSIONIS");
                    break;

                case InteriorKind.Library:
                    Item("lc_int_bookshelf", L + 2.8f, B + 6.4f, 3f, 0.8f);
                    Item("lc_int_bookshelf", L + 6.6f, B + 6.4f, 3f, 0.8f);
                    Item("lc_int_bookshelf", L + 2.8f, B + 3.6f, 3f, 0.8f);
                    Item("lc_int_bookshelf", L + 6.6f, B + 3.6f, 3f, 0.8f);
                    Item("lc_int_table", L + 11f, B + 6.4f, 2.1f, 0.9f);
                    Item(staff, L + 11.5f, B + 4.1f, 0.6f, 0.4f);
                    Item("lc_int_counter", L + 11f, B + 3f, 4f, 0.9f);
                    Item("lc_int_plant", L + 13f, B + 0.8f, 0.6f, 0.4f);
                    Wall("lc_int_window", L + 11f, wallY);
                    Spot(L + 11f, B + 2.3f, "PUSTAKAWAN");
                    break;

                case InteriorKind.Workshop:
                    Item("lc_vehicle_motorcycle", L + 3.5f, B + 5f, 2.6f, 0.6f);
                    Item("lc_vehicle_motorcycle", L + 7f, B + 5f, 2.6f, 0.6f);
                    Item("lc_vehicle_car", L + 3.6f, B + 1.8f, 3.6f, 0.6f);
                    Item("lc_int_shelf", L + 11.8f, B + 6.4f, 3f, 0.8f);
                    Item(staff, L + 11f, B + 3.7f, 0.6f, 0.4f);
                    Item("lc_int_counter", L + 10.5f, B + 2.6f, 4f, 0.9f);
                    Wall("lc_int_window", L + 7f, wallY);
                    Spot(L + 10.5f, B + 1.9f, "MONTIR");
                    break;

                default: // kantor / balai / rumah sakit / sekolah
                    Item(staff.Replace("_down_0", "_down_0"), L + 3f, B + 6.9f, 0.6f, 0.4f);
                    Item("lc_int_desk", L + 3f, B + 5.8f, 2.5f, 0.8f);
                    Item("lc_int_desk", L + 6.5f, B + 5.8f, 2.5f, 0.8f);
                    Item("lc_int_bookshelf", L + 12f, B + 6.6f, 3f, 0.8f);
                    Item(staff, L + 11f, B + 4.1f, 0.6f, 0.4f);
                    Item("lc_int_counter", L + 10.5f, B + 3f, 4f, 0.9f);
                    Item("lc_int_plant", L + 1f, B + 0.8f, 0.6f, 0.4f);
                    Item("lc_int_plant", L + 13f, B + 0.8f, 0.6f, 0.4f);
                    Wall("lc_int_window", L + 3.5f, wallY); Wall("lc_int_window", L + 7f, wallY);
                    Spot(L + 10.5f, B + 2.3f, "MEJA LAYANAN");
                    break;
            }
        }

        // ------------------------------------------------------------------ util

        SpriteRenderer Furn(Sprite sprite, float x, float y, int order)
        {
            var go = new GameObject("Perabot");
            go.transform.SetParent(_interiorRoot, false);
            go.transform.position = new Vector3(x, y, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sharedMaterial = PixelArt.SpriteMaterial;
            sr.sortingOrder = order;
            return sr;
        }

        /// <summary>Perabot berdiri: digambar menurut kedalaman, alasnya penghalang.</summary>
        void Item(string sprite, float x, float y, float footW, float footH)
        {
            Furn(PixelArt.Get(sprite), x, y, Order(y));
            Obstacles.Add(new Rect(x - footW * 0.5f, y, footW, footH));
        }

        /// <summary>Benda rata di lantai (karpet, keset): di bawah semua, bukan penghalang.</summary>
        void Flat(string sprite, float x, float y) => Furn(PixelArt.Get(sprite), x, y, -3400);

        /// <summary>Hiasan di dinding belakang.</summary>
        void Wall(string sprite, float x, float y) => Furn(PixelArt.Get(sprite), x, y, -3450);

        void Spot(float x, float y, string label) =>
            _interiorSpots.Add(new InteriorSpot { Pos = new Vector2(x, y), Label = label });
    }
}
