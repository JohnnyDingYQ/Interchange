using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;
using Unity.Plastic.Newtonsoft.Json;
using System;

public class Intersection
{
    public ulong Id { get; set; }
    [JsonProperty]
    private readonly List<Node> nodes = new();
    [JsonIgnore]
    public List<Node> Nodes { get { return new List<Node>(nodes); } }
    [JsonIgnore]
    public int Count { get { return Nodes.Count; } }
    [JsonProperty]
    private readonly HashSet<Road> inRoads = new();
    [JsonIgnore]
    public ReadOnlySet<Road> InRoads { get { return inRoads.AsReadOnly(); } }
    [JsonProperty]
    private readonly HashSet<Road> outRoads = new();
    [JsonIgnore]
    public ReadOnlySet<Road> OutRoads { get { return outRoads.AsReadOnly(); } }
    [JsonIgnore]
    public HashSet<Road> Roads { get { return GetRoads(); } }
    [JsonProperty]
    public Plane Plane { get; private set; }
    [JsonProperty]
    public float3 Normal { get; private set; }
    [JsonProperty]
    public float3 Tangent { get; private set; }
    [JsonProperty]
    public float3 PointOnInSide { get; private set; }

    public Intersection() { }

    public Intersection(Road road, Side side)
    {
        AddRoad(road, side);
        Normal = GetAttribute(AttributeTypes.Normal);
        Tangent = GetAttribute(AttributeTypes.Tangent);
        PointOnInSide = GetAttribute(AttributeTypes.PointOnInSide);
        Plane = GetPlane();
    }

    public void AddRoad(Road road, Side side)
    {
        if (side != Side.Start && side != Side.End)
            throw new InvalidOperationException("Invalid side");

        if (side == Side.Start)
            outRoads.Add(road);
        else if (side == Side.End)
            inRoads.Add(road);

        foreach (Node n in road.GetNodes(side))
        {
            n.Intersection = this;
            if (!nodes.Contains(n))
                nodes.Add(n);
        }
        nodes.Sort();
    }

    public void RemoveRoad(Road road, Side side)
    {
        if (side != Side.Start && side != Side.End)
            throw new InvalidOperationException("Invalid side");

        if (side == Side.Start)
            outRoads.Remove(road);
        else if (side == Side.End)
            inRoads.Remove(road);
    }

    public bool IsEmpty()
    {
        return nodes.Count == 0 && inRoads.Count == 0 && outRoads.Count == 0;
    }

    public void RemoveNode(Node node)
    {
        nodes.Remove(node);
    }

    float3 GetAttribute(AttributeTypes attributeType)
    {
        if (inRoads.Count != 0)
        {
            Road randomInRoad = inRoads.First();
            BezierSeries bs = randomInRoad.BezierSeries;
            if (attributeType == AttributeTypes.Normal)
                return bs.Evaluate2DNormalizedNormal(1);
            if (attributeType == AttributeTypes.Tangent)
                return math.normalize(bs.EvaluateTangent(1));
            if (attributeType == AttributeTypes.PointOnInSide)
                return bs.EvaluatePosition(1) - math.normalize(bs.EvaluateTangent(1));
        }
        if (outRoads.Count != 0)
        {
            Road randomOutRoad = outRoads.First();
            BezierSeries bs = randomOutRoad.BezierSeries;
            if (attributeType == AttributeTypes.Normal)
                return bs.Evaluate2DNormalizedNormal(0);
            if (attributeType == AttributeTypes.Tangent)
                return math.normalize(bs.EvaluateTangent(0));
            if (attributeType == AttributeTypes.PointOnInSide)
                return bs.EvaluatePosition(0) - math.normalize(bs.EvaluateTangent(0));
        }
        throw new InvalidOperationException("intersection is empty");
    }

    Plane GetPlane()
    {
        if (inRoads.Count != 0)
        {
            Road randomInRoad = inRoads.First();
            return new(randomInRoad.EndPos, randomInRoad.EndPos + Normal, randomInRoad.EndPos - new float3(0, 1, 0));
        }
        else if (outRoads.Count != 0)
        {
            Road randomOutRoad = outRoads.First();
            return new(randomOutRoad.StartPos, randomOutRoad.StartPos + Normal, randomOutRoad.EndPos - new float3(0, 1, 0));
        }
        throw new InvalidOperationException("intersection is empty");
    }

    private enum AttributeTypes { Normal, PointOnInSide, Tangent }

    public HashSet<Road> GetRoads()
    {
        HashSet<Road> h = new(inRoads);
        h.UnionWith(outRoads);
        return h;
    }

    public Node FirstNodeWithRoad(Direction direction)
    {
        foreach (Node n in nodes)
            if (n.GetLanes(direction).Count != 0)
                return n;
        return null;
    }

    public Node LastNodeWithRoad(Direction direction)
    {
        for (int i = nodes.Count - 1; i >= 0; i--)
            if (nodes[i].GetLanes(direction).Count != 0)
                return nodes[i];
        return null;
    }

 
}