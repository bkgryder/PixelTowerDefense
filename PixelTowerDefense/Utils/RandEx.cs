using System;

namespace PixelTowerDefense.Utils
{
    public static class RandEx
    {
        public static float NextFloat(this Random _, float min, float max)
            => min + (float)_.NextDouble() * (max - min);
    }
}
