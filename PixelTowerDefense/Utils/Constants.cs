namespace PixelTowerDefense.Utils
{
    public static class Constants
    {
        public const int ENEMY_W = 2, ENEMY_H = 10;
        public const float PART_LEN = 1.0f;

        public const int ARENA_LEFT = 0, ARENA_RIGHT = 200;
        public const int ARENA_TOP = 0, ARENA_BOTTOM = 200;

        public const float WANDER_SPEED = 10f;
        public const float FRICTION = 3f;
        public const float EXPLODE_VZ_THRESHOLD = 55f;
        public const float STUN_VZ_THRESHOLD = 5f;
        public const float STUN_TIME = 1.2f;
        public const float Z_GRAVITY = 120f;
        public const float PICKUP_RADIUS = 5f;
        public const float THROW_VZ_SCALE = 0.25f;
        public const float INITIAL_Z = 2f;
        public const float THROW_SENSITIVITY = .5f;
        public const float GRAB_Z = 4f;
        public const float VOMIT_SPIN_THRESHOLD = 30f;
        public const float ANGULAR_DAMPING = 1.5f;
        public const float DRAG_SPRING = 16f;
        public const float DRAG_DAMPING = 6f;
        public const float ENEMY_MAX_HEALTH = 100f;
        public const float BURN_DURATION = 5f;    // seconds of burning
        public const float BURN_DPS = 10f;   // damage/sec
        public const float FIRE_PARTICLE_RATE = 0.05f; // spawn rate

        /// <summary>Minimum debris launch speed</summary>
        public const float EXPLOSION_FORCE_MIN = 2f;
        /// <summary>Maximum debris launch speed</summary>
        public const float EXPLOSION_FORCE_MAX = 55f;
        /// <summary>Amount of friction to apply to debris</summary>
        public const float DEBRIS_FRICTION = 2.5f;  // much lower than your 3f
    }
}
