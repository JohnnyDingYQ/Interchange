using System;
using System.Collections.Generic;
using System.Linq;
using GraphExtensions;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

public static class Build
{
    private static float3 pivotPos;
    private static bool startAssigned, pivotAssigned;
    public static int LaneCount { get; set; }
    private static BuildTargets startTarget;
    private static BuildTargets endTarget;
    private static Zone startZone;
    private static Zone endZone;
    public static bool AutoDivideOn { get; set; }
    public static List<Tuple<float3, float3, float>> SupportLines { get; }
    public static bool BuildsGhostRoad { get; set; }
    public static bool EnforcesTangent { get; set; }

    static Build()
    {
        LaneCount = 1;
        startAssigned = false;
        pivotAssigned = false;
        AutoDivideOn = true;
        SupportLines = new();
        BuildsGhostRoad = true;
        EnforcesTangent = true;
    }

    public static BuildTargets GetStartTarget()
    {
        return startTarget;
    }

    public static List<Tuple<float3, float3, float>> SetSupportLines()
    {
        SupportLines.Clear();
        if (startAssigned)
        {
            float3 startPoint = startTarget.SnapNotNull ? startTarget.MedianPoint : startTarget.ClickPos;
            float3 pivotPoint = pivotPos;
            startPoint.y = 0;
            pivotPoint.y = 0;
            SupportLines.Add(new(startPoint, pivotPoint, math.length(startPoint - pivotPoint)));
        }
        if (pivotAssigned && endTarget != null)
        {
            float3 endPoint = endTarget.SnapNotNull ? endTarget.MedianPoint : endTarget.ClickPos;
            float3 pivotPoint = pivotPos;
            endPoint.y = 0;
            pivotPoint.y = 0;
            SupportLines.Add(new(pivotPoint, endPoint, math.length(endPoint - pivotPoint)));
        }
        return SupportLines;
    }

    public static Road BuildGhostRoad(float3 endTargetClickPos)
    {
        endTarget = new(endTargetClickPos, LaneCount, Game.Nodes.Values);
        Road road = BuildRoad(startTarget, pivotPos, endTarget, BuildMode.Ghost);
        if (road == null)
            Game.RemoveRoad(Constants.GhostRoadId);
        return road;
    }

    public static void HandleHover(float3 hoverPos)
    {
        if (startAssigned && !pivotAssigned)
            pivotPos = hoverPos;
        if (EnforcesTangent && !pivotAssigned)
            pivotPos = AlignPivotPos(startTarget, pivotPos, endTarget);
        if (startAssigned && pivotAssigned && BuildsGhostRoad)
            BuildGhostRoad(hoverPos);
        SetSupportLines();
    }

    public static Road HandleBuildCommand(float3 clickPos)
    {
        if (!startAssigned)
        {
            startAssigned = true;
            startTarget = new(clickPos, LaneCount, Game.Nodes.Values);
            startZone = clickPos.y == Constants.MinElevation ? Game.HoveredZone : null;
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
            endZone = clickPos.y == Constants.MinElevation ? Game.HoveredZone : null;
            Road road = null;
            if (!(startZone == endZone && startZone != null))
                road = BuildRoad(startTarget, pivotPos, endTarget, BuildMode.Actual);
            ResetSelection();
            return road;
        }

    }

