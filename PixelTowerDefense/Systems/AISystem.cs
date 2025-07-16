using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PixelTowerDefense.Components;
using PixelTowerDefense.Utils;

namespace PixelTowerDefense.Systems
{
    public class AISystem
    {
        public void Update(float dt, List<Enemy> enemies, System.Random rng)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                var e = enemies[i];
                switch (e.State)
                {
                    case EnemyState.Walking:
                        e.WanderTimer -= dt;
                        if (e.WanderTimer <= 0)
                        {
                            e.Dir = rng.NextFloat(-1f, 1f) < 0 ? -1 : 1;
                            e.WanderTimer = rng.NextFloat(1f, 3f);
                        }
                        break;
                    case EnemyState.Stunned:
                        e.StunTimer -= dt;
                        if (e.StunTimer <= 0)
                        {
                            e.State = EnemyState.Walking;
                            e.Angle = 0f;
                            e.AngularVel = 0f;
                            e.WanderTimer = rng.NextFloat(0.5f, 2.5f);
                            e.Dir = rng.NextFloat(-1f, 1f) < 0 ? -1 : 1;
                        }
                        break;
                }
                enemies[i] = e;
            }
        }
    }
}
