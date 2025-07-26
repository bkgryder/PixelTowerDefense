namespace PixelTowerDefense.World
{
    public class GameWorld
    {
        public readonly Chunk[,] Chunks;

        public GameWorld()
        {
            Chunks = new Chunk[1, 1];
            Chunks[0, 0] = new Chunk();
        }
    }
}
