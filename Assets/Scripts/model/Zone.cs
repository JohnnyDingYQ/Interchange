using System.Collections.Generic;

public class Zone : IPersistable
{
    public uint Id { get; set; }
    public bool Enabled { get; set; }
    [SaveIDCollection]
    protected HashSet<Vertex> vertices = new();
    [NotSaved]
    public ReadOnlySet<Vertex> Vertices { get => vertices.AsReadOnly(); }
    [NotSaved]
    public const int DistrictBitWidth = 6;

    public Zone() { }

    public Zone(uint id)
    {
        Enabled = true;
        Id = id;
    }

    public void AddVertex(Vertex v)
    {
        if (Enabled)
            vertices.Add(v);
    }

    public void RemoveVertex(Vertex v)
    {
        if (Enabled)
            vertices.Remove(v);
    }
}