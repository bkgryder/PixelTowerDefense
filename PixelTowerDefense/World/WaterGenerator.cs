using System;
using System.Collections.Generic;

namespace PixelTowerDefense.World
{
    public static class WaterGenerator
    {
        public static WaterMap Generate(int width, int height, int riverCount, int lakeCount, Random rng)
        {
            var map = new WaterMap(width, height);
            if (width <= 0 || height <= 0)
                return map;

            // helper local function to apply depth/flow safely
            void SetCell(int x, int y, byte depth, sbyte fx, sbyte fy)
            {
                if (x < 0 || x >= width || y < 0 || y >= height)
                    return;
                if (depth > map.Depth[x, y])
                {
                    map.Depth[x, y] = depth;
                    map.FlowX[x, y] = fx;
                    map.FlowY[x, y] = fy;
                }
            }

            // ----- Rivers -----
            for (int r = 0; r < riverCount; r++)
            {
                bool horizontal = rng.NextDouble() < 0.5;
                int x, y, target;
                int dir;
                if (horizontal)
                {
                    y = rng.Next(height);
                    if (rng.NextDouble() < 0.5)
                    {
                        x = 0;
                        target = width - 1;
                        dir = 1;
                    }
                    else
                    {
                        x = width - 1;
                        target = 0;
                        dir = -1;
                    }
                }
                else
                {
                    x = rng.Next(width);
                    if (rng.NextDouble() < 0.5)
                    {
                        y = 0;
                        target = height - 1;
                        dir = 1;
                    }
                    else
                    {
                        y = height - 1;
                        target = 0;
                        dir = -1;
                    }
                }

                int steps = horizontal ? Math.Abs(target - x) : Math.Abs(target - y);
                int maxWidth = rng.Next(2, 5);
                int prevX = x;
                int prevY = y;

                for (int step = 0; step <= steps; step++)
                {
                    // bias movement toward target with some jitter
                    if (horizontal)
                    {
                        x += dir;
                        if (rng.NextDouble() < 0.4)
                            y += rng.Next(-1, 2);
                    }
                    else
                    {
                        y += dir;
                        if (rng.NextDouble() < 0.4)
                            x += rng.Next(-1, 2);
                    }

                    x = Math.Clamp(x, 0, width - 1);
                    y = Math.Clamp(y, 0, height - 1);

                    int w = 1 + (int)((step / (float)steps) * maxWidth);
                    float centerDepth = 1f;

                    int dx = x - prevX;
                    int dy = y - prevY;
                    prevX = x;
                    prevY = y;
                    float len = MathF.Max(1f, MathF.Sqrt(dx * dx + dy * dy));
                    sbyte fx = (sbyte)Math.Clamp((int)MathF.Round(dx / len * 2f), -2, 2);
                    sbyte fy = (sbyte)Math.Clamp((int)MathF.Round(dy / len * 2f), -2, 2);

                    for (int oy = -w; oy <= w; oy++)
                    {
                        for (int ox = -w; ox <= w; ox++)
                        {
                            int cx = x + ox;
                            int cy = y + oy;
                            float dist = MathF.Sqrt(ox * ox + oy * oy);
                            if (dist > w)
                                continue;
                            float t = 1f - dist / (w + 1f);
                            byte depth = (byte)Math.Clamp((int)(255 * t * t * centerDepth), 0, 255);
                            SetCell(cx, cy, depth, fx, fy);
                        }
                    }

                    if ((horizontal && x == target) || (!horizontal && y == target))
                        break;
                }
            }

            // ----- Lakes -----
            for (int i = 0; i < lakeCount; i++)
            {
                int cx = rng.Next(width);
                int cy = rng.Next(height);
                int rx = rng.Next(3, 8);
                int ry = rng.Next(3, 8);

                for (int y0 = cy - ry; y0 <= cy + ry; y0++)
                {
                    for (int x0 = cx - rx; x0 <= cx + rx; x0++)
                    {
                        float nx = (x0 - cx) / (float)rx;
                        float ny = (y0 - cy) / (float)ry;
                        float distSq = nx * nx + ny * ny;
                        if (distSq <= 1f)
                        {
                            float t = 1f - MathF.Sqrt(distSq);
                            byte depth = (byte)Math.Clamp((int)(255 * t * t), 0, 255);
                            SetCell(x0, y0, depth, 0, 0);
                        }
                    }
                }
            }

            return map;
        }
    }
}
