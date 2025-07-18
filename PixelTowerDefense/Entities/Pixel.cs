using Microsoft.Xna.Framework;

namespace PixelTowerDefense.Entities
{
    public struct Pixel
    {
        public Vector2 Pos, Vel;
        public Color Col;
        public float AngularVel;

        public Pixel(Vector2 p, Vector2 v, Color c, float angVel = 0f)
        {
            Pos = p;
            Vel = v;
            Col = c;
            AngularVel = angVel;
        }

        public Rectangle Bounds => new((int)Pos.X, (int)Pos.Y, 1, 1);
    }
}
