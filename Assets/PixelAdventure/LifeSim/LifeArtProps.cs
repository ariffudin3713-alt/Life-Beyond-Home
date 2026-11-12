using UnityEngine;

namespace PixelAdventure.LifeSim
{
    public static partial class LifeArt
    {
        // ------------------------------------------------------------------ props

        static void BuildProps()
        {
            BuildFountain();
            BuildBench();
            BuildLamp();
            BuildSign();
            BuildTree();
            BuildBush();
            BuildFlowers();
            BuildFarmGround();
            BuildOldTownFacade();
            BuildPersonalVehicles();
            BuildOfficeTower();
            BuildPalm();
            BuildBeachUmbrella();
            BuildMarketStall();
            BuildGazebo();
            BuildBridge();
            BuildEasel();
            BuildBusStop();
            BuildIcons();
        }

        static void BuildPalm()
        {
            var c = new PixelCanvas(72, 112);
            Color32 trunkDark = PixelArt.Hex("69472f");
            Color32 trunk = PixelArt.Hex("a77a48");
            Color32 leafDark = PixelArt.Hex("28633e");
            Color32 leaf = PixelArt.Hex("39894b");
            Color32 leafLight = PixelArt.Hex("72bf5b");
            c.Ellipse(36f, 5f, 31f, 5f, new Color32(18, 12, 24, 85));
            c.Line(35, 6, 39, 54, trunkDark, 9);
            c.Line(36, 7, 39, 54, trunk, 5);
            for (int y = 15; y < 51; y += 10)
                c.Rect(34, y, 10, 2, trunkDark);
            c.Disc(39f, 57f, 7f, trunkDark);
            c.Disc(39f, 58f, 4f, trunk);

            Vector2[][] fronds =
            {
                new[] { new Vector2(39f, 58f), new Vector2(4f, 91f), new Vector2(26f, 65f) },
                new[] { new Vector2(39f, 58f), new Vector2(13f, 105f), new Vector2(31f, 67f) },
                new[] { new Vector2(39f, 58f), new Vector2(37f, 110f), new Vector2(42f, 68f) },
                new[] { new Vector2(39f, 58f), new Vector2(66f, 100f), new Vector2(46f, 66f) },
                new[] { new Vector2(39f, 58f), new Vector2(71f, 76f), new Vector2(49f, 60f) },
                new[] { new Vector2(39f, 58f), new Vector2(7f, 72f), new Vector2(29f, 60f) }
            };
            for (int i = 0; i < fronds.Length; i++)
            {
                Vector2 a = fronds[i][0], b = fronds[i][1], mid = fronds[i][2];
                c.Line((int)a.x, (int)a.y, (int)b.x, (int)b.y, leafDark, 5);
                c.Line((int)a.x, (int)a.y, (int)b.x, (int)b.y, i % 2 == 0 ? leaf : leafLight, 3);
                for (int t = 1; t <= 4; t++)
                {
                    float blend = t / 5f;
                    int x = Mathf.RoundToInt(Mathf.Lerp(a.x, b.x, blend));
                    int y = Mathf.RoundToInt(Mathf.Lerp(a.y, b.y, blend));
                    int spread = 3 + t;
                    c.Line(x, y, x - spread, y + spread, leafDark, 2);
                    c.Line(x, y, x + spread, y + spread, leaf, 2);
                }
            }
            Add(c, new Vector2(0.5f, 0f), "palm");
        }

        static void BuildBeachUmbrella()
        {
            var c = new PixelCanvas(64, 52);
            Color32 outline = PixelArt.Hex("3b2d37");
            Color32 pole = PixelArt.Hex("eee1bf");
            Color32 red = PixelArt.Hex("e75b57");
            Color32 cream = PixelArt.Hex("fff1d6");
            c.Ellipse(32f, 5f, 28f, 5f, new Color32(18, 12, 24, 80));
            c.Line(32, 4, 32, 41, outline, 4);
            c.Line(32, 4, 32, 41, pole, 2);
            c.Ellipse(32f, 33f, 29f, 16f, outline);
            c.Ellipse(32f, 35f, 27f, 13f, cream);
            c.Triangle(new Vector2(5f, 36f), new Vector2(32f, 49f),
                new Vector2(32f, 35f), red);
            c.Triangle(new Vector2(32f, 49f), new Vector2(59f, 36f),
                new Vector2(32f, 35f), cream);
            c.Triangle(new Vector2(5f, 36f), new Vector2(32f, 35f),
                new Vector2(32f, 22f), cream);
            c.Triangle(new Vector2(32f, 35f), new Vector2(59f, 36f),
                new Vector2(32f, 22f), red);
            c.Line(5, 36, 32, 22, outline, 2);
            c.Line(59, 36, 32, 22, outline, 2);
            c.Set(30, 40, PixelArt.Hex("fff7cf"));
            Add(c, new Vector2(0.5f, 0f), "beach_umbrella");
        }

