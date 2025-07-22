using Microsoft.Xna.Framework;

namespace PixelTowerDefense.Entities
{
    public enum BuildingType
    {
        StockpileHut,
        CarpenterHut
    }

    public struct Building
    {
        public Vector2 Pos;
        public BuildingType Kind;
        public int StoredBerries;
        public int StoredLogs;
        public int StoredPlanks;
        public float CraftTimer;
        public int? ReservedBy;

        public const int CAPACITY = 30;
    }
}
