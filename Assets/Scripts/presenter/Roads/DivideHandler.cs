using System;
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
        float distance = CurveUtility.GetNearestPoint(road.Curve, ray, out float3 position, out float interpolation);
        return interpolation;
    }

    public static SubRoads DivideRoad(Road road, float interpolation)
    {
        if (!Game.Roads.ContainsKey(road.Id))
            return null;
        Game.RemoveRoad(road);
        int laneCount = road.LaneCount;
        CurveUtility.Split(road.Curve, interpolation, out BezierCurve left, out BezierCurve right);
        Road LeftRoad = new(left, laneCount);
        Game.RegisterRoad(LeftRoad);
        Road RightRoad = new(right, laneCount);
        Game.RegisterRoad(RightRoad);
        OperateNodes();

        return new SubRoads(LeftRoad, RightRoad);

        void OperateNodes()
        {
            for (int i = 0; i < LeftRoad.LaneCount; i++)
            {
                Lane laneLeft = LeftRoad.Lanes[i];
                Lane laneRight = RightRoad.Lanes[i];
                Lane lane = road.Lanes[i];
                laneLeft.EndNode = laneRight.StartNode;
                laneLeft.EndNode.Lanes.Add(laneLeft);
                Game.RegisterNode(laneLeft.EndNode);

                laneLeft.StartNode = lane.StartNode;
                lane.StartNode.Lanes.Add(laneLeft);

                laneRight.EndNode = lane.EndNode;
                lane.EndNode.Lanes.Add(laneRight);
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