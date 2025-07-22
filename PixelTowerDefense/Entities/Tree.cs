using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace PixelTowerDefense.Entities
{
    public struct Tree
    {
        public Vector2 Pos;
        public Point[] TrunkPixels;
        public Point[] LeafPixels;
        public float CollisionRadius;

        public Tree(Vector2 pos, System.Random rng)
        {
            Pos = pos;

            var trunk = new List<Point>();
            int height = rng.Next(12, 20);
            int baseWidth = rng.Next(2, 4); // half-width of the trunk base
            float lean = Utils.RandEx.NextFloat(rng, -0.3f, 0.3f);

            for (int i = 0; i < height; i++)
            {
                float t = i / (float)height;
                int width = (int)MathF.Max(0, MathF.Round(baseWidth * (1f - t)));
                int offset = (int)MathF.Round(lean * i);
                for (int x = -width; x <= width; x++)
                    trunk.Add(new Point(offset + x, -i));

                if (i > height / 3 && i < height - 2 && rng.NextDouble() < 0.15)
                {
                    int dir = rng.NextDouble() < 0.5 ? -1 : 1;
                    int branchLen = rng.Next(2, 5);
                    for (int j = 1; j <= branchLen; j++)
                        trunk.Add(new Point(offset + dir * (width + j), -i - j / 2));
                }
            }
            TrunkPixels = trunk.ToArray();

            var leaves = new List<Point>();
            int radius = rng.Next(4, 7);
            int topOffset = (int)MathF.Round(lean * height);
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    int r = x * x + (y * 2) * (y * 2);
                    if (r <= radius * radius * 4 && rng.NextDouble() > 0.2)
                        leaves.Add(new Point(topOffset + x, -height + y));
                }
            }
            LeafPixels = leaves.ToArray();

            CollisionRadius = baseWidth + 0.5f;
        }
    }
}
