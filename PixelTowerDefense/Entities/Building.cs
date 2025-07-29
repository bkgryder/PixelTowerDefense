using Microsoft.Xna.Framework;

namespace PixelTowerDefense.Entities
{
    public enum BuildingType
    {
        StorageHut,
        HousingHut,
        CarpenterHut
    }

    public struct Building
    {
        public Vector2 Pos;
        public BuildingType Kind;
        public int StoredBerries;
        public int StoredWood;
        public int HousedMeeples;
        public int? ReservedBy;

        public int BerryCapacity;
        public int LogCapacity;
        public int PlankCapacity;
        public int BedSlots;
    }
}
