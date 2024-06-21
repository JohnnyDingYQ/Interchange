using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

public class Node : IComparable<Node>
{
    public ulong Id { get; set; }
    [JsonIgnore]
    public HashSet<Lane> InLanes { get; set; }
    [JsonIgnore]
    public HashSet<Lane> OutLanes { get; set; }
    public List<ulong> InLanes_ { get; set; }
    public List<ulong> OutLanes_ { get; set; }
    [JsonIgnore]
    public HashSet<Lane> Lanes
    {
        get
        {
            HashSet<Lane> h = new();
            h.UnionWith(InLanes);
            h.UnionWith(OutLanes);
            return h;
        }
    }
    [JsonProperty]
    public float3 Pos { get; private set; }
    [JsonProperty]
    public int NodeIndex { get; private set; }
    public Intersection Intersection { get; set; }

    public Node() { }
    public Node(float3 pos, float elevation, int order)
    {
        pos.y = elevation;
        Pos = pos;
        NodeIndex = order;
        Id = 0;
        InLanes = new();
        OutLanes = new();
    }

    public void AddLane(Lane lane, Direction direction)
    {
        if (InLanes.Contains(lane) || OutLanes.Contains(lane))
            throw new InvalidOperationException("lane already exists");
        if (direction == Direction.In)
            InLanes.Add(lane);
        else if (direction == Direction.Out)
            OutLanes.Add(lane);
        else
            throw new ArgumentException("direction");
    }

    public bool RemoveLane(Lane lane)
    {
        if (InLanes.Contains(lane))
        {
            InLanes.Remove(lane);
            return true;
        }
        else if (OutLanes.Contains(lane))
        {
            OutLanes.Remove(lane);
            return true;
        }
        return false;
    }

    // O(n) operation but meh
    public ReadOnlySet<Lane> GetLanes(Direction direction)
    {
        if (direction == Direction.In)
            return InLanes.AsReadOnly();
        if (direction == Direction.Out)
            return OutLanes.AsReadOnly();
        if (direction == Direction.Both)
            return Lanes.AsReadOnly();
        throw new ArgumentException("direction");
    }

    public HashSet<Road> GetRoads(Direction direction)
    {
        HashSet<Road> r = new();
        foreach (Lane l in GetLanes(direction))
            r.Add(l.Road);
        return r;
    }

    public int CompareTo(Node other)
    {
        return NodeIndex.CompareTo(other.NodeIndex);
    }

    public override string ToString()
    {
        return "Node " + Id;
    }

}