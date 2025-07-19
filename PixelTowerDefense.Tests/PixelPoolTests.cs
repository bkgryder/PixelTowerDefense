using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PixelTowerDefense.Entities;
using PixelTowerDefense.Utils;
using Xunit;

namespace PixelTowerDefense.Tests;

public class PixelPoolTests
{
    [Fact]
    public void Spawn_DoesNotExceedMaxDebris()
    {
        var pixels = new List<Pixel>();
        for (int i = 0; i < Constants.MAX_DEBRIS + 50; i++)
        {
            pixels.Spawn(new Pixel(Vector2.Zero, Vector2.Zero, Color.White));
        }

        Assert.True(pixels.Count <= Constants.MAX_DEBRIS);
    }
}