        static void BuildMarketStall()
        {
            var c = new PixelCanvas(104, 76);
            Color32 outline = PixelArt.Hex("3a2a31");
            Color32 wood = PixelArt.Hex("8a5a3c");
            Color32 woodLight = PixelArt.Hex("bd8755");
            Color32[] awning =
            {
                PixelArt.Hex("e75b57"), PixelArt.Hex("fff1d6"),
                PixelArt.Hex("4e9a65"), PixelArt.Hex("f3c54a")
            };
            c.Ellipse(52f, 5f, 48f, 5f, new Color32(18, 12, 24, 85));
            Outlined(c, 6, 7, 92, 36, wood, outline);
            c.Rect(10, 11, 84, 25, woodLight);
            c.Rect(4, 39, 96, 6, outline);
            c.Rect(7, 41, 90, 3, wood);
            for (int x = 3, i = 0; x < 101; x += 14, i++)
            {
                c.Rect(x, 52, 14, 13, awning[i % awning.Length]);
                c.Triangle(new Vector2(x, 52f), new Vector2(x + 14f, 52f),
                    new Vector2(x + 7f, 45f), awning[i % awning.Length]);
                c.Rect(x, 49, 14, 3, outline);
            }
            c.Rect(8, 65, 88, 3, outline);
            c.Set(21, 29, PixelArt.Hex("e75b57"));
            c.Set(23, 30, PixelArt.Hex("f3c54a"));
            c.Set(25, 29, PixelArt.Hex("579b4d"));
            c.Set(75, 28, PixelArt.Hex("e75b57"));
            c.Set(77, 29, PixelArt.Hex("f3c54a"));
            Add(c, new Vector2(0.5f, 0f), "market_stall");
        }

        static void BuildOldTownFacade()
        {
            var c = new PixelCanvas(104, 104);
            Color32 outline = PixelArt.Hex("49342d");
            Color32 plaster = PixelArt.Hex("ead9b4");
            Color32 plasterDark = PixelArt.Hex("c6b18c");
            Color32 roof = PixelArt.Hex("a84f3c");
            Color32 roofLight = PixelArt.Hex("d27755");
            Color32 wood = PixelArt.Hex("72503b");
            Color32 glass = PixelArt.Hex("6da8b8");

            c.Ellipse(52f, 5f, 49f, 5f, new Color32(18, 12, 24, 90));
            Outlined(c, 5, 4, 94, 67, plaster, outline);
            c.Rect(10, 9, 84, 57, plaster);
            c.Rect(10, 9, 84, 5, plasterDark);
            for (int x = 13; x < 94; x += 10)
                c.Rect(x, 15, 2, 48, plasterDark);

            // Dutch colonial stepped gable with red roof tiles.
            c.Rect(3, 68, 98, 4, outline);
            c.Rect(7, 72, 90, 3, roof);
            c.Rect(12, 75, 80, 3, roofLight);
            c.Rect(17, 78, 70, 3, roof);
            c.Rect(23, 81, 58, 3, roofLight);
            c.Rect(29, 84, 46, 3, roof);
            c.Rect(35, 87, 34, 3, roofLight);
            c.Rect(41, 90, 22, 4, roof);
            c.Rect(46, 94, 12, 4, roofLight);
            c.Rect(48, 98, 8, 3, outline);

            for (int x = 17; x < 91; x += 24)
            {
                Outlined(c, x, 32, 15, 26, glass, wood);
                c.Rect(x + 2, 35, 11, 3, PixelArt.Hex("c2e5e8"));
                c.Rect(x + 7, 33, 2, 23, wood);
                c.Rect(x + 1, 46, 13, 2, wood);
                c.Rect(x - 2, 30, 19, 2, wood);
                c.Rect(x - 2, 57, 19, 2, plasterDark);
            }
            Outlined(c, 43, 5, 18, 26, wood, outline);
            c.Rect(46, 8, 12, 21, PixelArt.Hex("3e302d"));
            c.Rect(48, 9, 8, 16, PixelArt.Hex("9e6942"));
            c.Set(54, 18, PixelArt.Hex("ffe08a"));
            Add(c, new Vector2(0.5f, 0f), "oldtown_facade");
        }

