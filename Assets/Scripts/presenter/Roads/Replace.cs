using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public static class ReplaceHandler
{
    public static void ReplaceRoad(Road road, int newLaneCount, int offset)
    {
        // float centeroffset = -((float)newLaneCount / 2 - (float)road.LaneCount / 2 + offset) * Constants.LaneWidth / 2;

        // BezierSeries bs = new(road.BezierCurve);
        // bs = bs.Offset(Constants.LaneWidth / 2, Orientation.Left);
        // Gizmos.DrawBeizerSeries(bs, Color.blue, 10000);
    }
}