using PixelTowerDefense.Utils;

namespace PixelTowerDefense.World
{
    public static class GroundGenerator
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
            int xi = (int)System.MathF.Floor(x);
            int yi = (int)System.MathF.Floor(y);
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

        static float FractalNoise(float x, float y, int seed)
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

        public static GroundMap Generate(int worldWidthPx, int worldHeightPx, int cellPixels = Constants.CELL_PIXELS)
        {
            int w = worldWidthPx / cellPixels;
            int h = worldHeightPx / cellPixels;
            var map = new GroundMap(w, h);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float nx = x / (float)w;
                    float ny = y / (float)h;

                    float temp = FractalNoise(nx * 2f, ny * 2f, 1);
                    float moist = FractalNoise(nx * 2f + 100f, ny * 2f + 100f, 2);

                    Biome biome;
                    if (temp < 0.3f)
                        biome = moist < 0.5f ? Biome.Rock : Biome.Snow;
                    else if (temp > 0.7f)
                        biome = moist > 0.6f ? Biome.Marsh : Biome.Sand;
                    else
                        biome = moist > 0.6f ? Biome.Forest : (moist < 0.3f ? Biome.Meadow : Biome.Grass);

                    byte variant = (byte)(Hash(x, y, 3) % 4);

                    map.Cells[x, y] = new GroundCell
                    {
                        Biome = biome,
                        Moisture = (byte)(moist * 255),
                        Fertility = (byte)(temp * 255),
                        Variant = variant
                    };
                }
            }

            return map;
        }
    }
}
