using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PixelTowerDefense.Entities;
using PixelTowerDefense.Utils;
using PixelTowerDefense;

namespace PixelTowerDefense.Systems
{
    public class PhysicsSystem
    {
        public void UpdateEnemies(float dt, List<Enemy> enemies, List<Pixel> pixels)
        {
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                var e = enemies[i];

                switch (e.State)
                {
                    case EnemyState.Walking:
                        e.Pos.X += e.Dir * GameConstants.WanderSpeed * dt;
                        if (e.Pos.X < GameConstants.ArenaLeft) { e.Pos.X = GameConstants.ArenaLeft; e.Dir = 1; }
                        if (e.Pos.X > GameConstants.ArenaRight) { e.Pos.X = GameConstants.ArenaRight; e.Dir = -1; }
                        e.Angle = 0f;
                        e.AngularVel = 0f;
                        break;

                    case EnemyState.Launched:
                        e.Vel.Y += GameConstants.Gravity * dt;
                        e.Pos += e.Vel * dt;
                        e.Angle += e.AngularVel * dt;

                        if (e.Pos.Y < e.MaxAirY) e.MaxAirY = e.Pos.Y;

                        Vector2 head = e.GetPartPos(-1);
                        if (head.Y < GameConstants.ArenaTop)
                        {
                            float dy = GameConstants.ArenaTop - head.Y;
                            e.Pos.Y += dy;
                            e.Vel.Y *= GameConstants.WallBounce;
                            e.AngularVel *= GameConstants.WallAngularDamping;
                        }
                        Vector2 feet = e.GetPartPos(1);
                        if (feet.Y > GameConstants.FloorY)
                        {
                            float fallDist = GameConstants.FloorY - e.MaxAirY;
                            if (fallDist > GameConstants.FallExplodeThreshold)
                            {
                                ExplodeEnemy(e, pixels);
                                enemies.RemoveAt(i);
                                continue;
                            }
                            else if (fallDist > GameConstants.FallStunThreshold)
                            {
                                e.Pos.Y = GameConstants.FloorY - 1f;
                                e.State = EnemyState.Stunned;
                                e.StunTimer = GameConstants.StunTime;
                                e.Angle = MathHelper.PiOver2;
                                e.AngularVel = 0f;
                                e.Vel = Vector2.Zero;
                            }
                            else
                            {
                                e.Pos.Y = GameConstants.FloorY - 1f;
                                e.State = EnemyState.Walking;
                                e.Angle = 0f;
                                e.AngularVel = 0f;
                                e.Vel = Vector2.Zero;
                            }
                        }
                        Vector2 leftMost = e.GetPartPos(-1);
                        Vector2 rightMost = e.GetPartPos(1);
                        if (leftMost.X < GameConstants.ArenaLeft)
                        {
                            float dx = GameConstants.ArenaLeft - leftMost.X;
                            e.Pos.X += dx;
                            e.Vel.X *= GameConstants.WallBounce;
                            e.AngularVel *= GameConstants.WallAngularDamping;
                        }
                        if (rightMost.X > GameConstants.ArenaRight)
                        {
                            float dx = rightMost.X - GameConstants.ArenaRight;
                            e.Pos.X -= dx;
                            e.Vel.X *= GameConstants.WallBounce;
                            e.AngularVel *= GameConstants.WallAngularDamping;
                        }
                        break;

                    case EnemyState.Stunned:
                        e.Pos.Y = GameConstants.FloorY - 1f;
                        e.Angle = MathHelper.PiOver2;
                        e.AngularVel = 0f;
                        break;
                }
                enemies[i] = e;
            }
        }

        public void UpdatePixels(float dt, List<Pixel> pixels)
        {
            for (int i = pixels.Count - 1; i >= 0; i--)
            {
                var p = pixels[i];
                p.Vel.Y += GameConstants.Gravity * dt;
                p.Pos += p.Vel * dt;

                if (p.Pos.X < GameConstants.ArenaLeft) { p.Pos.X = GameConstants.ArenaLeft; p.Vel.X *= GameConstants.PixelBounce; }
                if (p.Pos.X > GameConstants.ArenaRight - 1) { p.Pos.X = GameConstants.ArenaRight - 1; p.Vel.X *= GameConstants.PixelBounce; }
                if (p.Pos.Y < GameConstants.ArenaTop) { p.Pos.Y = GameConstants.ArenaTop; p.Vel.Y *= GameConstants.PixelBounce; }
                if (p.Pos.Y >= GameConstants.FloorY - 1) { p.Pos.Y = GameConstants.FloorY - 1; p.Vel = Vector2.Zero; }

                pixels[i] = p;
            }
        }

        private void ExplodeEnemy(Enemy e, List<Pixel> pixels)
        {
            for (int part = -1; part <= 1; part++)
            {
                Vector2 pos = e.GetPartPos(part);
                Color c = part == -1
                    ? new Color(255, 80, 90)
                    : part == 0
                        ? new Color(220, 40, 40)
                        : new Color(110, 10, 15);

                float vx = new System.Random().NextFloat(-28, 28);
                float vy = new System.Random().NextFloat(-82, -34);
                var vel = new Vector2(vx, vy);

                pixels.Add(new Pixel(pos, vel, c));
            }
        }
    }
}
