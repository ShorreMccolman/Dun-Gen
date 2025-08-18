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

        public void BuildAndLaunch(GenerationSettings settings)
        {
            MapData mapData = _generator.GenerateInstant(settings);

            SpawnDungeonTiles(mapData.Map);
            SpawnPlayer(mapData.Entrance, mapData.StartingDirection);
        }

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

        public void SpawnPlayer(MapTile entrance, ECardinal direction)
        {
            GameObject player = Instantiate(PlayerObject, new Vector3(entrance.X, 0, -entrance.Y), Quaternion.identity);
            FPSPlayer controller = player.GetComponent<FPSPlayer>();
        }
    }
}