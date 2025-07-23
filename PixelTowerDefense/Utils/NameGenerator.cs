using System;

namespace PixelTowerDefense.Utils
{
    public static class NameGenerator
    {
        static readonly string[] FIRST =
        {
            "Alrik", "Beppo", "Ciri", "Dorn", "Elda",
            "Fenn", "Gala", "Hilda", "Ivor", "Jora"
        };

        static readonly string[] LAST =
        {
            "Bright", "Ash", "Storm", "Silk", "Hill",
            "Stone", "Vale", "Wick", "Hart", "Frost"
        };

        public static string RandomName(Random rng)
            => $"{FIRST[rng.Next(FIRST.Length)]} {LAST[rng.Next(LAST.Length)]}";
    }
}
