using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine.Assertions;

public static class RoadBuilder
{
    public static Road Single(float3 start, float3 pivot, float3 end, int laneCount)
    {
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

    public static List<Road> Many(float3 start, float3 pivot, float3 end, int laneCount)
    {
        Build.LaneCount = laneCount;
        Build.HandleBuildCommand(start);
        Build.HandleBuildCommand(pivot);
        List<Road> roads = Build.HandleBuildCommand(end);
        return roads;
    }
}
