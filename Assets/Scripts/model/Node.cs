using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

public class Node : IComparable<Node>
{
    public int Id { get; set; }
    [JsonProperty]
    private readonly HashSet<Lane> inLanes;
    [JsonProperty]
    private readonly HashSet<Lane> outLanes;
    [JsonIgnore]
    public HashSet<Lane> Lanes
    {
        get
        {
            HashSet<Lane> h = new();
            h.UnionWith(inLanes);
            h.UnionWith(outLanes);
            return h;
        }
    }
    [JsonProperty]
    public float3 Pos { get; private set; }
    [JsonProperty]
    public int NodeIndex { get; private set; }
    public Intersection Intersection { get; set; }

    public Node() { }
    public Node(float3 pos, int order)
    {
        Pos = pos;
        NodeIndex = order;
        Id = -1;
        inLanes = new();
        outLanes = new();
    }

    /// <summary>
    /// Implicit assumption: all lanes at the node has the same tangent because of pivot adjustment
    /// </summary>
    public float3 GetTangent()
    {
        if (Lanes.Count == 0)
            throw new InvalidOperationException("Node has no lane... Cannot get tangent");
        if (GetRoads(Direction.In).Count() != 0)
        {
            BezierSeries bs = GetRoads(Direction.In).First().BezierSeries;
            return bs.EvaluateTangent(1);
        }
        BezierSeries bt = GetRoads(Direction.Out).First().BezierSeries;
        return bt.EvaluateTangent(0);
    }

    public void AddLane(Lane lane, Direction direction)
    {
        if (inLanes.Contains(lane) || outLanes.Contains(lane))
            throw new InvalidOperationException("lane already exists");
        if (direction == Direction.In)
            inLanes.Add(lane);
        else if (direction == Direction.Out)
            outLanes.Add(lane);
        else
            throw new ArgumentException("direction");
    }

    public bool RemoveLane(Lane lane)
    {
        if (inLanes.Contains(lane))
        {
            inLanes.Remove(lane);
            return true;
        }
        else if (outLanes.Contains(lane))
        {
            outLanes.Remove(lane);
            return true;
        }
        return false;
    }

    // O(n) operation but meh
    public ReadOnlySet<Lane> GetLanes(Direction direction)
    {
        if (direction == Direction.In)
            return inLanes.AsReadOnly();
        if (direction == Direction.Out)
            return outLanes.AsReadOnly();
        if (direction == Direction.Both)
            return Lanes.AsReadOnly();
        throw new ArgumentException("direction");
    }

    public IEnumerable<Road> GetRoads(Direction direction)
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