    static Road BuildRoad(BuildTargets startTarget, float3 pivotPos, BuildTargets endTarget, BuildMode buildMode)
    {
        List<Node> startNodes = startTarget.Nodes;
        List<Node> endNodes = endTarget.Nodes;
        float3 startPos = startTarget.SnapNotNull ? startTarget.MedianPoint : startTarget.ClickPos;
        float3 endPos = endTarget.SnapNotNull ? endTarget.MedianPoint : endTarget.ClickPos;

        if (EnforcesTangent)
            pivotPos = AlignPivotPos(startTarget, pivotPos, endTarget);

        if (RoadIsTooBent())
            return null;

        Road road = new(startPos, pivotPos, endPos, LaneCount)
        {
            Id = buildMode == BuildMode.Ghost ? Constants.GhostRoadId : 0
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

        if (buildMode == BuildMode.Actual)
        {
            RegisterUnregisteredNodes(road);
            ReplaceExistingRoad();
            endZone?.AddInRoad(road);
            startZone?.AddOutRoad(road);
            if (AutoDivideOn)
                AutoDivideRoad(road);
        }
        return road;

        # region extracted funcitons
        static float GetLongestLaneLength(Road road)
        {
            float length = 0;
            foreach (Lane lane in road.Lanes)
                length = Math.Max(length, lane.Length);
            return length;
        }

        /// Returns last road
        static Road AutoDivideRoad(Road road)
        {
            float longestLength = GetLongestLaneLength(road);
            if (longestLength <= Constants.MaximumLaneLength)
                return road;
            int divisions = 2;
            while (longestLength / divisions > Constants.MaximumLaneLength)
                divisions++;
            return RecursiveRoadDivision(road, divisions);
        }

        static Road RecursiveRoadDivision(Road road, int divisions)
        {
            if (divisions == 1)
                return road;
            SubRoads subRoads = DivideHandler.DivideRoad(road, 1 / (float)divisions);
            return RecursiveRoadDivision(subRoads.Right, divisions - 1);
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
                    List<Path> paths = Game.ShortestPathAStar(start, end)?.ToList();
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
            float angle = MathF.Abs(
                MathF.Acos(
                    Math.Clamp(math.dot(v1, v2) / math.length(v1) / math.length(v2), -1f, 1f)
                    )
            );
            if (angle > Constants.MaxRoadBendAngle * MathF.PI / 180)
                return true;
            return false;
        }
        #endregion
    }

    static float3 AlignPivotPos(BuildTargets startTarget, float3 pivotPos, BuildTargets endTarget)
    {
        float oldY = pivotPos.y;
        if (startTarget != null && startTarget.SnapNotNull)
            pivotPos = math.project(pivotPos - startTarget.MedianPoint, startTarget.Intersection.Tangent) + startTarget.MedianPoint;
        if (endTarget != null && endTarget.SnapNotNull)
            pivotPos = math.project(pivotPos - endTarget.MedianPoint, endTarget.Intersection.Tangent) + endTarget.MedianPoint;
        pivotPos.y = oldY;
        return pivotPos;
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

    public static bool RemoveRoad(Road road, bool retainVertices)
    {
        Game.Roads.Remove(road.Id);
        foreach (Lane lane in road.Lanes)
        {
            List<Path> toRemove = new();
            if (!retainVertices)
            {
                foreach (Path p in Game.Paths.Values)
                    if (p.Source == lane.StartVertex || p.Target == lane.EndVertex || p.Source == lane.EndVertex || p.Target == lane.StartVertex)
                        toRemove.Add(p);
                Game.RemoveVertex(lane.StartVertex);
                Game.RemoveVertex(lane.EndVertex);
            }
            else
            {

                foreach (Path p in Game.Paths.Values)
                    if (p.Source == lane.StartVertex && p.Target == lane.EndVertex)
                        toRemove.Add(p);
            }

            foreach (Path p in toRemove)
                Game.RemovePath(p);

            foreach (Node node in new List<Node>() { lane.StartNode, lane.EndNode })
            {
                node.RemoveLane(lane);
                if (node.Lanes.Count == 0)
                {
                    Game.Nodes.Remove(node.Id);
                    road.StartIntersection.RemoveNode(node);
                    road.EndIntersection.RemoveNode(node);
                }
            }
            Game.RemoveLane(lane);
        }
        road.StartIntersection.RemoveRoad(road, Side.Start);
        road.EndIntersection.RemoveRoad(road, Side.End);
        if (!road.StartIntersection.IsEmpty())
        {
            IntersectionUtil.EvaluatePaths(road.StartIntersection);
            IntersectionUtil.EvaluateOutline(road.StartIntersection);
        }
        else
            Game.RemoveIntersection(road.StartIntersection);

        if (!road.EndIntersection.IsEmpty())
        {
            IntersectionUtil.EvaluatePaths(road.EndIntersection);
            IntersectionUtil.EvaluateOutline(road.EndIntersection);
        }
        else
            Game.RemoveIntersection(road.EndIntersection);

        if (Game.Zones.ContainsKey(road.StartZoneId))
            Game.Zones[road.StartZoneId].RemoveRoad(road);

        if (Game.Zones.ContainsKey(road.EndZoneId))
            Game.Zones[road.EndZoneId].RemoveRoad(road);

        Game.InvokeRoadRemoved(road);
        return true;
    }

    public static void Reset()
    {
        ResetSelection();
        AutoDivideOn = true;
        EnforcesTangent = true;
        BuildsGhostRoad = true;
    }

    public static void ResetSelection()
    {
        startAssigned = false;
        pivotAssigned = false;
        startTarget = null;
        endTarget = null;
        pivotPos = 0;
        Game.RemoveRoad(Constants.GhostRoadId);
    }
}
