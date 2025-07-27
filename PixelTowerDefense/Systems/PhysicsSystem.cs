using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PixelTowerDefense.Entities;
using PixelTowerDefense.Utils;
using PixelTowerDefense.World;

namespace PixelTowerDefense.Systems
{
    public static class PhysicsSystem
    {
        private static Random _rng = new Random();

        public static void SimulateAll(
            List<Meeple> meeples,
            List<Pixel> debris,
            List<BerryBush> bushes,
            List<Building> buildings,
            List<Tree> trees,
            List<Log> logs,
            WaterMap water,
            float dt
        )
        {
            for (int i = meeples.Count - 1; i >= 0; i--)
            {
                var e = meeples[i];

                if (e.IsBurning)
                {
                    e.BurnTimer -= dt;
                    e.Health -= Constants.BURN_DPS * dt;
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
                            debris.Spawn(new Pixel(
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
                    if (e.Health <= 0f && e.State != MeepleState.Dead && e.State != MeepleState.Ragdoll)
                    {
                        EmitSmoke(e.GetPartPos(0) - new Vector2(0, e.z), Constants.DEATH_SMOKE_COUNT, debris);
                        AshEnemy(e, debris);
                        meeples.RemoveAt(i);
                        continue;
                    }
                }

                switch (e.State)
                {
                    case MeepleState.Idle:
                        e.Hunger = MathF.Min(Constants.HUNGER_MAX, e.Hunger + Constants.HUNGER_RATE * dt);
                        if (e.Worker != null)
                        {
                            if (e.CarriedBerries == 1 && e.Hunger > Constants.HUNGER_THRESHOLD)
                            {
                                int hidx = FindNearestDepositHut(e.Pos, buildings);
                                if (hidx >= 0)
                                {
                                    var hut = buildings[hidx];
                                    Vector2 dir = hut.Pos - e.Pos;
                                    float dist = dir.Length();
                                    if (dist < 1f)
                                    {
                                        hut.StoredBerries++;
                                        e.CarriedBerries = 0;
                                        buildings[hidx] = hut;
                                        e.WanderTimer = 0f;
                                    }
                                    else
                                    {
                                        if (dist > 0f) dir /= dist; else dir = Vector2.Zero;
                                        e.Vel = dir * e.MoveSpeed;
                                        e.Pos += e.Vel * dt;
                                    }
                                    e.Angle = 0f;
                                    break;
                                }
                            }

                            if (e.Hunger >= Constants.HUNGER_THRESHOLD)
                            {
                                if (e.CarriedBerries > 0)
                                {
                                    e.CarriedBerries--;
                                    e.Hunger = 0f;
                                    e.WanderTimer = 0f;
                                }
                                else
                                {
                                    int hidx = FindNearestFoodHut(e.Pos, buildings);
                                    if (hidx >= 0)
                                    {
                                        var hut = buildings[hidx];
                                        Vector2 dir = hut.Pos - e.Pos;
                                        float dist = dir.Length();
                                        if (dist < 1f)
                                        {
                                            if (hut.StoredBerries > 0)
                                            {
                                                hut.StoredBerries--;
                                                e.Hunger = 0f;
                                                buildings[hidx] = hut;
                                            }
                                            e.WanderTimer = 0f;
                                        }
                                        else
                                        {
                                            if (dist > 0f) dir /= dist; else dir = Vector2.Zero;
                                            e.Vel = dir * e.MoveSpeed;
                                            e.Pos += e.Vel * dt;
                                        }
                                        e.Angle = 0f;
                                        break;
                                    }
                                }
                            }

                            if (e.Hunger >= Constants.HUNGER_THRESHOLD && e.CarriedBerries == 0)
                            {
                                int bidx;
                                var wdata = e.Worker.Value;
                                if (wdata.CurrentJob == JobType.HarvestBerries && wdata.TargetIdx != null)
                                    bidx = wdata.TargetIdx.Value;
                                else
                                {
                                    bidx = FindNearestBush(e.Pos, bushes);
                                    if (bidx >= 0)
                                    {
                                        wdata.CurrentJob = JobType.HarvestBerries;
                                        wdata.TargetIdx = bidx;
                                        var rbush = bushes[bidx];
                                        rbush.ReservedBy = i;
                                        bushes[bidx] = rbush;
                                    }
                                }
                                if (bidx >= 0)
                                {
                                    var bush = bushes[bidx];
                                    Vector2 dir = bush.Pos - e.Pos;
                                    float dist = dir.Length();
                                    if (dist < Constants.HARVEST_RANGE)
                                    {
                                        if (bush.Berries > 0)
                                        {
                                            bush.Berries--;
                                            e.CarriedBerries = 1;
                                            bush.ReservedBy = null;
                                            bushes[bidx] = bush;
                                            wdata.CurrentJob = JobType.None;
                                            wdata.TargetIdx = null;
                                        }
                                    }
                                    else
                                    {
                                        if (dist > 0f) dir /= dist; else dir = Vector2.Zero;
                                        e.Vel = dir * e.MoveSpeed;
                                        e.Pos += e.Vel * dt;
                                    }
                                    e.Angle = 0f;
                                    e.Worker = wdata;
                                    break;
                                }
                            }

                            // --- log & tree jobs ---
                            if (e.CarriedLogIdx >= 0)
                            {
                                if (e.CarriedLogIdx < logs.Count)
                                {
                                    var log = logs[e.CarriedLogIdx];
                                    int hidx = FindNearestCarpenter(e.Pos, buildings);
                                    if (hidx < 0)
                                        hidx = FindNearestStockpileForLogs(e.Pos, buildings);
                                    if (hidx >= 0)
                                    {
                                        var hut = buildings[hidx];
                                        Vector2 dir = hut.Pos - e.Pos;
                                        float dist = dir.Length();
                                        if (dist < 1f)
                                        {
                                            hut.StoredLogs++;
                                            if (hut.CraftTimer <= 0f)
                                                hut.CraftTimer = Constants.BASE_CRAFT / (1f + 0.1f * e.Intellect);
                                            buildings[hidx] = hut;
                                            logs.RemoveAt(e.CarriedLogIdx);
                                            e.CarriedLogIdx = -1;
                                        }
                                        else
                                        {
                                            if (dist > 0f) dir /= dist; else dir = Vector2.Zero;
                                            e.Vel = dir * e.MoveSpeed;
                                            e.Pos += e.Vel * dt;
                                            log.Pos = e.Pos;
                                            logs[e.CarriedLogIdx] = log;
                                        }
                                        e.Angle = 0f;
                                        break;
                                    }
                                }
                                else
                                {
                                    e.CarriedLogIdx = -1;
                                }
                            }
                            else
                            {
                                int lidx = FindNearestLooseLog(e.Pos, logs);
                                int tidx = FindNearestTree(e.Pos, trees);

                                float haulScore = lidx >= 0 ? JobAffinity(JobType.HaulLog, e) : -1f;
                                float chopScore = tidx >= 0 ? JobAffinity(JobType.ChopTree, e) : -1f;

                                if (haulScore >= chopScore && lidx >= 0)
                                {
                                    var log = logs[lidx];
                                    Vector2 dir = log.Pos - e.Pos;
                                    float dist = dir.Length();
                                    if (dist < Constants.HARVEST_RANGE)
                                    {
                                        log.IsCarried = true;
                                        logs[lidx] = log;
                                        e.CarriedLogIdx = lidx;
                                        e.WanderTimer = 0f;
                                    }
                                    else
                                    {
                                        if (dist > 0f) dir /= dist; else dir = Vector2.Zero;
                                        e.Vel = dir * e.MoveSpeed;
                                        e.Pos += e.Vel * dt;
                                    }
                                    e.Angle = 0f;
                                    break;
                                }
                                else if (tidx >= 0)
                                {
                                    var tree = trees[tidx];
                                    Vector2 dir = tree.Pos - e.Pos;
                                    float dist = dir.Length();
                                    if (dist < tree.CollisionRadius + Constants.TOUCH_RANGE)
                                    {
                                        if (!tree.IsStump)
                                        {
                                            tree.Health--;
                                            if (tree.Health <= 0)
                                            {
                                                tree.IsStump = true;
                                                tree.CollisionRadius = Constants.STUMP_RADIUS;
                                                logs.Add(new Log(tree.Pos, _rng));
                                            }
                                            trees[tidx] = tree;
                                        }
                                        e.WanderTimer = Constants.BASE_CHOP / (1f + 0.1f * e.Strength);
                                    }
                                    else
                                    {
                                        if (dist > 0f) dir /= dist; else dir = Vector2.Zero;
                                        e.Vel = dir * e.MoveSpeed;
                                        e.Pos += e.Vel * dt;
                                    }
                                    e.Angle = 0f;
                                    break;
                                }
                            }
                        }
                        e.WanderTimer -= dt;
                        if (e.WanderTimer <= 0f)
                        {
                            if (e.IsBurning)
                            {
                                e.WanderTimer = _rng.NextFloat(Constants.BURN_WANDER_TIME_MIN,
                                                               Constants.BURN_WANDER_TIME_MAX);
                                float ang = MathHelper.ToRadians(_rng.Next(360));
                                e.Vel = new Vector2(MathF.Cos(ang), MathF.Sin(ang))
                                              * e.MoveSpeed * Constants.BURNING_SPEED_MULT;
                            }
                            else
                            {
                                e.WanderTimer = _rng.NextFloat(1f, 3f);
                                float ang = MathHelper.ToRadians(_rng.Next(360));
                                e.Vel = new Vector2(MathF.Cos(ang), MathF.Sin(ang))
                                              * e.MoveSpeed;
                            }
                        }
                        if (e.IsBurning)
                            e.Vel = Vector2.Normalize(e.Vel) * e.MoveSpeed * Constants.BURNING_SPEED_MULT;
                        e.Pos += e.Vel * dt;
                        e.Angle = 0f;
                        break;

                    case MeepleState.Dead:
                        e.Vel = Vector2.Zero;
                        e.AngularVel = 0f;
                        e.DecompTimer += dt;
                        break;

                    case MeepleState.Launched:
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
                                meeples.RemoveAt(i);
                                continue;
                            }
                            else if (impact > Constants.STUN_VZ_THRESHOLD)
                            {
                                e.State = MeepleState.Stunned;
                                e.StunTimer = Constants.STUN_TIME;
                                e.Angle = MathHelper.PiOver2;
                                e.Vel = Vector2.Zero;
                                e.AngularVel = 0f;
                            }
                            else
                            {
                                e.State = MeepleState.Idle;
                                e.WanderTimer = _rng.NextFloat(0.5f, 2.5f);
                                float landAng = MathHelper.ToRadians(_rng.Next(360));
                                e.Vel = new Vector2(MathF.Cos(landAng), MathF.Sin(landAng))
                                                  * e.MoveSpeed;
                                e.Angle = 0f;
                                e.AngularVel = 0f;
                            }
                        }
                        else
                        {
                            e.Pos += e.Vel * dt;
                            e.Vel *= MathF.Max(0f, 1f - Constants.FRICTION * dt);
                        }

                        e.Angle += e.AngularVel * dt;
                        e.AngularVel *= MathF.Exp(-Constants.ANGULAR_DAMPING * dt);
                        break;

                    case MeepleState.Ragdoll:
                        e.vz -= Constants.Z_GRAVITY * dt;
                        e.z += e.vz * dt;

                        if (e.z <= 0f)
                        {
                            e.z = 0f;
                            e.vz = 0f;
                            e.State = MeepleState.Dead;
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

                    case MeepleState.Stunned:
                        e.AngularVel = 0f;
                        e.StunTimer -= dt;
                        if (e.StunTimer <= 0f)
                        {
                            e.State = MeepleState.Idle;
                            e.WanderTimer = _rng.NextFloat(0.5f, 2.5f);
                            float upAng = MathHelper.ToRadians(_rng.Next(360));
                            e.Vel = new Vector2(MathF.Cos(upAng), MathF.Sin(upAng))
                                                 * e.MoveSpeed;
                            e.Angle = 0f;
                        }
                        break;
                }

                int wx = Math.Clamp((int)e.Pos.X, 0, water.Width - 1);
                int wy = Math.Clamp((int)e.Pos.Y, 0, water.Height - 1);
                byte depth = water.Depth[wx, wy];
                if (depth > 0)
                {
                    Vector2 push = new Vector2(water.FlowX[wx, wy], water.FlowY[wx, wy])
                                     * Constants.WATER_PUSH;
                    e.Vel += push * dt;
                    e.Vel *= (1f - Constants.WATER_DRAG * dt);
                    e.InWaterTime += dt;

                    if (e.IsBurning)
                    {
                        e.BurnTimer -= Constants.WATER_QUENCH_RATE * dt;
                        if (_rng.NextDouble() < dt)
                            EmitSteam(e.Pos - new Vector2(0, e.z), debris);
                        if (e.BurnTimer <= 0f)
                            e.IsBurning = false;
                    }

                    float flowMag = MathF.Sqrt(water.FlowX[wx, wy] * water.FlowX[wx, wy] +
                                               water.FlowY[wx, wy] * water.FlowY[wx, wy]);
                    if (flowMag >= 2f && _rng.NextDouble() < dt * 10f)
                        EmitSparkle(e.Pos - new Vector2(0, e.z), debris);

                    if (depth >= Constants.DROWN_DEPTH && e.InWaterTime >= Constants.DROWN_TIME)
                        e.Health = 0f;
                }
                else
                {
                    e.InWaterTime = 0f;
                }

                e.Pos.X = MathHelper.Clamp(e.Pos.X,
                           Constants.ARENA_LEFT + 2,
                           Constants.ARENA_RIGHT - 2);
               e.Pos.Y = MathHelper.Clamp(e.Pos.Y,
                          Constants.ARENA_TOP + 2,
                          Constants.ARENA_BOTTOM - 2);

                foreach (var t in trees)
                {
                    if (t.IsStump) continue;

                    Vector2 diff = e.Pos - t.Pos;
                    float dist = diff.Length();
                    if (dist < t.CollisionRadius)
                    {
                        if (dist > 0f)
                            diff /= dist;
                        else
                            diff = new Vector2(0f, -1f);
                        e.Pos = t.Pos + diff * t.CollisionRadius;
                    }
                }

                if (e.Health <= 0f && e.State != MeepleState.Dead && e.State != MeepleState.Ragdoll)
                {
                    AshEnemy(e, debris);
                    meeples.RemoveAt(i);
                    continue;
                }

                meeples[i] = e;
            }

            ResolveDominoCollisions(meeples);

            for (int i = 0; i < buildings.Count; i++)
            {
                var b = buildings[i];
                if (b.Kind == BuildingType.CarpenterHut && b.StoredLogs > 0)
                {
                    if (b.CraftTimer > 0f)
                    {
                        b.CraftTimer -= dt;
                        if (b.CraftTimer <= 0f)
                        {
                            b.StoredLogs--;
                            b.StoredPlanks++;
                            b.CraftTimer = b.StoredLogs > 0 ? Constants.BASE_CRAFT : 0f;
                        }
                    }
                }
                buildings[i] = b;
            }
        }

        private static void ExplodeEnemy(Meeple e, List<Pixel> debris)
        {
            Vector2 center = e.Pos;
            int half = Constants.ENEMY_H / 2;

            for (int part = -half; part < half; part++)
            {
                Vector2 pos = e.GetPartPos(part);

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

                Vector2 dir = pos - center;
                if (dir == Vector2.Zero)
                {
                    dir = new Vector2(
                        _rng.NextFloat(-1f, 1f),
                        _rng.NextFloat(-1f, 1f)
                    );
                }
                dir.Normalize();

                float mag = _rng.NextFloat(
                    Constants.EXPLOSION_FORCE_MIN,
                    Constants.EXPLOSION_FORCE_MAX
                );
                Vector2 vel = dir * mag;

                if (_rng.NextDouble() < 0.4)
                {
                    int clusterSize = _rng.Next(3, 6);
                    for (int j = 0; j < clusterSize; j++)
                    {
                        Vector2 offset = new Vector2(
                            _rng.NextFloat(-0.5f, 0.5f),
                            _rng.NextFloat(-0.5f, 0.5f)
                        );
                        debris.Spawn(new Pixel(
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
                    debris.Spawn(new Pixel(
                        pos,
                        vel,
                        c,
                        0f,
                        _rng.NextFloat(Constants.DEBRIS_LIFETIME_MIN,
                                       Constants.DEBRIS_LIFETIME_MAX)));
                }
            }
        }

        private static void AshEnemy(Meeple e, List<Pixel> debris)
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

                    debris.Spawn(new Pixel(
                        pos,
                        vel,
                        c,
                        0f,
                        _rng.NextFloat(Constants.DEBRIS_LIFETIME_MIN,
                                       Constants.DEBRIS_LIFETIME_MAX)));
                }
            }

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
                debris.Spawn(new Pixel(pos, v, c, 0f, Constants.SMOKE_LIFETIME));
            }
        }

