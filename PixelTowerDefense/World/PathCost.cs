namespace PixelTowerDefense.World
{
    /// <summary>
    /// Utility helpers for terrain path costs.
    /// TODO: implement grid A* pathfinding later.
    /// </summary>
    public static class PathCost
    {
        public const int BLOCK_DEPTH = 200;

        public static float ForCell(WaterMap water, int x, int y, float baseCost, float wadePenalty)
        {
            byte depth = water.Depth[x, y];
            if (depth >= BLOCK_DEPTH)
                return float.PositiveInfinity;

            return baseCost + wadePenalty * (depth / 255f);
        }
    }
}
