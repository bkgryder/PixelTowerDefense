using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PixelTowerDefense.Entities;
using PixelTowerDefense.Utils;

namespace PixelTowerDefense.Systems
{
    public static class CombatSystem
    {
        private static Random _rng = new Random();

        private static int FindNearestEnemyIndex(Soldier me, List<Soldier> soldiers, float radius)
        {
            int idx = -1;
            float best = radius;
            for (int i = 0; i < soldiers.Count; i++)
            {
                if (soldiers[i].Side == me.Side || !soldiers[i].Alive) continue;
                float d = Vector2.Distance(me.Pos, soldiers[i].Pos);
                if (d < best)
                {
                    best = d;
                    idx = i;
                }
            }
            return idx;
        }

        public static void ResolveCombat(List<Soldier> soldiers, List<Pixel> debris, float dt)
        {
            for (int i = 0; i < soldiers.Count; i++)
            {
                var s = soldiers[i];
                if (!s.Alive) continue;

                int enemyIdx = FindNearestEnemyIndex(s, soldiers, Constants.SEEK_RADIUS);
                switch (s.State)
                {
                    case SoldierState.Idle:
                        if (enemyIdx >= 0)
                            s.State = SoldierState.Charging;
                        s.Angle = 0f;
                        s.AngularVel = 0f;
                        break;

                    case SoldierState.Charging:
                        if (enemyIdx < 0)
                        {
                            s.State = SoldierState.Idle;
                            s.Vel = Vector2.Zero;
                            break;
                        }
                        var target = soldiers[enemyIdx];
                        Vector2 dir = target.Pos - s.Pos;
                        float dist = dir.Length();
                        if (dist > 0f) dir /= dist;
                        s.Vel = dir * Constants.WANDER_SPEED * 1.8f;
                        s.Pos += s.Vel * dt;
                        if (dist < Constants.TOUCH_RANGE)
                            s.State = SoldierState.Melee;
                        s.Angle = 0f;
                        s.AngularVel = 0f;
                        break;

                    case SoldierState.Melee:
                        if (enemyIdx < 0)
                        {
                            s.State = SoldierState.Idle;
                            s.Vel = Vector2.Zero;
                            break;
                        }
                        target = soldiers[enemyIdx];
                        dir = target.Pos - s.Pos;
                        dist = dir.Length();
                        s.Vel = Vector2.Zero;
                        s.Angle = 0f;
                        s.AngularVel = 0f;
                        if (dist > Constants.TOUCH_RANGE)
                        {
                            s.State = SoldierState.Charging;
                            break;
                        }
                        if (s.Combat.AttackCooldown > 0f)
                            s.Combat.AttackCooldown -= dt;
                        if (s.Combat.AttackCooldown <= 0f)
                        {
                            target.Combat.Health -= Constants.MELEE_DMG;
                            s.Combat.AttackCooldown = Constants.ATTACK_WINDUP;
                            if (target.Combat.Health <= 0f)
                            {
                                target.Combat.Health = 0f;
                                target.State = SoldierState.Ragdoll;
                                target.IsBurning = false;
                                Vector2 knock = target.Pos - s.Pos;
                                if (knock.LengthSquared() > 0f)
                                    knock.Normalize();
                                target.Vel = knock * Constants.MELEE_KNOCKBACK;
                                target.z = 0f;
                                target.vz = Constants.MELEE_KNOCKBACK_UPWARD;
                                // preserve target.Angle so the body keeps its
                                // current orientation when entering ragdoll
                                target.AngularVel = _rng.NextFloat(-4f, 4f);
                                EmitBlood(target.Pos, debris);
                            }
                        }
                        if (!target.Alive)
                            s.State = SoldierState.Idle;
                        soldiers[enemyIdx] = target;
                        break;
                }
                soldiers[i] = s;
            }
        }

        private static void EmitBlood(Vector2 pos, List<Pixel> debris)
        {
            Color blood = new Color(160, 0, 0);
            int count = _rng.Next(6, 10);
            for (int i = 0; i < count; i++)
            {
                Vector2 off = new Vector2(_rng.NextFloat(-1f, 1f), _rng.NextFloat(-1f, 1f));
                Vector2 vel = new Vector2(_rng.NextFloat(-15f, 15f), _rng.NextFloat(-15f, 0f));
                debris.Add(new Pixel(
                    pos + off,
                    vel,
                    blood,
                    0f,
                    _rng.NextFloat(Constants.DEBRIS_LIFETIME_MIN,
                                   Constants.DEBRIS_LIFETIME_MAX)));
            }
        }
    }
}
