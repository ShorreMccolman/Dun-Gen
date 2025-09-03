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

        public void Initialize(IDGPlayerController player, MapData map)
        {
            _map = map;
            _player = player;
            _playerTransform = _player.GetTransform();
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

            MapTile tile = _map.GetTile(Mathf.RoundToInt(xPos), Mathf.RoundToInt(yPos));

            if(tile == null)
            {
                Debug.LogError("Player out of bounds!!!");
            }

            return tile;
        }
    }
}
