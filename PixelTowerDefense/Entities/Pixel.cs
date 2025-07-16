using Microsoft.Xna.Framework;

namespace PixelTowerDefense.Entities
{
    /// <summary>
    /// A small particle emitted when enemies explode or collide.
    /// </summary>
    public struct Pixel
    {
        public Vector2 Pos, Vel;
        public Color Col;
        public Rectangle Bounds => new((int)Pos.X, (int)Pos.Y, 1, 1);
        public Pixel(Vector2 p, Vector2 v, Color c)
        {
            Pos = p;
            Vel = v;
            Col = c;
        }
    }
}
