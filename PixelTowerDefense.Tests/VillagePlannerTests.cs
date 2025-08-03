using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PixelTowerDefense.Entities;
using PixelTowerDefense.Systems;
using PixelTowerDefense.Utils;
using Xunit;

namespace PixelTowerDefense.Tests;

public class VillagePlannerTests
{
    [Fact]
    public void Planner_Spawns_StorageHutSeed_When_Requirements_Met()
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
            new Building { Pos = Vector2.Zero, Kind = BuildingType.StorageHut, StoredWood = 10 }
        };
        var logs = new List<Wood>();
        for(int i=0;i<10;i++) logs.Add(new Wood(Vector2.Zero, rng));
        var seeds = new List<BuildingSeed>();

        VillagePlanner.Update(meeples, buildings, logs, seeds, rng);

        Assert.Single(seeds);
        Assert.Equal(BuildingType.StorageHut, seeds[0].Kind);
        float dist = Vector2.Distance(
            IsoUtils.ToCart(seeds[0].Pos, 1, 1),
            IsoUtils.ToCart(buildings[0].Pos, 1, 1));
        // Distances account for the larger hut footprints
        Assert.InRange(dist, 15f, 21f);
    }

    [Fact]
    public void Planner_Spawns_HousingSeed_When_Requirements_Met()
    {
        var rng = new System.Random(1);
        var meeples = new List<Meeple>
        {
            new Meeple(Vector2.Zero, Faction.Friendly, Color.White,5,5,5,5){ Worker = new Worker() },
            new Meeple(Vector2.One, Faction.Friendly, Color.White,5,5,5,5){ Worker = new Worker() },
            new Meeple(new Vector2(2f,2f), Faction.Friendly, Color.White,5,5,5,5){ Worker = new Worker() },
            new Meeple(new Vector2(3f,3f), Faction.Friendly, Color.White,5,5,5,5){ Worker = new Worker() },
            new Meeple(new Vector2(4f,4f), Faction.Friendly, Color.White,5,5,5,5){ Worker = new Worker() },
            new Meeple(new Vector2(5f,5f), Faction.Friendly, Color.White,5,5,5,5){ Worker = new Worker() },
            new Meeple(new Vector2(6f,6f), Faction.Friendly, Color.White,5,5,5,5){ Worker = new Worker() },
            new Meeple(new Vector2(7f,7f), Faction.Friendly, Color.White,5,5,5,5){ Worker = new Worker() }
        };
        var buildings = new List<Building>
        {
            new Building { Pos = Vector2.Zero, Kind = BuildingType.StorageHut, StoredWood = 15 },
            new Building { Pos = new Vector2(10f,0f), Kind = BuildingType.StorageHut, StoredWood = 0 }
        };
        var logs = new List<Wood>();
        var seeds = new List<BuildingSeed>();

        VillagePlanner.Update(meeples, buildings, logs, seeds, rng);

        Assert.Single(seeds);
        Assert.Equal(BuildingType.HousingHut, seeds[0].Kind);
    }
}
