using System.Collections.Generic;
using QuikGraph;
using Unity.Plastic.Newtonsoft.Json;

public class GameSave
{
    [JsonProperty]
    public Dictionary<uint, Node> Nodes { get; private set; }
    [JsonProperty]
    public Dictionary<uint, Road> Roads { get; private set; }
    [JsonIgnore]
    public AdjacencyGraph<Vertex, Path> Graph { get; private set; }
    public List<Path> GraphSave { get; set; }
    public Dictionary<uint, Intersection> Intersections { get; private set; }
    public Dictionary<uint, Lane> Lanes { get; private set; }
    [JsonIgnore]
    public Dictionary<uint, Zone> Zones { get; private set; }
    [JsonIgnore]
    public Dictionary<uint, Car> Cars { get; private set; }
    public float Elevation { get; set; }
    public uint CarServiced { get; set; }
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