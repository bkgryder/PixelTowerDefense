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

    public struct ColorPair
    {
        public Color Base;
        public Color Tip;

        public ColorPair(Color baseColor, Color tipColor)
        {
            Base = baseColor;
            Tip = tipColor;
        }
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
        public ColorPair[] TrunkVariations;
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
            TrunkVariations = new[]
            {
                new ColorPair(new Color(70, 50, 35), new Color(110, 85, 60)),
                new ColorPair(new Color(60, 45, 30), new Color(100, 75, 55)),
            },
            LeafA = new Color(28, 92, 36),
            LeafB = new Color(18, 70, 26),
        };

        public static readonly TreeArchetype Pine = new()
        {
            Species = TreeSpecies.Pine,
            HeightMin = 28,
            HeightMax = 48,
            BaseWidthMin = 2,
            BaseWidthMax = 2,
            LeafRadiusMin = 4,
            LeafRadiusMax = 4,
            LeanMin = -0.001f,
            LeanMax = 0.001f,
            BranchSplitProb = 0.55f,
            BranchLenRange = new Point(10, 20),
            CanopyEllipseRatio = 1f,
            LeafKeepProb = 0.99f,
            TrunkBase = new Color(70, 55, 40),
            TrunkTip = new Color(110, 85, 60),
            TrunkVariations = new[]
            {
                new ColorPair(new Color(60, 48, 35), new Color(100, 78, 55)),
                new ColorPair(new Color(80, 62, 45), new Color(120, 95, 70)),
            },
            LeafA = new Color(22, 80, 40),
            LeafB = new Color(14, 56, 30),
        };

        public static readonly TreeArchetype Birch = new()
        {
            Species = TreeSpecies.Birch,
            HeightMin = 20,
            HeightMax = 30,
            BaseWidthMin = 1,
            BaseWidthMax = 1,
            LeafRadiusMin = 5,
            LeafRadiusMax = 7,
            LeanMin = -0.01f,
            LeanMax = 0.01f,
            BranchSplitProb = 0.05f,
            BranchLenRange = new Point(5, 10),
            CanopyEllipseRatio = 0.7f,
            LeafKeepProb = 0.80f,
            TrunkBase = new Color(200, 200, 200),
            TrunkTip = new Color(240, 240, 240),
            TrunkVariations = new[]
            {
                new ColorPair(new Color(220, 220, 220), new Color(255, 255, 255)),
                new ColorPair(new Color(180, 180, 180), new Color(220, 220, 220)),
                new ColorPair(new Color(100, 100, 100), new Color(150, 150, 150)),
            },
            LeafA = new Color(40, 120, 40),
            LeafB = new Color(28, 96, 28),
        };
    }
}
