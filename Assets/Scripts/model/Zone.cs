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
    Dictionary<Zone, HashSet<Path>> connectedTargets;
    [NotSaved]
    public ReadOnlySet<Vertex> Vertices { get => vertices.AsReadOnly(); }
    [NotSaved]
    public HashSet<Zone> ConnectedZones { get; set; }
    [NotSaved]
    public const int DistrictBitWidth = 6;

    public Zone() { ConnectedZones = new(); }

    public Zone(uint id)
    {
        Enabled = true;
        Id = id;
        ConnectedZones = new();
    }

    public void InitConnectedTargets(IEnumerable<Zone> otherZones)
    {
        connectedTargets = new();
        foreach (Zone zone in otherZones)
            connectedTargets.Add(zone, new());
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

    public void AddPathTo(Zone target, Path path)
    {
        Assert.IsNotNull(connectedTargets);
        connectedTargets[target].Add(path);
        ConnectedZones.Add(target);
    }

    public void RemovePathTo(Zone target, Path path)
    {
        Assert.IsNotNull(connectedTargets);
        Assert.IsTrue(connectedTargets[target].Contains(path));
        connectedTargets[target].Remove(path);
        if (connectedTargets[target].Count == 0)
            ConnectedZones.Remove(target);
    }

    public void ClearPath()
    {
        Assert.IsNotNull(connectedTargets);
        foreach (var set in connectedTargets.Values)
            set.Clear();
        ConnectedZones.Clear();
    }

    public HashSet<Path> GetPathsTo(Zone target)
    {
        return connectedTargets[target];
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