using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DunGen
{
    public class ShootBranch : BranchGenerator
    {
        public override bool Create(MapData map, MapTile startingCell)
        {
            int stepCount = Random.Range(2, 10);

            MapTile current = startingCell;
            MapTile neighbor = startingCell.GetRandomUnoccupiedNeighbor(map);

            if (neighbor == null)
            {
                return false;
            }

            int xDiff = current.X - neighbor.X;
            int yDiff = current.Y - neighbor.Y;

            ECardinal direction = Cardinals.FromCoordinateDifference(xDiff, yDiff);

            while (neighbor != null && stepCount > 0)
            {
                neighbor.UpdateCellType(ECellType.Hallway);

                current.ConnectCardinally(direction);
                current.UpdateTileIDByConnectedNeighbors(map);
                neighbor.ConnectCardinally(Cardinals.Flip(direction));
                neighbor.UpdateTileIDByConnectedNeighbors(map);

                current = neighbor;
                neighbor = current.GetUnoccupiedTileInDirection(map, direction);
                stepCount--;
            }

            return true;
        }
    }
}