using System;

namespace PixelTowerDefense.Helpers
{
    /// <summary>
    /// Utility extensions for <see cref="Random"/>.
    /// </summary>
    internal static class RandomExtensions
    {
        public static float NextFloat(this Random rng, float min, float max)
        {
            return min + (float)rng.NextDouble() * (max - min);
        }
    }
}
