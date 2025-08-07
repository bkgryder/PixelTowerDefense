using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PixelTowerDefense.Entities;
using PixelTowerDefense.Utils;

namespace PixelTowerDefense.Systems
{
    public static class ShadowSystem
    {
        /// <summary>
        /// Update shadow positions for all meeples.
        /// The shadow follows the lowest body part projected on the ground.
        /// </summary>
        public static void UpdateShadows(List<Meeple> meeples, float dt)
        {
            for (int i = 0; i < meeples.Count; i++)
            {
                var e = meeples[i];
                int halfSeg = Constants.ENEMY_H / 2;
                float bottom = float.MinValue;
                for (int p = -halfSeg; p < halfSeg; p++)
                {
                    bottom = MathF.Max(bottom, e.GetPartPos(p).Y);
                }

                float target = bottom + 1f;
                float lerp = MathHelper.Clamp(48f * dt, 0f, 1f);
                e.ShadowY = MathHelper.Lerp(e.ShadowY, target, lerp);
                meeples[i] = e;
            }
        }
    }
}
