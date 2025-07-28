using Microsoft.Xna.Framework;

namespace PixelTowerDefense.Entities
{
    public struct Wolf
    {
        public Vector2 Pos;
        public Vector2 Vel;
        public float z;
        public float vz;
        public float WanderTimer;
        public float ShadowY;

        // survival & lifecycle
        public float Hunger;
        public float FullTimer;
        public float Age;
        public float GrowthDuration;

        // home reference (-1 = none)
        public int HomeId;
    }
}
