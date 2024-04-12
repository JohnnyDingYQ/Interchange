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
        int span;
        
        span = 0;
        BuildStraightPath();
        span = 1;
        BuildRightLaneChangePath();
        span = -1;
        BuildLeftLaneChangePath();

        if (BranchExists())
            DeleteAllLaneChangingPaths();
        else
            BuildSidePaths();

        Patch();

        #region extracted functions
        void Patch()
        {
            List<Node> nodes = GetRelevantNodes();
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
                            BuildLanePath(l1, l2);
                }
                nodes.Reverse();
            }
        }

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
                    if (from.Contains(node))
                        continue;

                    span = from.First().Order - node.Order;
                    if (span == 1)
                        BuildLanePath(lane, to.First());

                    span = from.Last().Order - node.Order;
                    if (span == -1)
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
            Game.Graph.TryGetEdge(start, end, out Path edge);
            if (edge != null)
                return null;
            float3 pos1 = start.Pos + Constants.MinimumLaneLength / 4 * start.Tangent;
            float3 pos2 = end.Pos - Constants.MinimumLaneLength / 4 * end.Tangent;
            BezierCurve bezierCurve = new(start.Pos, pos1, pos2, end.Pos);
            ICurve curve = new BezierCurveAdapter(bezierCurve);
            Path p = new(curve, start, end, direction == Direction.Out ? span : -span);
            Game.AddEdge(p);
            return p;
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

        List<Node> GetRelevantNodes()
        {
            HashSet<Node> nodes = new();
            foreach (Road road in GetRelevantRoads())
                foreach (Lane lane in road.Lanes)
                {
                    Node node = direction == Direction.Out ? lane.EndNode : lane.StartNode;
                    nodes.Add(node);
                }
            List<Node> n = new(nodes);
            n.Sort();
            return n;
        }
        #endregion
    }

    public static void UpdateOutPath(Road road)
    {
        // if (Game.Graph.TryGetOutEdges(Lanes.First().EndVertex, out IEnumerable<Path> lEdges))
        // {
        //     int numPoints = (int)(Constants.MinimumLaneLength * Constants.MeshResolution);
        //     List<Path> l = new(lEdges);
        //     l.Sort();
        //     LeftOutline.Right = GetOutline(Lanes.First().EndVertex, l.First().Target, numPoints, true);
        // }
        // if (Game.Graph.TryGetOutEdges(Lanes.Last().EndVertex, out IEnumerable<Path> rEdges))
        // {
        //     int numPoints = (int)(Constants.MinimumLaneLength * Constants.MeshResolution);
        //     List<Path> l = new(rEdges);
        //     l.Sort();
        //     RightOutline.Right = GetOutline(Lanes.Last().EndVertex, l.Last().Target, numPoints, false);
        // }
    }
}