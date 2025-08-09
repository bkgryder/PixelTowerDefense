using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace PixelTowerDefense.Entities
{
    public struct StonePixel
    {
        public Point Offset;
        public Color Color;
    }

    public struct Stone
    {
        public Vector2 Pos;
        public StonePixel[] Pixels;

        public Stone(Vector2 pos, System.Random rng)
        {
            Pos = pos;
            var pixels = new List<StonePixel>();
            int radius = rng.Next(2, 4);
            for (int y = -radius; y <= radius; y++)
                for (int x = -radius; x <= radius; x++)
                    if (x * x + y * y <= radius * radius)
                    {
                        int ox = x + rng.Next(-1, 2);
                        int oy = y + rng.Next(-1, 2);
                        byte baseShade = (byte)rng.Next(110, 170);
                        int var = rng.Next(-20, 21);
                        byte shade = (byte)Math.Clamp(baseShade + var, 0, 255);
                        pixels.Add(new StonePixel
                        {
                            Offset = new Point(ox, oy),
                            Color = new Color(shade, shade, shade)
                        });
                    }
            Pixels = pixels.ToArray();
        }

        public Rectangle Bounds
        {
            get
            {
                int minX = int.MaxValue, maxX = int.MinValue;
                int minY = int.MaxValue, maxY = int.MinValue;
                foreach (var px in Pixels)
                {
                    var p = px.Offset;
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