        static void BuildPersonalVehicles()
        {
            Color32 outline = PixelArt.Hex("322b36");
            Color32 glass = PixelArt.Hex("bce5e8");
            Color32 rubber = PixelArt.Hex("241a2b");

            var motorcycle = new PixelCanvas(48, 30);
            motorcycle.Ellipse(24f, 5f, 21f, 4f, new Color32(18, 12, 24, 90));
            motorcycle.Disc(10f, 9f, 6f, outline);
            motorcycle.Disc(10f, 9f, 3f, PixelArt.Hex("aab2b4"));
            motorcycle.Disc(38f, 9f, 6f, outline);
            motorcycle.Disc(38f, 9f, 3f, PixelArt.Hex("aab2b4"));
            motorcycle.Line(11, 10, 21, 17, outline, 4);
            motorcycle.Line(21, 17, 35, 10, outline, 4);
            motorcycle.Line(16, 14, 34, 14, PixelArt.Hex("ef6355"), 7);
            motorcycle.Line(20, 19, 30, 19, PixelArt.Hex("292b3b"), 4);
            motorcycle.Line(31, 18, 38, 20, outline, 2);
            motorcycle.Disc(32f, 23f, 3f, PixelArt.Hex("f5d270"));
            Add(motorcycle, new Vector2(0.5f, 0f), "vehicle_motorcycle");

            var angkot = new PixelCanvas(64, 34);
            angkot.Ellipse(32f, 5f, 29f, 4f, new Color32(18, 12, 24, 90));
            Outlined(angkot, 5, 8, 54, 21, PixelArt.Hex("278e86"), outline);
            angkot.Rect(10, 12, 44, 7, glass);
            angkot.Rect(28, 11, 3, 9, outline);
            angkot.Rect(42, 11, 3, 9, outline);
            angkot.Rect(8, 20, 48, 4, PixelArt.Hex("f4cf66"));
            angkot.Rect(5, 13, 4, 7, PixelArt.Hex("f8e5a8"));
            angkot.Rect(55, 13, 4, 7, PixelArt.Hex("f36a58"));
            angkot.Disc(16f, 9f, 5f, rubber);
            angkot.Disc(48f, 9f, 5f, rubber);
            angkot.Disc(16f, 9f, 2f, PixelArt.Hex("c4c3b8"));
            angkot.Disc(48f, 9f, 2f, PixelArt.Hex("c4c3b8"));
            Add(angkot, new Vector2(0.5f, 0f), "vehicle_angkot");

            var car = new PixelCanvas(64, 34);
            car.Ellipse(32f, 5f, 30f, 4f, new Color32(18, 12, 24, 90));
            Outlined(car, 5, 8, 54, 18, PixelArt.Hex("b94e54"), outline);
            car.Triangle(new Vector2(16f, 22f), new Vector2(23f, 30f),
                new Vector2(43f, 30f), outline);
            car.Triangle(new Vector2(18f, 22f), new Vector2(24f, 28f),
                new Vector2(31f, 28f), glass);
            car.Triangle(new Vector2(33f, 28f), new Vector2(40f, 28f),
                new Vector2(45f, 22f), glass);
            car.Rect(10, 14, 44, 4, PixelArt.Hex("cf5c5c"));
            car.Rect(6, 16, 4, 4, PixelArt.Hex("fff0b2"));
            car.Rect(54, 16, 4, 4, PixelArt.Hex("f27a69"));
            car.Disc(16f, 9f, 5f, rubber);
            car.Disc(48f, 9f, 5f, rubber);
            car.Disc(16f, 9f, 2f, PixelArt.Hex("c4c3b8"));
            car.Disc(48f, 9f, 2f, PixelArt.Hex("c4c3b8"));
            Add(car, new Vector2(0.5f, 0f), "vehicle_car");
        }

        static void BuildOfficeTower()
        {
            var c = new PixelCanvas(80, 160);
            Color32 outline = PixelArt.Hex("273849");
            Color32 glass = PixelArt.Hex("47758a");
            Color32 glassLight = PixelArt.Hex("78a9b4");
            Color32 glassDark = PixelArt.Hex("345b70");
            Color32 concrete = PixelArt.Hex("c1bba9");

            c.Ellipse(40f, 5f, 37f, 5f, new Color32(18, 12, 24, 90));
            Outlined(c, 9, 5, 62, 139, glass, outline);
            c.Rect(14, 12, 52, 126, glassDark);
            c.Rect(18, 12, 5, 126, glassLight);
            c.Rect(30, 12, 4, 126, glass);
            c.Rect(45, 12, 5, 126, glassLight);
            c.Rect(59, 12, 4, 126, glass);
            for (int y = 18; y < 137; y += 12)
            {
                c.Rect(13, y, 54, 3, concrete);
                c.Rect(15, y + 1, 50, 1, PixelArt.Hex("eee6d2"));
            }
            c.Rect(5, 144, 70, 5, outline);
            c.Rect(13, 149, 54, 7, concrete);
            c.Rect(38, 0, 4, 7, outline);
            c.Rect(39, 0, 2, 5, PixelArt.Hex("b8dce0"));
            Add(c, new Vector2(0.5f, 0f), "office_tower");
        }

        static void BuildFountain()
        {
            var c = new PixelCanvas(56, 52);
            Color32 stoneDark = PixelArt.Hex("6d6a78");
            Color32 stone = PixelArt.Hex("9a96a4");
            Color32 stoneLight = PixelArt.Hex("c2bdc8");
            Color32 water = PixelArt.Hex("3f8ec4");
            Color32 waterLight = PixelArt.Hex("78c8ec");
            Color32 waterDark = PixelArt.Hex("2c6f9e");

            c.Ellipse(28f, 9f, 25f, 6f, new Color32(18, 12, 24, 90));

            // basin
            c.Ellipse(28f, 15f, 26f, 14f, stoneDark);
            c.Ellipse(28f, 16f, 24f, 12f, stone);
            c.Ellipse(28f, 18f, 22f, 10f, stoneDark);
            c.Ellipse(28f, 18f, 20f, 8.5f, waterDark);
            c.Ellipse(28f, 18f, 18f, 7f, water);
            c.Ellipse(28f, 19f, 14f, 5f, waterLight);

            // ripples
            for (int i = 0; i < 6; i++)
            {
                float rx = 5f + i * 2.4f;
                c.Ellipse(28f, 19f, rx, rx * 0.34f, new Color32(150, 220, 245, 70));
            }

            // rim stones
            for (int a = 0; a < 360; a += 24)
            {
                float r = a * Mathf.Deg2Rad;
                int x = 28 + Mathf.RoundToInt(Mathf.Cos(r) * 22f);
                int y = 18 + Mathf.RoundToInt(Mathf.Sin(r) * 9f);
                c.Rect(x - 2, y - 1, 4, 3, stoneLight);
            }

            // centre pillar
            Outlined(c, 25, 14, 6, 22, stone, stoneDark);
            c.Rect(26, 18, 2, 16, stoneLight);
            c.Ellipse(28f, 37f, 9f, 4f, stoneDark);
            c.Ellipse(28f, 38f, 8f, 3f, stone);
            c.Ellipse(28f, 38f, 6f, 2f, waterLight);

            // water jets
            for (int i = 0; i < 14; i++)
            {
                float t = i / 13f;
                int x = 28 + Mathf.RoundToInt(Mathf.Sin(t * 3.1f) * (4f + t * 6f));
                int y = 38 + Mathf.RoundToInt(t * 8f);
                c.Set(x, y, waterLight);
                if (i % 2 == 0) c.Set(x + 1, y - 1, water);
            }
            c.Set(20, 34, waterLight);
            c.Set(36, 33, waterLight);
            c.Set(19, 30, water);
            c.Set(37, 29, water);

            Add(c, new Vector2(0.5f, 0f), "fountain");
        }

