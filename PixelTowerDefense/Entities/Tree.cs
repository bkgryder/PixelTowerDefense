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
            for (int i = 0; i < height; i++)
            {
                trunk.Add(new Point(-5, -i));
                if (rng.NextDouble() < 0.3)
                    trunk.Add(new Point(rng.Next(-1, 2), -i));
            }
            TrunkPixels = trunk.ToArray();

            var leaves = new List<Point>();
            int radius = rng.Next(4, 8);
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    int r = x * x + y * y;
                    if (r <= radius * radius && rng.NextDouble() > 0.2)
                        leaves.Add(new Point(x, -height + y));
                }
            }
            LeafPixels = leaves.ToArray();

            CollisionRadius = 1.5f;
        }
    }
}
