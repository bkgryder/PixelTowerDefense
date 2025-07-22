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
            List<Meeple> meeples,
            List<Pixel> debris,
            List<BerryBush> bushes,
            List<Building> buildings,
            List<Tree> trees,
            List<Log> logs,
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
                                        e.Vel = dir * Constants.WANDER_SPEED;
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
                                            e.Vel = dir * Constants.WANDER_SPEED;
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
                                        e.Vel = dir * Constants.WANDER_SPEED;
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
                                var wdata = e.Worker.Value;
                                if (e.CarriedLogIdx < logs.Count)
                                {
                                    var log = logs[e.CarriedLogIdx];
                                    int hidx;
                                    if (wdata.CurrentJob == JobType.DepositResource && wdata.TargetIdx != null)
                                        hidx = wdata.TargetIdx.Value;
                                    else
                                    {
                                        hidx = FindNearestCarpenter(e.Pos, buildings);
                                        if (hidx < 0)
                                            hidx = FindNearestStockpileForLogs(e.Pos, buildings);
                                        if (hidx >= 0)
                                        {
                                            wdata.CurrentJob = JobType.DepositResource;
                                            wdata.TargetIdx = hidx;
                                            var hutReserve = buildings[hidx];
                                            hutReserve.ReservedBy = i;
                                            buildings[hidx] = hutReserve;
                                        }
                                    }
                                    if (hidx >= 0)
                                    {
                                        var hut = buildings[hidx];
                                        Vector2 dir = hut.Pos - e.Pos;
                                        float dist = dir.Length();
                                        if (dist < 1f)
                                        {
                                            hut.StoredLogs++;
                                            buildings[hidx] = hut;
                                            logs.RemoveAt(e.CarriedLogIdx);
                                            e.CarriedLogIdx = -1;
                                            hut.ReservedBy = null;
                                            wdata.CurrentJob = JobType.None;
                                            wdata.TargetIdx = null;
                                            e.WanderTimer = 0f;
                                        }
                                        else
                                        {
                                            if (dist > 0f) dir /= dist; else dir = Vector2.Zero;
                                            e.Vel = dir * Constants.WANDER_SPEED;
                                            e.Pos += e.Vel * dt;
                                            log.Pos = e.Pos;
                                            logs[e.CarriedLogIdx] = log;
                                        }
                                        e.Angle = 0f;
                                        e.Worker = wdata;
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
                                var wdata2 = e.Worker.Value;
                                int lidx;
                                if (wdata2.CurrentJob == JobType.HaulLog && wdata2.TargetIdx != null)
                                    lidx = wdata2.TargetIdx.Value;
                                else
                                {
                                    lidx = FindNearestLooseLog(e.Pos, logs);
                                    if (lidx >= 0)
                                    {
                                        wdata2.CurrentJob = JobType.HaulLog;
                                        wdata2.TargetIdx = lidx;
                                        var rlog = logs[lidx];
                                        rlog.ReservedBy = i;
                                        logs[lidx] = rlog;
                                    }
                                }
                                if (lidx >= 0)
                                {
                                    var log = logs[lidx];
                                    Vector2 dir = log.Pos - e.Pos;
                                    float dist = dir.Length();
                                    if (dist < Constants.HARVEST_RANGE)
                                    {
                                        log.IsCarried = true;
                                        logs[lidx] = log;
                                        e.CarriedLogIdx = lidx;
                                        log.ReservedBy = null;
                                        wdata2.CurrentJob = JobType.None;
                                        wdata2.TargetIdx = null;
                                        e.WanderTimer = 0f;
                                    }
                                    else
                                    {
                                        if (dist > 0f) dir /= dist; else dir = Vector2.Zero;
                                        e.Vel = dir * Constants.WANDER_SPEED;
                                        e.Pos += e.Vel * dt;
                                    }
                                    e.Angle = 0f;
                                    e.Worker = wdata2;
                                    break;
                                }

                                int tidx;
                                if (wdata2.CurrentJob == JobType.ChopTree && wdata2.TargetIdx != null)
                                    tidx = wdata2.TargetIdx.Value;
                                else
                                {
                                    tidx = FindNearestTree(e.Pos, trees);
                                    if (tidx >= 0)
                                    {
                                        wdata2.CurrentJob = JobType.ChopTree;
                                        wdata2.TargetIdx = tidx;
                                        var rt = trees[tidx];
                                        rt.ReservedBy = i;
                                        trees[tidx] = rt;
                                    }
                                }
                                if (tidx >= 0)
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
                                        e.WanderTimer = 0.5f;
                                        if (tree.IsStump)
                                        {
                                            tree.ReservedBy = null;
                                            trees[tidx] = tree;
                                            wdata2.CurrentJob = JobType.None;
                                            wdata2.TargetIdx = null;
                                        }
                                    }
                                    else
                                    {
                                        if (dist > 0f) dir /= dist; else dir = Vector2.Zero;
                                        e.Vel = dir * Constants.WANDER_SPEED;
                                        e.Pos += e.Vel * dt;
                                    }
                                    e.Angle = 0f;
                                    e.Worker = wdata2;
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
                        if (e.IsBurning)
                            e.Vel = Vector2.Normalize(e.Vel) * Constants.WANDER_SPEED * Constants.BURNING_SPEED_MULT;
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
                                                  * Constants.WANDER_SPEED;
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
                                                 * Constants.WANDER_SPEED;
                            e.Angle = 0f;
                        }
                        break;
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
                    b.CraftTimer += dt;
                    if (b.CraftTimer >= Constants.CARPENTER_CRAFT_TIME)
                    {
                        b.CraftTimer = 0f;
                        b.StoredLogs--;
                        b.StoredPlanks++;
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
                    building.CraftTimer = Constants.CARPENTER_CRAFT_TIME;
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
                    building.CraftTimer = building.StoredLogs > 0 ? Constants.CARPENTER_CRAFT_TIME : 0f;
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
    }
}
