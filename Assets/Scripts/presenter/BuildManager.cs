using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public static class BuildManager
{
    private static float3 pivotClick;
    private static bool startAssigned, pivotAssigned;
    public static int LaneCount { get; set; }
    public static IBuildManagerBoundary Client;
    private static BuildTargets startTarget;
    private static BuildTargets endTarget;

    static BuildManager()
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
        Client = null;
    }

    public static void HandleBuildCommand()
    {
        float3 clickPos = Client.GetPos();

        if (!startAssigned)
        {
            startAssigned = true;
            startTarget = new BuildTargets(clickPos, LaneCount, Side.Start);
        }
        else if (!pivotAssigned)
        {
            pivotAssigned = true;
            pivotClick = clickPos;
        }
        else
        {
            endTarget = new BuildTargets(clickPos, LaneCount, Side.End);
            BuildRoad(startTarget, pivotClick, endTarget);
            startAssigned = false;
            pivotAssigned = false;
        }

    }

    static void BuildRoad(BuildTargets startTarget, float3 pivotPos, BuildTargets endTarget)
    {
        List<Node> startNodes = startTarget.Nodes;
        List<Node> endNodes = endTarget.Nodes;
        float3 startPos = startTarget.SnapNotNull ? startTarget.MedianPoint : startTarget.ClickPos;
        float3 endPos = endTarget.SnapNotNull ? endTarget.MedianPoint : endTarget.ClickPos;
        
        AlignPivotPos();

        Road road = InitRoad(startPos, pivotPos, endPos);

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

        AssignNodeNumber(road);

        void AlignPivotPos()
        {
            float oldY = pivotPos.y;
            if (startTarget.SnapNotNull)
            {
                Node arbitaryNode = startNodes.First();
                pivotPos = (float3) Vector3.Project(pivotPos - startPos, arbitaryNode.GetTangent()) + startPos;
            }
            if (endTarget.SnapNotNull)
            {
                Node arbitaryNode = endNodes.First();
                pivotPos = (float3) Vector3.Project(pivotPos - endPos, arbitaryNode.GetTangent()) + endPos;
            }
            pivotPos.y = oldY;
        }
    }

    static void AssignNodeNumber(Road road)
    {
        foreach (Lane lane in road.Lanes)
        {
            
            if (lane.StartNode.Id == -1)
            {
                lane.StartNode.Id = Game.NextAvailableNodeId++;
                Game.Nodes[lane.StartNode.Id] = lane.StartNode;
            }

            if (lane.EndNode.Id == -1)
            {
                lane.EndNode.Id = Game.NextAvailableNodeId++;
                Game.Nodes[lane.EndNode.Id] = lane.EndNode;
            }

        }
    }

    static Road InitRoad(float3 startPos, float3 pivotPos, float3 endPos)
    {
        Road road = new(startPos, pivotPos, endPos, LaneCount)
        {
            Id = Game.NextAvailableRoadId++
        };
        Game.Roads.Add(road.Id, road);

        Client.InstantiateRoad(road);
        return road;
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
