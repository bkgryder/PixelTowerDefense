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

        private static int FindNearestEnemyIndex(int meIdx, List<Meeple> meeples, float radius)
        {
            var me = meeples[meIdx];
            int idx = -1;
            float best = radius;
            for (int i = 0; i < meeples.Count; i++)
            {
                if (i == meIdx) continue;
                var other = meeples[i];
                if (other.Side == me.Side || other.Combatant == null || !other.Alive) continue;
                float d = Vector2.Distance(me.Pos, other.Pos);
                if (d < best)
                {
                    best = d;
                    idx = i;
                }
            }
            return idx;
        }

        public static void ResolveCombat(List<Meeple> meeples, List<Pixel> debris, float dt)
        {
            for (int i = 0; i < meeples.Count; i++)
            {
                var m = meeples[i];
                if (m.Combatant == null || !m.Alive) continue;

                int enemyIdx = FindNearestEnemyIndex(i, meeples, Constants.SEEK_RADIUS);
                switch (m.State)
                {
                    case MeepleState.Idle:
                        if (enemyIdx >= 0)
                            m.State = MeepleState.Charging;
                        m.Angle = 0f;
                        m.AngularVel = 0f;
                        break;

                    case MeepleState.Charging:
                        if (enemyIdx < 0)
                        {
                            m.State = MeepleState.Idle;
                            m.Vel = Vector2.Zero;
                            break;
                        }
                        var target = meeples[enemyIdx];
                        Vector2 dir = target.Pos - m.Pos;
                        float dist = dir.Length();
                        if (dist > 0f) dir /= dist;
                        m.Vel = dir * Constants.WANDER_SPEED * 1.8f;
                        m.Pos += m.Vel * dt;
                        if (dist < Constants.TOUCH_RANGE)
                            m.State = MeepleState.Melee;
                        m.Angle = 0f;
                        m.AngularVel = 0f;
                        break;

                    case MeepleState.Melee:
                        if (enemyIdx < 0)
                        {
                            m.State = MeepleState.Idle;
                            m.Vel = Vector2.Zero;
                            break;
                        }
                        target = meeples[enemyIdx];
                        dir = target.Pos - m.Pos;
                        dist = dir.Length();
                        m.Vel = Vector2.Zero;
                        m.Angle = 0f;
                        m.AngularVel = 0f;
                        if (dist > Constants.TOUCH_RANGE)
                        {
                            m.State = MeepleState.Charging;
                            break;
                        }
                        var combat = m.Combatant.Value;
                        if (combat.AttackCooldown > 0f)
                            combat.AttackCooldown -= dt;
                        if (combat.AttackCooldown <= 0f)
                        {
                            target.Health -= Constants.MELEE_DMG;
                            combat.AttackCooldown = Constants.ATTACK_WINDUP;
                            if (target.Health <= 0f)
                            {
                                target.Health = 0f;
                                target.State = MeepleState.Ragdoll;
                                target.IsBurning = false;
                                Vector2 knock = target.Pos - m.Pos;
                                if (knock.LengthSquared() > 0f)
                                    knock.Normalize();
                                target.Vel = knock * Constants.MELEE_KNOCKBACK;
                                target.z = 0f;
                                target.vz = Constants.MELEE_KNOCKBACK_UPWARD;
                                target.AngularVel = _rng.NextFloat(-4f, 4f);
                                EmitBlood(target.Pos, debris);
                            }
                        }
                        if (!target.Alive)
                            m.State = MeepleState.Idle;
                        meeples[enemyIdx] = target;
                        m.Combatant = combat;
                        break;
                }
                meeples[i] = m;
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
                debris.Spawn(new Pixel(
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
