using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public static class Gizmos
{
    public static void DrawBezierCurve(BezierCurve curve, float startT, float endT, Color color, float duration)
    {
        int resolution = 32;
        float3 pos1;
        float3 pos2;
        for (int i = 1; i <= resolution; i++)
        {
            pos1 = CurveUtility.EvaluatePosition(curve, 1 / (float)resolution * (i - 1) * (endT - startT) + startT);
            pos2 = CurveUtility.EvaluatePosition(curve, 1 / (float)resolution * i * (endT - startT) + startT);
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
            pos1 = spline.EvaluatePosition(1 / (float)resolution * (i - 1) * (endT - startT) + startT);
            pos2 = spline.EvaluatePosition(1 / (float)resolution * i * (endT - startT) + startT);
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
                if ((lane.Order + 1) * 2 - 1 != laneCount)
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
        foreach (Road road in Game.Roads.Values)
        {
            foreach (Lane lane in road.Lanes)
            {
                DebugExtension.DebugPoint(lane.StartVertex.Pos, Color.cyan, 1, duration);
                DebugExtension.DebugPoint(lane.EndVertex.Pos, Color.cyan, 1, duration);
            }
        }
    }

    public static void DrawControlPoints(float duration)
    {
        foreach (Road road in Game.Roads.Values)
        {
            DebugExtension.DebugPoint(road.StartPos, Color.magenta, 1, duration);
            DebugExtension.DebugPoint(road.PivotPos, Color.magenta, 1, duration);
            DebugExtension.DebugPoint(road.EndPos, Color.magenta, 1, duration);
        }
    }

    public static void DrawPaths(float duration)
    {
        foreach (Path path in Game.Graph.Edges)
        {
            path.Curve.Draw(duration);
        }
    }

    public static void DrawOutline(float duration)
    {
        foreach (Road road in Game.Roads.Values)
        {
            DebugExtension.DebugPoint(road.LeftOutline.Mid.First(), Color.green, 1, duration);
            DebugExtension.DebugPoint(road.LeftOutline.Mid.Last(), Color.green, 1, duration);
            DebugExtension.DebugPoint(road.RightOutline.Mid.First(), Color.green, 1, duration);
            DebugExtension.DebugPoint(road.RightOutline.Mid.Last(), Color.green, 1, duration);
            for (int i = 1; i < road.LeftOutline.Mid.Count; i++)
            {
                Debug.DrawLine(road.LeftOutline.Mid[i - 1], road.LeftOutline.Mid[i], Color.green, duration);
                Debug.DrawLine(road.RightOutline.Mid[i - 1], road.RightOutline.Mid[i], Color.green, duration);
            }
            if (road.LeftOutline.Right != null)
                for (int i = 1; i < road.LeftOutline.Right.Count; i++)
                {
                    Debug.DrawLine(road.LeftOutline.Right[i - 1], road.LeftOutline.Right[i], Color.green, duration);
                    Debug.DrawLine(road.RightOutline.Right[i - 1], road.RightOutline.Right[i], Color.green, duration);
                }
            if (road.LeftOutline.Left != null)
                for (int i = 1; i < road.LeftOutline.Left.Count; i++)
                {
                    Debug.DrawLine(road.LeftOutline.Left[i - 1], road.LeftOutline.Left[i], Color.green, duration);
                    Debug.DrawLine(road.LeftOutline.Left[i - 1], road.LeftOutline.Left[i], Color.green, duration);
                }
            
        }
    }
}