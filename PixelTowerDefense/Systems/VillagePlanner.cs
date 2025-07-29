using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using PixelTowerDefense.Entities;

namespace PixelTowerDefense.Systems
{
    public static class VillagePlanner
    {
        public const int LOG_THRESHOLD = 20;

        public static int BuiltCount { get; private set; }

        public static void Update(
            List<Meeple> meeples,
            List<Building> buildings,
            List<Wood> logs,
            List<BuildingSeed> seeds,
            System.Random rng)
        {
            int idle = meeples.Count(m => m.Worker != null && m.Worker.Value.CurrentJob == JobType.None);
            int totalLogs = logs.Count + buildings.Sum(b => b.StoredWood);
            if (totalLogs >= LOG_THRESHOLD && idle >= 3 && seeds.Count == 0)
            {
                var stock = buildings.FirstOrDefault(b => b.Kind == BuildingType.StorageHut);
                Vector2 origin = stock.Pos;
                Vector2 pos = origin + new Vector2(rng.Next(-4, 5), rng.Next(-4, 5));
                seeds.Add(new BuildingSeed
                {
                    Pos = pos,
                    Kind = BuildingType.HousingHut,
                    Stage = BuildStage.Planned,
                    RequiredResources = 5,
                    ReservedBy = null
                });
            }
        }

        public static void OnBuildComplete()
        {
            BuiltCount++;
        }
    }
}
