using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace PixelTowerDefense.Entities
{
    public static class BuildingSprites
    {
        public const int TILE_SIZE = 32;

        private static Rectangle Tile(int x, int y)
            => new Rectangle(x * TILE_SIZE, y * TILE_SIZE, TILE_SIZE, TILE_SIZE);

        public static readonly Dictionary<(BuildingType, BuildStage), Rectangle> Sprites = new()
        {
            { (BuildingType.StorageHut, BuildStage.Built), Tile(0, 0) },
            { (BuildingType.HousingHut, BuildStage.Built), Tile(1, 0) },
            { (BuildingType.CarpenterHut, BuildStage.Built), Tile(2, 0) },
        };
    }
}

