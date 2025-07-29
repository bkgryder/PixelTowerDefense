using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PixelTowerDefense.Entities;
using PixelTowerDefense.Systems;
using Xunit;

namespace PixelTowerDefense.Tests;

public class VillagePlannerTests
{
    [Fact]
    public void Planner_Spawns_Seed_When_Enough_Logs_And_Idle()
    {
        var rng = new System.Random(0);
        var meeples = new List<Meeple>
        {
            new Meeple(Vector2.Zero, Faction.Friendly, Color.White,5,5,5,5){ Worker = new Worker() },
            new Meeple(Vector2.One, Faction.Friendly, Color.White,5,5,5,5){ Worker = new Worker() },
            new Meeple(new Vector2(2f,2f), Faction.Friendly, Color.White,5,5,5,5){ Worker = new Worker() }
        };
        var buildings = new List<Building>
        {
            new Building { Pos = Vector2.Zero, Kind = BuildingType.StorageHut, StoredWood = 20 }
        };
        var logs = new List<Wood>();
        var seeds = new List<BuildingSeed>();

        VillagePlanner.Update(meeples, buildings, logs, seeds, rng);

        Assert.Single(seeds);
        Assert.Equal(BuildStage.Planned, seeds[0].Stage);
    }
}