        static void BuildBench()
        {
            var c = new PixelCanvas(40, 28);
            Color32 wood = PixelArt.Hex("b97b4a");
            Color32 woodLight = PixelArt.Hex("d39a63");
            Color32 woodDark = PixelArt.Hex("7b4a2a");
            Color32 iron = PixelArt.Hex("3a3340");

            c.Ellipse(20f, 4f, 18f, 4f, new Color32(18, 12, 24, 90));

            // legs
            Outlined(c, 4, 2, 4, 12, iron, Outline);
            Outlined(c, 32, 2, 4, 12, iron, Outline);
            // seat
            Outlined(c, 2, 11, 36, 6, wood, woodDark);
            c.Rect(3, 14, 34, 2, woodLight);
            // back rest
            Outlined(c, 2, 18, 36, 10, wood, woodDark);
            c.Rect(3, 24, 34, 2, woodLight);
            c.Rect(4, 20, 32, 1, woodDark);
            c.Rect(9, 18, 2, 10, woodDark);
            c.Rect(29, 18, 2, 10, woodDark);
            c.Rect(4, 11, 2, 12, woodDark);
            c.Rect(34, 11, 2, 12, woodDark);

            Add(c, new Vector2(0.5f, 0f), "bench");
        }

        static void BuildLamp()
        {
            var c = new PixelCanvas(20, 64);
            Color32 metal = PixelArt.Hex("3a3340");
            Color32 metalLight = PixelArt.Hex("5d5468");
            Color32 glass = PixelArt.Hex("ffe08a");
            Color32 glassLight = PixelArt.Hex("fff7cf");

            c.Ellipse(10f, 3f, 7f, 3f, new Color32(18, 12, 24, 90));
            Outlined(c, 5, 1, 10, 4, metal, Outline);
            c.Rect(9, 4, 2, 45, metal);
            c.Rect(9, 4, 1, 45, metalLight);
            c.Rect(7, 48, 6, 2, metal);
            Outlined(c, 4, 49, 12, 11, metal, Outline);
            c.Rect(6, 51, 8, 7, glass);
            c.Rect(7, 54, 6, 3, glassLight);
            c.Rect(9, 51, 1, 7, metal);
            c.Rect(6, 53, 8, 1, metal);
            c.Rect(4, 60, 12, 2, metal);
            c.Rect(6, 62, 8, 2, metalLight);

            Add(c, new Vector2(0.5f, 0f), "lamp");
        }

        static void BuildSign()
        {
            var c = new PixelCanvas(34, 40);
            Color32 wood = PixelArt.Hex("b97b4a");
            Color32 woodLight = PixelArt.Hex("d39a63");
            Color32 woodDark = PixelArt.Hex("7b4a2a");
            Color32 ink = PixelArt.Hex("fff1d6");

            c.Ellipse(17f, 3f, 12f, 3f, new Color32(18, 12, 24, 80));
            Outlined(c, 7, 0, 3, 20, wood, woodDark);
            Outlined(c, 24, 0, 3, 20, wood, woodDark);
            Outlined(c, 2, 15, 30, 22, wood, woodDark);
            c.Rect(4, 17, 26, 18, woodLight);
            c.Rect(4, 18, 26, 1, woodDark);
            c.Rect(4, 33, 26, 1, woodDark);
            c.Rect(7, 29, 20, 2, ink);
            c.Rect(7, 25, 15, 2, ink);
            c.Rect(7, 21, 18, 2, ink);
            // arrow
            c.Rect(24, 20, 3, 2, woodDark);
            c.Rect(26, 21, 2, 3, woodDark);

            Add(c, new Vector2(0.5f, 0f), "sign");
        }

