using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PixelTowerDefense.Entities;
using PixelTowerDefense.Systems;
using PixelTowerDefense.Utils;
using PixelTowerDefense.World;

namespace PixelTowerDefense.Tests;

public class RandomStatTests
{
    [Fact]
    public void SpawnMeeple_Stats_AreWithinRange()
    {
        var rng = new System.Random(0);
        var m = Meeple.SpawnMeeple(Vector2.Zero, Faction.Friendly, Color.White, rng);
        Assert.InRange(m.Strength, 3, 10);
        Assert.InRange(m.Dexterity, 3, 10);
        Assert.InRange(m.Intellect, 3, 10);
        Assert.InRange(m.Grit, 3, 10);
    }

    [Fact]
    public void RandomStats_InfluenceJobChoiceAndSpeed()
    {
        var rng = new System.Random(0);
        var worker = Meeple.SpawnMeeple(Vector2.Zero, Faction.Friendly, Color.White, rng);
        worker.Worker = new Worker();
        var workers = new List<Meeple> { worker };
        var trees = new List<Tree> { new Tree(new Vector2(1f,0f), rng) };
        var logs = new List<Log> { new Log(new Vector2(0f,1f), rng) };
        var buildings = new List<Building>();
        var bushes = new List<BerryBush>();
        var debris = new List<Pixel>();
        var water = new WaterMap(1, 1);

        PhysicsSystem.SimulateAll(workers, debris, bushes, buildings, trees, logs, water, 0.1f);

        if (worker.Dexterity >= worker.Strength)
        {
            Assert.True(workers[0].CarriedLogIdx >= 0);
            Assert.Equal(Constants.TREE_HEALTH, trees[0].Health);
        }
        else
        {
            Assert.Equal(Constants.TREE_HEALTH - 1, trees[0].Health);
        }

        if (trees[0].Health < Constants.TREE_HEALTH)
        {
            Assert.Equal(Constants.BASE_CHOP / (1f + 0.1f * worker.Strength), workers[0].WanderTimer);
        }
    }
}
