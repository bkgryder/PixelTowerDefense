using PixelTowerDefense.Utils;

namespace PixelTowerDefense.World
{
    public class Chunk
    {
        public const int Size = Constants.CHUNK_TILES;
        public readonly Tile[,] Tiles;

        public Chunk()
        {
            Tiles = new Tile[Size, Size];
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                    Tiles[x, y] = new Tile { Type = TileType.Grass };
        }
    }
}
