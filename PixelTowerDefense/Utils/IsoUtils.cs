using Microsoft.Xna.Framework;

namespace PixelTowerDefense.Utils
{
    public static class IsoUtils
    {
        public static Vector2 ToIso(Vector2 cart, int tileW, int tileH)
            => new Vector2(
                (cart.X - cart.Y) * (tileW * 0.5f),
                (cart.X + cart.Y) * (tileH * 0.5f));

        public static Vector2 ToIso(Point cart, int tileW, int tileH)
            => ToIso(new Vector2(cart.X, cart.Y), tileW, tileH);

        public static Vector2 ToIsoDirection(Vector2 cartDir, int tileW, int tileH)
            => ToIso(cartDir, tileW, tileH);

        public static Vector2 ToIsoDirection(Point cartDir, int tileW, int tileH)
            => ToIso(new Vector2(cartDir.X, cartDir.Y), tileW, tileH);

        public static Vector2 ToCart(Vector2 iso, int tileW, int tileH)
            => new Vector2(
                iso.X / tileW + iso.Y / tileH,
                iso.Y / tileH - iso.X / tileW);

        public static Vector2 ToCart(Point iso, int tileW, int tileH)
            => ToCart(new Vector2(iso.X, iso.Y), tileW, tileH);

        public static Vector2 ToCartDirection(Vector2 isoDir, int tileW, int tileH)
            => ToCart(isoDir, tileW, tileH);

        public static Vector2 ToCartDirection(Point isoDir, int tileW, int tileH)
            => ToCart(new Vector2(isoDir.X, isoDir.Y), tileW, tileH);
    }
}
