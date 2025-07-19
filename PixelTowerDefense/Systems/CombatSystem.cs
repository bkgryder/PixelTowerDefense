using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PixelTowerDefense.Entities;
using PixelTowerDefense.Utils;

namespace PixelTowerDefense.Systems
{
    public static class CombatSystem
    {
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

        public static void ResolveCombat(List<Soldier> soldiers, float dt)
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
                        s.Angle = (float)Math.Atan2(dir.X, dir.Y);
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
                        }
                        if (!target.Alive)
                            s.State = SoldierState.Idle;
                        soldiers[enemyIdx] = target;
                        break;
                }
                soldiers[i] = s;
            }
        }
    }
}
