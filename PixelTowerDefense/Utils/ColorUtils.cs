using System;
using Microsoft.Xna.Framework;

namespace PixelTowerDefense.Utils
{
    public static class ColorUtils
    {
        public static void ToHsv(Color color, out float h, out float s, out float v)
        {
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;
            float max = MathF.Max(r, MathF.Max(g, b));
            float min = MathF.Min(r, MathF.Min(g, b));
            v = max;
            float delta = max - min;
            if (delta <= 0f)
            {
                h = 0f;
                s = 0f;
                return;
            }
            s = delta / max;
            if (r >= max)
                h = ((g - b) / delta) % 6f;
            else if (g >= max)
                h = ((b - r) / delta) + 2f;
            else
                h = ((r - g) / delta) + 4f;
            h *= 60f;
            if (h < 0f)
                h += 360f;
        }

        public static Color FromHsv(float h, float s, float v)
        {
            h = h % 360f;
            if (h < 0f)
                h += 360f;
            double c = v * s;
            double hh = h / 60.0;
            double x = c * (1 - Math.Abs(hh % 2 - 1));
            double r1 = 0, g1 = 0, b1 = 0;
            if (hh >= 0 && hh < 1)
            { r1 = c; g1 = x; }
            else if (hh < 2)
            { r1 = x; g1 = c; }
            else if (hh < 3)
            { g1 = c; b1 = x; }
            else if (hh < 4)
            { g1 = x; b1 = c; }
            else if (hh < 5)
            { r1 = x; b1 = c; }
            else
            { r1 = c; b1 = x; }
            double m = v - c;
            byte r = (byte)Math.Round((r1 + m) * 255);
            byte g = (byte)Math.Round((g1 + m) * 255);
            byte b = (byte)Math.Round((b1 + m) * 255);
            return new Color(r, g, b, (byte)255);
        }

        public static Color AdjustColor(Color color, float hueShiftDeg, float satMul = 1f, float valMul = 1f)
        {
            ToHsv(color, out float h, out float s, out float v);
            h += hueShiftDeg;
            s = Math.Clamp(s * satMul, 0f, 1f);
            v = Math.Clamp(v * valMul, 0f, 1f);
            return FromHsv(h, s, v);
        }
    }
}
