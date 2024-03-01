using System.Collections.Generic;
using Unity.Mathematics;

public static class RoadBuilder
{
    public static void BuildRoad(float3 start, float3 pivot, float3 end, int LaneCount)
    {
        MockBuildClient c = new(new List<float3> { start, pivot, end });
        BuildManager.Client = c;
        BuildManager.LaneCount = LaneCount;
        for (int i = 0; i < 3; i++)
        {
            BuildManager.HandleBuildCommand();
        }

    }
}

public class MockBuildClient : IBuildManagerBoundary
{
    List<float3> MockPos;
    int count;

    public MockBuildClient(List<float3> mockCoord)
    {
        MockPos = mockCoord;
        count = 0;
    }

    public float3 GetPos()
    {
        return MockPos[count++];
    }

    public void InstantiateRoad(Road road)
    {
        return;
    }

    public void RedrawAllRoads()
    {
        throw new System.NotImplementedException();
    }
}