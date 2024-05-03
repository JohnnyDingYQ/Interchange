using System;
using System.Collections.Generic;
using System.Linq;
using GraphExtensions;
using QuikGraph;
using QuikGraph.Algorithms;
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
    public static bool AutoDivideOn { get; set; }

    static Build()
    {
        LaneCount = 1;
        startAssigned = false;
        pivotAssigned = false;
        AutoDivideOn = true;
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
            road.StartIntersection = startTarget.Intersection;

        if (endTarget.SnapNotNull)
            road.EndIntersection = endTarget.Intersection;

        Game.RegisterRoad(road);

        if (startTarget.SnapNotNull)
            ConnectRoadStartToNodes(startNodes, road);
        else
            IntersectionUtil.EvaluateOutline(road.StartIntersection);

        if (endTarget.SnapNotNull)
            ConnectRoadEndToNodes(endNodes, road);
        else
            IntersectionUtil.EvaluateOutline(road.EndIntersection);

        RegisterUnregisteredNodes(road);
        if (buildMode == BuildMode.Actual)
        {
            ReplaceExistingRoad();
            if (AutoDivideOn)
                AutoDivideRoad(road);
        }
        return road;

        # region extracted funcitons

        void AlignPivotPos()
        {
            float oldY = pivotPos.y;
            if (startTarget.SnapNotNull)
                pivotPos = (float3)Vector3.Project(pivotPos - startPos, startTarget.Intersection.Tangent) + startPos;
            if (endTarget.SnapNotNull)
                pivotPos = (float3)Vector3.Project(pivotPos - endPos, endTarget.Intersection.Tangent) + endPos;
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

        void ReplaceExistingRoad()
        {
            if (!(startTarget.SnapNotNull && endTarget.SnapNotNull))
                return;
            List<Vertex> startV = new();
            foreach (Node n in startNodes)
                foreach (Lane l in n.GetLanes(Direction.Out))
                    startV.Add(l.StartVertex);
            List<Vertex> endV = new();
            foreach (Node n in endNodes)
                foreach (Lane l in n.GetLanes(Direction.In))
                    endV.Add(l.EndVertex);

            HashSet<Road> roads = new();
            foreach (Vertex start in startV)
                foreach (Vertex end in endV)
                {
                    List<Path> paths = Game.Graph.GetPathsFromAtoB(start, end)?.ToList();
                    if (paths != null)
                        foreach (Path p in paths)
                        {
                            roads.Add(p.Source.Road);
                            roads.Add(p.Target.Road);
                        }
                }
            foreach (Road r in roads)
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
        if (nodes.First().Intersection != road.StartIntersection)
            throw new InvalidOperationException("Not the same intersection");
        if (!Game.Roads.ContainsKey(road.Id))
            throw new InvalidOperationException("Road to connect not registered");
        for (int i = 0; i < road.LaneCount; i++)
        {
            nodes[i].AddLane(road.Lanes[i], Direction.Out);
            road.Lanes[i].StartNode = nodes[i];
        }
        road.StartIntersection.AddRoad(road, Side.Start);
        IntersectionUtil.EvaluatePaths(road.StartIntersection);
        IntersectionUtil.EvaluateOutline(road.StartIntersection);
    }

    public static void ConnectRoadEndToNodes(List<Node> nodes, Road road)
    {
        if (nodes.First().Intersection != road.EndIntersection)
            throw new InvalidOperationException("Not the same intersection");
        if (!Game.Roads.ContainsKey(road.Id))
            throw new InvalidOperationException("Road to connect not registered");
        for (int i = 0; i < road.LaneCount; i++)
        {
            nodes[i].AddLane(road.Lanes[i], Direction.In);
            road.Lanes[i].EndNode = nodes[i];
        }
        road.EndIntersection.AddRoad(road, Side.End);
        IntersectionUtil.EvaluatePaths(road.EndIntersection);
        IntersectionUtil.EvaluateOutline(road.EndIntersection);
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
        AutoDivideOn = true;
    }
}
