using System.Collections.Generic;

namespace PixelTowerDefense.Entities
{
    public static class BuildingSprites
    {
        public static readonly Dictionary<(BuildingType, BuildStage), string[]> Sprites = new()
        {
            { (BuildingType.StorageHut, BuildStage.Planned), new[]
                {
                    "pppppppppp",
                    "p........p",
                    "p........p",
                    "p........p",
                    "p........p",
                    "p........p",
                    "p........p",
                    "p........p",
                    "p........p",
                    "pppppppppp"
                }
            },
            { (BuildingType.StorageHut, BuildStage.Framed), new[]
                {
                    "ffffffffff",
                    "f........f",
                    "f........f",
                    "ffffffffff",
                    "f........f",
                    "f........f",
                    "ffffffffff",
                    "f........f",
                    "f........f",
                    "ffffffffff"
                }
            },
            { (BuildingType.StorageHut, BuildStage.Built), new[]
                {
                    ".rrrrrrrr.",
                    "wwwwwwwwww",
                    "wwwwwwwwww",
                    "wwwwwwwwww",
                    "wwwwwwwwww",
                    "www....www",
                    "www....www",
                    "www....www",
                    "www....www",
                    "wwwwwwwwww"
                }
            },
            { (BuildingType.HousingHut, BuildStage.Planned), new[]
                {
                    "pppppppppp",
                    "p........p",
                    "p........p",
                    "p........p",
                    "p........p",
                    "p........p",
                    "p........p",
                    "p........p",
                    "p........p",
                    "pppppppppp"
                }
            },
            { (BuildingType.HousingHut, BuildStage.Framed), new[]
                {
                    "ffffffffff",
                    "f........f",
                    "f........f",
                    "ffffffffff",
                    "f........f",
                    "f........f",
                    "ffffffffff",
                    "f........f",
                    "f........f",
                    "ffffffffff"
                }
            },
            { (BuildingType.HousingHut, BuildStage.Built), new[]
                {
                    ".rrrrrrrr.",
                    "wwwwwwwwww",
                    "wwwwwwwwww",
                    "wwwwwwwwww",
                    "wwwwwwwwww",
                    "wwww..wwww",
                    "wwww..wwww",
                    "wwww..wwww",
                    "wwww..wwww",
                    "wwwwwwwwww"
                }
            }
        };
    }
}
