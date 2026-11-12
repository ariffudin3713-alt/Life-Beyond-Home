using UnityEngine;

namespace PixelAdventure
{
    public static class GameLayers
    {
        public const int Ground = 8;
        public const int Player = 9;
        public const int Enemy = 10;       // trigger hitboxes
        public const int Pickup = 11;
        public const int Hazard = 12;
        public const int EnemyBody = 13;   // solid body, ignored by player

        public static int GroundMask => 1 << Ground;
    }
}
