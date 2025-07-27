using Microsoft.Xna.Framework;
using PixelTowerDefense.Entities;
using PixelTowerDefense.Systems;
using PixelTowerDefense.Utils;
using PixelTowerDefense.World;

namespace PixelTowerDefense.Tests;

public class TreeCollisionTests
{
    [Fact]
    public void Meeple_PushedOutsideTreeRadius()
    {
        var rng = new System.Random(0);
        var trees = new List<Tree> { new Tree(Vector2.Zero, rng) };
        var meeples = new List<Meeple>
        {
            new Meeple(Vector2.Zero, Faction.Friendly, Color.White,
                       5, 5, 5, 5)
        };
        var debris = new List<Pixel>();
        var bushes = new List<BerryBush>();
        var buildings = new List<Building>();
        var logs = new List<Log>();
        var water = new WaterMap(1, 1);

        PhysicsSystem.SimulateAll(meeples, debris, bushes, buildings, trees, logs, water, 0.1f);

        float dist = Vector2.Distance(meeples[0].Pos, trees[0].Pos);
        Assert.True(dist >= trees[0].CollisionRadius - 0.001f);
    }

    [Fact]
    public void Meeple_IgnoresStumpCollision()
    {
        var rng = new System.Random(0);
        var stump = new Tree(new Vector2(5f, 5f), rng)
        {
            IsStump = true,
            CollisionRadius = Constants.STUMP_RADIUS
        };
        var trees = new List<Tree> { stump };
        var meeples = new List<Meeple>
        {
            new Meeple(new Vector2(5f, 5f), Faction.Friendly, Color.White,
                       5, 5, 5, 5)
        };
        var debris = new List<Pixel>();
        var bushes = new List<BerryBush>();
        var buildings = new List<Building>();
        var logs = new List<Log>();
        var water = new WaterMap(1, 1);

        PhysicsSystem.SimulateAll(meeples, debris, bushes, buildings, trees, logs, water, 0.1f);

        Assert.True(Vector2.Distance(new Vector2(5f, 5f), meeples[0].Pos) < 0.001f);
    }
}
