using System.Collections.Generic;
using Unity.Mathematics;

public static class RoadBuilder
{
    public static void BuildRoad(float3 start, float3 pivot, float3 end, int laneCount)
    {
        BuildHandler.LaneCount = laneCount;
        BuildHandler.HandleBuildCommand(start);
        BuildHandler.HandleBuildCommand(pivot);
        BuildHandler.HandleBuildCommand(end);
    }
}
