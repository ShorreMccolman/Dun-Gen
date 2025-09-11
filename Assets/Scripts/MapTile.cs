using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DunGen
{
    public enum ECellType
    {
        Unoccupied,
        PrimaryRoom,
        PremadeRoom,
        Hallway,
        Border,
        Door,
        Invalid
    }

    public enum ECardinal
    {
        N = 0,
        E = 1,
        S = 2,
        W = 3
    }

    public class MapTile : MonoBehaviour
    {
        [SerializeField] Image BG;

        public int X => _x;
        public int Y => _y;
        int _x, _y;

        public int TileID => _tileID;
        int _tileID = -1;

        public int RoomID => _roomID;
        int _roomID = -1;

        public ECellType CellType => _type;
        ECellType _type;

        public bool IsPremade => _isPremade;
        bool _isPremade;

        public bool IsAvailable => _isAvailable;
        bool _isAvailable;

        //
        // Connections is an array that encodes whether or not neighboring tiles are connected or not.
        // A neighboring tile is not neccesarily connected just because it is occupied, this allows us to determine whether or not
        // a wall should be placed in between them. Neighbors are ordered N E S W
        //
        bool[] _connections;

        //
        // Set initial position in the grid
        //
        public void Init(int x, int y)
        {
            gameObject.name = x + "," + y;
            _x = x;
            _y = y;
            _connections = new bool[4] { false, false, false, false };
            _isAvailable = true;
            _isPremade = false;
        }

        //
        // Cell type determines properties of the cell when spawning the rooms in 3D
        //
        public void UpdateCellType(ECellType type)
        {
            _type = type;
            BG.enabled = type != ECellType.Unoccupied;
            if(type != ECellType.Unoccupied)
            {
                _isAvailable = false;
            }
        }

        //
        // Room ID is used to group tiles together
        //
        public void UpdateRoomID(int id)
        {
            _roomID = id;
        }

        //
        // Tile ID is based on connected neighboring cells and determines the sprite used in the map
        //
        public void UpdateTileID(int id)
        {
            _tileID = id;
            UpdateSprite(SpriteHandler.FetchSprite("Grid/Wall_" + id));
        }

        //
        // Tile color is primarily used for debug purposes
        // TODO: color tile in game based on whether cell has been visited or not
        //
        public void ColorTile(Color color)
        {
            BG.color = color;
        }

        //
        // Determined by Tile ID
        //
        void UpdateSprite(Sprite sprite)
        {
            BG.sprite = sprite;
        }

        //
        // A string used to print the position of the tile, used for logging
        //
        public string DebugName()
        {
            return _x + "," + _y;
        }

        //
        // Returns whether or not a neighbor in a particular cardinal direction is connected to this tile
        //
        bool IsCellConnected(ECardinal direction)
        {
            return _connections[(int)direction];
        }

        //
        // This sets all occupied neighbor tiles to be connected to this tile, used for the early stage of map generation before creating branches
        //
        public void ConnectToOccupiedNeighbors(List<MapTile> grid, int width, int height)
        {
            if (_type == ECellType.Unoccupied)
                return;

            _connections = new bool[4];
            List<MapTile> neighbors = GetOccupiedNeighbors(grid, width, height);

            for (int i = 0; i < 4; i++)
            {
                _connections[i] = neighbors[i] != null;
            }
        }

        //
        // This determines the tile ID based on which neighboring cells are occupied.
        // It uses a bitmap style algorithm which sets the sprite of the tile based on the resulting value
        // 
        //       0 3 5
        //       1 X 6    X is the current cell and the indices refer to particular neighbors based on this diagram
        //       2 4 7
        //
        public void UpdateTileProximity(List<MapTile> grid, int width, int height)
        {
            if (_type == ECellType.Unoccupied)
                return;

            int bitval = 0;

            bool[] comp = new bool[8];
            int k = 0;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0)
                        continue;

                    int newX = _x + i;
                    int newY = _y + j;

                    if (newX < 0 || newX > width - 1 || newY < 0 || newY > height - 1)
                    {
                        comp[k] = false;
                    }
                    else
                    {
                        MapTile neighbor = grid[newX + newY * width];

                        comp[k] = neighbor.CellType != ECellType.Unoccupied;
                    }

                    k++;
                }
            }

            bitval += comp[0] && comp[1] && comp[3] ? 1 : 0;
            bitval += comp[1] ? 2 : 0;
            bitval += comp[2] && comp[1] && comp[4] ? 4 : 0;
            bitval += comp[3] ? 8 : 0;
            bitval += comp[4] ? 16 : 0;
            bitval += comp[5] && comp[3] && comp[6] ? 32 : 0;
            bitval += comp[6] ? 64 : 0;
            bitval += comp[7] && comp[4] && comp[6] ? 128 : 0;

            UpdateTileID(bitval);
        }

        //
        // This uses neighbor connectedness to update the cells tile ID as opposed to simply relying on whether that neighbor is occupied or not
        // This allows us to place walls in between neighboring cells to create more intricate layouts
        //
        public void UpdateTileIDByConnectedNeighbors(List<MapTile> grid, int width, int height)
        {
            if (_type == ECellType.Unoccupied)
                return;

            int bitval = 0;

            List<MapTile> neighbors = GetOccupiedNeighbors(grid, width, height);

            bitval += IsCellConnected(ECardinal.W) && neighbors[3].IsCellConnected(ECardinal.N) && IsCellConnected(ECardinal.N) && neighbors[0].IsCellConnected(ECardinal.W) ? 1 : 0;
            bitval += IsCellConnected(ECardinal.W) ? 2 : 0;
            bitval += IsCellConnected(ECardinal.W) && neighbors[3].IsCellConnected(ECardinal.S) && IsCellConnected(ECardinal.S) && neighbors[2].IsCellConnected(ECardinal.W) ? 4 : 0;
            bitval += IsCellConnected(ECardinal.N) ? 8 : 0;
            bitval += IsCellConnected(ECardinal.S) ? 16 : 0;
            bitval += IsCellConnected(ECardinal.N) && neighbors[0].IsCellConnected(ECardinal.E) && IsCellConnected(ECardinal.E) && neighbors[1].IsCellConnected(ECardinal.N) ? 32 : 0;
            bitval += IsCellConnected(ECardinal.E) ? 64 : 0;
            bitval += IsCellConnected(ECardinal.S) && neighbors[2].IsCellConnected(ECardinal.E) && IsCellConnected(ECardinal.E) && neighbors[1].IsCellConnected(ECardinal.S) ? 128 : 0;

            UpdateTileID(bitval);
        }

        //
        // Gets a random unoccupied tile in one of the four cardinal directions
        //
        public MapTile GetRandomUnoccupiedNeighbor(List<MapTile> grid, int width, int height)
        {
            List<MapTile> neighbors = GetUnoccupiedNeighbors(grid, width, height);
            neighbors.Shuffle();

            for (int i = 0; i < neighbors.Count; i++)
            {
                if (neighbors[i] != null)
                {
                    return neighbors[i];
                }
            }

            return null;
        }

        //
        // Automatically sets a nighboring tile in a particular cardinal direction to be connected
        // TODO: Should maybe verify that tile is occupied?
        //
        public void ConnectCardinally(ECardinal direction)
        {
            _connections[(int)direction] = true;
        }

        //
        // Sets the tile to type to either an entrance or an exit and updates the sprite accordingly
        //
        public void SetAsDoor(bool isExit)
        {
            _type = ECellType.Door;

            string suffix = isExit ? "_Exit" : "_Door";
            UpdateSprite(SpriteHandler.FetchSprite("Grid/Wall_" + _tileID + suffix));
        }

        //
        // Sets the tile to premade tile to flag that we shouldn't spawn it in as generic
        //
        public void SetAsPremade()
        {
            _isPremade = true;
        }

        #region Tile Getters
        //
        // Returns a tile in the grid by position
        //
        public static MapTile GetTileByPosition(List<MapTile> grid, int x, int y, int width, int height)
        {
            if (x < 0 || x > width - 1 || y < 0 || y > height - 1)
            {
                return null;
            }

            return grid[x + y * width];
        }

        //
        // Returns an unoccupied tile in the grid by position
        //
        public static MapTile GetUnoccupiedTileByPosition(List<MapTile> grid, int x, int y, int width, int height)
        {
            MapTile cell = GetTileByPosition(grid, x, y, width, height);
            if (cell == null || cell.CellType != ECellType.Unoccupied)
            {
                return null;
            }

            return cell;
        }

        //
        // Returns a list of neighboring unoccupied tiles in the order of N E S W, sets that tile to null if currently occupied
        //
        public List<MapTile> GetUnoccupiedNeighbors(List<MapTile> grid, int width, int height)
        {
            List<MapTile> neighbors = new List<MapTile>();
            neighbors.Add(GetUnoccupiedTileByPosition(grid, _x, _y - 1, width, height));
            neighbors.Add(GetUnoccupiedTileByPosition(grid, _x + 1, _y, width, height));
            neighbors.Add(GetUnoccupiedTileByPosition(grid, _x, _y + 1, width, height));
            neighbors.Add(GetUnoccupiedTileByPosition(grid, _x - 1, _y, width, height));
            return neighbors;
        }

        //
        // Returns an occupied tile in the grid by position
        //
        public static MapTile GetOccupiedCell(List<MapTile> grid, int x, int y, int width, int height)
        {
            MapTile neighbor = GetTileByPosition(grid, x, y, width, height);
            if (neighbor == null || neighbor.CellType == ECellType.Unoccupied)
            {
                return null;
            }

            return neighbor;
        }

        //
        // Returns a list of neighboring occupied tiles in the order of N E S W, sets that tile to null if currently occupied
        //
        public List<MapTile> GetOccupiedNeighbors(List<MapTile> grid, int width, int height)
        {
            List<MapTile> neighbors = new List<MapTile>();
            neighbors.Add(GetOccupiedCell(grid, _x, _y - 1, width, height));
            neighbors.Add(GetOccupiedCell(grid, _x + 1, _y, width, height));
            neighbors.Add(GetOccupiedCell(grid, _x, _y + 1, width, height));
            neighbors.Add(GetOccupiedCell(grid, _x - 1, _y, width, height));
            return neighbors;
        }

        //
        // Returns the neighboring tile in a particular cardinal direction
        //
        public MapTile GetTileInDirection(List<MapTile> grid, int width, int height, ECardinal direction)
        {
            int xDiff = 0, yDiff = 0;
            switch (direction)
            {
                case ECardinal.N:
                    xDiff = 0;
                    yDiff = -1;
                    break;
                case ECardinal.E:
                    xDiff = 1;
                    yDiff = 0;
                    break;
                case ECardinal.S:
                    xDiff = 0;
                    yDiff = 1;
                    break;
                case ECardinal.W:
                    xDiff = -1;
                    yDiff = 0;
                    break;
            }

            return GetTileByPosition(grid, _x + xDiff, _y + yDiff, width, height);
        }

        //
        // Returns the neighboring tile in a particular cardinal direction if it is unoccupied
        //
        public MapTile GetUnoccupiedTileInDirection(List<MapTile> grid, int width, int height, ECardinal direction)
        {
            MapTile cell = GetTileInDirection(grid, width, height, direction);
            if (cell != null && cell.CellType == ECellType.Unoccupied)
                return cell;

            return null;
        }
        #endregion
    }
}
