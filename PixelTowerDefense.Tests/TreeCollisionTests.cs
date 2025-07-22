using Microsoft.Xna.Framework;
using PixelTowerDefense.Entities;
using PixelTowerDefense.Systems;
using PixelTowerDefense.Utils;

namespace PixelTowerDefense.Tests;

public class TreeCollisionTests
{
    [Fact]
    public void Meeple_PushedOutsideTreeRadius()
    {
        var rng = new System.Random(0);
        var trees = new List<Tree> { new Tree(Vector2.Zero, rng) };
        var meeples = new List<Meeple> { new Meeple(Vector2.Zero, Faction.Friendly, Color.White) };
        var debris = new List<Pixel>();
        var bushes = new List<BerryBush>();
        var buildings = new List<Building>();

        PhysicsSystem.SimulateAll(meeples, debris, bushes, buildings, trees, 0.1f);

        float dist = Vector2.Distance(meeples[0].Pos, trees[0].Pos);
        Assert.True(dist >= trees[0].CollisionRadius - 0.001f);
    }
}
