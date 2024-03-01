using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public static class BuildManager
{
    private static float3 pivotClick;
    private static bool startAssigned, pivotAssigned;
    public static int LaneCount { get; set; }
    private static int nextAvailableId;
    private static int nextAvailableNodeID;
    public static IBuildManagerBoundary Client;
    private static BuildTargets startTarget;
    private static BuildTargets endTarget;

    static BuildManager()
    {
        LaneCount = 1;
        startAssigned = false;
        pivotAssigned = false;
        nextAvailableId = 0;
        nextAvailableNodeID = 0;
    }

    public static void Reset()
    {
        LaneCount = 1;
        startAssigned = false;
        pivotAssigned = false;
        Client = null;
        nextAvailableId = 0;
        nextAvailableNodeID = 0;
    }

    public static void HandleBuildCommand()
    {
        float3 clickPos = Client.GetPos();

        if (!startAssigned)
        {
            startAssigned = true;
            startTarget = new BuildTargets(clickPos, LaneCount);
            Utility.Info.Log($"Road Manager: StartNode: ");
        }
        else if (!pivotAssigned)
        {
            pivotAssigned = true;
            pivotClick = clickPos;
            Utility.Info.Log($"Road Manager: PivotNode: " + pivotClick);
        }
        else
        {
            endTarget = new BuildTargets(clickPos, LaneCount);
            Utility.Info.Log($"Road Manager: EndNode: ");
            BuildRoad(startTarget, pivotClick, endTarget);
            startAssigned = false;
            pivotAssigned = false;
        }

    }

    static void BuildRoad(BuildTargets startTarget, float3 pivotPos, BuildTargets endTarget)
    {
        List<BuildNode> startNodes = startTarget.BuildNodes;
        List<BuildNode> endNodes = endTarget.BuildNodes;
        float3 startPos = startTarget.SnapNotNull ? startTarget.MedianPoint : startTarget.ClickPos;
        float3 endPos = endTarget.SnapNotNull ? endTarget.MedianPoint : endTarget.ClickPos;

        AlignPivotPos();

        Road road = InitRoad(startPos, pivotPos, endPos);

        if (startTarget.SnapNotNull)
        {
            for (int i = 0; i < startNodes.Count; i++)
            {
                Game.NodeWithLane[startNodes[i].Node].Add(road.Lanes[i]);
                road.Lanes[i].StartNode = startNodes[i].Node;
                Utility.Info.Log("BuildManager: Connecting Start");
            }

        }
        if (endTarget.SnapNotNull)
        {
            for (int i = 0; i < endNodes.Count; i++)
            {
                Game.NodeWithLane[endNodes[i].Node].Add(road.Lanes[i]);
                road.Lanes[i].EndNode = endNodes[i].Node;
                Utility.Info.Log("BuildManager: Connecting End");
            }
        }

        AssignUnassignedNodeNumber(road);

        void AlignPivotPos()
        {
            if (startTarget.SnapNotNull)
            {
                Road other = startTarget.Road;
                pivotPos = (float3) Vector3.Project(pivotPos - startPos, other.Curve.Tangent1) + startPos;
            }
            if (endTarget.SnapNotNull && endTarget.NodeType == NodeType.StartNode)
            {
                Road other = endTarget.Road;
                pivotPos = (float3) Vector3.Project(pivotPos - endPos, other.Curve.Tangent0) + endPos;
            }
            else if (endTarget.SnapNotNull && endTarget.NodeType == NodeType.EndNode)
            {
                Road other = endTarget.Road;
                pivotPos = (float3) Vector3.Project(pivotPos - endPos, other.Curve.Tangent1) + endPos;
            }
        }
    }

    static void AssignUnassignedNodeNumber(Road road)
    {
        foreach (Lane lane in road.Lanes)
        {
            if (lane.StartNode == -1)
            {
                Game.NodeWithLane[nextAvailableNodeID] = new HashSet<Lane>() { lane };
                lane.StartNode = nextAvailableNodeID++;
            }

            if (lane.EndNode == -1)
            {
                Game.NodeWithLane[nextAvailableNodeID] = new HashSet<Lane>() { lane };
                lane.EndNode = nextAvailableNodeID++;
            }

        }
    }

    static Road InitRoad(float3 startPos, float3 pivotPos, float3 endPos)
    {
        Road road = new(startPos, pivotPos, endPos, LaneCount)
        {
            Id = nextAvailableId++
        };
        Game.RoadWatcher.Add(road.Id, road);

        Client.InstantiateRoad(road);
        return road;
    }
    static void ReloadAllSpline()
    {
        foreach (Road road in Game.RoadWatcher.Values)
        {
            road.InitCurve();
            foreach (Lane lane in road.Lanes)
                lane.InitSpline();
        }
    }
    public static void ComplyToNewGameState()
    {
        ReloadAllSpline();
        nextAvailableId = Game.RoadWatcher.Last().Key + 1;
        nextAvailableNodeID = Game.NodeWithLane.Last().Key + 1;
    }
}
