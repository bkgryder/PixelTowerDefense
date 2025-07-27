namespace PixelTowerDefense.World
{
    public struct GroundCell
    {
        public Biome Biome;
        public byte Moisture;
        public byte Fertility;
        public byte Variant;
    }

    public class GroundMap
    {
        public int W { get; }
        public int H { get; }
        public GroundCell[,] Cells { get; }

        public GroundMap(int w, int h)
        {
            W = w; H = h;
            Cells = new GroundCell[w, h];
        }
    }
}
