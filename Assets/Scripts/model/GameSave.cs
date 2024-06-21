using System.Collections.Generic;
using QuikGraph;
using Unity.Plastic.Newtonsoft.Json;

public class GameSave
{
    [JsonProperty]
    public SortedDictionary<ulong, Node> Nodes { get; private set; }
    [JsonProperty]
    public SortedDictionary<ulong, Road> Roads { get; private set; }
    [JsonIgnore]
    public AdjacencyGraph<Vertex, Path> Graph { get; private set; }
    public List<Path> GraphSave { get; set; }
    public SortedDictionary<ulong, Intersection> Intersections { get; private set; }
    [JsonIgnore]
    public SortedDictionary<ulong, Zone> Zones { get; private set; }
    [JsonIgnore]
    public SortedDictionary<ulong, Car> Cars { get; private set; }
    public ulong NextAvailableRoadId { get; set; }
    public ulong NextAvailableNodeId { get; set; }
    public ulong NextAvailablePathId { get; set; }
    public ulong NextAvailableIntersectionId { get; set; }
    [JsonIgnore]
    public ulong NextAvailableCarId { get; set; }
    public float Elevation { get; set; }
    public ulong CarServiced { get; set; }
    public GameSave()
    {
        Roads = new();
        Nodes = new();
        Graph = new(false);
        Intersections = new();
        Zones = new();
        Cars = new();
        NextAvailableNodeId = 1;
        NextAvailableRoadId = 1;
        NextAvailablePathId = 1;
        NextAvailableCarId = 1;
        NextAvailableIntersectionId = 1;
        CarServiced = 0;
    }
}