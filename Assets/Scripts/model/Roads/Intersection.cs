using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;
using Unity.Plastic.Newtonsoft.Json;
using System;
using UnityEngine.Assertions;

public class Intersection
{
    public uint Id { get; set; }
    [JsonIgnore]
    private List<Node> nodes = new();
    [JsonIgnore]
    public List<Node> Nodes { get => new(nodes); }
    [JsonProperty]
    private HashSet<Road> inRoads = new();
    [JsonIgnore]
    public ReadOnlySet<Road> InRoads { get => inRoads.AsReadOnly(); }
    [JsonProperty]
    private HashSet<Road> outRoads = new();
    [JsonIgnore]
    public ReadOnlySet<Road> OutRoads { get => outRoads.AsReadOnly(); }
    [JsonIgnore]
    public HashSet<Road> Roads { get => GetRoads(); }
    [JsonIgnore]
    public Plane Plane { get => GetPlane(); }
    [JsonIgnore]
    public float3 Normal { get => GetAttribute(AttributeTypes.Normal); }
    [JsonIgnore]
    public float3 Tangent { get => GetAttribute(AttributeTypes.Tangent); }
    [JsonIgnore]
    public float3 PointOnInSide { get => GetAttribute(AttributeTypes.PointOnInSide); }
    [JsonProperty]
    public List<uint> Nodes_ { get; set; }
    [JsonProperty]
    public List<uint> InRoads_ { get; set; }
    [JsonProperty]
    public List<uint> OutRoads_ { get; set; }

    public Intersection() { }

    public Intersection(Road road, Direction direction)
    {
        AddRoad(road, direction);
    }

    public void SetNodes(List<Node> nodes)
    {
        this.nodes = nodes;
        foreach (Node node in nodes)
            node.Intersection = this;
    }

    public void AddNode(int nodeIndex)
    {
        Assert.AreNotEqual(0, nodes.Count);
        foreach (Node n in nodes)
            if (n.NodeIndex == nodeIndex)
                return;
        int firstIndex = nodes.First().NodeIndex;
        Node newNode = new(
            nodes.First().Pos + Normal * Constants.LaneWidth * (firstIndex - nodeIndex),
            nodes.First().Pos.y,
            nodeIndex
        )
        { Intersection = this };
        nodes.Add(newNode);
        nodes.Sort();
    }

    public Node GetNodeByIndex(int nodeIndex)
    {
        foreach (Node n in nodes)
            if (n.NodeIndex == nodeIndex)
                return n;
        return null;
    }

    public void SetInRoads(HashSet<Road> roads)
    {
        inRoads = roads;
    }

    public void SetOutRoads(HashSet<Road> roads)
    {
        outRoads = roads;
    }

    public bool IsRoadEmpty()
    {
        return inRoads.Count == 0 && outRoads.Count == 0;
    }

    public void AddRoad(Road road, Direction direction)
    {
        Assert.IsTrue(direction == Direction.Out || direction == Direction.In);

        if (direction == Direction.Out)
            outRoads.Add(road);
        else if (direction == Direction.In)
            inRoads.Add(road);

        foreach (Node n in road.GetNodes(direction == Direction.Out ? Side.Start : Side.End))
        {
            n.Intersection = this;
            if (!nodes.Contains(n))
                nodes.Add(n);
        }
        nodes.Sort();
    }

    public void RemoveRoad(Road road, Direction direction)
    {
        Assert.IsTrue(direction == Direction.Out || direction == Direction.In);

        if (direction == Direction.Out)
            outRoads.Remove(road);
        else if (direction == Direction.In)
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

    private HashSet<Road> GetRoads()
    {
        HashSet<Road> h = new(inRoads);
        h.UnionWith(outRoads);
        return h;
    }

    public Node FirstNodeWithRoad(Direction direction)
    {
        foreach (Node n in nodes)
        {
            if (direction == Direction.In)
            {
                if (n.InLane != null)
                    return n;
            }
            else if (direction == Direction.Out)
            {
                if (n.OutLane != null)
                    return n;
            }
        }
        return null;
    }

    public Node LastNodeWithRoad(Direction direction)
    {
        for (int i = nodes.Count - 1; i >= 0; i--)
        {
            if (direction == Direction.In)
            {
                if (nodes[i].InLane != null)
                    return nodes[i];
            }
            else if (direction == Direction.Out)
            {
                if (nodes[i].OutLane != null)
                    return nodes[i];
            }
        }
        return null;
    }

    public bool IsForLaneChangeOnly()
    {
        if (InRoads.Count != 1 || OutRoads.Count != 1)
            return false;
        if (inRoads.Single().LaneCount != outRoads.Single().LaneCount)
            return false;
        foreach (Node n in nodes)
            if (n.InLane == null || n.OutLane == null)
                return false;
        return true;
    }
}