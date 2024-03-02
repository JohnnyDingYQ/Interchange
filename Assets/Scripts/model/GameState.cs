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

    public int NextAvailableRoadId { get; set; }
    public int NextAvailableNodeId { get; set; }

    public GameState()
    {
        NextAvailableNodeId = 0;
        NextAvailableRoadId = 0;
    }
}