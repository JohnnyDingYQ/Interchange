using System.Collections.Generic;
using Unity.Mathematics;

public static class RoadBuilder
{
    public static Road Build(float3 start, float3 pivot, float3 end, int laneCount)
    {
        global::Build.LaneCount = laneCount;
        global::Build.HandleBuildCommand(start);
        global::Build.HandleBuildCommand(pivot);
        return global::Build.HandleBuildCommand(end);
    }
}
