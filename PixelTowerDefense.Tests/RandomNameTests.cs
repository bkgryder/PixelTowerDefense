using Microsoft.Xna.Framework;
using PixelTowerDefense.Entities;
using PixelTowerDefense.Utils;

namespace PixelTowerDefense.Tests;

public class RandomNameTests
{
    [Fact]
    public void SpawnMeeple_HasRandomName()
    {
        var rng = new System.Random(0);
        var m = Meeple.SpawnMeeple(Vector2.Zero, Faction.Friendly, Color.White, rng);
        Assert.False(string.IsNullOrWhiteSpace(m.Name));
    }
}
