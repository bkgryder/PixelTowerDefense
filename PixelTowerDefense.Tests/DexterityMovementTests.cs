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
    public void FasterWorkers_MoveFarther_WhenHaulingWood()
    {
        var rng = new System.Random(0);
        var workers = new List<Meeple>
        {
            new Meeple(Vector2.Zero, Faction.Friendly, Color.White,
                       5, 3, 5, 5)
            {
                Worker = new Worker(),
                CarriedWoodIdx = 0
            },
            new Meeple(Vector2.Zero, Faction.Friendly, Color.White,
                       5, 8, 5, 5)
            {
                Worker = new Worker(),
                CarriedWoodIdx = 1
            }
        };
        var logs = new List<Wood>
        {
            new Wood(Vector2.Zero, rng) { IsCarried = true },
            new Wood(Vector2.Zero, rng) { IsCarried = true }
        };
        var buildings = new List<Building>
        {
            new Building
            {
                Pos = new Vector2(10f, 0f),
                Kind = BuildingType.StorageHut,
                BerryCapacity = Constants.STORAGE_BERRY_CAPACITY,
                LogCapacity = Constants.STORAGE_LOG_CAPACITY,
                PlankCapacity = Constants.STORAGE_PLANK_CAPACITY,
                BedSlots = 0
            }
        };
        var debris = new List<Pixel>();
        var bushes = new List<BerryBush>();
        var trees = new List<Tree>();
        var water = new WaterMap(Constants.CHUNK_PIXEL_SIZE, Constants.CHUNK_PIXEL_SIZE);

        PhysicsSystem.SimulateAll(workers, debris, bushes, buildings, trees, logs, water, 1f);

        var slowCart = IsoUtils.ToCart(workers[0].Pos, 1, 1);
        var fastCart = IsoUtils.ToCart(workers[1].Pos, 1, 1);
        Assert.True(fastCart.Length() > slowCart.Length());
    }
}

