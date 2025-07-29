using System;
using Microsoft.Xna.Framework;
using PixelTowerDefense.Utils;

namespace PixelTowerDefense.Entities
{
    public enum MeepleState { Idle, Charging, Melee, Launched, Stunned, Ragdoll, Dead }

    public enum Faction { Friendly, Enemy }

    public struct Combatant
    {
        public float AttackCooldown;
    }

    public enum JobType
    {
        None,
        HarvestBerries,
        ChopTree,
        HaulWood,
        DepositResource
    }

    public struct Worker
    {
        public JobType CurrentJob;
        public int? TargetIdx;
    }

    public struct Meeple
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

        // rendering helper
        public float ShadowY;

        // stun
        public float StunTimer;

        // appearance / state
        public Color ShirtColor;
        public MeepleState State;

        public Faction Side;
        public bool IsBurning;
        public float BurnTimer;
        public float InWaterTime;

        // time spent decomposing once dead
        public float DecompTimer;

        // survival
        public float Hunger;

        public float Health;

        public int CarriedBerries;
        public int CarriedWood;
        public int CarriedWoodIdx;

        public string Name;

        // basic attributes
        public int Strength;
        public int Dexterity;
        public int Intellect;
        public int Grit;

        public Combatant? Combatant;
        public Worker? Worker;

        public bool Alive => Health > 0f && State != MeepleState.Dead && State != MeepleState.Ragdoll;

        public int CarryCapacity => Strength;
        public float MoveSpeed => Constants.WANDER_SPEED * (1f + Dexterity / 10f);

        public Meeple(Vector2 spawn, Faction side, Color shirt, float health = Constants.ENEMY_MAX_HEALTH)
        {
            Pos = spawn;
            Vel = Vector2.Zero;
            WanderTimer = 1f;
            Angle = 0f;
            AngularVel = 0f;
            z = 0f;
            vz = 0f;
            ShadowY = 0f;
            StunTimer = 0f;
            ShirtColor = shirt;
            State = MeepleState.Idle;
            Side = side;
            IsBurning = false;
            BurnTimer = 0f;
            InWaterTime = 0f;
            DecompTimer = 0f;
            Hunger = 0f;
            Health = health;
            CarriedBerries = 0;
            CarriedWood = 0;
            CarriedWoodIdx = -1;
            Name = string.Empty;
            Combatant = null;
            Worker = null;
        }

        public Meeple(Vector2 spawn, Faction side, Color shirt,
                      int strength, int dexterity, int intellect, int grit,
                      float health = Constants.ENEMY_MAX_HEALTH)
            : this(spawn, side, shirt, health)
        {
            Strength = strength;
            Dexterity = dexterity;
            Intellect = intellect;
            Grit = grit;
        }

        /// <summary>
        /// Get world position of a body segment.
        /// Segment indices range from -ENEMY_H/2 to ENEMY_H/2-1
        /// (head to feet).
        /// </summary>
        public Vector2 GetPartPos(int part)
        {
            float l = part * Constants.PART_LEN;
            return new Vector2(
                Pos.X + MathF.Sin(Angle) * l,
                Pos.Y + MathF.Cos(Angle) * l
            );
        }

        public static Meeple SpawnMeeple(Vector2 pos, Faction side, Color shirt, Random rng)
        {
            var m = new Meeple(pos, side, shirt);
            m.Name = NameGenerator.RandomName(rng);
            m.Strength = rng.Next(3, 11);
            m.Dexterity = rng.Next(3, 11);
            m.Intellect = rng.Next(3, 11);
            m.Grit = rng.Next(3, 11);
            m.Health = Constants.ENEMY_MAX_HEALTH + (m.Grit - 5) * 10f;
            return m;
        }
    }
}
