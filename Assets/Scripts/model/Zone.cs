using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Model.Roads;
using UnityEngine;
using UnityEngine.Assertions;

public class Zone : IPersistable
{
    public uint Id { get; set; }
    public bool Enabled { get; set; }
    [SaveIDCollection]
    protected HashSet<Vertex> vertices = new();
    [NotSaved]
    readonly Dictionary<Zone, Path> connectedTargets;
    [NotSaved]
    public ReadOnlySet<Vertex> Vertices { get => vertices.AsReadOnly(); }
    [NotSaved]
    public IEnumerable<Zone> ConnectedZones { get => connectedTargets.Keys; }
    [NotSaved]
    public const int DistrictBitWidth = 6;

    public Zone() { connectedTargets = new(); }

    public Zone(uint id)
    {
        Enabled = true;
        Id = id;
        connectedTargets = new();
    }

    public void AddVertex(Vertex v)
    {
        if (Enabled)
            vertices.Add(v);
    }

    public void AddVertex(IEnumerable<Vertex> v)
    {
        if (Enabled)
            vertices.UnionWith(v);
    }

    public void RemoveVertex(Vertex v)
    {
        if (Enabled)
            vertices.Remove(v);
    }

    public void SetPathTo(Zone target, Path path)
    {
        if (connectedTargets.ContainsKey(target))
            RemovePathTo(target);
        IncreaseEdgeCost(path);
        connectedTargets.Add(target, path);
    }

    public void RemovePathTo(Zone target)
    {
        Assert.IsTrue(connectedTargets.ContainsKey(target));
        DecreaseEdgeCost(connectedTargets[target]);
        connectedTargets.Remove(target);
    }

    public bool TryGetPathTo(Zone target, out Path path)
    {
        return connectedTargets.TryGetValue(target, out path);
    }

    void IncreaseEdgeCost(Path path)
    {
        foreach (Edge edge in path.Edges)
            edge.CostFactor += Constants.EdgeCostIncreaseForPath;
    }

    void DecreaseEdgeCost(Path path)
    {
        foreach (Edge edge in path.Edges)
            edge.CostFactor -= Constants.EdgeCostIncreaseForPath;
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