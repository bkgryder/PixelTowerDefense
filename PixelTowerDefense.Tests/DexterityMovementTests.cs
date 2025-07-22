using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PixelTowerDefense.Entities;
using PixelTowerDefense.Systems;

namespace PixelTowerDefense.Tests;

public class DexterityMovementTests
{
    [Fact]
    public void FasterWorkers_MoveFarther_WhenHaulingLogs()
    {
        var rng = new System.Random(0);
        var workers = new List<Meeple>
        {
            new Meeple(Vector2.Zero, Faction.Friendly, Color.White)
            {
                Strength = 5,
                Dexterity = 3,
                Intellect = 5,
                Grit = 5,
                Worker = new Worker(),
                CarriedLogIdx = 0
            },
            new Meeple(Vector2.Zero, Faction.Friendly, Color.White)
            {
                Strength = 5,
                Dexterity = 8,
                Intellect = 5,
                Grit = 5,
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

        PhysicsSystem.SimulateAll(workers, debris, bushes, buildings, trees, logs, 1f);

        Assert.True(workers[1].Pos.X > workers[0].Pos.X);
    }
}