        private static void EmitSteam(Vector2 pos, List<Pixel> debris)
        {
            var c = Color.LightGray;
            var v = new Vector2(
                _rng.NextFloat(-0.5f, 0.5f),
                -_rng.NextFloat(10f, 20f)
            );
            debris.Spawn(new Pixel(pos, v, c, 0f, 0.5f));
        }

        private static void EmitSparkle(Vector2 pos, List<Pixel> debris)
        {
            var c = Color.LightBlue;
            var v = new Vector2(
                _rng.NextFloat(-1f, 1f),
                _rng.NextFloat(-1f, 1f)
            );
            debris.Spawn(new Pixel(pos, v, c, 0f, 0.3f));
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

                p.Vel *= MathF.Max(0f, 1f - Constants.DEBRIS_FRICTION * dt);
                p.Pos += p.Vel * dt;

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

        public static void UpdateLogs(List<Log> logs, float dt)
        {
            for (int i = 0; i < logs.Count; i++)
            {
                var l = logs[i];

                l.Vel *= MathF.Max(0f, 1f - Constants.DEBRIS_FRICTION * dt);
                l.Pos += l.Vel * dt;

                if (l.Pos.X < Constants.ARENA_LEFT)
                { l.Pos.X = Constants.ARENA_LEFT; l.Vel.X *= -0.5f; }
                if (l.Pos.X > Constants.ARENA_RIGHT - 1)
                { l.Pos.X = Constants.ARENA_RIGHT - 1; l.Vel.X *= -0.5f; }
                if (l.Pos.Y < Constants.ARENA_TOP)
                { l.Pos.Y = Constants.ARENA_TOP; l.Vel.Y *= -0.5f; }
                if (l.Pos.Y > Constants.ARENA_BOTTOM - 1)
                { l.Pos.Y = Constants.ARENA_BOTTOM - 1; l.Vel.Y *= -0.5f; }

                logs[i] = l;
            }
        }

        public static void UpdateSeeds(List<Seed> seeds, List<Tree> trees, float dt)
        {
            for (int i = seeds.Count - 1; i >= 0; i--)
            {
                var s = seeds[i];

                s.Vel *= MathF.Max(0f, 1f - Constants.DEBRIS_FRICTION * dt);
                s.Pos += s.Vel * dt;
                s.vz -= Constants.Z_GRAVITY * dt;
                s.z += s.vz * dt;
                if (s.z <= 0f)
                {
                    s.z = 0f;
                    s.vz = 0f;
                }

                if (s.Pos.X < Constants.ARENA_LEFT)
                { s.Pos.X = Constants.ARENA_LEFT; s.Vel.X *= -0.5f; }
                if (s.Pos.X > Constants.ARENA_RIGHT - 1)
                { s.Pos.X = Constants.ARENA_RIGHT - 1; s.Vel.X *= -0.5f; }
                if (s.Pos.Y < Constants.ARENA_TOP)
                { s.Pos.Y = Constants.ARENA_TOP; s.Vel.Y *= -0.5f; }
                if (s.Pos.Y > Constants.ARENA_BOTTOM - 1)
                { s.Pos.Y = Constants.ARENA_BOTTOM - 1; s.Vel.Y *= -0.5f; }

                s.Age += dt;
                if (s.Age >= s.GrowTime)
                {
                    bool nearTree = false;
                    foreach (var t in trees)
                    {
                        if (Vector2.Distance(t.Pos, s.Pos) < Constants.SEED_MIN_TREE_DIST)
                        { nearTree = true; break; }
                    }
                    bool nearSeed = false;
                    if (!nearTree)
                    {
                        for (int j = 0; j < seeds.Count; j++)
                        {
                            if (j == i) continue;
                            if (Vector2.Distance(seeds[j].Pos, s.Pos) < Constants.SEED_MIN_SEED_DIST)
                            { nearSeed = true; break; }
                        }
                    }
                    if (!nearTree && !nearSeed)
                        trees.Add(new Tree(s.Pos, _rng));

                    seeds.RemoveAt(i);
                    continue;
                }

                seeds[i] = s;
            }
        }

        public static void UpdateTrees(List<Tree> trees, List<Seed> seeds, List<Pixel> debris, float dt)
        {
            for (int i = trees.Count - 1; i >= 0; i--)
            {
                var t = trees[i];
                bool grow = !t.IsBurning;
                t.Grow(dt, _rng, grow);

                if (t.IsBurning)
                {
                    t.BurnTimer -= dt;
                    t.BurnProgress = MathF.Min(1f, t.BurnProgress + dt / Constants.TREE_BURN_DURATION);

                    float leafTop = 0f;
                    if (t.LeafPixels.Length > 0)
                        foreach (var p in t.LeafPixels)
                            if (p.Y < leafTop) leafTop = p.Y;
                    float trunkTop = 0f;
                    if (t.TrunkPixels.Length > 0)
                        foreach (var p in t.TrunkPixels)
                            if (p.Y < trunkTop) trunkTop = p.Y;

                    float leafThresh = MathHelper.Lerp(leafTop, 0f, MathF.Min(1f, t.BurnProgress * 2f));
                    float trunkThresh = MathHelper.Lerp(trunkTop, 0f, t.BurnProgress);

                    if (t.LeafPixels.Length > 0)
                    {
                        var list = new List<Point>();
                        foreach (var p in t.LeafPixels)
                        {
                            if (p.Y <= leafThresh)
                            {
                                if (_rng.NextDouble() < Constants.TREE_EMBER_RATE * dt)
                                {
                                    var pos = t.Pos + new Vector2(p.X, p.Y);
                                    var vel = new Vector2(
                                        _rng.NextFloat(-4f, 4f),
                                        _rng.NextFloat(-20f, -10f));
                                    Color[] firePal =
                                    {
                                        Color.OrangeRed,
                                        Color.Orange,
                                        Color.Yellow,
                                        new Color(255, 100, 0)
                                    };
                                    var c = firePal[_rng.Next(firePal.Length)];
                                    debris.Spawn(new Pixel(
                                        pos,
                                        vel,
                                        c,
                                        0f,
                                        _rng.NextFloat(Constants.EMBER_LIFETIME * 0.5f, Constants.EMBER_LIFETIME)));
                                }
                            }
                            else
                            {
                                list.Add(p);
                            }
                        }
                        t.LeafPixels = list.ToArray();
                    }

                    if (t.TrunkPixels.Length > 0)
                    {
                        var list = new List<Point>();
                        foreach (var p in t.TrunkPixels)
                        {
                            if (p.Y <= trunkThresh && (t.LeafPixels.Length == 0 || t.BurnProgress >= 0.5f))
                            {
                                if (_rng.NextDouble() < Constants.TREE_EMBER_RATE * dt)
                                {
                                    var pos = t.Pos + new Vector2(p.X, p.Y);
                                    var vel = new Vector2(
                                        _rng.NextFloat(-4f, 4f),
                                        _rng.NextFloat(-20f, -10f));
                                    Color[] firePal =
                                    {
                                        Color.OrangeRed,
                                        Color.Orange,
                                        Color.Yellow,
                                        new Color(255, 100, 0)
                                    };
                                    var c = firePal[_rng.Next(firePal.Length)];
                                    debris.Spawn(new Pixel(
                                        pos,
                                        vel,
                                        c,
                                        0f,
                                        _rng.NextFloat(Constants.EMBER_LIFETIME * 0.5f, Constants.EMBER_LIFETIME)));
                                }
                            }
                            else
                            {
                                list.Add(p);
                            }
                        }
                        t.TrunkPixels = list.ToArray();
                        if (t.TrunkPixels.Length == 0)
                            t.CollisionRadius = 0f;
                    }

                    if (_rng.NextDouble() < Constants.SMOKE_PARTICLE_RATE * dt)
                        EmitSmoke(t.Pos, 1, debris);

                    if (t.BurnTimer <= 0f && t.TrunkPixels.Length == 0 && t.LeafPixels.Length == 0)
                    {
                        trees.RemoveAt(i);
                        continue;
                    }

                    trees[i] = t;
                    continue;
                }

                if (t.IsDead && t.LeafPixels.Length > 0 &&
                    _rng.NextDouble() < Constants.LEAF_FALL_CHANCE * dt)
                {
                    int idx = _rng.Next(t.LeafPixels.Length);
                    var lp = t.LeafPixels[idx];
                    var pos = t.Pos + new Vector2(lp.X, lp.Y);
                    var vel = new Vector2(
                        _rng.NextFloat(-5f, 5f),
                        _rng.NextFloat(-30f, -10f)
                    );
                    debris.Spawn(new Pixel(
                        pos,
                        vel,
                        Color.Goldenrod,
                        0f,
                        _rng.NextFloat(Constants.DEBRIS_LIFETIME_MIN, Constants.DEBRIS_LIFETIME_MAX)));
                }

                if (!t.IsStump && !t.IsDead && t.Age >= t.GrowthDuration &&
                    _rng.NextDouble() < Constants.TREE_SEED_CHANCE * dt)
                {
                    int topY = 0;
                    int topX = 0;
                    foreach (var p in t.TrunkPixels)
                    {
                        if (p.Y < topY)
                        {
                            topY = p.Y;
                            topX = p.X;
                        }
                    }
                    Vector2 spawn = t.Pos + new Vector2(topX, topY);
                    Vector2 dir = new Vector2(_rng.NextFloat(-1f, 1f), _rng.NextFloat(-1f, 1f));
                    if (dir.LengthSquared() > 0f) dir.Normalize();
                    float speed = _rng.NextFloat(Constants.TREE_SEED_SPEED_MIN, Constants.TREE_SEED_SPEED_MAX);
                    Vector2 vel = dir * speed;
                    float vz0 = _rng.NextFloat(Constants.TREE_SEED_UPWARD_MIN, Constants.TREE_SEED_UPWARD_MAX);
                    seeds.Add(new Seed(spawn, vel, vz0, _rng));
                }

                if (t.Fallen && t.DecompTimer >= Constants.TREE_DISINTEGRATE_TIME)
                {
                    trees.RemoveAt(i);
                    continue;
                }

                trees[i] = t;
            }
        }

        public static void WorkerChopTree(ref Meeple worker, ref Tree tree, List<Log> logs, Random rng)
        {
            if (Vector2.Distance(worker.Pos, tree.Pos) <= 1f && tree.Health > 0)
            {
                tree.Health--;
                logs.Add(new Log(tree.Pos, rng));
                logs.Add(new Log(tree.Pos, rng));
                if (tree.Health <= 0)
                {
                    tree.IsStump = true;
                    tree.CollisionRadius = Constants.STUMP_RADIUS;
                }
                worker.WanderTimer = Constants.BASE_CHOP / (1f + 0.1f * worker.Strength);
            }
        }

        public static void WorkerDepositLog(ref Meeple worker, ref Building building)
        {
            if (building.Kind == BuildingType.CarpenterHut &&
                Vector2.Distance(worker.Pos, building.Pos) <= 1f &&
                worker.CarriedLogs > 0)
            {
                building.StoredLogs += worker.CarriedLogs;
                worker.CarriedLogs = 0;
                if (building.CraftTimer <= 0f)
                    building.CraftTimer = Constants.BASE_CRAFT / (1f + 0.1f * worker.Intellect);
            }
        }

        public static void UpdateCarpenter(ref Building building, float dt)
        {
            if (building.Kind != BuildingType.CarpenterHut) return;

            if (building.StoredLogs > 0)
            {
                building.CraftTimer -= dt;
                if (building.CraftTimer <= 0f)
                {
                    building.StoredLogs--;
                    building.StoredPlanks++;
                    building.CraftTimer = building.StoredLogs > 0 ? Constants.BASE_CRAFT : 0f;
                }
            }
        }

        private static void ResolveDominoCollisions(List<Meeple> meeples)
        {
            for (int i = 0; i < meeples.Count; i++)
            {
                var a = meeples[i];
                if (!a.Alive || a.State != MeepleState.Launched) continue;

                for (int j = 0; j < meeples.Count; j++)
                {
                    if (i == j) continue;
                    var b = meeples[j];
                    if (!b.Alive) continue;
                    if (b.State == MeepleState.Launched ||
                        b.State == MeepleState.Ragdoll ||
                        b.State == MeepleState.Stunned ||
                        b.State == MeepleState.Dead)
                        continue;

                    if (Vector2.Distance(a.Pos, b.Pos) < Constants.DOMINO_RANGE)
                    {
                        Vector2 dir = b.Pos - a.Pos;
                        if (dir.LengthSquared() > 0f)
                            dir.Normalize();
                        else
                            dir = new Vector2(0f, -1f);

                        b.State = MeepleState.Launched;
                        b.Vel += dir * Constants.DOMINO_KNOCKBACK;
                        b.vz += Constants.DOMINO_KNOCKBACK_UPWARD;
                        b.AngularVel = _rng.NextFloat(-4f, 4f);
                        meeples[j] = b;
                    }
                }
            }
        }

        private static int FindNearestBush(Vector2 pos, List<BerryBush> bushes)
        {
            int idx = -1;
            float best = float.MaxValue;
            for (int i = 0; i < bushes.Count; i++)
            {
                if (bushes[i].Berries <= 0) continue;
                if (bushes[i].ReservedBy != null) continue;
                float d = Vector2.Distance(pos, bushes[i].Pos);
                if (d < best)
                {
                    best = d;
                    idx = i;
                }
            }
            return idx;
        }

        private static int FindNearestDepositHut(Vector2 pos, List<Building> huts)
        {
            int idx = -1;
            float best = float.MaxValue;
            for (int i = 0; i < huts.Count; i++)
            {
                if (huts[i].Kind != BuildingType.StockpileHut) continue;
                if (huts[i].StoredBerries >= Building.CAPACITY) continue;
                if (huts[i].ReservedBy != null) continue;
                float d = Vector2.Distance(pos, huts[i].Pos);
                if (d < best)
                {
                    best = d;
                    idx = i;
                }
            }
            return idx;
        }

        private static int FindNearestFoodHut(Vector2 pos, List<Building> huts)
        {
            int idx = -1;
            float best = float.MaxValue;
            for (int i = 0; i < huts.Count; i++)
            {
                if (huts[i].Kind != BuildingType.StockpileHut) continue;
                if (huts[i].StoredBerries <= 0) continue;
                if (huts[i].ReservedBy != null) continue;
                float d = Vector2.Distance(pos, huts[i].Pos);
                if (d < best)
                {
                    best = d;
                    idx = i;
                }
            }
            return idx;
        }

        private static int FindNearestCarpenter(Vector2 pos, List<Building> huts)
        {
            int idx = -1;
            float best = float.MaxValue;
            for (int i = 0; i < huts.Count; i++)
            {
                if (huts[i].Kind != BuildingType.CarpenterHut) continue;
                if (huts[i].ReservedBy != null) continue;
                float d = Vector2.Distance(pos, huts[i].Pos);
                if (d < best)
                {
                    best = d;
                    idx = i;
                }
            }
            return idx;
        }

        private static int FindNearestStockpileForLogs(Vector2 pos, List<Building> huts)
        {
            int idx = -1;
            float best = float.MaxValue;
            for (int i = 0; i < huts.Count; i++)
            {
                if (huts[i].Kind != BuildingType.StockpileHut) continue;
                if (huts[i].ReservedBy != null) continue;
                float d = Vector2.Distance(pos, huts[i].Pos);
                if (d < best)
                {
                    best = d;
                    idx = i;
                }
            }
            return idx;
        }

        private static int FindNearestLooseLog(Vector2 pos, List<Log> logs)
        {
            int idx = -1;
            float best = float.MaxValue;
            for (int i = 0; i < logs.Count; i++)
            {
                if (logs[i].IsCarried) continue;
                if (logs[i].ReservedBy != null) continue;
                float d = Vector2.Distance(pos, logs[i].Pos);
                if (d < best)
                {
                    best = d;
                    idx = i;
                }
            }
            return idx;
        }

        private static int FindNearestTree(Vector2 pos, List<Tree> trees)
        {
            int idx = -1;
            float best = float.MaxValue;
            for (int i = 0; i < trees.Count; i++)
            {
                if (trees[i].IsStump) continue;
                if (trees[i].ReservedBy != null) continue;
                float d = Vector2.Distance(pos, trees[i].Pos);
                if (d < best)
                {
                    best = d;
                    idx = i;
                }
            }
            return idx;
        }

        private static float JobAffinity(JobType job, Meeple m)
        {
            return job switch
            {
                JobType.ChopTree => m.Strength,
                JobType.HaulLog => m.Dexterity,
                JobType.CarryLogToCarpenter => 100f,
                _ => 0f
            };
        }
    }
}
