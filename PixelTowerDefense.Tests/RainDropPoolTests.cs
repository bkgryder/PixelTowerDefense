using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PixelTowerDefense.Entities;
using PixelTowerDefense.Utils;
using Xunit;

namespace PixelTowerDefense.Tests;

public class RainDropPoolTests
{
    [Fact]
    public void Spawn_DoesNotExceedMax()
    {
        var drops = new List<RainDrop>();
        for (int i = 0; i < Constants.MAX_RAIN_DROPS + 50; i++)
        {
            drops.Spawn(new RainDrop(Vector2.Zero, 1f, 1f));
        }

        Assert.True(drops.Count <= Constants.MAX_RAIN_DROPS);
    }
}
