using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using PixelTowerDefense.Entities;
using PixelTowerDefense.Utils;

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

            int storageCount = buildings.Count(b => b.Kind == BuildingType.StorageHut);
            int housingCount = buildings.Count(b => b.Kind == BuildingType.HousingHut);

            int totalLogs = logs.Count + buildings.Sum(b => b.StoredWood);

            if (totalLogs >= LOG_THRESHOLD && idle >= 3 && storageCount < 3)
            {
                Vector2 origin = buildings[rng.Next(buildings.Count)].Pos;
                Vector2 pos = FindValidSpot(origin, BuildingType.StorageHut, buildings, seeds, rng);
                seeds.Add(new BuildingSeed(pos, BuildingType.StorageHut));
                return;
            }

            int totalPlanks = buildings.Sum(b => b.StoredWood);

            if (storageCount >= 2 && totalPlanks >= 15 && idle >= 2 && housingCount < meeples.Count / 4)
            {
                Vector2 origin = buildings[rng.Next(buildings.Count)].Pos;
                Vector2 pos = FindValidSpot(origin, BuildingType.HousingHut, buildings, seeds, rng);
                seeds.Add(new BuildingSeed(pos, BuildingType.HousingHut));
            }
        }

        private static Vector2 FindValidSpot(
            Vector2 origin,
            BuildingType type,
            List<Building> buildings,
            List<BuildingSeed> seeds,
            System.Random rng)
        {
            int w = GetFootprintW(type);
            int h = GetFootprintH(type);

            for (int attempt = 0; attempt < 10; attempt++)
            {
                // Base distance plus half the footprint to avoid overlap with
                // existing larger buildings
                float dist = rng.Next(6, 11) + System.MathF.Max(w, h) / 2f;
                float ang = rng.NextFloat(0f, System.MathF.Tau);
                Vector2 pos = origin + new Vector2(System.MathF.Cos(ang), System.MathF.Sin(ang)) * dist;
                if (!Collides(pos, w, h, buildings, seeds))
                    return pos;
            }

            return origin;
        }

        private static bool Collides(
            Vector2 pos,
            int w,
            int h,
            List<Building> buildings,
            List<BuildingSeed> seeds)
        {
            foreach (var b in buildings)
            {
                int bw = GetFootprintW(b.Kind);
                int bh = GetFootprintH(b.Kind);
                if (Overlaps(pos, w, h, b.Pos, bw, bh))
                    return true;
            }

            foreach (var s in seeds)
            {
                if (Overlaps(pos, w, h, s.Pos, s.FootprintW, s.FootprintH))
                    return true;
            }

            return false;
        }

        private static bool Overlaps(Vector2 aPos, int aw, int ah, Vector2 bPos, int bw, int bh)
        {
            return System.MathF.Abs(aPos.X - bPos.X) * 2 < (aw + bw) &&
                   System.MathF.Abs(aPos.Y - bPos.Y) * 2 < (ah + bh);
        }

        private static int GetFootprintW(BuildingType kind)
            => kind switch
            {
                BuildingType.StorageHut => Utils.Constants.HUT_FOOTPRINT_W,
                BuildingType.HousingHut => Utils.Constants.HUT_FOOTPRINT_W,
                BuildingType.CarpenterHut => Utils.Constants.HUT_FOOTPRINT_W,
                _ => 1
            };

        private static int GetFootprintH(BuildingType kind)
            => kind switch
            {
                BuildingType.StorageHut => Utils.Constants.HUT_FOOTPRINT_H,
                BuildingType.HousingHut => Utils.Constants.HUT_FOOTPRINT_H,
                BuildingType.CarpenterHut => Utils.Constants.HUT_FOOTPRINT_H,
                _ => 1
            };

        public static void OnBuildComplete()
        {
            BuiltCount++;
        }
    }
}
