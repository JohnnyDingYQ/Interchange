using System.Collections.Generic;
using QuikGraph;
using Unity.Plastic.Newtonsoft.Json;

public class GameState
{
    [JsonProperty]
    public SortedDictionary<int, Road> Roads { get; private set; }
    [JsonProperty]
    public SortedDictionary<int, Node> Nodes { get; private set; }
    [JsonIgnore]
    public AdjacencyGraph<Vertex, Path> Graph { get; private set; }
    public List<Path> GraphSave { get; set; }
    public int NextAvailableRoadId { get; set; }
    public int NextAvailableNodeId { get; set; }
    public GameState()
    {
        Roads = new();
        Nodes = new();
        Graph = new(false);
        NextAvailableNodeId = 1;
        NextAvailableRoadId = 1;
    }
}