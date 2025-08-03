using Microsoft.Xna.Framework;
using PixelTowerDefense.Utils;
using Xunit;

namespace PixelTowerDefense.Tests;

public class IsoUtilsTests
{
    [Theory]
    [InlineData(1f, 1f, 2f, 0f)]
    [InlineData(-1f, 1f, 0f, 2f)]
    [InlineData(-1f, -1f, -2f, 0f)]
    [InlineData(1f, -1f, 0f, -2f)]
    public void ToCartDirection_MapsScreenDiagonals_ToCardinals(float sx, float sy, float ex, float ey)
    {
        var result = IsoUtils.ToCartDirection(new Vector2(sx, sy), 1, 1);
        var expected = new Vector2(ex, ey);
        Assert.True(Vector2.Distance(expected, result) < 0.0001f);
    }
}
