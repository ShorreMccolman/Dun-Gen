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
    public struct ExitTile
    {
        public MapTile Tile;
        public ECardinal ExitDirection;

        public ExitTile(MapTile tile, ECardinal exitDirection)
        {
            Tile = tile;
            ExitDirection = exitDirection;
        }
    }

    //
    // 
    //
    public class MapRoom
    {
        public List<MapTile> Tiles;
        public List<ExitTile> Exits;
        public int RoomID;
        public bool IsPremade;

        public int MinX, MaxX, MinY, MaxY;

        public MapRoom(List<MapTile> tiles, int id)
        {
            Tiles = tiles;
            RoomID = id;
            Exits = new List<ExitTile>();
            IsPremade = false;

            MinX = tiles[0].X;
            MaxX = tiles[0].X;
            MinY = tiles[0].Y;
            MaxY = tiles[0].Y;
            foreach (var tile in tiles)
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

        public MapRoom(List<MapTile> tiles, List<ExitTile> exits, int id)
        {
            Tiles = tiles;
            Exits = exits;
            RoomID = id;
            IsPremade = true;

            MinX = tiles[0].X;
            MaxX = tiles[0].X;
            MinY = tiles[0].Y;
            MaxY = tiles[0].Y;
            foreach (var tile in tiles)
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
        //
        // This class represents two primary dungeon rooms and the path/hallway connected them together.
        // Used as an intermediate step for generating the full map
        //
        class Connection
        {
            public MapRoom First;
            public MapRoom Second;
            public List<MapTile> Path;

            public Connection(MapRoom first, MapRoom second, List<MapTile> path)
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
        class Graph
        {
            public List<Connection> Connections;
            public List<MapRoom> Nodes;
            public List<MapTile> AllCells;

            public Graph()
            {
                Connections = new List<Connection>();
                Nodes = new List<MapRoom>();
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
                foreach (var room in Nodes)
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

        [SerializeField] GameObject _mapCellPrefab;
        [SerializeField] RectTransform _gridParent;
        [SerializeField] PremadeTile[] _requiredTiles;

        List<MapRoom> _rooms;
        List<MapRoom> _premadeRooms;

        MapTile _entrance;
        MapTile _exit;

        Pathfinding _pathfinding;

        bool _isWaiting;

        MapData _mapData;

        public static int NextRoomID
        {
            get
            {
                _nextRoomID++;
                return _nextRoomID;
            }
            private set { _nextRoomID = value; }
        }
        static int _nextRoomID;

        private void Start()
        {
            _rooms = new List<MapRoom>();
            _premadeRooms = new List<MapRoom>();
            NextRoomID = -1;
        }

        void ClearMap()
        {
            if (_mapData != null)
            {
                foreach (var cell in _mapData.Map)
                {
                    Destroy(cell.gameObject);
                }
                _mapData = null;
            }
            _rooms.Clear();
            _premadeRooms.Clear();
            _pathfinding = new Pathfinding();
            NextRoomID = -1;
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

            _entrance = _mapData.Map[data.Entrance.X + settings.GridWidth * data.Entrance.Y];

            _mapData.SetEntrance(_entrance, data.StartingDirection);

            return _mapData;
        }

        void CopyTilesFromData(MapData data)
        {
            for (int i = 0; i < _mapData.Map.Count; i++)
            {
                _mapData.Map[i].CopyTile(data.Map[i]);
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
            _mapData.UpdateTilesProximally();

            // Connect nodes and graphs
            List<Connection> connections = ConnectRooms();

            // Update tiles proximally
            _mapData.UpdateTilesProximally();

            // Create list of disconnected groups of connected rooms
            List<Graph> graphs = FindDisconnectedGraphs(connections);

            // Connect all graphs to each other
            ConnectGraphs(graphs);

            // Connect premade rooms to the rest of the graph
            ConnectPremadeRooms(graphs);

            // Update tiles based on connectivity
            _mapData.ConnectNeighborsAndUpdate();

            // Get a list of edges that can be used for branching off of
            List<MapTile> edges = GetBranchableEdges();

            // Generate a set of branching hallways if settings call for any
            GenerateBranches(edges, settings);

            // Create Entrance and Exit
            GeneratePortals(edges);

            return _mapData;
        }

        void PopulateBlankCells(int width, int height)
        {
            GridLayoutGroup layout = _gridParent.GetComponent<GridLayoutGroup>();

            layout.cellSize = new Vector2(_gridParent.rect.width / width, _gridParent.rect.height / height);

            int numCells = width * height;

            List<MapTile> tiles = new List<MapTile>();
            for (int i = 0; i < numCells; i++)
            {
                GameObject go = Instantiate(_mapCellPrefab);
                MapTile cell = go.GetComponent<MapTile>();
                cell.UpdateCellType(ECellType.Unoccupied);
                tiles.Add(cell);

                cell.Init(i % width, i / width);

                go.transform.parent = _gridParent;
            }

            _mapData = new MapData(tiles, width, height);
            _pathfinding.CreateNetwork(width, height, tiles);
        }

        void GeneratePremadeRooms(PremadeTile[] tiles)
        {
            foreach(var tile in tiles)
            {
                List<MapTile> options = new List<MapTile>(_mapData.Map);
                options.Shuffle();

                while (options.Count > 0)
                {
                    MapTile option = options[0];
                    options.RemoveAt(0);

                    // Only bother looking at tiles that start and end at least 2 cells away from the edge of the grid
                    if(option.X >= 2 && option.Y >= 2 && option.X + tile.Width < _mapData.Width - 2 && option.Y + tile.Height < _mapData.Height - 2)
                    {
                        List<MapTile> placementArea;
                        if(_mapData.IsAreaUnoccupied(tile.Width, tile.Height, option, out placementArea))
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
            List<ExitTile> exitTiles = new List<ExitTile>();
            List<int> exitIndexes = new List<int>();
            List<MapTile> cellsToRemove = new List<MapTile>();

            int roomID = NextRoomID;
            int[] ids = tile.GetGridIDs();
            for(int i=0; i<area.Count; i++)
            {
                MapTile next = area[i];
                next.UpdateTileID(ids[i]);
                if(ids[i] >= 0)
                {
                    next.UpdateCellType(ECellType.PremadeRoom);
                    next.UpdateRoomID(roomID);
                }
                cellsToRemove.Add(next);

                // It is safe to assume adjacent tiles will not wrap in the grid since premade rooms are not placed on the map edges
                ECardinal exitDirection = DungeonTileData.GetExitDirectionForTileID(ids[i]);
                int exitIndex = -1;
                // TODO: Move this switch into map data class - something like Get index in cardinal direction
                switch (exitDirection)
                {
                    case ECardinal.N:
                        exitIndex = _mapData.GetIndex(next.X, next.Y - 1);
                        break;
                    case ECardinal.E:
                        exitIndex = _mapData.GetIndex(next.X + 1, next.Y);
                        break;
                    case ECardinal.S:
                        exitIndex = _mapData.GetIndex(next.X, next.Y + 1);
                        break;
                    case ECardinal.W:
                        exitIndex = _mapData.GetIndex(next.X - 1, next.Y);
                        break;
                }

                if(exitIndex > -1)
                {
                    exitTiles.Add(new ExitTile(_mapData.Map[exitIndex], exitDirection));
                    _mapData.Map[exitIndex].ConnectCardinally(Cardinals.Flip(exitDirection));
                    exitIndexes.Add(exitIndex);
                }
            }

            // TODO: This doesn't work for rooms that exit within the area, so need to rethink this!!!
            for (int i = 0; i < tile.Width + 2; i++)
            {
                for (int j = 0; j < tile.Height + 2; j++)
                {
                    int index = _mapData.GetIndex(area[0].X - 1 + i, area[0].Y - 1 + j);
                    if (i == 0 || i == tile.Width + 1 || j == 0 || j == tile.Height + 1)
                    {
                        if (!exitIndexes.Contains(index))
                        {
                            MapTile borderTile = _mapData.Map[index];
                            borderTile.UpdateCellType(ECellType.Invalid);
                            cellsToRemove.Add(borderTile);
                        }
                    }
                }
            }
            _pathfinding.RemoveCellsFromGrid(cellsToRemove);

            _premadeRooms.Add(new MapRoom(area, exitTiles, roomID));
        }

        void GeneratePrimaryRooms(int rooms)
        {
            RoomDistributor distributor = new EvenDistributor();
            _rooms = distributor.GenerateRooms(_mapData, rooms);
        }

        void MergeAdjacentRooms()
        {
            List<MapRoom> unmerged = new List<MapRoom>(_rooms);

            List<MapRoom> completedRooms = new List<MapRoom>();
            while(unmerged.Count > 0)
            {
                MapRoom currentRoom = unmerged[0];
                unmerged.RemoveAt(0);

                MapRoom roomToMerge = null;
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
                    MapRoom room = new MapRoom(allTiles, currentRoom.RoomID);
                    unmerged.Add(room);
                }
                else
                {
                    completedRooms.Add(currentRoom);
                }
            }
            _rooms = completedRooms;
        }

        bool AreRoomsAdjacent(MapRoom first, MapRoom second)
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

        List<Connection> ConnectRooms()
        {
            // Create list of directly connected nodes
            List<Connection> connections = new List<Connection>();

            List<MapRoom> unused = new List<MapRoom>(_rooms);
            while (unused.Count > 0)
            {
                MapRoom next = unused[0];
                unused.RemoveAt(0);

                MapRoom nearest = _pathfinding.FindNearestNode(next, _rooms);
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

        List<Graph> FindDisconnectedGraphs(List<Connection> connections)
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

        void ConnectGraphs(List<Graph> updatedGraphs)
        {
            // Count to prevent infinite loops in case of error
            int count = 0;
            while (updatedGraphs.Count > 1 && count < 100)
            {
                count++;

                Graph first = updatedGraphs[0];
                updatedGraphs.Remove(first);

                Graph nearest = null;
                MapRoom bestFirst = null;
                MapRoom bestSecond = null;
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

        Graph MergeGraphs(Graph first, MapRoom firstNode, Graph second, MapRoom secondNode)
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

        void ConnectPremadeRooms(List<Graph> graph)
        {
            if(graph.Count != 1)
            {
                Debug.LogError("Graphs were not correctly merged before connecting premade rooms!");
            }

            foreach(var room in _premadeRooms)
            {
                List<MapRoom> graphRooms = new List<MapRoom>(graph[0].Nodes);
                foreach (var tile in room.Exits)
                {
                    MapRoom target = PickBestRoomForConnecting(graphRooms, tile);

                    List<MapTile> path = _pathfinding.FindPath(tile.Tile, target.GetAnchorTile());
                    foreach (var pathTile in path)
                    {
                        if (pathTile.CellType == ECellType.Unoccupied)
                        {
                            pathTile.UpdateCellType(ECellType.Hallway);
                        }
                    }

                    Connection connection = new Connection(room, target, path);
                    graph[0].AddConnection(connection);
                }
            }
        }

        MapRoom PickBestRoomForConnecting(List<MapRoom> rooms, ExitTile origin)
        {
            List<MapRoom> candidates = new List<MapRoom>();
            switch(origin.ExitDirection)
            {
                case ECardinal.N:
                    foreach(var room in rooms)
                    {
                        if(room.MaxY < origin.Tile.Y)
                        {
                            candidates.Add(room);
                        }
                    }
                    break;
                case ECardinal.E:
                    foreach (var room in rooms)
                    {
                        if (room.MinX > origin.Tile.X)
                        {
                            candidates.Add(room);
                        }
                    }
                    break;
                case ECardinal.S:
                    foreach (var room in rooms)
                    {
                        if (room.MinY > origin.Tile.Y)
                        {
                            candidates.Add(room);
                        }
                    }
                    break;
                case ECardinal.W:
                    foreach (var room in rooms)
                    {
                        if (room.MaxX < origin.Tile.X)
                        {
                            candidates.Add(room);
                        }
                    }
                    break;
            }
            if(candidates.Count == 0)
            {
                candidates.AddRange(rooms);
            }

            candidates.OrderBy(x => _pathfinding.EvaluateH(origin.Tile, x.GetAnchorTile()));
            return candidates[0];
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
            List<MapTile> endPieces = _mapData.Map.FindAll(x => DungeonTileData.EndPieces.Contains(x.TileID));
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
                    endPieces = _mapData.Map.FindAll(x => DungeonTileData.EndPieces.Contains(x.TileID));
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

            _mapData.SetEntrance(_entrance, ECardinal.N);
        }

        //
        // Returns a list of map tiles that can be used to spin off branches
        //
        List<MapTile> GetBranchableEdges()
        {
            return _mapData.Map.FindAll(x => x.CellType != ECellType.PremadeRoom &&
            (DungeonTileData.WallPieces.Contains(x.TileID) || DungeonTileData.HallPieces.Contains(x.TileID) || DungeonTileData.TurnPieces.Contains(x.TileID)));
        }

        bool CreateBranch(MapTile startingCell, EBranchType type)
        {
            BranchGenerator generator;
            switch (type)
            {
                case EBranchType.Shoot:
                    generator = new ShootBranch();
                    return generator.Create(_mapData, startingCell);
                case EBranchType.Snake:
                    generator = new SnakeBranch();
                    return generator.Create(_mapData, startingCell);
                case EBranchType.Bridge:
                    generator = new BridgeBranch();
                    return generator.Create(_mapData, startingCell);
                case EBranchType.Crank:
                    generator = new CrankBranch();
                    return generator.Create(_mapData, startingCell);
            }

            return false;
        }
    }
}