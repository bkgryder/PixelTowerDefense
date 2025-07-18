using System;
using Microsoft.Xna.Framework;
using PixelTowerDefense.Utils;

namespace PixelTowerDefense.Entities
{
    public enum EnemyState { Walking, Launched, Stunned }

    public struct Enemy
    {
        // planar
        public Vector2 Pos, Vel;
        public float WanderTimer;

        // rotation / ragdoll
        public float Angle, AngularVel;

        // vertical
        public float z, vz;

        // stun
        public float StunTimer;

        // appearance / state
        public Color ShirtColor;
        public EnemyState State;

        public float Health;
        public bool IsBurning;
        public float BurnTimer;

        public Enemy(Vector2 spawn, Color shirt)
        {
            Pos = spawn;
            Vel = Vector2.Zero;
            WanderTimer = 1f;
            Angle = 0f;
            AngularVel = 0f;
            z = 0f;
            vz = 0f;
            StunTimer = 0f;
            ShirtColor = shirt;
            State = EnemyState.Walking;
            Health = Constants.ENEMY_MAX_HEALTH;
            IsBurning = false;
            BurnTimer = 0f;

        }

        /// <summary>
        /// Get world‐pos of segment −2..+2 (head..foot)
        /// </summary>
        public Vector2 GetPartPos(int part)
        {
            // sin/cos so that when Angle=0 the chain is vertical
            float l = part * Constants.PART_LEN;
            return new Vector2(
                Pos.X + MathF.Sin(Angle) * l,
                Pos.Y + MathF.Cos(Angle) * l
            );
        }
    }
}
