using System.Collections.Generic;
using QuikGraph;

public static class Game
{
    public static ISaveSystemBoundary SaveSystem { get; set; }
    public static GameState GameState { get; set; }

    public static SortedDictionary<int, Road> Roads
    {
        get
        {
            return GameState.Roads;
        }
    }

    public static SortedDictionary<int, Node> Nodes
    {
        get
        {
            return GameState.Nodes;
        }
    }

    public static AdjacencyGraph<Vertex, Path> Graph
    {
        get
        {
            return GameState.Graph;
        }
    }

    public static int NextAvailableNodeId
    {
        get
        {
            return GameState.NextAvailableNodeId;
        }
        set
        {
            GameState.NextAvailableNodeId = value;
        }
    }
    public static int NextAvailableRoadId
    {
        get
        {
            return GameState.NextAvailableRoadId;
        }
        set
        {
            GameState.NextAvailableRoadId = value;
        }
    }

    static Game()
    {
        GameState = new();
    }

    public static void WipeState()
    {
        GameState = new();
    }

    public static void SaveGame()
    {
        SaveSystem.SaveGame();
    }

    public static void LoadGame()
    {
        SaveSystem.LoadGame();
        BuildHandler.ComplyToNewGameState();
    }

    public static void RegisterRoad(Road road)
    {
        road.Id = NextAvailableRoadId++;
        Roads.Add(road.Id, road);
    }

    public static void RegisterNode(Node node)
    {
        node.Id = NextAvailableNodeId++;
        Nodes[node.Id] = node;
    }

    public static bool RemoveRoad(Road road)
    {
        if (!Roads.ContainsKey(road.Id))
            return false;
        Roads.Remove(road.Id);
        foreach (Lane lane in road.Lanes)
        {
            Graph.RemoveEdgeIf(e => e.Source == lane.EndVertex && e.Target == lane.StartVertex);
            foreach (Node node in new List<Node>() { lane.StartNode, lane.EndNode })
            {
                node.RemoveLane(lane);
                if (node.Lanes.Count == 0)
                    Nodes.Remove(node.Id);
            }
        }
        return true;
    }

    public static void AddVertex(Vertex vertex)
    {
        Graph.AddVertex(vertex);
    }

    public static void AddEdge(Path path)
    {
        Graph.AddEdge(path);
    }

    public static bool HasEdge(Lane from, Lane to)
    {
        return HasEdge(from.EndVertex, to.StartVertex);
    }

    public static bool HasEdge(Vertex from, Vertex to)
    {
        if (Graph.ContainsEdge(from, to))
            return true;
        return false;
    }
}