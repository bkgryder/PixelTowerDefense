using System;

namespace PixelTowerDefense
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Configuration.Initialize();
            GameSystems.Initialize();

            using var game = new Game1();
            game.Run();
        }
    }
}

