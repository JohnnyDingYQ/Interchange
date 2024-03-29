using System;
using System.Collections.Generic;
using QuikGraph.Predicates;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public static class BuildHandler
{
    private static float3 pivotClick;
    private static bool startAssigned, pivotAssigned;
    public static int LaneCount { get; set; }
    private static BuildTargets startTarget;
    private static BuildTargets endTarget;

    static BuildHandler()
    {
        LaneCount = 1;
        startAssigned = false;
        pivotAssigned = false;
    }

    public static void Reset()
    {
        LaneCount = 1;
        startAssigned = false;
        pivotAssigned = false;
    }

    public static Road HandleBuildCommand(float3 clickPos)
    {
        if (!startAssigned)
        {
            startAssigned = true;
            startTarget = new BuildTargets(clickPos, LaneCount, Side.Start, Game.Nodes.Values);
            return null;
        }
        else if (!pivotAssigned)
        {
            pivotAssigned = true;
            pivotClick = clickPos;
            return null;
        }
        else
        {
            endTarget = new BuildTargets(clickPos, LaneCount, Side.End, Game.Nodes.Values);
            startAssigned = false;
            pivotAssigned = false;
            return BuildRoad(startTarget, pivotClick, endTarget);
        }

    }

    static Road BuildRoad(BuildTargets startTarget, float3 pivotPos, BuildTargets endTarget)
    {
        List<Node> startNodes = startTarget.Nodes;
        List<Node> endNodes = endTarget.Nodes;
        float3 startPos = startTarget.SnapNotNull ? startTarget.MedianPoint : startTarget.ClickPos;
        float3 endPos = endTarget.SnapNotNull ? endTarget.MedianPoint : endTarget.ClickPos;

        AlignPivotPos();

        Road road = new(startPos, pivotPos, endPos, LaneCount);

        if (HasLaneShorterThanMinimumLaneLength(road))
        {
            Debug.Log("Lane length is less than " + Constants.MinimumLaneLength);
            return null;
        }

        Game.RegisterRoad(road);

        if (startTarget.SnapNotNull)
        {
            BuildAllPaths(road.Lanes, startNodes, Direction.Out);
            for (int i = 0; i < LaneCount; i++)
            {
                startNodes[i].AddLane(road.Lanes[i], Direction.Out);
                road.Lanes[i].StartNode = startNodes[i];
            }

        }
        if (endTarget.SnapNotNull)
        {
            BuildAllPaths(road.Lanes, endNodes, Direction.In);
            for (int i = 0; i < endNodes.Count; i++)
            {
                road.Lanes[i].EndNode = endNodes[i];
                endNodes[i].AddLane(road.Lanes[i], Direction.In);
            }
        }
        RegisterNodes(road);
        AutoDivideRoad(road);
        return road;

        # region extracted funcitons

        void AlignPivotPos()
        {
            float oldY = pivotPos.y;
            if (startTarget.SnapNotNull)
            {
                Node arbitraryNode = GetArbitraryRegisteredNode(startNodes);
                pivotPos = (float3)Vector3.Project(pivotPos - startPos, arbitraryNode.GetTangent()) + startPos;
            }
            if (endTarget.SnapNotNull)
            {
                Node arbitraryNode = GetArbitraryRegisteredNode(endNodes);
                pivotPos = (float3)Vector3.Project(pivotPos - endPos, arbitraryNode.GetTangent()) + endPos;
            }
            pivotPos.y = oldY;
        }

        static bool HasLaneShorterThanMinimumLaneLength(Road road)
        {
            foreach (Lane lane in road.Lanes)
                if (lane.Length < Constants.MinimumLaneLength)
                    return true;
            return false;
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

        Node GetArbitraryRegisteredNode(List<Node> nodes)
        {
            foreach (Node node in nodes)
            {
                if (node.IsRegistered())
                {
                    return node;
                }
            }
            return null;
        }
        #endregion
    }
    public static void BuildAllPaths(List<Lane> to, List<Node> from, Direction laneDirection)
    {
        BuildStraightPath(to, from, laneDirection);
        BuildRightLaneChangePath(to, from, laneDirection);
        BuildLeftLaneChangePath(to, from, laneDirection);

        static void BuildStraightPath(List<Lane> to, List<Node> from, Direction laneDirection)
        {
            for (int i = 0; i < LaneCount; i++)
                foreach (Lane lane in from[i].GetLanes(InvertDirection(laneDirection)))
                    BuildPath(lane, to[i], laneDirection);
        }
        static void BuildRightLaneChangePath(List<Lane> to, List<Node> from, Direction laneDirection)
        {
            for (int i = 1; i < LaneCount; i++)
                foreach (Lane lane in from[i - 1].GetLanes(InvertDirection(laneDirection)))
                    BuildPath(lane, to[i], laneDirection);
        }
        static void BuildLeftLaneChangePath(List<Lane> to, List<Node> from, Direction laneDirection)
        {
            for (int i = 0; i < LaneCount - 1; i++)
                foreach (Lane lane in from[i + 1].GetLanes(InvertDirection(laneDirection)))
                    BuildPath(lane, to[i], laneDirection);
        }
        static Direction InvertDirection(Direction direction)
        {
            if (direction == Direction.In)
                return Direction.Out;
            return Direction.In;
        }
    }

    static Path BuildPath(Lane l1, Lane l2, Direction l1Direction)
    {
        Path path;
        if (l1Direction == Direction.Out)
            path = BuildPath(l1.EndVertex, l2.StartVertex);
        else
            path = BuildPath(l2.EndVertex, l1.StartVertex);
        return path;
    }
    static Path BuildPath(Vertex start, Vertex end)
    {
        float3 pos1 = start.Pos + Constants.MinimumLaneLength / 4 * start.Tangent;
        float3 pos2 = end.Pos - Constants.MinimumLaneLength / 4 * end.Tangent;
        BezierCurve bezierCurve = new(start.Pos, pos1, pos2, end.Pos);
        ICurve curve = new BezierCurveAdapter(bezierCurve, 0, 1);
        Path p = new(curve, start, end);
        Game.AddEdge(p);
        return p;
    }

    static void RegisterNodes(Road road)
    {
        foreach (Lane lane in road.Lanes)
        {
            if (!lane.StartNode.IsRegistered())
                Game.RegisterNode(lane.StartNode);

            if (!lane.EndNode.IsRegistered())
                Game.RegisterNode(lane.EndNode);
        }
    }

    static void ReloadAllSpline()
    {
        foreach (Road road in Game.Roads.Values)
        {
            road.InitCurve();
            foreach (Lane lane in road.Lanes)
                lane.InitSpline();
        }
    }
    public static void ComplyToNewGameState()
    {
        ReloadAllSpline();
    }
}
