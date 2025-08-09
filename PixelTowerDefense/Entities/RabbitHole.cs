using System;
using Microsoft.Xna.Framework;

namespace PixelTowerDefense.Entities
{
    public struct RabbitHole
    {
        public Vector2 Pos;
        public float VacantTimer;

        public Rectangle Bounds => new Rectangle(
            (int)MathF.Round(Pos.X) - 1,
            (int)MathF.Round(Pos.Y) - 1,
            2,
            2
        );
    }
}
