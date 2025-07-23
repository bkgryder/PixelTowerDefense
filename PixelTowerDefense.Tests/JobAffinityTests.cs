using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PixelTowerDefense.Entities;
using PixelTowerDefense.Systems;
using PixelTowerDefense.Utils;
using Xunit;

namespace PixelTowerDefense.Tests;

public class JobAffinityTests
{
    [Fact]
    public void StrongWorker_Prefers_Chopping()
    {
        var rng = new System.Random(0);
        var workers = new List<Meeple>
        {
            new Meeple(Vector2.Zero, Faction.Friendly, Color.White,
                       8, 3, 5, 5)
            { Worker = new Worker() }
        };
        var trees = new List<Tree> { new Tree(new Vector2(1f, 0f), rng) };
        var logs = new List<Log> { new Log(new Vector2(0f, 1f), rng) };
        var buildings = new List<Building>();
        var bushes = new List<BerryBush>();
        var debris = new List<Pixel>();

        PhysicsSystem.SimulateAll(workers, debris, bushes, buildings, trees, logs, 0.1f);

        Assert.Equal(Constants.TREE_HEALTH - 1, trees[0].Health);
        Assert.Equal(-1, workers[0].CarriedLogIdx);
    }

    [Fact]
    public void DexterousWorker_Prefers_Hauling()
    {
        var rng = new System.Random(0);
        var workers = new List<Meeple>
        {
            new Meeple(Vector2.Zero, Faction.Friendly, Color.White,
                       3, 8, 5, 5)
            { Worker = new Worker() }
        };
        var trees = new List<Tree> { new Tree(new Vector2(1f, 0f), rng) };
        var logs = new List<Log> { new Log(new Vector2(0f, 1f), rng) };
        var buildings = new List<Building>();
        var bushes = new List<BerryBush>();
        var debris = new List<Pixel>();

        PhysicsSystem.SimulateAll(workers, debris, bushes, buildings, trees, logs, 0.1f);

        Assert.True(workers[0].CarriedLogIdx >= 0);
        Assert.Equal(Constants.TREE_HEALTH, trees[0].Health);
    }
}
