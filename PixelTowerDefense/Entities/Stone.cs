using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace PixelTowerDefense.Entities
{
    public struct Stone
    {
        public Vector2 Pos;
        public Point[] Shape;
        public Color Color;

        public Stone(Vector2 pos, System.Random rng)
        {
            Pos = pos;
            byte shade = (byte)rng.Next(100, 200);
            Color = new Color(shade, shade, shade);
            var shape = new List<Point>();
            for (int y = -3; y <= 3; y++)
                for (int x = -3; x <= 3; x++)
                {
                    int r = x * x + y * y;
                    if (r <= 9 && rng.NextDouble() > 0.1)
                        shape.Add(new Point(x + rng.Next(-1, 2), y + rng.Next(-1, 2)));
                }
            Shape = shape.ToArray();
        }
    }
}
