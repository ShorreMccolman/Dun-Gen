using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DunGen
{
    public struct MapData
    {
        public List<MapTile> Map { get; private set; }
        public MapTile Entrance { get; private set; }
        public ECardinal StartingDirection { get; private set; }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public MapData(List<MapTile> tiles, MapTile entranceTile, ECardinal startingDir, int width, int height)
        {
            Map = tiles;
            Entrance = entranceTile;
            StartingDirection = startingDir;
            Width = width;
            Height = height;
        }

        public MapTile GetTile(int x, int y)
        {
            return MapTile.GetTileByPosition(Map, x, y, Width, Height);
        }
    }

    [RequireComponent(typeof(MapGenerator))]
    public class DungeonBuilder : MonoBehaviour
    {
        public static DungeonBuilder Instance => _instance;
        static DungeonBuilder _instance;
        private void Awake() { _instance = this; }

        [SerializeField] GameObject PlayerObject;
        [SerializeField] DungeonTileData TileSet;
        [SerializeField] GameMap GameMap;

        MapGenerator _generator;

        private void Start()
        {
            _generator = GetComponent<MapGenerator>();
        }

        //
        // Generate the map, construct the 3D dungeon and then spawn player based on chosen settings
        //
        public void BuildAndLaunch(GenerationSettings settings)
        {
            MapData mapData = _generator.GenerateInstant(settings);

            SpawnDungeonTiles(mapData);
            IDGPlayerController player = SpawnPlayer(mapData.Entrance, mapData.StartingDirection);

            GameMap.Initialize(player, mapData);
        }

        //
        // This spawns in our 3D dungeon matching the generated map tiles. Uses the chosen 3D tile set objects
        // TODO: Add the ability to set the tile set (not currently neccesary since we only have one tile set at the moment)
        //
        public void SpawnDungeonTiles(MapData data)
        {
            foreach(var premade in TileSet.PremadeTiles)
            {
                PremadeTile tile = premade.GetComponent<PremadeTile>();
                int index = FindMatchingTileGroup(data, tile);
                if(index >= 0)
                {
                    // TODO: Replace this section of the dungeon with the premade tile and flag
                }
            }

            foreach (var square in data.Map)
            {
                if (square.CellType != ECellType.Unoccupied)
                {
                    GameObject go = Instantiate(TileSet.GetTile(square.TileID), transform);
                    go.transform.SetPositionAndRotation(new Vector3(square.X, 0, -square.Y), Quaternion.identity);
                }
            }
        }

        //
        //  This spawns our player object facing in a given direction
        //  TODO: Add the ability to set the player object. Depending on which control styles we want to support on release we may need to
        //  initialize a bunch of game mode specific settings and eventually make it generic enough to support whatever player controller the
        //  user has set up for their game. Right now we're just worrying about supporting our demo controllers so that user specific stuff
        //  can be handled later on.
        //
        public IDGPlayerController SpawnPlayer(MapTile entrance, ECardinal direction)
        {
            GameObject go = Instantiate(PlayerObject, new Vector3(entrance.X, 0, -entrance.Y), Quaternion.identity);
            IDGPlayerController controller = go.GetComponent<IDGPlayerController>();
            return controller;
        }

        //
        // Pattern recognition algorithm for matching map data with a premade tile layout
        //
        public int FindMatchingTileGroup(MapData data, PremadeTile tile)
        {
            int[] ids = tile.GetGridIDs();

            for(int i=0; i<data.Width; i++)
            {
                for(int j = 0; j<data.Height; i++)
                {
                    // Only need to compare if the premade tile can fit at current index (disregard indexes too close to the right or bottom edge)
                    if(data.Width - i >= tile.Width && data.Height - j >= tile.Height)
                    {
                        int currentIndex = i + j * data.Width;
                        bool isMatch = ScanForMatch(data, currentIndex, ids, tile.Width, tile.Height);
                        if(isMatch)
                        {
                            return currentIndex;
                        }
                    }
                }
            }

            return -1;
        }

        //
        // Compare a premade tile with a same size chunk of the full map
        //
        bool ScanForMatch(MapData mapData, int startingIndex, int[] ids, int tileWidth, int tileHeight)
        {
            for (int i = 0; i < tileWidth; i++)
            {
                for (int j = 0; j < tileHeight; j++)
                {
                    int tileIndex = i + j * tileWidth;                          // current index for the tile itself
                    int mapIndex = startingIndex + i + j * mapData.Width;       // matching index for the chunk of the full map we are comparing to

                    if (ids[tileIndex] != mapData.Map[mapIndex].TileID)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}