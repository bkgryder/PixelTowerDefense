using System.Collections.Generic;

namespace PixelTowerDefense.Entities
{
    public static class BuildingSprites
    {
        public static readonly Dictionary<(BuildingType, BuildStage), string[]> Sprites = new()
        {
            { (BuildingType.StorageHut, BuildStage.Planned), new[]
                {
                    "...",
                    ".p.",
                    "p.p",
                    ".p."
                }
            },
            { (BuildingType.StorageHut, BuildStage.Framed), new[]
                {
                    "...",
                    "fff",
                    "f.f",
                    "fff"
                }
            },
            { (BuildingType.StorageHut, BuildStage.Built), new[]
                {
                    ".r.",
                    "www",
                    "w.w",
                    "www"
                }
            },
            { (BuildingType.HousingHut, BuildStage.Planned), new[]
                {
                    "...",
                    ".p.",
                    "p.p",
                    ".p."
                }
            },
            { (BuildingType.HousingHut, BuildStage.Framed), new[]
                {
                    "...",
                    "fff",
                    "f.f",
                    "fff"
                }
            },
            { (BuildingType.HousingHut, BuildStage.Built), new[]
                {
                    ".r.",
                    "www",
                    "w.w",
                    "www"
                }
            }
        };
    }
}
