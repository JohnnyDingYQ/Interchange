using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

public static class Log
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

    public static void ShowTile(int id, Color color, float duration)
    {
        Vector3 pos = Grid.GetWorldPosByID(id);
        float offset = (float)Grid.Dim / 2;
        Vector3 a = pos + new Vector3(offset, 0, offset);
        Vector3 b = pos + new Vector3(offset, 0, -offset);
        Vector3 c = pos + new Vector3(-offset, 0, offset);
        Vector3 d = pos + new Vector3(-offset, 0, -offset);
        Debug.DrawLine(a, b, color, duration);
        Debug.DrawLine(c, d, color, duration);
        Debug.DrawLine(a, c, color, duration);
        Debug.DrawLine(b, d, color, duration);
    }

    public static void DrawGridBounds()
    {
        Vector3 bottomLeft = new(0, 0, 0);
        Vector3 bottomRight = new(Grid.Dim * Grid.Width, Grid.Level, 0);
        Vector3 topLeft = new(0, Grid.Level, Grid.Dim * Grid.Height);
        Vector3 topRight = new(Grid.Dim * Grid.Width, Grid.Level, Grid.Dim * Grid.Height);
        Debug.DrawLine(bottomLeft, bottomRight, Color.cyan, 100);
        Debug.DrawLine(bottomLeft, topLeft, Color.cyan, 100);
        Debug.DrawLine(topRight, topLeft, Color.cyan, 100);
        Debug.DrawLine(topRight, bottomRight, Color.cyan, 100);
    }

    public static void DrawSpline(Spline spline, int duration)
    {
        int count = 1;
        IEnumerable<BezierKnot> k = spline.Knots;
        float3 d = new(0, 0.1f, 0);
        while (count < k.Count())
        {
            Debug.DrawLine(k.ElementAt(count).Position + d, k.ElementAt(count - 1).Position + d, Color.white, duration);
            count += 1;
        }
    }
}