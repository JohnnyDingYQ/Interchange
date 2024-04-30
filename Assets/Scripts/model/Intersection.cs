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
    public float3 Tangent { get; private set; }
    [JsonIgnore]
    public float3 PointOnInSide { get; private set; }

    [JsonConstructor]
    public Intersection(int Id)
    {
    }
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

    public bool IsEmpty()
    {
        return nodes.Count == 0 && inRoads.Count == 0 && outRoads.Count == 0;
    }

    public void RemoveNode(Node node)
    {
        nodes.Remove(node);
    }

    public void SetNodeReferenece()
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
            Tangent = math.normalize(bs.EvaluateTangent(bs.EndLocation));
            Plane = new(randomInRoad.EndPos, randomInRoad.EndPos + Normal, randomInRoad.EndPos - new float3(0, 1, 0));
            PointOnInSide = bs.EvaluatePosition(bs.EndLocation) - math.normalize(bs.EvaluateTangent(bs.EndLocation));
        }
        else if (outRoads.Count != 0)
        {
            Road randomOutRoad = OutRoads.First();
            BezierSeries bs = randomOutRoad.BezierSeries;
            Normal = math.normalize(bs.Evaluate2DNormalizedNormal(bs.StartLocation));
            Tangent = math.normalize(bs.EvaluateTangent(bs.StartLocation));
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
            Build.BuildAllPaths(r.Lanes, r.GetNodes(Side.Start), Direction.Out);
        UpdateOutline();
    }

    public override string ToString()
    {
        return "Intersection " + Id;
    }

    public void UpdateOutline()
    {
        foreach (Road r in outRoads)
        {
            r.LeftOutline.Start.Clear();
            r.RightOutline.Start.Clear();
        }
        foreach (Road r in inRoads)
        {
            r.LeftOutline.End.Clear();
            r.RightOutline.End.Clear();
        }
        if (inRoads.Count != 0 && outRoads.Count != 0)
            EvaluateSideOutline();

        foreach (Road r in outRoads)
        {
            if (r.LeftOutline.Start.Count == 0)
                r.LeftOutline.Start = GetOutLineAtTwoEnds(r, Orientation.Left, Side.Start);
            if (r.RightOutline.Start.Count == 0)
                r.RightOutline.Start = GetOutLineAtTwoEnds(r, Orientation.Right, Side.Start);
        }
        foreach (Road r in inRoads)
        {
            if (r.LeftOutline.End.Count == 0)
                r.LeftOutline.End = GetOutLineAtTwoEnds(r, Orientation.Left, Side.End);
            if (r.RightOutline.End.Count == 0)
                r.RightOutline.End = GetOutLineAtTwoEnds(r, Orientation.Right, Side.End);
        }

        foreach (Road r in GetRoads())
            Game.InvokeUpdateRoadMesh(r);

        #region extracted
        void EvaluateSideOutline()
        {
            Node firstNodeWithInRoad = FirstWithRoad(Direction.In);
            Node lastNodeWithInRoad = LastWithRoad(Direction.In);
            Road leftmostRoad = firstNodeWithInRoad.GetRoads(Direction.In).First();
            Road rightmostRoad = lastNodeWithInRoad.GetRoads(Direction.In).First();

            Game.Graph.TryGetOutEdges(leftmostRoad.Lanes.First().EndVertex, out IEnumerable<Path> leftEdges);
            Game.Graph.TryGetOutEdges(rightmostRoad.Lanes.Last().EndVertex, out IEnumerable<Path> rightEdges);

            List<Path> leftPaths = new(leftEdges);
            List<Path> rightPaths = new(rightEdges);
            leftPaths.Sort();
            rightPaths.Sort();

            List<float3> leftPathOutline = leftPaths.First().GetOutline(Orientation.Left);
            List<float3> rightPathOutline = rightPaths.Last().GetOutline(Orientation.Right);

            SeparateOutlineWithEndofRoad(leftPathOutline, out List<float3> leftStart, out List<float3> leftEnd);
            SeparateOutlineWithEndofRoad(rightPathOutline, out List<float3> rightStart, out List<float3> rightEnd);
            leftmostRoad.LeftOutline.End = leftEnd;
            leftPaths.First().Target.Road.LeftOutline.Start = leftStart;

            rightmostRoad.RightOutline.End = rightEnd;
            rightPaths.Last().Target.Road.RightOutline.Start = rightStart;

        }

        void SeparateOutlineWithEndofRoad(List<float3> interRoadOutline, out List<float3> outlineStart, out List<float3> outlineEnd)
        {
            outlineEnd = new();
            outlineStart = new();
            bool crossed = false;
            float3 prevPt = 0;
            foreach (float3 pt in interRoadOutline)
            {
                if (!crossed)
                    if (Plane.SameSide(pt, PointOnInSide))
                        outlineEnd.Add(pt);
                    else
                    {
                        crossed = true;
                        Ray ray = new(pt, prevPt - pt);
                        Plane.Raycast(ray, out float distance);
                        float3 commonPt = ray.GetPoint(distance);
                        outlineEnd.Add(commonPt);
                        outlineStart.Add(commonPt);
                        outlineStart.Add(pt);
                    }
                else
                    outlineStart.Add(pt);
                prevPt = pt;
            }
        }

        static List<float3> GetOutLineAtTwoEnds(Road road, Orientation orientation, Side side)
        {
            List<float3> results = new();
            Lane lane = orientation == Orientation.Left ? road.Lanes.First() : road.Lanes.Last();
            float normalMultiplier = orientation == Orientation.Left ? Constants.RoadOutlineSeparation : -Constants.RoadOutlineSeparation;
            int numPoints = (int)(Constants.MinimumLaneLength * Constants.MeshResolution / 2);
            BezierSeries bs = lane.BezierSeries;
            float interpolationOfLocation;
            if (side == Side.Start)
                interpolationOfLocation = bs.LocationToInterpolation(lane.StartVertex.SeriesLocation);
            else
                interpolationOfLocation = bs.LocationToInterpolation(lane.EndVertex.SeriesLocation);
            for (int i = 0; i <= numPoints; i++)
            {
                float t;
                if (side == Side.Start)
                    t = (float)i / numPoints * interpolationOfLocation;
                else
                    t = interpolationOfLocation + (float)i / numPoints * (1 - interpolationOfLocation);
                float3 normal = bs.Evaluate2DNormalizedNormal(t);
                results.Add(bs.EvaluatePosition(t) + normal * normalMultiplier);
            }
            return results;
        }
        #endregion
    }
}