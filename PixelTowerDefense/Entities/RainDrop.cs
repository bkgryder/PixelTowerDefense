using System;
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

        public Rectangle Bounds => new Rectangle(
            (int)MathF.Round(Pos.X),
            (int)MathF.Round(Pos.Y),
            1,
            1
        );
    }
}
