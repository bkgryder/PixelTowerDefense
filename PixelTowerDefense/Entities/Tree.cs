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
        public float BurnProgress;

        /// <summary>Remove tree immediately once fallen.</summary>
        public bool RemoveWhenFallen;

        // appearance
        public float ShadowRadius;

        // cached final size parameters
        private int _maxHeight;
        private int _baseWidth;
        private int _leafRadius;
        private float _lean;

        public TreeSpecies Species;
        public TreeGenotype Gen;
        public Color TrunkBase, TrunkTip, LeafA, LeafB;

        private float _branchSplitProb;
        private int _branchLenMin, _branchLenMax;
        private float _leafKeepProb;
        private float _ellipseRatio;

        public int MaxHeight => _maxHeight; // expose for rendering gradient

        public Tree(Vector2 pos, System.Random rng) : this(pos, rng, TreeLibrary.Oak, 0)
        {
        }

        public Tree(Vector2 pos, System.Random rng, TreeArchetype arch, int worldSeed) : this()
        {
            Pos = pos;

            // genotype
            int seed = Hash(worldSeed, pos);
            var grng = new Random(seed);
            Gen = new TreeGenotype
            {
                Seed = seed,
                HeightBias = RandSpan(grng),
                RadiusBias = RandSpan(grng),
                LeanBias = RandSpan(grng),
                BranchProbBias = RandSpan(grng),
                LeafDensityBias = RandSpan(grng),
                HueShift = grng.Next(-10, 11)
            };

            Species = arch.Species;

            _maxHeight = (int)MathF.Round(MathHelper.Lerp(arch.HeightMin, arch.HeightMax, 0.5f + 0.5f * Gen.HeightBias));
            _baseWidth = (int)MathF.Round(MathHelper.Lerp(arch.BaseWidthMin, arch.BaseWidthMax, 0.5f));
            _leafRadius = (int)MathF.Round(MathHelper.Lerp(arch.LeafRadiusMin, arch.LeafRadiusMax, 0.5f + 0.5f * Gen.RadiusBias));
            _lean = MathHelper.Lerp(arch.LeanMin, arch.LeanMax, 0.5f + 0.5f * Gen.LeanBias);

            _branchSplitProb = Math.Clamp(arch.BranchSplitProb * (1f + 0.4f * Gen.BranchProbBias), 0f, 1f);
            _branchLenMin = arch.BranchLenRange.X;
            _branchLenMax = arch.BranchLenRange.Y;
            _leafKeepProb = Math.Clamp(arch.LeafKeepProb * (1f + 0.3f * Gen.LeafDensityBias), 0.1f, 0.99f);
            _ellipseRatio = arch.CanopyEllipseRatio;

            float trunkLight = 1f + RandSpan(grng) * 0.05f;
            float leafLight = 1f + RandSpan(grng) * 0.05f;

            Color baseCol = arch.TrunkBase;
            Color tipCol = arch.TrunkTip;
            if (arch.TrunkVariations != null && arch.TrunkVariations.Length > 0)
            {
                var pair = arch.TrunkVariations[grng.Next(arch.TrunkVariations.Length)];
                baseCol = pair.Base;
                tipCol = pair.Tip;
            }

            TrunkBase = Utils.ColorUtils.AdjustColor(
                baseCol, Gen.HueShift, 1f, trunkLight);
            TrunkTip = Utils.ColorUtils.AdjustColor(
                tipCol, Gen.HueShift, 1f, trunkLight);
            LeafA = Utils.ColorUtils.AdjustColor(
                arch.LeafA, Gen.HueShift, 1f, leafLight);
            LeafB = Utils.ColorUtils.AdjustColor(
                arch.LeafB, Gen.HueShift, 1f, leafLight);

            // existing lifecycle init
            TrunkPixels = Array.Empty<Point>();
            LeafPixels = Array.Empty<Point>();

            Age = 0f;
            GrowthDuration = Utils.RandEx.NextFloat(rng, Utils.Constants.TREE_GROW_TIME_MIN, Utils.Constants.TREE_GROW_TIME_MAX);
            DeathAge = GrowthDuration + Utils.RandEx.NextFloat(rng, Utils.Constants.TREE_LIFESPAN_MIN, Utils.Constants.TREE_LIFESPAN_MAX);
            Seed = grng.Next();

            CollisionRadius = _baseWidth + 0.5f;
            Health = Utils.Constants.TREE_HEALTH;
            IsStump = false; ReservedBy = null;
            IsDead = false; PaleTimer = 0f; FallDelay = 0f; FallTimer = 0f; Fallen = false;
            DecompTimer = 0f; FallDir = rng.NextDouble() < 0.5 ? -1 : 1;
            LeafTimer = 0f; IsBurning = false; BurnTimer = 0f; BurnProgress = 0f;
            RemoveWhenFallen = false;

            GenerateShape(0f);
        }

        private static int Hash(int worldSeed, Vector2 p)
        {
            unchecked
            {
                int h = worldSeed;
                h = h * 16777619 ^ (int)MathF.Round(p.X * 1000f);
                h = h * 16777619 ^ (int)MathF.Round(p.Y * 1000f);
                return h;
            }
        }
        private static float RandSpan(Random r) => (float)r.NextDouble() * 2f - 1f;


        private void GenerateShape(float factor)
        {
            var rng = new Random(Seed);
            factor = Math.Clamp(factor, 0f, 1f);
            factor = MathF.Max(0.1f, factor);

            int height = Math.Max(1, (int)MathF.Round(_maxHeight * factor));
            int baseWidth = Math.Max(1, (int)MathF.Round(_baseWidth * factor));
            float lean = _lean * factor;

            var trunk = new List<Point>();
            var leaves = new List<Point>();
            for (int i = 0; i < height; i++)
            {
                float t = i / (float)height;
                int width = (int)MathF.Max(0, MathF.Round(baseWidth * (1.8f - t)));
                int offset = (int)MathF.Round(lean * i);
                for (int x = -width; x <= width; x++)
                    trunk.Add(new Point(offset + x, -i));

                if (i > height / 3 && i < height - 2 && rng.NextDouble() < _branchSplitProb)
                {
                    int dir = rng.NextDouble() < 0.5 ? -1 : 1;
                    int branchLen = rng.Next(_branchLenMin, _branchLenMax + 1);
                    for (int j = 1; j <= branchLen; j++)
                    {
                        int bx = offset + dir * (width + j);
                        int by = -i - j / 3;
                        trunk.Add(new Point(bx, by));
                        if (factor >= 0.3f && j >= branchLen / 2)
                        {
                            int brad = Math.Max(1, (int)MathF.Round(_leafRadius * factor * 0.3f));
                            for (int ly = -brad; ly <= brad; ly++)
                                for (int lx = -brad; lx <= brad; lx++)
                                    if (lx * lx + ly * ly <= brad * brad && rng.NextDouble() < _leafKeepProb)
                                        leaves.Add(new Point(bx + lx, by + ly));
                        }
                    }
                }

            }
            TrunkPixels = trunk.ToArray();

            if (factor >= 0.3f)
            {
                int radius = Math.Max(1, (int)MathF.Round(_leafRadius * factor));
                int topOffset = (int)MathF.Round(lean * height);
                for (int y = -radius; y <= radius; y++)
                {
                    for (int x = -radius; x <= radius; x++)
                    {
                        if ((x * x) + (int)(y * y) <= radius * radius && rng.NextDouble() < _leafKeepProb)
                            leaves.Add(new Point(topOffset + x, -height + y));
                    }
                }

            }
            LeafPixels = leaves.ToArray();

            CollisionRadius = baseWidth + 0.5f;
            ShadowRadius = MathF.Max(baseWidth + 1, _leafRadius * factor);
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

        public Rectangle Bounds
        {
            get
            {
                int r = (int)MathF.Ceiling(CollisionRadius);
                int x = (int)MathF.Round(Pos.X) - r;
                int y = (int)MathF.Round(Pos.Y) - r;
                return new Rectangle(x, y, r * 2, r * 2);
            }
        }
    }
}
