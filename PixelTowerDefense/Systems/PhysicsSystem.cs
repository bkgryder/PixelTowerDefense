using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PixelTowerDefense.Components;
using PixelTowerDefense.Utils;

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
                        e.Pos.X += e.Dir * GameConfig.WanderSpeed * dt;
                        if (e.Pos.X < GameConfig.ArenaLeft) { e.Pos.X = GameConfig.ArenaLeft; e.Dir = 1; }
                        if (e.Pos.X > GameConfig.ArenaRight) { e.Pos.X = GameConfig.ArenaRight; e.Dir = -1; }
                        e.Angle = 0f;
                        e.AngularVel = 0f;
                        break;

                    case EnemyState.Launched:
                        e.Vel.Y += GameConfig.Gravity * dt;
                        e.Pos += e.Vel * dt;
                        e.Angle += e.AngularVel * dt;

                        if (e.Pos.Y < e.MaxAirY) e.MaxAirY = e.Pos.Y;

                        Vector2 head = e.GetPartPos(-1);
                        if (head.Y < GameConfig.ArenaTop)
                        {
                            float dy = GameConfig.ArenaTop - head.Y;
                            e.Pos.Y += dy;
                            e.Vel.Y *= -0.6f;
                            e.AngularVel *= 0.7f;
                        }
                        Vector2 feet = e.GetPartPos(1);
                        if (feet.Y > GameConfig.FloorY)
                        {
                            float fallDist = GameConfig.FloorY - e.MaxAirY;
                            if (fallDist > GameConfig.FallExplodeThreshold)
                            {
                                ExplodeEnemy(e, pixels);
                                enemies.RemoveAt(i);
                                continue;
                            }
                            else if (fallDist > GameConfig.FallStunThreshold)
                            {
                                e.Pos.Y = GameConfig.FloorY - 1f;
                                e.State = EnemyState.Stunned;
                                e.StunTimer = GameConfig.StunTime;
                                e.Angle = MathHelper.PiOver2;
                                e.AngularVel = 0f;
                                e.Vel = Vector2.Zero;
                            }
                            else
                            {
                                e.Pos.Y = GameConfig.FloorY - 1f;
                                e.State = EnemyState.Walking;
                                e.Angle = 0f;
                                e.AngularVel = 0f;
                                e.Vel = Vector2.Zero;
                            }
                        }
                        Vector2 leftMost = e.GetPartPos(-1);
                        Vector2 rightMost = e.GetPartPos(1);
                        if (leftMost.X < GameConfig.ArenaLeft)
                        {
                            float dx = GameConfig.ArenaLeft - leftMost.X;
                            e.Pos.X += dx;
                            e.Vel.X *= -0.6f;
                            e.AngularVel *= 0.7f;
                        }
                        if (rightMost.X > GameConfig.ArenaRight)
                        {
                            float dx = rightMost.X - GameConfig.ArenaRight;
                            e.Pos.X -= dx;
                            e.Vel.X *= -0.6f;
                            e.AngularVel *= 0.7f;
                        }
                        break;

                    case EnemyState.Stunned:
                        e.Pos.Y = GameConfig.FloorY - 1f;
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
                p.Vel.Y += GameConfig.Gravity * dt;
                p.Pos += p.Vel * dt;

                if (p.Pos.X < GameConfig.ArenaLeft) { p.Pos.X = GameConfig.ArenaLeft; p.Vel.X *= -0.5f; }
                if (p.Pos.X > GameConfig.ArenaRight - 1) { p.Pos.X = GameConfig.ArenaRight - 1; p.Vel.X *= -0.5f; }
                if (p.Pos.Y < GameConfig.ArenaTop) { p.Pos.Y = GameConfig.ArenaTop; p.Vel.Y *= -0.5f; }
                if (p.Pos.Y >= GameConfig.FloorY - 1) { p.Pos.Y = GameConfig.FloorY - 1; p.Vel = Vector2.Zero; }

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
