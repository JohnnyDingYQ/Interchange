using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;
using Unity.Plastic.Newtonsoft.Json;
using System;
using UnityEngine.Splines;

public class Intersection
{
    public int Id { get; set; }
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
    [JsonIgnore]
    public Plane Plane { get { return GetPlane(); } }
    [JsonIgnore]
    public float3 Normal { get { return GetAttribute(AttributeTypes.Normal); } }
    [JsonIgnore]
    public float3 Tangent { get { return GetAttribute(AttributeTypes.Tangent); } }
    [JsonIgnore]
    public float3 PointOnInSide { get { return GetAttribute(AttributeTypes.PointOnInSide); } }

    public Intersection() { }

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
                return bs.Evaluate2DNormalizedNormal(bs.EndLocation);
            if (attributeType == AttributeTypes.Tangent)
                return math.normalize(bs.EvaluateTangent(bs.EndLocation));
            if (attributeType == AttributeTypes.PointOnInSide)
                return bs.EvaluatePosition(bs.EndLocation) - math.normalize(bs.EvaluateTangent(bs.EndLocation));
        }
        if (outRoads.Count != 0)
        {
            Road randomOutRoad = outRoads.First();
            BezierSeries bs = randomOutRoad.BezierSeries;
            if (attributeType == AttributeTypes.Normal)
                return bs.Evaluate2DNormalizedNormal(bs.StartLocation);
            if (attributeType == AttributeTypes.Tangent)
                return math.normalize(bs.EvaluateTangent(bs.StartLocation));
            if (attributeType == AttributeTypes.PointOnInSide)
                return bs.EvaluatePosition(bs.StartLocation) - math.normalize(bs.EvaluateTangent(bs.StartLocation));
        }
        throw new InvalidOperationException("nope");
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
        throw new InvalidOperationException("nope");
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

    public void EvaluateOutline()
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
            Node firstNodeWithInRoad = FirstNodeWithRoad(Direction.In);
            Node lastNodeWithInRoad = LastNodeWithRoad(Direction.In);
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

    public void EvaluatePaths()
    {

        ClearAllPaths();

        if (inRoads.Count == 0 || outRoads.Count == 0)
            return;

        foreach (Node n in nodes)
            BuildPathNode2Node(n, n, 0);

        for (int i = 0; i < nodes.Count; i++)
        {
            Node n = nodes[i];
            if (i - 1 >= 0)
            {
                Node other = nodes[i - 1];
                if (NodesBelongToUniqueRoad(n, other))
                    BuildPathNode2Node(n, other, -1);
            }
            if (i + 1 < nodes.Count)
            {
                Node other = nodes[i + 1];
                if (NodesBelongToUniqueRoad(n, other))
                    BuildPathNode2Node(n, other, 1);
            }
        }

        foreach (Node n in nodes)
        {
            if (n.GetLanes(Direction.Out).Count == 0)
            {
                if (n.NodeIndex < LastNodeWithRoad(Direction.Out).NodeIndex && n.NodeIndex > FirstNodeWithRoad(Direction.Out).NodeIndex)
                    continue;
                int indexFirst = n.NodeIndex - FirstNodeWithRoad(Direction.Out).NodeIndex;
                int indexLast = n.NodeIndex - LastNodeWithRoad(Direction.Out).NodeIndex;
                if (Math.Abs(indexFirst) < Math.Abs(indexLast))
                    BuildPathNode2Node(n, FirstNodeWithRoad(Direction.Out), indexFirst);
                else
                    BuildPathNode2Node(n, LastNodeWithRoad(Direction.Out), indexLast);
            }

            if (n.GetLanes(Direction.In).Count == 0)
            {
                if (n.NodeIndex < LastNodeWithRoad(Direction.In).NodeIndex && n.NodeIndex > FirstNodeWithRoad(Direction.In).NodeIndex)
                    continue;
                int indexFirst = n.NodeIndex - FirstNodeWithRoad(Direction.In).NodeIndex;
                int indexLast = n.NodeIndex - LastNodeWithRoad(Direction.In).NodeIndex;
                if (Math.Abs(indexFirst) < Math.Abs(indexLast))
                    BuildPathNode2Node(FirstNodeWithRoad(Direction.In), n, indexFirst);
                else
                    BuildPathNode2Node(LastNodeWithRoad(Direction.In), n, indexLast);
            }
        }

        void ClearAllPaths()
        {
            foreach (Road r in inRoads)
                foreach (Lane l in r.Lanes)
                    Game.Graph.RemoveOutEdgeIf(l.EndVertex, (e) => true);
        }

        bool NodesBelongToUniqueRoad(Node n1, Node n2)
        {
            if (n1.GetLanes(Direction.In).Count == 1 && n2.GetLanes(Direction.Out).Count == 1
                    && n1.GetLanes(Direction.Out).Count == 1 && n2.GetLanes(Direction.In).Count == 1)
            {
                HashSet<Road> ins = n1.GetRoads(Direction.In);
                ins.UnionWith(n2.GetRoads(Direction.In));
                HashSet<Road> outs = n1.GetRoads(Direction.Out);
                outs.UnionWith(n2.GetRoads(Direction.Out));
                if (ins.Count == 1 && outs.Count == 1)
                    return true;

            }
            return false;
        }

        static void BuildPathNode2Node(Node n1, Node n2, int span)
        {
            foreach (Lane inLane in n1.GetLanes(Direction.In))
            {
                foreach (Lane outLane in n2.GetLanes(Direction.Out))
                    BuildPathLane2Lane(inLane, outLane, span);
            }
        }

        static void BuildPathLane2Lane(Lane l1, Lane l2, int span)
        {
            BuildPath(l1.EndVertex, l2.StartVertex, span);
        }

        static void BuildPath(Vertex start, Vertex end, int span)
        {
            Game.Graph.TryGetEdge(start, end, out Path edge);
            if (edge != null)
                return;
            float3 pos1 = start.Pos + Constants.MinimumLaneLength / 3 * start.Tangent;
            float3 pos2 = end.Pos - Constants.MinimumLaneLength / 3 * end.Tangent;
            BezierSeries bs = new(new BezierCurve(start.Pos, pos1, pos2, end.Pos));
            Path p = new(bs, start, end, span);
            Game.AddEdge(p);
        }

    }
}