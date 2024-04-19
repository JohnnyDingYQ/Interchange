using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine.Splines;
using UnityEngine;


public static class InterRoad
{
    public static void BuildAllPaths(List<Lane> to, List<Node> from, Direction direction)
    {
        int laneCount = to.Count;
        ReadOnlySet<Road> roadsOnOtherside = GetRelevantRoads();

        BuildStraightPath();
        BuildRightLaneChangePath();
        BuildLeftLaneChangePath();

        if (BranchExists())
            DeleteAllLaneChangingPaths();
        else
            BuildSidePaths();

        PatchSidePaths();

        #region extracted 
        void PatchSidePaths()
        {
            List<Node> nodes = new();
            foreach (Road road in roadsOnOtherside)
            {
                nodes = direction == Direction.Out ? road.GetNodes(Side.End) : road.GetNodes(Side.Start);
                for (int i = 0; i < 2; i++)
                {

                    if (!nodes.First().HasLanes(direction))
                    {
                        Node connected = nodes.First();
                        foreach (Node node in nodes)
                            if (node.HasLanes(direction))
                            {
                                connected = node;
                                break;
                            }
                        foreach (Lane l1 in nodes.First().GetLanes(InvertDirection(direction)))
                            foreach (Lane l2 in connected.GetLanes(direction))
                                BuildLanePath(l1, l2, nodes.First().Order - connected.Order);
                    }
                    nodes.Reverse();
                }
            }
        }

        void BuildStraightPath()
        {
            for (int i = 0; i < laneCount; i++)
                foreach (Lane lane in from[i].GetLanes(InvertDirection(direction)))
                    BuildLanePath(lane, to[i], 0);
        }
        void BuildRightLaneChangePath()
        {
            for (int i = 1; i < laneCount; i++)
                foreach (Lane lane in from[i - 1].GetLanes(InvertDirection(direction)))
                    BuildLanePath(lane, to[i], direction == Direction.Out ? 1 : -1);
        }
        void BuildLeftLaneChangePath()
        {
            for (int i = 0; i < laneCount - 1; i++)
                foreach (Lane lane in from[i + 1].GetLanes(InvertDirection(direction)))
                    BuildLanePath(lane, to[i], direction == Direction.Out ? -1 : 1);
        }
        void BuildSidePaths()
        {
            foreach (Road road in roadsOnOtherside)
                foreach (Lane lane in road.Lanes)
                {
                    Node node = direction == Direction.Out ? lane.EndNode : lane.StartNode;
                    if (from.Contains(node))
                        continue;

                    int span = from.First().Order - node.Order;
                    if (span == 1)
                        BuildLanePath(lane, to.First(), direction == Direction.Out ? span : -span);

                    span = from.Last().Order - node.Order;
                    if (span == -1)
                        BuildLanePath(lane, to.Last(), direction == Direction.Out ? span : -span);
                }
        }

        static Direction InvertDirection(Direction direction)
        {
            if (direction == Direction.In)
                return Direction.Out;
            return Direction.In;
        }

        Path BuildLanePath(Lane l1, Lane l2, int span)
        {
            Path path;
            if (direction == Direction.Out)
                path = BuildPath(l1.EndVertex, l2.StartVertex, span);
            else
                path = BuildPath(l2.EndVertex, l1.StartVertex, span);
            return path;
        }

        Path BuildPath(Vertex start, Vertex end, int span)
        {
            Game.Graph.TryGetEdge(start, end, out Path edge);
            if (edge != null)
                return null;
            float3 pos1 = start.Pos + Constants.MinimumLaneLength / 3 * start.Tangent;
            float3 pos2 = end.Pos - Constants.MinimumLaneLength / 3 * end.Tangent;
            BezierCurve bezierCurve = new(start.Pos, pos1, pos2, end.Pos);
            ICurve curve = new BezierCurveAdapter(bezierCurve);
            Path p = new(curve, start, end, span);
            Game.AddEdge(p);
            return p;
        }

        void DeleteAllLaneChangingPaths()
        {
            foreach (Road road in roadsOnOtherside)
                foreach (Lane lane in road.Lanes)
                    if (direction == Direction.Out)
                        Game.Graph.RemoveEdgeIf(e => e.Source == lane.EndVertex && Math.Abs(e.Span) > 0);
                    else
                        Game.Graph.RemoveEdgeIf(e => e.Target == lane.StartVertex && Math.Abs(e.Span) > 0);
        }

        bool BranchExists()
        {
            HashSet<Road> seenRoads = new();
            foreach (Road road in roadsOnOtherside)
                foreach (Lane lane in road.Lanes)
                {
                    Node node = direction == Direction.Out ? lane.EndNode : lane.StartNode;
                    seenRoads.UnionWith(node.GetRoads(direction));
                }
            return seenRoads.Count > 1;
        }

        ReadOnlySet<Road> GetRelevantRoads()
        {
            NodeGroup nodeGroup = new(from.First());
            if (direction == Direction.Out)
                return nodeGroup.InRoads;
            return nodeGroup.OutRoads;
        }
        #endregion
    }

