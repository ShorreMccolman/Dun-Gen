using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

//
//  This is the primary class responsible for generating the actual map layout that will be used to spawn the dungeon
//

namespace DunGen
{
    public enum EBranchType
    {
        Shoot,
        Snake,
        Bridge,
        Crank,
        Count
    }

    //
    // 
    //
    public class Room
    {
        public List<MapTile> Tiles;
        public int RoomID;
        public bool IsPremade;

        public int MinX, MaxX, MinY, MaxY;

        public Room(List<MapTile> tiles, int id, bool isPremade = false)
        {
            Tiles = tiles;
            RoomID = id;
            IsPremade = isPremade;

            MinX = tiles[0].X;
            MaxX = tiles[0].X;
            MinY = tiles[0].Y;
            MaxY = tiles[0].Y;
            foreach(var tile in tiles)
            {
                if (tile.X < MinX)
                    MinX = tile.X;
                if (tile.X > MaxX)
                    MaxX = tile.X;
                if (tile.Y < MinY)
                    MinY = tile.Y;
                if (tile.Y > MaxY)
                    MaxY = tile.Y;
            }
        }

        // TODO rethink this probably
        public MapTile GetAnchorTile()
        {
            return Tiles.RandomChoice();
        }
    }

    //
    // This class represents two primary dungeon rooms and the path/hallway connected them together.
    // Used as an intermediate step for generating the full map
    //
    public class Connection
    {
        public Room First;
        public Room Second;
        public List<MapTile> Path;

        public Connection(Room first, Room second, List<MapTile> path)
        {
            First = first;
            Second = second;
            Path = path;
        }
    }

    //
    // A graph in this instance is a list of connected connections, that is a subset of primary dungeon rooms that are all currently linked together with hallways/paths.
    // The end goal of the map generation algorithm is to produce a single graph including all of the dungeon rooms connected together
    //
    public class Graph
    {
        public List<Connection> Connections;
        public List<Room> Nodes;
        public List<MapTile> AllCells;

        public Graph()
        {
            Connections = new List<Connection>();
            Nodes = new List<Room>();
        }

        public void AddConnection(Connection connection)
        {
            Connections.Add(connection);

            if (!Nodes.Contains(connection.First))
                Nodes.Add(connection.First);
            if (!Nodes.Contains(connection.Second))
                Nodes.Add(connection.Second);
        }

        public void CompileCells()
        {
            AllCells = new List<MapTile>();
            foreach(var room in Nodes)
            {
                AllCells.AddRange(room.Tiles);
            }

            foreach (var connection in Connections)
            {
                foreach (var cell in connection.Path)
                {
                    if (!AllCells.Contains(cell))
                    {
                        AllCells.Add(cell);
                    }
                }
            }
        }
    }


    /// 
    /// To give a quick summary of the map generation algorithm I will describe it roughly in steps.
    /// 1. Pick a number of tiles on the grid, making sure they are seperated by at least one tile in between them, to act as our primary rooms.
    /// 2. Iterate through each primary room, everytime we find an isolated room we construct a path to the nearest room using an A* algorithm.
    ///    At this stage each room will be connected to at least one other room, but a path likely does not exist from any given room to any other given room.
    /// 
    /// 3. Scan through the rooms to construct graphs representing subsets of rooms connected together by a network of paths.
    /// 4. Iterate through each graph, connecting it to its nearest graph similar to how we connected rooms together in step 2.
    ///    Each time we connect two graphs together we merge them into a single graph.
    /// 5. Repeat step 4 until all graphs are merged together. This will mean all our primary rooms are connected together in a single network.
    /// 
    /// 6. At this point, to add variation we create a bunch of offshoot paths called branches that have different properties based on the branch type.
    /// 7. Choose a tile to act as the dungeon entrance and another tile for the exit.
    ///
    /// I have ideas for how to add more variation to the dungeons but for now I will flesh out the rest of the features of the tool
    ///
    public class MapGenerator : MonoBehaviour
    {
        [SerializeField] GameObject _mapCellPrefab;
        [SerializeField] RectTransform _gridParent;
        List<MapTile> _map;
        List<Room> _rooms;

        List<MapTile> _availableTiles;

        [SerializeField] PremadeTile[] _requiredTiles;

        MapTile _entrance;
        MapTile _exit;

        Pathfinding _pathfinding;

        int _width, _height;
        int _nextRoomID;

        bool _isWaiting;

