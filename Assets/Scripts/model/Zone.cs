using System.Collections.Generic;

public class Zone : IPersistable
{
    public uint Id { get; set; }
    [SaveIDCollection]
    protected HashSet<Vertex> vertices = new();
    [NotSaved]
    public ReadOnlySet<Vertex> Vertices { get => vertices.AsReadOnly(); }

    public Zone() { }

    public Zone(uint id)
    {
        Id = id;
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