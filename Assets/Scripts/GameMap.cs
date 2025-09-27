using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DunGen
{
    public class GameMap : MonoBehaviour
    {
        MapData _map;
        IDGPlayerController _player;

        Transform _playerTransform;
        MapTile _activeTile;
        float _tileSize;

        public void Initialize(IDGPlayerController player, MapData map, float tileSize)
        {
            _map = map;
            _player = player;
            _playerTransform = _player.GetTransform();
            _tileSize = tileSize;
        }

        private void Update()
        {
            if(_player != null)
            {
                MapTile activeTile = DetermineActiveTile();
                if(activeTile != _activeTile)
                {
                    if(_activeTile != null)
                    {
                        _activeTile.ColorTile(Color.white);
                    }

                    _activeTile = activeTile;
                    activeTile.ColorTile(Color.green);
                }
            }
        }

        public MapTile DetermineActiveTile()
        {
            float xPos = _playerTransform.position.x;
            float yPos = -_playerTransform.position.z;

            MapTile tile = _map.GetTile(Mathf.RoundToInt(xPos / _tileSize), Mathf.RoundToInt(yPos / _tileSize));

            if(tile == null)
            {
                Debug.LogError("Player out of bounds!!!");
            }

            return tile;
        }
    }
}
