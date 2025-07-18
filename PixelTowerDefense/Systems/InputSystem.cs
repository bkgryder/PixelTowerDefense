using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PixelTowerDefense.Entities;
using PixelTowerDefense.Utils;

namespace PixelTowerDefense.Systems
{
    public static class InputSystem
    {
        private static Random _rng = new Random();

        public static void HandleDrag(
            GameTime gt,
            MouseState ms, MouseState prevMs,
            ref bool dragging,
            ref int dragIdx,
            ref int dragPart,
            ref Vector2 dragStartWorld,
            ref float dragStartTime,
            Vector2 mouseWorld,
            List<Enemy> enemies,
            List<Pixel> debris
        )
        {
            bool mPress = ms.LeftButton == ButtonState.Pressed &&
                          prevMs.LeftButton == ButtonState.Released;
            bool mRel = ms.LeftButton == ButtonState.Released &&
                          prevMs.LeftButton == ButtonState.Pressed;

            // start drag
            if (!dragging && mPress)
            {
                float minD = Constants.PICKUP_RADIUS;
                for (int i = enemies.Count - 1; i >= 0; i--)
                {
                    var e = enemies[i];
                    if (e.State != EnemyState.Walking) continue;
                    for (int p = -2; p <= 2; p++)
                    {
                        float d = Vector2.Distance(e.GetPartPos(p), mouseWorld);
                        if (d < minD)
                        {
                            minD = d;
                            dragging = true;
                            dragIdx = i;
                            dragPart = p;
                            dragStartWorld = mouseWorld;
                            dragStartTime = (float)gt.TotalGameTime.TotalSeconds;
                            // lift
                            e.z = Constants.GRAB_Z;
                            e.vz = 0f;
                            enemies[i] = e;
                        }
                    }
                    if (dragging) break;
                }
            }

            // follow drag
            if (dragging && dragIdx >= 0 && dragIdx < enemies.Count)
            {
                var e = enemies[dragIdx];

                // simple spring‐torque on that segment
                float l = dragPart * Constants.PART_LEN;
                var grabVec = new Vector2(
                    MathF.Cos(e.Angle) * l,
                    MathF.Sin(e.Angle) * l
                );

                // spring + damping
                float spring = 16f, damping = 8f;
                float targetAngle = MathF.Atan2(mouseWorld.Y - e.Pos.Y,
                                                mouseWorld.X - e.Pos.X);
                if (dragPart != 0)
                    targetAngle -= MathF.Asin(dragPart * Constants.PART_LEN / Constants.ENEMY_H);

                float diff = targetAngle - e.Angle;
                diff = (diff + MathF.PI) % (2 * MathF.PI) - MathF.PI;
                e.AngularVel += diff * spring * (float)gt.ElapsedGameTime.TotalSeconds;
                e.AngularVel *= MathF.Exp(-damping * (float)gt.ElapsedGameTime.TotalSeconds);
                e.Angle += e.AngularVel * (float)gt.ElapsedGameTime.TotalSeconds;

                // follow mouse
                e.Pos = mouseWorld - grabVec;
                e.z = Constants.GRAB_Z;
                e.vz = 0f;

                enemies[dragIdx] = e;
            }

            // release / throw
            if (dragging && mRel && dragIdx >= 0 && dragIdx < enemies.Count)
            {
                var e = enemies[dragIdx];
                float t2 = (float)gt.TotalGameTime.TotalSeconds;
                float dt = MathF.Max(0.01f, t2 - dragStartTime);
                var delta = mouseWorld - dragStartWorld;
                var avgV = delta / dt;

                e.Vel = avgV * Constants.THROW_SENSITIVITY;
                e.vz = avgV.Length() * Constants.THROW_VZ_SCALE + Constants.INITIAL_Z * 2f;

                // puke if spinning
                if (Math.Abs(e.AngularVel) > Constants.VOMIT_SPIN_THRESHOLD)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        var head = e.GetPartPos(-2);
                        var v = new Vector2(
                            _rng.NextFloat(-30, 30),
                            -_rng.NextFloat(20, 50)
                        );
                        debris.Add(new Pixel(head, v, Color.LimeGreen));
                    }
                }

                e.State = EnemyState.Launched;
                e.AngularVel = e.Vel.X * 0.05f;
                enemies[dragIdx] = e;

                dragging = false;
                dragIdx = -1;
            }
        }
    }
}
