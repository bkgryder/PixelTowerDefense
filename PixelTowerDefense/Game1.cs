using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PixelTowerDefense.Entities;
using PixelTowerDefense.Systems;
using PixelTowerDefense.Utils;

namespace PixelTowerDefense
{
    public class Game1 : Game
    {
        GraphicsDeviceManager _gfx;
        SpriteBatch _sb;
        Texture2D _px;

        List<Enemy> _enemies = new();
        List<Pixel> _pixels = new();
        Random _rng = new();

        float _camX, _camY, _zoom = 3.5f;
        KeyboardState _prevKb;
        MouseState _prevMs;

        Ability _currentAbility = Ability.None;

        // drag state
        bool _dragging;
        int _dragIdx, _dragPart;
        Vector2 _dragStartWorld;
        float _dragStartTime;

        public Game1()
        {
            _gfx = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _gfx.PreferredBackBufferWidth = 1280;
            _gfx.PreferredBackBufferHeight = 720;
            _gfx.ApplyChanges();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _sb = new SpriteBatch(GraphicsDevice);
            _px = new Texture2D(GraphicsDevice, 1, 1);
            _px.SetData(new[] { Color.White });

            for (int i = 0; i < 10; i++)
                SpawnEnemy();

            var midX = (Constants.ARENA_LEFT + Constants.ARENA_RIGHT) * 0.5f;
            var midY = (Constants.ARENA_TOP + Constants.ARENA_BOTTOM) * 0.5f;
            _camX = midX - (GraphicsDevice.Viewport.Width * 0.5f) / _zoom;
            _camY = midY - (GraphicsDevice.Viewport.Height * 0.5f) / _zoom;
        }

        protected override void Update(GameTime gt)
        {
            float dt = (float)gt.ElapsedGameTime.TotalSeconds;
            var kb = Keyboard.GetState();
            var ms = Mouse.GetState();
            if (kb.IsKeyDown(Keys.Escape)) Exit();

            // zoom
            if (Edge(kb, Keys.OemPlus) || Edge(kb, Keys.Add))
                _zoom = MathHelper.Clamp(_zoom * 2f, 0.5f, 8f);
            if (Edge(kb, Keys.OemMinus) || Edge(kb, Keys.Subtract))
                _zoom = MathHelper.Clamp(_zoom * 0.5f, 0.5f, 8f);

            // camera pan
            float camSp = 400f / _zoom;
            if (kb.IsKeyDown(Keys.W)) _camY -= camSp * dt;
            if (kb.IsKeyDown(Keys.S)) _camY += camSp * dt;
            if (kb.IsKeyDown(Keys.A)) _camX -= camSp * dt;
            if (kb.IsKeyDown(Keys.D)) _camX += camSp * dt;

            if (Edge(kb, Keys.F11))
            {
                if (_gfx.IsFullScreen)
                {
                    _gfx.IsFullScreen = false;
                    _gfx.PreferredBackBufferWidth = 1280;
                    _gfx.PreferredBackBufferHeight = 720;
                }
                else
                {
                    var dm = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
                    _gfx.PreferredBackBufferWidth = dm.Width;
                    _gfx.PreferredBackBufferHeight = dm.Height;
                    _gfx.IsFullScreen = true;
                }
                _gfx.ApplyChanges();

                var midX = (Constants.ARENA_LEFT + Constants.ARENA_RIGHT) * 0.5f;
                var midY = (Constants.ARENA_TOP + Constants.ARENA_BOTTOM) * 0.5f;
                _camX = midX - (GraphicsDevice.Viewport.Width * 0.5f) / _zoom;
                _camY = midY - (GraphicsDevice.Viewport.Height * 0.5f) / _zoom;
            }

            if (Edge(kb, Keys.P)) SpawnEnemy();

            // ability switching
            if (Edge(kb, Keys.D1) || Edge(kb, Keys.NumPad1))
                _currentAbility = Ability.None;
            if (Edge(kb, Keys.D2) || Edge(kb, Keys.NumPad2))
                _currentAbility = Ability.Fire;
            if (Edge(kb, Keys.D3) || Edge(kb, Keys.NumPad3))
                _currentAbility = Ability.Telekinesis;

            var mscr = new Point(ms.X, ms.Y);
            var mworld = new Vector2(_camX + mscr.X / _zoom,
                                     _camY + mscr.Y / _zoom);
            var prevWorld = new Vector2(_camX + _prevMs.X / _zoom,
                                        _camY + _prevMs.Y / _zoom);

            if (_currentAbility == Ability.Fire)
            {
                bool mPress = ms.LeftButton == ButtonState.Pressed &&
                              _prevMs.LeftButton == ButtonState.Released;
                if (mPress)
                {
                    float minD = Constants.PICKUP_RADIUS;
                    for (int i = _enemies.Count - 1; i >= 0; i--)
                    {
                        var e = _enemies[i];
                        for (int p = -2; p <= 2; p++)
                        {
                            float d = Vector2.Distance(e.GetPartPos(p), mworld);
                            if (d < minD)
                            {
                                e.IsBurning = true;
                                e.BurnTimer = Constants.BURN_DURATION;
                                _enemies[i] = e;
                                i = -1; // break outer
                                break;
                            }
                        }
                    }
                }
            }
            else if (_currentAbility == Ability.Telekinesis)
            {
                InputSystem.HandleDrag(
                    gt, ms, _prevMs,
                    ref _dragging, ref _dragIdx, ref _dragPart,
                    ref _dragStartWorld, ref _dragStartTime,
                    mworld, prevWorld,
                    _enemies, _pixels
                );
            }
            else
            {
                _dragging = false;
            }

            PhysicsSystem.SimulateAll(_enemies, _pixels, dt);
            PhysicsSystem.UpdatePixels(_pixels, dt);

            _prevKb = kb;
            _prevMs = ms;
            base.Update(gt);
        }

