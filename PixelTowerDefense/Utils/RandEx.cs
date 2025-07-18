using System;

namespace PixelTowerDefense.Utils
{
    public static class RandEx
    {
        private static Random _r = new Random();
        public static float NextFloat(this Random _, float min, float max)
            => min + (float)_r.NextDouble() * (max - min);
    }
}
