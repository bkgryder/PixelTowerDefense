using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PixelTowerDefense.Utils
{
    public static class Perlin
    {
        static int[] _perm = new int[512];

        static void Init(int seed)
        {
            var rand = new Random(seed);
            int[] p = new int[256];
            for (int i = 0; i < 256; i++)
                p[i] = i;
            for (int i = 255; i > 0; i--)
            {
                int swap = rand.Next(i + 1);
                int tmp = p[i];
                p[i] = p[swap];
                p[swap] = tmp;
            }
            for (int i = 0; i < 512; i++)
                _perm[i] = p[i & 255];
        }

        static float Fade(float t) => t * t * t * (t * (t * 6f - 15f) + 10f);

        static float Grad(int hash, float x, float y)
        {
            int h = hash & 3;
            float u = h < 2 ? x : y;
            float v = h < 2 ? y : x;
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }

        static float Noise(float x, float y)
        {
            int xi = (int)MathF.Floor(x) & 255;
            int yi = (int)MathF.Floor(y) & 255;
            float xf = x - MathF.Floor(x);
            float yf = y - MathF.Floor(y);

            int aa = _perm[_perm[xi] + yi];
            int ab = _perm[_perm[xi] + yi + 1];
            int ba = _perm[_perm[xi + 1] + yi];
            int bb = _perm[_perm[xi + 1] + yi + 1];

            float u = Fade(xf);
            float v = Fade(yf);

            float x1 = MathHelper.Lerp(Grad(aa, xf, yf), Grad(ba, xf - 1f, yf), u);
            float x2 = MathHelper.Lerp(Grad(ab, xf, yf - 1f), Grad(bb, xf - 1f, yf - 1f), u);
            return MathHelper.Lerp(x1, x2, v);
        }

        public static Texture2D GenerateTexture(GraphicsDevice device, int width, int height, int seed, float scale, int octaves, int pixelSize)
        {
            Init(seed);
            var data = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int px = x / pixelSize;
                    int py = y / pixelSize;
                    float nx = px / (float)(width / pixelSize) * scale;
                    float ny = py / (float)(height / pixelSize) * scale;
                    float amp = 1f;
                    float freq = 1f;
                    float sum = 0f;
                    float norm = 0f;
                    for (int o = 0; o < octaves; o++)
                    {
                        sum += (Noise(nx * freq, ny * freq) + 1f) * 0.5f * amp;
                        norm += amp;
                        amp *= 0.5f;
                        freq *= 2f;
                    }
                    float val = sum / norm;
                    byte a = (byte)(val * 255);
                    data[y * width + x] = new Color((byte)0, (byte)0, (byte)0, a);
                }
            }
            var tex = new Texture2D(device, width, height);
            tex.SetData(data);
            return tex;
        }
    }
}
