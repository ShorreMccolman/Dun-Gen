using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DunGen
{
    public class Pathfinding
    {
        public class PathNode
        {
            public MapTile Square;
            public List<PathNode> Adjacent = new List<PathNode>();

            public PathNode Parent;
            public float g, h, f;

            public bool IsBlackedOut;

            public PathNode(MapTile square)
            {
                Square = square;
            }

            public void AddAdjacent(PathNode square)
            {
                Adjacent.Add(square);
            }
        }

        List<PathNode> Grid;

        List<PathNode> Open;
        List<PathNode> Closed;

        // 
        // Initialize pathfinding nodes using a grid of map tiles
        //
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

        //
        // Typical A* pathfinding algorithm
        //
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

            while (Open.Count > 0)
            {
                PathNode current = Open[0];

                if (current.Square == target)
                {
                    return ConstructPath(current);
                }

                Open.Remove(current);
                Closed.Add(current);

                foreach (var neighbor in current.Adjacent)
                {
                    if (Closed.Contains(neighbor) || neighbor.IsBlackedOut)
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

            Debug.LogError("No path from " + start.X + ":" + start.Y + " to " + target.X + ":" + target.Y);
            return null;
        }

        //
        // Since no diagonal movement across the grid is allowed we can use the Manhattan distance for our heuristic, at least for now this is our best option
        //
        public float EvaluateH(MapTile current, MapTile next)
        {
            return Mathf.Abs(current.X - next.X) + Mathf.Abs(current.Y - next.Y);
        }

        //
        // Helper function for finding the nearest tile from a list of given options. Just a one off operation so no need to sort
        //
        public MapRoom FindNearestNode(MapRoom current, List<MapRoom> options)
        {
            MapRoom best = null;
            float bestDistance = float.MaxValue;

            MapTile anchor = current.GetAnchorTile();

            foreach (var option in options)
            {
                if (option == current)
                    continue;

                MapTile target = option.GetAnchorTile();
                float distance = EvaluateH(anchor, target);
                if (distance < bestDistance)
                {
                    best = option;
                    bestDistance = distance;
                }
            }
            return best;
        }

        //
        // Returns the ordered list of map tiles that will form our path
        //
        List<MapTile> ConstructPath(PathNode endNode)
        {
            List<MapTile> path = new List<MapTile>();

            PathNode next = endNode;
            while (next != null)
            {
                if (next.Square.CellType != ECellType.PrimaryRoom)
                {
                    path.Insert(0, next.Square);
                }
                next = next.Parent;
            }

            return path;
        }

        public void RemoveCellsFromGrid(List<MapTile> tiles)
        {
            List<PathNode> removed = Grid.FindAll(x => tiles.Contains(x.Square));
            foreach(var node in removed)
            {
                node.IsBlackedOut = true;
            }
        }
    }
}