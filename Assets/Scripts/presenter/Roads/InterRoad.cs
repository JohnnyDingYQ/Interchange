using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine.Splines;
using UnityEngine;
using GraphExtensions;
using QuikGraph;
using ListExtensions;


public static class InterRoad
{
    public static void BuildAllPaths(List<Lane> to, List<Node> from, Direction direction)
    {
        int laneCount = to.Count;
        Intersection intersection = from.GetIntersection();
        ReadOnlySet<Road> roadsOnOtherSide = direction == Direction.Out ? intersection.InRoads : intersection.OutRoads;
        
        BuildStraightPath();
        BuildRightLaneChangePath();
        BuildLeftLaneChangePath();

        if (BranchExists())
            DeleteAllLaneChangingPaths();
        else
            BuildSidePaths();
        if (intersection.InRoads.Count != 0 && intersection.OutRoads.Count != 0)
            PatchUnconnectedLanes();

        #region extracted 
        void PatchUnconnectedLanes()
        {
            
            Node firstNodeWithOutRoad = intersection.FirstWithRoad(Direction.Out);
            Node lastNodeWithOutRoad = intersection.LastWithRoad(Direction.Out);
            foreach (Road inRoad in intersection.InRoads)
                foreach (Lane inLane in inRoad.Lanes)
                    if (Game.Graph.OutDegree(inLane.EndVertex) == 0)
                    {
                        Node n;
                        int laneNodeIndex = inLane.EndNode.NodeIndex;
                        if (laneNodeIndex < lastNodeWithOutRoad.NodeIndex && laneNodeIndex > firstNodeWithOutRoad.NodeIndex)
                            continue;
                        if (laneNodeIndex < (float)intersection.Count / 2)
                            n = firstNodeWithOutRoad;
                        else
                            n = lastNodeWithOutRoad;
                        foreach (Lane outLane in n.GetLanes(Direction.Out))
                            BuildPath(inLane.EndVertex, outLane.StartVertex, n.NodeIndex - laneNodeIndex);
                    }
            Node firstNodeWithInRoad = intersection.FirstWithRoad(Direction.In);
            Node lastNodeWithInRoad = intersection.LastWithRoad(Direction.In);
            foreach (Road outRoad in intersection.OutRoads)
                foreach (Lane outLane in outRoad.Lanes)
                    if (Game.Graph.InDegree(outLane.StartVertex) == 0)
                    {
                        Node n;
                        int laneNodeIndex = outLane.StartNode.NodeIndex;
                        if (laneNodeIndex < lastNodeWithInRoad.NodeIndex && laneNodeIndex > firstNodeWithInRoad.NodeIndex)
                            continue;
                        if (laneNodeIndex < (float)intersection.Count / 2)
                            n = firstNodeWithInRoad;
                        else
                            n = lastNodeWithInRoad;
                        foreach (Lane inLane in n.GetLanes(Direction.In))
                            BuildPath(inLane.EndVertex, outLane.StartVertex, -n.NodeIndex + laneNodeIndex);
                    }
        }

        void BuildStraightPath()
        {
            for (int i = 0; i < laneCount; i++)
                foreach (Lane lane in from[i].GetLanes(InvertDirection(direction)))
                    BuildPathLane2Lane(lane, to[i], 0);
        }
        void BuildRightLaneChangePath()
        {
            for (int i = 1; i < laneCount; i++)
                foreach (Lane lane in from[i - 1].GetLanes(InvertDirection(direction)))
                    BuildPathLane2Lane(lane, to[i], 1);
        }
        void BuildLeftLaneChangePath()
        {
            for (int i = 0; i < laneCount - 1; i++)
                foreach (Lane lane in from[i + 1].GetLanes(InvertDirection(direction)))
                    BuildPathLane2Lane(lane, to[i], -1);
        }
        void BuildSidePaths()
        {
            foreach (Road road in roadsOnOtherSide)
                foreach (Lane lane in road.Lanes)
                {
                    Node node = direction == Direction.Out ? lane.EndNode : lane.StartNode;
                    if (from.Contains(node))
                        continue;

                    int span = from.First().NodeIndex - node.NodeIndex;
                    if (span == 1)
                        BuildPathLane2Lane(lane, to.First(), span);

                    span = from.Last().NodeIndex - node.NodeIndex;
                    if (span == -1)
                        BuildPathLane2Lane(lane, to.Last(), span);
                }
        }

        static Direction InvertDirection(Direction direction)
        {
            if (direction == Direction.In)
                return Direction.Out;
            return Direction.In;
        }

        Path BuildPathLane2Lane(Lane l1, Lane l2, int span)
        {
            Path path;
            if (direction == Direction.Out)
                path = BuildPath(l1.EndVertex, l2.StartVertex, span);
            else
                path = BuildPath(l2.EndVertex, l1.StartVertex, -span);
            return path;
        }

        Path BuildPath(Vertex start, Vertex end, int span)
        {
            Game.Graph.TryGetEdge(start, end, out Path edge);
            if (edge != null)
                return null;
            float3 pos1 = start.Pos + Constants.MinimumLaneLength / 3 * start.Tangent;
            float3 pos2 = end.Pos - Constants.MinimumLaneLength / 3 * end.Tangent;
            BezierSeries bs = new(new BezierCurve(start.Pos, pos1, pos2, end.Pos));
            Path p = new(bs, start, end, span);
            Game.AddEdge(p);
            return p;
        }

        void DeleteAllLaneChangingPaths()
        {
            foreach (Road road in roadsOnOtherSide)
                foreach (Lane lane in road.Lanes)
                    if (direction == Direction.Out)
                        Game.Graph.RemoveEdgeIf(e => e.Source == lane.EndVertex && Math.Abs(e.Span) > 0);
                    else
                        Game.Graph.RemoveEdgeIf(e => e.Target == lane.StartVertex && Math.Abs(e.Span) > 0);
        }

        bool BranchExists()
        {
            HashSet<Road> seenRoads = new();
            foreach (Road road in roadsOnOtherSide)
                foreach (Lane lane in road.Lanes)
                {
                    Node node = direction == Direction.Out ? lane.EndNode : lane.StartNode;
                    seenRoads.UnionWith(node.GetRoads(direction));
                }
            return seenRoads.Count > 1;
        }
        #endregion
    }

    public static void UpdateOutline(Intersection nodeGroup)
    {
        if (nodeGroup.Count == 0)
            return;
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

        foreach (Road r in nodeGroup.GetRoads())
            Game.InvokeUpdateRoadMesh(r);

        #region extracted
        void EvaluateSideOutline()
        {
            Node firstNodeWithInRoad = nodeGroup.FirstWithRoad(Direction.In);
            Node lastNodeWithInRoad = nodeGroup.LastWithRoad(Direction.In);
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
                    if (nodeGroup.Plane.SameSide(pt, nodeGroup.PointOnInSide))
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

    public static List<float3> GetOutLineAtTwoEnds(Road road, Orientation orientation, Side side)
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

    public static void UpdateOutline(Road road, Side side)
    {
        if (side == Side.Start)
            UpdateOutline(road.StartIntersection);
        if (side == Side.End)
            UpdateOutline(road.EndIntersection);
        if (side == Side.Both)
        {
            UpdateOutline(road.StartIntersection);
            UpdateOutline(road.EndIntersection);
        }
    }
}