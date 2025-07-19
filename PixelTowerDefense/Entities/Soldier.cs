using System;
using Microsoft.Xna.Framework;
using PixelTowerDefense.Utils;

namespace PixelTowerDefense.Entities
{
    public enum SoldierState { Idle, Charging, Melee, Launched, Stunned }

    public enum Faction { Friendly, Enemy }

    public struct CombatStats
    {
        public float Health;
        public float AttackCooldown;
    }

    public struct Soldier
    {
        public static readonly Color[] FRIENDLY_SHIRTS =
        {
            new Color(60,130,60), new Color(90,150,90)
        };
        public static readonly Color[] ENEMY_SHIRTS =
        {
            new Color(215,175,130), new Color(190,155,110)
        };

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
        public SoldierState State;

        public Faction Side;
        public CombatStats Combat;
        public bool IsBurning;
        public float BurnTimer;

        public bool Alive => Combat.Health > 0f;

        public Soldier(Vector2 spawn, Faction side, Color shirt)
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
            State = SoldierState.Idle;
            Side = side;
            Combat = new CombatStats { Health = Constants.ENEMY_MAX_HEALTH, AttackCooldown = 0f };
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
