using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public static class DivideHandler
{
    public static SubRoads HandleDivideCommand(Road road, float3 clickPos)
    {
        if (road == null)
            throw new InvalidOperationException("Road to divide cannot be null");
        return DivideRoad(road, GetInterpolation(road, clickPos));
    }

    public static float GetInterpolation(Road road, float3 clickPos)
    {
        clickPos.y = 0;
        Ray ray = new(clickPos, Vector3.up);
        float distance = CurveUtility.GetNearestPoint(road.BezierCurve, ray, out float3 position, out float interpolation);
        return interpolation;
    }

    public static SubRoads DivideRoad(Road road, float interpolation)
    {
        if (!Game.Roads.ContainsKey(road.Id))
            return null;
        int laneCount = road.LaneCount;
        interpolation = CurveUtility.GetDistanceToInterpolation(road.BezierCurve, road.Length * interpolation);
        CurveUtility.Split(road.BezierCurve, interpolation, out BezierCurve left, out BezierCurve right);
        Road leftRoad = new(left, laneCount);
        Road rightRoad = new(right, laneCount);
        if (leftRoad.HasLaneShorterThanMinimumLaneLength() || rightRoad.HasLaneShorterThanMinimumLaneLength())
            return null;
        Game.RemoveRoad(road);
        Game.RegisterRoad(leftRoad);
        Game.RegisterRoad(rightRoad);
        List<Node> leftNodes = new();
        for (int i = 0; i < leftRoad.LaneCount; i++)
            leftNodes.Add(leftRoad.Lanes[i].EndNode);
        OperateNodes();
        BuildHandler.ConnectRoadStartToNodes(leftNodes, rightRoad);
        Game.InvokeUpdateRoadMesh(leftRoad);
        Game.InvokeUpdateRoadMesh(rightRoad);

        return new SubRoads(leftRoad, rightRoad);

        void OperateNodes()
        {
            for (int i = 0; i < leftRoad.LaneCount; i++)
            {
                Lane laneLeft = leftRoad.Lanes[i];
                Lane laneRight = rightRoad.Lanes[i];
                Lane lane = road.Lanes[i];

                laneLeft.StartNode = lane.StartNode;
                lane.StartNode.AddLane(laneLeft, Direction.Out);

                laneRight.EndNode = lane.EndNode;
                lane.EndNode.AddLane(laneRight, Direction.In);

                Game.RegisterNode(laneLeft.EndNode);
                Game.RegisterNode(laneLeft.StartNode);
                Game.RegisterNode(laneRight.EndNode);
            }
        }
    }
}

public class SubRoads
{
    public SubRoads(Road left, Road right)
    {
        Left = left;
        Right = right;
    }
    public Road Left { get; set; }
    public Road Right { get; set; }
}