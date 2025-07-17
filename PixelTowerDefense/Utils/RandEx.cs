using System;

namespace PixelTowerDefense.Utils
{
    internal static class RandEx
    {
        private static Random R = new();
        public static float NextFloat(this Random _, float min, float max) => min + (float)R.NextDouble() * (max - min);
    }
}
