using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PixelTowerDefense.Entities
{
    public class Enemy
    {
        public Vector2 Pos, Vel;
        public float Angle, AngularVel;
        public float Dir, WanderTimer;
        public float MaxAirY;
        public Color ShirtColor;

        public EnemyState State;
        public float StunTimer;

        public Rectangle Bounds => new((int)Pos.X - 1, (int)Pos.Y - 1, 3, 3);

        public Enemy(Vector2 p, Color shirt)
        {
            Pos = p; Vel = Vector2.Zero;
            Angle = 0; AngularVel = 0;
            Dir = 1; WanderTimer = 1;
            ShirtColor = shirt;
            MaxAirY = p.Y;
            State = EnemyState.Walking;
            StunTimer = 0f;
        }

        public Vector2 GetPartPos(int part)
        {
            float l = part * 1.0f;
            return new Vector2(
                Pos.X + MathF.Sin(Angle) * l,
                Pos.Y + MathF.Cos(Angle) * l
            );
        }
    }
}
