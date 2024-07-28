using System.Collections.Generic;
using QuikGraph;

public class GameSave
{
    public Dictionary<uint, Node> Nodes { get; private set; }
    public Dictionary<uint, Road> Roads { get; private set; }
    public AdjacencyGraph<Vertex, Path> Graph { get; private set; }
    public Dictionary<uint, Intersection> Intersections { get; private set; }
    public Dictionary<uint, Lane> Lanes { get; private set; }
    public Dictionary<uint, Vertex> Vertices { get; private set; }
    public Dictionary<uint, Path> Paths { get; private set; }
    public Dictionary<uint, Car> Cars { get; private set; }
    public uint CarServiced { get; set; }
    public GameSave()
    {
        Vertices = new();
        Paths = new();
        Lanes = new();
        Roads = new();
        Nodes = new();
        Graph = new(false);
        Intersections = new();
        Cars = new();
        CarServiced = 0;
    }
}