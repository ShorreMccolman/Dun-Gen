using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DunGen
{
    public abstract class RoomDistributor
    {
        public abstract List<MapRoom> GenerateRooms(MapData map, int roomCount);

        protected MapRoom PlacePrimaryRoom(List<MapTile> area)
        {
            int roomID = MapGenerator.NextRoomID;
            foreach (var tile in area)
            {
                tile.UpdateCellType(ECellType.PrimaryRoom);
                tile.UpdateRoomID(roomID);
            }

            return new MapRoom(area, roomID);
        }
    }
}