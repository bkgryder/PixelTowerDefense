using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PixelTowerDefense.Entities;

namespace PixelTowerDefense.Utils
{
    public static class PixelPool
    {
        public static void Spawn(this List<Pixel> pixels, Pixel pixel)
        {
            if (pixels.Count >= Constants.MAX_DEBRIS && pixels.Count > 0)
            {
                var old = pixels[0];
                old.Lifetime = 0f;
                pixels[0] = old;
            }
            pixels.Add(pixel);
        }
    }
}
