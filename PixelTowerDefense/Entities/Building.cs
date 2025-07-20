using Microsoft.Xna.Framework;

namespace PixelTowerDefense.Entities
{
    public enum BuildingType { StockpileHut }

    public struct Building
    {
        public Vector2 Pos;
        public BuildingType Kind;
        public int StoredBerries;
        public const int CAPACITY = 30;
    }
}
