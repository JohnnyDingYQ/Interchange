using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    public override bool Equals(object obj)
    {
        if (obj is Zone other)
            return Id == other.Id && Enabled == other.Enabled && vertices.Select(v => Id).SequenceEqual(other.vertices.Select(v => Id));
        else
            return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}