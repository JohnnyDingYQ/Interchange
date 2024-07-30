using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;
using System;
using UnityEngine.Assertions;
using System.Collections.ObjectModel;

public class Intersection : IPersistable
{
    public uint Id { get; set; }
    [SaveIDCollection]
    private HashSet<Road> inRoads = new();
    [SaveIDCollection]
    private HashSet<Road> outRoads = new();
    [IPersistableImplemented]
    private readonly PersistableSortedList nodes = new();
    public float3 Normal { get; private set; }
    public float3 Tangent { get; private set; }
    public float3 PointOnInSide { get; private set; }
    [NotSaved]
    public ReadOnlyCollection<Node> Nodes { get => nodes.Values.ToList().AsReadOnly(); }
    [NotSaved]
    public ReadOnlySet<Road> InRoads { get => inRoads.AsReadOnly(); }
    [NotSaved]
    public ReadOnlySet<Road> OutRoads { get => outRoads.AsReadOnly(); }
    [NotSaved]
    public HashSet<Road> Roads { get => GetRoads(); }
    [NotSaved]
    public Plane Plane { get => GetPlane(); }

    public Intersection() { }

    public Intersection(Road road, Direction direction)
    {
        AddRoad(road, direction);
        Normal = GetAttribute(AttributeTypes.Normal);
        Tangent = GetAttribute(AttributeTypes.Tangent);
        PointOnInSide = GetAttribute(AttributeTypes.PointOnInSide);
    }

    public void SetNodes(List<Node> nodes)
    {
        foreach (Node node in nodes)
            this.nodes[node.NodeIndex] = node;
        foreach (Node node in nodes)
            node.Intersection = this;
    }

    public Node AddNode(int nodeIndex)
    {
        Assert.AreNotEqual(0, nodes.Count);
        if (nodes.ContainsKey(nodeIndex))
            return null;
        Node firstNode = nodes.Values.First();
        int firstIndex = firstNode.NodeIndex;
        Node newNode = new(
            firstNode.Pos + (firstIndex - nodeIndex) * Constants.LaneWidth * Normal,
            firstNode.Pos.y,
            nodeIndex
        )
        { Intersection = this };
        nodes[newNode.NodeIndex] = newNode;
        return newNode;
    }

    // invokes AddNode if walked node index does not exist
    public List<Node> WalkNodes(int startNodeIndex, int count)
    {
        List<Node> results = new();
        for (int i = startNodeIndex; i < startNodeIndex + count; i++)
        {
            Node node = GetNodeByIndex(i);
            node ??= AddNode(i);
            results.Add(node);
        }
        return results;
    }

    public Node GetNodeByIndex(int nodeIndex)
    {
        if (nodes.ContainsKey(nodeIndex))
            return nodes[nodeIndex];
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
            nodes[n.NodeIndex] = n;
        }
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
        nodes.Remove(node.NodeIndex);
    }

    float3 GetAttribute(AttributeTypes attributeType)
    {
        if (inRoads.Count != 0)
        {
            Road randomInRoad = inRoads.First();
            Curve bs = randomInRoad.Curve;
            if (attributeType == AttributeTypes.Normal)
                return bs.EndNormal;
            if (attributeType == AttributeTypes.Tangent)
                return math.normalize(bs.EndTangent);
            if (attributeType == AttributeTypes.PointOnInSide)
                return bs.EndPos - math.normalize(bs.EndTangent);
        }
        if (outRoads.Count != 0)
        {
            Road randomOutRoad = outRoads.First();
            Curve bs = randomOutRoad.Curve;
            if (attributeType == AttributeTypes.Normal)
                return bs.StartNormal;
            if (attributeType == AttributeTypes.Tangent)
                return math.normalize(bs.StartTangent);
            if (attributeType == AttributeTypes.PointOnInSide)
                return bs.StartPos - math.normalize(bs.StartTangent);
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
        for (int i = 0; i < nodes.Count; i++)
        {
            if (direction == Direction.In)
            {
                if (nodes.Values[i].InLane != null)
                    return nodes.Values[i];
            }
            else if (direction == Direction.Out)
            {
                if (nodes.Values[i].OutLane != null)
                    return nodes.Values[i];
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
                if (nodes.Values[i].InLane != null)
                    return nodes.Values[i];
            }
            else if (direction == Direction.Out)
            {
                if (nodes.Values[i].OutLane != null)
                    return nodes.Values[i];
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
        foreach (Node n in nodes.Values)
            if (n.InLane == null || n.OutLane == null)
                return false;
        return true;
    }
    public override string ToString()
    {
        return "Intersection " + Id;
    }

    public override bool Equals(object obj)
    {
        if (obj is Intersection other)
            return Id == other.Id && nodes.Keys.SequenceEqual(other.nodes.Keys)
                && nodes.Values.Select(n => n.Id).SequenceEqual(other.nodes.Values.Select(n => n.Id))
                && inRoads.Select(r => r.Id).ToHashSet().SetEquals(other.inRoads.Select(r => r.Id))
                && outRoads.Select(r => r.Id).ToHashSet().SetEquals(other.outRoads.Select(r => r.Id));
        else
            return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}