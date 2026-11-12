using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure
{
    public static partial class PixelArt
    {
        // ---------------------------------------------------------------- characters

        static readonly Dictionary<char, Color32> HeroPal = Palette(
            'o', "1b1b2a", 'h', "cdd6e8", 'H', "8f9bb3", 'r', "e8484f",
            's', "f4c28b", 'e', "24243a", 'y', "ffc857", 'b', "3d7bd9",
            'B', "2a56a0", 'p', "33395c", 't', "6b4026");

        static readonly string[] HeroTop =
        {
            "................",
            "......rr........",
            ".....rhhr.......",
            "....rhhhhr......",
            "....oHhhHo......",
            "...oHHHHHHo.....",
            "...ohssssho.....",
            "...osseesso.....",
            "...osssssso.....",
            "....osssso......",
            "...oyyyyyyo.....",
            "..obbbbbbbbo....",
            "..obBbbbbBbo....",
            "..obbbbbbbbo...."
        };

        static readonly Dictionary<char, Color32> SlimePal = Palette(
            'o', "1d2b1d", 'g', "58c05a", 'G', "8be36a", 'd', "2f8a42",
            'w', "f4fff0", 'e', "1a2a1a");

        static readonly string[] SlimeRows =
        {
            "................",
            "................",
            ".....oooooo.....",
            "...oogGGgggoo...",
            "..ogGGggggggo...",
            "..oggggggggggo..",
            "..ogweggggewgo..",
            "..oggggggggggo..",
            "..odggggggggdo..",
            "..oddggggggddo..",
            "...ooddddddoo...",
            "....oooooooo...."
        };

        static readonly Dictionary<char, Color32> BatPal = Palette(
            'o', "241d3a", 'p', "7a4fd0", 'P', "b488ff", 'e', "ff4d6d",
            'w', "f4eaff");

        static readonly string[] BatA =
        {
            "................",
            "..oo........oo..",
            ".oPPo......oPPo.",
            ".oPPPo....oPPPo.",
            "..oPPPo..oPPPo..",
            "...oPPPooPPPo...",
            "....oPPPPPPo....",
            ".....oPePePo....",
            "......oPPPPo....",
            ".......oooo....."
        };

        static readonly string[] BatB =
        {
            "................",
            "................",
            "...oo......oo...",
            "..oPPo....oPPo..",
            "..oPPo.oo.oPPo..",
            "...oPPoPPoPPo...",
            "....oPPPPPPo....",
            ".....oPePePo....",
            "......oPPPPo....",
            ".......oooo....."
        };

        static readonly Dictionary<char, Color32> GemPal = Palette(
            'o', "0d4f5c", 'y', "1fb8a6", 'Y', "6ef2d8", 'w', "e8ffff", 'b', "0a8f8a");

        static readonly string[] GemA =
        {
            "....oooo....",
            "...oyyyyo...",
            "..oyYYYYyo..",
            ".oyYYwwYYyo.",
            ".oyYwwwwYyo.",
            ".oyYwwwwYyo.",
            ".oyYYwwYYyo.",
            "..oyYYYYyo..",
            "...oyyyyo...",
            "....oooo....",
            "............",
            "............"
        };

        static readonly string[] GemB =
        {
            "....oooo....",
            "...oyyyyo...",
            "..oyYYYYyo..",
            ".oyYYYYyyyo.",
            ".oyYYYwwYyo.",
            ".oyYYYwYYyo.",
            ".oyYYYYyyyo.",
            "..oyYYYYyo..",
            "...oyyyyo...",
            "....oooo....",
            "............",
            "............"
        };

        static readonly string[] HeartRows =
        {
            ".oo...oo..",
            "orRroorRro",
            "oRRRRRRRRo",
            "oRRwRRRRRo",
            "oRRRRRRRRo",
            ".oRRRRRRo.",
            "..oRRRRo..",
            "...oRRo...",
            "....oo...."
        };

        static readonly string[] SpikeRows =
        {
            "................",
            "................",
            "..o...o...o...o.",
            ".oSo.oSo.oSo.oSo",
            ".oSo.oSo.oSo.oSo",
            "oSSSoSSSoSSSoSSS",
            "oSSSoSSSoSSSoSSS",
            "osssosssosssosss",
            "oooooooooooooooo",
            "................"
        };

        static readonly string[] SpringRows =
        {
            "................",
            "....oooooooo....",
            "...oRRRRRRRRo...",
            "...oRwwwwwwRo...",
            "...oRRRRRRRRo...",
            "..oSSSSSSSSSSo..",
            "..oSSSSSSSSSSo..",
            "..oooooooooooo..",
            "................",
            "................"
        };

        static readonly string[] ShrineRows =
        {
            ".....CC.....",
            "....CCCC....",
            "...CCCCCC...",
            "....CCCC....",
            ".....CC.....",
            "....o..o....",
            "...oSssSo...",
            "..oSssssSo..",
            "..oSssssSo..",
            "..oSssssSo..",
            "..oSssssSo..",
            "..oSssssSo..",
            ".oSssssssSo.",
            ".oSssssssSo.",
            ".oSssssssSo.",
            "oSssssssssSo",
            "oSssssssssSo",
            "oooooooooooo"
        };

        static readonly string[] MushroomRows =
        {
            "...oooo...",
            "..orrrro..",
            ".orrwrrro.",
            "orrwwwrrro",
            "orrrrrrrro",
            ".oooooooo.",
            "...osso...",
            "...osso...",
            "..osssso..",
            "..oooooo.."
        };

        static readonly string[] FlowerRows =
        {
            "..oo....",
            ".oPPo...",
            "oPwwPo..",
            "oPwwPo..",
            ".oPPo...",
            "..oo....",
            "..oso...",
            "..oso...",
            ".osso...",
            ".oooo..."
        };

        static void BuildCore()
        {
            // Hero -----------------------------------------------------------------
            AddRows(HeroTop, HeroPal, new Vector2(0.5f, 0f), "hero_idle");

            string[] run1Rows = Append(HeroTop,
                "...oppppppo.....",
                "..oppo..oppo....");
            string[] run2Rows = Append(HeroTop,
                "...oppppppo.....",
                "..otto..otto....");
            string[] jumpRows = Append(HeroTop,
                "...oppppppo.....",
                "..ott....tto....");

            AddRows(run1Rows, HeroPal, new Vector2(0.5f, 0f), "hero_run1");
            AddRows(run2Rows, HeroPal, new Vector2(0.5f, 0f), "hero_run2");
            AddRows(jumpRows, HeroPal, new Vector2(0.5f, 0f), "hero_jump");

            AddRows(SlimeRows, SlimePal, new Vector2(0.5f, 0f), "slime");
            AddRows(BatA, BatPal, new Vector2(0.5f, 0.5f), "bat_a");
            AddRows(BatB, BatPal, new Vector2(0.5f, 0.5f), "bat_b");
            AddRows(GemA, GemPal, new Vector2(0.5f, 0.5f), "gem_a");
            AddRows(GemB, GemPal, new Vector2(0.5f, 0.5f), "gem_b");

            AddRows(HeartRows, Palette('o', "2a1520", 'r', "e8484f", 'R', "ff7a7a",
                'w', "ffd0d0"), new Vector2(0.5f, 0.5f), "heart_full");
            AddRows(HeartRows, Palette('o', "171522", 'r', "343042", 'R', "434054",
                'w', "555269"), new Vector2(0.5f, 0.5f), "heart_empty");

            AddRows(SpikeRows, Palette('o', "232735", 's', "9aa4b5", 'S', "d2dae7"),
                new Vector2(0.5f, 0f), "spike");

            AddRows(SpringRows, Palette('o', "3a1a22", 'r', "c8384a", 'R', "ff6b78",
                'w', "fff4f0", 'S', "c9d4e4"), new Vector2(0.5f, 0f), "spring");

            AddRows(ShrineRows, Palette('o', "22203a", 'C', "8ffff0", 's', "5b6484",
                'S', "919bb8"), new Vector2(0.5f, 0f), "shrine");

            AddRows(MushroomRows, Palette('o', "2a1520", 'r', "e8484f", 'w', "ffe6d6",
                's', "e3c79a"), new Vector2(0.5f, 0f), "mushroom_grove");
            AddRows(MushroomRows, Palette('o', "1b1730", 'r', "4fc6e0", 'w', "d8ffff",
                's', "9aa8c9"), new Vector2(0.5f, 0f), "mushroom_cave");
            AddRows(MushroomRows, Palette('o', "2a1520", 'r', "f08a3c", 'w', "ffe6c6",
                's', "c9a97a"), new Vector2(0.5f, 0f), "mushroom_ruins");

            AddRows(FlowerRows, Palette('o', "2a1520", 'P', "ff7aa8", 'w', "ffe066",
                's', "4a9c3f"), new Vector2(0.5f, 0f), "flower_grove");
            AddRows(FlowerRows, Palette('o', "2a1520", 'P', "ffb347", 'w', "ffe9a8",
                's', "7d8f3f"), new Vector2(0.5f, 0f), "flower_ruins");

            // Explicit pixel value definitions for runtime tinting.
            Sprites["pixel_white"] = MakeSolid(Hex("ffffff"), PPU);
            Sprites["pixel_black"] = MakeSolid(Hex("000000"), PPU);
        }

        static string[] Append(string[] top, params string[] bottom)
        {
            var result = new string[top.Length + bottom.Length];
            System.Array.Copy(top, result, top.Length);
            System.Array.Copy(bottom, 0, result, top.Length, bottom.Length);
            return result;
        }

        static Sprite AddRows(string[] rows, Dictionary<char, Color32> pal, Vector2 pivot, string key,
            float ppu = PPU)
        {
            return Make(rows, pal, pivot, ppu, FilterMode.Point, key);
        }

        // ---------------------------------------------------------------- themes + tiles

        static void BuildThemes()
        {
            BuildTileSet("grove",
                Palette('1', "a6f06a", '2', "5ec24a", '3', "2f7a3a",
                    'd', "b98454", 'D', "8a5a36", '5', "cf9b63"),
                Palette('d', "b98454", 'D', "8a5a36", '5', "6b4229"),
                Palette('S', "d6a066", 's', "a9713f", 'o', "6b4229"));

            BuildTileSet("cave",
                Palette('1', "6ef2d8", '2', "2fa79c", '3', "145a5f",
                    'd', "4c5270", 'D', "343a52", '5', "6e7391"),
                Palette('d', "4c5270", 'D', "343a52", '5', "23283c"),
                Palette('S', "8f7bd9", 's', "5f4bab", 'o', "352a6e"));

            BuildTileSet("ruins",
                Palette('1', "b9d96a", '2', "7d9c3f", '3', "4a6122",
                    'd', "dcc39c", 'D', "b89a6e", '5', "8a6f4a"),
                Palette('d', "dcc39c", 'D', "b89a6e", '5', "8a6f4a"),
                Palette('S', "e0cba8", 's', "bfa176", 'o', "7b5f3e"));

            BuildTree("tree_grove_a", 11, "1e5c32", "3f9b4a", "7fd35a", "6f4327", "4a2b18");
            BuildTree("tree_grove_b", 27, "1e5c32", "3f9b4a", "7fd35a", "6f4327", "4a2b18");
            BuildTree("tree_cave_a", 13, "1f4e5f", "2f8a8a", "6ef2d8", "3a3350", "211d33");
            BuildTree("tree_cave_b", 31, "1f4e5f", "2f8a8a", "6ef2d8", "3a3350", "211d33");
            BuildTree("tree_ruins_a", 17, "5c3a2a", "a55f33", "f0a05a", "3f2b1e", "2a1a12");
            BuildTree("tree_ruins_b", 41, "5c3a2a", "a55f33", "f0a05a", "3f2b1e", "2a1a12");

            BuildBush("bush_grove", 7, "1e5c32", "3f9b4a", "7fd35a");
            BuildBush("bush_cave", 9, "1f4e5f", "2f8a8a", "6ef2d8");
            BuildBush("bush_ruins", 21, "5c3a2a", "a55f33", "f0a05a");

            BuildRock("rock_grove", 5, "3f4459", "626981", "8f96ab", "cfd8e8");
            BuildRock("rock_cave", 8, "23283c", "343a52", "5b6484", "8f7bd9");
            BuildRock("rock_ruins", 12, "6b5238", "8a6f4a", "b89a6e", "e0cba8");

            BuildCrystal("crystal_cave_a", 3, "352a6e", "5f4bab", "b488ff", "8ffff0");
            BuildCrystal("crystal_cave_b", 19, "1f4e5f", "2f8a8a", "6ef2d8", "e8ffff");
            BuildCrystal("crystal_ruins_a", 23, "6b4229", "a9713f", "ffcd75", "fff0c0");

            BuildPillar("pillar_ruins", 33);

            BuildCloud("cloud_grove", 1, "f4f7ff", "cfd8ee");
            BuildCloud("cloud_ruins", 2, "ffe0bc", "d99a78");
            BuildCloud("cloud_cave", 3, "6b5a9a", "463b6b");

            BuildSun("sun_grove", "fff3b0", "ffcc55");
            BuildSun("sun_cave", "d8f6ff", "5fd3e0");
            BuildSun("sun_ruins", "ffe6a0", "ff7a3c");

            BuildPortal();
            BuildStars();
        }

        static readonly string[] GroundPattern =
        {
            "1111111111111111",
            "1211121112111211",
            "2222222222222222",
            "3333333333333333",
            "dDDDDDDDDDDDDDDd",
            "DDDDDDDDDDDDDDDD",
            "DDD5DDDDDD5DDDDD",
            "DDDDDDDDDDDDDDDD",
            "DDDDD5DDDDDDDDDD",
            "DDDDDDDDDDD5DDDD",
            "D5DDDDDDDDDDDDDD",
            "DDDDDDDDDD5DDDDD",
            "DDDDDD5DDDDDDDDD",
            "DD5DDDDDDDDDD5DD",
            "DDDDDDDDDDDDDDDD",
            "DDDDDDDDDDDDDDDD"
        };

        static readonly string[] FillPattern =
        {
            "DDDDDDDDDDDDDDDD",
            "DDD5DDDDDD5DDDDD",
            "DDDDDDDDDDDDDDDD",
            "DDDDD5DDDDDDDDDD",
            "DDDDDDDDDDD5DDDD",
            "DDDDDDDDDDDDDDDD",
            "D5DDDDDDDDDDDDDD",
            "DDDDDDDDDD5DDDDD",
            "DDDDDD5DDDDDDDDD",
            "DD5DDDDDDDDDD5DD",
            "DDDDDDDDDDDDDDDD",
            "DDDDDDDDDDDDDDDD",
            "DDDD5DDDD5DDDDDD",
            "DDDDDDDDDDDDDDDD",
            "DDDDDDDDDDDD5DDD",
            "DDDDDDDDDDDDDDDD"
        };

        static readonly string[] BrickPattern =
        {
            "SSSSSSSSSSSSSSSS",
            "ssssssssSsssssss",
            "ssssssssSsssssss",
            "ssssssssSsssssss",
            "oooooooooooooooo",
            "sssssSssssssssss",
            "sssssSssssssssss",
            "sssssSssssssssss",
            "oooooooooooooooo",
            "SSSSSSSSSSSSSSSS",
            "ssssssssSsssssss",
            "ssssssssSsssssss",
            "ssssssssSsssssss",
            "oooooooooooooooo",
            "sssssSssssssssss",
            "sssssSssssssssss"
        };

        static void BuildTileSet(string theme, Dictionary<char, Color32> topPal,
            Dictionary<char, Color32> fillPal, Dictionary<char, Color32> brickPal)
        {
            AddRows(GroundPattern, topPal, new Vector2(0.5f, 0.5f), "tile_" + theme + "_top");
            AddRows(FillPattern, fillPal, new Vector2(0.5f, 0.5f), "tile_" + theme + "_fill");
            AddRows(BrickPattern, brickPal, new Vector2(0.5f, 0.5f), "tile_" + theme + "_platform");
        }

        // ---------------------------------------------------------------- procedural decor

        static void BuildTree(string key, int seed, string darkHex, string midHex, string lightHex,
            string trunkHex, string trunkDarkHex)
        {
            var rng = new System.Random(seed);
            var c = new PixelCanvas(28, 46);
            Color32 dark = Hex(darkHex), mid = Hex(midHex), light = Hex(lightHex);
            Color32 trunk = Hex(trunkHex), trunkDark = Hex(trunkDarkHex);

            // trunk + outline
            c.Rect(11, 1, 8, 24, trunkDark);
            c.Rect(12, 1, 6, 23, trunk);
            c.Rect(12, 2, 2, 20, Hex("8a5a3a"));
            c.Rect(11, 0, 9, 2, trunkDark);
            c.Rect(9, 0, 3, 1, trunkDark);
            c.Rect(17, 0, 3, 1, trunkDark);

            var blobs = new[]
            {
                new Vector3(14f, 31f, 12f),
                new Vector3(6f, 27f, 7.5f),
                new Vector3(22f, 27f, 7.5f),
                new Vector3(10f, 22f, 7f),
                new Vector3(18f, 22f, 7f),
                new Vector3(14f, 37f, 7f)
            };

            foreach (var b in blobs) c.Disc(b.x, b.y, b.z + 1.4f, dark);
            foreach (var b in blobs) c.Disc(b.x - 0.4f, b.y + 0.5f, b.z, mid);
            foreach (var b in blobs) c.Disc(b.x - 1.4f, b.y + 2.1f, b.z * 0.55f, light);

            // organic leaf noise inside the canopy
            for (int i = 0; i < 140; i++)
            {
                int x = rng.Next(0, 28);
                int y = rng.Next(14, 46);
                bool inside = false;
                foreach (var b in blobs)
                {
                    float dx = x - b.x, dy = y - b.y;
                    if (dx * dx + dy * dy < (b.z - 1.5f) * (b.z - 1.5f)) { inside = true; break; }
                }
                if (!inside) continue;
                Color32 col = rng.Next(3) == 0 ? light : (rng.Next(2) == 0 ? dark : mid);
                if (rng.Next(4) == 0) col = Hex("f7e07a");
                c.Set(x, y, col);
            }

            c.ToSprite(new Vector2(0.5f, 0f), PPU, key);
        }

        static void BuildBush(string key, int seed, string darkHex, string midHex, string lightHex)
        {
            var rng = new System.Random(seed);
            var c = new PixelCanvas(22, 14);
            Color32 dark = Hex(darkHex), mid = Hex(midHex), light = Hex(lightHex);
            var blobs = new[]
            {
                new Vector3(5f, 5f, 4.5f), new Vector3(11f, 7f, 6f),
                new Vector3(17f, 5f, 4.5f), new Vector3(11f, 4f, 5f)
            };
            foreach (var b in blobs) c.Disc(b.x, b.y, b.z + 1f, dark);
            foreach (var b in blobs) c.Disc(b.x, b.y + 0.5f, b.z, mid);
            foreach (var b in blobs) c.Disc(b.x - 0.7f, b.y + 1.4f, b.z * 0.5f, light);
            for (int i = 0; i < 30; i++)
            {
                int x = rng.Next(1, 21), y = rng.Next(1, 11);
                if (rng.Next(3) == 0) c.Set(x, y, light);
            }
            c.Rect(0, 0, 22, 1, dark);
            c.ToSprite(new Vector2(0.5f, 0f), PPU, key);
        }

        static void BuildRock(string key, int seed, string darkHex, string midHex, string lightHex,
            string accentHex)
        {
            var rng = new System.Random(seed);
            var c = new PixelCanvas(18, 12);
            Color32 dark = Hex(darkHex), mid = Hex(midHex), light = Hex(lightHex), accent = Hex(accentHex);
            c.Ellipse(8.5f, 5f, 7.5f, 5f, dark);
            c.Ellipse(8f, 5.5f, 6.3f, 4.2f, mid);
            c.Ellipse(6.3f, 7f, 3.2f, 2.2f, light);
            for (int i = 0; i < 20; i++)
            {
                int x = rng.Next(2, 16), y = rng.Next(2, 9);
                if (rng.Next(2) == 0) c.Set(x, y, dark);
            }
            c.Set(12, 6, accent);
            c.Set(13, 6, accent);
            c.Set(12, 7, accent);
            c.ToSprite(new Vector2(0.5f, 0f), PPU, key);
        }

        static void BuildCrystal(string key, int seed, string darkHex, string midHex,
            string lightHex, string sparkHex)
        {
            var rng = new System.Random(seed);
            var c = new PixelCanvas(22, 34);
            Color32 dark = Hex(darkHex), mid = Hex(midHex), light = Hex(lightHex), spark = Hex(sparkHex);

            DrawShard(c, 6, 2, 5, 18, dark, mid, light);
            DrawShard(c, 15, 2, 6, 25, dark, mid, light);
            DrawShard(c, 10, 2, 7, 13, dark, mid, light);

            for (int i = 0; i < 24; i++)
            {
                int x = rng.Next(0, 22), y = rng.Next(0, 34);
                if (rng.Next(3) == 0) c.Set(x, y, spark);
            }

            c.ToSprite(new Vector2(0.5f, 0f), PPU, key);
        }

        static void DrawShard(PixelCanvas c, int cx, int baseY, int halfWidth, int height,
            Color32 dark, Color32 mid, Color32 light)
        {
            c.Triangle(new Vector2(cx - halfWidth - 1, baseY), new Vector2(cx + halfWidth + 1, baseY),
                new Vector2(cx, baseY + height + 2), dark);
            c.Triangle(new Vector2(cx - halfWidth, baseY), new Vector2(cx + halfWidth, baseY),
                new Vector2(cx, baseY + height), mid);
            c.Triangle(new Vector2(cx - halfWidth + 2, baseY), new Vector2(cx + 1, baseY),
                new Vector2(cx, baseY + height - 3), light);
            c.Set(cx, baseY + height - 1, light);
        }

        static void BuildPillar(string key, int seed)
        {
            var rng = new System.Random(seed);
            var c = new PixelCanvas(18, 46);
            Color32 dark = Hex("5c4632"), mid = Hex("8a6f4a"), light = Hex("c2a678"), moss = Hex("6d8f32");

            c.Rect(1, 0, 16, 4, dark);
            c.Rect(2, 0, 14, 3, mid);
            c.Rect(3, 4, 12, 36, dark);
            c.Rect(4, 4, 10, 35, mid);
            c.Rect(4, 4, 2, 35, light);

            // broken top
            c.Rect(3, 40, 12, 3, dark);
            c.Rect(4, 40, 10, 2, mid);
            c.Rect(4, 31, 10, 1, dark);
            c.Rect(4, 21, 10, 1, dark);
            c.Rect(4, 12, 10, 1, dark);
            c.Rect(0, 42, 18, 2, dark);

            for (int i = 0; i < 16; i++)
            {
                int x = rng.Next(4, 14), y = rng.Next(6, 38);
                c.Set(x, y, rng.Next(2) == 0 ? dark : light);
            }
            for (int i = 0; i < 8; i++) c.Set(rng.Next(2, 8), rng.Next(24, 39), moss);

            c.ToSprite(new Vector2(0.5f, 0f), PPU, key);
        }

        static void BuildCloud(string key, int seed, string lightHex, string shadeHex)
        {
            var rng = new System.Random(seed);
            var c = new PixelCanvas(64, 24);
            Color32 light = Hex(lightHex), shade = Hex(shadeHex);
            c.Ellipse(18, 10, 12, 8, light);
            c.Ellipse(32, 13, 15, 10, light);
            c.Ellipse(47, 10, 12, 8, light);
            c.Rect(8, 3, 49, 7, light);
            c.Rect(8, 3, 49, 2, shade);
            c.Ellipse(18, 7, 10, 4, shade);
            c.Ellipse(46, 7, 9, 4, shade);
            c.Rect(8, 3, 49, 1, shade);
            for (int i = 0; i < 10; i++) c.Set(rng.Next(10, 55), rng.Next(13, 21), light);
            c.ToSprite(new Vector2(0.5f, 0.5f), PPU, key);
        }

        static void BuildSun(string key, string coreHex, string edgeHex)
        {
            int size = 64;
            var c = new PixelCanvas(size, size);
            Color32 core = Hex(coreHex), edge = Hex(edgeHex);
            Vector2 center = new Vector2(size / 2f, size / 2f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center) / (size * 0.5f);
                    if (d > 1f) continue;
                    if (d > 0.52f)
                    {
                        float a = Mathf.Pow(1f - Mathf.InverseLerp(0.52f, 1f, d), 2.2f) * 0.55f;
                        c.Set(x, y, new Color32(edge.r, edge.g, edge.b, (byte)(a * 255f)));
                    }
                    else
                    {
                        c.Set(x, y, d < 0.34f ? core : edge);
                    }
                }
            }
            c.ToSprite(new Vector2(0.5f, 0.5f), PPU, key, FilterMode.Bilinear);
        }

        static void BuildPortal()
        {
            for (int frame = 0; frame < 2; frame++)
            {
                var c = new PixelCanvas(28, 40);
                float phase = frame * Mathf.PI;
                Color32 dark = Hex("2a1b4e"), mid = Hex("7a4fd0"), light = Hex("6ef2d8"),
                    white = Hex("e8ffff");

                for (int y = 0; y < 40; y++)
                {
                    for (int x = 0; x < 28; x++)
                    {
                        float dx = (x + 0.5f - 14f) / 11f;
                        float dy = (y + 0.5f - 19f) / 17f;
                        float d = Mathf.Sqrt(dx * dx + dy * dy);
                        if (d < 0.62f)
                        {
                            float swirl = Mathf.Sin(Mathf.Atan2(dy, dx) * 5f + phase + d * 8f);
                            float a = Mathf.Clamp01(0.20f + 0.16f * swirl) * (1f - d / 0.62f);
                            if (swirl > 0.72f) c.Set(x, y, new Color32(light.r, light.g, light.b, 210));
                            else c.Set(x, y, new Color32(dark.r, dark.g, dark.b, (byte)(a * 255f)));
                        }
                        else if (d < 0.94f)
                        {
                            float t = Mathf.InverseLerp(0.62f, 0.94f, d);
                            Color32 col = t < 0.35f ? light : (t < 0.7f ? mid : dark);
                            byte alpha = (byte)(255f * Mathf.InverseLerp(0.94f, 0.72f, d));
                            c.Set(x, y, new Color32(col.r, col.g, col.b, alpha));
                        }
                        else if (d < 1.06f)
                        {
                            c.Set(x, y, new Color32(mid.r, mid.g, mid.b,
                                (byte)(110f * Mathf.InverseLerp(1.06f, 0.94f, d))));
                        }
                    }
                }

                // sparkles on the ring
                for (int i = 0; i < 7; i++)
                {
                    float ang = i / 7f * Mathf.PI * 2f + phase;
                    int sx = Mathf.RoundToInt(14f + Mathf.Cos(ang) * 11f);
                    int sy = Mathf.RoundToInt(19f + Mathf.Sin(ang) * 17f);
                    c.Set(sx, sy, white);
                    c.Set(sx + 1, sy, white);
                }

                c.ToSprite(new Vector2(0.5f, 0f), PPU, frame == 0 ? "portal_a" : "portal_b");
            }
        }

        static void BuildStars()
        {
            var a = new PixelCanvas(5, 5);
            Color32 w = Hex("ffffff");
            a.Set(2, 0, w); a.Set(2, 1, w); a.Set(1, 2, w); a.Set(2, 2, w); a.Set(3, 2, w);
            a.Set(2, 3, w); a.Set(2, 4, w);
            a.ToSprite(new Vector2(0.5f, 0.5f), PPU, "star_a");

            var b = new PixelCanvas(3, 3);
            b.Set(1, 0, w); b.Set(0, 1, w); b.Set(1, 1, w); b.Set(2, 1, w); b.Set(1, 2, w);
            b.ToSprite(new Vector2(0.5f, 0.5f), PPU, "star_b");
        }

        static void BuildFontSupport()
        {
            // unit sprite used by panels / particles
            Sprites["pixel_unit"] = MakeSolid(Hex("ffffff"), 1f);
        }
    }
}
