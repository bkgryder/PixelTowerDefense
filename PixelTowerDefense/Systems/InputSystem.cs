using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PixelTowerDefense.Entities;
using PixelTowerDefense.Helpers;
using PixelTowerDefense;

namespace PixelTowerDefense.Systems
{
    public class InputSystem
    {
        private KeyboardState _prevKb;
        private MouseState _prevMs;
        private bool _dragging;
        private int _dragIdx = -1;
        private int _dragPart;
        private Vector2 _dragPrevMouseWorld;
        private Vector2 _launchVel;

        public float CamX { get; private set; }
        public float CamY { get; private set; }
        public float Zoom { get; private set; } = GameConstants.DefaultZoom;

        public InputSystem(float camX = 0, float camY = 0, float zoom = GameConstants.DefaultZoom)
        {
            CamX = camX;
            CamY = camY;
            Zoom = zoom;
        }

        public void Update(float dt, List<Enemy> enemies, Action spawnEnemy)
        {
            var kb = Keyboard.GetState();
            var ms = Mouse.GetState();

            if (Edge(kb, Keys.OemPlus) || Edge(kb, Keys.Add))
                Zoom = MathHelper.Clamp(Zoom * GameConstants.ZoomInFactor, GameConstants.MinZoom, GameConstants.MaxZoom);
            if (Edge(kb, Keys.OemMinus) || Edge(kb, Keys.Subtract))
                Zoom = MathHelper.Clamp(Zoom * GameConstants.ZoomOutFactor, GameConstants.MinZoom, GameConstants.MaxZoom);

            float camSp = GameConstants.CameraSpeed / Zoom;
            if (kb.IsKeyDown(Keys.W)) CamY -= camSp * dt;
            if (kb.IsKeyDown(Keys.S)) CamY += camSp * dt;
            if (kb.IsKeyDown(Keys.A)) CamX -= camSp * dt;
            if (kb.IsKeyDown(Keys.D)) CamX += camSp * dt;

            if (Edge(kb, Keys.P)) spawnEnemy?.Invoke();

            var mouseScreen = new Point(ms.X, ms.Y);
            var mouseWorldF = new Vector2(CamX + mouseScreen.X / Zoom, CamY + mouseScreen.Y / Zoom);

            bool mPress = ms.LeftButton == ButtonState.Pressed && _prevMs.LeftButton == ButtonState.Released;
            bool mRel = ms.LeftButton == ButtonState.Released && _prevMs.LeftButton == ButtonState.Pressed;

            if (!_dragging && mPress)
            {
                float minDist = GameConstants.GrabDistance;
                for (int i = enemies.Count - 1; i >= 0; i--)
                {
                    var e = enemies[i];
                    if (e.State != EnemyState.Walking) continue;
                    for (int part = -1; part <= 1; part++)
                    {
                        Vector2 partPos = e.GetPartPos(part);
                        float d = Vector2.Distance(partPos, mouseWorldF);
                        if (d < minDist)
                        {
                            minDist = d;
                            _dragging = true; _dragIdx = i; _dragPart = part;
                            _dragPrevMouseWorld = mouseWorldF;
                        }
                    }
                    if (_dragging) break;
                }
            }
            if (_dragging && _dragIdx >= 0 && _dragIdx < enemies.Count)
            {
                var e = enemies[_dragIdx];
                float l = _dragPart * GameConstants.DragPartOffset;
                Vector2 grabVec = new Vector2(MathF.Cos(e.Angle) * l, MathF.Sin(e.Angle) * l);

                float spring = GameConstants.DragSpring, damping = GameConstants.DragDamping;
                float targetAngle = MathF.Atan2(mouseWorldF.Y - e.Pos.Y, mouseWorldF.X - e.Pos.X);
                if (_dragPart != 0)
                {
                    targetAngle -= MathF.Asin(_dragPart * GameConstants.DragPartOffset / GameConstants.EnemyHeight);
                }
                float angleDiff = targetAngle - e.Angle;
                angleDiff = (angleDiff + MathF.PI) % (MathF.PI * 2) - MathF.PI;
                e.AngularVel += angleDiff * spring * dt;
                e.AngularVel *= MathF.Exp(-damping * dt);
                e.Angle += e.AngularVel * dt;

                e.Pos = mouseWorldF - grabVec;
                e.Vel = (mouseWorldF - _dragPrevMouseWorld) / Math.Max(dt, GameConstants.DragEpsilon);
                _launchVel = e.Vel;
                _dragPrevMouseWorld = mouseWorldF;

                enemies[_dragIdx] = e;

                if (mRel)
                {
                    e.State = EnemyState.Launched;
                    e.Vel = _launchVel;
                    e.AngularVel = _launchVel.X * GameConstants.LaunchAngularFactor;
                    e.MaxAirY = e.Pos.Y;
                    enemies[_dragIdx] = e;
                    _dragging = false; _dragIdx = -1;
                }
            }

            _prevKb = kb;
            _prevMs = ms;
        }

        private bool Edge(KeyboardState kb, Keys k) => kb.IsKeyDown(k) && _prevKb.IsKeyUp(k);
    }
}
