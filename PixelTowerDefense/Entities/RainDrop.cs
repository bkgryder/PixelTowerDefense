using Microsoft.Xna.Framework;

namespace PixelTowerDefense.Entities
{
    public struct RainDrop
    {
        public Vector2 Pos;
        public float z;
        public float vz;

        public RainDrop(Vector2 pos, float height, float speed)
        {
            Pos = pos;
            z = height;
            vz = speed;
        }
    }
}
