using Microsoft.Xna.Framework;
using PixelTowerDefense.World;
using System.Linq;

public class PathfinderTests
{
    [Fact]
    public void FindsPathAroundObstacle()
    {
        var map = new WaterMap(5,5);
        // block center cell
        map.Depth[2,2] = PathCost.BLOCK_DEPTH;

        var path = Pathfinder.FindPath(map, new Point(0,0), new Point(4,4));
        Assert.NotNull(path);
        Assert.Equal(new Point(0,0), path![0]);
        Assert.Equal(new Point(4,4), path[^1]);
        Assert.DoesNotContain(new Point(2,2), path);
    }
}
