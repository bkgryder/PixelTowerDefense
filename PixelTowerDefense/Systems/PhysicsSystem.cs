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
            List<Soldier> soldiers,
            List<Pixel> debris,
            float dt
        )
        {
            for (int i = soldiers.Count - 1; i >= 0; i--)
            {
                var e = soldiers[i];

                if (e.IsBurning)
                {
                    e.BurnTimer -= dt;
                    e.Combat.Health -= Constants.BURN_DPS * dt;
                    int half = Constants.ENEMY_H / 2;
                    for (int part = -half; part < half; part++)
                    {
                        if (_rng.NextDouble() < Constants.FIRE_PARTICLE_RATE * dt)
                        {
                            var pos = e.GetPartPos(0);
                            pos.Y -= e.z;
                            var pv = new Vector2(
                                _rng.NextFloat(-4f, 4f),
                                _rng.NextFloat(-20f, -10f)
                            );
                            Color[] firePal =
                            {
                                Color.OrangeRed,
                                Color.Orange,
                                Color.Yellow,
                                new Color(255, 100, 0)
                            };
                            var c = firePal[_rng.Next(firePal.Length)];
                            debris.Add(new Pixel(
                                pos,
                                pv,
                                c,
                                0f,
                                _rng.NextFloat(Constants.EMBER_LIFETIME * 0.5f,
                                               Constants.EMBER_LIFETIME)));
                        }
                    }
                    if (_rng.NextDouble() < Constants.SMOKE_PARTICLE_RATE * dt)
                        EmitSmoke(e.GetPartPos(0) - new Vector2(0, e.z), 1, debris);
                    if (e.BurnTimer <= 0f)
                        e.IsBurning = false;
                    if (e.Combat.Health <= 0f && e.State != SoldierState.Dead && e.State != SoldierState.Ragdoll)
                    {
                        EmitSmoke(e.GetPartPos(0) - new Vector2(0, e.z), Constants.DEATH_SMOKE_COUNT, debris);
                        AshEnemy(e, debris);
                        soldiers.RemoveAt(i);
                        continue;
                    }
                }

                switch (e.State)
                {
                    case SoldierState.Idle:
                        // wander
                        e.WanderTimer -= dt;
                        if (e.WanderTimer <= 0f)
                        {
                            if (e.IsBurning)
                            {
                                e.WanderTimer = _rng.NextFloat(Constants.BURN_WANDER_TIME_MIN,
                                                               Constants.BURN_WANDER_TIME_MAX);
                                float ang = MathHelper.ToRadians(_rng.Next(360));
                                e.Vel = new Vector2(MathF.Cos(ang), MathF.Sin(ang))
                                             * Constants.WANDER_SPEED * Constants.BURNING_SPEED_MULT;
                            }
                            else
                            {
                                e.WanderTimer = _rng.NextFloat(1f, 3f);
                                float ang = MathHelper.ToRadians(_rng.Next(360));
                                e.Vel = new Vector2(MathF.Cos(ang), MathF.Sin(ang))
                                             * Constants.WANDER_SPEED;
                            }
                        }
                        // ensure burning enemies keep running fast
                        if (e.IsBurning)
                            e.Vel = Vector2.Normalize(e.Vel) * Constants.WANDER_SPEED * Constants.BURNING_SPEED_MULT;
                        e.Pos += e.Vel * dt;
                        e.Angle = 0f;
                        break;

                    case SoldierState.Dead:
                        e.Vel = Vector2.Zero;
                        e.Angle = 0f;
                        e.AngularVel = 0f;
                        e.DecompTimer += dt;
                        break;

                    case SoldierState.Launched:
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
                                soldiers.RemoveAt(i);
                                continue;
                            }
                            else if (impact > Constants.STUN_VZ_THRESHOLD)
                            {
                                // go stunned (flat on ground)
                                e.State = SoldierState.Stunned;
                                e.StunTimer = Constants.STUN_TIME;
                                e.Angle = MathHelper.PiOver2; // lay flat
                                e.Vel = Vector2.Zero;
                                e.AngularVel = 0f;
                            }
                            else
                            {
                                // gentle landing → resume Idle
                                e.State = SoldierState.Idle;
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

                    case SoldierState.Ragdoll:
                        e.vz -= Constants.Z_GRAVITY * dt;
                        e.z += e.vz * dt;

                        if (e.z <= 0f)
                        {
                            e.z = 0f;
                            e.vz = 0f;
                            e.State = SoldierState.Dead;
                            e.DecompTimer = 0f;
                            e.Angle = MathHelper.PiOver2;
                            e.Vel = Vector2.Zero;
                            e.AngularVel = 0f;
                        }
                        else
                        {
                            e.Pos += e.Vel * dt;
                            e.Vel *= MathF.Max(0f, 1f - Constants.FRICTION * dt);
                            e.Angle += e.AngularVel * dt;
                            e.AngularVel *= MathF.Exp(-Constants.ANGULAR_DAMPING * dt);
                        }
                        break;

                    case SoldierState.Stunned:
                        // **no rotation while stunned**
                        e.AngularVel = 0f;
                        // count down
                        e.StunTimer -= dt;
                        if (e.StunTimer <= 0f)
                        {
                            // stand up and resume walking
                            e.State = SoldierState.Idle;
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

                if (e.Combat.Health <= 0f && e.State != SoldierState.Dead && e.State != SoldierState.Ragdoll)
                {
                    AshEnemy(e, debris);
                    soldiers.RemoveAt(i);
                    continue;
                }

                soldiers[i] = e;
            }
        }

        private static void ExplodeEnemy(Soldier e, List<Pixel> debris)
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
                Color[] bonePal =
                {
                    new Color(255, 245, 235),
                    new Color(255, 220, 230),
                    new Color(245, 200, 210)
                };
                if (_rng.NextDouble() < 0.15)
                    c = bonePal[_rng.Next(bonePal.Length)];

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
                        debris.Add(new Pixel(
                            pos + offset,
                            vel,
                            c,
                            0f,
                            _rng.NextFloat(Constants.DEBRIS_LIFETIME_MIN,
                                           Constants.DEBRIS_LIFETIME_MAX)));
                    }
                }
                else
                {
                    // single‐pixel fallback
                    debris.Add(new Pixel(
                        pos,
                        vel,
                        c,
                        0f,
                        _rng.NextFloat(Constants.DEBRIS_LIFETIME_MIN,
                                       Constants.DEBRIS_LIFETIME_MAX)));
                }
            }
        }

        private static void AshEnemy(Soldier e, List<Pixel> debris)
        {
            Vector2 ground = e.Pos;
            int half = Constants.ENEMY_H / 2;

            for (int part = -half; part < half; part++)
            {
                int count = _rng.Next(Constants.ASH_PARTICLES_MIN,
                                     Constants.ASH_PARTICLES_MAX + 1);
                for (int j = 0; j < count; j++)
                {
                    int shade = _rng.Next(60, 111);
                    Color c = new Color(shade, shade, shade);
                    Vector2 pos = ground + new Vector2(
                        _rng.NextFloat(-1f, 1f),
                        _rng.NextFloat(-0.5f, 0.5f)
                    );
                    Vector2 dir = new Vector2(
                        _rng.NextFloat(-0.3f, 0.3f),
                        _rng.NextFloat(0.6f, 1f)
                    );
                    dir.Normalize();
                    float mag = _rng.NextFloat(
                        Constants.ASH_FORCE_MIN,
                        Constants.ASH_FORCE_MAX
                    );
                    Vector2 vel = dir * mag;

                    debris.Add(new Pixel(
                        pos,
                        vel,
                        c,
                        0f,
                        _rng.NextFloat(Constants.DEBRIS_LIFETIME_MIN,
                                       Constants.DEBRIS_LIFETIME_MAX)));
                }
            }

            // lingering smoke from ash pile
            EmitSmoke(ground, 3, debris);
        }

        private static void EmitSmoke(Vector2 pos, int count, List<Pixel> debris)
        {
            Color[] smokePal =
            {
                new Color(80, 80, 80),
                new Color(100, 100, 100),
                new Color(120, 120, 120),
                new Color(150, 150, 150)
            };
            for (int i = 0; i < count; i++)
            {
                var c = smokePal[_rng.Next(smokePal.Length)];
                var v = new Vector2(
                    _rng.NextFloat(-0.5f, 0.5f),
                    -_rng.NextFloat(Constants.SMOKE_FORCE_MIN, Constants.SMOKE_FORCE_MAX)
                );
                debris.Add(new Pixel(pos, v, c, 0f, Constants.SMOKE_LIFETIME));
            }
        }


        public static void UpdatePixels(List<Pixel> debris, float dt)
        {
            for (int i = debris.Count - 1; i >= 0; i--)
            {
                var p = debris[i];

                if (p.Lifetime > 0f)
                {
                    p.Lifetime -= dt;
                    if (p.Lifetime <= 0f)
                    {
                        debris.RemoveAt(i);
                        continue;
                    }
                }

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
