using PixelAdventure.LifeSim.Pulang;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Jalan raya setiap wilayah: rambu-rambu, lampu lalu lintas (Kota), kendaraan, dan
    /// pejalan kaki. Koordinat mengikuti jaringan jalan di LifeArtTown:
    /// - Kota   : jalan utama y 30–38 (trotoar 28–30 & 38–40), simpang x 44–52, zebra x 42–43.6 / 52.4–54
    /// - Pasar  : jalan y 30–34 (trotoar 27–30 & 34–37)
    /// - Pantai : jalan y 10–14
    /// - Desa / Kampung : jalan tanah y 30–33
    /// Lajur kiri: ke timur di bagian utara jalan, ke barat di bagian selatan.
    /// </summary>
    public partial class LifeWorld
    {
        const string Moto = "lc_vehicle_motorcycle";
        const string Car = "lc_vehicle_car";
        const string CarBlue = "lc_vehicle_car_blue";
        const string CarWhite = "lc_vehicle_car_white";
        const string Taxi = "lc_vehicle_taxi";
        const string Angkot = "lc_vehicle_angkot";
        const string Bus = "lc_vehicle_bus";

        void BuildStreet(Region r, Transform root)
        {
            var t = LifeTraffic.Attach(root);
            switch (r)
            {
                case Region.KotaJogokerto:
                {
                    int w1 = t.AddLane(30.5f, -1), w2 = t.AddLane(32.3f, -1);
                    int e1 = t.AddLane(34.3f, 1), e2 = t.AddLane(36.1f, 1);
                    Vehicle(t, w1, Moto, 5.6f, 20f); Vehicle(t, w1, CarBlue, 4.6f, 62f);
                    Vehicle(t, w2, Bus, 3.3f, 80f); Vehicle(t, w2, Angkot, 4.0f, 28f);
                    Vehicle(t, e1, Car, 4.6f, 8f); Vehicle(t, e1, Taxi, 4.8f, 56f);
                    Vehicle(t, e2, Moto, 5.8f, 34f); Vehicle(t, e2, CarWhite, 4.4f, 76f);

                    // Lampu lalu lintas di empat sudut simpang; garis henti tepat sebelum zebra.
                    t.SetStopLines(41.7f, 54.3f);
                    foreach (var p in new[] { new Vector2(41.9f, 40.3f), new Vector2(54.1f, 40.3f),
                                              new Vector2(41.9f, 27.0f), new Vector2(54.1f, 27.0f) })
                        t.AddTrafficLight(Sign("lc_traffic_light_green", p.x, p.y));

                    // Rambu: arah timur di sisi utara, arah barat di sisi selatan.
                    Sign("lc_sign_speed", 3f, 40.4f);          // batas 40 masuk kota dari barat
                    Sign("lc_sign_no_parking", 26f, 40.4f);    // di antara Toko Serba Ada & Minimarket
                    Sign("lc_sign_crossing", 55.6f, 40.4f);    // zebra timur
                    Sign("lc_sign_crossing", 40.4f, 27.3f);    // zebra barat
                    Sign("lc_sign_warning", 33f, 27.3f);       // hati-hati, ada sekolah di selatan
                    Sign("lc_sign_speed", 93f, 27.3f);         // batas 40 dari timur

                    // Pejalan kaki di trotoar utara/selatan dan trotoar jalan simpang.
                    t.AddWalker("warga1", new Vector2(3f, 39.0f), new Vector2(93f, 39.0f), 1.3f, 0.2f);
                    t.AddWalker("warga2", new Vector2(3f, 39.1f), new Vector2(93f, 39.1f), 1.1f, 0.75f);
                    t.AddWalker("warga3", new Vector2(3f, 29.0f), new Vector2(93f, 29.0f), 1.2f, 0.4f);
                    t.AddWalker("warga4", new Vector2(3f, 28.9f), new Vector2(93f, 28.9f), 1.4f, 0.9f);
                    t.AddWalker("warga5", new Vector2(43.1f, 41.2f), new Vector2(43.1f, 69f), 1.1f, 0.3f);
                    t.AddWalker("warga6", new Vector2(52.9f, 1.5f), new Vector2(52.9f, 26.8f), 1.2f, 0.6f);
                    break;
                }

                case Region.PasarRamaiJaya:
                {
                    int w = t.AddLane(30.3f, -1), e = t.AddLane(32.1f, 1);
                    Vehicle(t, w, Angkot, 3.8f, 20f); Vehicle(t, w, Moto, 5.2f, 70f);
                    Vehicle(t, e, Car, 4.2f, 40f); Vehicle(t, e, Moto, 5.4f, 85f);

                    Sign("lc_sign_speed", 24f, 37.4f);
                    Sign("lc_sign_crossing", 56f, 37.4f);
                    Sign("lc_sign_warning", 40f, 27.3f);       // pasar ramai, pelan-pelan
                    Sign("lc_sign_no_parking", 74f, 27.3f);
                    Sign("lc_sign_no_entry", 9f, 27.3f);       // masuk area los pasar

                    t.AddWalker("warga2", new Vector2(8f, 35.6f), new Vector2(94f, 35.6f), 1.0f, 0.3f);
                    t.AddWalker("warga5", new Vector2(8f, 35.5f), new Vector2(94f, 35.5f), 1.2f, 0.8f);
                    t.AddWalker("warga3", new Vector2(4f, 28.7f), new Vector2(94f, 28.7f), 1.1f, 0.5f);
                    break;
                }

                case Region.PantaiCemara:
                {
                    int w = t.AddLane(10.3f, -1), e = t.AddLane(12.1f, 1);
                    Vehicle(t, w, CarWhite, 4.4f, 30f);
                    Vehicle(t, e, Moto, 5.4f, 70f); Vehicle(t, e, Angkot, 3.9f, 15f);

                    Sign("lc_sign_speed", 16f, 9.2f);
                    Sign("lc_sign_no_parking", 40f, 9.2f);
                    Sign("lc_sign_warning", 60f, 15.6f);       // hati-hati, banyak pejalan kaki

                    t.AddWalker("warga6", new Vector2(9f, 14.9f), new Vector2(94f, 14.9f), 1.1f, 0.4f);
                    t.AddWalker("warga4", new Vector2(10f, 33f), new Vector2(90f, 33f), 0.9f, 0.6f);
                    break;
                }

                case Region.DesaSukaMakmur:
                {
                    int w = t.AddLane(30.2f, -1), e = t.AddLane(31.7f, 1);
                    Vehicle(t, w, Moto, 4.6f, 60f);
                    Vehicle(t, e, Moto, 4.8f, 15f);

                    Sign("lc_sign_warning", 34f, 34.3f);
                    Sign("lc_sign_speed", 58f, 29.0f);

                    t.AddWalker("warga3", new Vector2(3f, 33.6f), new Vector2(84f, 33.6f), 0.9f, 0.3f);
                    t.AddWalker("warga5", new Vector2(55f, 29.5f), new Vector2(94f, 29.5f), 0.8f, 0.7f);
                    break;
                }

                case Region.KampungCempaka:
                {
                    int w = t.AddLane(30.2f, -1), e = t.AddLane(31.7f, 1);
                    Vehicle(t, w, Moto, 4.6f, 70f); Vehicle(t, w, Angkot, 3.6f, 25f);
                    Vehicle(t, e, Moto, 4.8f, 40f);

                    Sign("lc_sign_warning", 51f, 34.4f);       // hati-hati, banyak anak-anak
                    Sign("lc_sign_speed", 27.5f, 29.0f);

                    t.AddWalker("warga1", new Vector2(8f, 33.6f), new Vector2(28f, 33.6f), 0.9f, 0.2f);
                    t.AddWalker("warga6", new Vector2(50f, 33.6f), new Vector2(94f, 33.6f), 1.0f, 0.5f);
                    t.AddWalker("warga2", new Vector2(31f, 36f), new Vector2(31f, 60f), 0.8f, 0.4f);
                    break;
                }
            }
        }

        static void Vehicle(LifeTraffic t, int lane, string sprite, float speed, float x) =>
            t.AddVehicle(lane, sprite, sprite == Moto, speed, x);

        /// <summary>Rambu/lampu bertiang: digambar menurut kedalaman, tiangnya jadi penghalang kecil.</summary>
        SpriteRenderer Sign(string sprite, float x, float y)
        {
            var sr = Make(sprite, x, y, sprite, Order(y));
            AddObstacle(x - 0.14f, y, 0.28f, 0.22f);
            return sr;
        }
    }
}
