using System;
using System.Collections.Generic;
using System.Linq;
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
        SpriteFont _font;

        List<Meeple> _meeples = new();
        List<Pixel> _pixels = new(Constants.MAX_DEBRIS);
        List<Seed> _seeds = new();
        List<BerryBush> _bushes = new();
        List<Log> _logs = new();
        List<Stone> _stones = new();
        List<Tree> _trees = new();
        List<Building> _buildings = new();
        Random _rng = new();

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

            SpawnMeeple(0);
            SpawnBerryBushes(5);
            SpawnLogs(4);
            SpawnStones(4);

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
                    _trees.Add(new Tree(new Vector2(cx + ox, cy + oy), _rng));
                }
            }

            var midX = (Constants.ARENA_LEFT + Constants.ARENA_RIGHT) * 0.5f;
            var midY = (Constants.ARENA_TOP + Constants.ARENA_BOTTOM) * 0.5f;
            _buildings.Add(new Building
            {
                Pos = new Vector2(midX, midY),
                Kind = BuildingType.StockpileHut,
                StoredBerries = 0,
                StoredLogs = 0,
                StoredPlanks = 0,
                CraftTimer = 0f
            });
            _buildings.Add(new Building
            {
                Pos = new Vector2(midX + 6, midY),
                Kind = BuildingType.CarpenterHut,
                StoredBerries = 0,
                StoredLogs = 0,
                StoredPlanks = 0,
                CraftTimer = 0f
            });
            _camX = midX - (GraphicsDevice.Viewport.Width * 0.5f) / _zoom;
            _camY = midY - (GraphicsDevice.Viewport.Height * 0.5f) / _zoom;
        }

        protected override void Update(GameTime gt)
        {
            float dt = (float)gt.ElapsedGameTime.TotalSeconds;
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
                    float minD = Constants.PICKUP_RADIUS;
                    for (int i = _meeples.Count - 1; i >= 0; i--)
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
                                i = -1; // break outer
                                break;
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
                    if (_dragging)
                        _mana = MathF.Max(0f, _mana - Constants.TELEKINESIS_DRAIN * dt);
                }
                else
                {
                    _dragging = false;
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

                    var ground = _cloudCenter - new Vector2(0f, Constants.PRECIPITATE_CLOUD_OFFSET_Y);
                    for (int b = 0; b < _bushes.Count; b++)
                    {
                        var bush = _bushes[b];
                        if (bush.Berries >= Constants.BUSH_BERRIES)
                        {
                            bush.RegrowTimer = 0f;
                            _bushes[b] = bush;
                            continue;
                        }

                        var diff = bush.Pos - ground;
                        float ellipse = (diff.X / 2f) * (diff.X / 2f) + diff.Y * diff.Y;
                        if (ellipse <= 1f)
                        {
                            bush.RegrowTimer += dt;
                            if (bush.RegrowTimer >= Constants.BUSH_REGROW_INTERVAL)
                            {
                                bush.RegrowTimer = 0f;
                                bush.Berries++;
                            }
                        }
                        else
                        {
                            bush.RegrowTimer = 0f;
                        }
                        _bushes[b] = bush;
                    }
                }
            }
            else
            {
                _dragging = false;
            }

            PhysicsSystem.SimulateAll(_meeples, _pixels, _bushes, _buildings, _trees, _logs, dt);
            PhysicsSystem.UpdatePixels(_pixels, dt);
            PhysicsSystem.UpdateLogs(_logs, dt);
            PhysicsSystem.UpdateSeeds(_seeds, _trees, dt);
            PhysicsSystem.UpdateTrees(_trees, _seeds, dt);
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
            GraphicsDevice.Clear(Color.DarkSeaGreen);
            var cam = Matrix.CreateScale(_zoom, _zoom, 1f)
                      * Matrix.CreateTranslation(-_camX * _zoom, -_camY * _zoom, 0);
            _sb.Begin(transformMatrix: cam, samplerState: SamplerState.PointClamp);

            // --- arena border ---
            // Removed dark border bars

            // --- debris ---
            foreach (var p in _pixels)
                _sb.Draw(_px, p.Bounds, p.Col);

            foreach (var s in _seeds)
            {
                var rect = new Rectangle((int)s.Pos.X, (int)s.Pos.Y, 1, 1);
                _sb.Draw(_px, rect, Color.SandyBrown);
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

            // --- berry bushes ---
            foreach (var b in _bushes)
                DrawBush(b);

            // --- stones ---
            foreach (var s in _stones)
                DrawStone(s);

            // --- logs ---
            foreach (var l in _logs)
                DrawLog(l);

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

            // --- airborne soldiers/entities ---
            foreach (var e in _meeples.Where(m => m.z > 0f).OrderBy(m => m.ShadowY))
                DrawMeepleSprite(e);

            _sb.End();

            float phase = MathF.Sin(_timeOfDay * MathHelper.TwoPi) * 0.5f + 0.5f;
            float ambient = MathHelper.Lerp(Constants.NIGHT_BRIGHTNESS, 1f, phase);
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

        private void SpawnLogs(int count)
        {
            for (int i = 0; i < count; i++)
            {
                float x = _rng.NextFloat(Constants.ARENA_LEFT + 5,
                                       Constants.ARENA_RIGHT - 5);
                float y = _rng.NextFloat(Constants.ARENA_TOP + 5,
                                       Constants.ARENA_BOTTOM - 5);
                _logs.Add(new Log(new Vector2(x, y), _rng));
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

        private void SpawnTrees(int count)
        {
            for (int i = 0; i < count; i++)
            {
                float x = _rng.NextFloat(Constants.ARENA_LEFT + 5,
                                       Constants.ARENA_RIGHT - 5);
                float y = _rng.NextFloat(Constants.ARENA_TOP + 5,
                                       Constants.ARENA_BOTTOM - 5);
                _trees.Add(new Tree(new Vector2(x, y), _rng));
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
            int logs = _buildings.Sum(b => b.StoredLogs);
            int planks = _buildings.Sum(b => b.StoredPlanks);
            string text = $"BERRIES: {berries}   LOGS: {logs}   PLANKS: {planks}";
            _sb.DrawString(_font, text, new Vector2(35, 8), Color.White);
        }

        private static string JobLabel(JobType job)
        {
            return job switch
            {
                JobType.None => "IDLE",
                JobType.HarvestBerries => "BERRY",
                JobType.ChopTree => "CHOP",
                JobType.HaulLog => "HAUL",
                JobType.CarryLogToCarpenter => "CARRY",
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

        private void DrawBush(BerryBush b)
        {
            int baseX = (int)MathF.Round(b.Pos.X);
            int baseY = (int)MathF.Round(b.Pos.Y);
            foreach (var p in b.Shape)
                _sb.Draw(_px, new Rectangle(baseX + p.X, baseY + p.Y, 1, 1), new Color(0, 120, 0));
            for (int i = 0; i < b.Berries && i < b.BerryPixels.Length; i++)
            {
                var off = b.BerryPixels[i];
                _sb.Draw(_px, new Rectangle(baseX + off.X, baseY + off.Y, 1, 1), Color.Red);
            }
        }

        private void DrawBuilding(Building b)
        {
            var pos = b.Pos;
            switch (b.Kind)
            {
                case BuildingType.StockpileHut:
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
                    break;
                case BuildingType.CarpenterHut:
                    _sb.Draw(_px, new Rectangle((int)pos.X - 1, (int)pos.Y - 1, 3, 3), Color.Sienna);
                    _sb.Draw(_px, new Rectangle((int)pos.X, (int)pos.Y - 2, 1, 1), Color.Peru);
                    for (int i = 0; i < b.StoredLogs; i++)
                    {
                        int y = (int)pos.Y - 3 - i;
                        _sb.Draw(_px, new Rectangle((int)pos.X - 2, y, 1, 1), Color.SaddleBrown);
                    }
                    for (int i = 0; i < b.StoredPlanks; i++)
                    {
                        int y = (int)pos.Y - 3 - i;
                        _sb.Draw(_px, new Rectangle((int)pos.X + 2, y, 1, 1), Color.BurlyWood);
                    }
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

        private void DrawLog(Log l)
        {
            int baseX = (int)MathF.Round(l.Pos.X);
            int baseY = (int)MathF.Round(l.Pos.Y);
            foreach (var p in l.Shape)
                _sb.Draw(_px, new Rectangle(baseX + p.X, baseY + p.Y, 1, 1), l.Color);
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
                foreach (var p in t.TrunkPixels)
                    _sb.Draw(_px, new Rectangle(baseX + p.X, baseY + p.Y, 1, 1), new Color(100, 70, 40));
            }
        }

        private void DrawTreeTop(Tree t)
        {
            if (t.IsStump) return;

            int baseX = (int)MathF.Round(t.Pos.X);
            int baseY = (int)MathF.Round(t.Pos.Y);
            foreach (var p in t.LeafPixels)
                _sb.Draw(_px, new Rectangle(baseX + p.X, baseY + p.Y, 1, 1), new Color(20, 110, 20));
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
