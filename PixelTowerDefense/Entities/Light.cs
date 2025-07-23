using Microsoft.Xna.Framework;

namespace PixelTowerDefense.Entities
{
    public struct Light
    {
        public Vector2 Pos;
        public float Radius;
        public float Intensity;
        public float Lifetime;

        public Light(Vector2 pos, float radius, float intensity, float lifetime)
        {
            Pos = pos;
            Radius = radius;
            Intensity = intensity;
            Lifetime = lifetime;
        }
    }
}
