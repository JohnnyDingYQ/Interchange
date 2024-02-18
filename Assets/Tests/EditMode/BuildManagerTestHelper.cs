using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

public static class BuildManagerTestHelper
{
    public static void CheckTwoOneLaneRoadsConnection(int enteringRoadStartNode, int exitingRoadStartNode)
    {
        BuildManager.LaneCount = 1;
        for (int i = 0; i < 6; i++)
        {
            BuildManager.HandleBuildCommand();
        }

        Road roadA = FindRoadWithStartNode(enteringRoadStartNode);
        Road roadB = FindRoadWithStartNode(exitingRoadStartNode);
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

    static Road FindRoadWithStartNode(int start)
    {
        foreach (var (key, value) in BuildManager.RoadWatcher)
        {
            if (value.StartIx.GetNodes().Contains(start))
            {
                return value;
            }
        }
        return null;
    }
}