using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

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
            ResetBuild();
            return road;
        }

    }

    static Road BuildRoad(BuildTargets startTarget, float3 pivotPos, BuildTargets endTarget, BuildMode buildMode)
    {
        if (startTarget.SnapNotNull && endTarget.SnapNotNull && HasRepeatedTarget())
            return null;
        List<Node> startNodes = startTarget.Nodes;
        List<Node> endNodes = endTarget.Nodes;
        float3 startPos = startTarget.SnapNotNull ? startTarget.MedianPoint : startTarget.ClickPos;
        float3 endPos = endTarget.SnapNotNull ? endTarget.MedianPoint : endTarget.ClickPos;
        AlignPivotPos();

        if (RoadIsTooBent())
            return null;

        if (buildMode == BuildMode.Actual)
            if (startTarget.SnapNotNull && endTarget.SnapNotNull)
                RemoveExistingRoad();

        Road road = new(startPos, pivotPos, endPos, LaneCount)
        {
            IsGhost = buildMode == BuildMode.Ghost
        };

        if (road.HasLaneShorterThanMinimumLaneLength())
        {
            Game.RemoveRoad(road);
            return null;
        }
        Game.RegisterRoad(road);

        if (startTarget.SnapNotNull)
            ConnectRoadStartToNodes(startNodes, road);
        else
            InterRoad.UpdateOutline(road, Side.Start);

        if (endTarget.SnapNotNull)
            ConnectRoadEndToNodes(endNodes, road);
        else
            InterRoad.UpdateOutline(road, Side.End);

        RegisterUnregisteredNodes(road);
        if (buildMode == BuildMode.Actual)
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
                if (Game.HasNode(node))
                    return node;
            return null;
        }

        void RemoveExistingRoad()
        {
            HashSet<Road> startRoads = new();
            HashSet<Road> endRoads = new();
            foreach (Node node in startTarget.Nodes)
                startRoads.UnionWith(node.GetRoads(Direction.Out));
            foreach (Node node in endTarget.Nodes)
                endRoads.UnionWith(node.GetRoads(Direction.In));
            startRoads.IntersectWith(endRoads);
            foreach (Road r in startRoads)
                Game.RemoveRoad(r);
        }

        bool HasRepeatedTarget()
        {
            HashSet<Node> nodes = new(startTarget.Nodes);
            nodes.IntersectWith(endTarget.Nodes);
            if (nodes.Count != 0)
                return true;
            return false;
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
        for (int i = 0; i < road.LaneCount; i++)
        {
            nodes[i].AddLane(road.Lanes[i], Direction.Out);
            road.Lanes[i].StartNode = nodes[i];
        }
        InterRoad.BuildAllPaths(road.Lanes, nodes, Direction.Out);
        NodeGroup nodeGroup = new(nodes.First());
        InterRoad.UpdateOutline(road, Side.Start);

        if (nodeGroup.InRoads.Count == 1)
        {
            road.LeftOutline.AddStartFixedPoint(nodeGroup.InRoads.First().LeftOutline.End.Last());
            road.RightOutline.AddStartFixedPoint(nodeGroup.InRoads.First().RightOutline.End.Last());
        }
        else
        {
            road.LeftOutline.AddStartFixedPoint(nodes.First().Pos + nodeGroup.Normal * Constants.LaneWidth / 2);
            road.RightOutline.AddStartFixedPoint(nodes.Last().Pos - nodeGroup.Normal * Constants.LaneWidth / 2);
        }
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
        InterRoad.UpdateOutline(road, Side.End);
        if (nodeGroup.OutRoads.Count == 1)
        {
            road.LeftOutline.AddEndFixedPoint(nodeGroup.OutRoads.First().LeftOutline.Start.First());
            road.RightOutline.AddEndFixedPoint(nodeGroup.OutRoads.First().RightOutline.Start.First());
        }
        else
        {
            road.LeftOutline.AddEndFixedPoint(nodes.First().Pos + nodeGroup.Normal * Constants.LaneWidth / 2);
            road.RightOutline.AddEndFixedPoint(nodes.Last().Pos - nodeGroup.Normal * Constants.LaneWidth / 2);
        }
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

    public static void ResetBuild()
    {
        startAssigned = false;
        pivotAssigned = false;
        startTarget = null;
        endTarget = null;
    }
}
