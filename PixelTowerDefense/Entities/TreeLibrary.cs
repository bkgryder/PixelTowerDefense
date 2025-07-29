using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace PixelTowerDefense.Entities
{
    public enum TreeSpecies { Oak, Pine, Willow, Birch, Deadwood }

    public struct TreeGenotype
    {
        public int Seed;
        public float HeightBias, RadiusBias, LeanBias, BranchProbBias, LeafDensityBias;
        public float HueShift;
    }

    public sealed class TreeArchetype
    {
        public TreeSpecies Species;
        public int HeightMin, HeightMax;
        public int BaseWidthMin, BaseWidthMax;
        public int LeafRadiusMin, LeafRadiusMax;

        public float LeanMin, LeanMax;
        public float BranchSplitProb;
        public Point BranchLenRange;
        public float CanopyEllipseRatio;
        public float LeafKeepProb;

        public Color TrunkBase, TrunkTip;
        public Color LeafA, LeafB;
    }

    public static class TreeLibrary
    {
        public static readonly TreeArchetype Oak = new()
        {
            Species = TreeSpecies.Oak,
            HeightMin = 22,
            HeightMax = 34,
            BaseWidthMin = 2,
            BaseWidthMax = 3,
            LeafRadiusMin = 6,
            LeafRadiusMax = 9,
            LeanMin = -0.15f,
            LeanMax = 0.15f,
            BranchSplitProb = 0.18f,
            BranchLenRange = new Point(6, 12),
            CanopyEllipseRatio = 2.0f,
            LeafKeepProb = 0.85f,
            TrunkBase = new Color(80, 60, 40),
            TrunkTip = new Color(120, 95, 70),
            LeafA = new Color(28, 92, 36),
            LeafB = new Color(18, 70, 26),
        };

        public static readonly TreeArchetype Pine = new()
        {
            Species = TreeSpecies.Pine,
            HeightMin = 28,
            HeightMax = 48,
            BaseWidthMin = 2,
            BaseWidthMax = 3,
            LeafRadiusMin = 4,
            LeafRadiusMax = 7,
            LeanMin = -0.05f,
            LeanMax = 0.10f,
            BranchSplitProb = 0.10f,
            BranchLenRange = new Point(8, 16),
            CanopyEllipseRatio = 1.3f,
            LeafKeepProb = 0.92f,
            TrunkBase = new Color(70, 55, 40),
            TrunkTip = new Color(110, 85, 60),
            LeafA = new Color(22, 80, 40),
            LeafB = new Color(14, 56, 30),
        };

        public static readonly TreeArchetype Birch = new()
        {
            Species = TreeSpecies.Birch,
            HeightMin = 20,
            HeightMax = 30,
            BaseWidthMin = 1,
            BaseWidthMax = 2,
            LeafRadiusMin = 5,
            LeafRadiusMax = 7,
            LeanMin = -0.10f,
            LeanMax = 0.20f,
            BranchSplitProb = 0.15f,
            BranchLenRange = new Point(5, 10),
            CanopyEllipseRatio = 1.7f,
            LeafKeepProb = 0.80f,
            TrunkBase = new Color(200, 200, 200),
            TrunkTip = new Color(240, 240, 240),
            LeafA = new Color(40, 120, 40),
            LeafB = new Color(28, 96, 28),
        };
    }
}
