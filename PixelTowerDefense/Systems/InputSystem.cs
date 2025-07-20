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
            Vector2 prevMouseWorld,
            List<Meeple> meeples,
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
                for (int i = meeples.Count - 1; i >= 0; i--)
                {
                    var e = meeples[i];
                    if (e.State != MeepleState.Idle) continue;
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
                            meeples[i] = e;
                        }
                    }
                    if (dragging) break;
                }
            }

            // follow drag
            if (dragging && dragIdx >= 0)
            {
                if (dragIdx >= meeples.Count)
                {
                    dragging = false;
                    dragIdx = -1;
                    return;
                }

                var e = meeples[dragIdx];

                float l = dragPart * Constants.PART_LEN;
                var grabVec = new Vector2(
                    MathF.Sin(e.Angle) * l,
                    MathF.Cos(e.Angle) * l
                );

                float spring = Constants.DRAG_SPRING,
                      damping = Constants.DRAG_DAMPING;
                float targetAngle = MathF.Atan2(mouseWorld.X - e.Pos.X,
                                                mouseWorld.Y - e.Pos.Y);
                if (dragPart != 0)
                    targetAngle -= MathF.Asin(dragPart * Constants.PART_LEN / (Constants.ENEMY_H * 0.5f));

                float prevTargetAngle = MathF.Atan2(prevMouseWorld.X - e.Pos.X,
                                                   prevMouseWorld.Y - e.Pos.Y);
                if (dragPart != 0)
                    prevTargetAngle -= MathF.Asin(dragPart * Constants.PART_LEN / (Constants.ENEMY_H * 0.5f));

                float diff = targetAngle - e.Angle;
                diff = (diff + MathF.PI) % (2 * MathF.PI) - MathF.PI;

                float mouseAngVel = targetAngle - prevTargetAngle;
                mouseAngVel = (mouseAngVel + MathF.PI) % (2 * MathF.PI) - MathF.PI;

                float dt = (float)gt.ElapsedGameTime.TotalSeconds;
                e.AngularVel += (diff * spring + mouseAngVel / dt) * dt;
                e.AngularVel *= MathF.Exp(-damping * dt);
                e.Angle += e.AngularVel * dt;

                e.Pos = mouseWorld - grabVec;
                e.z = Constants.GRAB_Z;
                e.vz = 0f;

                meeples[dragIdx] = e;
            }

            // release / throw
            if (dragging && mRel && dragIdx >= 0)
            {
                if (dragIdx >= meeples.Count)
                {
                    dragging = false;
                    dragIdx = -1;
                    return;
                }

                var e = meeples[dragIdx];
                float t2 = (float)gt.TotalGameTime.TotalSeconds;
                float dt = MathF.Max(0.01f, t2 - dragStartTime);
                var delta = mouseWorld - dragStartWorld;
                var avgV = delta / dt;

                e.Vel = avgV * Constants.THROW_SENSITIVITY;
                e.vz = avgV.Length() * Constants.THROW_VZ_SCALE + Constants.INITIAL_Z * 2f;

                if (Math.Abs(e.AngularVel) > Constants.VOMIT_SPIN_THRESHOLD)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        var head = e.GetPartPos(-2);
                        var v = new Vector2(
                            _rng.NextFloat(-30, 30),
                            -_rng.NextFloat(20, 50)
                        );
                        debris.Spawn(new Pixel(
                            head,
                            v,
                            Color.LimeGreen,
                            0f,
                            _rng.NextFloat(Constants.DEBRIS_LIFETIME_MIN,
                                          Constants.DEBRIS_LIFETIME_MAX)));
                    }
                }

                e.State = MeepleState.Launched;
                e.AngularVel = e.Vel.X * 0.05f;
                meeples[dragIdx] = e;

                dragging = false;
                dragIdx = -1;
            }
        }
    }
}
