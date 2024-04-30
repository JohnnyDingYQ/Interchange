using System;
using System.Collections.Generic;
using System.Linq;
using GraphExtensions;
using ListExtensions;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public static class Build
{
    private static float3 pivotPos;
    private static bool startAssigned, pivotAssigned;
    public static int LaneCount { get; set; }
    private static BuildTargets startTarget;
    private static BuildTargets endTarget;

    static Build()
    {
        LaneCount = 1;
        startAssigned = false;
        pivotAssigned = false;
    }

    public static BuildTargets GetStartTarget()
    {
        return startTarget;
    }

    public static bool ShouldShowGhostRoad()
    {
        return startAssigned == true && pivotAssigned == true;
    }

    public static Road BuildGhostRoad(float3 endTargetClickPos)
    {
        BuildTargets tempEnd = new(endTargetClickPos, LaneCount, Game.Nodes.Values);
        Road road = BuildRoad(startTarget, pivotPos, tempEnd, BuildMode.Ghost);
        return road;
    }

    public static Road HandleBuildCommand(float3 clickPos)
    {
        if (!startAssigned)
        {
            startAssigned = true;
            startTarget = new(clickPos, LaneCount, Game.Nodes.Values);
            return null;
        }
        else if (!pivotAssigned)
        {
            pivotAssigned = true;
            pivotPos = clickPos;
            return null;
        }
        else
        {
            endTarget = new(clickPos, LaneCount, Game.Nodes.Values);
            Road road = BuildRoad(startTarget, pivotPos, endTarget, BuildMode.Actual);
            Reset();
            return road;
        }

    }

    static Road BuildRoad(BuildTargets startTarget, float3 pivotPos, BuildTargets endTarget, BuildMode buildMode)
    {
        List<Node> startNodes = startTarget.Nodes;
        List<Node> endNodes = endTarget.Nodes;
        float3 startPos = startTarget.SnapNotNull ? startTarget.MedianPoint : startTarget.ClickPos;
        float3 endPos = endTarget.SnapNotNull ? endTarget.MedianPoint : endTarget.ClickPos;

        AlignPivotPos();

        if (RoadIsTooBent())
            return null;

        Road road = new(startPos, pivotPos, endPos, LaneCount)
        {
            IsGhost = buildMode == BuildMode.Ghost
        };

        if (road.HasLaneShorterThanMinimumLaneLength())
            return null;

        if (startTarget.SnapNotNull)
            road.StartIntersection = startNodes.GetIntersection();

        if (endTarget.SnapNotNull)
            road.EndIntersection = endNodes.GetIntersection();

        Game.RegisterRoad(road);
            
        if (startTarget.SnapNotNull)
            ConnectRoadStartToNodes(startNodes, road);
        else
            road.StartIntersection.UpdateOutline();

        if (endTarget.SnapNotNull)
            ConnectRoadEndToNodes(endNodes, road);
        else
            road.EndIntersection.UpdateOutline();

        RegisterUnregisteredNodes(road);
        if (buildMode == BuildMode.Actual)
        {
            AutoDivideRoad(road);
            RemoveExistingRoad();
        }
        return road;

        # region extracted funcitons

        void AlignPivotPos()
        {
            float oldY = pivotPos.y;
            if (startTarget.SnapNotNull)
                pivotPos = (float3)Vector3.Project(pivotPos - startPos, startNodes.GetIntersection().Tangent) + startPos;
            if (endTarget.SnapNotNull)
                pivotPos = (float3)Vector3.Project(pivotPos - endPos, endNodes.GetIntersection().Tangent) + endPos;
            pivotPos.y = oldY;
        }

        static float GetLongestLaneLength(Road road)
        {
            float length = 0;
            foreach (Lane lane in road.Lanes)
                length = Math.Max(length, lane.Length);
            return length;
        }

        static void AutoDivideRoad(Road road)
        {
            float longestLength = GetLongestLaneLength(road);
            if (longestLength <= Constants.MaximumLaneLength)
                return;
            int divisions = 2;
            while (longestLength / divisions > Constants.MaximumLaneLength)
                divisions++;
            RecursiveRoadDivision(road, divisions);
        }

        static void RecursiveRoadDivision(Road road, int divisions)
        {
            if (divisions == 1)
                return;
            SubRoads subRoads = DivideHandler.DivideRoad(road, 1 / (float)divisions);
            RecursiveRoadDivision(subRoads.Right, divisions - 1);
        }

        void RemoveExistingRoad()
        {
            if (!(startTarget.SnapNotNull && endTarget.SnapNotNull))
                return;
            HashSet<Road> startRoads = new();
            HashSet<Road> endRoads = new();
            foreach (Node node in startTarget.Nodes)
                startRoads.UnionWith(node.GetRoads(Direction.Out));
            foreach (Node node in endTarget.Nodes)
                endRoads.UnionWith(node.GetRoads(Direction.In));
            startRoads.IntersectWith(endRoads);
            foreach (Road r in startRoads)
                if (r != road)
                    Game.RemoveRoad(r);
        }

        bool RoadIsTooBent()
        {
            float3 v1 = pivotPos - startPos;
            float3 v2 = endPos - pivotPos;
            float angle = MathF.Abs(MathF.Acos(math.dot(v1, v2) / math.length(v1) / math.length(v2)));
            if (angle > Constants.MaxRoadBendAngle * MathF.PI / 180)
                return true;
            return false;
        }
        #endregion
    }

    public static void ConnectRoadStartToNodes(List<Node> nodes, Road road)
    {
        if (nodes.GetIntersection() != road.StartIntersection)
            throw new InvalidOperationException("Not the same intersection");
        if (!Game.Roads.ContainsKey(road.Id))
            throw new InvalidOperationException("Road to connect not registered");
        for (int i = 0; i < road.LaneCount; i++)
        {
            nodes[i].AddLane(road.Lanes[i], Direction.Out);
            road.Lanes[i].StartNode = nodes[i];
        }
        road.StartIntersection.AddRoad(road, Side.Start);
        BuildAllPaths(road.Lanes, nodes, Direction.Out);
    }

    public static void ConnectRoadEndToNodes(List<Node> nodes, Road road)
    {
        if (nodes.GetIntersection() != road.EndIntersection)
            throw new InvalidOperationException("Not the same intersection");
        if (!Game.Roads.ContainsKey(road.Id))
            throw new InvalidOperationException("Road to connect not registered");
        for (int i = 0; i < road.LaneCount; i++)
        {
            nodes[i].AddLane(road.Lanes[i], Direction.In);
            road.Lanes[i].EndNode = nodes[i];
        }
        road.EndIntersection.AddRoad(road, Side.End);
        BuildAllPaths(road.Lanes, nodes, Direction.In);
    }

    static void RegisterUnregisteredNodes(Road road)
    {
        foreach (Lane lane in road.Lanes)
        {
            if (!Game.HasNode(lane.StartNode))
                Game.RegisterNode(lane.StartNode);

            if (!Game.HasNode(lane.EndNode))
                Game.RegisterNode(lane.EndNode);
        }
    }

    public static BuildTargets PollBuildTarget(float3 clickPos)
    {
        if (!startAssigned)
            return new(clickPos, LaneCount, Game.Nodes.Values);
        return new(clickPos, LaneCount, Game.Nodes.Values);
    }

    public static void Reset()
    {
        startAssigned = false;
        pivotAssigned = false;
        startTarget = null;
        endTarget = null;
    }

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
        
        intersection.UpdateOutline();

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
}
