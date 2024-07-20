using System.Collections.Generic;

public class Zone
{
    public uint Id { get; set; }
    readonly HashSet<Vertex> vertices = new();
    public ReadOnlySet<Vertex> Vertices { get => vertices.AsReadOnly(); }

    public Dictionary<uint, int> Destinations = new();
    public float CarSpawnInterval;

    public Zone(uint id)
    {
        Id = id;
    }

    public void AddVertex(Vertex v)
    {
        vertices.Add(v);
    }

}