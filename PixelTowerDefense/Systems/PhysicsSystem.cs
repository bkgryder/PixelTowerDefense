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
                            e.Vel = new Vector2(MathF.Cos(ang), MathF.Sin(ang))
                                         * Constants.WANDER_SPEED;
                        }
                        e.Pos += e.Vel * dt;
                        e.Angle = 0f;
                        break;

                    case EnemyState.Launched:
                        // vertical flight
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
                                // go stunned (flat on ground)
                                e.State = EnemyState.Stunned;
                                e.StunTimer = Constants.STUN_TIME;
                                e.Angle = MathHelper.PiOver2; // lay flat
                                e.Vel = Vector2.Zero;
                                e.AngularVel = 0f;
                            }
                            else
                            {
                                // gentle landing → resume Walking
                                e.State = EnemyState.Walking;
                                e.WanderTimer = _rng.NextFloat(0.5f, 2.5f);
                                float landAng = MathHelper.ToRadians(_rng.Next(360));
                                e.Vel = new Vector2(MathF.Cos(landAng), MathF.Sin(landAng))
                                                  * Constants.WANDER_SPEED;
                                e.Angle = 0f;
                                e.AngularVel = 0f;
                            }
                        }
                        else
                        {
                            // still airborne: planar + friction
                            e.Pos += e.Vel * dt;
                            e.Vel *= MathF.Max(0f, 1f - Constants.FRICTION * dt);
                        }

                        // apply spin while flying
                        e.Angle += e.AngularVel * dt;
                        e.AngularVel *= MathF.Exp(-Constants.ANGULAR_DAMPING * dt);
                        break;

                    case EnemyState.Stunned:
                        // **no rotation while stunned**
                        e.AngularVel = 0f;
                        // count down
                        e.StunTimer -= dt;
                        if (e.StunTimer <= 0f)
                        {
                            // stand up and resume walking
                            e.State = EnemyState.Walking;
                            e.WanderTimer = _rng.NextFloat(0.5f, 2.5f);
                            float upAng = MathHelper.ToRadians(_rng.Next(360));
                            e.Vel = new Vector2(MathF.Cos(upAng), MathF.Sin(upAng))
                                                 * Constants.WANDER_SPEED;
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
            Vector2 center = e.Pos;
            int half = Constants.ENEMY_H / 2;

            for (int part = -half; part < half; part++)
            {
                Vector2 pos = e.GetPartPos(part);

                // pick blood color per part (as before)…
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
                    dir = new Vector2(
                        _rng.NextFloat(-1f, 1f),
                        _rng.NextFloat(-1f, 1f)
                    );
                }
                dir.Normalize();

                // pick a random magnitude
                float mag = _rng.NextFloat(
                    Constants.EXPLOSION_FORCE_MIN,
                    Constants.EXPLOSION_FORCE_MAX
                );
                Vector2 vel = dir * mag;

                // decide: emit a single pixel or a little cluster?
                if (_rng.NextDouble() < 0.4)  // 40% chance to cluster
                {
                    int clusterSize = _rng.Next(3, 6);  // between 3 and 5 pixels
                    for (int j = 0; j < clusterSize; j++)
                    {
                        // tiny random offset so they form a “chunk”
                        Vector2 offset = new Vector2(
                            _rng.NextFloat(-0.5f, 0.5f),
                            _rng.NextFloat(-0.5f, 0.5f)
                        );
                        debris.Add(new Pixel(pos + offset, vel, c));
                    }
                }
                else
                {
                    // single‐pixel fallback
                    debris.Add(new Pixel(pos, vel, c));
                }
            }
        }


        public static void UpdatePixels(List<Pixel> debris, float dt)
        {
            for (int i = debris.Count - 1; i >= 0; i--)
            {
                var p = debris[i];

                // gentle drag
                p.Vel *= MathF.Max(0f, 1f - Constants.DEBRIS_FRICTION * dt);
                p.Pos += p.Vel * dt;

                // bounce & clamp to arena
                if (p.Pos.X < Constants.ARENA_LEFT)
                { p.Pos.X = Constants.ARENA_LEFT; p.Vel.X *= -0.5f; }
                if (p.Pos.X > Constants.ARENA_RIGHT - 1)
                { p.Pos.X = Constants.ARENA_RIGHT - 1; p.Vel.X *= -0.5f; }
                if (p.Pos.Y < Constants.ARENA_TOP)
                { p.Pos.Y = Constants.ARENA_TOP; p.Vel.Y *= -0.5f; }
                if (p.Pos.Y > Constants.ARENA_BOTTOM - 1)
                { p.Pos.Y = Constants.ARENA_BOTTOM - 1; p.Vel.Y *= -0.5f; }

                debris[i] = p;
            }
        }
    }
}
