using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PixelTowerDefense.Entities;
using PixelTowerDefense.Systems;

namespace PixelTowerDefense.Tests;

public class VillagePlannerRulesTests
{
    [Fact]
    public void Planner_Spawns_Seeds_When_Requirements_Met()
    {
        var rng = new System.Random(2);
        var meeples = new List<Meeple>
        {
            new Meeple(Vector2.Zero, Faction.Friendly, Color.White,5,5,5,5){ Worker = new Worker() },
            new Meeple(Vector2.One, Faction.Friendly, Color.White,5,5,5,5){ Worker = new Worker() },
            new Meeple(new Vector2(2f,2f), Faction.Friendly, Color.White,5,5,5,5){ Worker = new Worker() },
            new Meeple(new Vector2(3f,3f), Faction.Friendly, Color.White,5,5,5,5){ Worker = new Worker() },
            new Meeple(new Vector2(4f,4f), Faction.Friendly, Color.White,5,5,5,5){ Worker = new Worker() }
        };
        var buildings = new List<Building>
        {
            new Building { Pos = Vector2.Zero, Kind = BuildingType.StorageHut, StoredWood = VillagePlanner.LOG_THRESHOLD },
            new Building { Pos = new Vector2(10f,0f), Kind = BuildingType.StorageHut, StoredWood = 0 }
        };
        var logs = new List<Wood>();
        var seeds = new List<BuildingSeed>();

        VillagePlanner.Update(meeples, buildings, logs, seeds, rng);
        Assert.Single(seeds);
        Assert.Equal(BuildingType.StorageHut, seeds[0].Kind);

        buildings.Add(new Building { Pos = new Vector2(20f,0f), Kind = BuildingType.StorageHut, StoredWood = 15 });
        logs.Clear();
        seeds.Clear();
        VillagePlanner.Update(meeples, buildings, logs, seeds, rng);
        Assert.Single(seeds);
        Assert.Equal(BuildingType.HousingHut, seeds[0].Kind);
    }
}
