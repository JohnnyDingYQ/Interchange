using System.Collections.Generic;
using Codice.Client.Common.GameUI;
using Unity.Mathematics;
using UnityEngine.Splines;

public static class DivideHandler
{
    public static void HandleDivideCommand(float3 clickPos)
    {
        DivideTargets dt = new(clickPos, Game.Roads.Values);
        DivideRoad(dt.Road, dt.Interpolation);
    }

    public static SubRoads DivideRoad(Road road, float interpolation)
    {
        if (!Game.Roads.ContainsKey(road.Id))
            return null;
        Game.RemoveRoad(road);
        int laneCount = road.LaneCount;
        CurveUtility.Split(road.Curve, interpolation, out BezierCurve left, out BezierCurve right);
        Road lRoad = new(left, laneCount);
        Road rRoad = new(right, laneCount);
        OperateNodes();

        return new SubRoads(lRoad, rRoad);

        void OperateNodes()
        {
            for (int i = 0; i < lRoad.LaneCount; i++)
            {
                Lane laneLeft = lRoad.Lanes[i];
                Lane laneRight = rRoad.Lanes[i];
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