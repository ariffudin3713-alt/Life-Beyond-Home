using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure
{
    public class Theme
    {
        public string Id;
        public string Title;
        public Color SkyColor;
        public Color Accent;
        public Color Ambient;
        public Color SunTint;
        public string SpritePrefix;
        public string TreeA;
        public string TreeB;
        public string Bush;
        public string Rock;
        public string Mushroom;
        public string Flower;
        public string Crystal;
        public string Pillar;
        public string Cloud;
        public string Sun;
        public string Sky;
        public string HillsFar;
        public string HillsNear;
        public bool HasClouds;
        public bool HasStars;
        public int MusicIndex;
    }

    public struct EntitySpawn
    {
        public char Type;
        public int X;
        public int Row;

        public EntitySpawn(char type, int x, int row)
        {
            Type = type;
            X = x;
            Row = row;
        }
    }

    public class LevelDef
    {
        public string Title;
        public string Subtitle;
        public Theme Theme;
        public int Width;
        public int Height;
        public char[,] Tiles;
        public int SpawnX;
        public int SpawnRow;
        public List<EntitySpawn> Entities = new List<EntitySpawn>();
        public int GemCount;

        public char Get(int x, int row)
        {
            if (x < 0 || row < 0 || x >= Width || row >= Height) return ' ';
            return Tiles[x, row];
        }

        public bool IsSolid(int x, int row)
        {
            char c = Get(x, row);
            return c == '#' || c == '=';
        }
    }

    public class LevelBuilder
    {
        readonly int _width;
        readonly int _height;
        readonly char[,] _tiles;
        readonly List<EntitySpawn> _entities = new List<EntitySpawn>();
        int _spawnX, _spawnRow;
        int _gems;

        public LevelBuilder(int width, int height)
        {
            _width = width;
            _height = height;
            _tiles = new char[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    _tiles[x, y] = ' ';
        }

        public LevelBuilder Ground(int x0, int x1)
        {
            for (int x = x0; x <= x1; x++)
            {
                _tiles[x, 16] = '#';
                _tiles[x, 17] = '#';
            }
            return this;
        }

        public LevelBuilder Raised(int x0, int x1, int topRow)
        {
            for (int x = x0; x <= x1; x++)
                for (int y = topRow; y < _height; y++)
                    _tiles[x, y] = '#';
            return this;
        }

        public LevelBuilder Platform(int x, int row, int width)
        {
            for (int i = 0; i < width; i++) _tiles[x + i, row] = '=';
            return this;
        }

        public LevelBuilder Spawn(int x, int row) { _spawnX = x; _spawnRow = row; return this; }

        public LevelBuilder Gem(int x, int row) { return Add('o', x, row); }
        public LevelBuilder Slime(int x, int row) { return Add('s', x, row); }
        public LevelBuilder Bat(int x, int row) { return Add('f', x, row); }
        public LevelBuilder Spike(int x, int row) { return Add('^', x, row); }
        public LevelBuilder Spring(int x, int row) { return Add('b', x, row); }
        public LevelBuilder Checkpoint(int x, int row) { return Add('C', x, row); }
        public LevelBuilder Goal(int x, int row) { return Add('G', x, row); }
        public LevelBuilder Tree(int x, int row) { return Add('T', x, row); }
        public LevelBuilder Bush(int x, int row) { return Add('B', x, row); }
        public LevelBuilder Rock(int x, int row) { return Add('R', x, row); }
        public LevelBuilder Mushroom(int x, int row) { return Add('M', x, row); }
        public LevelBuilder Flower(int x, int row) { return Add('F', x, row); }
        public LevelBuilder Crystal(int x, int row) { return Add('Y', x, row); }
        public LevelBuilder Pillar(int x, int row) { return Add('P', x, row); }

        LevelBuilder Add(char type, int x, int row)
        {
            _entities.Add(new EntitySpawn(type, x, row));
            if (type == 'o') _gems++;
            return this;
        }

        public LevelDef Build(string title, string subtitle, Theme theme)
        {
            return new LevelDef
            {
                Title = title,
                Subtitle = subtitle,
                Theme = theme,
                Width = _width,
                Height = _height,
                Tiles = _tiles,
                SpawnX = _spawnX,
                SpawnRow = _spawnRow,
                Entities = _entities,
                GemCount = _gems
            };
        }
    }

    public static class Levels
    {
        public static readonly Theme Grove = new Theme
        {
            Id = "grove", Title = "EMERALD GROVE", SpritePrefix = "grove",
            SkyColor = PixelArt.C("8fd4e8"), Accent = PixelArt.C("8be36a"), Ambient = PixelArt.C("d9ff9a"),
            SunTint = Color.white,
            TreeA = "tree_grove_a", TreeB = "tree_grove_b", Bush = "bush_grove",
            Rock = "rock_grove", Mushroom = "mushroom_grove", Flower = "flower_grove",
            Crystal = null, Pillar = null, Cloud = "cloud_grove", Sun = "sun_grove",
            Sky = "sky_grove", HillsFar = "hills_grove_far", HillsNear = "hills_grove_near",
            HasClouds = true, HasStars = false, MusicIndex = 0
        };

        public static readonly Theme Cave = new Theme
        {
            Id = "cave", Title = "CRYSTAL CAVERN", SpritePrefix = "cave",
            SkyColor = PixelArt.C("1a1638"), Accent = PixelArt.C("6ef2d8"), Ambient = PixelArt.C("6ef2d8"),
            SunTint = Color.white,
            TreeA = "tree_cave_a", TreeB = "tree_cave_b", Bush = "bush_cave",
            Rock = "rock_cave", Mushroom = "mushroom_cave", Flower = null,
            Crystal = "crystal_cave_a", Pillar = null, Cloud = "cloud_cave", Sun = "sun_cave",
            Sky = "sky_cave", HillsFar = "hills_cave_far", HillsNear = "hills_cave_near",
            HasClouds = false, HasStars = true, MusicIndex = 1
        };

        public static readonly Theme Ruins = new Theme
        {
            Id = "ruins", Title = "SUNSET RUINS", SpritePrefix = "ruins",
            SkyColor = PixelArt.C("c86a58"), Accent = PixelArt.C("ffcd75"), Ambient = PixelArt.C("ffb347"),
            SunTint = Color.white,
            TreeA = "tree_ruins_a", TreeB = "tree_ruins_b", Bush = "bush_ruins",
            Rock = "rock_ruins", Mushroom = "mushroom_ruins", Flower = "flower_ruins",
            Crystal = "crystal_ruins_a", Pillar = "pillar_ruins", Cloud = "cloud_ruins", Sun = "sun_ruins",
            Sky = "sky_ruins", HillsFar = "ruins_far", HillsNear = "ruins_near",
            HasClouds = true, HasStars = true, MusicIndex = 2
        };

        static LevelDef[] _all;

        public static LevelDef[] All
        {
            get
            {
                if (_all == null) _all = new[] { BuildLevel1(), BuildLevel2(), BuildLevel3() };
                return _all;
            }
        }

        static LevelDef BuildLevel1()
        {
            var b = new LevelBuilder(64, 18);

            // Ground with two gaps.
            b.Ground(0, 22).Ground(26, 44).Ground(48, 63);
            b.Raised(29, 33, 15);

            // Platforms.
            b.Platform(14, 14, 5);   // first ledge
            b.Platform(19, 12, 3);   // high ledge reachable from it
            b.Platform(24, 14, 2);   // stepping stone over the first gap
            b.Platform(36, 14, 5);   // mid ledge
            b.Platform(44, 12, 4);   // spring reward, above second gap
            b.Platform(50, 14, 5);   // ledge before the goal

            b.Spawn(3, 15);

            // Ground path gems.
            b.Gem(4, 15).Gem(6, 15).Gem(28, 15).Gem(55, 15);

            // Ledge gems.
            b.Gem(15, 13).Gem(17, 13);       // above first ledge
            b.Gem(20, 11);                   // high ledge
            b.Gem(24, 13);                   // stepping stone
            b.Gem(37, 13).Gem(39, 13);       // mid ledge
            b.Gem(45, 11).Gem(46, 11);       // spring reward
            b.Gem(51, 13).Gem(52, 13);       // last ledge

            // Enemies and hazards.
            b.Slime(8, 15);
            b.Slime(30, 14);
            b.Slime(53, 13);
            b.Slime(58, 15);
            b.Bat(46, 7);
            b.Spike(11, 15).Spike(12, 15);
            b.Spring(43, 15);
            b.Checkpoint(27, 15);
            b.Goal(60, 15);

            // Scenery.
            b.Tree(2, 15).Tree(22, 15).Tree(48, 15).Tree(62, 15);
            b.Bush(13, 15).Bush(34, 15).Bush(49, 15);
            b.Rock(41, 15).Rock(57, 15);
            b.Mushroom(5, 15).Mushroom(26, 15).Mushroom(40, 15);
            b.Flower(7, 15).Flower(35, 15).Flower(56, 15);

            return b.Build("LEVEL 1", "EMERALD GROVE", Grove);
        }

        static LevelDef BuildLevel2()
        {
            var b = new LevelBuilder(68, 18);

            b.Ground(0, 16).Ground(20, 36).Ground(40, 54).Ground(58, 67);
            b.Raised(31, 34, 15);

            b.Platform(7, 14, 4);
            b.Platform(12, 12, 3);
            b.Platform(18, 14, 2);
            b.Platform(24, 13, 4);
            b.Platform(29, 11, 3);
            b.Platform(34, 14, 2);
            b.Platform(38, 13, 2);
            b.Platform(44, 14, 5);
            b.Platform(50, 12, 3);
            b.Platform(56, 14, 2);
            b.Platform(60, 13, 4);

            b.Spawn(2, 15);

            b.Gem(4, 15).Gem(5, 15);
            b.Gem(8, 13).Gem(10, 13);
            b.Gem(13, 11).Gem(14, 11);
            b.Gem(24, 12).Gem(27, 12);
            b.Gem(30, 10).Gem(31, 10);
            b.Gem(45, 13).Gem(47, 13).Gem(49, 13);
            b.Gem(51, 11).Gem(52, 11);
            b.Gem(62, 12).Gem(63, 12);
            b.Gem(59, 15);

            b.Slime(8, 15);
            b.Slime(25, 12);
            b.Slime(33, 14);
            b.Slime(46, 15);
            b.Slime(61, 12);
            b.Bat(16, 8).Bat(30, 7).Bat(39, 9).Bat(55, 8);
            b.Spike(10, 15).Spike(11, 15);
            b.Spike(27, 15).Spike(28, 15);
            b.Spike(52, 15).Spike(53, 15);
            b.Spike(62, 15).Spike(63, 15);
            b.Spring(15, 15).Spring(42, 15).Spring(58, 15);
            b.Checkpoint(22, 15);
            b.Goal(65, 15);

            b.Tree(6, 15).Tree(33, 15).Tree(49, 15).Tree(66, 15);
            b.Bush(3, 15).Bush(23, 15).Bush(48, 15);
            b.Rock(9, 15).Rock(26, 15).Rock(51, 15);
            b.Mushroom(4, 15).Mushroom(32, 15).Mushroom(47, 15);
            b.Crystal(16, 15).Crystal(43, 15).Crystal(57, 15).Crystal(64, 15);

            return b.Build("LEVEL 2", "CRYSTAL CAVERN", Cave);
        }

        static LevelDef BuildLevel3()
        {
            var b = new LevelBuilder(76, 18);

            b.Ground(0, 10).Ground(14, 24).Ground(28, 41).Ground(45, 58).Ground(62, 75);
            b.Raised(20, 23, 15);

            b.Platform(5, 14, 4);
            b.Platform(9, 12, 2);
            b.Platform(16, 14, 3);
            b.Platform(20, 12, 4);
            b.Platform(29, 14, 4);
            b.Platform(34, 12, 3);
            b.Platform(38, 14, 2);
            b.Platform(45, 14, 4);
            b.Platform(50, 11, 4);
            b.Platform(54, 14, 3);
            b.Platform(62, 13, 4);
            b.Platform(67, 11, 3);
            b.Platform(71, 14, 3);

            b.Spawn(2, 15);

            b.Gem(3, 15).Gem(6, 13).Gem(7, 13);
            b.Gem(10, 11).Gem(11, 11);
            b.Gem(17, 13).Gem(18, 13);
            b.Gem(21, 11).Gem(22, 11);
            b.Gem(29, 13).Gem(31, 13);
            b.Gem(35, 11).Gem(36, 11);
            b.Gem(37, 13).Gem(39, 13);
            b.Gem(46, 13).Gem(48, 13);
            b.Gem(51, 10).Gem(52, 10);
            b.Gem(55, 13).Gem(56, 13);
            b.Gem(63, 12).Gem(65, 12);
            b.Gem(68, 10).Gem(69, 10);
            b.Gem(73, 15);

            b.Slime(4, 15);
            b.Slime(16, 15);
            b.Slime(30, 15);
            b.Slime(38, 13);
            b.Slime(47, 15);
            b.Slime(55, 15);
            b.Slime(64, 12);
            b.Slime(72, 15);

            b.Bat(13, 8).Bat(25, 7).Bat(33, 8).Bat(43, 9).Bat(50, 7).Bat(59, 8).Bat(70, 8);

            b.Spike(8, 15).Spike(9, 15);
            b.Spike(31, 15).Spike(32, 15);
            b.Spike(39, 15).Spike(40, 15);
            b.Spike(56, 15).Spike(57, 15);
            b.Spike(64, 15).Spike(65, 15);

            b.Spring(24, 15).Spring(49, 15).Spring(58, 15);
            b.Checkpoint(46, 15);
            b.Goal(74, 15);

            b.Tree(1, 15).Tree(23, 15).Tree(52, 15).Tree(75, 15);
            b.Bush(3, 15).Bush(19, 15).Bush(53, 15).Bush(66, 15);
            b.Rock(7, 15).Rock(17, 15).Rock(48, 15).Rock(61, 15);
            b.Mushroom(2, 15).Mushroom(29, 15).Mushroom(57, 15);
            b.Flower(6, 15).Flower(18, 15).Flower(54, 15).Flower(70, 15);
            b.Pillar(12, 15).Pillar(26, 15).Pillar(44, 15).Pillar(73, 15);
            b.Crystal(60, 15).Crystal(67, 15);

            return b.Build("LEVEL 3", "SUNSET RUINS", Ruins);
        }
    }
}
