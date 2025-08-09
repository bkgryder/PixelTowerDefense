using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace PixelTowerDefense.Entities
{
    public struct Wood
    {
        public Vector2 Pos;
        public Vector2 Vel;
        public bool IsCarried;
        public Point[] Shape;
        public Color Color;
        public int? ReservedBy;

        public Wood(Vector2 pos, System.Random rng)
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

        public Rectangle Bounds
        {
            get
            {
                int minX = int.MaxValue, maxX = int.MinValue;
                int minY = int.MaxValue, maxY = int.MinValue;
                foreach (var p in Shape)
                {
                    if (p.X < minX) minX = p.X;
                    if (p.X > maxX) maxX = p.X;
                    if (p.Y < minY) minY = p.Y;
                    if (p.Y > maxY) maxY = p.Y;
                }
                return new Rectangle(
                    (int)MathF.Round(Pos.X) + minX,
                    (int)MathF.Round(Pos.Y) + minY,
                    maxX - minX + 1,
                    maxY - minY + 1);
            }
        }
    }
}
