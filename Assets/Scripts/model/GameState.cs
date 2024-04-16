using System.Collections.Generic;
using QuikGraph;
using Unity.Plastic.Newtonsoft.Json;

public class GameState
{
    public SortedDictionary<int, Road> Roads { get; set; }
    public SortedDictionary<int, Node> Nodes { get; set; }
    [JsonIgnore]
    public AdjacencyGraph<Vertex, Path> Graph { get; set; }
    public List<Path> GraphSave { get; set; }
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