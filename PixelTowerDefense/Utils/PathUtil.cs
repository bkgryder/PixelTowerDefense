using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace PixelTowerDefense.Utils
{
    public static class PathUtil
    {
        public static bool FollowPath(ref Vector2 pos, ref Vector2 vel, Queue<Point> path, float speed, float dt)
        {
            if (path == null || path.Count == 0)
            {
                vel = Vector2.Zero;
                return false;
            }

            while (path.Count > 0)
            {
                var cell = path.Peek();
                var target = new Vector2(
                    cell.X * Constants.CELL_PIXELS + Constants.CELL_PIXELS / 2f,
                    cell.Y * Constants.CELL_PIXELS + Constants.CELL_PIXELS / 2f);

                Vector2 diff = target - pos;
                float dist = diff.Length();
                if (dist < 0.1f)
                {
                    pos = target;
                    path.Dequeue();
                    continue;
                }

                if (dist > 0f)
                    diff /= dist;
                else
                    diff = Vector2.Zero;

                vel = diff * speed;
                pos += vel * dt;
                return true;
            }

            vel = Vector2.Zero;
            return false;
        }
    }
}
