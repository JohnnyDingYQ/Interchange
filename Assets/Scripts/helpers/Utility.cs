using System.Collections.Generic;
using System.Linq;
using CodiceApp;
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
    public static void DrawCurve(BezierCurve curve, Color color, float duration)
    {
        int resolution = 32;
        float3 pos1;
        float3 pos2;
        for (int i = 1; i <= resolution; i++)
        {
            pos1 = CurveUtility.EvaluatePosition(curve, 1 / (float) resolution * (i-1));
            pos2 = CurveUtility.EvaluatePosition(curve, 1 / (float) resolution * i);
            Debug.DrawLine(pos1, pos2, color, duration);
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
        foreach (Road road in Game.Roads.Values)
        {
            if (road.LaneCount % 2 == 0)
            {
                DrawCurve(road.Curve, Color.red, duration);
            }
            int laneCount = road.Lanes.Count;
            foreach (Lane lane in road.Lanes)
            {
                if ((lane.LaneIndex + 1) * 2 - 1 != laneCount)
                {
                    DrawSpline(lane.Spline, Color.white, duration);
                }
                else
                {
                    DrawSpline(lane.Spline, Color.magenta, duration);
                }
                    
            }
        }
    }

    public static void DrawControlPoints(float duration)
    {
        foreach(Road road in Game.Roads.Values)
        {
            DebugExtension.DebugPoint(road.StartPos, Color.magenta, 1, duration);
            DebugExtension.DebugPoint(road.PivotPos, Color.magenta, 1, duration);
            DebugExtension.DebugPoint(road.EndPos, Color.magenta, 1, duration);
        }
    }

    public static Road FindRoadWithStartPos(float3 startPos)
    {
        foreach (Road road in Game.Roads.Values)
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
        foreach (Road road in Game.Roads.Values)
        {
            if (road.EndPos.Equals(endPos))
            {
                return road;
            }
        }
        return null;
    }
}