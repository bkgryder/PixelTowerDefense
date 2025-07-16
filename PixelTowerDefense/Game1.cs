using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PixelTowerDefense.Entities;
using PixelTowerDefense.Systems;
using PixelTowerDefense.Helpers;

namespace PixelTowerDefense
{
    /// <summary>
    /// Main MonoGame <see cref="Game"/> implementation hosting the game loop and systems.
    /// </summary>
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _gfx;
        private SpriteBatch _sb;
        private readonly List<Enemy> _enemies = new();
        private readonly List<Pixel> _pixels = new();
        private readonly Random _rng = new();

        private InputSystem _input;
        private AISystem _ai;
        private PhysicsSystem _physics;
        private RenderSystem _render;

        public Game1()
        {
            _gfx = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _gfx.PreferredBackBufferWidth = GameConstants.WindowWidth;
            _gfx.PreferredBackBufferHeight = GameConstants.WindowHeight;
            _gfx.ApplyChanges();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _sb = new SpriteBatch(GraphicsDevice);
            float mid = (GameConstants.ArenaLeft + GameConstants.ArenaRight) * 0.5f;
            _input = new InputSystem(mid - (_gfx.PreferredBackBufferWidth * 0.5f), 0, GameConstants.DefaultZoom);
            _ai = new AISystem();
            _physics = new PhysicsSystem();
            _render = new RenderSystem(GraphicsDevice);

            for (int i = 0; i < GameConstants.InitialEnemyCount; i++) SpawnEnemy();
        }

        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            _input.Update(dt, _enemies, SpawnEnemy);
            _ai.Update(dt, _enemies, _rng);
            _physics.UpdateEnemies(dt, _enemies, _pixels);
            _physics.UpdatePixels(dt, _pixels);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _render.Draw(_sb, _enemies, _pixels, _input.Zoom, _input.CamX, _input.CamY);
            base.Draw(gameTime);
        }

        protected override void EndRun()
        {
            _render.Dispose();
            base.EndRun();
        }

        private void SpawnEnemy()
        {
            float x = _rng.NextFloat(GameConstants.ArenaLeft + GameConstants.SpawnEdgeOffset,
                                   GameConstants.ArenaRight - GameConstants.SpawnEdgeOffset);
            Color shirt = RandomShirtColor(_rng);
            var e = new Enemy(new Vector2(x, GameConstants.FloorY - 1f), shirt);
            _enemies.Add(e);
        }

        private static Color RandomShirtColor(Random rng)
        {
            Color[] palette = { Color.Blue, Color.Green, Color.Red, Color.Yellow, Color.Purple, Color.Orange, Color.Cyan };
            return palette[rng.Next(palette.Length)];
        }
    }
}
