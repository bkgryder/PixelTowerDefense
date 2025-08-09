using System;
using Microsoft.Xna.Framework;

namespace PixelTowerDefense.Entities
{
    public enum BuildingType
    {
        StorageHut,
        HousingHut,
        CarpenterHut
    }

    public enum BuildingStage
    {
        Ghost,
        Built
    }

    public struct Building
    {
        public Vector2 Pos;
        public BuildingType Kind;
        public BuildingStage Stage;
        public int StoredBerries;
        public int StoredWood;
        public int HousedMeeples;
        public int? ReservedBy;

        public int BerryCapacity;
        public int LogCapacity;
        public int PlankCapacity;
        public int BedSlots;

        public int RequiredLogs;
        public int RequiredPlanks;
        public float WorkProgress;

        public Rectangle Bounds
        {
            get
            {
                int tileSize = BuildingSprites.TILE_SIZE;
                int startX = (int)MathF.Round(Pos.X) - tileSize / 2;
                int startY = (int)MathF.Round(Pos.Y) - tileSize / 2 + tileSize / 3;
                return new Rectangle(startX, startY, tileSize, tileSize / 3);
            }
        }
    }
}
