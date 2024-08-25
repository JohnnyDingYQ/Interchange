using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine.Assertions;

public static class RoadBuilder
{
    public static Road Single(float3 start, float3 pivot, float3 end, int laneCount)
    {
        Build.ContinuousBuilding = false;
        Build.ParallelBuildOn = false;
        Build.ResetSelection();
        Build.LaneCount = laneCount;
        Build.HandleBuildCommand(start);
        Build.HandleBuildCommand(pivot);
        List<Road> roads = Build.HandleBuildCommand(end);
        if (roads == null)
            return null;
        Assert.AreEqual(1, roads.Count);
        return roads.First();
    }

    public static List<Road> Parallel(float3 start, float3 pivot, float3 end, int laneCount)
    {
        Build.ContinuousBuilding = false;
        Build.ParallelBuildOn = true;
        Build.ResetSelection();
        Build.LaneCount = laneCount;
        Build.HandleBuildCommand(start);
        Build.HandleBuildCommand(pivot);
        List<Road> roads = Build.HandleBuildCommand(end);
        return roads;
    }

    public static Road ZoneToZone(float3 start, float3 pivot, float3 end, Zone source, Zone target, int laneCount = 1)
    {
        Build.ContinuousBuilding = false;
        Build.LaneCount = laneCount;
        Game.HoveredZone = source;
        Build.HandleBuildCommand(start);
        Build.HandleBuildCommand(pivot);
        Game.HoveredZone = target;
        Road road = Build.HandleBuildCommand(end).Single();
        Game.HoveredZone = null;
        return road;
    }
}
