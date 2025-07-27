using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PixelTowerDefense.Entities;
using PixelTowerDefense.Systems;
using PixelTowerDefense.Utils;
using PixelTowerDefense.World;

namespace PixelTowerDefense.Tests;

public class DexterityMovementTests
{
    [Fact]
    public void FasterWorkers_MoveFarther_WhenHaulingLogs()
    {
        var rng = new System.Random(0);
        var workers = new List<Meeple>
        {
            new Meeple(Vector2.Zero, Faction.Friendly, Color.White,
                       5, 3, 5, 5)
            {
                Worker = new Worker(),
                CarriedLogIdx = 0
            },
            new Meeple(Vector2.Zero, Faction.Friendly, Color.White,
                       5, 8, 5, 5)
            {
                Worker = new Worker(),
                CarriedLogIdx = 1
            }
        };
        var logs = new List<Log>
        {
            new Log(Vector2.Zero, rng) { IsCarried = true },
            new Log(Vector2.Zero, rng) { IsCarried = true }
        };
        var buildings = new List<Building>
        {
            new Building { Pos = new Vector2(10f, 0f), Kind = BuildingType.CarpenterHut }
        };
        var debris = new List<Pixel>();
        var bushes = new List<BerryBush>();
        var trees = new List<Tree>();
        var water = new WaterMap(Constants.CHUNK_PIXEL_SIZE, Constants.CHUNK_PIXEL_SIZE);

        PhysicsSystem.SimulateAll(workers, debris, bushes, buildings, trees, logs, water, 1f);

        Assert.True(workers[1].Pos.X > workers[0].Pos.X);
    }
}

