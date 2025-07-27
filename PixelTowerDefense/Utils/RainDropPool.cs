using System.Collections.Generic;
using PixelTowerDefense.Entities;

namespace PixelTowerDefense.Utils
{
    public static class RainDropPool
    {
        public static void Spawn(this List<RainDrop> drops, RainDrop drop)
        {
            if (drops.Count >= Constants.MAX_RAIN_DROPS && drops.Count > 0)
                drops.RemoveAt(0);
            drops.Add(drop);
        }
    }
}
