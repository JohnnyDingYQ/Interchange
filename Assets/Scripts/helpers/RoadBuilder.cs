using System.Collections.Generic;
using Unity.Mathematics;

public static class RoadBuilder
{
    public static Road B(float3 start, float3 pivot, float3 end, int laneCount)
    {
        Build.LaneCount = laneCount;
        Build.HandleBuildCommand(start);
        Build.HandleBuildCommand(pivot);
        return Build.HandleBuildCommand(end);
    }
}
