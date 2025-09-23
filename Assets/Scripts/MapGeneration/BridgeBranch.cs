using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DunGen
{
    public class BridgeBranch : BranchGenerator
    {
        public override bool Create(MapData map, MapTile startingCell)
        {
            MapTile current = startingCell;
            MapTile neighbor = startingCell.GetRandomUnoccupiedNeighbor(map);

            if (neighbor == null)
            {
                return false;
            }

            List<MapTile> bridgeTiles = new List<MapTile>() { current, neighbor };
            ECardinal direction = Cardinals.FromCoordinateDifference(current.X - neighbor.X, current.Y - neighbor.Y);

            bool scanning = true;
            while (scanning)
            {
                current = neighbor;
                neighbor = current.GetTileInDirection(map, direction);

                if (neighbor == null || neighbor.CellType == ECellType.Invalid)
                {
                    return false;
                }
                else if (neighbor.CellType == ECellType.Hallway || neighbor.CellType == ECellType.PrimaryRoom)
                {
                    scanning = false;
                }

                bridgeTiles.Add(neighbor);
            }

            for (int i = 0; i < bridgeTiles.Count; i++)
            {
                if (i < bridgeTiles.Count - 1)
                {
                    bridgeTiles[i].ConnectCardinally(direction);
                }
                if (i > 0)
                {
                    bridgeTiles[i].ConnectCardinally(Cardinals.Flip(direction));
                }
                if (i < bridgeTiles.Count - 1 && i > 0)
                {
                    bridgeTiles[i].UpdateCellType(ECellType.Hallway);
                }
            }

            foreach (var tile in bridgeTiles)
            {
                tile.UpdateTileIDByConnectedNeighbors(map);
            }

            return true;
        }
    }
}