using UnityEngine;

public static class Visualizer
{
    public static void DrawAllSplines()
    {
        foreach (Road road in BuildManager.RoadWatcher.Values)
            foreach (Lane lane in road.Lanes)
                {
                    Utility.DrawSpline(lane.Spline, Color.white, 1000);
                }
    }
}