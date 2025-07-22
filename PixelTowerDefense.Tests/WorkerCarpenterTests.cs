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
        worker.Worker = new Worker();
        worker.CarriedLogs = 1;
        var hut = new Building { Pos = Vector2.Zero, Kind = BuildingType.CarpenterHut };

        PhysicsSystem.WorkerDepositLog(ref worker, ref hut);

        Assert.Equal(0, worker.CarriedLogs);
        Assert.Equal(1, hut.StoredLogs);
    }

    [Fact]
    public void CarpenterHut_ConvertsLogsIntoPlanks()
    {
        var hut = new Building { Pos = Vector2.Zero, Kind = BuildingType.CarpenterHut, StoredLogs = 1, CraftTimer = Constants.CARPENTER_CRAFT_TIME };

        PhysicsSystem.UpdateCarpenter(ref hut, Constants.CARPENTER_CRAFT_TIME);

        Assert.Equal(0, hut.StoredLogs);
        Assert.Equal(1, hut.StoredPlanks);
    }
}
