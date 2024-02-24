using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public static class Utility
{

    private const int DrawBoundsDuration = 10000;
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
    public static void DrawSpline(Spline spline, Color color, int duration)
    {
        int count = 1;
        IEnumerable<BezierKnot> k = spline.Knots;
        float3 d = new(0, 0.1f, 0);
        while (count < k.Count())
        {
            Debug.DrawLine(k.ElementAt(count).Position + d, k.ElementAt(count - 1).Position + d, color, duration);
            count += 1;
        }
    }

    public static void DrawAllSplines()
    {
        foreach (Road road in Game.RoadWatcher.Values)
            foreach (Lane lane in road.Lanes)
                {
                    DrawSpline(lane.Spline, Color.white, 1000);
                }
    }
}