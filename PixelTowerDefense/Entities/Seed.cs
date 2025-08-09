using System;
using Microsoft.Xna.Framework;

namespace PixelTowerDefense.Entities
{
    public enum SeedKind
    {
        Tree,
        Bush
    }

    public struct Seed
    {
        public Vector2 Pos;
        public Vector2 Vel;
        public float z;
        public float vz;
        public float Age;
        public float GrowTime;
        public SeedKind Kind;

        public Seed(Vector2 pos, Vector2 vel, float vz0, System.Random rng, SeedKind kind = SeedKind.Tree)
        {
            Pos = pos;
            Vel = vel;
            z = 0f;
            vz = vz0;
            Age = 0f;
            GrowTime = Utils.RandEx.NextFloat(rng,
                Utils.Constants.SEED_GROW_TIME_MIN,
                Utils.Constants.SEED_GROW_TIME_MAX);
            Kind = kind;
        }

        public Rectangle Bounds => new Rectangle(
            (int)MathF.Round(Pos.X),
            (int)MathF.Round(Pos.Y),
            1,
            1
        );
    }
}
