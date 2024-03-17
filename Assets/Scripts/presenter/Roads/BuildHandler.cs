using System.Collections.Generic;
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
        foreach (Lane lane in road.Lanes)
        {
            float length = lane.Length;
            if (length < Constants.MinimumLaneLength || length > Constants.MaximumLaneLength)
            {
                Debug.Log("Lane length of " + length + " is not between " + Constants.MinimumLaneLength + " and " + Constants.MaximumLaneLength);
                return null;
            }
        }

        Game.RegisterRoad(road);

        if (startTarget.SnapNotNull)
        {
            for (int i = 0; i < startNodes.Count; i++)
            {
                startNodes[i].Lanes.Add(road.Lanes[i]);
                road.Lanes[i].StartNode = startNodes[i];
            }

        }
        if (endTarget.SnapNotNull)
        {
            for (int i = 0; i < endNodes.Count; i++)
            {
                endNodes[i].Lanes.Add(road.Lanes[i]);
                road.Lanes[i].EndNode = endNodes[i];
            }
        }

        RegisterNodes(road);
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
