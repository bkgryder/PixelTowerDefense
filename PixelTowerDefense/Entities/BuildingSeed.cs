using Microsoft.Xna.Framework;

namespace PixelTowerDefense.Entities
{
    public enum BuildStage
    {
        Planned,
        Framed,
        Built
    }

    public struct BuildingSeed
    {
        public Vector2 Pos;
        public BuildingType Kind;
        public BuildStage Stage;
        public int RequiredResources;
        public int FootprintW;
        public int FootprintH;
        public int? ReservedBy;

        public BuildingSeed(Vector2 pos, BuildingType kind)
        {
            Pos = pos;
            Kind = kind;
            Stage = BuildStage.Planned;
            ReservedBy = null;

            switch (kind)
            {
                case BuildingType.StorageHut:
                    FootprintW = Utils.Constants.HUT_FOOTPRINT_W;
                    FootprintH = Utils.Constants.HUT_FOOTPRINT_H;
                    RequiredResources = Utils.Constants.STORAGE_HUT_COSTS[0];
                    break;
                case BuildingType.HousingHut:
                    FootprintW = Utils.Constants.HUT_FOOTPRINT_W;
                    FootprintH = Utils.Constants.HUT_FOOTPRINT_H;
                    RequiredResources = Utils.Constants.HOUSING_HUT_COSTS[0];
                    break;
                case BuildingType.CarpenterHut:
                    FootprintW = Utils.Constants.HUT_FOOTPRINT_W;
                    FootprintH = Utils.Constants.HUT_FOOTPRINT_H;
                    RequiredResources = Utils.Constants.STORAGE_HUT_COSTS[0];
                    break;
                default:
                    FootprintW = 1;
                    FootprintH = 1;
                    RequiredResources = 0;
                    break;
            }
        }
    }
}
