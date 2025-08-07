using System;

namespace PixelTowerDefense.Utils
{
    public static class Noise
    {
        static float Hash(int x, int y, int seed)
        {
            int h = x;
            h = h * 374761393 + y * 668265263 + seed * 1013904223;
            h = (h ^ (h >> 13)) * 1274126177;
            return (h & 0x7fffffff) / (float)int.MaxValue;
        }

        static float ValueNoise(float x, float y, int seed)
        {
            int xi = (int)MathF.Floor(x);
            int yi = (int)MathF.Floor(y);
            float xf = x - xi;
            float yf = y - yi;
            float v00 = Hash(xi, yi, seed);
            float v10 = Hash(xi + 1, yi, seed);
            float v01 = Hash(xi, yi + 1, seed);
            float v11 = Hash(xi + 1, yi + 1, seed);
            float i1 = v00 + (v10 - v00) * xf;
            float i2 = v01 + (v11 - v01) * xf;
            return i1 + (i2 - i1) * yf;
        }

        public static float FractalNoise(float x, float y, int seed)
        {
            float sum = 0f;
            float amp = 1f;
            float freq = 1f;
            float norm = 0f;
            for (int i = 0; i < 4; i++)
            {
                sum += ValueNoise(x * freq, y * freq, seed + i) * amp;
                norm += amp;
                amp *= 0.5f;
                freq *= 2f;
            }
            return sum / norm;
        }
    }
}