        void Start()
        {
            _map = new List<MapTile>();
            _rooms = new List<Room>();
            _pathfinding = new Pathfinding();
            _isWaiting = true;
            _nextRoomID = 0;
        }

        void ClearMap()
        {
            if (_map != null)
            {
                foreach (var cell in _map)
                {
                    Destroy(cell.gameObject);
                }
                _map.Clear();
                _rooms.Clear();
                _pathfinding = new Pathfinding();
                _nextRoomID = 0;
            }
        }

        //
        // Copy map data to this generator
        // **I'm assuming atm that we don't need to run any of the generation on this map data so I'm not copying the pathfinding changes or other related changes
        // **That suggest to me that we probably should seperate some logic out of this class but thats a TODO for now
        //
        public MapData CopyMapData(GenerationSettings settings, MapData data)
        {
            // Clear existing map if one exists
            ClearMap();

            // Generate grid of empty cells based on setting dimensions
            PopulateBlankCells(settings.GridWidth, settings.GridHeight);

            CopyTilesFromData(data);

            _entrance = _map[data.Entrance.X + settings.GridWidth * data.Entrance.Y];

            return new MapData(_map, _entrance, data.StartingDirection, _width, _height);
        }

        void CopyTilesFromData(MapData data)
        {
            for (int i = 0; i < _map.Count; i++)
            {
                _map[i].CopyTile(data.Map[i]);
            }
        }

        //
        // This generates the full map in a single step, for demo purposes we will want to split these up into steps to give the user
        // a visual picture of how the map generation algorithm works
        //
        public MapData GenerateInstant(GenerationSettings settings)
        {
            // Clear existing map if one exists
            ClearMap();

            // Generate grid of empty cells based on setting dimensions
            PopulateBlankCells(settings.GridWidth, settings.GridHeight);

            // Place required tiles (TODO: Pull these from settings)
            GeneratePremadeRooms(_requiredTiles);

            // Place primary rooms
            GeneratePrimaryRooms(settings.PrimaryRooms.Evaluate());

            // Merge adjacent rooms together
            MergeAdjacentRooms();

            // Update tiles based on neighbors regardless of connectivity
            UpdateTilesProximally();

            // Connect nodes and graphs
            List<Connection> connections = ConnectRooms();

            // Update tiles proximally
            UpdateTilesProximally();

            // Create list of disconnected groups of connected rooms
            List<Graph> graphs = FindDisconnectedGraphs(connections);

            // Connect all graphs to each other
            ConnectGraphs(graphs);

            // Update tiles based on connectivity
            ConnectNeighborsAndUpdate();

            // Get a list of edges that can be used for branching off of
            List<MapTile> edges = GetBranchableEdges();

            // Generate a set of branching hallways if settings call for any
            GenerateBranches(edges, settings);

            // Create Entrance and Exit
            GeneratePortals(edges);

            return new MapData(_map, _entrance, ECardinal.N, _width, _height);
        }

        void PopulateBlankCells(int width, int height)
        {
            GridLayoutGroup layout = _gridParent.GetComponent<GridLayoutGroup>();

            _width = width;
            _height = height;

            layout.cellSize = new Vector2(_gridParent.rect.width / _width, _gridParent.rect.height / _height);

            int numCells = _width * _height;

            for (int i = 0; i < numCells; i++)
            {
                GameObject go = Instantiate(_mapCellPrefab);
                MapTile cell = go.GetComponent<MapTile>();
                cell.UpdateCellType(ECellType.Unoccupied);
                _map.Add(cell);

                cell.Init(i % _width, i / _width);

                go.transform.parent = _gridParent;
            }

            _pathfinding.CreateNetwork(_width, _height, _map);
            _availableTiles = new List<MapTile>(_map);
        }

