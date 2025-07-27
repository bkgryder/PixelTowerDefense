namespace PixelTowerDefense.World
{
    public static class PatternAtlas
    {
        static readonly bool[,,] _patterns = new bool[4,4,4]
        {
            // 0 - checker
            {
                { true, false, true, false },
                { false, true, false, true },
                { true, false, true, false },
                { false, true, false, true }
            },
            // 1 - diagonal
            {
                { true, false, false, false },
                { false, true, false, false },
                { false, false, true, false },
                { false, false, false, true }
            },
            // 2 - dots
            {
                { true, false, false, true },
                { false, false, false, false },
                { false, false, false, false },
                { true, false, false, true }
            },
            // 3 - vertical stripes
            {
                { true, false, true, false },
                { true, false, true, false },
                { true, false, true, false },
                { true, false, true, false }
            }
        };

        public static bool IsSet(int id, int x, int y)
        {
            id = id % 4;
            x &= 3; y &= 3;
            return _patterns[id, y, x];
        }
    }
}
