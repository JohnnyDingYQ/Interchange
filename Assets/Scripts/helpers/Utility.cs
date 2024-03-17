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
    public static void DrawBezierCurve(BezierCurve curve, float startT, float endT, Color color, float duration)
    {
        int resolution = 32;
        float3 pos1;
        float3 pos2;
        for (int i = 1; i <= resolution; i++)
        {
            pos1 = CurveUtility.EvaluatePosition(curve, 1 / (float) resolution * (i-1) * (endT - startT) + startT);
            pos2 = CurveUtility.EvaluatePosition(curve, 1 / (float) resolution * i * (endT - startT) + startT);
            Debug.DrawLine(pos1, pos2, color, duration);
        }
    }
    public static void DrawBezierCurve(BezierCurve curve, Color color, float duration)
    {
        DrawBezierCurve(curve, 0, 1, color, duration);
    }

    public static void DrawSpline(Spline spline, float startT, float endT, Color color, float duration)
    {
        int resolution = 32;
        float3 pos1;
        float3 pos2;
        for (int i = 1; i <= resolution; i++)
        {
            pos1 = spline.EvaluatePosition(1 / (float) resolution * (i-1) * (endT - startT) + startT);
            pos2 = spline.EvaluatePosition(1 / (float) resolution * i * (endT - startT) + startT);
            Debug.DrawLine(pos1, pos2, color, duration);
        }
    }

    public static void DrawSpline(Spline spline, Color color, float duration)
    {
        DrawSpline(spline, 0, 1, color, duration);
    }

    public static void DrawRoadsAndLanes(float duration)
    {
        foreach (Road road in Game.Roads.Values)
        {
            if (road.LaneCount % 2 == 0)
            {
                DrawBezierCurve(road.BezierCurve, Color.red, duration);
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

    public static void DrawVertices(float duration)
    {
        foreach(Road road in Game.Roads.Values)
        {
            foreach(Lane lane in road.Lanes)
            {
                DebugExtension.DebugPoint(lane.StartVertex.Pos, Color.cyan, 1, duration);
                DebugExtension.DebugPoint(lane.EndVertex.Pos, Color.cyan, 1, duration);
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

    public static void DrawPaths(float duration)
    {
        foreach(Path path in Game.GameState.Paths)
        {
            foreach(ICurve curve in path.Curves)
            {
                curve.Draw(duration);
            }
        }
    }

    public static bool AreNumericallyEqual(float3 a, float3 b)
    {
        return Vector3.Distance(a, b) < 0.01f;
    }
}