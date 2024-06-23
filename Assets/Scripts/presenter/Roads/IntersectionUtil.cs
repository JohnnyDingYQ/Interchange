using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public static class IntersectionUtil
{
    public static void EvaluateOutline(Intersection i)
    {
        foreach (Road r in i.OutRoads)
        {
            r.LeftOutline.Start.Clear();
            r.RightOutline.Start.Clear();

            List<float3> outline = GetPath(r, Orientation.Left, Direction.Out)?.GetOutline(Orientation.Left);
            if (outline != null)
                r.LeftOutline.Start = SeparateOutlineWithEndofRoad(outline, Direction.Out);
            else
                r.LeftOutline.Start = GetOutLineAtTwoEnds(r, Orientation.Left, Side.Start);

            outline = GetPath(r, Orientation.Right, Direction.Out)?.GetOutline(Orientation.Right);
            if (outline != null)
                r.RightOutline.Start = SeparateOutlineWithEndofRoad(outline, Direction.Out);
            else
                r.RightOutline.Start = GetOutLineAtTwoEnds(r, Orientation.Right, Side.Start);


        }
        foreach (Road r in i.InRoads)
        {
            r.LeftOutline.End.Clear();
            r.RightOutline.End.Clear();

            List<float3> outline = GetPath(r, Orientation.Left, Direction.In)?.GetOutline(Orientation.Left);
            if (outline != null)
                r.LeftOutline.End = SeparateOutlineWithEndofRoad(outline, Direction.In);
            else
                r.LeftOutline.End = GetOutLineAtTwoEnds(r, Orientation.Left, Side.End);

            outline = GetPath(r, Orientation.Right, Direction.In)?.GetOutline(Orientation.Right);
            if (outline != null)
                r.RightOutline.End = SeparateOutlineWithEndofRoad(outline, Direction.In);
            else
                r.RightOutline.End = GetOutLineAtTwoEnds(r, Orientation.Right, Side.End);
        }

        foreach (Road r in i.Roads)
            Game.InvokeRoadUpdated(r);

        #region extracted

        Path GetPath(Road road, Orientation orientation, Direction direction)
        {
            List<Path> edges = null;
            if (direction == Direction.In)
                edges = Graph.GetOutPaths(road.Lanes[orientation == Orientation.Left ? 0 : road.Lanes.Count - 1].EndVertex);
            else if (direction == Direction.Out)
                edges = Graph.GetInPaths(road.Lanes[orientation == Orientation.Left ? 0 : road.Lanes.Count - 1].StartVertex);
            else
                throw new ArgumentException("direction");

            if (edges != null && edges.Count() != 0)
            {
                if (direction == Direction.In)
                    if (orientation == Orientation.Left)
                        return edges.OrderBy(e => Component(-i.Normal, e.Target.Pos)).First();
                    else
                        return edges.OrderBy(e => Component(-i.Normal, e.Target.Pos)).Last();
                if (direction == Direction.Out)
                    if (orientation == Orientation.Left)
                        return edges.OrderBy(e => Component(-i.Normal, e.Source.Pos)).First();
                    else
                        return edges.OrderBy(e => Component(-i.Normal, e.Source.Pos)).Last();
            }
            return null;
        }

        static float Component(float3 u, float3 v)
        {
            return math.dot(u, v) / math.length(v);
        }

        List<float3> SeparateOutlineWithEndofRoad(List<float3> interRoadOutline, Direction direction)
        {
            if (direction != Direction.In && direction != Direction.Out)
                throw new ArgumentException("direction");
            List<float3> outlineEnd = new();
            List<float3> outlineStart = new();
            bool crossed = false;
            float3 prevPt = 0;
            foreach (float3 pt in interRoadOutline)
            {
                if (!crossed)
                    if (i.Plane.SameSide(pt, i.PointOnInSide))
                        outlineEnd.Add(pt);
                    else
                    {
                        crossed = true;
                        Ray ray = new(pt, prevPt - pt);
                        i.Plane.Raycast(ray, out float distance);
                        float3 commonPt = ray.GetPoint(distance);
                        outlineEnd.Add(commonPt);
                        outlineStart.Add(commonPt);
                        outlineStart.Add(pt);
                    }
                else
                    outlineStart.Add(pt);
                prevPt = pt;
            }
            return direction == Direction.In ? outlineEnd : outlineStart;
        }

        static List<float3> GetOutLineAtTwoEnds(Road road, Orientation orientation, Side side)
        {
            List<float3> results = new();
            Lane lane = orientation == Orientation.Left ? road.Lanes.First() : road.Lanes.Last();
            float normalMultiplier = orientation == Orientation.Left ? Constants.RoadOutlineSeparation : -Constants.RoadOutlineSeparation;
            int numPoints = (int)(Constants.VertexDistanceFromRoadEnds * Constants.MeshResolution / 2) + 1;
            BezierSeries bs = lane.BezierSeries;
            float interpolationOfLocation;
            if (side == Side.Start)
                interpolationOfLocation = lane.StartVertex.SeriesInterpolation;
            else
                interpolationOfLocation = lane.EndVertex.SeriesInterpolation;
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

    public static void EvaluatePaths(Intersection i)
    {

        ClearAllPaths();

        if (i.InRoads.Count == 0 || i.OutRoads.Count == 0)
            return;

        foreach (Node n in i.Nodes)
            BuildPathNode2Node(n, n);

        for (int j = 0; j < i.Nodes.Count; j++)
        {
            Node n = i.Nodes[j];
            if (j - 1 >= 0)
            {
                Node other = i.Nodes[j - 1];
                if (NodesBelongToUniqueRoad(n, other))
                {
                    Path l = BuildPathLane2Lane(n.GetLanes(Direction.In).Single(), other.GetLanes(Direction.Out).Single());
                    Path r = BuildPathLane2Lane(other.GetLanes(Direction.In).Single(), n.GetLanes(Direction.Out).Single());
                    l.InterweavingPath = r;
                    r.InterweavingPath = l;
                }
            }
        }

        foreach (Node n in i.Nodes)
        {
            if (n.GetLanes(Direction.Out).Count == 0)
            {
                if (n.NodeIndex < i.LastNodeWithRoad(Direction.Out).NodeIndex && n.NodeIndex > i.FirstNodeWithRoad(Direction.Out).NodeIndex)
                    continue;
                int indexFirst = n.NodeIndex - i.FirstNodeWithRoad(Direction.Out).NodeIndex;
                int indexLast = n.NodeIndex - i.LastNodeWithRoad(Direction.Out).NodeIndex;
                if (Math.Abs(indexFirst) < Math.Abs(indexLast))
                    BuildPathNode2Node(n, i.FirstNodeWithRoad(Direction.Out));
                else
                    BuildPathNode2Node(n, i.LastNodeWithRoad(Direction.Out));
            }

            if (n.GetLanes(Direction.In).Count == 0)
            {
                if (n.NodeIndex < i.LastNodeWithRoad(Direction.In).NodeIndex && n.NodeIndex > i.FirstNodeWithRoad(Direction.In).NodeIndex)
                    continue;
                int indexFirst = n.NodeIndex - i.FirstNodeWithRoad(Direction.In).NodeIndex;
                int indexLast = n.NodeIndex - i.LastNodeWithRoad(Direction.In).NodeIndex;
                if (Math.Abs(indexFirst) < Math.Abs(indexLast))
                    BuildPathNode2Node(i.FirstNodeWithRoad(Direction.In), n);
                else
                    BuildPathNode2Node(i.LastNodeWithRoad(Direction.In), n);
            }
        }

        void ClearAllPaths()
        {
            List<Path> toRemove = new();
            foreach (Road r in i.InRoads)
                foreach (Lane l in r.Lanes)
                    toRemove.AddRange(Graph.GetOutPaths(l.EndVertex));

            foreach (Path p in toRemove)
                Game.RemovePath(p);
        }

        // i.e. nodes are internal to a a road
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
                foreach (Lane outLane in n2.GetLanes(Direction.Out))
                    BuildPathLane2Lane(inLane, outLane);

        }

        static Path BuildPathLane2Lane(Lane l1, Lane l2)
        {
            return BuildPath(l1.EndVertex, l2.StartVertex);
        }

        static Path BuildPath(Vertex start, Vertex end)
        {
            Path path = Graph.GetPath(start, end);
            if (path != null)
                return null;
            float3 pos1 = start.Pos + Constants.MinimumLaneLength / 3 * start.Tangent;
            float3 pos2 = end.Pos - Constants.MinimumLaneLength / 3 * end.Tangent;
            BezierSeries bs = new(new BezierCurve(start.Pos, pos1, pos2, end.Pos));
            Path p = new(bs, start, end);
            Game.RegisterPath(p);
            return p;
        }
    }
}