using System.Collections.Generic;
using QuikGraph;
using Unity.Plastic.Newtonsoft.Json;

public class GameSave
{
    [JsonProperty]
    public Dictionary<ulong, Node> Nodes { get; private set; }
    [JsonProperty]
    public Dictionary<ulong, Road> Roads { get; private set; }
    [JsonIgnore]
    public AdjacencyGraph<Vertex, Path> Graph { get; private set; }
    public List<Path> GraphSave { get; set; }
    public Dictionary<ulong, Intersection> Intersections { get; private set; }
    public Dictionary<ulong, Lane> Lanes { get; private set; }
    [JsonIgnore]
    public Dictionary<ulong, Zone> Zones { get; private set; }
    [JsonIgnore]
    public Dictionary<ulong, Car> Cars { get; private set; }
    public float Elevation { get; set; }
    public ulong CarServiced { get; set; }
    public GameSave()
    {
        Lanes = new();
        Roads = new();
        Nodes = new();
        Graph = new(false);
        Intersections = new();
        Zones = new();
        Cars = new();
        CarServiced = 0;
    }
}