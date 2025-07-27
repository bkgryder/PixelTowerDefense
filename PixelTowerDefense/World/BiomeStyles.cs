using Microsoft.Xna.Framework;

namespace PixelTowerDefense.World
{
    public struct BiomeStyle
    {
        public Color Base;
        public Color Shade;
        public int PatternId;
        public float ShadePct;
        public Color Detail;
    }

    public static class BiomeStyles
    {
        public static BiomeStyle Get(Biome biome)
            => biome switch
            {
                Biome.Grass => new BiomeStyle
                {
                    Base = new Color(70, 160, 70),
                    Shade = new Color(30, 80, 30),
                    PatternId = 0,
                    ShadePct = 0.5f,
                    Detail = new Color(90, 180, 90)
                },
                Biome.Meadow => new BiomeStyle
                {
                    Base = new Color(120, 180, 90),
                    Shade = new Color(70, 120, 60),
                    PatternId = 1,
                    ShadePct = 0.4f,
                    Detail = new Color(150, 200, 110)
                },
                Biome.Forest => new BiomeStyle
                {
                    Base = new Color(40, 120, 40),
                    Shade = new Color(20, 60, 20),
                    PatternId = 0,
                    ShadePct = 0.6f,
                    Detail = new Color(60, 140, 60)
                },
                Biome.Marsh => new BiomeStyle
                {
                    Base = new Color(40, 100, 70),
                    Shade = new Color(20, 50, 40),
                    PatternId = 2,
                    ShadePct = 0.5f,
                    Detail = new Color(60, 120, 90)
                },
                Biome.Sand => new BiomeStyle
                {
                    Base = new Color(194, 178, 128),
                    Shade = new Color(150, 140, 90),
                    PatternId = 3,
                    ShadePct = 0.3f,
                    Detail = new Color(220, 200, 150)
                },
                Biome.Rock => new BiomeStyle
                {
                    Base = new Color(110, 110, 120),
                    Shade = new Color(70, 70, 80),
                    PatternId = 1,
                    ShadePct = 0.6f,
                    Detail = new Color(130, 130, 140)
                },
                Biome.Snow => new BiomeStyle
                {
                    Base = new Color(235, 235, 235),
                    Shade = new Color(180, 180, 180),
                    PatternId = 2,
                    ShadePct = 0.4f,
                    Detail = new Color(255, 255, 255)
                },
                _ => new BiomeStyle
                {
                    Base = Color.Magenta,
                    Shade = Color.Black,
                    PatternId = 0,
                    ShadePct = 0.5f,
                    Detail = Color.Magenta
                }
            };
    }
}
