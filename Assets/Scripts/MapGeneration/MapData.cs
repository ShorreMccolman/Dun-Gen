using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DunGen
{
    public class MapData
    {
        public List<MapTile> Map { get; private set; }
        public MapTile Entrance { get; private set; }
        public ECardinal StartingDirection { get; private set; }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public MapData(List<MapTile> tiles, int width, int height)
        {
            Map = tiles;
            Entrance = null;
            StartingDirection = ECardinal.None;
            Width = width;
            Height = height;
        }

        public MapData(List<MapTile> tiles, MapTile entranceTile, ECardinal startingDir, int width, int height)
        {
            Map = tiles;
            Entrance = entranceTile;
            StartingDirection = startingDir;
            Width = width;
            Height = height;
        }

        public void SetEntrance(MapTile tile, ECardinal direction)
        {
            Entrance = tile;
            StartingDirection = direction;
        }

        public int GetIndex(int x, int y)
        {
            return x + y * Width;
        }

        public MapTile GetTile(int x, int y)
        {
            if (x < 0 || x > Width - 1 || y < 0 || y > Height - 1)
            {
                return null;
            }

            return Map[GetIndex(x, y)];
        }

        public void ConnectNeighborsAndUpdate()
        {
            foreach (var tile in Map)
            {
                tile.ConnectToOccupiedNeighbors(this);
            }
            foreach (var tile in Map)
            {
                tile.UpdateTileIDByConnectedNeighbors(this);
            }
        }

        public void UpdateTilesProximally()
        {
            foreach (var tile in Map)
                tile.UpdateTileProximity(Map, Width, Height);
        }

        public bool IsAreaUnoccupied(int width, int height, MapTile startingTile, out List<MapTile> area)
        {
            area = new List<MapTile>();

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    MapTile tile = GetTile(startingTile.X + j, startingTile.Y + i);
                    if (!tile.IsAvailable)
                    {
                        return false;
                    }

                    area.Add(tile);
                }
            }
            return true;
        }

        public List<MapTile> GetSubregion(int x, int y, int width, int height)
        {
            List<MapTile> tiles = new List<MapTile>();
            for(int i=0; i < height; i++)
            {
                for(int j=0; j < width; j++)
                {
                    MapTile tile = GetTile(x + j, y + i);
                    if(tile != null)
                    {
                        tiles.Add(tile);
                    }
                }
            }

            return tiles;
        }
    }
}