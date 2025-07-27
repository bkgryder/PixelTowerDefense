using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace PixelTowerDefense.Entities
{
    public struct BerryBush
    {
        public Vector2 Pos;
        public int Berries;
        public float RegrowTimer;
        public Point[] Shape;
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

            Berries = mature ? Utils.Constants.BUSH_BERRIES : 0;

            Shape = System.Array.Empty<Point>();
            BerryPixels = System.Array.Empty<Point>();

            GenerateShape(mature ? 1f : 0.1f);
            Age = mature ? GrowthDuration : 0f;
        }

        private void GenerateShape(float factor)
        {
            var rng = new System.Random(Seed);
            factor = System.Math.Clamp(factor, 0f, 1f);
            factor = System.MathF.Max(0.1f, factor);

            var shapeList = new List<Point>();
            for (int y = -6; y < 6; y++)
                for (int x = -6; x < 6; x++)
                {
                    float r2 = x * x + y * y;
                    if (r2 <= 32 * factor && rng.NextDouble() > 0.1)
                        shapeList.Add(new Point(x, y));
                }
            Shape = shapeList.ToArray();
            var berryList = new List<Point>();
            for (int i = 0; i < Utils.Constants.BUSH_BERRIES && shapeList.Count > 0; i++)
            {
                int idx = rng.Next(shapeList.Count);
                berryList.Add(shapeList[idx]);
                shapeList.RemoveAt(idx);
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