        static void BuildTree()
        {
            var c = new PixelCanvas(48, 66);
            Color32 trunk = PixelArt.Hex("8a5a3c");
            Color32 trunkDark = PixelArt.Hex("5e3826");
            Color32 leafDark = PixelArt.Hex("2f6f3e");
            Color32 leaf = PixelArt.Hex("4a9c48");
            Color32 leafLight = PixelArt.Hex("7ac45c");

            c.Ellipse(24f, 8f, 19f, 5f, new Color32(18, 12, 24, 90));
            Outlined(c, 20, 2, 8, 26, trunk, trunkDark);
            c.Rect(21, 4, 2, 22, PixelArt.Hex("a97c4c"));

            c.Disc(16f, 38f, 13f, leafDark);
            c.Disc(32f, 39f, 13f, leafDark);
            c.Disc(24f, 47f, 15f, leafDark);
            c.Disc(24f, 40f, 18f, leafDark);
            c.Disc(17f, 40f, 11f, leaf);
            c.Disc(31f, 42f, 11f, leaf);
            c.Disc(24f, 49f, 12f, leaf);
            c.Disc(20f, 47f, 7f, leafLight);
            c.Disc(29f, 45f, 5f, leafLight);
            c.Disc(25f, 53f, 5f, leafLight);

            for (int i = 0; i < 18; i++)
            {
                int x = (int)(Hash(i, 61) % 32u) + 8;
                int y = (int)(Hash(i, 67) % 30u) + 28;
                c.Set(x, y, i % 3 == 0 ? leafLight : leafDark);
            }
            for (int i = 0; i < 5; i++)
            {
                int x = (int)(Hash(i, 71) % 28u) + 10;
                int y = (int)(Hash(i, 73) % 22u) + 32;
                c.Rect(x, y, 2, 2, PixelArt.Hex("ff9fc0"));
            }

            Add(c, new Vector2(0.5f, 0f), "tree");
        }

        static void BuildBush()
        {
            var c = new PixelCanvas(30, 20);
            Color32 dark = PixelArt.Hex("2f6f3e");
            Color32 mid = PixelArt.Hex("4a9c48");
            Color32 light = PixelArt.Hex("7ac45c");

            c.Ellipse(15f, 5f, 13f, 4f, new Color32(18, 12, 24, 70));
            c.Disc(9f, 10f, 7f, dark);
            c.Disc(21f, 10f, 7f, dark);
            c.Disc(15f, 13f, 8f, dark);
            c.Disc(10f, 11f, 5f, mid);
            c.Disc(20f, 12f, 5f, mid);
            c.Disc(15f, 15f, 5f, light);
            c.Set(8, 12, PixelArt.Hex("ff9fc0"));
            c.Set(22, 14, PixelArt.Hex("ffd35c"));
            c.Set(16, 17, PixelArt.Hex("fff1d6"));

            Add(c, new Vector2(0.5f, 0f), "bush");
        }

        static void BuildFlowers()
        {
            var c = new PixelCanvas(26, 18);
            Color32 stem = PixelArt.Hex("3f8639");
            for (int i = 0; i < 6; i++)
            {
                int x = 3 + i * 4;
                int h = 5 + (int)(Hash(i, 83) % 5u);
                c.Rect(x, 0, 1, h, stem);
                Color32 petal = i % 3 == 0 ? PixelArt.Hex("ff9fc0")
                    : i % 3 == 1 ? PixelArt.Hex("ffd35c") : PixelArt.Hex("e8f6ff");
                c.Rect(x - 1, h, 3, 2, petal);
                c.Set(x, h + 2, petal);
                c.Set(x + 1, h + 1, PixelArt.Hex("fff7cf"));
            }
            c.Rect(1, 0, 24, 1, PixelArt.Hex("4e9a45"));
            Add(c, new Vector2(0.5f, 0f), "flowers");
        }

        // ------------------------------------------------------------------ farming

        static void BuildFarmGround()
        {
            var c = new PixelCanvas(80, 80);
            Color32 edge = PixelArt.Hex("7b5433");
            Color32 dirt = PixelArt.Hex("9a6a41");
            Color32 dirtDark = PixelArt.Hex("83582f");
            Color32 dirtLight = PixelArt.Hex("b18455");

            c.Rect(2, 2, 76, 76, edge);
            c.Rect(5, 5, 70, 70, dirt);
            for (int y = 8; y < 74; y += 8)
            {
                c.Rect(6, y, 68, 3, dirtDark);
                c.Rect(6, y + 3, 68, 1, dirtLight);
            }
            for (int i = 0; i < 90; i++)
            {
                int x = (int)(Hash(i, 91) % 66u) + 7;
                int y = (int)(Hash(i, 97) % 66u) + 7;
                c.Set(x, y, i % 2 == 0 ? dirtDark : dirtLight);
            }

            Add(c, new Vector2(0.5f, 0.5f), "farm_ground");
        }

        static void BuildPlots()
        {
            MakePlot("plot_empty", 0);
            MakePlot("plot_seed", 1);
            MakePlot("plot_water", 2);
            MakePlot("plot_bloom", 3);
        }

