using Microsoft.Xna.Framework;

namespace PixelTowerDefense.Entities
{
    public enum BuildStage
    {
        Planned,
        Framed,
        Built
    }

    public struct BuildingSeed
    {
        public Vector2 Pos;
        public BuildStage Stage;
        public int RequiredResources;
        public int? ReservedBy;
    }
}
