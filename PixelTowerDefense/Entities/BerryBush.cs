using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace PixelTowerDefense.Entities
{
    public struct BerryBush
    {
        public Vector2 Pos;
        public int Berries;
        public float RegrowTimer;
        public Point[] TrunkPixels;
        public Point[] LeafPixels;
        public Point[] BerryPixels;
        public int? ReservedBy;

        // lifecycle
        public float Age;
        public float GrowthDuration;
        public float DeathAge;
        public int Seed;
        public bool IsDead;
        public bool IsBurning;
        public float BurnTimer;

        // cached final size parameters
        private int _maxHeight;
        private int _baseWidth;
        private int _leafRadius;
        private float _lean;

        public BerryBush(Vector2 pos, System.Random rng, bool mature = true)
        {
            Pos = pos;
            RegrowTimer = 0f;
            ReservedBy = null;

            Age = mature ? 1f : 0f; // temp set, overwritten below
            GrowthDuration = Utils.RandEx.NextFloat(rng,
                Utils.Constants.BUSH_GROW_TIME_MIN,
                Utils.Constants.BUSH_GROW_TIME_MAX);
            DeathAge = GrowthDuration + Utils.RandEx.NextFloat(rng,
                Utils.Constants.BUSH_LIFESPAN_MIN,
                Utils.Constants.BUSH_LIFESPAN_MAX);
            Seed = rng.Next();
            IsDead = false;
            IsBurning = false;
            BurnTimer = 0f;

            _maxHeight = rng.Next(3, 6);
            _baseWidth = 1;
            _leafRadius = rng.Next(2, 4);
            _lean = Utils.RandEx.NextFloat(rng, -0.2f, 0.2f);

            Berries = mature ? Utils.Constants.BUSH_BERRIES : 0;

            TrunkPixels = Array.Empty<Point>();
            LeafPixels = Array.Empty<Point>();
            BerryPixels = Array.Empty<Point>();

            GenerateShape(mature ? 1f : 0.1f);
            Age = mature ? GrowthDuration : 0f;
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
                int offset = (int)MathF.Round(lean * i);
                for (int x = -baseWidth; x <= baseWidth; x++)
                    trunk.Add(new Point(offset + x, -i));

                if (i > height / 3 && rng.NextDouble() < 0.3)
                {
                    int dir = rng.NextDouble() < 0.5 ? -1 : 1;
                    int branchLen = rng.Next(2, 4);
                    for (int j = 1; j <= branchLen; j++)
                        trunk.Add(new Point(offset + dir * (baseWidth + j), -i - j / 2));
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

            var berryList = new List<Point>();
            for (int i = 0; i < Utils.Constants.BUSH_BERRIES && leaves.Count > 0; i++)
            {
                int idx = rng.Next(leaves.Count);
                berryList.Add(leaves[idx]);
                leaves.RemoveAt(idx);
            }
            BerryPixels = berryList.ToArray();
        }

        public void Grow(float dt, bool raining)
        {
            if (!IsBurning)
            {
                Age += dt;
                if (!IsDead && Age >= GrowthDuration)
                {
                    if (Age >= DeathAge)
                        IsDead = true;
                }

                float factor = System.Math.Clamp(Age / GrowthDuration, 0f, 1f);
                GenerateShape(factor);

                if (!IsDead && Age >= GrowthDuration && Berries < Utils.Constants.BUSH_BERRIES)
                {
                    float interval = Utils.Constants.BUSH_REGROW_INTERVAL;
                    if (!raining)
                        interval *= Utils.Constants.BUSH_REGROW_CLEAR_MULT;
                    RegrowTimer += dt;
                    if (RegrowTimer >= interval)
                    {
                        RegrowTimer = 0f;
                        Berries++;
                    }
                }
                else
                {
                    RegrowTimer = 0f;
                }
            }
            else
            {
                BurnTimer -= dt;
            }
        }
    }
}
