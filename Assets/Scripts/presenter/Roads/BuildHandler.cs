using System.Collections.Generic;
using System.Linq;
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

    public static void HandleBuildCommand(float3 clickPos)
    {
        if (!startAssigned)
        {
            startAssigned = true;
            startTarget = new BuildTargets(clickPos, LaneCount, Side.Start, Game.Nodes.Values);
        }
        else if (!pivotAssigned)
        {
            pivotAssigned = true;
            pivotClick = clickPos;
        }
        else
        {
            endTarget = new BuildTargets(clickPos, LaneCount, Side.End, Game.Nodes.Values);
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

        BezierCurve curve = new(startPos, pivotPos, endPos);
        float length = CurveUtility.CalculateLength(curve);
        if (length < GConsts.MinimumRoadLength || length > GConsts.MaximumRoadLength)
        {
            Debug.Log("Road length of " + length + " is not between " + GConsts.MinimumRoadLength + " and " + GConsts.MaximumRoadLength);
            return;
        }
            
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

        # region extracted funcitons
        void AlignPivotPos()
        {
            float oldY = pivotPos.y;
            if (startTarget.SnapNotNull)
            {
                Node arbitraryNode = GetArbitraryRegisteredNode(startNodes);
                pivotPos = (float3) Vector3.Project(pivotPos - startPos, arbitraryNode.GetTangent()) + startPos;
            }
            if (endTarget.SnapNotNull)
            {
                Node arbitraryNode = GetArbitraryRegisteredNode(endNodes);
                pivotPos = (float3) Vector3.Project(pivotPos - endPos, arbitraryNode.GetTangent()) + endPos;
            }
            pivotPos.y = oldY;
        }

        Node GetArbitraryRegisteredNode(List<Node> nodes)
        {
            foreach(Node node in nodes)
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

    static void AssignNodeNumber(Road road)
    {
        foreach (Lane lane in road.Lanes)
        {
            
            if (!lane.StartNode.IsRegistered())
            {
                lane.StartNode.Id = Game.NextAvailableNodeId++;
                Game.Nodes[lane.StartNode.Id] = lane.StartNode;
            }

            if (!lane.EndNode.IsRegistered())
            {
                lane.EndNode.Id = Game.NextAvailableNodeId++;
                Game.Nodes[lane.EndNode.Id] = lane.EndNode;
            }

        }
    }

    static Road InitRoad(float3 startPos, float3 pivotPos, float3 endPos)
    {
        Road road = new(startPos, pivotPos, endPos, LaneCount);
        Game.RegisterRoad(road);

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
