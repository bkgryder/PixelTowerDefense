using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

#nullable enable

namespace PixelTowerDefense.World
{
    /// <summary>
    /// Basic grid A* pathfinding using water depth as traversal cost.
    /// </summary>
    public static class Pathfinder
    {
        private class Node
        {
            public Point Pos;
            public float G;
            public float F;
            public Node? Parent;
        }

        private static readonly Point[] Neighbors =
        {
            new Point(1,0), new Point(-1,0), new Point(0,1), new Point(0,-1)
        };

        public static List<Point>? FindPath(WaterMap water, Point start, Point goal,
                                            float baseCost = 1f, float wadePenalty = 1f)
        {
            var open = new List<Node>();
            var closed = new HashSet<Point>();
            var startNode = new Node { Pos = start, G = 0f, F = Heuristic(start, goal) };
            open.Add(startNode);

            while (open.Count > 0)
            {
                open.Sort((a, b) => a.F.CompareTo(b.F));
                var current = open[0];
                open.RemoveAt(0);

                if (current.Pos == goal)
                    return Reconstruct(current);

                closed.Add(current.Pos);

                foreach (var off in Neighbors)
                {
                    var np = current.Pos + off;
                    if (np.X < 0 || np.Y < 0 || np.X >= water.Width || np.Y >= water.Height)
                        continue;
                    if (closed.Contains(np))
                        continue;

                    float cellCost = PathCost.ForCell(water, np.X, np.Y, baseCost, wadePenalty);
                    if (float.IsInfinity(cellCost))
                        continue;

                    float tentativeG = current.G + cellCost;
                    var existing = open.Find(n => n.Pos == np);
                    if (existing == null)
                    {
                        existing = new Node { Pos = np };
                        open.Add(existing);
                    }

                    if (tentativeG < existing.G || existing.Parent == null)
                    {
                        existing.G = tentativeG;
                        existing.F = tentativeG + Heuristic(np, goal);
                        existing.Parent = current;
                    }
                }
            }

            return null;
        }

        private static float Heuristic(Point a, Point b)
            => MathF.Abs(a.X - b.X) + MathF.Abs(a.Y - b.Y);

        private static List<Point> Reconstruct(Node node)
        {
            var path = new List<Point>();
            Node? cur = node;
            while (cur != null)
            {
                path.Add(cur.Pos);
                cur = cur.Parent;
            }
            path.Reverse();
            return path;
        }
    }
}
