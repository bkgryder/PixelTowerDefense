using System;
using Microsoft.Xna.Framework;

namespace PixelTowerDefense.Entities
{
    public struct Light
    {
        public Vector2 Pos;
        public float Radius;
        public float Intensity;
        public float Lifetime;

        public Light(Vector2 pos, float radius, float intensity, float lifetime)
        {
            Pos = pos;
            Radius = radius;
            Intensity = intensity;
            Lifetime = lifetime;
        }

        public Rectangle Bounds => new Rectangle(
            (int)MathF.Round(Pos.X - Radius),
            (int)MathF.Round(Pos.Y - Radius),
            (int)MathF.Ceiling(Radius * 2),
            (int)MathF.Ceiling(Radius * 2)
        );
    }
}