        static void MakePlot(string key, int stage)
        {
            var c = new PixelCanvas(32, 32);
            Color32 edge = PixelArt.Hex("6e4a2c");
            Color32 dirt = PixelArt.Hex("9a6a41");
            Color32 dirtDark = PixelArt.Hex("7f552e");
            Color32 dirtLight = PixelArt.Hex("b18455");

            c.Rect(1, 1, 30, 30, edge);
            Color32 soil = stage == 2 ? PixelArt.Hex("78502e") : dirt;
            c.Rect(3, 3, 26, 26, soil);
            for (int y = 6; y < 28; y += 6)
            {
                c.Rect(4, y, 24, 2, dirtDark);
                c.Rect(4, y + 2, 24, 1, dirtLight);
            }
            for (int i = 0; i < 24; i++)
            {
                int x = (int)(Hash(i, stage * 13 + 101) % 22u) + 5;
                int y = (int)(Hash(i, stage * 17 + 103) % 22u) + 5;
                c.Set(x, y, i % 2 == 0 ? dirtLight : dirtDark);
            }

            if (stage >= 1)
            {
                Color32 stem = PixelArt.Hex("4a9c3f");
                Color32 leaf = PixelArt.Hex("7ac45c");
                int[][] spots = { new[] { 9, 8 }, new[] { 16, 7 }, new[] { 22, 9 } };
                for (int i = 0; i < spots.Length; i++)
                {
                    int x = spots[i][0], y = spots[i][1];
                    c.Rect(x, y, 1, 6, stem);
                    c.Rect(x - 2, y + 4, 3, 2, leaf);
                    c.Rect(x + 1, y + 5, 3, 2, leaf);
                    if (stage == 3)
                    {
                        Color32 petal = i == 0 ? PixelArt.Hex("ff9fc0")
                            : i == 1 ? PixelArt.Hex("ffd35c") : PixelArt.Hex("e8f6ff");
                        c.Rect(x - 2, y + 8, 3, 3, petal);
                        c.Rect(x + 1, y + 8, 3, 3, petal);
                        c.Set(x, y + 9, PixelArt.Hex("fff7cf"));
                        c.Set(x + 1, y + 11, PixelArt.Hex("fff7cf"));
                    }
                }
            }

            if (stage == 2)
            {
                c.Set(8, 24, PixelArt.Hex("9fd8f0"));
                c.Set(15, 22, PixelArt.Hex("9fd8f0"));
                c.Set(23, 25, PixelArt.Hex("9fd8f0"));
                c.Set(12, 20, PixelArt.Hex("c7f0ff"));
                c.Set(19, 20, PixelArt.Hex("c7f0ff"));
            }

            Add(c, new Vector2(0.5f, 0.5f), key);
        }

        // ------------------------------------------------------------------ icons

        static void BuildGazebo()
        {
            var c = new PixelCanvas(144, 136);
            Color32 shadow = new Color32(18, 12, 24, 85);
            Color32 wood = PixelArt.Hex("b88355");
            Color32 woodDark = PixelArt.Hex("7d5436");
            Color32 roof = PixelArt.Hex("6f4a6e");
            Color32 roofLight = PixelArt.Hex("a06a94");
            Color32 trim = PixelArt.Hex("f0dfb8");
            Color32 leaf = PixelArt.Hex("4f9a55");

            c.Ellipse(72f, 7f, 62f, 7f, shadow);
            // stone base
            c.Rect(18, 9, 108, 10, PixelArt.Hex("8c8377"));
            c.Rect(22, 16, 100, 8, PixelArt.Hex("b5ac9d"));
            // posts
            for (int i = 0; i < 4; i++)
            {
                int x = 30 + i * 28;
                c.Rect(x - 3, 24, 7, 62, woodDark);
                c.Rect(x - 1, 24, 3, 62, wood);
            }
            // railings
            c.Rect(30, 42, 84, 4, woodDark);
            c.Rect(30, 68, 84, 4, woodDark);
            c.Rect(30, 42, 84, 1, trim);
            // roof
            c.Triangle(new Vector2(72f, 132f), new Vector2(10f, 82f),
                new Vector2(134f, 82f), roofDarkSafe());
            c.Triangle(new Vector2(72f, 128f), new Vector2(18f, 86f),
                new Vector2(126f, 86f), roof);
            c.Triangle(new Vector2(72f, 124f), new Vector2(42f, 96f),
                new Vector2(102f, 96f), roofLight);
            c.Rect(6, 82, 132, 5, trim);
            c.Disc(72f, 130f, 5f, trim);
            // climbing flowers
            for (int i = 0; i < 10; i++)
            {
                int x = 14 + (int)(Hash(i, 313) % 116u);
                int y = 84 + (int)(Hash(i, 317) % 10u);
                c.Disc(x, y, 3f, leaf);
                if (i % 2 == 0) c.Set(x + 1, y + 1, PixelArt.Hex("ff9fc0"));
            }
            Add(c, new Vector2(0.5f, 0f), "gazebo");
        }

        // Small helper so the gazebo roof stays readable against the night tint.
        static Color32 roofDarkSafe() => PixelArt.Hex("4a3149");

        static void BuildBridge()
        {
            var c = new PixelCanvas(144, 56);
            Color32 shadow = new Color32(18, 12, 24, 70);
            Color32 wood = PixelArt.Hex("c09263");
            Color32 woodDark = PixelArt.Hex("8a623f");
            Color32 rail = PixelArt.Hex("6d4a30");
            Color32 water = PixelArt.Hex("5eb6d6");

            c.Ellipse(72f, 4f, 68f, 4f, shadow);
            c.Rect(0, 8, 144, 8, water);
            c.Rect(0, 10, 144, 2, PixelArt.Hex("9ce1e8"));
            // deck
            c.Rect(4, 18, 136, 22, woodDark);
            c.Rect(6, 20, 132, 18, wood);
            for (int x = 10; x < 136; x += 12)
                c.Rect(x, 20, 2, 18, woodDark);
            // rails
            c.Rect(6, 38, 132, 4, rail);
            c.Rect(6, 46, 132, 3, rail);
            for (int x = 8; x < 140; x += 16)
                c.Rect(x, 36, 3, 17, rail);
            Add(c, new Vector2(0.5f, 0f), "bridge");
        }

