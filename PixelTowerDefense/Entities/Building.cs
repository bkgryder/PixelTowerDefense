using Microsoft.Xna.Framework;

namespace PixelTowerDefense.Entities
{
    public enum BuildingType
    {
        StorageHut,
        HousingHut
    }

    public struct Building
    {
        public Vector2 Pos;
        public BuildingType Kind;
        public int StoredBerries;
        public int StoredWood;
        public int HousedMeeples;
        public int? ReservedBy;

        public const int CAPACITY = 30;
        public const int HOUSE_CAPACITY = 5;
    }
}
