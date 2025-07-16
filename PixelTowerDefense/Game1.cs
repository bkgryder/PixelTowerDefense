using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PixelTowerDefense
{
    public class Game1 : Game
    {
        // Arena bounds and constants
        private const int ENEMY_W = 1, ENEMY_H = 3;
        private const int FLOOR_Y = 100, ARENA_LEFT = 0, ARENA_RIGHT = 100, ARENA_TOP = 0;
        private const float GRAVITY = 98.0f, WANDER_SPEED = 10f;
        private const float FALL_EXPLODE_THRESHOLD = 24f; // Explode if fall distance > this
        private const float FALL_STUN_THRESHOLD = 4f;     // Stun if fall distance > this
        private const float STUN_TIME = 1.5f;             // Seconds to stay stunned

        // Engine/State
        private GraphicsDeviceManager _gfx;
        private SpriteBatch _sb;
        private Texture2D _px;
        private List<Enemy> _enemies = new();
        private List<Pixel> _pixels = new();
        private Random _rng = new();
        private float _camX, _camY, _zoom = 4f;
        private KeyboardState _prevKb;
        private MouseState _prevMs;
        private bool _dragging;
        private int _dragIdx = -1, _dragPart = 0;
        private Vector2 _dragPrevMouseWorld, _launchVel;

        public Game1() { _gfx = new GraphicsDeviceManager(this); Content.RootDirectory = "Content"; IsMouseVisible = true; }
        protected override void Initialize()
        {
            _gfx.PreferredBackBufferWidth = 1280; _gfx.PreferredBackBufferHeight = 720; _gfx.ApplyChanges();
            base.Initialize();
        }
        protected override void LoadContent()
        {
            _sb = new SpriteBatch(GraphicsDevice);
            _px = new Texture2D(GraphicsDevice, 1, 1);
            _px.SetData(new[] { Color.White });
            for (int i = 0; i < 10; i++) SpawnEnemy();
            float mid = (ARENA_LEFT + ARENA_RIGHT) * 0.5f;
            _camX = mid - (_gfx.PreferredBackBufferWidth * 0.5f) / _zoom;
            _camY = 0;
        }

        protected override void Update(GameTime gt)
        {
            float dt = (float)gt.ElapsedGameTime.TotalSeconds;
            var kb = Keyboard.GetState(); var ms = Mouse.GetState();
            if (kb.IsKeyDown(Keys.Escape)) Exit();

            // Zoom
            if (Edge(kb, Keys.OemPlus) || Edge(kb, Keys.Add)) _zoom = MathHelper.Clamp(_zoom * 2f, 0.5f, 8f);
            if (Edge(kb, Keys.OemMinus) || Edge(kb, Keys.Subtract)) _zoom = MathHelper.Clamp(_zoom * 0.5f, 0.5f, 8f);

            // WASD Camera
            float camSp = 400f / _zoom;
            if (kb.IsKeyDown(Keys.W)) _camY -= camSp * dt;
            if (kb.IsKeyDown(Keys.S)) _camY += camSp * dt;
            if (kb.IsKeyDown(Keys.A)) _camX -= camSp * dt;
            if (kb.IsKeyDown(Keys.D)) _camX += camSp * dt;

            // Spawn on P
            if (Edge(kb, Keys.P)) SpawnEnemy();

            // Mouse to world
            var mouseScreen = new Point(ms.X, ms.Y);
            var mouseWorldF = new Vector2(_camX + mouseScreen.X / _zoom, _camY + mouseScreen.Y / _zoom);

            bool mPress = ms.LeftButton == ButtonState.Pressed && _prevMs.LeftButton == ButtonState.Released;
            bool mRel = ms.LeftButton == ButtonState.Released && _prevMs.LeftButton == ButtonState.Pressed;

            // --- DRAGGING ---
            if (!_dragging && mPress)
            {
                float minDist = 2f;
                for (int i = _enemies.Count - 1; i >= 0; i--)
                {
                    var e = _enemies[i];
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
            if (_dragging && _dragIdx >= 0 && _dragIdx < _enemies.Count)
            {
                var e = _enemies[_dragIdx];
                float l = _dragPart * 1.5f;
                Vector2 grabVec = new Vector2(MathF.Cos(e.Angle) * l, MathF.Sin(e.Angle) * l);

                float spring = 16f, damping = 8f;
                float targetAngle = MathF.Atan2(mouseWorldF.Y - e.Pos.Y, mouseWorldF.X - e.Pos.X);
                if (_dragPart != 0)
                {
                    targetAngle -= MathF.Asin(_dragPart * 1.5f / 3f);
                }
                float angleDiff = targetAngle - e.Angle;
                angleDiff = (angleDiff + MathF.PI) % (MathF.PI * 2) - MathF.PI;
                e.AngularVel += angleDiff * spring * dt;
                e.AngularVel *= MathF.Exp(-damping * dt);
                e.Angle += e.AngularVel * dt;

                e.Pos = mouseWorldF - grabVec;
                e.Vel = (mouseWorldF - _dragPrevMouseWorld) / Math.Max(dt, 0.001f);
                _launchVel = e.Vel;
                _dragPrevMouseWorld = mouseWorldF;

                _enemies[_dragIdx] = e;

                if (mRel)
                {
                    e.State = EnemyState.Launched;
                    e.Vel = _launchVel;
                    e.AngularVel = _launchVel.X * 0.005f;
                    e.MaxAirY = e.Pos.Y;
                    _enemies[_dragIdx] = e;
                    _dragging = false; _dragIdx = -1;
                }
            }

            UpdateEnemies(dt);
            UpdatePixels(dt);

            _prevKb = kb; _prevMs = ms;
            base.Update(gt);
        }

        private bool Edge(KeyboardState kb, Keys k) => kb.IsKeyDown(k) && _prevKb.IsKeyUp(k);

        private void SpawnEnemy()
        {
            float x = _rng.NextFloat(ARENA_LEFT + 10, ARENA_RIGHT - 10);
            Color shirt = RandomShirtColor(_rng);
            var e = new Enemy(new Vector2(x, FLOOR_Y - 1f), shirt);
            _enemies.Add(e);
        }

        private static Color RandomShirtColor(Random rng)
        {
            Color[] palette = { Color.Blue, Color.Green, Color.Red, Color.Yellow, Color.Purple, Color.Orange, Color.Cyan };
            return palette[rng.Next(palette.Length)];
        }

        private void UpdateEnemies(float dt)
        {
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                var e = _enemies[i];

                switch (e.State)
                {
                    case EnemyState.Walking:
                        if (_dragging && i == _dragIdx) continue;
                        e.WanderTimer -= dt;
                        if (e.WanderTimer <= 0)
                        {
                            e.Dir = _rng.NextFloat(-1f, 1f) < 0 ? -1 : 1;
                            e.WanderTimer = _rng.NextFloat(1f, 3f);
                        }
                        e.Pos.X += e.Dir * WANDER_SPEED * dt;
                        if (e.Pos.X < ARENA_LEFT) { e.Pos.X = ARENA_LEFT; e.Dir = 1; }
                        if (e.Pos.X > ARENA_RIGHT) { e.Pos.X = ARENA_RIGHT; e.Dir = -1; }
                        e.Angle = 0f;
                        e.AngularVel = 0f;
                        break;

                    case EnemyState.Launched:
                        e.Vel.Y += GRAVITY * dt;
                        e.Pos += e.Vel * dt;
                        e.Angle += e.AngularVel * dt;

                        if (e.Pos.Y < e.MaxAirY) e.MaxAirY = e.Pos.Y;

                        // Ceiling
                        Vector2 head = e.GetPartPos(-1);
                        if (head.Y < ARENA_TOP)
                        {
                            float dy = ARENA_TOP - head.Y;
                            e.Pos.Y += dy;
                            e.Vel.Y *= -0.6f;
                            e.AngularVel *= 0.7f;
                        }
                        // Floor
                        Vector2 feet = e.GetPartPos(1);
                        if (feet.Y > FLOOR_Y)
                        {
                            float fallDist = FLOOR_Y - e.MaxAirY;
                            if (fallDist > FALL_EXPLODE_THRESHOLD)
                            {
                                ExplodeEnemy(e);
                                _enemies.RemoveAt(i);
                                continue;
                            }
                            else if (fallDist > FALL_STUN_THRESHOLD)
                            {
                                // Lay stunned
                                e.Pos.Y = FLOOR_Y - 1f;
                                e.State = EnemyState.Stunned;
                                e.StunTimer = STUN_TIME;
                                e.Angle = MathHelper.PiOver2; // Lay flat
                                e.AngularVel = 0f;
                                e.Vel = Vector2.Zero;
                            }
                            else
                            {
                                // Just get up and walk again
                                e.Pos.Y = FLOOR_Y - 1f;
                                e.State = EnemyState.Walking;
                                e.Angle = 0f;
                                e.AngularVel = 0f;
                                e.Vel = Vector2.Zero;
                                e.WanderTimer = _rng.NextFloat(0.5f, 2.5f);
                                e.Dir = _rng.NextFloat(-1f, 1f) < 0 ? -1 : 1;
                            }
                        }
                        // Walls
                        Vector2 leftMost = e.GetPartPos(-1);
                        Vector2 rightMost = e.GetPartPos(1);
                        if (leftMost.X < ARENA_LEFT)
                        {
                            float dx = ARENA_LEFT - leftMost.X;
                            e.Pos.X += dx;
                            e.Vel.X *= -0.6f;
                            e.AngularVel *= 0.7f;
                        }
                        if (rightMost.X > ARENA_RIGHT)
                        {
                            float dx = rightMost.X - ARENA_RIGHT;
                            e.Pos.X -= dx;
                            e.Vel.X *= -0.6f;
                            e.AngularVel *= 0.7f;
                        }
                        break;

                    case EnemyState.Stunned:
                        e.StunTimer -= dt;
                        if (e.StunTimer <= 0)
                        {
                            e.State = EnemyState.Walking;
                            e.Angle = 0f;
                            e.AngularVel = 0f;
                            e.WanderTimer = _rng.NextFloat(0.5f, 2.5f);
                            e.Dir = _rng.NextFloat(-1f, 1f) < 0 ? -1 : 1;
                        }
                        // Keep them on ground, flat
                        e.Pos.Y = FLOOR_Y - 1f;
                        e.Angle = MathHelper.PiOver2;
                        e.AngularVel = 0f;
                        break;
                }
                _enemies[i] = e;
            }
        }

        private void UpdatePixels(float dt)
        {
            for (int i = _pixels.Count - 1; i >= 0; i--)
            {
                var p = _pixels[i];
                p.Vel.Y += GRAVITY * dt;
                p.Pos += p.Vel * dt;

                if (p.Pos.X < ARENA_LEFT) { p.Pos.X = ARENA_LEFT; p.Vel.X *= -0.5f; }
                if (p.Pos.X > ARENA_RIGHT - 1) { p.Pos.X = ARENA_RIGHT - 1; p.Vel.X *= -0.5f; }
                if (p.Pos.Y < ARENA_TOP) { p.Pos.Y = ARENA_TOP; p.Vel.Y *= -0.5f; }
                if (p.Pos.Y >= FLOOR_Y - 1) { p.Pos.Y = FLOOR_Y - 1; p.Vel = Vector2.Zero; }

                _pixels[i] = p;
            }
        }

        private void ExplodeEnemy(Enemy e)
        {
            for (int part = -1; part <= 1; part++)
            {
                Vector2 pos = e.GetPartPos(part);
                Color c = part == -1
                    ? new Color(255, 80, 90)
                    : part == 0
                        ? new Color(220, 40, 40)
                        : new Color(110, 10, 15);

                float vx = _rng.NextFloat(-28, 28);
                float vy = _rng.NextFloat(-82, -34);
                var vel = new Vector2(vx, vy);

                _pixels.Add(new Pixel(pos, vel, c));
            }
        }

        // Draw
        protected override void Draw(GameTime gt)
        {
            GraphicsDevice.Clear(Color.Black);
            var cam = Matrix.CreateScale(_zoom, _zoom, 1f)
                * Matrix.CreateTranslation(-_camX * _zoom, -_camY * _zoom, 0);
            _sb.Begin(transformMatrix: cam);

            // Arena outline
            int thickness = 2;
            _sb.Draw(_px, new Rectangle(ARENA_LEFT, ARENA_TOP, ARENA_RIGHT - ARENA_LEFT, thickness), Color.DimGray);
            _sb.Draw(_px, new Rectangle(ARENA_LEFT, FLOOR_Y, ARENA_RIGHT - ARENA_LEFT, thickness), Color.DimGray);
            _sb.Draw(_px, new Rectangle(ARENA_LEFT, ARENA_TOP, thickness, FLOOR_Y - ARENA_TOP), Color.DimGray);
            _sb.Draw(_px, new Rectangle(ARENA_RIGHT - thickness, ARENA_TOP, thickness, FLOOR_Y - ARENA_TOP), Color.DimGray);

            // debris
            foreach (var p in _pixels) _sb.Draw(_px, p.Bounds, p.Col);

            // enemies (draw as ragdoll 3-pixel bar)
            for (int i = 0; i < _enemies.Count; i++)
            {
                var e = _enemies[i];
                for (int part = -1; part <= 1; part++)
                {
                    Vector2 partPos = e.GetPartPos(part);
                    Color c =
                        part == -1 ? new Color(255, 219, 172) :
                        part == 0 ? e.ShirtColor :
                                     new Color(68, 36, 14);
                    _sb.Draw(_px, new Rectangle((int)partPos.X, (int)partPos.Y, 1, 1), c);
                }
            }
            _sb.End();
            base.Draw(gt);
        }

        protected override void EndRun()
        {
            _px.Dispose();
            base.EndRun();
        }

        // --- Enemy as ragdoll bar ---
        private enum EnemyState { Walking, Launched, Stunned }

        private struct Enemy
        {
            public Vector2 Pos, Vel;
            public float Angle, AngularVel;
            public float Dir, WanderTimer;
            public bool IsLaunched => State == EnemyState.Launched;
            public float MaxAirY; // For fall height tracking
            public Color ShirtColor;

            public EnemyState State;
            public float StunTimer;

            public Rectangle Bounds => new((int)Pos.X - 1, (int)Pos.Y - 1, 3, 3);

            public Enemy(Vector2 p, Color shirt)
            {
                Pos = p; Vel = Vector2.Zero;
                Angle = 0; AngularVel = 0;
                Dir = 1; WanderTimer = 1;
                ShirtColor = shirt;
                MaxAirY = p.Y;
                State = EnemyState.Walking;
                StunTimer = 0f;
            }
            public Vector2 GetPartPos(int part)
            {
                float l = part * 1.0f;
                return new Vector2(
                    Pos.X + MathF.Sin(Angle) * l,
                    Pos.Y + MathF.Cos(Angle) * l
                );
            }
        }
        private struct Pixel
        {
            public Vector2 Pos, Vel;
            public Color Col;
            public Rectangle Bounds => new((int)Pos.X, (int)Pos.Y, 1, 1);
            public Pixel(Vector2 p, Vector2 v, Color c) { Pos = p; Vel = v; Col = c; }
        }
    }

    internal static class RandEx
    {
        private static Random R = new();
        public static float NextFloat(this Random _, float min, float max) => min + (float)R.NextDouble() * (max - min);
    }
}
