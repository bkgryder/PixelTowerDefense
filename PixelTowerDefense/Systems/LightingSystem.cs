using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PixelTowerDefense.Entities;

namespace PixelTowerDefense.Systems
{
    public static class LightingSystem
    {
        public static void AddLight(List<Light> lights, Vector2 pos, float radius, float intensity, float lifetime)
        {
            lights.Add(new Light(pos, radius, intensity, lifetime));
        }

        public static void Update(List<Light> lights, float dt)
        {
            for (int i = lights.Count - 1; i >= 0; i--)
            {
                var l = lights[i];
                l.Lifetime -= dt;
                if (l.Lifetime <= 0f)
                    lights.RemoveAt(i);
                else
                    lights[i] = l;
            }
        }

        public static void DrawLights(SpriteBatch sb, Texture2D px, List<Light> lights, Matrix cam)
        {
            sb.Begin(transformMatrix: cam, blendState: BlendState.Additive, samplerState: SamplerState.PointClamp);
            foreach (var l in lights)
            {
                int r = (int)MathF.Ceiling(l.Radius);
                int cx = (int)MathF.Round(l.Pos.X);
                int cy = (int)MathF.Round(l.Pos.Y);
                for (int y = -r; y <= r; y++)
                {
                    for (int x = -r; x <= r; x++)
                    {
                        float dist = MathF.Sqrt(x * x + y * y);
                        if (dist > l.Radius)
                            continue;
                        float t = 1f - dist / l.Radius;
                        byte val = (byte)Math.Clamp(t * l.Intensity * 255f, 0f, 255f);
                        var col = new Color(val, val, val);
                        sb.Draw(px, new Rectangle(cx + x, cy + y, 1, 1), col);
                    }
                }
            }
            sb.End();
        }
    }
}
