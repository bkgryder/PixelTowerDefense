using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace PixelTowerDefense.Entities
{
    public struct Log
    {
        public Vector2 Pos;
        public Vector2 Vel;
        public bool IsCarried;
        public Point[] Shape;
        public Color Color;
        public int? ReservedBy;

        public Log(Vector2 pos, System.Random rng)
        {
            Pos = pos;
            Vel = Vector2.Zero;
            IsCarried = false;
            ReservedBy = null;
            Color = new Color(
                (byte)rng.Next(90, 130),
                (byte)rng.Next(60, 90),
                (byte)rng.Next(40, 70));
            var shape = new List<Point>();
            int length = rng.Next(4, 7);
            for (int x = -length / 2; x < length / 2; x++)
            {
                int offset = rng.Next(-1, 0);
                shape.Add(new Point(x, offset));
                shape.Add(new Point(x, offset + 1));
            }
            Shape = shape.ToArray();
        }
    }
}
