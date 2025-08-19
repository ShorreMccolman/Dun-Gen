using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DunGen
{
    public struct MapData
    {
        public List<MapTile> Map;
        public MapTile Entrance;
        public ECardinal StartingDirection;
    }

    [RequireComponent(typeof(MapGenerator))]
    public class DungeonBuilder : MonoBehaviour
    {
        public static DungeonBuilder Instance => _instance;
        static DungeonBuilder _instance;
        private void Awake() { _instance = this; }

        [SerializeField] GameObject PlayerObject;
        [SerializeField] DungeonTileData TileSet;

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

            SpawnDungeonTiles(mapData.Map);
            SpawnPlayer(mapData.Entrance, mapData.StartingDirection);
        }

        //
        // This spawns in our 3D dungeon matching the generated map tiles. Uses the chosen 3D tile set objects
        // TODO: Add the ability to set the tile set (not currently neccesary since we only have one tile set at the moment)
        //
        public void SpawnDungeonTiles(List<MapTile> map)
        {
            foreach (var square in map)
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
        public void SpawnPlayer(MapTile entrance, ECardinal direction)
        {
            GameObject player = Instantiate(PlayerObject, new Vector3(entrance.X, 0, -entrance.Y), Quaternion.identity);
            FPSPlayer controller = player.GetComponent<FPSPlayer>();
        }
    }
}