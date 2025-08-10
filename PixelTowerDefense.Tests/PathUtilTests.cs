using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PixelTowerDefense.Utils;

public class PathUtilTests
{
    [Fact]
    public void FollowsStoredPath()
    {
        var path = new Queue<Point>();
        path.Enqueue(new Point(0,0));
        path.Enqueue(new Point(1,0));
        Vector2 pos = new Vector2(Constants.CELL_PIXELS / 2f, Constants.CELL_PIXELS / 2f);
        Vector2 vel = Vector2.Zero;
        while (PathUtil.FollowPath(ref pos, ref vel, path, 2f, 0.1f)) { }
        Assert.Empty(path);
        Assert.Equal(new Vector2(Constants.CELL_PIXELS * 1 + Constants.CELL_PIXELS / 2f,
                                 Constants.CELL_PIXELS / 2f), pos);
    }

    [Fact]
    public void HandlesNullPath()
    {
        Vector2 pos = Vector2.Zero;
        Vector2 vel = Vector2.One;
        bool moved = PathUtil.FollowPath(ref pos, ref vel, null, 1f, 0.1f);
        Assert.False(moved);
        Assert.Equal(Vector2.Zero, vel);
    }
}
