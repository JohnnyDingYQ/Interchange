using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Splines;


public class Node : IComparable<Node>
{
    public int Id { get; set; }
    [JsonProperty]
    private readonly HashSet<Lane> lanes;
    [JsonIgnore]
    public ReadOnlySet<Lane> Lanes
    {
        get
        {
            return lanes.AsReadOnly();
        }
    }
    public float3 Pos { get; set; }
    public int Order { get; set; }
    private readonly Dictionary<Lane, Direction> directions;

    public Node() { }

    public Node(float3 pos, int order)
    {
        Pos = pos;
        Order = order;
        Id = -1;
        lanes = new();
        directions = new();
    }

    /// <summary>
    /// Implicit assumption: all lanes at the node has the same tangent because of pivot adjustment
    /// </summary>
    public float3 GetTangent()
    {
        if (Lanes.Count == 0)
        {
            throw new InvalidOperationException("Node has no lane... Cannot get tangent");
        }
        Lane lane = Lanes.First();
        if (lane.StartNode == this)
        {
            return lane.Spline.EvaluateTangent(0);
        }
        return lane.Spline.EvaluateTangent(1);
    }

    public void AddLane(Lane lane, Direction direction)
    {
        lanes.Add(lane);
        directions[lane] = direction;
    }

    public bool RemoveLane(Lane lane)
    {
        if (!lanes.Contains(lane))
            return false;
        lanes.Remove(lane);
        directions.Remove(lane);
        return true;
    }

    public Direction GetDirection(Lane lane)
    {
        return directions[lane];
    }

    // O(n) operation but meh
    public List<Lane> GetLanes(Direction direction)
    {
        List<Lane> r = new();
        foreach(Lane lane in directions.Keys)
        {
            if (directions[lane] == direction)
                r.Add(lane);
        }
        return r;
    }

    public int CompareTo(Node other)
    {
        return Order.CompareTo(other.Order);
    }

    public bool IsRegistered()
    {
        return Id != -1;
    }

    public bool LanesSetEquals(HashSet<Lane> h)
    {
        return lanes.SetEquals(h);
    }

    public override string ToString()
    {
        return "Node " + Id;
    }

}