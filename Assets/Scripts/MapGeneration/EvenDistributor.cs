using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DunGen
{
    public class EvenDistributor : RoomDistributor
    {
        public override List<MapRoom> GenerateRooms(MapData map, int roomCount)
        {
            float rat = (float)map.Width / (float)map.Height;   
            float diff = Mathf.Sqrt(roomCount);
            int rounded = Mathf.FloorToInt(diff);
            int xCount = Mathf.RoundToInt(rounded * rat);
            int yCount = Mathf.RoundToInt(rounded);

            int xCellWidth = map.Width / xCount;
            int yCellWidth = map.Height / yCount;

            List<List<MapTile>> subregions = new List<List<MapTile>>();
            for(int i=0;i<xCount;i++)
            {
                for(int j=0;j<yCount;j++)
                {
                    subregions.Add(map.GetSubregion(i * xCellWidth, j * yCellWidth, xCellWidth, yCellWidth));
                }
            }
            subregions.Shuffle();

            List<MapRoom> rooms = new List<MapRoom>();
            for (int n = 0; n < subregions.Count; n++)
            {
                List<MapTile> options = new List<MapTile>(subregions[n]);
                options.Shuffle();

                while (options.Count > 0)
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
                        break;
                    }
                }
            }

            return rooms;
        }
    }
}