        bool IsAreaUnoccupied(int width, int height, MapTile startingTile, out List<MapTile> area)
        {
            area = new List<MapTile>();

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int index = startingTile.X + j + (startingTile.Y + i) * _width;
                    if (!_map[index].IsAvailable)
                    {
                        return false;
                    }

                    area.Add(_map[index]);
                }
            }
            return true;
        }

        void GeneratePremadeRooms(PremadeTile[] tiles)
        {
            foreach(var tile in tiles)
            {
                List<MapTile> options = new List<MapTile>(_availableTiles);
                options.Shuffle();

                while (options.Count > 0)
                {
                    MapTile option = options[0];
                    options.RemoveAt(0);

                    // Only bother looking at tiles that start and end at least 2 cells away from the edge of the grid
                    if(option.X >= 2 && option.Y >= 2 && option.X + tile.Width < _width - 2 && option.Y + tile.Height < _height - 2)
                    {
                        List<MapTile> placementArea;
                        if(IsAreaUnoccupied(tile.Width, tile.Height, option, out placementArea))
                        {
                            foreach (var placedTile in placementArea)
                                options.Remove(placedTile);

                            PlacePremadeRoom(tile, placementArea);
                            break;
                        }
                    }
                }
            }
        }

        void PlacePremadeRoom(PremadeTile tile, List<MapTile> area)
        {
            List<MapTile> exitTiles = new List<MapTile>();
            List<MapTile> cellsToRemove = new List<MapTile>();

            int[] ids = tile.GetGridIDs();
            for(int i=0; i<area.Count; i++)
            {
                MapTile next = area[i];
                next.UpdateTileID(ids[i]);
                if(ids[i] >= 0)
                {
                    next.UpdateCellType(ECellType.PremadeRoom);
                    next.UpdateRoomID(_nextRoomID);
                }
                _availableTiles.Remove(next);
                cellsToRemove.Add(next);

                // It is safe to assume adjacent tiles will not wrap in the grid since premade rooms are not placed on the map edges
                ECardinal exitDirection = DungeonTileData.GetExitDirectionForTileID(ids[i]);
                int exitIndex;
                switch (exitDirection)
                {
                    case ECardinal.N:
                        exitIndex = next.X + (next.Y - 1) * _width;
                        exitTiles.Add(_map[exitIndex]);
                        break;
                    case ECardinal.E:
                        exitIndex = next.X + 1 + next.Y * _width;
                        exitTiles.Add(_map[exitIndex]);
                        break;
                    case ECardinal.S:
                        exitIndex = next.X + (next.Y + 1) * _width;
                        exitTiles.Add(_map[exitIndex]);
                        break;
                    case ECardinal.W:
                        exitIndex = next.X - 1 + next.Y * _width;
                        exitTiles.Add(_map[exitIndex]);
                        break;
                }
            }

            // TODO: This doesn't work for rooms that exit within the area, so need to rethink this!!!
            for (int i = 0; i < tile.Width + 2; i++)
            {
                for (int j = 0; j < tile.Height + 2; j++)
                {
                    int index = area[0].X - 1 + i + (area[0].Y - 1 + j) * _width;
                    if (i == 0 || i == tile.Width + 1 || j == 0 || j == tile.Height + 1)
                    {
                        _map[index].UpdateCellType(ECellType.Invalid);
                        _availableTiles.Remove(_map[index]);
                        cellsToRemove.Add(_map[index]);
                    }
                }
            }
            _pathfinding.RemoveCellsFromGrid(cellsToRemove);

            //_rooms.Add(new Room(area, _nextRoomID, true));
            _nextRoomID++;
        }

        void GeneratePrimaryRooms(int rooms)
        {
            // Pick random squares to act as primary rooms
            List<MapTile> options = new List<MapTile>(_availableTiles);
            options.Shuffle();

            List<List<MapTile>> primaryRooms = new List<List<MapTile>>();
            for (int i = 0; i < rooms; i++)
            {
                MapTile option = options[0];
                options.RemoveAt(0);

                int roomWidth = Random.Range(2, 4);
                int roomHeight = Random.Range(2, 4);

                if (roomWidth > _width - option.X)
                    roomWidth = _width - option.X;
                if (roomHeight > _height - option.Y)
                    roomHeight = _height - option.Y;

                if (roomWidth < 2 || roomHeight < 2)
                    continue;

                List<MapTile> placementArea;
                if(IsAreaUnoccupied(roomWidth, roomHeight, option, out placementArea))
                {
                    primaryRooms.Add(placementArea);
                    PlacePrimaryRoom(placementArea);
                }
            }
        }

        void PlacePrimaryRoom(List<MapTile> area)
        {
            foreach(var tile in area)
            {
                tile.UpdateCellType(ECellType.PrimaryRoom);
                tile.UpdateRoomID(_nextRoomID);
                _availableTiles.Remove(tile);
            }

            _rooms.Add(new Room(area, _nextRoomID));
            _nextRoomID++;
        }

        void MergeAdjacentRooms()
        {
            List<Room> unmerged = new List<Room>(_rooms);

            List<Room> completedRooms = new List<Room>();
            while(unmerged.Count > 0)
            {
                Room currentRoom = unmerged[0];
                unmerged.RemoveAt(0);

                Room roomToMerge = null;
                foreach (var testRoom in unmerged)
                {
                    if(currentRoom.MaxX + 1 < testRoom.MinX
                        || currentRoom.MinX > testRoom.MaxX + 1
                        || currentRoom.MaxY + 1 < testRoom.MinY
                        || currentRoom.MinY > testRoom.MaxY + 1)
                    {
                        continue;
                    }

                    if(AreRoomsAdjacent(currentRoom, testRoom))
                    {
                        roomToMerge = testRoom;
                        break;
                    }
                }

                if(roomToMerge != null)
                {
                    unmerged.Remove(roomToMerge);
                    List<MapTile> allTiles = new List<MapTile>(currentRoom.Tiles);
                    allTiles.AddRange(roomToMerge.Tiles);
                    Room room = new Room(allTiles, currentRoom.RoomID);
                    unmerged.Add(room);
                }
                else
                {
                    completedRooms.Add(currentRoom);
                }
            }
            _rooms = completedRooms;
        }

        bool AreRoomsAdjacent(Room first, Room second)
        {
            foreach(var tile in first.Tiles)
            {
                foreach(var other in second.Tiles)
                {
                    int xDiff = Mathf.Abs(tile.X - other.X);
                    int yDiff = Mathf.Abs(tile.Y - other.Y);

                    if (xDiff + yDiff < 2)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public List<Connection> ConnectRooms()
        {
            // Create list of directly connected nodes
            List<Connection> connections = new List<Connection>();

            List<Room> rooms = new List<Room>();
            foreach (var room in _rooms)
            {
                // TODO deal with premade rooms
                if (!room.IsPremade)
                {
                    rooms.Add(room);
                }
            }

            List<Room> unused = new List<Room>(rooms);
            while (unused.Count > 0)
            {
                Room next = unused[0];
                unused.RemoveAt(0);

                Room nearest = _pathfinding.FindNearestNode(next, rooms);
                List<MapTile> path = _pathfinding.FindPath(next.GetAnchorTile(), nearest.GetAnchorTile());

                if (unused.Contains(nearest))
                {
                    unused.Remove(nearest);
                }

                foreach(var tile in path)
                {
                    if(tile.CellType == ECellType.Unoccupied)
                    {
                        tile.UpdateCellType(ECellType.Hallway);
                    }
                }

                connections.Add(new Connection(next, nearest, path));
            }
            return connections;
        }

        public List<Graph> FindDisconnectedGraphs(List<Connection> connections)
        {
            // Determine disconnected graphs
            List<Graph> graphs = new List<Graph>();
            List<Connection> candidates = new List<Connection>(connections);
            while (candidates.Count > 0)
            {
                Connection connection = candidates[0];

                Graph newGraph = new Graph();
                newGraph.AddConnection(connection);

                List<Connection> connected = new List<Connection>() { connection };
                while (connected.Count > 0)
                {
                    Connection next = connected[0];
                    connected.Remove(next);
                    candidates.Remove(next);
                    foreach (var other in candidates)
                    {
                        if (newGraph.Nodes.Contains(other.First) || newGraph.Nodes.Contains(other.Second))
                        {
                            connected.Add(other);
                            newGraph.AddConnection(other);
                        }
                    }
                }
                graphs.Add(newGraph);
            }

            List<Graph> updatedGraphs = new List<Graph>(graphs);
            foreach (var graph in updatedGraphs)
            {
                graph.CompileCells();
            }

            return updatedGraphs;
        }

        public void ConnectGraphs(List<Graph> updatedGraphs)
        {
            // Count to prevent infinite loops in case of error
            int count = 0;
            while (updatedGraphs.Count > 1 && count < 100)
            {
                count++;

                Graph first = updatedGraphs[0];
                updatedGraphs.Remove(first);

                Graph nearest = null;
                Room bestFirst = null;
                Room bestSecond = null;
                float bestDist = float.MaxValue;
                foreach (var graph in updatedGraphs)
                {
                    foreach (var room in first.Nodes)
                    {
                        foreach (var other in graph.Nodes)
                        {
                            float distance = _pathfinding.EvaluateH(room.GetAnchorTile(), other.GetAnchorTile());
                            if (distance < bestDist)
                            {
                                bestDist = distance;
                                nearest = graph;
                                bestFirst = room;
                                bestSecond = other;
                            }
                        }
                    }
                }

                updatedGraphs.Remove(nearest);

                Graph merged = MergeGraphs(first, bestFirst, nearest, bestSecond);
                updatedGraphs.Add(merged);

                foreach (var graph in updatedGraphs)
                {
                    graph.CompileCells();
                }
            }
        }

        Graph MergeGraphs(Graph first, Room firstNode, Graph second, Room secondNode)
        {
            List<MapTile> path = _pathfinding.FindPath(firstNode.GetAnchorTile(), secondNode.GetAnchorTile());

            foreach (var tile in path)
            {
                if (tile.CellType == ECellType.Unoccupied)
                {
                    tile.UpdateCellType(ECellType.Hallway);
                }
            }

            Connection merger = new Connection(firstNode, secondNode, path);

            Graph newGraph = new Graph();
            newGraph.AddConnection(merger);
            foreach (var connection in first.Connections)
            {
                newGraph.AddConnection(connection);
            }
            foreach (var connection in second.Connections)
            {
                newGraph.AddConnection(connection);
            }

            return newGraph;
        }

        void GenerateBranches(List<MapTile> edges, GenerationSettings settings)
        {
            // Create branches
            if (settings.CanBranch)
            {
                int branchCount = 10;
                edges.Shuffle();
                while (edges.Count > 0 && branchCount > 0)
                {
                    bool success = CreateBranch(edges[0], settings.GetRandomBranchType());

                    if (success)
                    {
                        branchCount--;
                    }

                    edges.RemoveAt(0);
                }
            }
        }

        void GeneratePortals(List<MapTile> edges)
        {
            List<MapTile> endPieces = _map.FindAll(x => DungeonTileData.EndPieces.Contains(x.TileID));
            int doorsNeeded = 2 - endPieces.Count;
            while (edges.Count > 0)
            {
                bool success = CreateBranch(edges[0], EBranchType.Shoot);

                if (success)
                {
                    doorsNeeded--;
                }

                edges.RemoveAt(0);

                if (doorsNeeded <= 0)
                {
                    endPieces = _map.FindAll(x => DungeonTileData.EndPieces.Contains(x.TileID));
                    break;
                }
            }
            endPieces.Shuffle();

            if (endPieces.Count < 2)
            {
                // This shouldn't be possible unless settings allowed for a exremely crowded dungeon which shouldn't be allowed so logging JIC
                Debug.LogError("Cound not find end piece for entrance and or exit!!!");
            }
            else
            {
                _entrance = endPieces[0];
                endPieces[0].SetAsDoor(false);
                _exit = endPieces[1];
                endPieces[1].SetAsDoor(true);
            }
        }

        void ConnectNeighborsAndUpdate()
        {
            foreach (var tile in _map)
            {
                tile.ConnectToOccupiedNeighbors(_map, _width, _height);
            }
            foreach (var tile in _map)
            {
                tile.UpdateTileIDByConnectedNeighbors(_map, _width, _height);
            }
        }

        void UpdateTilesProximally()
        {
            foreach (var tile in _map)
                tile.UpdateTileProximity(_map, _width, _height);
        }

        //
        // Returns a list of map tiles that can be used to spin off branches
        //
        List<MapTile> GetBranchableEdges()
        {
            return _map.FindAll(x => x.CellType != ECellType.PremadeRoom &&
            (DungeonTileData.WallPieces.Contains(x.TileID) || DungeonTileData.HallPieces.Contains(x.TileID) || DungeonTileData.TurnPieces.Contains(x.TileID)));
        }

        bool CreateBranch(MapTile startingCell, EBranchType type)
        {
            switch (type)
            {
                case EBranchType.Shoot:
                    return CreateShoot(startingCell);
                case EBranchType.Snake:
                    return CreateSnake(startingCell);
                case EBranchType.Bridge:
                    return CreateBridge(startingCell);
                case EBranchType.Crank:
                    return CreateCrank(startingCell);
            }

            return false;
        }

        bool CreateShoot(MapTile startingCell)
        {
            int stepCount = Random.Range(2, 10);

            MapTile current = startingCell;
            MapTile neighbor = startingCell.GetRandomUnoccupiedNeighbor(_map, _width, _height);

            if (neighbor == null)
            {
                return false;
            }

            int xDiff = current.X - neighbor.X;
            int yDiff = current.Y - neighbor.Y;

            ECardinal direction = Cardinals.FromCoordinateDifference(xDiff, yDiff);

            while (neighbor != null && stepCount > 0)
            {
                neighbor.UpdateCellType(ECellType.Hallway);

                current.ConnectCardinally(direction);
                current.UpdateTileIDByConnectedNeighbors(_map, _width, _height);
                neighbor.ConnectCardinally(Cardinals.Flip(direction));
                neighbor.UpdateTileIDByConnectedNeighbors(_map, _width, _height);

                current = neighbor;
                neighbor = current.GetUnoccupiedTileInDirection(_map, _width, _height, direction);
                stepCount--;
            }

            return true;
        }

        bool CreateSnake(MapTile startingCell)
        {
            int stepCount = Random.Range(3, 20);

            MapTile current = startingCell;
            MapTile neighbor = startingCell.GetRandomUnoccupiedNeighbor(_map, _width, _height);

            if (neighbor == null)
            {
                return false;
            }

            while (neighbor != null && stepCount > 0)
            {
                int xDiff = current.X - neighbor.X;
                int yDiff = current.Y - neighbor.Y;

                ECardinal direction = Cardinals.FromCoordinateDifference(xDiff, yDiff);

                neighbor.UpdateCellType(ECellType.Hallway);

                current.ConnectCardinally(direction);
                current.UpdateTileIDByConnectedNeighbors(_map, _width, _height);
                neighbor.ConnectCardinally(Cardinals.Flip(direction));
                neighbor.UpdateTileIDByConnectedNeighbors(_map, _width, _height);

                current = neighbor;
                neighbor = current.GetRandomUnoccupiedNeighbor(_map, _width, _height);
                stepCount--;
            }

            return true;
        }

        bool CreateBridge(MapTile startingCell)
        {
            MapTile current = startingCell;
            MapTile neighbor = startingCell.GetRandomUnoccupiedNeighbor(_map, _width, _height);

            if (neighbor == null)
            {
                return false;
            }

            List<MapTile> bridgeTiles = new List<MapTile>() { current, neighbor };
            ECardinal direction = Cardinals.FromCoordinateDifference(current.X - neighbor.X, current.Y - neighbor.Y);
            
            bool scanning = true;
            while (scanning)
            {
                current = neighbor;
                neighbor = current.GetTileInDirection(_map, _width, _height, direction);

                if(neighbor == null || neighbor.CellType == ECellType.Invalid)
                {
                    return false;
                }
                else if(neighbor.CellType == ECellType.Hallway || neighbor.CellType == ECellType.PrimaryRoom)
                {
                    scanning = false;
                }

                bridgeTiles.Add(neighbor);
            }

            for(int i=0;i<bridgeTiles.Count;i++)
            {
                if(i < bridgeTiles.Count - 1)
                {
                    bridgeTiles[i].ConnectCardinally(direction);
                }
                if(i > 0)
                {
                    bridgeTiles[i].ConnectCardinally(Cardinals.Flip(direction));
                }
                if(i < bridgeTiles.Count - 1 && i > 0)
                {
                    bridgeTiles[i].UpdateCellType(ECellType.Hallway);
                }
            }

            foreach(var tile in bridgeTiles)
            {
                tile.UpdateTileIDByConnectedNeighbors(_map, _width, _height);
            }

            return true;
        }

        bool CreateCrank(MapTile startingCell)
        {
            List<MapTile> toUpdate = new List<MapTile>();

            MapTile current = startingCell;
            MapTile neighbor = startingCell.GetRandomUnoccupiedNeighbor(_map, _width, _height);

            if (neighbor == null)
            {
                return false;
            }

            toUpdate.Add(current);
            toUpdate.Add(neighbor);

            int xDiff = current.X - neighbor.X;
            int yDiff = current.Y - neighbor.Y;

            ECardinal direction = Cardinals.FromCoordinateDifference(xDiff, yDiff);
            bool connected = false;

            bool clockwise = Random.Range(0, 2) == 0;

            while (neighbor != null)
            {
                if (!connected)
                {
                    neighbor.UpdateCellType(ECellType.Hallway);
                }

                current.ConnectCardinally(direction);
                neighbor.ConnectCardinally(Cardinals.Flip(direction));

                if (connected)
                    break;

                if (Random.Range(0, 4) == 0)
                {
                    direction = Cardinals.Rotate(direction, clockwise);
                }

                current = neighbor;
                neighbor = current.GetTileInDirection(_map, _width, _height, direction);

                if (neighbor != null)
                {
                    if (neighbor.CellType == ECellType.Invalid)
                    {
                        neighbor = null;
                    }
                    else
                    {
                        toUpdate.Add(neighbor);
                        if (current.CanConnectToTile(neighbor))
                        {
                            connected = true;
                        }
                    }
                }
            }

            foreach (var cell in toUpdate)
                cell.UpdateTileIDByConnectedNeighbors(_map, _width, _height);

            return true;
        }
    }
}