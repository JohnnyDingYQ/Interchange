using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;
using Unity.Plastic.Newtonsoft.Json;
using System;
using UnityEngine.Splines;
using GraphExtensions;

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

            List<float3> outline = GetPath(r, Orientation.Left, Direction.Out)?.GetOutline(Orientation.Left);
            if (outline != null)
            {
                SeparateOutlineWithEndofRoad(outline, out List<float3> outlineEnd, out List<float3> outlineStart);
                r.LeftOutline.Start = outlineStart;
            }
            else
                r.LeftOutline.Start = GetOutLineAtTwoEnds(r, Orientation.Left, Side.Start);

            outline = GetPath(r, Orientation.Right, Direction.Out)?.GetOutline(Orientation.Right);
            if (outline != null)
            {
                SeparateOutlineWithEndofRoad(outline, out List<float3> outlineEnd, out List<float3> outlineStart);
                r.RightOutline.Start = outlineStart;
            }
            else
                r.RightOutline.Start = GetOutLineAtTwoEnds(r, Orientation.Right, Side.Start);


        }
        foreach (Road r in inRoads)
        {
            r.LeftOutline.End.Clear();
            r.RightOutline.End.Clear();

            List<float3> outline = GetPath(r, Orientation.Left, Direction.In)?.GetOutline(Orientation.Left);
            if (outline != null)
            {
                SeparateOutlineWithEndofRoad(outline, out List<float3> outlineEnd, out List<float3> outlineStart);
                r.LeftOutline.End = outlineEnd;
            }
            else
                r.LeftOutline.End = GetOutLineAtTwoEnds(r, Orientation.Left, Side.End);

            outline = GetPath(r, Orientation.Right, Direction.In)?.GetOutline(Orientation.Right);
            if (outline != null)
            {
                SeparateOutlineWithEndofRoad(outline, out List<float3> outlineEnd, out List<float3> outlineStart);
                r.RightOutline.End = outlineEnd;
            }
            else
                r.RightOutline.End = GetOutLineAtTwoEnds(r, Orientation.Right, Side.End);
        }

        foreach (Road r in GetRoads())
            Game.InvokeUpdateRoadMesh(r);

        #region extracted

        Path GetPath(Road road, Orientation orientation, Direction direction)
        {
            IEnumerable<Path> edges = null;
            if (direction == Direction.In)
                Game.Graph.TryGetOutEdges(road.Lanes[orientation == Orientation.Left ? 0 : road.Lanes.Count - 1].EndVertex, out edges);
            else if (direction == Direction.Out)
                edges = Game.Graph.GetInEdges(road.Lanes[orientation == Orientation.Left ? 0 : road.Lanes.Count - 1].StartVertex);
            else
                throw new ArgumentException("direction");

            if (edges != null && edges.Count() != 0)
            {
                if (direction == Direction.In)
                    if (orientation == Orientation.Left)
                        return edges.OrderBy(e => Component(-Normal, e.Target.Pos)).First();
                    else
                        return edges.OrderBy(e => Component(-Normal, e.Target.Pos)).Last();
                if (direction == Direction.Out)
                    if (orientation == Orientation.Left)
                        return edges.OrderBy(e => Component(-Normal, e.Source.Pos)).First();
                    else
                        return edges.OrderBy(e => Component(-Normal, e.Source.Pos)).Last();
            }
            return null;
        }

        static float Component(float3 u, float3 v)
        {
            return math.dot(u, v) / math.length(v);
        }

        void SeparateOutlineWithEndofRoad(List<float3> interRoadOutline, out List<float3> outlineEnd, out List<float3> outlineStart)
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
            BuildPathNode2Node(n, n);

        for (int i = 0; i < nodes.Count; i++)
        {
            Node n = nodes[i];
            if (i - 1 >= 0)
            {
                Node other = nodes[i - 1];
                if (NodesBelongToUniqueRoad(n, other))
                    BuildPathNode2Node(n, other);
            }
            if (i + 1 < nodes.Count)
            {
                Node other = nodes[i + 1];
                if (NodesBelongToUniqueRoad(n, other))
                    BuildPathNode2Node(n, other);
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
                    BuildPathNode2Node(n, FirstNodeWithRoad(Direction.Out));
                else
                    BuildPathNode2Node(n, LastNodeWithRoad(Direction.Out));
            }

            if (n.GetLanes(Direction.In).Count == 0)
            {
                if (n.NodeIndex < LastNodeWithRoad(Direction.In).NodeIndex && n.NodeIndex > FirstNodeWithRoad(Direction.In).NodeIndex)
                    continue;
                int indexFirst = n.NodeIndex - FirstNodeWithRoad(Direction.In).NodeIndex;
                int indexLast = n.NodeIndex - LastNodeWithRoad(Direction.In).NodeIndex;
                if (Math.Abs(indexFirst) < Math.Abs(indexLast))
                    BuildPathNode2Node(FirstNodeWithRoad(Direction.In), n);
                else
                    BuildPathNode2Node(LastNodeWithRoad(Direction.In), n);
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

        static void BuildPathNode2Node(Node n1, Node n2)
        {
            foreach (Lane inLane in n1.GetLanes(Direction.In))
            {
                foreach (Lane outLane in n2.GetLanes(Direction.Out))
                    BuildPathLane2Lane(inLane, outLane);
            }
        }

        static void BuildPathLane2Lane(Lane l1, Lane l2)
        {
            BuildPath(l1.EndVertex, l2.StartVertex);
        }

        static void BuildPath(Vertex start, Vertex end)
        {
            Game.Graph.TryGetEdge(start, end, out Path edge);
            if (edge != null)
                return;
            float3 pos1 = start.Pos + Constants.MinimumLaneLength / 3 * start.Tangent;
            float3 pos2 = end.Pos - Constants.MinimumLaneLength / 3 * end.Tangent;
            BezierSeries bs = new(new BezierCurve(start.Pos, pos1, pos2, end.Pos));
            Path p = new(bs, start, end);
            Game.AddEdge(p);
        }

    }
}