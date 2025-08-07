using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using PixelTowerDefense.Entities;
using PixelTowerDefense.Systems;
using PixelTowerDefense.Utils;
using PixelTowerDefense.World;

namespace PixelTowerDefense.Tests;

public class ResourceReservationTests
{
    [Fact]
    public void Workers_Reserve_Different_Bushes()
    {
        var rng = new System.Random(0);
        var workers = new List<Meeple>
        {
            new Meeple(Vector2.Zero, Faction.Friendly, Color.White,
                       5, 5, 5, 5)
            {
                Worker = new Worker(),
                Hunger = Utils.Constants.HUNGER_THRESHOLD
            },
            new Meeple(new Vector2(1f, 0f), Faction.Friendly, Color.White,
                       5, 5, 5, 5)
            {
                Worker = new Worker(),
                Hunger = Utils.Constants.HUNGER_THRESHOLD
            }
        };
        var bushes = new List<BerryBush>
        {
            new BerryBush(new Vector2(0f, 2f), rng),
            new BerryBush(new Vector2(2f, 2f), rng)
        };
        var buildings = new List<Building>();
        var trees = new List<Tree>();
        var logs = new List<Wood>();
        var debris = new List<Pixel>();
        var water = new WaterMap(Constants.CHUNK_PIXEL_SIZE, Constants.CHUNK_PIXEL_SIZE);

        var seeds = new List<BuildingSeed>();
        PhysicsSystem.SimulateAll(workers, debris, bushes, buildings, seeds, trees, logs, water, 0.1f);

        var reserved = bushes.Where(b => b.ReservedBy != null).Select(b => b.ReservedBy).ToArray();
        Assert.Equal(2, reserved.Length);
        Assert.NotEqual(reserved[0], reserved[1]);
    }
}
