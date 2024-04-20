using System;
using System.Collections.Generic;
using System.Linq;
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
        Game.RegisterRoad(road);

        if (road.HasLaneShorterThanMinimumLaneLength())
        {
            Debug.Log("Lane length is less than " + Constants.MinimumLaneLength);
            Game.RemoveRoad(road);
            return null;
        }

        if (startTarget.SnapNotNull)
            ConnectRoadStartToNodes(startNodes, road);
        if (endTarget.SnapNotNull)
            ConnectRoadEndToNodes(endNodes, road);

        RegisterUnregisteredNodes(road);
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
                if (node.IsRegistered())
                    return node;
            return null;
        }
        #endregion
    }

    public static void ConnectRoadStartToNodes(List<Node> nodes, Road road)
    {
        for (int i = 0; i < road.LaneCount; i++)
        {
            nodes[i].AddLane(road.Lanes[i], Direction.Out);
            road.Lanes[i].StartNode = nodes[i];
        }
        InterRoad.BuildAllPaths(road.Lanes, nodes, Direction.Out);
        NodeGroup nodeGroup = new(nodes.First());
        InterRoad.UpdateOutline(nodeGroup);
        road.LeftOutline.AddStartFixedPoint(nodes.First().Pos + nodeGroup.Normal * Constants.LaneWidth / 2);
        road.RightOutline.AddStartFixedPoint(nodes.Last().Pos - nodeGroup.Normal * Constants.LaneWidth / 2);
        foreach (Road r in nodeGroup.GetRoads())
            Game.InvokeUpdateRoadMesh(r);
    }

    public static void ConnectRoadEndToNodes(List<Node> nodes, Road road)
    {
        for (int i = 0; i < road.LaneCount; i++)
        {
            nodes[i].AddLane(road.Lanes[i], Direction.In);
            road.Lanes[i].EndNode = nodes[i];
        }
        InterRoad.BuildAllPaths(road.Lanes, nodes, Direction.In);
        NodeGroup nodeGroup = new(nodes.First());
        InterRoad.UpdateOutline(nodeGroup);
        road.LeftOutline.AddEndFixedPoint(nodes.First().Pos + nodeGroup.Normal * Constants.LaneWidth / 2);
        road.RightOutline.AddEndFixedPoint(nodes.Last().Pos - nodeGroup.Normal * Constants.LaneWidth / 2);
        foreach (Road r in nodeGroup.GetRoads())
            Game.InvokeUpdateRoadMesh(r);
    }

    static void RegisterUnregisteredNodes(Road road)
    {
        foreach (Lane lane in road.Lanes)
        {
            if (!lane.StartNode.IsRegistered())
                Game.RegisterNode(lane.StartNode);

            if (!lane.EndNode.IsRegistered())
                Game.RegisterNode(lane.EndNode);
        }
    }
}
