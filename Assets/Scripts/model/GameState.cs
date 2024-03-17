using System.Collections.Generic;

public class GameState
{
    private SortedDictionary<int, Road> roads;
    public SortedDictionary<int, Road> Roads
    {
        get
        {
            roads ??= new();
            return roads;
        }
        set
        {
            roads = value;
        }
    }
    private SortedDictionary<int, Node> nodes;
    public SortedDictionary<int, Node> Nodes {
        get {
            nodes ??= new();
            return nodes;
        }
        set {
            nodes = value;
        }
    }

    private List<Path> paths;
    public List<Path> Paths
    {
        get
        {
            paths ??= new();
            return paths;
        }
        set
        {
            paths = value;
        }
    }

    public int NextAvailableRoadId { get; set; }
    public int NextAvailableNodeId { get; set; }

    public GameState()
    {
        NextAvailableNodeId = 1;
        NextAvailableRoadId = 1;
    }
}