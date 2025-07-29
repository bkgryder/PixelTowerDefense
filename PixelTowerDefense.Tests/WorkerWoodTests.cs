using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PixelTowerDefense.Entities;
using PixelTowerDefense.Systems;
using PixelTowerDefense.Utils;
using Xunit;

namespace PixelTowerDefense.Tests;

public class WorkerWoodTests
{
    [Fact]
    public void Worker_ChopsTree_SpawnsWood()
    {
        var rng = new System.Random(0);
        var worker = new Meeple(Vector2.Zero, Faction.Friendly, Color.White,
                                5, 5, 5, 5)
        { Worker = new Worker() };
        var tree = new Tree(new Vector2(0.5f, 0f), rng);
        tree.Health = 3;
        var logs = new List<Wood>();

        PhysicsSystem.WorkerChopTree(ref worker, ref tree, logs, rng);

        Assert.Equal(2, logs.Count);
        Assert.Equal(2, tree.Health);
        Assert.Equal(Constants.BASE_CHOP / (1f + 0.1f * worker.Strength),
                     worker.WanderTimer);
    }

    [Fact]
    public void Worker_DepositsWood_AtStorageHut()
    {
        var worker = new Meeple(Vector2.Zero, Faction.Friendly, Color.White,
                                5, 5, 5, 5)
        {
            Worker = new Worker(),
            CarriedWood = 1
        };
        var hut = new Building
        {
            Pos = Vector2.Zero,
            Kind = BuildingType.StorageHut,
            BerryCapacity = Constants.STORAGE_BERRY_CAPACITY,
            LogCapacity = Constants.STORAGE_LOG_CAPACITY,
            PlankCapacity = Constants.STORAGE_PLANK_CAPACITY,
            BedSlots = 0
        };

        PhysicsSystem.WorkerDepositWood(ref worker, ref hut);

        Assert.Equal(0, worker.CarriedWood);
        Assert.Equal(1, hut.StoredWood);
    }

    [Fact]
    public void StrongWorkers_Have_Shorter_ChopCooldown()
    {
        var rng = new System.Random(0);
        var weak = new Meeple(Vector2.Zero, Faction.Friendly, Color.White,
                               3, 5, 5, 5)
        { Worker = new Worker() };
        var strong = new Meeple(Vector2.Zero, Faction.Friendly, Color.White,
                                 10, 5, 5, 5)
        { Worker = new Worker() };
        var tree = new Tree(new Vector2(0.5f, 0f), rng);
        var logs = new List<Wood>();

        PhysicsSystem.WorkerChopTree(ref weak, ref tree, logs, rng);
        float weakCooldown = weak.WanderTimer;
        Assert.Equal(Constants.BASE_CHOP / (1f + 0.1f * weak.Strength), weakCooldown);

        tree.Health = 3;
        PhysicsSystem.WorkerChopTree(ref strong, ref tree, logs, rng);
        float strongCooldown = strong.WanderTimer;
        Assert.Equal(Constants.BASE_CHOP / (1f + 0.1f * strong.Strength), strongCooldown);

        Assert.True(strongCooldown < weakCooldown);
    }

}