        protected override void Draw(GameTime gt)
        {
            GraphicsDevice.Clear(Color.DimGray);
            var cam = Matrix.CreateScale(_zoom, _zoom, 1f)
                      * Matrix.CreateTranslation(-_camX * _zoom, -_camY * _zoom, 0);
            _sb.Begin(transformMatrix: cam, samplerState: SamplerState.PointClamp);

            // arena border
            int t = 2;
            _sb.Draw(_px,
                     new Rectangle(Constants.ARENA_LEFT,
                                   Constants.ARENA_TOP,
                                   Constants.ARENA_RIGHT - Constants.ARENA_LEFT,
                                   t),
                     Color.Black);
            _sb.Draw(_px,
                     new Rectangle(Constants.ARENA_LEFT,
                                   Constants.ARENA_BOTTOM,
                                   Constants.ARENA_RIGHT - Constants.ARENA_LEFT,
                                   t),
                     Color.Black);
            _sb.Draw(_px,
                     new Rectangle(Constants.ARENA_LEFT,
                                   Constants.ARENA_TOP,
                                   t,
                                   Constants.ARENA_BOTTOM - Constants.ARENA_TOP),
                     Color.Black);
            _sb.Draw(_px,
                     new Rectangle(Constants.ARENA_RIGHT - t,
                                   Constants.ARENA_TOP,
                                   t,
                                   Constants.ARENA_BOTTOM - Constants.ARENA_TOP),
                     Color.Black);

            // debris
            foreach (var p in _pixels)
                _sb.Draw(_px, p.Bounds, p.Col);

            // enemies (with resized shadow)
            foreach (var e in _enemies)
            {
                // --- dynamic shadow ---
                // total stick length in pixels:
                float stickLen = Constants.ENEMY_H * Constants.PART_LEN;
                // project that onto X by sin(angle):
                int shLen = (int)MathF.Round(
                    MathF.Abs(MathF.Sin(e.Angle)) * stickLen
                ) + Constants.ENEMY_W;
                int shThick = 2;
                var shRect = new Rectangle(
                    (int)MathF.Round(e.Pos.X - shLen / 2f),
                    (int)MathF.Round(e.Pos.Y + shThick),
                    shLen, shThick
                );
                _sb.Draw(_px, shRect, new Color(0, 0, 0, 100));

                // --- draw each 2×1 segment rotated around center ---
                int half = Constants.ENEMY_H / 2;
                for (int part = -half; part < half; part++)
                {
                    var pt = e.GetPartPos(part);
                    pt.Y -= e.z;

                    // map part → [0..ENEMY_H-1]
                    int seg = part + half;

                    // bucket segments:
                    Color c;
                    if (seg <= 1)
                        c = new Color(255, 219, 172);          // head
                    else if (seg <= Constants.ENEMY_H * 2 / 5)
                        c = e.ShirtColor;                      // upper body
                    else if (seg <= Constants.ENEMY_H * 3 / 5)
                        c = e.ShirtColor;                      // waist
                    else if (seg <= Constants.ENEMY_H * 4 / 5)
                        c = new Color(32, 32, 128);            // legs
                    else
                        c = new Color(16, 16, 64);             // feet

                    int w = Constants.ENEMY_W;
                    int h = (int)Constants.PART_LEN;
                    var dest = new Rectangle(
                        (int)(pt.X - w / 2f),
                        (int)(pt.Y - h / 2f),
                        w, h
                    );

                    _sb.Draw(
                        _px, dest, null, c,
                        e.Angle,
                        new Vector2(w / 2f, h / 2f),
                        SpriteEffects.None, 0f
                    );
                }

                if (e.IsBurning)
                {
                    var head = e.GetPartPos(-2);
                    head.Y -= e.z + 1;
                    var flame = new Rectangle((int)head.X - 1, (int)head.Y - 2, 2, 2);
                    _sb.Draw(_px, flame, Color.OrangeRed);
                }
            }

            _sb.End();
            base.Draw(gt);
        }

        private void SpawnEnemy()
        {
            var x = _rng.NextFloat(Constants.ARENA_LEFT + 10,
                                   Constants.ARENA_RIGHT - 10);
            var y = _rng.NextFloat(Constants.ARENA_TOP + 10,
                                   Constants.ARENA_BOTTOM - 10);
            _enemies.Add(new Enemy(new Vector2(x, y),
                                   RandomShirtColor()));
        }

        private Color RandomShirtColor()
        {
            var pal = new[]
            {
                Color.Blue, Color.Green, Color.Red,
                Color.Yellow, Color.Purple, Color.Orange, Color.Cyan
            };
            return pal[_rng.Next(pal.Length)];
        }

        private bool Edge(KeyboardState kb, Keys k)
            => kb.IsKeyDown(k) && _prevKb.IsKeyUp(k);
    }
}
