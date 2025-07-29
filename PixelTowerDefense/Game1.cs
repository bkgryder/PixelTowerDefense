using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PixelTowerDefense.Entities;
using PixelTowerDefense.Systems;
using PixelTowerDefense.Utils;
using PixelTowerDefense.World;

namespace PixelTowerDefense
{
    public class Game1 : Game
    {
        GraphicsDeviceManager _gfx;
        SpriteBatch _sb;
        Texture2D _px;
        SpriteFont _font;

        List<Meeple> _meeples = new();
        List<Rabbit> _rabbits = new();
        List<RabbitHole> _rabbitHomes = new();
        List<Wolf> _wolves = new();
        List<WolfDen> _wolfDens = new();
        List<Pixel> _pixels = new(Constants.MAX_DEBRIS);
        List<Seed> _seeds = new();
        List<BerryBush> _bushes = new();
        List<Wood> _wood = new();
        List<Stone> _stones = new();
        List<Tree> _trees = new();
        List<Building> _buildings = new();
        Random _rng = new();
        World.GameWorld _world = new();
        GroundMap _ground;
        WaterMap _water = new WaterMap(Constants.CHUNK_PIXEL_SIZE, Constants.CHUNK_PIXEL_SIZE);
        Weather _weather = Weather.Clear;
        List<RainDrop> _rain = new(Constants.MAX_RAIN_DROPS);

        float _camX, _camY, _zoom = 3.5f;
        KeyboardState _prevKb;
        MouseState _prevMs;

        Ability _currentAbility = Ability.None;

        // ability toolbar
        Rectangle _abilityButtonRect;
        Rectangle[] _abilityOptionRects;
        readonly Ability[] _abilityOptions =
            { Ability.None, Ability.Fire, Ability.Telekinesis, Ability.Explosion, Ability.Precipitate };
        bool _abilityMenuOpen;

        // drag state
        bool _dragging;
        int _dragIdx, _dragPart;
        Vector2 _dragStartWorld;
        float _dragStartTime;

        bool _rabbitDragging;
        int _rabbitDragIdx;
        Vector2 _rabbitDragStartWorld;
        float _rabbitDragStartTime;

        bool _wolfDragging;
        int _wolfDragIdx;
        Vector2 _wolfDragStartWorld;
        float _wolfDragStartTime;

        // hovered meeple index
        int _hoverIdx;

        // precipitate state
        float _rainAlpha;
        bool _raining;
        List<Pixel> _cloudPixels = new();
        Vector2 _cloudCenter;

        float _mana = Constants.MANA_MAX;

        List<Light> _lights = new();
        float _timeOfDay;

        bool _debugOverlay;

        float _fpsTimer;
        int _fpsFrames;
        int _fps;

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
            _abilityButtonRect = new Rectangle(5, 5, 24, 24);
            _abilityOptionRects = new Rectangle[_abilityOptions.Length];
            for (int i = 0; i < _abilityOptionRects.Length; i++)
                _abilityOptionRects[i] =
                    new Rectangle(5, 31 + i * 26, 24, 24);
            _rainAlpha = 0f;
            _raining = false;
            _cloudPixels.Clear();
            _cloudCenter = Vector2.Zero;
            _hoverIdx = -1;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _sb = new SpriteBatch(GraphicsDevice);
            _px = new Texture2D(GraphicsDevice, 1, 1);
            _px.SetData(new[] { Color.White });
            _font = Content.Load<SpriteFont>("PixelFont");

            int arenaW = Constants.ARENA_RIGHT - Constants.ARENA_LEFT;
            int arenaH = Constants.ARENA_BOTTOM - Constants.ARENA_TOP;
            _ground = GroundGenerator.Generate(arenaW, arenaH);
            _water = WaterGenerator.Generate(
                _ground,
                Constants.RIVER_COUNT,
                Constants.LAKE_COUNT,
                _rng);

            SpawnMeeple(0);
            SpawnRabbits(8);
            SpawnWolves(2);
            SpawnBerryBushes(5);
            //SpawnWood(4);
            //SpawnStones(4);

            TreeArchetype PickArch(Biome biome)
            {
                // map your ground biomes to archetypes
                return biome switch
                {
                    Biome.Forest => _rng.NextDouble() < 0.3 ? TreeLibrary.Pine : TreeLibrary.Oak,
                    Biome.Snow => TreeLibrary.Pine,
                    Biome.Meadow => _rng.NextDouble() < 0.7 ? TreeLibrary.Oak : TreeLibrary.Birch,
                    //Biome.Marsh => TreeLibrary.Willow,      // if you add one
                    Biome.Grass => TreeLibrary.Birch,
                    _ => TreeLibrary.Oak
                };
            }

            // spawn clustered tree patches
            for (int i = 0; i < 6; i++)
            {
                float cx = _rng.NextFloat(Constants.ARENA_LEFT + 10, Constants.ARENA_RIGHT - 10);
                float cy = _rng.NextFloat(Constants.ARENA_TOP + 10, Constants.ARENA_BOTTOM - 10);
                int count = _rng.Next(1, 15);
                for (int j = 0; j < count; j++)
                {
                    float ox = _rng.NextFloat(-65f, 65f);
                    float oy = _rng.NextFloat(-65f, 65f);
                    var p = new Vector2(cx + ox, cy + oy);

                    // sample biome cell:
                    int gx = Math.Clamp((int)((p.X - Constants.ARENA_LEFT) / Constants.CELL_PIXELS), 0, _ground.W - 1);
                    int gy = Math.Clamp((int)((p.Y - Constants.ARENA_TOP) / Constants.CELL_PIXELS), 0, _ground.H - 1);
                    var biome = _ground.Cells[gx, gy].Biome;

                    var arch = PickArch(biome);
                    _trees.Add(new Tree(p, _rng, arch, worldSeed: 12345));
                }
            }

            var midX = (Constants.ARENA_LEFT + Constants.ARENA_RIGHT) * 0.5f;
            var midY = (Constants.ARENA_TOP + Constants.ARENA_BOTTOM) * 0.5f;
            _buildings.Add(new Building
            {
                Pos = new Vector2(midX, midY),
                Kind = BuildingType.StorageHut,
                StoredBerries = 0,
                StoredWood = 0,
                HousedMeeples = 0,
                ReservedBy = null
            });
            _buildings.Add(new Building
            {
                Pos = new Vector2(midX + 6, midY),
                Kind = BuildingType.HousingHut,
                StoredBerries = 0,
                StoredWood = 0,
                HousedMeeples = 0,
                ReservedBy = null
            });
            _camX = midX - (GraphicsDevice.Viewport.Width * 0.5f) / _zoom;
            _camY = midY - (GraphicsDevice.Viewport.Height * 0.5f) / _zoom;
        }

