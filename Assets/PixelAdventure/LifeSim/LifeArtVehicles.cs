using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Kendaraan pribadi yang dinaiki pemain: sepeda, skuter matic, sepeda motor, mobil.
    /// Tiap kendaraan punya tiga sudut pandang (samping menghadap kanan, depan, belakang),
    /// pivot tengah bawah = kaki pemain. Sepeda/skuter/motor digambar di atas kaki pemain
    /// (pemain terlihat duduk di atasnya); mobil menggantikan sosok pemain (sopir di balik kaca).
    /// Juga ikon inventori 16x16 dan kepulan asap saat kendaraan dipanggil / disimpan.
    /// </summary>
    public static partial class LifeArt
    {
        static readonly Color32 VInk = PixelArt.Hex("241a2b");
        static readonly Color32 VRubber = PixelArt.Hex("2b2233");
        static readonly Color32 VRim = PixelArt.Hex("aab2b4");
        static readonly Color32 VChrome = PixelArt.Hex("c4c9cf");
        static readonly Color32 VSeat = PixelArt.Hex("3a2b2b");
        static readonly Color32 VLamp = PixelArt.Hex("ffe08a");
        static readonly Color32 VTail = PixelArt.Hex("e0383c");
        static readonly Color32 VGlass = PixelArt.Hex("9fd8e8");
        static readonly Color32 VSkin = PixelArt.Hex("f2c5a0");
        static readonly Color32 VHair = PixelArt.Hex("3a2a2a");
        static readonly Color32 VShadow = new Color32(18, 12, 24, 80);

        /// <summary>Id kendaraan yang punya gambar berkendara.</summary>
        public static readonly string[] RideIds = { "sepeda", "skuter", "motor", "mobil" };

        static void BuildVehicleArt()
        {
            BicycleArt();
            ScooterArt();
            MotorArt();
            CarArt();
            foreach (string id in RideIds)
                Add(Shrink(PixelArt.Get(Prefix + "ride_" + id + "_side"), 16, 16), new Vector2(0.5f, 0.5f), "item_vehicle_" + id);
            PuffArt();
        }

        // ------------------------------------------------------------------ util

        static void Wheel(PixelCanvas c, float cx, float cy, float r, bool spokes)
        {
            c.Disc(cx, cy, r, VRubber);
            c.Disc(cx, cy, r - 1.4f, spokes ? Clear : VRim);
            if (!spokes) c.Disc(cx, cy, r - 2.5f, PixelArt.Hex("6b737c"));   // velg
            if (spokes)
            {
                int x = Mathf.RoundToInt(cx), y = Mathf.RoundToInt(cy), k = Mathf.RoundToInt(r - 1f);
                c.Line(x - k, y, x + k, y, VRim);
                c.Line(x, y - k, x, y + k, VRim);
            }
            c.Disc(cx, cy, 1f, VInk);
        }

        static void NumberPlate(PixelCanvas c, int x, int y, int w)
        {
            c.Rect(x, y, w, 3, VInk);
            c.Rect(x + 1, y + 1, w - 2, 1, PixelArt.Hex("f4f5f8"));   // pelat nomor hitam bertulisan putih
        }

        /// <summary>Kecilkan sprite menjadi ikon (nearest neighbour, rasio terjaga, di tengah).</summary>
        static PixelCanvas Shrink(Sprite s, int w, int h)
        {
            var dst = new PixelCanvas(w, h);
            if (s == null) return dst;
            var tex = s.texture;
            Rect r = s.textureRect;
            int sw = (int)r.width, sh = (int)r.height;
            float scale = Mathf.Min((w - 1f) / sw, (h - 1f) / sh);
            int ow = Mathf.Max(1, Mathf.RoundToInt(sw * scale)), oh = Mathf.Max(1, Mathf.RoundToInt(sh * scale));
            int ox = (w - ow) / 2, oy = (h - oh) / 2;
            Color32[] px = tex.GetPixels32();
            for (int y = 0; y < oh; y++)
                for (int x = 0; x < ow; x++)
                {
                    int sx = (int)r.x + Mathf.Min(sw - 1, Mathf.FloorToInt((x + 0.5f) / scale));
                    int sy = (int)r.y + Mathf.Min(sh - 1, Mathf.FloorToInt((y + 0.5f) / scale));
                    Color32 col = px[sy * tex.width + sx];
                    if (col.a > 100) dst.Set(ox + x, oy + y, col);
                }
            return dst;
        }

        // ------------------------------------------------------------------ sepeda

        static void BicycleArt()
        {
            Color32 frame = PixelArt.Hex("3c9a5f"), wicker = PixelArt.Hex("c8a46a");

            var s = new PixelCanvas(32, 20);
            s.Ellipse(16f, 1.5f, 14f, 1.5f, VShadow);
            Wheel(s, 7f, 6f, 5.5f, true);
            Wheel(s, 25f, 6f, 5.5f, true);
            s.Line(7, 6, 13, 12, frame, 2);       // seat stay
            s.Line(13, 12, 15, 6, frame, 2);      // seat tube
            s.Line(7, 6, 15, 6, frame, 1);        // chain stay
            s.Line(13, 12, 22, 12, frame, 2);     // top tube
            s.Line(15, 6, 22, 12, frame, 2);      // down tube
            s.Line(25, 6, 22, 14, VChrome, 1);    // garpu depan
            s.Line(22, 14, 20, 16, VInk, 1);
            s.Rect(19, 16, 3, 1, VInk);           // setang
            Outlined(s, 10, 13, 6, 2, VSeat, VInk);
            s.Disc(15f, 6f, 1.6f, VChrome);       // gir
            s.Rect(14, 3, 3, 1, VInk);            // pedal
            Outlined(s, 23, 10, 6, 4, wicker, VInk);   // keranjang
            s.Rect(24, 12, 4, 1, PixelArt.Hex("8a6a44"));
            Add(s, BottomCenter, "ride_sepeda_side");

            var f = new PixelCanvas(16, 20);
            f.Ellipse(8f, 1.2f, 5f, 1.2f, VShadow);
            f.Rect(7, 0, 2, 11, VRubber);
            f.Rect(6, 9, 4, 2, VChrome);
            Outlined(f, 4, 10, 8, 5, wicker, VInk);
            f.Rect(5, 12, 6, 1, PixelArt.Hex("8a6a44"));
            f.Rect(7, 8, 2, 1, VLamp);
            f.Rect(1, 15, 14, 1, VInk);
            f.Rect(0, 15, 2, 2, VRubber); f.Rect(14, 15, 2, 2, VRubber);
            Add(f, BottomCenter, "ride_sepeda_front");

            var b = new PixelCanvas(16, 20);
            b.Ellipse(8f, 1.2f, 5f, 1.2f, VShadow);
            b.Rect(7, 0, 2, 10, VRubber);
            Outlined(b, 5, 8, 6, 3, frame, VInk);   // boncengan
            b.Rect(7, 6, 2, 1, VTail);              // reflektor
            b.Rect(0, 14, 3, 2, VRubber); b.Rect(13, 14, 3, 2, VRubber);
            Add(b, BottomCenter, "ride_sepeda_back");
        }

        // ------------------------------------------------------------------ skuter matic

        static void ScooterArt()
        {
            Color32 body = PixelArt.Hex("2fb7a8"), dark = PixelArt.Hex("1f8a7e");

            var s = new PixelCanvas(32, 20);
            s.Ellipse(16f, 1.5f, 14f, 1.5f, VShadow);
            Wheel(s, 7f, 4.5f, 4f, false);
            Wheel(s, 25f, 4.5f, 4f, false);
            Outlined(s, 3, 6, 13, 6, body, VInk);         // bodi belakang
            s.Rect(4, 7, 11, 1, dark);
            s.Rect(12, 5, 10, 2, PixelArt.Hex("6b737c"));  // dek kaki
            Outlined(s, 20, 5, 5, 11, body, VInk);        // tameng depan
            Outlined(s, 21, 8, 7, 3, body, VInk);         // spakbor depan
            s.Line(23, 15, 24, 18, VInk, 1);
            s.Rect(21, 18, 6, 1, VInk);                   // setang
            s.Rect(24, 13, 2, 2, VLamp);
            Outlined(s, 4, 11, 11, 3, VSeat, VInk);       // jok
            s.Rect(2, 9, 2, 2, VTail);
            Add(s, BottomCenter, "ride_skuter_side");

            var f = new PixelCanvas(16, 20);
            f.Ellipse(8f, 1.2f, 5f, 1.2f, VShadow);
            f.Rect(6, 0, 4, 8, VRubber);
            Outlined(f, 5, 6, 6, 3, body, VInk);
            Outlined(f, 4, 8, 8, 7, body, VInk);
            f.Rect(6, 12, 4, 2, VLamp);
            f.Rect(5, 9, 6, 1, dark);
            f.Rect(1, 15, 14, 2, VInk);
            f.Rect(1, 17, 1, 2, VInk); f.Rect(14, 17, 1, 2, VInk);
            f.Rect(0, 19, 3, 1, VChrome); f.Rect(13, 19, 3, 1, VChrome);
            Add(f, BottomCenter, "ride_skuter_front");

            var b = new PixelCanvas(16, 20);
            b.Ellipse(8f, 1.2f, 5f, 1.2f, VShadow);
            b.Rect(6, 0, 4, 7, VRubber);
            Outlined(b, 3, 5, 10, 6, body, VInk);
            b.Rect(5, 9, 6, 1, VTail);
            NumberPlate(b, 5, 5, 6);
            b.Rect(0, 14, 3, 2, VRubber); b.Rect(13, 14, 3, 2, VRubber);
            b.Rect(1, 16, 1, 2, VInk); b.Rect(14, 16, 1, 2, VInk);
            Add(b, BottomCenter, "ride_skuter_back");
        }

        // ------------------------------------------------------------------ sepeda motor

        static void MotorArt()
        {
            Color32 body = PixelArt.Hex("e04b42"), dark = PixelArt.Hex("a8322c");

            var s = new PixelCanvas(36, 22);
            s.Ellipse(18f, 1.5f, 16f, 1.5f, VShadow);
            Wheel(s, 8f, 5.5f, 5.5f, false);
            Wheel(s, 28f, 5.5f, 5.5f, false);
            Outlined(s, 14, 4, 8, 5, PixelArt.Hex("8a9098"), VInk);   // mesin
            s.Rect(3, 3, 13, 2, VChrome);                               // knalpot
            Outlined(s, 3, 9, 12, 5, body, VInk);                       // bodi belakang
            s.Rect(4, 10, 10, 1, dark);
            Outlined(s, 17, 10, 8, 5, body, VInk);                      // tangki
            s.Rect(18, 13, 5, 1, PixelArt.Hex("ff8a7a"));
            Outlined(s, 6, 13, 12, 3, VSeat, VInk);                     // jok
            s.Line(28, 6, 25, 16, VChrome, 2);                          // garpu
            Outlined(s, 25, 13, 6, 5, body, VInk);                      // kedok lampu
            s.Rect(29, 14, 2, 2, VLamp);
            s.Rect(22, 18, 6, 1, VInk);                                 // setang
            s.Rect(2, 11, 2, 2, VTail);
            Add(s, BottomCenter, "ride_motor_side");

            var f = new PixelCanvas(16, 22);
            f.Ellipse(8f, 1.2f, 5f, 1.2f, VShadow);
            f.Rect(6, 0, 4, 10, VRubber);
            f.Rect(5, 8, 1, 6, VChrome); f.Rect(10, 8, 1, 6, VChrome);
            Outlined(f, 5, 8, 6, 2, body, VInk);
            Outlined(f, 4, 11, 8, 6, body, VInk);
            f.Disc(8f, 14f, 2f, VLamp);
            f.Rect(0, 15, 16, 2, VInk);
            f.Rect(1, 17, 1, 2, VInk); f.Rect(14, 17, 1, 2, VInk);
            f.Rect(0, 19, 3, 1, VChrome); f.Rect(13, 19, 3, 1, VChrome);
            Add(f, BottomCenter, "ride_motor_front");

            var b = new PixelCanvas(16, 22);
            b.Ellipse(8f, 1.2f, 5f, 1.2f, VShadow);
            b.Rect(6, 0, 4, 9, VRubber);
            b.Rect(12, 3, 3, 2, VChrome);
            Outlined(b, 3, 7, 10, 5, body, VInk);
            b.Rect(6, 10, 4, 1, VTail);
            NumberPlate(b, 5, 4, 6);
            b.Rect(0, 15, 3, 2, VRubber); b.Rect(13, 15, 3, 2, VRubber);
            b.Rect(1, 17, 1, 2, VInk); b.Rect(14, 17, 1, 2, VInk);
            Add(b, BottomCenter, "ride_motor_back");
        }

        // ------------------------------------------------------------------ mobil

        static void CarArt()
        {
            Color32 body = PixelArt.Hex("3b6fb6"), light = PixelArt.Hex("5a8fd6"), dark = PixelArt.Hex("2a5290");

            var s = new PixelCanvas(52, 30);
            s.Ellipse(26f, 2f, 24f, 2f, VShadow);
            Outlined(s, 12, 13, 27, 12, body, VInk);          // kabin
            s.Rect(14, 15, 10, 8, VGlass);                    // kaca belakang
            s.Rect(26, 15, 11, 8, VGlass);                    // kaca depan (sopir)
            s.Rect(24, 14, 2, 10, dark);
            s.Rect(15, 21, 3, 1, PixelArt.Hex("d8f4ff"));
            s.Disc(31f, 18.5f, 2.6f, VSkin);                  // sopir
            s.Rect(29, 19, 5, 3, VHair);
            s.Rect(33, 17, 1, 1, VInk);
            Outlined(s, 2, 4, 48, 11, body, VInk);            // bodi bawah
            s.Rect(3, 12, 46, 1, light);
            s.Rect(25, 5, 1, 9, dark);                        // garis pintu
            s.Rect(28, 10, 3, 1, VInk);                       // gagang pintu
            s.Rect(48, 9, 2, 3, VLamp);
            s.Rect(2, 9, 2, 3, VTail);
            s.Rect(1, 4, 50, 2, PixelArt.Hex("6b737c"));      // bemper
            Wheel(s, 12f, 5f, 4.5f, false);
            Wheel(s, 40f, 5f, 4.5f, false);
            Add(s, BottomCenter, "ride_mobil_side");

            var f = new PixelCanvas(30, 30);
            f.Ellipse(15f, 1.5f, 14f, 1.5f, VShadow);
            f.Rect(2, 0, 4, 4, VRubber); f.Rect(24, 0, 4, 4, VRubber);
            Outlined(f, 1, 3, 28, 12, body, VInk);
            f.Rect(2, 12, 26, 1, light);
            Outlined(f, 4, 14, 22, 10, VGlass, VInk);          // kaca depan
            f.Disc(10f, 18.5f, 2.6f, VSkin);                   // sopir di kanan (setir kanan)
            f.Rect(8, 20, 5, 2, VHair);
            f.Rect(9, 18, 1, 1, VInk); f.Rect(11, 18, 1, 1, VInk);
            f.Rect(16, 15, 6, 1, PixelArt.Hex("d8f4ff"));
            Outlined(f, 5, 23, 20, 4, body, VInk);             // atap
            f.Rect(3, 9, 5, 3, VLamp); f.Rect(22, 9, 5, 3, VLamp);
            f.Rect(10, 7, 10, 3, VInk);                         // gril
            f.Rect(1, 3, 28, 2, PixelArt.Hex("6b737c"));
            NumberPlate(f, 11, 3, 8);
            f.Rect(0, 16, 2, 2, body); f.Rect(28, 16, 2, 2, body);   // spion
            Add(f, BottomCenter, "ride_mobil_front");

            var b = new PixelCanvas(30, 30);
            b.Ellipse(15f, 1.5f, 14f, 1.5f, VShadow);
            b.Rect(2, 0, 4, 4, VRubber); b.Rect(24, 0, 4, 4, VRubber);
            Outlined(b, 1, 3, 28, 12, body, VInk);
            b.Rect(2, 12, 26, 1, light);
            Outlined(b, 4, 14, 22, 10, VGlass, VInk);          // kaca belakang
            b.Disc(20f, 18.5f, 2.6f, VHair);                   // kepala sopir dari belakang
            Outlined(b, 5, 23, 20, 4, body, VInk);
            b.Rect(3, 9, 5, 3, VTail); b.Rect(22, 9, 5, 3, VTail);
            b.Rect(1, 3, 28, 2, PixelArt.Hex("6b737c"));
            NumberPlate(b, 11, 6, 8);
            b.Rect(0, 16, 2, 2, body); b.Rect(28, 16, 2, 2, body);
            Add(b, BottomCenter, "ride_mobil_back");
        }

        // ------------------------------------------------------------------ asap

        static void PuffArt()
        {
            for (int i = 0; i < 3; i++)
            {
                var c = new PixelCanvas(24, 16);
                float r = 3f + i * 1.6f;
                byte a = (byte)(220 - i * 60);
                Color32 smoke = new Color32(236, 232, 240, a), shade = new Color32(190, 184, 200, a);
                c.Disc(7f, 6f, r, shade); c.Disc(17f, 6f, r, shade); c.Disc(12f, 9f, r + 0.6f, shade);
                c.Disc(7f, 7f, r - 1f, smoke); c.Disc(17f, 7f, r - 1f, smoke); c.Disc(12f, 10f, r - 0.4f, smoke);
                Add(c, new Vector2(0.5f, 0.2f), "puff_" + i);
            }
        }
    }
}