        static void BuildEasel()
        {
            var c = new PixelCanvas(56, 72);
            Color32 wood = PixelArt.Hex("a7794c");
            Color32 woodDark = PixelArt.Hex("6f4a2e");
            Color32 paper = PixelArt.Hex("fff4da");
            Color32 ink = PixelArt.Hex("4a3b58");
            Color32 pink = PixelArt.Hex("e8749a");

            c.Ellipse(28f, 3f, 24f, 3f, new Color32(18, 12, 24, 70));
            c.Line(12, 4, 24, 60, woodDark, 3);
            c.Line(44, 4, 32, 60, woodDark, 3);
            c.Line(28, 10, 28, 44, woodDark, 3);
            c.Rect(8, 34, 40, 28, woodDark);
            c.Rect(10, 36, 36, 24, paper);
            // a little sunset sketch on the paper
            c.Disc(28f, 48f, 7f, pink);
            c.Rect(12, 38, 32, 4, PixelArt.Hex("8ecf7a"));
            c.Line(14, 54, 22, 50, ink, 1);
            c.Line(34, 50, 42, 54, ink, 1);
            c.Rect(8, 58, 40, 4, wood);
            Add(c, new Vector2(0.5f, 0f), "easel");
        }

        /// <summary>Shelter halte sederhana: tiang, atap, kaca, bangku, dan papan.</summary>
        static void BuildBusStop()
        {
            var c = new PixelCanvas(80, 64);
            Color32 outline = PixelArt.Hex("30252d");
            Color32 metal = PixelArt.Hex("3a3340");
            Color32 metalLight = PixelArt.Hex("5d5468");
            Color32 roof = PixelArt.Hex("2b7a78");
            Color32 roofLight = PixelArt.Hex("3fa9a2");
            Color32 roofDark = PixelArt.Hex("1f5e63");
            Color32 glass = PixelArt.Hex("78c1d6");
            Color32 glassLight = PixelArt.Hex("bce5e8");
            Color32 wood = PixelArt.Hex("b97b4a");
            Color32 woodLight = PixelArt.Hex("d39a63");
            Color32 sign = PixelArt.Hex("ffd35c");

            c.Ellipse(40f, 5f, 34f, 5f, new Color32(18, 12, 24, 90));

            // Empat tiang shelter.
            c.Rect(14, 6, 3, 34, metal);
            c.Rect(63, 6, 3, 34, metal);
            c.Rect(14, 6, 1, 34, metalLight);
            c.Rect(63, 6, 1, 34, metalLight);

            // Panel kaca belakang dengan rangka.
            Outlined(c, 12, 10, 56, 24, glass, outline);
            c.Rect(14, 12, 52, 18, glass);
            c.Rect(16, 27, 22, 2, glassLight);
            c.Rect(16, 24, 13, 2, glassLight);
            c.Rect(38, 10, 2, 24, outline);
            c.Rect(12, 21, 56, 2, outline);

            // Bangku tunggu.
            Outlined(c, 20, 12, 40, 4, wood, outline);
            c.Rect(22, 16, 36, 2, woodLight);
            c.Rect(24, 8, 2, 4, metal);
            c.Rect(54, 8, 2, 4, metal);

            // Atap dan papan informasi.
            Outlined(c, 4, 38, 72, 10, roof, outline);
            c.Rect(7, 44, 66, 3, roofLight);
            c.Rect(4, 36, 72, 3, roofDark);
            Outlined(c, 28, 50, 24, 11, sign, outline);
            c.Rect(32, 53, 16, 5, PixelArt.Hex("162a37"));
            c.Rect(34, 54, 12, 2, sign);
            c.Rect(34, 57, 2, 1, sign);
            c.Rect(38, 57, 2, 1, sign);
            c.Rect(42, 57, 2, 1, sign);

            Add(c, new Vector2(0.5f, 0f), "bus_stop");
        }

        static void BuildIcons()
        {
            MakeCoin();
            MakeHeart();
            MakeBolt();
            MakeClock();
            MakeSeedIcon();
            MakeFlowerIcon();
            MakeStarIcon();
            MakeDirectionIcon();
        }

        static void MakeCoin()
        {
            var c = new PixelCanvas(16, 16);
            c.Disc(8f, 8f, 7.4f, PixelArt.Hex("9a6a1e"));
            c.Disc(8f, 8f, 6.4f, PixelArt.Hex("ffd35c"));
            c.Disc(8f, 8.4f, 4.8f, PixelArt.Hex("ffe89a"));
            c.Rect(6, 5, 4, 6, PixelArt.Hex("c98a24"));
            c.Rect(6, 8, 4, 1, PixelArt.Hex("ffd35c"));
            c.Set(5, 11, PixelArt.Hex("fff7cf"));
            Add(c, new Vector2(0.5f, 0.5f), "icon_coin");
        }

        static void MakeHeart()
        {
            var c = new PixelCanvas(16, 16);
            Color32 dark = PixelArt.Hex("7a1f3d");
            Color32 main = PixelArt.Hex("ff5f86");
            Color32 light = PixelArt.Hex("ffb3c8");
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    float fx = (x + 0.5f - 8f);
                    float fy = (y + 0.5f - 8.4f);
                    float v = fx * fx + fy * fy - 4.6f;
                    if (v * v * v - fx * fx * fy * fy * fy <= 0f)
                        c.Set(x, y, main);
                }
            c.Rect(5, 10, 3, 2, light);
            c.Rect(4, 8, 2, 2, light);
            c.Rect(8, 3, 2, 1, dark);
            Add(c, new Vector2(0.5f, 0.5f), "icon_heart");
        }

