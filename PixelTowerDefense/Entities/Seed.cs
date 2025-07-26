using Microsoft.Xna.Framework;

namespace PixelTowerDefense.Entities
{
    public struct Seed
    {
        public Vector2 Pos;
        public Vector2 Vel;
        public float Age;
        public float GrowTime;

        public Seed(Vector2 pos, Vector2 vel, System.Random rng)
        {
            Pos = pos;
            Vel = vel;
            Age = 0f;
            GrowTime = Utils.RandEx.NextFloat(rng,
                Utils.Constants.SEED_GROW_TIME_MIN,
                Utils.Constants.SEED_GROW_TIME_MAX);
        }
    }
}