        protected override void Update(GameTime gt)
        {
            float dt = (float)gt.ElapsedGameTime.TotalSeconds;
            _fpsTimer += dt;
            if (_fpsTimer >= 1f)
            {
                _fps = _fpsFrames;
                _fpsFrames = 0;
                _fpsTimer -= 1f;
            }
            var kb = Keyboard.GetState();
            var ms = Mouse.GetState();
            _mana = MathF.Min(Constants.MANA_MAX, _mana + Constants.MANA_REGEN * dt);
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

            if (Edge(kb, Keys.F12))
                _debugOverlay = !_debugOverlay;

            if (Edge(kb, Keys.F6))
                _weather = _weather == Weather.Clear ? Weather.Rainy : Weather.Clear;



            // ability switching (keyboard)
            if (Edge(kb, Keys.D1) || Edge(kb, Keys.NumPad1))
                _currentAbility = Ability.None;
            if (Edge(kb, Keys.D2) || Edge(kb, Keys.NumPad2))
                _currentAbility = Ability.Fire;
            if (Edge(kb, Keys.D3) || Edge(kb, Keys.NumPad3))
                _currentAbility = Ability.Telekinesis;
            if (Edge(kb, Keys.D4) || Edge(kb, Keys.NumPad4))
                _currentAbility = Ability.Explosion;
            if (Edge(kb, Keys.D5) || Edge(kb, Keys.NumPad5))
                _currentAbility = Ability.Precipitate;

            // toolbar interaction
            var mousePoint = new Point(ms.X, ms.Y);
            bool mClick = ms.LeftButton == ButtonState.Pressed &&
                          _prevMs.LeftButton == ButtonState.Released;
            if (mClick && _abilityButtonRect.Contains(mousePoint))
            {
                _abilityMenuOpen = !_abilityMenuOpen;
            }
            else if (_abilityMenuOpen && mClick)
            {
                bool inside = false;
                for (int i = 0; i < _abilityOptionRects.Length; i++)
                {
                    if (_abilityOptionRects[i].Contains(mousePoint))
                    {
                        _currentAbility = _abilityOptions[i];
                        inside = true;
                        break;
                    }
                }
                if (!inside && !_abilityButtonRect.Contains(mousePoint))
                    _abilityMenuOpen = false;
                else
                    _abilityMenuOpen = inside ? false : _abilityMenuOpen;
            }

            var mscr = new Point(ms.X, ms.Y);
            var mworld = new Vector2(_camX + mscr.X / _zoom,
                                     _camY + mscr.Y / _zoom);
            var prevWorld = new Vector2(_camX + _prevMs.X / _zoom,
                                        _camY + _prevMs.Y / _zoom);

            // determine hovered meeple
            _hoverIdx = -1;
            float hoverDist = Constants.PICKUP_RADIUS;
            for (int i = _meeples.Count - 1; i >= 0; i--)
            {
                var e = _meeples[i];
                for (int p = -2; p <= 2; p++)
                {
                    float d = Vector2.Distance(e.GetPartPos(p), mworld);
                    if (d < hoverDist)
                    {
                        hoverDist = d;
                        _hoverIdx = i;
                    }
                }
            }

            if (_currentAbility == Ability.Fire)
            {
                bool mPress = ms.LeftButton == ButtonState.Pressed &&
                              _prevMs.LeftButton == ButtonState.Released;
                if (mPress && _mana >= Constants.FIRE_COST)
                {
                    bool affected = false;
                    float minD = Constants.PICKUP_RADIUS;
                    for (int i = _meeples.Count - 1; i >= 0 && !affected; i--)
                    {
                        var e = _meeples[i];
                        for (int p = -2; p <= 2; p++)
                        {
                            float d = Vector2.Distance(e.GetPartPos(p), mworld);
                            if (d < minD)
                            {
                                e.IsBurning = true;
                                e.BurnTimer = Constants.BURN_DURATION;
                                _meeples[i] = e;
                                affected = true;
                                break;
                            }
                        }
                    }

                    if (!affected)
                    {
                        for (int i = _trees.Count - 1; i >= 0 && !affected; i--)
                        {
                            var t = _trees[i];
                            int bx = (int)MathF.Round(t.Pos.X);
                            int by = (int)MathF.Round(t.Pos.Y);
                            foreach (var p in t.TrunkPixels)
                            {
                                var pos = new Vector2(bx + p.X, by + p.Y);
                                if (Vector2.Distance(pos, mworld) < minD)
                                {
                                    t.IsBurning = true;
                                    t.BurnTimer = Constants.TREE_BURN_DURATION;
                                    _trees[i] = t;
                                    affected = true;
                                    break;
                                }
                            }
                            if (!affected)
                            {
                                foreach (var p in t.LeafPixels)
                                {
                                    var pos = new Vector2(bx + p.X, by + p.Y);
                                    if (Vector2.Distance(pos, mworld) < minD)
                                    {
                                        t.IsBurning = true;
                                        t.BurnTimer = Constants.TREE_BURN_DURATION;
                                        _trees[i] = t;
                                        affected = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (!affected)
                    {
                        for (int i = _bushes.Count - 1; i >= 0 && !affected; i--)
                        {
                            var b = _bushes[i];
                            int bx = (int)MathF.Round(b.Pos.X);
                            int by = (int)MathF.Round(b.Pos.Y);
                            foreach (var p in b.TrunkPixels)
                            {
                                var pos = new Vector2(bx + p.X, by + p.Y);
                                if (Vector2.Distance(pos, mworld) < minD)
                                {
                                    b.IsBurning = true;
                                    b.BurnTimer = Constants.BUSH_BURN_DURATION;
                                    _bushes[i] = b;
                                    affected = true;
                                    break;
                                }
                            }
                            if (!affected)
                            {
                                foreach (var p in b.LeafPixels)
                                {
                                    var pos = new Vector2(bx + p.X, by + p.Y);
                                    if (Vector2.Distance(pos, mworld) < minD)
                                    {
                                        b.IsBurning = true;
                                        b.BurnTimer = Constants.BUSH_BURN_DURATION;
                                        _bushes[i] = b;
                                        affected = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    _mana = MathF.Max(0f, _mana - Constants.FIRE_COST);
                }
            }
            else if (_currentAbility == Ability.Telekinesis)
            {
                if (_mana > 0f)
                {
                    InputSystem.HandleDrag(
                        gt, ms, _prevMs,
                        ref _dragging, ref _dragIdx, ref _dragPart,
                        ref _dragStartWorld, ref _dragStartTime,
                        mworld, prevWorld,
                        _meeples, _pixels
                    );
                    InputSystem.HandleRabbitDrag(
                        gt, ms, _prevMs,
                        ref _rabbitDragging, ref _rabbitDragIdx,
                        ref _rabbitDragStartWorld, ref _rabbitDragStartTime,
                        mworld,
                        _rabbits
                    );
                    InputSystem.HandleWolfDrag(
                        gt, ms, _prevMs,
                        ref _wolfDragging, ref _wolfDragIdx,
                        ref _wolfDragStartWorld, ref _wolfDragStartTime,
                        mworld,
                        _wolves
                    );
                    if (_dragging)
                        _mana = MathF.Max(0f, _mana - Constants.TELEKINESIS_DRAIN * dt);
                    if (_rabbitDragging && !_dragging)
                        _mana = MathF.Max(0f, _mana - Constants.TELEKINESIS_DRAIN * dt);
                    if (_wolfDragging && !_dragging && !_rabbitDragging)
                        _mana = MathF.Max(0f, _mana - Constants.TELEKINESIS_DRAIN * dt);
                }
                else
                {
                    _dragging = false;
                    _rabbitDragging = false;
                    _wolfDragging = false;
                }
            }
            else if (_currentAbility == Ability.Explosion)
            {
                bool mPress = ms.LeftButton == ButtonState.Pressed &&
                              _prevMs.LeftButton == ButtonState.Released;
                if (mPress && _mana >= Constants.EXPLOSION_COST)
                {
                    TriggerExplosion(mworld);
                    _mana = MathF.Max(0f, _mana - Constants.EXPLOSION_COST);
                }
            }
            else if (_currentAbility == Ability.Precipitate)
            {
                bool held = ms.LeftButton == ButtonState.Pressed;
                bool released = ms.LeftButton == ButtonState.Released &&
                                 _prevMs.LeftButton == ButtonState.Pressed;
                bool prev = _raining;
                if (held && _mana > 0f)
                    _raining = true;
                if (released || _mana <= 0f)
                    _raining = false;

                if (_raining)
                {
                    var target = mworld + new Vector2(0f, Constants.PRECIPITATE_CLOUD_OFFSET_Y);
                    float lerp = MathHelper.Clamp(Constants.PRECIPITATE_CLOUD_LERP * dt, 0f, 1f);
                    _cloudCenter = Vector2.Lerp(_cloudCenter, target, lerp);
                }

                float fade = Constants.PRECIPITATE_FADE_SPEED * dt;
                if (_raining)
                    _rainAlpha = MathF.Min(1f, _rainAlpha + fade);
                else
                    _rainAlpha = MathF.Max(0f, _rainAlpha - fade);

                if (_raining && !prev)
                    InitCloud();

                UpdateCloud(dt);

                if (_raining)
                {
                    _mana = MathF.Max(0f, _mana - Constants.PRECIPITATE_DRAIN * dt);
                    for (int i = 0; i < _cloudPixels.Count; i++)
                    {
                        var cp = _cloudPixels[i];
                        cp.Lifetime -= dt;
                        if (cp.Lifetime <= 0f)
                        {
                            var pos = _cloudCenter + cp.Pos;
                            var vel = new Vector2(0f, Constants.PRECIPITATE_DROP_SPEED);
                            _pixels.Spawn(new Pixel(pos, vel, Color.CornflowerBlue, 0f, 1f));
                            cp.Lifetime = _rng.NextFloat(0.05f, 0.15f);
                        }
                        _cloudPixels[i] = cp;
                    }

                }
            }
            else
            {
                _dragging = false;
            }
            
            PhysicsSystem.SimulateAll(_meeples, _pixels, _bushes, _buildings, _trees, _wood, _water, dt);
            PhysicsSystem.SimulateRabbits(_rabbits, _bushes, _seeds, _rabbitHomes, dt);
            PhysicsSystem.SimulateWolves(_wolves, _rabbits, _meeples, _wolfDens, dt);
            if (_weather == Weather.Rainy)
                UpdateRain(dt);
            PhysicsSystem.UpdatePixels(_pixels, dt);
            PhysicsSystem.UpdateWood(_wood, dt);
            PhysicsSystem.UpdateSeeds(_seeds, _trees, _bushes, dt);
            PhysicsSystem.UpdateBushes(_bushes, _seeds, _pixels, dt, _weather == Weather.Rainy);
            PhysicsSystem.UpdateTrees(_trees, _seeds, _pixels, dt);
            CombatSystem.ResolveCombat(_meeples, _pixels, dt);

            // update shadow positions after all movement/physics
            ShadowSystem.UpdateShadows(_meeples, dt);

            LightingSystem.Update(_lights, dt);
            _timeOfDay = (_timeOfDay + dt / Constants.DAY_LENGTH) % 1f;
            foreach (var m in _meeples)
                if (m.IsBurning)
                    LightingSystem.AddLight(
                        _lights,
                        m.GetPartPos(0) - new Vector2(0f, m.z),
                        Constants.FIRE_LIGHT_RADIUS,
                        Constants.FIRE_LIGHT_INTENSITY,
                        dt);

            _prevKb = kb;
            _prevMs = ms;
            base.Update(gt);
        }

        protected override void Draw(GameTime gt)
        {
            _fpsFrames++;
            GraphicsDevice.Clear(Color.DarkSeaGreen);
            var cam = Matrix.CreateScale(_zoom, _zoom, 1f)
                      * Matrix.CreateTranslation(-_camX * _zoom, -_camY * _zoom, 0);
            _sb.Begin(transformMatrix: cam, samplerState: SamplerState.PointClamp);

            var visible = new Rectangle(
                (int)MathF.Floor(_camX),
                (int)MathF.Floor(_camY),
                (int)MathF.Ceiling(GraphicsDevice.Viewport.Width / _zoom),
                (int)MathF.Ceiling(GraphicsDevice.Viewport.Height / _zoom));
            DrawGround(_sb, _ground, _px, _zoom, visible);
            DrawWater(_sb, _water, _px, _zoom, (float)gt.TotalGameTime.TotalSeconds);

            // --- arena border ---
            // Removed dark border bars

            // --- debris ---
            foreach (var p in _pixels)
                _sb.Draw(_px, p.Bounds, p.Col);

            foreach (var s in _seeds)
            {
                var rect = new Rectangle((int)MathF.Round(s.Pos.X), (int)MathF.Round(s.Pos.Y - s.z), 1, 1);
                _sb.Draw(_px, rect, Color.SandyBrown);
            }

            foreach (var r in _rain)
            {
                var rect = new Rectangle((int)MathF.Round(r.Pos.X), (int)MathF.Round(r.Pos.Y - r.z), 1, 1);
                _sb.Draw(_px, rect, Color.CornflowerBlue);
            }

            if (_rainAlpha > 0f)
                DrawCloudShadow();

            foreach (var cp in _cloudPixels)
            {
                var pos = _cloudCenter + cp.Pos;
                var rect = new Rectangle((int)MathF.Round(pos.X), (int)MathF.Round(pos.Y), 1, 1);
                var col = new Color(cp.Col.R, cp.Col.G, cp.Col.B, (byte)(_rainAlpha * 255));
                _sb.Draw(_px, rect, col);
            }

            // --- bush shadows ---
            foreach (var b in _bushes)
                DrawBushShadow(b);

            // --- berry bushes ---
            foreach (var b in _bushes)
                DrawBushBottom(b);

            // --- stones ---
            foreach (var s in _stones)
                DrawStone(s);

            // --- wood ---
            foreach (var w in _wood)
                DrawWood(w);

            // --- rabbit homes ---
            foreach (var h in _rabbitHomes)
                DrawRabbitHome(h);

            // --- wolf dens ---
            foreach (var d in _wolfDens)
                DrawWolfDen(d);

            // --- rabbits ---
            foreach (var r in _rabbits)
                DrawRabbit(r);

            // --- wolves ---
            foreach (var w in _wolves)
                DrawWolf(w);

            // --- tree shadows ---
            foreach (var t in _trees)
                DrawTreeShadow(t);

            // --- tree trunks ---
            foreach (var t in _trees)
                DrawTreeBottom(t);

            // --- buildings ---
            foreach (var b in _buildings)
                DrawBuilding(b);

            // --- shadows ---
            foreach (var e in _meeples.OrderBy(s => s.ShadowY))
            {
                DrawShadow(e);
            }

            // --- soldiers/entities on ground ---
            foreach (var e in _meeples.Where(m => m.z <= 0f).OrderBy(m => m.ShadowY))
                DrawMeepleSprite(e);

            // --- tree leaves ---
            foreach (var t in _trees)
                DrawTreeTop(t);
            // --- bush leaves ---
            foreach (var b in _bushes)
                DrawBushTop(b);

            // --- tree flames ---
            foreach (var t in _trees)
                if (t.IsBurning)
                    DrawTreeFlame(t);

            // --- bush flames ---
            foreach (var b in _bushes)
                if (b.IsBurning)
                    DrawBushFlame(b);

            // --- airborne soldiers/entities ---
            foreach (var e in _meeples.Where(m => m.z > 0f).OrderBy(m => m.ShadowY))
                DrawMeepleSprite(e);

            if (_debugOverlay)
                DrawChunkBorders();

            _sb.End();

            float phase = MathF.Sin(_timeOfDay * MathHelper.TwoPi) * 0.5f + 0.5f;
            float ambient = MathHelper.Lerp(Constants.NIGHT_BRIGHTNESS, 1f, phase);
            if (_weather == Weather.Rainy)
                ambient *= Constants.RAIN_AMBIENT_MULT;
            byte dark = (byte)Math.Clamp(255f * (1f - ambient), 0f, 255f);
            _sb.Begin();
            _sb.Draw(_px, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), new Color((byte)0, (byte)0, (byte)0, dark));
            _sb.End();

            LightingSystem.DrawLights(_sb, _px, _lights, cam);

            // --- UI ---
            _sb.Begin();
            if (_rainAlpha > 0f)
            {
                var col = new Color((byte)40, (byte)40, (byte)40, (byte)(_rainAlpha * 255));
                _sb.Draw(_px, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, 20), col);
            }
            var uiMouse = Mouse.GetState();
            if (_hoverIdx >= 0 && _hoverIdx < _meeples.Count)
                DrawMeepleStats(_meeples[_hoverIdx], new Point(uiMouse.X, uiMouse.Y));
            DrawAbilityToolbar();
            DrawHint();
            DrawManaRing(new Point(uiMouse.X, uiMouse.Y));
            if (_debugOverlay)
                DrawDebugOverlay();
            _sb.End();
            base.Draw(gt);
        }


        private void SpawnWave(Faction side, int count, bool withCombat = true)
        {
            float x0 = side == Faction.Friendly ? Constants.ARENA_LEFT + 2 : Constants.ARENA_RIGHT - 30;
            float x1 = side == Faction.Friendly ? Constants.ARENA_LEFT + 30 : Constants.ARENA_RIGHT - 2;
            for (int i = 0; i < count; i++)
            {
                var x = _rng.NextFloat(x0, x1);
                var y = _rng.NextFloat(Constants.ARENA_TOP + 2, Constants.ARENA_BOTTOM - 2);
                var pal = side == Faction.Friendly ? Meeple.FRIENDLY_SHIRTS : Meeple.ENEMY_SHIRTS;
                var shirt = pal[_rng.Next(pal.Length)];
                _meeples.Add(new Meeple(new Vector2(x, y), side, shirt){Combatant = withCombat ? new Combatant() : (Combatant?)null});
            }
        }

        private void SpawnMeeple(int count)
        {
            for (int i = 0; i < count; i++)
            {
                float x = _rng.NextFloat(Constants.ARENA_LEFT + 5,
                                       Constants.ARENA_RIGHT - 5);
                float y = _rng.NextFloat(Constants.ARENA_TOP + 5,
                                       Constants.ARENA_BOTTOM - 5);
                var shirt = Meeple.FRIENDLY_SHIRTS[_rng.Next(Meeple.FRIENDLY_SHIRTS.Length)];
                var m = Meeple.SpawnMeeple(new Vector2(x, y), Faction.Friendly, shirt, _rng);
                m.Worker = new Worker();
                _meeples.Add(m);
            }
        }

        private void SpawnRabbits(int count)
        {
            for (int i = 0; i < count; i++)
            {
                float x = _rng.NextFloat(Constants.ARENA_LEFT + 5,
                                       Constants.ARENA_RIGHT - 5);
                float y = _rng.NextFloat(Constants.ARENA_TOP + 5,
                                       Constants.ARENA_BOTTOM - 5);
                _rabbits.Add(new Rabbit
                {
                    Pos = new Vector2(x, y),
                    Vel = Vector2.Zero,
                    z = 0f,
                    vz = 0f,
                    WanderTimer = 0f,
                    GrowthDuration = Constants.RABBIT_GROW_TIME,
                    Age = Constants.RABBIT_GROW_TIME,
                    Hunger = 0f,
                    FullTimer = 0f,
                    HomeId = -1
                });
            }
        }

        private void SpawnWolves(int count)
        {
            for (int i = 0; i < count; i++)
            {
                float x = _rng.NextFloat(Constants.ARENA_LEFT + 5,
                                       Constants.ARENA_RIGHT - 5);
                float y = _rng.NextFloat(Constants.ARENA_TOP + 5,
                                       Constants.ARENA_BOTTOM - 5);
                _wolves.Add(new Wolf
                {
                    Pos = new Vector2(x, y),
                    Vel = Vector2.Zero,
                    z = 0f,
                    vz = 0f,
                    WanderTimer = 0f,
                    GrowthDuration = Constants.WOLF_GROW_TIME,
                    Age = Constants.WOLF_GROW_TIME,
                    Hunger = 0f,
                    FullTimer = 0f,
                    HomeId = -1
                });
            }
        }

        private void SpawnBerryBushes(int count)
        {
            for (int i = 0; i < count; i++)
            {
                float x = _rng.NextFloat(Constants.ARENA_LEFT + 5,
                                       Constants.ARENA_RIGHT - 5);
                float y = _rng.NextFloat(Constants.ARENA_TOP + 5,
                                       Constants.ARENA_BOTTOM - 5);
                _bushes.Add(new BerryBush(new Vector2(x, y), _rng));
            }
        }

        private void SpawnWood(int count)
        {
            for (int i = 0; i < count; i++)
            {
                float x = _rng.NextFloat(Constants.ARENA_LEFT + 5,
                                       Constants.ARENA_RIGHT - 5);
                float y = _rng.NextFloat(Constants.ARENA_TOP + 5,
                                       Constants.ARENA_BOTTOM - 5);
                _wood.Add(new Wood(new Vector2(x, y), _rng));
            }
        }

        private void SpawnStones(int count)
        {
            for (int i = 0; i < count; i++)
            {
                float x = _rng.NextFloat(Constants.ARENA_LEFT + 5,
                                       Constants.ARENA_RIGHT - 5);
                float y = _rng.NextFloat(Constants.ARENA_TOP + 5,
                                       Constants.ARENA_BOTTOM - 5);
                _stones.Add(new Stone(new Vector2(x, y), _rng));
            }
        }


        private bool Edge(KeyboardState kb, Keys k)
            => kb.IsKeyDown(k) && _prevKb.IsKeyUp(k);

        private Color AbilityColor(Ability a)
            => a switch
            {
                Ability.None => Color.LightGray,
                Ability.Fire => Color.OrangeRed,
                Ability.Telekinesis => Color.MediumPurple,
                Ability.Explosion => Color.Gold,
                Ability.Precipitate => Color.SkyBlue,
                _ => Color.White
            };

        private void DrawAbilityToolbar()
        {
            _sb.Draw(_px, _abilityButtonRect, AbilityColor(_currentAbility));

            if (_abilityMenuOpen)
            {
                for (int i = 0; i < _abilityOptionRects.Length; i++)
                {
                    var rect = _abilityOptionRects[i];
                    _sb.Draw(_px, rect, AbilityColor(_abilityOptions[i]));
                }
            }
        }

        private void DrawHint()
        {
            int berries = _buildings.Sum(b => b.StoredBerries);
            int wood = _buildings.Sum(b => b.StoredWood);
            string text = $"BERRIES: {berries}   WOOD: {wood}";
            _sb.DrawString(_font, text, new Vector2(35, 8), Color.White);
        }

        private static void DrawGround(SpriteBatch sb, GroundMap map, Texture2D px, float zoom, Rectangle visible)
        {
            int cellSize = Constants.CELL_PIXELS;
            int x0 = Math.Clamp(visible.Left / cellSize, 0, map.W - 1);
            int y0 = Math.Clamp(visible.Top / cellSize, 0, map.H - 1);
            int x1 = Math.Clamp((visible.Right + cellSize - 1) / cellSize, 0, map.W);
            int y1 = Math.Clamp((visible.Bottom + cellSize - 1) / cellSize, 0, map.H);

            float shade = 0f;
            if (zoom >= 4f) shade = 0f;
            else if (zoom >= 3f) shade = Constants.GROUND_SHADE_ALPHA_MID;
            else if (zoom >= 2f) shade = Constants.GROUND_SHADE_ALPHA_NEAR;

            for (int y = y0; y < y1; y++)
            {
                for (int x = x0; x < x1; x++)
                {
                    var cell = map.Cells[x, y];
                    var style = BiomeTypes.Get(cell.Biome);
                    int pxX = Constants.ARENA_LEFT + x * cellSize;
                    int pxY = Constants.ARENA_TOP + y * cellSize;
                    sb.Draw(px, new Rectangle(pxX, pxY, cellSize, cellSize), style.Base);

                    // shade pattern
                    if (shade > 0f)
                    {
                        for (int iy = 0; iy < cellSize; iy++)
                            for (int ix = 0; ix < cellSize; ix++)
                                if (PatternAtlas.IsSet(style.PatternId, ix + cell.Variant, iy + cell.Variant))
                                {
                                    var r = new Rectangle(pxX + ix, pxY + iy, 1, 1);
                                    var col = style.Shade * shade;
                                    sb.Draw(px, r, col);
                                }
                    }

                    // borders
                    if (x + 1 < map.W && map.Cells[x + 1, y].Biome != cell.Biome)
                        sb.Draw(px, new Rectangle(pxX + cellSize - 1, pxY, 1, cellSize),
                            style.Base * Constants.GROUND_BORDER_ALPHA);
                    if (y + 1 < map.H && map.Cells[x, y + 1].Biome != cell.Biome)
                        sb.Draw(px, new Rectangle(pxX, pxY + cellSize - 1, cellSize, 1),
                            style.Base * Constants.GROUND_BORDER_ALPHA);

                    // sparse decal
                    if (zoom >= 4f)
                    {
                        int hash = (x * 73856093) ^ (y * 19349663);
                        if ((hash & 7) == 0)
                        {
                            int dx = hash % cellSize;
                            int dy = (hash >> 3) % cellSize;
                            sb.Draw(px, new Rectangle(pxX + dx, pxY + dy, 1, 1), style.Detail);
                        }
                    }
                }
            }
        }

        private static void DrawWater(SpriteBatch sb, WaterMap water, Texture2D px, float zoom, float time)
        {
            Color shallow = new Color(80, 150, 200);
            Color deep = new Color(10, 40, 80);
            int pxSize = 2;

            for (int y = 0; y < water.Height; y += pxSize)
            {
                for (int x = 0; x < water.Width; x += pxSize)
                {
                    byte d = water.Depth[x, y];
                    if (d == 0) continue;
                    float t = d / 255f;
                    Color baseCol = Color.Lerp(shallow, deep, t);

                    sbyte fx = water.FlowX[x, y];
                    sbyte fy = water.FlowY[x, y];
                    float phase = time * 2f + (x * fx + y * fy) * 0.1f;
                    float wave = (MathF.Sin(phase) + 1f) * 0.5f;
                    Color col = Color.Lerp(baseCol, shallow, wave * 0.3f);

                    sb.Draw(px,
                        new Rectangle(Constants.ARENA_LEFT + x, Constants.ARENA_TOP + y, pxSize, pxSize),
                        col);
                }
            }
        }

        private static string JobLabel(JobType job)
        {
            return job switch
            {
                JobType.None => "IDLE",
                JobType.HarvestBerries => "BERRY",
                JobType.ChopTree => "CHOP",
                JobType.HaulWood => "HAUL",
                JobType.DepositResource => "DEPOSIT",
                _ => job.ToString().ToUpper()
            };
        }

        private void DrawMeepleStats(Meeple m, Point mouse)
        {
            var list = new List<string>
            {
                m.Name,
                $"HP{(int)MathF.Ceiling(m.Health)}",
                $"HU{(int)MathF.Ceiling(m.Hunger)}",
                $"B{m.CarriedBerries}",
                $"ST{m.Strength}",
                $"DX{m.Dexterity}",
                $"IN{m.Intellect}",
                $"GR{m.Grit}"
            };
            if (m.Worker != null)
                list.Add(JobLabel(m.Worker.Value.CurrentJob));

            string[] lines = list.ToArray();

            int width = lines.Max(l => (int)_font.MeasureString(l).X) + 2;
            int height = lines.Length * _font.LineSpacing + 2;
            int x = mouse.X + 8;
            int y = mouse.Y + 8;

            _sb.Draw(_px, new Rectangle(x, y, width, height), new Color(0, 0, 0, 180));
            for (int i = 0; i < lines.Length; i++)
                _sb.DrawString(_font, lines[i], new Vector2(x + 1, y + 1 + i * _font.LineSpacing), Color.White);
        }

        private void DrawManaRing(Point mouse)
        {
            float ratio = _mana / Constants.MANA_MAX;
            int radius = 10;
            int segments = 32;
            for (int i = 0; i < segments; i++)
            {
                float t = i / (float)segments;
                if (t > ratio) break;
                float ang = t * MathF.PI * 2f;
                int x = (int)MathF.Round(mouse.X + MathF.Cos(ang) * radius);
                int y = (int)MathF.Round(mouse.Y + MathF.Sin(ang) * radius);
                _sb.Draw(_px, new Rectangle(x, y, 1, 1), Color.SkyBlue);
            }
        }

        private string ObjectLabelAt(Point p)
        {
            for (int i = _pixels.Count - 1; i >= 0; i--)
                if (_pixels[i].Bounds.Contains(p))
                    return "Debris";

            for (int i = _meeples.Count - 1; i >= 0; i--)
            {
                var m = _meeples[i];
                int half = Constants.ENEMY_H / 2;
                for (int part = -half; part < half; part++)
                {
                    var segPos = m.GetPartPos(part);
                    segPos.Y -= m.z;
                    if ((int)MathF.Round(segPos.X) == p.X &&
                        (int)MathF.Round(segPos.Y) == p.Y)
                        return $"Meeple {m.Name}";
                }
            }

            foreach (var l in _wood)
            {
                int bx = (int)MathF.Round(l.Pos.X);
                int by = (int)MathF.Round(l.Pos.Y);
                foreach (var off in l.Shape)
                    if (bx + off.X == p.X && by + off.Y == p.Y)
                        return "Wood";
            }

            foreach (var r in _rabbits)
            {
                int bx = (int)MathF.Round(r.Pos.X);
                int by = (int)MathF.Round(r.Pos.Y - r.z);
                if (bx == p.X && by == p.Y)
                    return "Rabbit";
            }

            foreach (var w in _wolves)
            {
                int bx = (int)MathF.Round(w.Pos.X);
                int by = (int)MathF.Round(w.Pos.Y - w.z);
                if (bx == p.X && by == p.Y)
                    return "Wolf";
            }

            foreach (var s in _stones)
            {
                int bx = (int)MathF.Round(s.Pos.X);
                int by = (int)MathF.Round(s.Pos.Y);
                foreach (var off in s.Shape)
                    if (bx + off.X == p.X && by + off.Y == p.Y)
                        return "Stone";
            }

            foreach (var b in _bushes)
            {
                int bx = (int)MathF.Round(b.Pos.X);
                int by = (int)MathF.Round(b.Pos.Y);
                foreach (var off in b.TrunkPixels)
                    if (bx + off.X == p.X && by + off.Y == p.Y)
                        return "BerryBush";
                foreach (var off in b.LeafPixels)
                    if (bx + off.X == p.X && by + off.Y == p.Y)
                        return "BerryBush";
            }

            foreach (var t in _trees)
            {
                int bx = (int)MathF.Round(t.Pos.X);
                int by = (int)MathF.Round(t.Pos.Y);
                foreach (var off in t.TrunkPixels)
                    if (bx + off.X == p.X && by + off.Y == p.Y)
                        return "Tree";
                foreach (var off in t.LeafPixels)
                    if (bx + off.X == p.X && by + off.Y == p.Y)
                        return "Tree";
            }

            foreach (var seed in _seeds)
            {
                int bx = (int)MathF.Round(seed.Pos.X);
                int by = (int)MathF.Round(seed.Pos.Y - seed.z);
                if (bx == p.X && by == p.Y)
                    return "Seed";
            }

            foreach (var bld in _buildings)
            {
                int bx = (int)MathF.Round(bld.Pos.X);
                int by = (int)MathF.Round(bld.Pos.Y);
                var rect = new Rectangle(bx - 1, by - 1, 3, 3);
                if (rect.Contains(p))
                    return bld.Kind.ToString();
            }

            int tx = (p.X - Constants.ARENA_LEFT) / Constants.TILE_SIZE;
            int ty = (p.Y - Constants.ARENA_TOP) / Constants.TILE_SIZE;
            if (tx >= 0 && tx < Chunk.Size && ty >= 0 && ty < Chunk.Size)
            {
                var tile = _world.Chunks[0, 0].Tiles[tx, ty];
                return tile.Type.ToString();
            }

            return "None";
        }

        private void DrawDebugOverlay()
        {
            var ms = Mouse.GetState();
            var world = new Vector2(_camX + ms.X / _zoom, _camY + ms.Y / _zoom);
            var p = new Point((int)MathF.Round(world.X), (int)MathF.Round(world.Y));

            string obj = ObjectLabelAt(p);
            int chunks = _world.Chunks.GetLength(0) * _world.Chunks.GetLength(1);
            string line1 = $"Mouse {p.X},{p.Y} | {obj} | Chunks {chunks}";
            string line2 = $"FPS {_fps} | Weather {_weather} | Trees {_trees.Count} | Entities {_meeples.Count}";
            var size1 = _font.MeasureString(line1);
            var size2 = _font.MeasureString(line2);
            float width = MathF.Max(size1.X, size2.X);
            var rect = new Rectangle(
                5,
                GraphicsDevice.Viewport.Height - (int)(size1.Y + size2.Y + 6) - 5,
                (int)width + 4,
                (int)(size1.Y + size2.Y + 6));
            _sb.Draw(_px, rect, new Color(0,0,0,180));
            _sb.DrawString(_font, line1, new Vector2(rect.X + 2, rect.Y + 2), Color.Yellow);
            _sb.DrawString(_font, line2, new Vector2(rect.X + 2, rect.Y + 4 + size1.Y), Color.Yellow);
        }

        private void DrawChunkBorders()
        {
            int width = _world.Chunks.GetLength(0) * Constants.CHUNK_PIXEL_SIZE;
            int height = _world.Chunks.GetLength(1) * Constants.CHUNK_PIXEL_SIZE;
            int left = Constants.ARENA_LEFT;
            int top = Constants.ARENA_TOP;

            _sb.Draw(_px, new Rectangle(left, top, width, 1), Color.Red);
            _sb.Draw(_px, new Rectangle(left, top + height - 1, width, 1), Color.Red);
            _sb.Draw(_px, new Rectangle(left, top, 1, height), Color.Red);
            _sb.Draw(_px, new Rectangle(left + width - 1, top, 1, height), Color.Red);
        }

        private void InitCloud()
        {
            _cloudPixels.Clear();
            int count = 40;
            for (int i = 0; i < count; i++)
            {
                float ang = MathHelper.ToRadians(_rng.Next(360));
                float r = _rng.NextFloat(0f, 1f);
                float rx = 8f * r;
                float ry = 3f * r;
                var off = new Vector2(MathF.Cos(ang) * rx, MathF.Sin(ang) * ry);
                byte g = (byte)_rng.Next(150, 220);
                _cloudPixels.Add(new Pixel(off, Vector2.Zero, new Color(g, g, g)));
            }

            // Add some darker pixels for depth
            int darkCount = count / 2;
            for (int i = 0; i < darkCount; i++)
            {
                float ang = MathHelper.ToRadians(_rng.Next(360));
                float r = _rng.NextFloat(0f, 1f);
                float rx = 8f * r;
                float ry = 3f * r;
                var off = new Vector2(MathF.Cos(ang) * rx, MathF.Sin(ang) * ry);
                byte g = (byte)_rng.Next(80, 150);
                _cloudPixels.Add(new Pixel(off, Vector2.Zero, new Color(g, g, g)));
            }
        }

        private void UpdateCloud(float dt)
        {
            for (int i = 0; i < _cloudPixels.Count; i++)
            {
                var p = _cloudPixels[i];
                p.Pos += p.Vel * dt;
                p.Vel *= 0.9f;
                p.Vel += new Vector2(
                    _rng.NextFloat(-1f, 1f),
                    _rng.NextFloat(-1f, 1f)) * Constants.PRECIPITATE_CLOUD_JITTER * dt;
                _cloudPixels[i] = p;
            }
        }

        private void DrawFlame(Meeple e)
        {
            var pos = e.GetPartPos(0);
            pos.Y -= e.z + 1f;
            int size = 3 + _rng.Next(2);
            int offX = _rng.Next(-1, 2);
            int offY = _rng.Next(-2, 1);
            var rect = new Rectangle(
                (int)pos.X - size / 2 + offX,
                (int)pos.Y - size + offY,
                size,
                size * 2
            );
            Color[] firePal =
            {
                Color.OrangeRed,
                Color.Orange,
                Color.Yellow,
                new Color(255, 100, 0)
            };
            var col = firePal[_rng.Next(firePal.Length)];
            _sb.Draw(_px, rect, col);
        }

        private void DrawBushBottom(BerryBush b)
        {
            int baseX = (int)MathF.Round(b.Pos.X);
            int baseY = (int)MathF.Round(b.Pos.Y);
            foreach (var p in b.TrunkPixels)
                _sb.Draw(_px, new Rectangle(baseX + p.X, baseY + p.Y, 1, 1), new Color(100, 70, 40));
        }

        private void DrawBushTop(BerryBush b)
        {
            int baseX = (int)MathF.Round(b.Pos.X);
            int baseY = (int)MathF.Round(b.Pos.Y);
            foreach (var p in b.LeafPixels)
                _sb.Draw(_px, new Rectangle(baseX + p.X, baseY + p.Y, 1, 1), new Color(20, 110, 20));
            for (int i = 0; i < b.Berries && i < b.BerryPixels.Length; i++)
            {
                var off = b.BerryPixels[i];
                _sb.Draw(_px, new Rectangle(baseX + off.X, baseY + off.Y, 1, 1), Color.Red);
            }
        }

        private void DrawBushFlame(BerryBush b)
        {
            int baseX = (int)MathF.Round(b.Pos.X);
            int baseY = (int)MathF.Round(b.Pos.Y);

            int minX = int.MaxValue, maxX = int.MinValue;
            int minY = 0, maxY = 0;
            foreach (var p in b.TrunkPixels)
            {
                if (minX == int.MaxValue)
                {
                    minX = maxX = p.X;
                    minY = maxY = p.Y;
                }
                else
                {
                    if (p.X < minX) minX = p.X;
                    if (p.X > maxX) maxX = p.X;
                    if (p.Y < minY) minY = p.Y;
                    if (p.Y > maxY) maxY = p.Y;
                }
            }
            foreach (var p in b.LeafPixels)
            {
                if (p.X < minX) minX = p.X;
                if (p.X > maxX) maxX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.Y > maxY) maxY = p.Y;
            }

            var rect = new Rectangle(baseX + minX - 1, baseY + minY - 1, (maxX - minX + 3), (maxY - minY + 3));
            Color[] firePal = { Color.OrangeRed, Color.Orange, Color.Yellow, new Color(255, 100, 0) };
            var col = firePal[_rng.Next(firePal.Length)];
            _sb.Draw(_px, rect, col);
        }

        private void DrawBushShadow(BerryBush b)
        {
            int baseX = (int)MathF.Round(b.Pos.X);
            int baseY = (int)MathF.Round(b.Pos.Y);

            int radX = (int)MathF.Round(b.ShadowRadius);
            int radY = Math.Max(1, radX / 2);

            byte alpha = 80;
            var col = new Color((byte)0, (byte)0, (byte)0, alpha);

            for (int y = -radY; y <= radY; y++)
            {
                for (int x = -radX; x <= radX; x++)
                {
                    float nx = x / (float)radX;
                    float ny = y / (float)radY;
                    if (nx * nx + ny * ny <= 1f)
                        _sb.Draw(_px, new Rectangle(baseX + x, baseY + y, 1, 1), col);
                }
            }
        }

        private void DrawBuilding(Building b)
        {
            var pos = b.Pos;
            switch (b.Kind)
            {
                case BuildingType.StorageHut:
                    _sb.Draw(_px, new Rectangle((int)pos.X - 1, (int)pos.Y - 1, 3, 3), Color.SaddleBrown);
                    _sb.Draw(_px, new Rectangle((int)pos.X, (int)pos.Y - 2, 1, 1), Color.BurlyWood);
                    int stackH = Building.CAPACITY / 3;
                    for (int i = 0; i < b.StoredBerries; i++)
                    {
                        int col = i / stackH;
                        int level = i % stackH;
                        int x = (int)pos.X - 1 + col;
                        int y = (int)pos.Y - 3 - level;
                        _sb.Draw(_px, new Rectangle(x, y, 1, 1), Color.Red);
                    }
                    for (int i = 0; i < b.StoredWood; i++)
                    {
                        int col = (i + b.StoredBerries) / stackH;
                        int level = (i + b.StoredBerries) % stackH;
                        int x = (int)pos.X - 1 + col;
                        int y = (int)pos.Y - 3 - level;
                        _sb.Draw(_px, new Rectangle(x, y, 1, 1), Color.SaddleBrown);
                    }
                    break;
                case BuildingType.HousingHut:
                    _sb.Draw(_px, new Rectangle((int)pos.X - 1, (int)pos.Y - 1, 3, 3), Color.Sienna);
                    _sb.Draw(_px, new Rectangle((int)pos.X, (int)pos.Y - 2, 1, 1), Color.Peru);
                    break;
            }
        }

        private void DrawStone(Stone s)
        {
            int baseX = (int)MathF.Round(s.Pos.X);
            int baseY = (int)MathF.Round(s.Pos.Y);
            foreach (var p in s.Shape)
                _sb.Draw(_px, new Rectangle(baseX + p.X, baseY + p.Y, 1, 1), s.Color);
        }

        private void DrawWood(Wood l)
        {
            int baseX = (int)MathF.Round(l.Pos.X);
            int baseY = (int)MathF.Round(l.Pos.Y);
            foreach (var p in l.Shape)
                _sb.Draw(_px, new Rectangle(baseX + p.X, baseY + p.Y, 1, 1), l.Color);
        }

        private void DrawRabbitHome(RabbitHole h)
        {
            int x = (int)MathF.Round(h.Pos.X);
            int y = (int)MathF.Round(h.Pos.Y);
            _sb.Draw(_px, new Rectangle(x - 1, y, 3, 1), Color.Black);
        }

        private void DrawWolfDen(WolfDen d)
        {
            int x = (int)MathF.Round(d.Pos.X);
            int y = (int)MathF.Round(d.Pos.Y);
            _sb.Draw(_px, new Rectangle(x - 1, y, 3, 1), Color.Gray);
        }

        // Palette
        static readonly Color Fur = new Color(235, 235, 235); // off‑white
        static readonly Color FurShade = new Color(210, 210, 210);
        static readonly Color EarPink = new Color(255, 180, 190);
        static readonly Color Eye = new Color(40, 40, 40);
        static readonly Color Nose = Color.Brown;
        static readonly Color Shadow = new Color(0, 0, 0, 90);

        // Baby 3x3 sprite
        static readonly string[] Rabbit3 =
        {
            "..p",
            "WWW",
            "sss",
        };

        // 6x6 glyph ('.' = empty). Designed facing RIGHT; mirrored when facing left.
        static readonly string[] Rabbit6 =
        {
            "..p...",
            "..pWW.",
            ".WWoW.",
            "WWWWW.",
            "WWWW..",
            "ssssss", // s = shadow hint directly under body (optional)
        };

        static readonly Color WolfFur = new Color(160, 160, 160);
        static readonly Color WolfFurShade = new Color(130, 130, 130);
        static readonly Color WolfEar = new Color(200, 200, 200);
        static readonly Color WolfEye = new Color(40, 40, 40);
        static readonly Color WolfNose = Color.Black;
        static readonly string[] Wolf8 =
        {
            "....e...",
            "W...eWW.",
            ".W..WoWb",
            "..WWWWW.",
            "..WWWWW.",
            "..W...W.",
            "..W...W.",
            "..sssss.",
        };


        /// <summary>
        /// Draw a rabbit at r.Pos, lifted by r.z. Babies use a smaller 3x3 sprite.
        /// </summary>
        private void DrawRabbit(Rabbit r)
        {
            bool baby = r.Age < r.GrowthDuration;
            int size = baby ? 3 : 6;
            // Base (top‑left) so the sprite is centered on Pos
            int baseX = (int)MathF.Round(r.Pos.X - size / 2f);
            int baseY = (int)MathF.Round(r.Pos.Y - size / 2f - r.z);

            bool faceRight = r.Vel.X >= 0f;

            // Soft round shadow that grows slightly with z
            int shW = (baby ? 2 : 4) + (int)MathF.Min(2f, r.z * 0.25f);
            int shH = (baby ? 1 : 2) + (int)MathF.Min(2f, r.z * 0.15f);
            var shRect = new Rectangle(
                (int)MathF.Round(r.Pos.X - shW * 0.5f),
                (int)MathF.Round(r.Pos.Y + 1), // on ground
                shW, shH
            );
            _sb.Draw(_px, shRect, Shadow);

            var sprite = baby ? Rabbit3 : Rabbit6;
            for (int gy = 0; gy < size; gy++)
            {
                string row = sprite[gy];
                for (int gx = 0; gx < size; gx++)
                {
                    // Mirror horizontally if needed
                    int ix = faceRight ? gx : (size - 1 - gx);
                    char ch = row[ix];
                    if (ch == '.') continue;

                    Color c = ch switch
                    {
                        'W' => Fur,
                        'p' => EarPink,
                        'o' => Eye,
                        'b' => Nose,
                        's' => Shadow,      // small body shadow hints
                        _ => FurShade
                    };

                    // Slight fur dither: alternate a few pixels to break flatness
                    if (ch == 'W' && ((gx + gy) & 1) == 1)
                        c = FurShade;

                    _sb.Draw(_px, new Rectangle(baseX + gx, baseY + gy, 1, 1), c);
                }
            }
        }

        private void DrawWolf(Wolf w)
        {
            int baseX = (int)MathF.Round(w.Pos.X - 3);
            int baseY = (int)MathF.Round(w.Pos.Y - 3 - w.z);

            bool faceRight = w.Vel.X >= 0f;

            int shW = 4 + (int)MathF.Min(2f, w.z * 0.25f);
            int shH = 2 + (int)MathF.Min(2f, w.z * 0.15f);
            var shRect = new Rectangle(
                (int)MathF.Round(w.Pos.X - shW * 0.5f),
                (int)MathF.Round(w.Pos.Y + 1),
                shW, shH);
            _sb.Draw(_px, shRect, Shadow);

            for (int gy = 0; gy < 8; gy++)
            {
                string row = Wolf8[gy];
                for (int gx = 0; gx < 8; gx++)
                {
                    int ix = faceRight ? gx : (7 - gx);
                    char ch = row[ix];
                    if (ch == '.') continue;

                    Color c = ch switch
                    {
                        'W' => WolfFur,
                        'e' => WolfEar,
                        'o' => WolfEye,
                        'b' => WolfNose,
                        's' => Shadow,
                        _ => WolfFurShade
                    };

                    if (ch == 'W' && ((gx + gy) & 1) == 1)
                        c = WolfFurShade;

                    _sb.Draw(_px, new Rectangle(baseX + gx, baseY + gy, 1, 1), c);
                }
            }
        }


        private void DrawTreeBottom(Tree t)
        {
            int baseX = (int)MathF.Round(t.Pos.X);
            int baseY = (int)MathF.Round(t.Pos.Y);
            if (t.IsStump)
            {
                for (int y = 0; y > -2; y--)
                    for (int x = -1; x <= 1; x++)
                        _sb.Draw(_px, new Rectangle(baseX + x, baseY + y, 1, 1), new Color(100, 70, 40));
            }
            else
            {
                float pale = t.IsDead ? MathF.Min(1f, t.PaleTimer / Constants.TREE_PALE_TIME) : 0f;
                float angle = 0f;
                if (t.FallTimer > 0f || t.Fallen)
                    angle = t.FallDir * MathHelper.PiOver2 * MathF.Min(t.FallTimer / Constants.TREE_FALL_TIME, 1f);
                float cos = MathF.Cos(angle);
                float sin = MathF.Sin(angle);
                float decomp = t.Fallen ? MathF.Min(1f, t.DecompTimer / Constants.TREE_DISINTEGRATE_TIME) : 0f;
                foreach (var p in t.TrunkPixels)
                {
                    float x = p.X * cos - p.Y * sin;
                    float y = p.X * sin + p.Y * cos;
                    int dx = baseX + (int)MathF.Round(x);
                    int dy = baseY + (int)MathF.Round(y);
                    Color col = Color.Lerp(t.TrunkBase, t.TrunkTip, Math.Clamp((-p.Y) / Math.Max(1, t.MaxHeight), 0f, 1f));
                    col = Color.Lerp(col, Color.SandyBrown, pale * 0.5f);
                    if (decomp > 0f)
                    {
                        float skipChance = MathF.Min(decomp, 0.9f);
                        int hash = (dx * 73856093) ^ (dy * 19349663);
                        double r = ((hash & 0x7fffffff) / (double)int.MaxValue);
                        if (r < skipChance)
                            continue;
                        col = Color.Lerp(col, Color.SandyBrown, decomp);
                    }
                    _sb.Draw(_px, new Rectangle(dx, dy, 1, 1), col);
                }
            }
        }

        private void DrawTreeShadow(Tree t)
        {
            if (t.Fallen)
                return;

            int baseX = (int)MathF.Round(t.Pos.X);
            int baseY = (int)MathF.Round(t.Pos.Y);

            byte alpha = 80;
            var col = new Color((byte)0, (byte)0, (byte)0, alpha);

            if (t.FallTimer <= 0f)
            {
                int radX = (int)MathF.Round(t.ShadowRadius);
                int radY = Math.Max(1, radX / 2);

                for (int y = -radY; y <= radY; y++)
                {
                    for (int x = -radX; x <= radX; x++)
                    {
                        float nx = x / (float)radX;
                        float ny = y / (float)radY;
                        if (nx * nx + ny * ny <= 1f)
                            _sb.Draw(_px, new Rectangle(baseX + x, baseY + y, 1, 1), col);
                    }
                }
                return;
            }

            float angle = t.FallDir * MathHelper.PiOver2 * MathF.Min(t.FallTimer / Constants.TREE_FALL_TIME, 1f);
            float cos = MathF.Cos(angle);
            float sin = MathF.Sin(angle);
            foreach (var p in t.TrunkPixels)
            {
                float x = p.X * cos - p.Y * sin;
                float y = p.X * sin + p.Y * cos;
                int dx = baseX + (int)MathF.Round(x);
                int dy = baseY + (int)MathF.Round(y);
                _sb.Draw(_px, new Rectangle(dx, dy, 1, 1), col);
            }
            foreach (var p in t.LeafPixels)
            {
                float x = p.X * cos - p.Y * sin;
                float y = p.X * sin + p.Y * cos;
                int dx = baseX + (int)MathF.Round(x);
                int dy = baseY + (int)MathF.Round(y);
                _sb.Draw(_px, new Rectangle(dx, dy, 1, 1), col);
            }
        }

        private void DrawTreeTop(Tree t)
        {
            if (t.IsStump) return;

            int baseX = (int)MathF.Round(t.Pos.X);
            int baseY = (int)MathF.Round(t.Pos.Y);
            float angle = 0f;
            if (t.FallTimer > 0f || t.Fallen)
                angle = t.FallDir * MathHelper.PiOver2 * MathF.Min(t.FallTimer / Constants.TREE_FALL_TIME, 1f);
            float cos = MathF.Cos(angle);
            float sin = MathF.Sin(angle);
            float leafDecay = t.IsDead ? MathF.Min(1f, t.LeafTimer / Constants.LEAF_DISINTEGRATE_TIME) : 0f;
            foreach (var p in t.LeafPixels)
            {
                float x = p.X * cos - p.Y * sin;
                float y = p.X * sin + p.Y * cos;
                int dx = baseX + (int)MathF.Round(x);
                int dy = baseY + (int)MathF.Round(y);

                if (leafDecay > 0f)
                {
                    float skipChance = MathF.Min(leafDecay, 0.95f);
                    int hash = (dx * 73856093) ^ (dy * 19349663);
                    double r = ((hash & 0x7fffffff) / (double)int.MaxValue);
                    if (r < skipChance) continue;
                }

                // subtle vertical gradient + dithering
                float tY = Math.Clamp((-p.Y) / Math.Max(1, t.MaxHeight), 0f, 1f);
                Color baseCol = Color.Lerp(t.LeafB, t.LeafA, tY);
                if (((dx + dy) & 1) == 1) baseCol = Color.Lerp(baseCol, t.LeafA, 0.25f);

                if (t.IsDead)
                    baseCol = Color.Lerp(baseCol, Color.Goldenrod, leafDecay);

                _sb.Draw(_px, new Rectangle(dx, dy, 1, 1), baseCol);
            }

        }

        private void DrawTreeFlame(Tree t)
        {
            int baseX = (int)MathF.Round(t.Pos.X);
            int baseY = (int)MathF.Round(t.Pos.Y);

            int minX = int.MaxValue, maxX = int.MinValue;
            int minY = 0, maxY = 0;
            bool haveLeaf = false;
            foreach (var p in t.LeafPixels)
            {
                if (!haveLeaf)
                {
                    minX = maxX = p.X;
                    minY = maxY = p.Y;
                    haveLeaf = true;
                }
                else
                {
                    if (p.X < minX) minX = p.X;
                    if (p.X > maxX) maxX = p.X;
                    if (p.Y < minY) minY = p.Y;
                    if (p.Y > maxY) maxY = p.Y;
                }
            }

            if (!haveLeaf)
            {
                foreach (var p in t.TrunkPixels)
                {
                    if (minX == int.MaxValue)
                    {
                        minX = maxX = p.X;
                        minY = maxY = p.Y;
                    }
                    else
                    {
                        if (p.X < minX) minX = p.X;
                        if (p.X > maxX) maxX = p.X;
                        if (p.Y < minY) minY = p.Y;
                        if (p.Y > maxY) maxY = p.Y;
                    }
                }
            }

            int width = maxX - minX + 1;
            int height = maxY - minY + 1;
            var rect = new Rectangle(
                baseX + minX - 1,
                baseY + minY - 1,
                width + 2,
                height + 2);
            Color[] firePal =
            {
                Color.OrangeRed,
                Color.Orange,
                Color.Yellow,
                new Color(255, 100, 0)
            };
            var col = firePal[_rng.Next(firePal.Length)];
            _sb.Draw(_px, rect, col);
        }

        private void DrawShadow(Meeple e)
        {
            bool isDead = e.State == MeepleState.Dead;
            float decomp = isDead
                ? MathF.Min(1f, e.DecompTimer / Constants.DECOMP_DURATION)
                : 0f;

            float stickLen = Constants.ENEMY_H * Constants.PART_LEN;
            int shLen = (int)MathF.Round(MathF.Abs(MathF.Sin(e.Angle)) * stickLen) + Constants.ENEMY_W;
            int shThick = 2;

            float shY = e.ShadowY;

            var shRect = new Rectangle(
                (int)MathF.Round(e.Pos.X - shLen / 2f),
                (int)MathF.Round(shY),
                shLen, shThick
            );
            byte shAlpha = (byte)(100 * (1f - decomp));
            _sb.Draw(_px, shRect, new Color((byte)0, (byte)0, (byte)0, shAlpha));
        }

        private void DrawMeepleSprite(Meeple e)
        {
            bool isDead = e.State == MeepleState.Dead;
            float decomp = isDead
                ? MathF.Min(1f, e.DecompTimer / Constants.DECOMP_DURATION)
                : 0f;

            int half = Constants.ENEMY_H / 2;
            for (int part = -half; part < half; part++)
            {
                var segPos = e.GetPartPos(part);
                segPos.Y -= e.z;

                int seg = part + half;
                Color c;
                if (seg <= 1)
                    c = Constants.HAND_COLOR;
                else if (seg <= Constants.ENEMY_H * 2 / 5)
                    c = e.ShirtColor;
                else if (seg <= Constants.ENEMY_H * 3 / 5)
                    c = e.ShirtColor;
                else if (seg <= Constants.ENEMY_H * 4 / 5)
                    c = e.Side == Faction.Friendly ? new Color(40, 70, 40) : new Color(128, 110, 90);
                else
                    c = e.Side == Faction.Friendly ? new Color(20, 40, 20) : new Color(80, 60, 40);

                if (isDead)
                    c = ApplyDecomposition(c, decomp);
                else if (e.State == MeepleState.Ragdoll)
                    c = Color.Lerp(c, Color.LightGray, 0.5f);

                float angle = e.Angle;
                float cos = MathF.Cos(angle);
                float sin = MathF.Sin(angle);

                if (e.State == MeepleState.Launched && MathF.Abs(e.AngularVel) > 0.01f)
                    DrawFatSegment(segPos, angle, Constants.ENEMY_W, Constants.PART_LEN, c);

                for (int dx = 0; dx < Constants.ENEMY_W; dx++)
                    for (int dy = 0; dy < (int)Constants.PART_LEN; dy++)
                    {
                        float localX = dx - (Constants.ENEMY_W * 0.5f - 0.5f);
                        float localY = dy - (Constants.PART_LEN * 0.5f - 0.5f);

                        float x = segPos.X + localX * cos - localY * sin;
                        float y = segPos.Y + localX * sin + localY * cos;

                        if (isDead && decomp > 0.5f)
                        {
                            float skipChance = MathF.Min((decomp - 0.5f) * 2f * 0.5f, 0.9f * 0.5f);
                            int hx = (int)MathF.Round(x), hy = (int)MathF.Round(y);
                            int hash = (hx * 73856093) ^ (hy * 19349663) ^ part;
                            double r = ((hash & 0x7fffffff) / (double)int.MaxValue);
                            if (r < skipChance)
                                continue;
                        }

                        _sb.Draw(_px, new Rectangle((int)MathF.Round(x), (int)MathF.Round(y), 1, 1), c);

                        if (e.IsBurning && _rng.NextDouble() < 0.03)
                        {
                            Color[] firePal = { Color.OrangeRed, Color.Orange, Color.Yellow, new Color(255, 100, 0) };
                            Color glowCol = firePal[_rng.Next(firePal.Length)];
                            var glow = new Rectangle((int)MathF.Round(x) + _rng.Next(-1, 2), (int)MathF.Round(y) + _rng.Next(-1, 2), 1, 1);
                            _sb.Draw(_px, glow, new Color(glowCol, 90));
                        }
                    }
            }

            {
                var bodyPos = e.GetPartPos(-1);
                bodyPos.Y -= e.z;
                float angle = e.Angle;
                float sideX = MathF.Cos(angle);
                float sideY = MathF.Sin(angle);
                float handOffset = Constants.ENEMY_W * 0.5f + 0.5f;

                float lx = bodyPos.X - sideX * handOffset;
                float ly = bodyPos.Y - sideY * handOffset;
                Color handCol = Constants.HAND_COLOR;
                if (isDead)
                    handCol = ApplyDecomposition(handCol, decomp);
                else if (e.State == MeepleState.Ragdoll)
                    handCol = Color.Lerp(handCol, Color.LightGray, 0.5f);
                _sb.Draw(_px, new Rectangle((int)MathF.Round(lx), (int)MathF.Round(ly), 1, 1), handCol);
                if (e.CarriedBerries > 0)
                    _sb.Draw(_px, new Rectangle((int)MathF.Round(lx), (int)MathF.Round(ly - 1), 1, 1), Color.Red);

                float rx = bodyPos.X + sideX * handOffset;
                float ry = bodyPos.Y + sideY * handOffset;
                _sb.Draw(_px, new Rectangle((int)MathF.Round(rx), (int)MathF.Round(ry), 1, 1), handCol);
            }

            if (e.IsBurning)
                DrawFlame(e);
        }

        private void DrawCloudShadow()
        {
            var ground = _cloudCenter - new Vector2(0f, Constants.PRECIPITATE_CLOUD_OFFSET_Y);
            int gx = (int)MathF.Round(ground.X);
            int gy = (int)MathF.Round(ground.Y);

            byte alpha = (byte)(80 * _rainAlpha);
            var col = new Color((byte)0, (byte)0, (byte)0, alpha);

            for (int y = -1; y <= 1; y++)
            {
                for (int x = -2; x <= 2; x++)
                {
                    float nx = x / 2f;
                    if (nx * nx + y * y <= 1f)
                        _sb.Draw(_px, new Rectangle(gx + x, gy + y, 1, 1), col);
                }
            }
        }

        private void TriggerExplosion(Vector2 pos)
        {
            // push soldiers away
            for (int i = 0; i < _meeples.Count; i++)
            {
                var s = _meeples[i];
                Vector2 dir = s.Pos - pos;
                float dist = dir.Length();
                if (dist > Constants.EXPLOSION_RADIUS)
                    continue;
                if (dist > 0f) dir /= dist; else dir = new Vector2(0f, -1f);
                float strength = (1f - dist / Constants.EXPLOSION_RADIUS) * Constants.EXPLOSION_PUSH;
                s.Vel += dir * strength;
                s.vz += Constants.EXPLOSION_UPWARD;
                s.State = MeepleState.Launched;
                _meeples[i] = s;
            }

            // affect nearby trees
            for (int i = 0; i < _trees.Count; i++)
            {
                var t = _trees[i];
                float dist = Vector2.Distance(t.Pos, pos);
                if (dist <= Constants.EXPLOSION_RADIUS && !t.IsDead)
                {
                    t.IsDead = true;
                    t.PaleTimer = 0f;
                    t.FallDelay = 0f;
                    t.FallTimer = 0f;
                    t.Fallen = false;
                    t.DecompTimer = 0f;
                    t.RemoveWhenFallen = true;
                    _trees[i] = t;
                }
            }

            LightingSystem.AddLight(
                _lights,
                pos,
                Constants.EXPLOSION_LIGHT_RADIUS,
                Constants.EXPLOSION_LIGHT_INTENSITY,
                0.5f);

            // visual particles
            Color[] smokePal = { Color.OrangeRed, Color.Orange, Color.Yellow, Color.Gray };
            for (int i = 0; i < Constants.EXPLOSION_PARTICLES; i++)
            {
                float ang = MathHelper.ToRadians(_rng.Next(360));
                float spd = _rng.NextFloat(10f, Constants.EXPLOSION_PUSH);
                var vel = new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * spd;
                var col = smokePal[_rng.Next(smokePal.Length)];
                _pixels.Spawn(new Pixel(
                    pos,
                    vel,
                    col,
                    0f,
                    _rng.NextFloat(Constants.DEBRIS_LIFETIME_MIN,
                                   Constants.DEBRIS_LIFETIME_MAX)));
            }
        }

        private static Color ApplyDecomposition(Color baseColor, float t)
        {
            var pale = Color.Lerp(baseColor, Color.LightGray, 0.5f);
            return t < 0.5f
                ? Color.Lerp(pale, Constants.DECOMP_PURPLE, t * 2f)
                : Color.Lerp(Constants.DECOMP_PURPLE, Constants.BONE_COLOR, (t - 0.5f) * 2f);
        }

        private void UpdateRain(float dt)
        {
            float viewW = GraphicsDevice.Viewport.Width / _zoom;
            float viewH = GraphicsDevice.Viewport.Height / _zoom;

            int count = (int)(Constants.RAIN_SPAWN_RATE * dt);
            for (int i = 0; i < count; i++)
            {
                float x = _camX + _rng.NextFloat(0f, viewW);
                float y = _camY + _rng.NextFloat(0f, viewH);
                float z0 = _rng.NextFloat(10f, 30f);
                _rain.Spawn(new RainDrop(new Vector2(x, y), z0, Constants.RAIN_SPEED));
            }

            for (int i = _rain.Count - 1; i >= 0; i--)
            {
                var r = _rain[i];
                r.z -= r.vz * dt;
                if (r.z <= 0f)
                {
                    _rain.RemoveAt(i);
                    continue;
                }
                _rain[i] = r;
            }
        }

        public GroundCell? GetCellAtWorld(int x, int y)
        {
            int cx = (x - Constants.ARENA_LEFT) / Constants.CELL_PIXELS;
            int cy = (y - Constants.ARENA_TOP) / Constants.CELL_PIXELS;
            if (cx < 0 || cy < 0 || cx >= _ground.W || cy >= _ground.H)
                return null;
            return _ground.Cells[cx, cy];
        }


        private void DrawFatSegment(Vector2 center, float angle, float width, float length, Color color)
        {
            var dir = new Vector2(MathF.Sin(angle), MathF.Cos(angle));
            var start = center - dir * (length * 0.5f);
            var end = center + dir * (length * 0.5f);

            float halfW = width * 0.5f;

            int minX = (int)MathF.Floor(MathF.Min(start.X, end.X) - halfW);
            int maxX = (int)MathF.Ceiling(MathF.Max(start.X, end.X) + halfW);
            int minY = (int)MathF.Floor(MathF.Min(start.Y, end.Y) - halfW);
            int maxY = (int)MathF.Ceiling(MathF.Max(start.Y, end.Y) + halfW);

            Vector2 ab = end - start;
            float abLenSq = ab.LengthSquared();

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
                    float t = 0f;
                    if (abLenSq > 0f)
                        t = Math.Clamp(Vector2.Dot(p - start, ab) / abLenSq, 0f, 1f);
                    Vector2 proj = start + ab * t;
                    float dist = Vector2.Distance(p, proj);
                    if (dist <= halfW)
                        _sb.Draw(_px, new Rectangle(x, y, 1, 1), color);
                }
            }
        }
    }
}
