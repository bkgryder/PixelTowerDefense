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
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"Pixel pool reached limit {Constants.MAX_DEBRIS}. Removing oldest pixel.");
#endif
                pixels.RemoveAt(0);
            }

            pixels.Add(pixel);

#if DEBUG
            System.Diagnostics.Debug.WriteLine($"Pixel pool size after spawn: {pixels.Count}");
#endif
        }
    }
}
