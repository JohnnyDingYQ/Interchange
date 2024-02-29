using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public static class Utility
{
    private static Logger info;
    public static Logger Info
    {
        get
        {
            info ??= new Logger(Debug.unityLogger.logHandler);
            return info;
        }
        set
        {
            info = value;
        }
    }
    public static void DrawSpline(Spline spline, Color color, float duration)
    {
        int count = 1;
        IEnumerable<BezierKnot> k = spline.Knots;
        while (count < k.Count())
        {
            Debug.DrawLine(k.ElementAt(count).Position, k.ElementAt(count - 1).Position, color, duration);
            count += 1;
        }
    }

    public static void DrawAllRoads(float duration)
    {
        foreach (Road road in Game.RoadWatcher.Values)
        {
            DrawSpline(road.Spline, Color.red, duration);
            int laneCount = road.Lanes.Count;
            foreach (Lane lane in road.Lanes)
            {
                if ((lane.LaneIndex + 1) * 2 - 1 != laneCount)
                    DrawSpline(lane.Spline, Color.white, duration);
            }
        }
    }

    public static Road FindRoadWithStartPos(float3 startPos)
    {
        foreach (Road road in Game.RoadWatcher.Values)
        {
            if (road.StartPos.Equals(startPos))
            {
                return road;
            }
        }
        return null;
    }
    public static Road FindRoadWithEndPos(float3 endPos)
    {
        foreach (Road road in Game.RoadWatcher.Values)
        {
            if (road.EndPos.Equals(endPos))
            {
                return road;
            }
        }
        return null;
    }
}