        static void MakeBolt()
        {
            var c = new PixelCanvas(16, 16);
            Color32 dark = PixelArt.Hex("8a6a1e");
            Color32 main = PixelArt.Hex("ffe066");
            Color32 light = PixelArt.Hex("fff7cf");
            c.Triangle(new Vector2(9f, 15f), new Vector2(3f, 7f), new Vector2(8f, 7f), dark);
            c.Triangle(new Vector2(8f, 7f), new Vector2(13f, 7f), new Vector2(6f, 1f), dark);
            c.Triangle(new Vector2(8.6f, 14f), new Vector2(4.2f, 7.8f), new Vector2(8.2f, 7.8f), main);
            c.Triangle(new Vector2(7.8f, 7.8f), new Vector2(12f, 7.8f), new Vector2(6.4f, 2f), main);
            c.Set(8, 12, light);
            Add(c, new Vector2(0.5f, 0.5f), "icon_bolt");
        }

        static void MakeClock()
        {
            var c = new PixelCanvas(16, 16);
            c.Disc(8f, 8f, 7.2f, PixelArt.Hex("241a2b"));
            c.Disc(8f, 8f, 6.2f, PixelArt.Hex("fff1d6"));
            c.Disc(8f, 8f, 4.8f, PixelArt.Hex("e8dcc0"));
            c.Rect(8, 8, 1, 4, PixelArt.Hex("3a2a3e"));
            c.Rect(8, 8, 3, 1, PixelArt.Hex("3a2a3e"));
            c.Rect(7, 2, 2, 1, PixelArt.Hex("3a2a3e"));
            c.Rect(7, 13, 2, 1, PixelArt.Hex("3a2a3e"));
            Add(c, new Vector2(0.5f, 0.5f), "icon_clock");
        }

        static void MakeSeedIcon()
        {
            var c = new PixelCanvas(16, 16);
            Color32 bag = PixelArt.Hex("c9a06a");
            Color32 bagDark = PixelArt.Hex("8a5f39");
            Outlined(c, 3, 3, 10, 9, bag, bagDark);
            c.Rect(2, 10, 12, 3, bagDark);
            c.Rect(4, 12, 8, 2, PixelArt.Hex("a97c4c"));
            c.Rect(6, 6, 2, 3, PixelArt.Hex("4a9c3f"));
            c.Set(5, 8, PixelArt.Hex("7ac45c"));
            c.Set(8, 8, PixelArt.Hex("7ac45c"));
            c.Set(7, 4, PixelArt.Hex("fff1d6"));
            Add(c, new Vector2(0.5f, 0.5f), "icon_seed");
        }

        static void MakeFlowerIcon()
        {
            var c = new PixelCanvas(16, 16);
            c.Rect(7, 1, 2, 6, PixelArt.Hex("3f8639"));
            c.Rect(4, 4, 3, 2, PixelArt.Hex("7ac45c"));
            c.Rect(9, 6, 3, 2, PixelArt.Hex("7ac45c"));
            Color32 petal = PixelArt.Hex("ff9fc0");
            c.Disc(8f, 10f, 2.2f, petal);
            c.Disc(5f, 11f, 2.2f, petal);
            c.Disc(11f, 11f, 2.2f, petal);
            c.Disc(8f, 13.4f, 2.2f, petal);
            c.Disc(8f, 11f, 1.6f, PixelArt.Hex("ffd35c"));
            Add(c, new Vector2(0.5f, 0.5f), "icon_flower");
        }

        static void MakeStarIcon()
        {
            var c = new PixelCanvas(16, 16);
            Color32 main = PixelArt.Hex("ffd35c");
            c.Triangle(new Vector2(8f, 15f), new Vector2(5.6f, 8.5f), new Vector2(10.4f, 8.5f), main);
            c.Triangle(new Vector2(2f, 10f), new Vector2(14f, 10f), new Vector2(8f, 5.5f), main);
            c.Triangle(new Vector2(4f, 2f), new Vector2(12f, 2f), new Vector2(8f, 7f), main);
            c.Rect(6, 6, 4, 4, PixelArt.Hex("fff7cf"));
            Add(c, new Vector2(0.5f, 0.5f), "icon_star");
        }

        static void MakeDirectionIcon()
        {
            var c = new PixelCanvas(16, 16);
            c.Triangle(new Vector2(8f, 15f), new Vector2(2f, 2f),
                new Vector2(8f, 5f), PixelArt.Hex("241a2b"));
            c.Triangle(new Vector2(8f, 15f), new Vector2(8f, 5f),
                new Vector2(14f, 2f), PixelArt.Hex("241a2b"));
            c.Triangle(new Vector2(8f, 13f), new Vector2(4f, 4f),
                new Vector2(8f, 6f), PixelArt.Hex("ff8a45"));
            c.Triangle(new Vector2(8f, 13f), new Vector2(8f, 6f),
                new Vector2(12f, 4f), PixelArt.Hex("ffca69"));
            Add(c, new Vector2(0.5f, 0.5f), "icon_direction");
        }
    }
}
