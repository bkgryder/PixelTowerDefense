using Microsoft.Xna.Framework;
﻿namespace PixelTowerDefense.Utils
{
    public static class Constants
    {
        public const int ENEMY_W = 2, ENEMY_H = 10;
        public const float PART_LEN = 1.0f;

        public const int ARENA_LEFT = 0, ARENA_RIGHT = 200;
        public const int ARENA_TOP = 0, ARENA_BOTTOM = 200;

        public const float WANDER_SPEED = 10f;
        public const float FRICTION = 3f;
        public const float EXPLODE_VZ_THRESHOLD = 85f;
        public const float STUN_VZ_THRESHOLD = 8f;
        public const float STUN_TIME = 1.2f;
        public const float Z_GRAVITY = 120f;
        public const float PICKUP_RADIUS = 5f;
        public const float THROW_VZ_SCALE = 0.65f;
        public const float INITIAL_Z = 2f;
        public const float THROW_SENSITIVITY = .25f;
        public const float GRAB_Z = 4f;
        public const float VOMIT_SPIN_THRESHOLD = 30f;
        public const float ANGULAR_DAMPING = 1.5f;
        public const float DRAG_SPRING = 16f;
        public const float DRAG_DAMPING = 6f;
        public const float ENEMY_MAX_HEALTH = 100f;
        public const float BURN_DURATION = 5f;    // seconds of burning
        public const float BURN_DPS = 20f;   // damage/sec
        public const float FIRE_PARTICLE_RATE = 0.05f; // spawn rate

        public const float BURNING_SPEED_MULT = 3f;
        public const float BURN_WANDER_TIME_MIN = 0.1f;
        public const float BURN_WANDER_TIME_MAX = 0.3f;

        /// <summary>Minimum debris launch speed</summary>
        public const float EXPLOSION_FORCE_MIN = 2f;
        /// <summary>Maximum debris launch speed</summary>
        public const float EXPLOSION_FORCE_MAX = 45f;
        /// <summary>Minimum ash drift speed</summary>
        public const float ASH_FORCE_MIN = 1f;
        /// <summary>Maximum ash drift speed</summary>
        public const float ASH_FORCE_MAX = 20f;
        /// <summary>Minimum ash pixels per body part</summary>
        public const int ASH_PARTICLES_MIN = 1;
        /// <summary>Maximum ash pixels per body part</summary>
        public const int ASH_PARTICLES_MAX = 2;
        /// <summary>Amount of friction to apply to debris</summary>
        public const float SMOKE_PARTICLE_RATE = 0.02f;
        public const float SMOKE_LIFETIME = 1.5f;
        public const float SMOKE_FORCE_MIN = 30f;
        public const float SMOKE_FORCE_MAX = 75f;
        public const int   DEATH_SMOKE_COUNT = 20;
        public const float DEBRIS_FRICTION = 2.5f;  // much lower than your 3f

        // Debris fade-out
        public const float DEBRIS_LIFETIME_MIN = 6f;
        public const float DEBRIS_LIFETIME_MAX = 10f;
        public const float EMBER_LIFETIME = 1.5f;

        // Maximum number of active debris pixels
        public const int MAX_DEBRIS = 1000;

        // Explosion ability
        public const float EXPLOSION_RADIUS = 25f;
        public const float EXPLOSION_PUSH = 70f;
        public const float EXPLOSION_UPWARD = 60f;
        public const int   EXPLOSION_PARTICLES = 30;

        public const float SEEK_RADIUS = 28f;
        public const float TOUCH_RANGE = 1.8f;
        public const float MELEE_DMG = 8f;
        public const float ATTACK_WINDUP = 0.35f;

        public const float MELEE_KNOCKBACK = 12f;
        public const float MELEE_KNOCKBACK_UPWARD = 50f;

        // How long corpses take to fully decompose
        public const float DECOMP_DURATION = 12f;

        // Palette for corpse decomposition
        public static readonly Color DECOMP_PURPLE = new Color(60, 0, 80);
        public static readonly Color BONE_COLOR = new Color(245, 245, 235);

        public static readonly Color HAND_COLOR = new Color(255, 219, 172);

        // Hunger & resources
        public const float HUNGER_MAX = 10f;
        public const float HUNGER_RATE = 1f;
        public const float HUNGER_THRESHOLD = 5f;
        public const float HARVEST_RANGE = 2f;
        public const int BUSH_BERRIES = 5;
        public const float BUSH_REGROW_INTERVAL = .25f;

        // Precipitate ability
        public const float PRECIPITATE_DROP_SPEED = 130f;
        public const float PRECIPITATE_FADE_SPEED = .1f;
        public const float PRECIPITATE_CLOUD_OFFSET_Y = -50f;
        public const float PRECIPITATE_CLOUD_JITTER = 10f;
        public const float PRECIPITATE_CLOUD_LERP = 8f;

        // Mana system
        public const float MANA_MAX = 100f;
        public const float MANA_REGEN = 20f; // mana per second
        public const float FIRE_COST = 20f;
        public const float EXPLOSION_COST = 40f;
        public const float TELEKINESIS_DRAIN = 10f; // per second
        public const float PRECIPITATE_DRAIN = 5f;  // per second

    }
}
