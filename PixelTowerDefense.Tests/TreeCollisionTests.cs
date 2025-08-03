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
        var logs = new List<Wood>();
        var water = new WaterMap(Constants.CHUNK_PIXEL_SIZE, Constants.CHUNK_PIXEL_SIZE);

        PhysicsSystem.SimulateAll(meeples, debris, bushes, buildings, trees, logs, water, 0.1f);

        float dist = Vector2.Distance(
            IsoUtils.ToCart(meeples[0].Pos, 1, 1),
            IsoUtils.ToCart(trees[0].Pos, 1, 1));
        Assert.True(dist >= trees[0].CollisionRadius - 0.001f);
    }

}
