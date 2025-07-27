namespace PixelTowerDefense.World
{
    public class WaterMap
    {
        public int Width { get; }
        public int Height { get; }
        public byte[,] Depth { get; }
        public sbyte[,] FlowX { get; }
        public sbyte[,] FlowY { get; }

        public WaterMap(int width, int height)
        {
            Width = width;
            Height = height;
            Depth = new byte[width, height];
            FlowX = new sbyte[width, height];
            FlowY = new sbyte[width, height];
        }
    }
}
