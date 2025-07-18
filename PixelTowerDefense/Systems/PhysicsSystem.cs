using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PixelTowerDefense.Entities;
using PixelTowerDefense.Utils;

namespace PixelTowerDefense.Systems
{
    public static class PhysicsSystem
    {
        private static Random _rng = new Random();

        public static void SimulateAll(
            List<Enemy> enemies,
            List<Pixel> debris,
            float dt
        )
        {
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                var e = enemies[i];

                switch (e.State)
                {
                    case EnemyState.Walking:
                        // wander
                        e.WanderTimer -= dt;
                        if (e.WanderTimer <= 0f)
                        {
                            e.WanderTimer = _rng.NextFloat(1f, 3f);
                            float ang = MathHelper.ToRadians(_rng.Next(360));
                            e.Vel = new Vector2(
                                MathF.Cos(ang),
                                MathF.Sin(ang)
                            ) * Constants.WANDER_SPEED;
                        }
                        e.Pos += e.Vel * dt;
                        e.Angle = 0f;
                        break;

                    case EnemyState.Launched:
                        // vertical
                        e.vz -= Constants.Z_GRAVITY * dt;
                        e.z += e.vz * dt;

                        if (e.z <= 0f)
                        {
                            float impact = MathF.Abs(e.vz);
                            e.z = 0f;
                            e.vz = 0f;

                            if (impact > Constants.EXPLODE_VZ_THRESHOLD)
                            {
                                ExplodeEnemy(e, debris);
                                enemies.RemoveAt(i);
                                continue;
                            }
                            else if (impact > Constants.STUN_VZ_THRESHOLD)
                            {
                                e.State = EnemyState.Stunned;
                                e.StunTimer = Constants.STUN_TIME;
                                e.Angle = MathHelper.PiOver2;
                                e.Vel = Vector2.Zero;
                            }
                            else
                            {
                                e.State = EnemyState.Walking;
                                e.WanderTimer = _rng.NextFloat(0.5f, 2.5f);
                                float ang = MathHelper.ToRadians(_rng.Next(360));
                                e.Vel = new Vector2(
                                                  MathF.Cos(ang),
                                                  MathF.Sin(ang)
                                              ) * Constants.WANDER_SPEED;
                                e.Angle = 0f;
                            }
                        }
                        else
                        {
                            // planar + friction
                            e.Pos += e.Vel * dt;
                            e.Vel *= MathF.Max(0f, 1f - Constants.FRICTION * dt);
                        }

                        e.Angle += e.AngularVel * dt;
                        e.AngularVel *= MathF.Exp(-Constants.ANGULAR_DAMPING * dt);
                        break;

                    case EnemyState.Stunned:
                        e.Angle += e.AngularVel * dt;
                        e.AngularVel *= MathF.Exp(-Constants.ANGULAR_DAMPING * dt);

                        e.StunTimer -= dt;
                        if (e.StunTimer <= 0f)
                        {
                            e.State = EnemyState.Walking;
                            e.WanderTimer = _rng.NextFloat(0.5f, 2.5f);
                            float ang = MathHelper.ToRadians(_rng.Next(360));
                            e.Vel = new Vector2(
                                               MathF.Cos(ang),
                                               MathF.Sin(ang)
                                           ) * Constants.WANDER_SPEED;
                            e.Angle = 0f;
                        }
                        break;
                }

                // clamp inside arena
                e.Pos.X = MathHelper.Clamp(e.Pos.X,
                           Constants.ARENA_LEFT + 2,
                           Constants.ARENA_RIGHT - 2);
                e.Pos.Y = MathHelper.Clamp(e.Pos.Y,
                           Constants.ARENA_TOP + 2,
                           Constants.ARENA_BOTTOM - 2);

                enemies[i] = e;
            }
        }

        private static void ExplodeEnemy(Enemy e, List<Pixel> debris)
        {
            // center of the ragdoll
            Vector2 center = e.Pos;

            for (int part = -2; part <= 2; part++)
            {
                Vector2 pos = e.GetPartPos(part);

                // pick blood color per part
                Color c = part switch
                {
                    -2 => new Color(255, 80, 90),
                    -1 => new Color(220, 40, 40),
                    0 => new Color(200, 0, 0),
                    1 => new Color(150, 0, 0),
                    2 => new Color(100, 0, 0),
                    _ => Color.Red
                };

                // compute a direction away from the ragdoll center
                Vector2 dir = pos - center;
                if (dir == Vector2.Zero)
                {
                    // fallback to random
                    dir = new Vector2(
                        _rng.NextFloat(-1f, 1f),
                        _rng.NextFloat(-1f, 1f)
                    );
                }
                dir.Normalize();

                // pick a random magnitude in [min, max]
                float mag = _rng.NextFloat(
                    Constants.EXPLOSION_FORCE_MIN,
                    Constants.EXPLOSION_FORCE_MAX
                );

                Vector2 vel = dir * mag;

                debris.Add(new Pixel(pos, vel, c));
            }
        }
        public static void UpdatePixels(List<Pixel> debris, float dt)
        {
            for (int i = debris.Count - 1; i >= 0; i--)
            {
                var p = debris[i];

                // much gentler slow-down
                p.Vel *= MathF.Max(0f, 1f - Constants.DEBRIS_FRICTION * dt);
                p.Pos += p.Vel * dt;

                // clamp to arena, bounce lightly
                if (p.Pos.X < Constants.ARENA_LEFT) { p.Pos.X = Constants.ARENA_LEFT; p.Vel.X *= -0.5f; }
                if (p.Pos.X > Constants.ARENA_RIGHT - 1) { p.Pos.X = Constants.ARENA_RIGHT - 1; p.Vel.X *= -0.5f; }
                if (p.Pos.Y < Constants.ARENA_TOP) { p.Pos.Y = Constants.ARENA_TOP; p.Vel.Y *= -0.5f; }
                if (p.Pos.Y > Constants.ARENA_BOTTOM - 1) { p.Pos.Y = Constants.ARENA_BOTTOM - 1; p.Vel.Y *= -0.5f; }

                debris[i] = p;
            }
        }


    }
}
