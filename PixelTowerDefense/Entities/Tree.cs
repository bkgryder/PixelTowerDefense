using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace PixelTowerDefense.Entities
{
    public struct Tree
    {
        public Vector2 Pos;
        public Point[] TrunkPixels;
        public Point[] LeafPixels;
        public float CollisionRadius;
        public int Health;
        public bool IsStump;
        public int? ReservedBy;

        // growth
        public float Age;
        public float GrowthDuration;
        public float DeathAge;
        public int Seed;

        // lifecycle
        public bool IsDead;
        public float PaleTimer;
        public float FallDelay;
        public float FallTimer;
        public bool Fallen;
        public float DecompTimer;
        public int FallDir;
        public float LeafTimer;
        public bool IsBurning;
        public float BurnTimer;

        // cached final size parameters
        private int _maxHeight;
        private int _baseWidth;
        private int _leafRadius;
        private float _lean;


        public Tree(Vector2 pos, System.Random rng)
        {
            Pos = pos;

            TrunkPixels = Array.Empty<Point>();
            LeafPixels = Array.Empty<Point>();

            Age = 0f;
            GrowthDuration = Utils.RandEx.NextFloat(rng, Utils.Constants.TREE_GROW_TIME_MIN,
                                                   Utils.Constants.TREE_GROW_TIME_MAX);
            DeathAge = GrowthDuration + Utils.RandEx.NextFloat(rng,
                                                              Utils.Constants.TREE_LIFESPAN_MIN,
                                                              Utils.Constants.TREE_LIFESPAN_MAX);
            Seed = rng.Next();

            _maxHeight = rng.Next(20, 28);
            _baseWidth = rng.Next(1, 2); // half-width of the trunk base
            _leafRadius = rng.Next(4, 7);
            _lean = Utils.RandEx.NextFloat(rng, -0.3f, 0.3f);

            CollisionRadius = _baseWidth + 0.5f;
            Health = Utils.Constants.TREE_HEALTH;
            IsStump = false;
            ReservedBy = null;

            IsDead = false;
            PaleTimer = 0f;
            FallDelay = 0f;
            FallTimer = 0f;
            Fallen = false;
            DecompTimer = 0f;
            FallDir = rng.NextDouble() < 0.5 ? -1 : 1;
            LeafTimer = 0f;
            IsBurning = false;
            BurnTimer = 0f;

            GenerateShape(0f);
        }

        private void GenerateShape(float factor)
        {
            var rng = new Random(Seed);
            factor = Math.Clamp(factor, 0f, 1f);
            factor = MathF.Max(0.1f, factor);

            int height = Math.Max(1, (int)MathF.Round(_maxHeight * factor));
            int baseWidth = Math.Max(1, (int)MathF.Round(_baseWidth * factor));
            float lean = _lean * factor;

            var trunk = new List<Point>();
            for (int i = 0; i < height; i++)
            {
                float t = i / (float)height;
                int width = (int)MathF.Max(0, MathF.Round(baseWidth * (1.8f - t)));
                int offset = (int)MathF.Round(lean * i);
                for (int x = -width; x <= width; x++)
                    trunk.Add(new Point(offset + x, -i));

                if (i > height / 3 && i < height - 2 && rng.NextDouble() < 0.15)
                {
                    int dir = rng.NextDouble() < 0.5 ? -1 : 1;
                    int branchLen = rng.Next(5, 10);
                    for (int j = 1; j <= branchLen; j++)
                        trunk.Add(new Point(offset + dir * (width + j), -i - j / 2));
                }
            }
            TrunkPixels = trunk.ToArray();

            var leaves = new List<Point>();
            if (factor >= 0.3f)
            {
                int radius = Math.Max(1, (int)MathF.Round(_leafRadius * factor));
                int topOffset = (int)MathF.Round(lean * height);
                for (int y = -radius; y <= radius; y++)
                {
                    for (int x = -radius; x <= radius; x++)
                    {
                        int r = x * x + (y * 2) * (y * 2);
                        if (r <= radius * radius * 4 && rng.NextDouble() > 0.2)
                            leaves.Add(new Point(topOffset + x, -height + y));
                    }
                }
            }
            LeafPixels = leaves.ToArray();

            CollisionRadius = baseWidth + 0.5f;
        }

        public void Grow(float dt, Random rng, bool grow)
        {
            Age += dt;
            float factor = Math.Clamp(Age / GrowthDuration, 0f, 1f);
            if (grow && !IsDead)
                GenerateShape(factor);

            if (!IsDead && Age >= DeathAge)
            {
                IsDead = true;
                PaleTimer = 0f;
                LeafTimer = 0f;
                FallDelay = Utils.RandEx.NextFloat(rng,
                    Utils.Constants.TREE_FALL_DELAY_MIN,
                    Utils.Constants.TREE_FALL_DELAY_MAX);
            }

            if (IsDead)
            {
                PaleTimer += dt;
                LeafTimer += dt;
                if (LeafTimer >= Utils.Constants.LEAF_DISINTEGRATE_TIME)
                    LeafPixels = Array.Empty<Point>();
                if (!Fallen)
                {
                    if (FallDelay > 0f)
                        FallDelay -= dt;
                    else
                    {
                        FallTimer += dt;
                        if (FallTimer >= Utils.Constants.TREE_FALL_TIME)
                        {
                            FallTimer = Utils.Constants.TREE_FALL_TIME;
                            Fallen = true;
                        }
                    }
                }
                else
                {
                    DecompTimer += dt;
                }
            }
        }
    }
}
