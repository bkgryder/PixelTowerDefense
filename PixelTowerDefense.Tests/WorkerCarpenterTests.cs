using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PixelTowerDefense.Entities;
using PixelTowerDefense.Systems;
using PixelTowerDefense.Utils;
using Xunit;

namespace PixelTowerDefense.Tests;

public class WorkerCarpenterTests
{
    [Fact]
    public void Worker_ChopsTree_SpawnsLogs()
    {
        var rng = new System.Random(0);
        var worker = new Meeple(Vector2.Zero, Faction.Friendly, Color.White);
        worker.Strength = 5;
        worker.Dexterity = 5;
        worker.Intellect = 5;
        worker.Grit = 5;
        worker.Worker = new Worker();
        var tree = new Tree(new Vector2(0.5f, 0f), rng);
        tree.Health = 3;
        var logs = new List<Log>();

        PhysicsSystem.WorkerChopTree(ref worker, ref tree, logs, rng);

        Assert.Equal(2, logs.Count);
        Assert.Equal(2, tree.Health);
    }

    [Fact]
    public void Worker_DepositsLog_AtCarpenterHut()
    {
        var worker = new Meeple(Vector2.Zero, Faction.Friendly, Color.White);
        worker.Strength = 5;
        worker.Dexterity = 5;
        worker.Intellect = 5;
        worker.Grit = 5;
        worker.Worker = new Worker();
        worker.CarriedLogs = 1;
        var hut = new Building { Pos = Vector2.Zero, Kind = BuildingType.CarpenterHut };

        PhysicsSystem.WorkerDepositLog(ref worker, ref hut);

        Assert.Equal(0, worker.CarriedLogs);
        Assert.Equal(1, hut.StoredLogs);
        Assert.Equal(Constants.BASE_CRAFT / (1f + 0.1f * worker.Intellect), hut.CraftTimer);
    }

    [Fact]
    public void CarpenterHut_ConvertsLogsIntoPlanks()
    {
        var hut = new Building { Pos = Vector2.Zero, Kind = BuildingType.CarpenterHut, StoredLogs = 1, CraftTimer = Constants.BASE_CRAFT };

        PhysicsSystem.UpdateCarpenter(ref hut, Constants.BASE_CRAFT);

        Assert.Equal(0, hut.StoredLogs);
        Assert.Equal(1, hut.StoredPlanks);
    }

    [Fact]
    public void StrongWorkers_Have_Shorter_ChopCooldown()
    {
        var rng = new System.Random(0);
        var weak = new Meeple(Vector2.Zero, Faction.Friendly, Color.White) { Strength = 3, Worker = new Worker() };
        var strong = new Meeple(Vector2.Zero, Faction.Friendly, Color.White) { Strength = 10, Worker = new Worker() };
        var tree = new Tree(new Vector2(0.5f, 0f), rng);
        var logs = new List<Log>();

        PhysicsSystem.WorkerChopTree(ref weak, ref tree, logs, rng);
        float weakCooldown = weak.WanderTimer;

        tree.Health = 3;
        PhysicsSystem.WorkerChopTree(ref strong, ref tree, logs, rng);
        float strongCooldown = strong.WanderTimer;

        Assert.True(strongCooldown < weakCooldown);
    }

    [Fact]
    public void SmartWorkers_CraftFaster()
    {
        var hutA = new Building { Pos = Vector2.Zero, Kind = BuildingType.CarpenterHut };
        var hutB = new Building { Pos = Vector2.Zero, Kind = BuildingType.CarpenterHut };

        var slow = new Meeple(Vector2.Zero, Faction.Friendly, Color.White) { Intellect = 1, Worker = new Worker(), CarriedLogs = 1 };
        var smart = new Meeple(Vector2.Zero, Faction.Friendly, Color.White) { Intellect = 10, Worker = new Worker(), CarriedLogs = 1 };

        PhysicsSystem.WorkerDepositLog(ref slow, ref hutA);
        PhysicsSystem.WorkerDepositLog(ref smart, ref hutB);

        PhysicsSystem.UpdateCarpenter(ref hutA, 0.75f);
        PhysicsSystem.UpdateCarpenter(ref hutB, 0.75f);

        Assert.Equal(0, hutA.StoredPlanks);
        Assert.Equal(1, hutB.StoredPlanks);
    }
}
