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

        List<Soldier> _soldiers = new();
        List<Pixel> _pixels = new();
        Random _rng = new();

        float _camX, _camY, _zoom = 3.5f;
        KeyboardState _prevKb;
        MouseState _prevMs;

        Ability _currentAbility = Ability.None;

        // ability toolbar
        Rectangle _abilityButtonRect;
        Rectangle[] _abilityOptionRects;
        readonly Ability[] _abilityOptions =
            { Ability.None, Ability.Fire, Ability.Telekinesis, Ability.Explosion };
        bool _abilityMenuOpen;

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
            _abilityButtonRect = new Rectangle(5, 5, 24, 24);
            _abilityOptionRects = new Rectangle[_abilityOptions.Length];
            for (int i = 0; i < _abilityOptionRects.Length; i++)
                _abilityOptionRects[i] =
                    new Rectangle(5, 31 + i * 26, 24, 24);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _sb = new SpriteBatch(GraphicsDevice);
            _px = new Texture2D(GraphicsDevice, 1, 1);
            _px.SetData(new[] { Color.White });

            SpawnWave(Faction.Friendly, 12);
            SpawnWave(Faction.Enemy, 12);

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

            if (Edge(kb, Keys.P)) SpawnWave(Faction.Enemy, 1);

            // ability switching (keyboard)
            if (Edge(kb, Keys.D1) || Edge(kb, Keys.NumPad1))
                _currentAbility = Ability.None;
            if (Edge(kb, Keys.D2) || Edge(kb, Keys.NumPad2))
                _currentAbility = Ability.Fire;
            if (Edge(kb, Keys.D3) || Edge(kb, Keys.NumPad3))
                _currentAbility = Ability.Telekinesis;
            if (Edge(kb, Keys.D4) || Edge(kb, Keys.NumPad4))
                _currentAbility = Ability.Explosion;

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

            if (_currentAbility == Ability.Fire)
            {
                bool mPress = ms.LeftButton == ButtonState.Pressed &&
                              _prevMs.LeftButton == ButtonState.Released;
                if (mPress)
                {
                    float minD = Constants.PICKUP_RADIUS;
                    for (int i = _soldiers.Count - 1; i >= 0; i--)
                    {
                        var e = _soldiers[i];
                        for (int p = -2; p <= 2; p++)
                        {
                            float d = Vector2.Distance(e.GetPartPos(p), mworld);
                            if (d < minD)
                            {
                                e.IsBurning = true;
                                e.BurnTimer = Constants.BURN_DURATION;
                                _soldiers[i] = e;
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
                    _soldiers, _pixels
                );
            }
            else if (_currentAbility == Ability.Explosion)
            {
                bool mPress = ms.LeftButton == ButtonState.Pressed &&
                              _prevMs.LeftButton == ButtonState.Released;
                if (mPress)
                {
                    TriggerExplosion(mworld);
                }
            }
            else
            {
                _dragging = false;
            }

            PhysicsSystem.SimulateAll(_soldiers, _pixels, dt);
            PhysicsSystem.UpdatePixels(_pixels, dt);
            CombatSystem.ResolveCombat(_soldiers, _pixels, dt);

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

            // --- arena border ---
            int t = 2;
            _sb.Draw(_px, new Rectangle(Constants.ARENA_LEFT, Constants.ARENA_TOP, Constants.ARENA_RIGHT - Constants.ARENA_LEFT, t), Color.Black);
            _sb.Draw(_px, new Rectangle(Constants.ARENA_LEFT, Constants.ARENA_BOTTOM, Constants.ARENA_RIGHT - Constants.ARENA_LEFT, t), Color.Black);
            _sb.Draw(_px, new Rectangle(Constants.ARENA_LEFT, Constants.ARENA_TOP, t, Constants.ARENA_BOTTOM - Constants.ARENA_TOP), Color.Black);
            _sb.Draw(_px, new Rectangle(Constants.ARENA_RIGHT - t, Constants.ARENA_TOP, t, Constants.ARENA_BOTTOM - Constants.ARENA_TOP), Color.Black);

            // --- debris ---
            foreach (var p in _pixels)
                _sb.Draw(_px, p.Bounds, p.Col);

            // --- soldiers/entities ---
            foreach (var e in _soldiers.OrderBy(e => e.z))
            {
                // ---- SHADOW ----
                float stickLen = Constants.ENEMY_H * Constants.PART_LEN;
                int shLen = (int)MathF.Round(MathF.Abs(MathF.Sin(e.Angle)) * stickLen) + Constants.ENEMY_W;
                int shThick = 2;

                // find the lowest point of the ragdoll in world space
                int halfSeg = Constants.ENEMY_H / 2;
                float bottomY = float.MinValue;
                for (int part = -halfSeg; part < halfSeg; part++)
                    bottomY = MathF.Max(bottomY, e.GetPartPos(part).Y);

                float shY = bottomY + 1f; // slightly below the lowest segment

                var shRect = new Rectangle(
                    (int)MathF.Round(e.Pos.X - shLen / 2f),
                    (int)MathF.Round(shY),
                    shLen, shThick
                );
                _sb.Draw(_px, shRect, new Color(0, 0, 0, 100));

                // ---- BODY: Each segment as pixels ----
                int half = Constants.ENEMY_H / 2;
                for (int part = -half; part < half; part++)
                {
                    var segPos = e.GetPartPos(part);
                    segPos.Y -= e.z;

                    // Determine color for this part
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

                    if (e.State == SoldierState.Dead)
                    {
                        float decomp = MathF.Min(1f, e.DecompTimer / Constants.DECOMP_DURATION);
                        var pale = Color.Lerp(c, Color.LightGray, 0.5f);
                        var purple = new Color(60, 0, 80);
                        var bone = new Color(245, 245, 235);
                        c = decomp < 0.5f
                            ? Color.Lerp(pale, purple, decomp * 2f)
                            : Color.Lerp(purple, bone, (decomp - 0.5f) * 2f);
                    }
                    else if (e.State == SoldierState.Ragdoll)
                    {
                        c = Color.Lerp(c, Color.LightGray, 0.5f);
                    }

                    // Pixel-by-pixel for a 2x1 "block", rotated in world space
                    float angle = e.Angle;
                    float cos = MathF.Cos(angle);
                    float sin = MathF.Sin(angle);

                    if (MathF.Abs(e.AngularVel) > 0.01f)
                        DrawFatSegment(segPos, angle,
                            Constants.ENEMY_W,
                            Constants.PART_LEN,
                            c);

                    for (int dx = 0; dx < Constants.ENEMY_W; dx++)
                        for (int dy = 0; dy < (int)Constants.PART_LEN; dy++)
                        {
                            // Local offset, so body is centered on segPos
                            float localX = dx - (Constants.ENEMY_W * 0.5f - 0.5f);
                            float localY = dy - (Constants.PART_LEN * 0.5f - 0.5f);

                            // Apply rotation
                            float x = segPos.X + localX * cos - localY * sin;
                            float y = segPos.Y + localX * sin + localY * cos;

                            // skip pixels as corpse decomposes
                            if (e.State == SoldierState.Dead)
                            {
                                float decomp = MathF.Min(1f, e.DecompTimer / Constants.DECOMP_DURATION);
                                if (decomp > 0.5f)
                                {
                                    float skipChance = MathF.Min((decomp - 0.5f) * 2f, 0.9f);
                                    int hx = (int)MathF.Round(x), hy = (int)MathF.Round(y);
                                    int hash = (hx * 73856093) ^ (hy * 19349663) ^ part;
                                    double r = ((hash & 0x7fffffff) / (double)int.MaxValue);
                                    if (r < skipChance)
                                        continue;
                                }
                            }

                            _sb.Draw(_px, new Rectangle((int)MathF.Round(x), (int)MathF.Round(y), 1, 1), c);

                            // Optionally: Burning glow behind pixels
                            if (e.IsBurning && _rng.NextDouble() < 0.03)
                            {
                                Color[] firePal = { Color.OrangeRed, Color.Orange, Color.Yellow, new Color(255, 100, 0) };
                                Color glowCol = firePal[_rng.Next(firePal.Length)];
                                var glow = new Rectangle((int)MathF.Round(x) + _rng.Next(-1, 2), (int)MathF.Round(y) + _rng.Next(-1, 2), 1, 1);
                                _sb.Draw(_px, glow, new Color(glowCol, 90));
                            }
                        }
                }

                // draw 1px hands at the sides of the body
                {
                    var bodyPos = e.GetPartPos(-1);
                    bodyPos.Y -= e.z;
                    float angle = e.Angle;
                    float sideX = MathF.Cos(angle);
                    float sideY = MathF.Sin(angle);
                    float handOffset = Constants.ENEMY_W * 0.5f + 0.5f;

                    // Left hand
                    float lx = bodyPos.X - sideX * handOffset;
                    float ly = bodyPos.Y - sideY * handOffset;
                    Color handCol = Constants.HAND_COLOR;
                    if (e.State == SoldierState.Dead)
                    {
                        float decomp = MathF.Min(1f, e.DecompTimer / Constants.DECOMP_DURATION);
                        var pale = Color.Lerp(handCol, Color.LightGray, 0.5f);
                        var purple = new Color(60, 0, 80);
                        var bone = new Color(245, 245, 235);
                        handCol = decomp < 0.5f
                            ? Color.Lerp(pale, purple, decomp * 2f)
                            : Color.Lerp(purple, bone, (decomp - 0.5f) * 2f);
                    }
                    else if (e.State == SoldierState.Ragdoll)
                    {
                        handCol = Color.Lerp(handCol, Color.LightGray, 0.5f);
                    }
                    _sb.Draw(_px, new Rectangle((int)MathF.Round(lx), (int)MathF.Round(ly), 1, 1), handCol);

                    // Right hand
                    float rx = bodyPos.X + sideX * handOffset;
                    float ry = bodyPos.Y + sideY * handOffset;
                    _sb.Draw(_px, new Rectangle((int)MathF.Round(rx), (int)MathF.Round(ry), 1, 1), handCol);
                }

                // ---- FLAME EFFECT on burning ----
                if (e.IsBurning)
                {
                    DrawFlame(e);
                }
            }

            _sb.End();

            // --- UI ---
            _sb.Begin();
            DrawAbilityToolbar();
            _sb.End();
            base.Draw(gt);
        }


        private void SpawnWave(Faction side, int count)
        {
            float x0 = side == Faction.Friendly ? Constants.ARENA_LEFT + 2 : Constants.ARENA_RIGHT - 30;
            float x1 = side == Faction.Friendly ? Constants.ARENA_LEFT + 30 : Constants.ARENA_RIGHT - 2;
            for (int i = 0; i < count; i++)
            {
                var x = _rng.NextFloat(x0, x1);
                var y = _rng.NextFloat(Constants.ARENA_TOP + 2, Constants.ARENA_BOTTOM - 2);
                var pal = side == Faction.Friendly ? Soldier.FRIENDLY_SHIRTS : Soldier.ENEMY_SHIRTS;
                var shirt = pal[_rng.Next(pal.Length)];
                _soldiers.Add(new Soldier(new Vector2(x, y), side, shirt));
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

        private void DrawFlame(Soldier e)
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

        private void TriggerExplosion(Vector2 pos)
        {
            // push soldiers away
            for (int i = 0; i < _soldiers.Count; i++)
            {
                var s = _soldiers[i];
                Vector2 dir = s.Pos - pos;
                float dist = dir.Length();
                if (dist > Constants.EXPLOSION_RADIUS)
                    continue;
                if (dist > 0f) dir /= dist; else dir = new Vector2(0f, -1f);
                float strength = (1f - dist / Constants.EXPLOSION_RADIUS) * Constants.EXPLOSION_PUSH;
                s.Vel += dir * strength;
                s.vz += Constants.EXPLOSION_UPWARD;
                s.State = SoldierState.Launched;
                _soldiers[i] = s;
            }

            // visual particles
            Color[] smokePal = { Color.OrangeRed, Color.Orange, Color.Yellow, Color.Gray };
            for (int i = 0; i < Constants.EXPLOSION_PARTICLES; i++)
            {
                float ang = MathHelper.ToRadians(_rng.Next(360));
                float spd = _rng.NextFloat(10f, Constants.EXPLOSION_PUSH);
                var vel = new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * spd;
                var col = smokePal[_rng.Next(smokePal.Length)];
                _pixels.Add(new Pixel(
                    pos,
                    vel,
                    col,
                    0f,
                    _rng.NextFloat(Constants.DEBRIS_LIFETIME_MIN,
                                   Constants.DEBRIS_LIFETIME_MAX)));
            }
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
