using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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
    // This class represents two primary dungeon rooms and the path/hallway connected them together.
    // Used as an intermediate step for generating the full map
    //
    public class Connection
    {
        public MapTile First;
        public MapTile Second;
        public List<MapTile> Path;

        public Connection(MapTile first, MapTile second, List<MapTile> path)
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
        public List<MapTile> Nodes;
        public List<MapTile> AllCells;

        public Color _color;

        public Graph()
        {
            Connections = new List<Connection>();
            Nodes = new List<MapTile>();
        }

        public void AddConnection(Connection connection)
        {
            Connections.Add(connection);

            if (!Nodes.Contains(connection.First))
                Nodes.Add(connection.First);
            if (!Nodes.Contains(connection.Second))
                Nodes.Add(connection.Second);
        }

        public void CompileCells(Color color)
        {
            _color = color;

            AllCells = new List<MapTile>(Nodes);

            foreach (var connection in Connections)
            {
                connection.First.ColorTile(color);
                connection.Second.ColorTile(color);

                foreach (var cell in connection.Path) // TODO: Sometimes no path could be found, need to check for this when creating the path and then try a new node if no path was created
                {
                    if (!AllCells.Contains(cell))
                    {
                        cell.ColorTile(color);
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

        MapTile _entrance;
        MapTile _exit;

        Pathfinding _pathfinding;

        int _width, _height;

        bool _isWaiting;

        void Start()
        {
            _map = new List<MapTile>();
            _pathfinding = new Pathfinding();
            _isWaiting = true;
        }

        //
        // This generates the full map in a single step, for demo purposes we will want to split these up into steps to give the user
        // a visual picture of how the map generation algorithm works
        //
        public MapData GenerateInstant(GenerationSettings settings)
        {
            if (_map != null)
            {
                foreach (var cell in _map)
                {
                    Destroy(cell.gameObject);
                }
                _map.Clear();
                _pathfinding = new Pathfinding();
            }

            PopulateBlankCells(settings.GridWidth, settings.GridHeight);

            // Pick rooms
            List<MapTile> primary = PickPrimaryRooms(settings.PrimaryRooms.Evaluate());
            UpdateTiles();

            // Connect nodes and graphs
            List<Connection> connections = ConnectNodes(primary);
            List<Graph> graphs = FindDisconnectedGraphs(connections);
            UpdateTiles();
            ConnectGraphs(graphs);
            foreach (var tile in _map)
            {
                tile.ConnectToOccupiedNeighbors(_map, _width, _height);
            }
            foreach (var tile in _map)
            {
                tile.UpdateTileIDByConnectedNeighbors(_map, _width, _height);
            }

            List<MapTile> Edges = GetBranchableEdges();

            // Create branches
            if (settings.CanBranch)
            {
                int branchCount = 10;
                Edges.Shuffle();
                while (Edges.Count > 0 && branchCount > 0)
                {
                    bool success = CreateBranch(Edges[0], settings.GetRandomBranchType());

                    if (success)
                    {
                        branchCount--;
                    }

                    Edges.RemoveAt(0);
                }
            }

            // Create Entrance and Exit
            List<MapTile> endPieces = _map.FindAll(x => DungeonTileData.EndPieces.Contains(x.TileID));
            int doorsNeeded = 2 - endPieces.Count;
            while (Edges.Count > 0)
            {
                bool success = CreateBranch(Edges[0], EBranchType.Shoot);

                if (success)
                {
                    doorsNeeded--;
                }

                Edges.RemoveAt(0);

                if(doorsNeeded <= 0)
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

            return new MapData(_map, _entrance, ECardinal.N, _width, _height);
        }


        //
        // This was a slapped together coroutine that I used when building the algorithm the first time,
        // TODO: make a cleaner version of this to be used by the demo tool
        //
        IEnumerator ConstructMap()
        {
            PopulateBlankCells(36, 20);

            while (_isWaiting)
            {
                yield return null;
            }
            _isWaiting = true;

            List<MapTile> primary = PickPrimaryRooms(30);
            UpdateTiles();

            while (_isWaiting)
            {
                yield return null;
            }
            _isWaiting = true;

            List<Connection> connections = ConnectNodes(primary);
            List<Graph> graphs = FindDisconnectedGraphs(connections);
            UpdateTiles();

            while (_isWaiting)
            {
                yield return null;
            }
            _isWaiting = true;

            ConnectGraphs(graphs);

            foreach (var tile in _map)
            {
                tile.ConnectToOccupiedNeighbors(_map, _width, _height);
            }

            foreach (var tile in _map)
            {
                tile.UpdateTileIDByConnectedNeighbors(_map, _width, _height);
            }

            while (_isWaiting)
            {
                yield return null;
            }
            _isWaiting = true;

            List<MapTile> Edges = GetBranchableEdges();

            int branchCount = 10;

            Edges.Shuffle();
            while (Edges.Count > 0)
            {
                bool success = CreateBranch(Edges[0], EBranchType.Shoot);

                if (success)
                {
                    branchCount--;
                    if (branchCount <= 0)
                        break;
                }

                Edges.RemoveAt(0);
            }

            while (_isWaiting)
            {
                yield return null;
            }
            _isWaiting = true;

            List<MapTile> endPieces = _map.FindAll(x => DungeonTileData.EndPieces.Contains(x.TileID));
            endPieces.Shuffle();

            _entrance = endPieces[0];
            _exit = endPieces[1];

            endPieces[0].SetAsDoor(false);
            endPieces[1].SetAsDoor(true);

            while (_isWaiting)
            {
                yield return null;
            }
            _isWaiting = true;
        }

        ECardinal GetCardinal(int xDiff, int yDiff)
        {
            if (xDiff == 0)
            {
                if (yDiff > 0)
                    return ECardinal.N;
                else
                    return ECardinal.S;
            }
            else if (xDiff > 0)
            {
                return ECardinal.W;
            }
            else
            {
                return ECardinal.E;
            }
        }

        ECardinal RotateCardinal(ECardinal direction, bool isClockwise)
        {
            if (isClockwise)
            {
                return (ECardinal)(((int)direction + 1) % 4);
            }

            return (ECardinal)(((int)direction + 3) % 4);
        }

        ECardinal FlipCardinal(ECardinal direction)
        {
            return (ECardinal)(((int)direction + 2) % 4);
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

                cell.SetPosition(i % _width, i / _width);

                go.transform.parent = _gridParent;
            }

            _pathfinding.CreateNetwork(_width, _height, _map);
        }

        List<MapTile> PickPrimaryRooms(int rooms)
        {
            // Pick random squares to act as primary rooms
            List<MapTile> options = new List<MapTile>(_map);
            List<MapTile> primary = new List<MapTile>();
            for (int i = 0; i < rooms; i++)
            {
                int rand = Random.Range(0, options.Count);

                MapTile option = options[rand];
                option.UpdateCellType(ECellType.PrimaryRoom);
                primary.Add(option);
                options.RemoveAt(rand);

                int useBorder = Random.Range(0, 2);
                if (useBorder == 0)
                {
                    List<MapTile> adjacentCells = new List<MapTile>();
                    for (int j = 0; j < _map.Count; j++)
                    {
                        MapTile other = _map[j];
                        if (Mathf.Abs(other.X - option.X) <= 1 && Mathf.Abs(other.Y - option.Y) <= 1)
                        {
                            if (other.X != option.X || other.Y != option.Y)
                            {
                                other.UpdateCellType(ECellType.Border);
                                adjacentCells.Add(other);
                            }
                        }
                    }

                    foreach (var cell in adjacentCells)
                    {
                        options.Remove(cell);
                    }
                }
            }

            return primary;
        }

        public List<Connection> ConnectNodes(List<MapTile> primary)
        {
            List<MapTile> failedPaths = new List<MapTile>();

            // Create list of directly connected nodes
            List<Connection> connections = new List<Connection>();
            List<MapTile> unused = new List<MapTile>(primary);
            while (unused.Count > 0)
            {
                MapTile next = unused[0];
                unused.Remove(next);

                MapTile nearest = _pathfinding.FindNearestNode(next, primary);

                List<MapTile> path = _pathfinding.FindPath(next, nearest);

                if(path == null)
                {
                    Debug.Log("Failed to create path to nearest node");
                    failedPaths.Add(next);
                }
                else
                {
                    if (unused.Contains(nearest))
                    {
                        unused.Remove(nearest);
                    }

                    connections.Add(new Connection(next, nearest, path));
                }
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
                graph.CompileCells(Color.white);
            }

            return updatedGraphs;
        }

        public void ConnectGraphs(List<Graph> updatedGraphs)
        {
            int count = 0;
            while (updatedGraphs.Count > 1 && count < 100)
            {
                count++;
                //Debug.Log("Number of graphs = " + updatedGraphs.Count);

                Graph first = updatedGraphs[0];
                updatedGraphs.Remove(first);

                Graph nearest = null;
                MapTile bestFirst = null;
                MapTile bestSecond = null;
                float bestDist = float.MaxValue;
                foreach (var graph in updatedGraphs)
                {
                    foreach (var square in first.AllCells)
                    {
                        foreach (var other in graph.AllCells)
                        {
                            float distance = _pathfinding.EvaluateH(square, other);
                            if (distance < bestDist)
                            {
                                bestDist = distance;
                                nearest = graph;
                                bestFirst = square;
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
                    if (updatedGraphs.Count == 1)
                    {
                        graph.CompileCells(Color.white);
                    }
                    else
                    {
                        graph.CompileCells(graph._color);
                    }
                }
            }
        }

        Graph MergeGraphs(Graph first, MapTile firstNode, Graph second, MapTile secondNode)
        {
            List<MapTile> path = _pathfinding.FindPath(firstNode, secondNode);

            Connection merger = new Connection(firstNode, secondNode, path);

            Graph newGraph = new Graph();
            newGraph._color = first._color;
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

        void UpdateTiles()
        {
            foreach (var tile in _map)
                tile.UpdateTileProximity(_map, _width, _height);
        }

        //
        // Returns a list of map tiles that can be used to spin off branches
        //
        List<MapTile> GetBranchableEdges()
        {
            return _map.FindAll(x => DungeonTileData.WallPieces.Contains(x.TileID) || DungeonTileData.HallPieces.Contains(x.TileID) || DungeonTileData.TurnPieces.Contains(x.TileID));
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

            ECardinal direction = GetCardinal(xDiff, yDiff);

            while (neighbor != null && stepCount > 0)
            {
                neighbor.UpdateCellType(ECellType.Hallway);

                current.ConnectCardinally(direction);
                current.UpdateTileIDByConnectedNeighbors(_map, _width, _height);
                neighbor.ConnectCardinally(FlipCardinal(direction));
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

                ECardinal direction = GetCardinal(xDiff, yDiff);

                neighbor.UpdateCellType(ECellType.Hallway);

                current.ConnectCardinally(direction);
                current.UpdateTileIDByConnectedNeighbors(_map, _width, _height);
                neighbor.ConnectCardinally(FlipCardinal(direction));
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

            int xDiff = current.X - neighbor.X;
            int yDiff = current.Y - neighbor.Y;

            ECardinal direction = GetCardinal(xDiff, yDiff);
            bool connected = false;

            while (neighbor != null)
            {
                if (!connected)
                {
                    neighbor.UpdateCellType(ECellType.Hallway);
                }

                current.ConnectCardinally(direction);
                current.UpdateTileIDByConnectedNeighbors(_map, _width, _height);
                neighbor.ConnectCardinally(FlipCardinal(direction));
                neighbor.UpdateTileIDByConnectedNeighbors(_map, _width, _height);

                if (connected)
                    break;

                current = neighbor;
                neighbor = current.GetTileInDirection(_map, _width, _height, direction);

                if (neighbor != null && neighbor.CellType != ECellType.Unoccupied)
                {
                    connected = true;
                }
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

            ECardinal direction = GetCardinal(xDiff, yDiff);
            bool connected = false;

            bool clockwise = Random.Range(0, 2) == 0;

            while (neighbor != null)
            {
                if (!connected)
                {
                    neighbor.UpdateCellType(ECellType.Hallway);
                }

                current.ConnectCardinally(direction);
                neighbor.ConnectCardinally(FlipCardinal(direction));

                if (connected)
                    break;

                if (Random.Range(0, 4) == 0)
                {
                    direction = RotateCardinal(direction, clockwise);
                }

                current = neighbor;
                neighbor = current.GetTileInDirection(_map, _width, _height, direction);

                if (neighbor != null)
                {
                    toUpdate.Add(neighbor);
                    if (neighbor.CellType != ECellType.Unoccupied)
                    {
                        connected = true;
                    }
                }
            }

            foreach (var cell in toUpdate)
                cell.UpdateTileIDByConnectedNeighbors(_map, _width, _height);

            return true;
        }
    }
}