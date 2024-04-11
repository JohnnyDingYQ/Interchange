using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine.Splines;
using UnityEngine;

public static class PathHandler
{
    public static void BuildAllPaths(List<Lane> to, List<Node> from, Direction direction)
    {
        int laneCount = to.Count;
        int span;

        DeletePathWithSpanLargerThanOne();

        span = 0;
        BuildStraightPath();
        span = 1;
        BuildRightLaneChangePath();
        span = -1;
        BuildLeftLaneChangePath();

        if (!BranchExists())
            BuildSidePaths();
        else
            DeleteAllLaneChangingPaths();

        #region extracted functions
        void BuildStraightPath()
        {
            for (int i = 0; i < laneCount; i++)
                foreach (Lane lane in from[i].GetLanes(InvertDirection(direction)))
                    BuildLanePath(lane, to[i]);
        }
        void BuildRightLaneChangePath()
        {
            for (int i = 1; i < laneCount; i++)
                foreach (Lane lane in from[i - 1].GetLanes(InvertDirection(direction)))
                    BuildLanePath(lane, to[i]);
        }
        void BuildLeftLaneChangePath()
        {
            for (int i = 0; i < laneCount - 1; i++)
                foreach (Lane lane in from[i + 1].GetLanes(InvertDirection(direction)))
                    BuildLanePath(lane, to[i]);
        }
        void BuildSidePaths()
        {
            foreach (Road road in GetRelevantRoads())
                foreach (Lane lane in road.Lanes)
                {
                    Node node = direction == Direction.Out ? lane.EndNode : lane.StartNode;
                    span = from.First().Order - node.Order;
                    if (span > 1 && node.GetLanes(direction).Count == 0)
                        BuildLanePath(lane, to.First());
                    else if (span == 1)
                        BuildLanePath(lane, to.First());
                    span = from.Last().Order - node.Order;
                    if (span < -1 && node.GetLanes(direction).Count == 0)
                        BuildLanePath(lane, to.Last());
                    else if (span == -1)
                        BuildLanePath(lane, to.Last());
                }
        }

        static Direction InvertDirection(Direction direction)
        {
            if (direction == Direction.In)
                return Direction.Out;
            return Direction.In;
        }

        Path BuildLanePath(Lane l1, Lane l2)
        {
            Path path;
            if (direction == Direction.Out)
                path = BuildPath(l1.EndVertex, l2.StartVertex);
            else
                path = BuildPath(l2.EndVertex, l1.StartVertex);
            return path;
        }

        Path BuildPath(Vertex start, Vertex end)
        {
            float3 pos1 = start.Pos + Constants.MinimumLaneLength / 4 * start.Tangent;
            float3 pos2 = end.Pos - Constants.MinimumLaneLength / 4 * end.Tangent;
            BezierCurve bezierCurve = new(start.Pos, pos1, pos2, end.Pos);
            ICurve curve = new BezierCurveAdapter(bezierCurve);
            Path p = new(curve, start, end, direction == Direction.Out ? span : -span);
            Game.AddEdge(p);
            return p;
        }

        void DeletePathWithSpanLargerThanOne()
        {
            foreach (Node node in from)
            {
                foreach (Lane lane in node.GetLanes(InvertDirection(direction)))
                    if (direction == Direction.Out)
                        Game.Graph.RemoveEdgeIf(e => e.Source == lane.EndVertex && Math.Abs(e.Span) > 1);
                    else
                        Game.Graph.RemoveEdgeIf(e => e.Target == lane.StartVertex && Math.Abs(e.Span) > 1);
            }
        }

        bool BranchExists()
        {
            HashSet<Road> seenRoads = new();
            foreach (Road road in GetRelevantRoads())
                foreach (Lane lane in road.Lanes)
                {
                    Node node = direction == Direction.Out ? lane.EndNode : lane.StartNode;
                    seenRoads.UnionWith(node.GetRoads(direction));
                }
            return seenRoads.Count > 1;
        }

        HashSet<Road> GetRelevantRoads()
        {
            HashSet<Road> roads = new();
            for (int i = 0; i < laneCount; i++)
                foreach (Lane lane in from[i].GetLanes(InvertDirection(direction)))
                    roads.Add(lane.Road);
            return roads;
        }
        void DeleteAllLaneChangingPaths()
        {
            foreach (Road road in GetRelevantRoads())
                foreach (Lane lane in road.Lanes)
                    if (direction == Direction.Out)
                        Game.Graph.RemoveEdgeIf(e => e.Source == lane.EndVertex && Math.Abs(e.Span) > 0);
                    else
                        Game.Graph.RemoveEdgeIf(e => e.Target == lane.StartVertex && Math.Abs(e.Span) > 0);
        }
        #endregion
    }
}