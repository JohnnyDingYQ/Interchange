using System.Collections.Generic;
using Unity.Mathematics;

public static class RoadBuilder
{
    public static void BuildRoad(float3 start, float3 pivot, float3 end, int LaneCount)
    {
        MockDataInputImpl c = new(new List<float3> { start, pivot, end });
        BuildHandler.dataInputBoundary = c;
        BuildHandler.LaneCount = LaneCount;
        for (int i = 0; i < 3; i++)
        {
            BuildHandler.HandleBuildCommand();
        }

    }

    private class MockDataInputImpl : IDataInputBoundary
    {
        private readonly List<float3> posList;
        private int index;
        public MockDataInputImpl(List<float3> pos)
        {
            posList = pos;
            index = 0;
        }
        public float3 GetCursorPos()
        {
            return posList[index++];
        }
    }
}
