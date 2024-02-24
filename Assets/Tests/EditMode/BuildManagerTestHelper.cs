using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;

public static class BuildManagerTestHelper
{
    public static void CheckTwoOneLaneRoadsConnection(float3 enteringRoadStartPos, float3 exitingRoadStartPos)
    {
        BuildManager.LaneCount = 1;
        for (int i = 0; i < 6; i++)
        {
            BuildManager.HandleBuildCommand();
        }

        Road roadA = FindRoadWithStartPos(enteringRoadStartPos);
        Road roadB = FindRoadWithStartPos(exitingRoadStartPos);
        Intersection intersection = roadA.EndIx;
        HashSet<Lane> expectedLanes = new() { roadA.Lanes.First(), roadB.Lanes.First() };

        Assert.NotNull(roadA);
        Assert.NotNull(roadB);
        Assert.AreEqual(1, roadA.StartIx.Roads.Count);
        Assert.AreEqual(2, intersection.Roads.Count);
        Assert.AreSame(intersection, roadB.StartIx);
        Assert.AreEqual(2, intersection.NodeWithLane.Values.First().Count); // connecting node registers two lanes
        Assert.True(intersection.NodeWithLane.Values.First().SetEquals(expectedLanes));
    }

    static Road FindRoadWithStartPos(float3 startPos)
    {
        foreach (var (key, value) in BuildManager.RoadWatcher)
        {
            if (value.StartPos.Equals(startPos))
            {
                return value;
            }
        }
        return null;
    }
}