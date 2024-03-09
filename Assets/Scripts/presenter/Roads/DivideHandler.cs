using Unity.Mathematics;
using UnityEngine.Splines;

public static class DivideHandler
{
    public static void HandleDivideCommand(float3 clickPos)
    {
        DivideTargets dt = new(clickPos, Game.Roads.Values);
        DivideRoad(dt.Road, dt.Interpolation);
    }

    public static void DivideRoad(Road road, float interpolation)
    {
        int laneCount = road.LaneCount;
        CurveUtility.Split(road.Curve, interpolation, out BezierCurve left, out BezierCurve right);
        Road roadLeft = new(left, laneCount);
        Road roadRight = new(right, laneCount);
        Game.RemoveRoad(road);
        Game.RegisterRoad(roadLeft);
        Game.RegisterRoad(roadRight);
    }
}