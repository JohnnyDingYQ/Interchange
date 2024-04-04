using System.Collections.Generic;
using QuikGraph;

public class GameState
{
    public SortedDictionary<int, Road> Roads { get; set; }
    public SortedDictionary<int, Node> Nodes { get; set; }
    public AdjacencyGraph<Vertex, Path> Graph { get; set; }
    public int NextAvailableRoadId { get; set; }
    public int NextAvailableNodeId { get; set; }
    public GameState()
    {
        Roads = new();
        Nodes = new();
        Graph = new();
        NextAvailableNodeId = 1;
        NextAvailableRoadId = 1;
    }
}