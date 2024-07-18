using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

public class Node : IComparable<Node>
{
    public uint Id { get; set; }
    [JsonIgnore]
    public Lane InLane { get; set; }
    [JsonIgnore]
    public Lane OutLane { get; set; }
    public uint InLane_ { get; set; }
    public uint OutLane_ { get; set; }
    [JsonProperty]
    public float3 Pos { get; private set; }
    [JsonProperty]
    public int NodeIndex { get; private set; }
    [JsonIgnore]
    public Intersection Intersection { get; set; }
    public bool BelongsToPoint { get; set; }
    public Node() { }
    public Node(float3 pos, float elevation, int order)
    {
        pos.y = elevation;
        Pos = pos;
        NodeIndex = order;
        Id = 0;
    }

    public HashSet<Road> GetRoads()
    {
        HashSet<Road> r = new();
        if (InLane != null)
            r.Add(InLane.Road);
        if (OutLane != null)
            r.Add(OutLane.Road);
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