using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PixelTowerDefense.Entities;
using PixelTowerDefense.Systems;
using PixelTowerDefense.Utils;

namespace PixelTowerDefense.Tests;

public class BuildingSeedProgressionTests
{
    private static void DeliverResource(ref BuildingSeed seed)
    {
        seed.RequiredResources--;
        if (seed.RequiredResources <= 0)
        {
            if (seed.Stage == BuildStage.Planned)
            {
                seed.Stage = BuildStage.Framed;
                int[] costs = seed.Kind switch
                {
                    BuildingType.StorageHut => Constants.STORAGE_HUT_COSTS,
                    BuildingType.HousingHut => Constants.HOUSING_HUT_COSTS,
                    BuildingType.CarpenterHut => Constants.STORAGE_HUT_COSTS,
                    _ => Constants.STORAGE_HUT_COSTS
                };
                seed.RequiredResources = costs[1];
            }
            else if (seed.Stage == BuildStage.Framed)
            {
                seed.Stage = BuildStage.Built;
                VillagePlanner.OnBuildComplete();
            }
        }
    }

    [Fact]
    public void Seed_Progresses_Planned_To_Framed_To_Built()
    {
        int initialBuilt = VillagePlanner.BuiltCount;
        var seed = new BuildingSeed(Vector2.Zero, BuildingType.StorageHut);

        for (int i = 0; i < Constants.STORAGE_HUT_COSTS[0]; i++)
            DeliverResource(ref seed);

        Assert.Equal(BuildStage.Framed, seed.Stage);
        Assert.Equal(Constants.STORAGE_HUT_COSTS[1], seed.RequiredResources);

        for (int i = 0; i < Constants.STORAGE_HUT_COSTS[1]; i++)
            DeliverResource(ref seed);

        Assert.Equal(BuildStage.Built, seed.Stage);
        Assert.Equal(initialBuilt + 1, VillagePlanner.BuiltCount);
    }
}
