using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace PixelTowerDefense.Entities
{
    public struct BerryBush
    {
        public Vector2 Pos;
        public int Berries;
        public Point[] Shape;
        public Point[] BerryPixels;

        public BerryBush(Vector2 pos, System.Random rng)
        {
            Pos = pos;
            Berries = Utils.Constants.BUSH_BERRIES;

            var shapeList = new List<Point>();
            for (int y = -3; y < 3; y++)
                for (int x = -3; x < 3; x++)
                {
                    if (x * x + y * y <= 8 && rng.NextDouble() > 0.1)
                        shapeList.Add(new Point(x, y));
                }
            Shape = shapeList.ToArray();
            var berryList = new List<Point>();
            for (int i = 0; i < Utils.Constants.BUSH_BERRIES && shapeList.Count > 0; i++)
            {
                int idx = rng.Next(shapeList.Count);
                berryList.Add(shapeList[idx]);
                shapeList.RemoveAt(idx);
            }
            BerryPixels = berryList.ToArray();
        }
    }
}
