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

        // drag state
        bool _dragging;
        int _dragIdx;
        int _dragPart;
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

            for (int i = 0; i < 10; i++) SpawnEnemy();

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

            if (Edge(kb, Keys.P)) SpawnEnemy();

            var mscr = new Point(ms.X, ms.Y);
            var mworld = new Vector2(
                _camX + mscr.X / _zoom,
                _camY + mscr.Y / _zoom
            );
            var prevWorld = new Vector2(
                _camX + _prevMs.X / _zoom,
                _camY + _prevMs.Y / _zoom
            );

            InputSystem.HandleDrag(
                gt, ms, _prevMs,
                ref _dragging, ref _dragIdx, ref _dragPart,
                ref _dragStartWorld, ref _dragStartTime,
                mworld, prevWorld,
                _enemies, _pixels
            );

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
            _sb.Begin(transformMatrix: cam);

            // arena border
            int t = 2;
            _sb.Draw(_px, new Rectangle(Constants.ARENA_LEFT, Constants.ARENA_TOP, Constants.ARENA_RIGHT - Constants.ARENA_LEFT, t), Color.Black);
            _sb.Draw(_px, new Rectangle(Constants.ARENA_LEFT, Constants.ARENA_BOTTOM, Constants.ARENA_RIGHT - Constants.ARENA_LEFT, t), Color.Black);
            _sb.Draw(_px, new Rectangle(Constants.ARENA_LEFT, Constants.ARENA_TOP, t, Constants.ARENA_BOTTOM - Constants.ARENA_TOP), Color.Black);
            _sb.Draw(_px, new Rectangle(Constants.ARENA_RIGHT - t, Constants.ARENA_TOP, t, Constants.ARENA_BOTTOM - Constants.ARENA_TOP), Color.Black);

            // debris
            foreach (var p in _pixels)
                _sb.Draw(_px, p.Bounds, p.Col);

            // enemies
            foreach (var e in _enemies)
            {
                // shadow length grows with |sin(angle)| --
                // a leaning enemy casts a wider shadow
                int shLen = (int)MathF.Round(1 + 4 * MathF.Abs(MathF.Sin(e.Angle)));
                var sh = new Rectangle((int)e.Pos.X + 1, (int)e.Pos.Y + 2, shLen, 1);
                _sb.Draw(_px, sh, new Color(0, 0, 0, 100));

                for (int part = -2; part <= 2; part++)
                {
                    var pt = e.GetPartPos(part);
                    pt.Y -= e.z;
                    Color c = part switch
                    {
                        -2 => new Color(255, 219, 172),
                        -1 or 0 => e.ShirtColor,
                        1 => new Color(32, 32, 128),
                        2 => new Color(16, 16, 64),
                        _ => Color.White
                    };
                    _sb.Draw(_px, new Rectangle((int)pt.X, (int)pt.Y, 1, 1), c);
                }
            }

            _sb.End();
            base.Draw(gt);
        }

        private void SpawnEnemy()
        {
            var x = _rng.NextFloat(Constants.ARENA_LEFT + 10, Constants.ARENA_RIGHT - 10);
            var y = _rng.NextFloat(Constants.ARENA_TOP + 10, Constants.ARENA_BOTTOM - 10);
            _enemies.Add(new Enemy(new Vector2(x, y), RandomShirtColor()));
        }

        private Color RandomShirtColor()
        {
            var pal = new[] { Color.Blue, Color.Green, Color.Red, Color.Yellow, Color.Purple, Color.Orange, Color.Cyan };
            return pal[_rng.Next(pal.Length)];
        }

        private bool Edge(KeyboardState kb, Keys k)
            => kb.IsKeyDown(k) && _prevKb.IsKeyUp(k);
    }
}
