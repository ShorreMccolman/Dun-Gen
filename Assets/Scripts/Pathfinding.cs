using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DunGen
{
    public class PathNode
    {
        public MapTile Square;
        public List<PathNode> Adjacent = new List<PathNode>();

        public PathNode Parent;
        public float g, h, f;

        public PathNode(MapTile square)
        {
            Square = square;
        }

        public void AddAdjacent(PathNode square)
        {
            Adjacent.Add(square);
        }
    }

    public class Pathfinding
    {
        List<PathNode> Grid;

        List<PathNode> Open;
        List<PathNode> Closed;

        public void CreateNetwork(int width, int height, List<MapTile> grid)
        {
            Grid = new List<PathNode>();
            foreach (var cell in grid)
            {
                Grid.Add(new PathNode(cell));
            }

            for (int i = 0; i < Grid.Count; i++)
            {
                PathNode node = Grid[i];
                MapTile cell = node.Square;

                if (cell.X > 0)
                {
                    node.AddAdjacent(Grid[i - 1]);
                }
                if (cell.X < width - 1)
                {
                    node.AddAdjacent(Grid[i + 1]);
                }
                if (cell.Y > 0)
                {
                    node.AddAdjacent(Grid[i - width]);
                }
                if (cell.Y < height - 1)
                {
                    node.AddAdjacent(Grid[i + width]);
                }
            }
        }

        public List<MapTile> FindPath(MapTile start, MapTile target)
        {
            Open = new List<PathNode>();
            Closed = new List<PathNode>();

            foreach (var existing in Grid)
            {
                existing.f = 0;
                existing.g = 0;
                existing.h = 0;
                existing.Parent = null;
            }

            PathNode node = Grid.Find(x => x.Square == start);
            Open.Add(node);

            node.g = 0;
            node.h = EvaluateH(start, target);
            node.f = node.g + node.h;

            int iteration = 0;

            while (Open.Count > 0 && iteration < 100)
            {
                PathNode current = Open[0];

                iteration++;

                if (current.Square == target)
                {
                    return ConstructPath(current);
                }

                Open.Remove(current);
                Closed.Add(current);

                foreach (var neighbor in current.Adjacent)
                {
                    if (Closed.Contains(neighbor))
                    {
                        continue;
                    }

                    float tentative = current.g + 1;

                    bool shouldAdd = false;
                    if (!Open.Contains(neighbor))
                    {
                        shouldAdd = true;
                    }
                    else if (tentative >= neighbor.g)
                    {
                        continue;
                    }

                    neighbor.Parent = current;
                    neighbor.g = tentative;
                    neighbor.h = EvaluateH(neighbor.Square, target);
                    neighbor.f = neighbor.g + neighbor.h;

                    if (shouldAdd)
                    {
                        int index = Open.Count;
                        for (int i = 0; i < Open.Count; i++)
                        {
                            if (neighbor.f < Open[i].f)
                            {
                                index = i;
                                break;
                            }
                        }

                        Open.Insert(index, neighbor);
                    }
                }
            }

            return null;
        }

        public MapTile FindNearestNode(MapTile current, List<MapTile> options)
        {
            MapTile best = null;
            float bestDistance = float.MaxValue;

            foreach (var option in options)
            {
                if (option == current)
                    continue;

                float distance = EvaluateH(current, option);
                if (distance < bestDistance)
                {
                    best = option;
                    bestDistance = distance;
                }
            }
            return best;
        }

        public float EvaluateH(MapTile current, MapTile next)
        {
            return Mathf.Abs(current.X - next.X) + Mathf.Abs(current.Y - next.Y);
        }

        List<MapTile> ConstructPath(PathNode endNode)
        {
            List<MapTile> path = new List<MapTile>();

            PathNode next = endNode;
            while (next != null)
            {
                if (next.Square.CellType != ECellType.PrimaryRoom)
                {
                    path.Insert(0, next.Square);
                    next.Square.UpdateCellType(ECellType.Hallway);
                }
                next = next.Parent;
            }

            return path;
        }
    }
}