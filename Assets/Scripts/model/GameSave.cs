using System.Collections.Generic;
using QuikGraph;
using Unity.Plastic.Newtonsoft.Json;

public class GameSave
{
    [JsonProperty]
    public SortedDictionary<int, Node> Nodes { get; private set; }
    [JsonProperty]
    public SortedDictionary<int, Road> Roads { get; private set; }
    [JsonIgnore]
    public AdjacencyGraph<Vertex, Path> Graph { get; private set; }
    public List<Path> GraphSave { get; set; }
    [JsonIgnore]
    public SortedDictionary<int, Zone> Zones { get; private set; }
    public int NextAvailableRoadId { get; set; }
    public int NextAvailableNodeId { get; set; }
    public int Elevation { get; set; }
    public GameSave()
    {
        Roads = new();
        Nodes = new();
        Graph = new(false);
        Zones = new();
        NextAvailableNodeId = 1;
        NextAvailableRoadId = 1;
    }
}