using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using Interchange;

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
                DrawCurve(lane.Curve, Color.white, duration);
    }

    public static void DrawRoadCenter(float duration)
    {
        foreach (Road road in Game.Roads.Values)
            DrawCurve(road.Curve, Color.magenta, duration);
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

    public static void DrawEdges(float duration)
    {
        foreach (Edge edge in Game.Edges.Values)
        {
            DrawCurve(edge.Curve, Color.yellow, duration);
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

    public static void DrawCurve(Curve curve, Color color, float duration)
    {
        int resolution = 30;
        bool flag = true;
        float3 prev = 0;
        foreach (float3 curr in curve.GetOutline(resolution))
        {
            if (flag)
            {
                flag = false;
                prev = curr;
                continue;
            }
            Debug.DrawLine(prev, curr, color, duration);
            prev = curr;
        }

    }

    public static void DrawListofPoints(IEnumerable<float3> l, Color color, float duration)
    {
        if (l == null)
            return;
        if (l.Count() == 1 || l.Count() == 0)
            return;
        float3 prev = l.First();
        l = l.Skip(1);
        foreach (float3 pos in l)
        {
            Debug.DrawLine(prev, pos, color, duration);
            prev = pos;
        }
    }
}