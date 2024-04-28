using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;
using Unity.Plastic.Newtonsoft.Json;
using System;

public class Intersection
{
    public int Id { get; set; }
    [JsonProperty]
    private readonly List<Node> nodes;
    [JsonIgnore]
    public List<Node> Nodes { get { return new List<Node>(nodes); } }
    [JsonIgnore]
    public int Count { get { return Nodes.Count; } }
    [JsonProperty]
    private readonly HashSet<Road> inRoads;
    [JsonIgnore]
    public ReadOnlySet<Road> InRoads { get { return inRoads.AsReadOnly(); } }
    [JsonProperty]
    private readonly HashSet<Road> outRoads;
    [JsonIgnore]
    public ReadOnlySet<Road> OutRoads { get { return outRoads.AsReadOnly(); } }
    [JsonIgnore]
    public HashSet<Road> Roads { get { return GetRoads(); } }
    [JsonIgnore]
    public Plane Plane { get; private set; }
    [JsonIgnore]
    public float3 Normal { get; private set; }
    [JsonIgnore]
    public float3 PointOnInSide { get; private set; }

    public Intersection()
    {
        nodes = new();
        inRoads = new();
        outRoads = new();
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
            if (!nodes.Contains(n))
                nodes.Add(n);
        }
        nodes.Sort();
        UpdateNormalAndPlane();
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

    public void RemoveNode(Node node)
    {
        nodes.Remove(node);
    }

    public void SetNodeReferece()
    {
        if (Id == 0)
            throw new InvalidOperationException("intersection id is 0");
        foreach (Node n in nodes)
            n.Intersection = this;
    }

    public void UpdateNormalAndPlane()
    {
        if (inRoads.Count != 0)
        {
            Road randomInRoad = InRoads.First();
            BezierSeries bs = randomInRoad.BezierSeries;
            Normal = math.normalize(bs.Evaluate2DNormalizedNormal(bs.EndLocation));
            Plane = new(randomInRoad.EndPos, randomInRoad.EndPos + Normal, randomInRoad.EndPos - new float3(0, 1, 0));
            PointOnInSide = bs.EvaluatePosition(bs.EndLocation) - math.normalize(bs.EvaluateTangent(bs.EndLocation));
        }
        else if (outRoads.Count != 0)
        {
            Road randomOutRoad = OutRoads.First();
            BezierSeries bs = randomOutRoad.BezierSeries;
            Normal = math.normalize(bs.Evaluate2DNormalizedNormal(bs.StartLocation));
            Plane = new(randomOutRoad.StartPos, randomOutRoad.StartPos + Normal, randomOutRoad.EndPos - new float3(0, 1, 0));
            PointOnInSide = bs.EvaluatePosition(bs.StartLocation) - math.normalize(bs.EvaluateTangent(bs.StartLocation));
        }
    }

    public HashSet<Road> GetRoads()
    {
        HashSet<Road> h = new(inRoads);
        h.UnionWith(outRoads);
        return h;
    }

    public Node FirstWithRoad(Direction direction)
    {
        foreach (Node n in nodes)
            if (n.GetLanes(direction).Count != 0)
                return n;
        return null;
    }

    public Node LastWithRoad(Direction direction)
    {
        for (int i = nodes.Count - 1; i >= 0; i--)
            if (nodes[i].GetLanes(direction).Count != 0)
                return nodes[i];
        return null;
    }

    public void ReevaluatePaths()
    {
        foreach (Road r in inRoads)
            foreach (Lane l in r.Lanes)
                Game.Graph.RemoveOutEdgeIf(l.EndVertex, (e) => true);
        foreach (Road r in outRoads)
            InterRoad.BuildAllPaths(r.Lanes, r.GetNodes(Side.Start), Direction.Out);
    }
}