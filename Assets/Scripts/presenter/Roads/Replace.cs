using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public static class ReplaceHandler
{
    public static void ReplaceRoad(Road road, int newLaneCount, int offset)
    {
        // float centeroffset = -((float)newLaneCount / 2 - (float)road.LaneCount / 2 + offset) * Constants.LaneWidth / 2;
        float centeroffset = -0.5f * Constants.LaneWidth;
        float3 pivot = road.PivotPos;
        pivot.y = 0;
        Ray ray = new(pivot, Vector3.up);
        float distance = CurveUtility.GetNearestPoint(road.BezierCurve, ray, out float3 position, out float interpolation);
        Road newRoad = new(
            road.StartPos + centeroffset * road.Get2DNormal(0),
            road.PivotPos + centeroffset * road.Get2DNormal(interpolation),
            road.EndPos + centeroffset * road.Get2DNormal(1),
            newLaneCount
        );
        Game.RegisterRoad(newRoad);
        // Game.RemoveRoad(road);
    }
}