    public static void UpdateOutline(NodeGroup nodeGroup)
    {
        ReadOnlySet<Road> outRoads = nodeGroup.OutRoads;
        ReadOnlySet<Road> inRoads = nodeGroup.InRoads;

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

        EvaluateSideOutline(Orientation.Left);
        EvaluateSideOutline(Orientation.Right);

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
        
        #region extracted
        void EvaluateSideOutline(Orientation orientation)
        {
            IEnumerable<Path> edges;
            List<float3> pathOutline;
            Node firstNodeWithInRoad = nodeGroup.FirstWithInRoad();
            Node lastNodeWithInRoad = nodeGroup.LastWithInRoad();
            Road leftmostRoad = firstNodeWithInRoad.GetRoads(Direction.In).First();
            Road rightmostRoad = lastNodeWithInRoad.GetRoads(Direction.In).First();

            if (orientation == Orientation.Left)
                Game.Graph.TryGetOutEdges(leftmostRoad.Lanes.First().EndVertex, out edges);
            else
                Game.Graph.TryGetOutEdges(rightmostRoad.Lanes.Last().EndVertex, out edges);

            List<Path> paths = new(edges);
            paths.Sort();

            int numPoints = (int)(Constants.MinimumLaneLength * Constants.MeshResolution);
            if (orientation == Orientation.Left)
                pathOutline = paths.First().GetOutline(numPoints, orientation);
            else
                pathOutline = paths.Last().GetOutline(numPoints, orientation);

            SeparateOutlineWithEndofRoad(pathOutline, out List<float3> outlineStart, out List<float3> outlineEnd);

            if (orientation == Orientation.Left)
            {
                leftmostRoad.LeftOutline.End = outlineEnd;
                paths.First().Target.Road.LeftOutline.Start = outlineStart;
            }
            else
            {
                rightmostRoad.RightOutline.End = outlineEnd;
                paths.Last().Target.Road.RightOutline.Start = outlineStart;
            }
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
                    if (nodeGroup.Plane.SameSide(pt, nodeGroup.PointOnInside))
                        outlineEnd.Add(pt);
                    else
                    {
                        crossed = true;
                        Ray ray = new(pt, prevPt - pt);
                        nodeGroup.Plane.Raycast(ray, out float distance);
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
        #endregion
    }

    static float3 Get2DNormal(Spline spline, float t)
    {
        float3 forward = spline.EvaluateTangent(t);
        float3 upVector = spline.EvaluateUpVector(t);
        float3 normal = Vector3.Cross(forward, upVector).normalized;
        normal.y = 0;
        return normal;
    }

    public static List<float3> GetOutLineAtTwoEnds(Road road, Orientation orientation, Side side)
    {
        List<float3> results = new();
        Lane lane = orientation == Orientation.Left ? road.Lanes.First() : road.Lanes.Last();
        float normalMultiplier = orientation == Orientation.Left ? Constants.RoadOutlineSeparation : -Constants.RoadOutlineSeparation;
        int numPoints = (int)(Constants.MinimumLaneLength * Constants.MeshResolution / 2);
        for (int i = 0; i <= numPoints; i++)
        {
            float t;
            if (side == Side.Start)
                t = (float)i / numPoints * lane.StartVertex.Interpolation;
            else
                t = lane.EndVertex.Interpolation + (float)i / numPoints * (1 - lane.EndVertex.Interpolation);
            float3 normal = Get2DNormal(lane.Spline, t);
            results.Add(lane.Spline.EvaluatePosition(t) + normal * normalMultiplier);
        }
        return results;
    }
}