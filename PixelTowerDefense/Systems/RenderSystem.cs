using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PixelTowerDefense.Components;
using PixelTowerDefense;

namespace PixelTowerDefense.Systems
{
    public class RenderSystem : System.IDisposable
    {
        private readonly Texture2D _px;

        public RenderSystem(GraphicsDevice device)
        {
            _px = new Texture2D(device, 1, 1);
            _px.SetData(new[] { Color.White });
        }

        public void Draw(SpriteBatch sb, List<Enemy> enemies, List<Pixel> pixels, float zoom, float camX, float camY)
        {
            var cam = Matrix.CreateScale(zoom, zoom, 1f)
                * Matrix.CreateTranslation(-camX * zoom, -camY * zoom, 0);
            sb.Begin(transformMatrix: cam);

            int thickness = 2;
            sb.Draw(_px, new Rectangle(GameConstants.ArenaLeft, GameConstants.ArenaTop, GameConstants.ArenaRight - GameConstants.ArenaLeft, thickness), Color.DimGray);
            sb.Draw(_px, new Rectangle(GameConstants.ArenaLeft, GameConstants.FloorY, GameConstants.ArenaRight - GameConstants.ArenaLeft, thickness), Color.DimGray);
            sb.Draw(_px, new Rectangle(GameConstants.ArenaLeft, GameConstants.ArenaTop, thickness, GameConstants.FloorY - GameConstants.ArenaTop), Color.DimGray);
            sb.Draw(_px, new Rectangle(GameConstants.ArenaRight - thickness, GameConstants.ArenaTop, thickness, GameConstants.FloorY - GameConstants.ArenaTop), Color.DimGray);

            foreach (var p in pixels) sb.Draw(_px, p.Bounds, p.Col);

            for (int i = 0; i < enemies.Count; i++)
            {
                var e = enemies[i];
                for (int part = -1; part <= 1; part++)
                {
                    Vector2 partPos = e.GetPartPos(part);
                    Color c =
                        part == -1 ? new Color(255, 219, 172) :
                        part == 0 ? e.ShirtColor :
                                     new Color(68, 36, 14);
                    sb.Draw(_px, new Rectangle((int)partPos.X, (int)partPos.Y, 1, 1), c);
                }
            }
            sb.End();
        }

        public void Dispose()
        {
            _px.Dispose();
        }
    }
}
