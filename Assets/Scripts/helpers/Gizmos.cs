using System;
using System.Collections.Generic;
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

    public static void DrawLanes(float duration)
    {
        foreach (Road road in Game.Roads.Values)
            foreach (Lane lane in road.Lanes)
                DrawBeizerSeries(lane.BezierSeries, Color.white, duration);
    }

    public static void DrawRoadCenter(float duration)
    {
        foreach (Road road in Game.Roads.Values)
            DrawBeizerSeries(road.BezierSeries, Color.magenta, duration);
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
            DebugExtension.DebugPoint(road.EndPos, Color.magenta, 1, duration);
        }
    }

    public static void DrawPaths(float duration)
    {
        foreach (Path path in Game.Graph.Edges)
        {
            DrawBeizerSeries(path.BezierSeries, Color.yellow, duration);
        }
    }

    public static void DrawOutline(float duration)
    {
        foreach (Road road in Game.Roads.Values)
        {
            Color start = new(99.0f / 255, 224.0f / 255, 103.0f / 255, 1);
            Color mid = new(43.0f / 255, 153.0f / 255, 47.0f / 255, 1);
            Color end = new(16.0f / 255, 99.0f / 255, 19.0f / 255, 1);
            DrawListofPoints(road.LeftOutline.Mid, mid, duration);
            DrawListofPoints(road.LeftOutline.Start, start, duration);
            DrawListofPoints(road.LeftOutline.End, end, duration);

            DrawListofPoints(road.RightOutline.Mid, mid, duration);
            DrawListofPoints(road.RightOutline.Start, start, duration);
            DrawListofPoints(road.RightOutline.End, end, duration);
        }
    }

    public static void DrawBeizerSeries(BezierSeries bs, Color color, float duration)
    {
        int resolution = 15;
        float3 prev = bs.EvaluatePosition(0);
        for (int i = 1; i <= resolution; i++)
        {
            float3 cur = bs.EvaluatePosition((float) i / resolution);
            Debug.DrawLine(prev, cur, color, duration);
            prev = cur;
        }

    }

    public static void DrawListofPoints(List<float3> l, Color color, float duration)
    {
        if (l == null)
            return;
        if (l.Count == 1)
            return;
        for (int i = 1; i < l.Count; i++)
            Debug.DrawLine(l[i - 1], l[i], color, duration);
    }

    public static void DrawSupportLine(float duration)
    {
        foreach (Tuple<float3, float3> t in Build.GetSupportLines())
            Debug.DrawLine(t.Item1, t.Item2, Color.blue, duration);
    }
}