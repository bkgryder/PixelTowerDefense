using PixelTowerDefense.World;
using PixelTowerDefense.Utils;

namespace PixelTowerDefense.Tests;

public class ChunkInitializationTests
{
    [Fact]
    public void SingleChunk_HasCorrectDimensions()
    {
        var world = new GameWorld();
        Assert.NotNull(world.Chunks[0,0]);
        Assert.Equal(Constants.CHUNK_TILES, world.Chunks[0,0].Tiles.GetLength(0));
        Assert.Equal(Constants.CHUNK_TILES, world.Chunks[0,0].Tiles.GetLength(1));
    }
}
