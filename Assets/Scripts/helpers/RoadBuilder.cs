using System.Collections.Generic;
using Unity.Mathematics;

public static class RoadBuilder
{
    public static Road BuildRoad(float3 start, float3 pivot, float3 end, int laneCount)
    {
        BuildHandler.LaneCount = laneCount;
        BuildHandler.HandleBuildCommand(start);
        BuildHandler.HandleBuildCommand(pivot);
        return BuildHandler.HandleBuildCommand(end);
    }
}
