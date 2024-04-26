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
    private readonly HashSet<Lane> lanes;
    [JsonIgnore]
    public ReadOnlySet<Lane> Lanes { get { return lanes.AsReadOnly(); } }
    [JsonProperty]
    public float3 Pos { get; private set; }
    [JsonProperty]
    public int NodeIndex { get; private set; }
    [JsonProperty]
    public List<KeyValuePair<Lane, Direction>> SerializedDirections
    {
        get { return directions.ToList(); }
        set { directions = value.ToDictionary(x => x.Key, x => x.Value); }
    }
    private Dictionary<Lane, Direction> directions;


    public Node(float3 pos, int order)
    {
        Pos = pos;
        NodeIndex = order;
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
        foreach (Lane lane in directions.Keys)
        {
            if (directions[lane] == direction)
                r.Add(lane);
        }
        return r;
    }

    public bool HasLanes(Direction direction)
    {
        return GetLanes(direction).Count != 0;
    }

    public IEnumerable<Road> GetRoads(Direction direction)
    {
        HashSet<Road> r = new();
        foreach (Lane lane in directions.Keys)
        {
            if (directions[lane] == direction)
                r.Add(lane.Road);
        }
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