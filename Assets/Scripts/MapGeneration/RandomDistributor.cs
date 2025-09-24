using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DunGen
{
    public class RandomDistributor : RoomDistributor
    {
        public override List<MapRoom> GenerateRooms(MapData map, int roomCount)
        {
            List<MapRoom> rooms = new List<MapRoom>();

            // Pick random squares to act as primary rooms
            List<MapTile> options = new List<MapTile>(map.Map);
            options.Shuffle();

            for (int i = 0; i < roomCount; i++)
            {
                MapTile option = options[0];
                options.RemoveAt(0);

                int roomWidth = Random.Range(2, 4);
                int roomHeight = Random.Range(2, 4);

                if (roomWidth > map.Width - option.X)
                    roomWidth = map.Width - option.X;
                if (roomHeight > map.Height - option.Y)
                    roomHeight = map.Height - option.Y;

                if (roomWidth < 2 || roomHeight < 2)
                    continue;

                List<MapTile> placementArea;
                if (map.IsAreaUnoccupied(roomWidth, roomHeight, option, out placementArea))
                {
                    rooms.Add(PlacePrimaryRoom(placementArea));
                }
            }

            return rooms;
        }
    }
}