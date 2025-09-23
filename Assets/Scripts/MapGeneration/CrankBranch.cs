using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DunGen
{
    public class CrankBranch : BranchGenerator
    {
        public override bool Create(MapData map, MapTile startingCell)
        {
            List<MapTile> toUpdate = new List<MapTile>();

            MapTile current = startingCell;
            MapTile neighbor = startingCell.GetRandomUnoccupiedNeighbor(map);

            if (neighbor == null)
            {
                return false;
            }

            toUpdate.Add(current);
            toUpdate.Add(neighbor);

            int xDiff = current.X - neighbor.X;
            int yDiff = current.Y - neighbor.Y;

            ECardinal direction = Cardinals.FromCoordinateDifference(xDiff, yDiff);
            bool connected = false;

            bool clockwise = Random.Range(0, 2) == 0;

            while (neighbor != null)
            {
                if (!connected)
                {
                    neighbor.UpdateCellType(ECellType.Hallway);
                }

                current.ConnectCardinally(direction);
                neighbor.ConnectCardinally(Cardinals.Flip(direction));

                if (connected)
                    break;

                if (Random.Range(0, 4) == 0)
                {
                    direction = Cardinals.Rotate(direction, clockwise);
                }

                current = neighbor;
                neighbor = current.GetTileInDirection(map, direction);

                if (neighbor != null)
                {
                    if (neighbor.CellType == ECellType.Invalid)
                    {
                        neighbor = null;
                    }
                    else
                    {
                        toUpdate.Add(neighbor);
                        if (current.CanConnectToTile(neighbor))
                        {
                            connected = true;
                        }
                    }
                }
            }

            foreach (var cell in toUpdate)
                cell.UpdateTileIDByConnectedNeighbors(map);

            return true;
        }
    }
}