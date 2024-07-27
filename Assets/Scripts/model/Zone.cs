using System.Collections.Generic;

public class Zone
{
    public uint Id { get; set; }
    readonly HashSet<Vertex> vertices = new();
    public ReadOnlySet<Vertex> Vertices { get => vertices.AsReadOnly(); }
    public ZoneType Type { get; set; }

    public Dictionary<uint, int> Destinations = new();
    public float CarSpawnInterval;

    public Zone(uint id, ZoneType zoneType)
    {
        Id = id;
        Type = zoneType;
    }

    public void AddVertex(Vertex v)
    {
        vertices.Add(v);
    }

    public void RemoveVertex(Vertex v)
    {
        vertices.Remove(v);
    }
}