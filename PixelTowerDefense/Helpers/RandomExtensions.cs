using System;

namespace PixelTowerDefense.Helpers
{
    internal static class RandomExtensions
    {
        public static float NextFloat(this Random rng, float min, float max)
        {
            return min + (float)rng.NextDouble() * (max - min);
        }
    }
}
