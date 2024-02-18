using UnityEngine;
using NUnit.Framework;
using Unity.Mathematics;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

public class BuildManagerTest
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Grid.Height = 100;
        Grid.Width = 200;
        Grid.Dim = 1;
        Grid.Level = 0;
    }

    [SetUp]
    public void SetUp()
    {
        BuildManager.Reset();
    }

    [Test]
    public void ResetSuccessful()
    {
        Assert.AreEqual(0, BuildManager.RoadWatcher.Count);
        Assert.IsNull(BuildManager.Client);
        Assert.AreEqual(1, BuildManager.LaneCount);
    }

    [Test]
    public void BuildOneLaneRoad()
    {
        BuildManager.Client = new MockClient(new() { 30, 60, 90, });
        for (int i = 0; i < 3; i++)
            BuildManager.HandleBuildCommand();
        Assert.AreEqual(1, BuildManager.RoadWatcher.Count);
        Road road = BuildManager.RoadWatcher.Values.First();
        Assert.IsNotNull(road.StartIx);
        Assert.IsNotNull(road.EndIx);
        Assert.IsNotNull(road.Spline);
        Assert.AreEqual(1, road.Lanes.Count);
        Assert.AreEqual(1, road.StartIx.NodeWithLane.Count);
        Assert.AreEqual(30, road.Lanes.First().Start);
        Assert.AreEqual(90, road.Lanes.First().End);
    }

    [Test]
    public void BuildTwoLaneRoad()
    {
        BuildManager.Client = new MockClient(new() { 30, 60, 90 });
        BuildManager.LaneCount = 2;
        for (int i = 0; i < 3; i++)
            BuildManager.HandleBuildCommand();

        Assert.AreEqual(1, BuildManager.RoadWatcher.Count);
        Road road = BuildManager.RoadWatcher.Values.First();
        Assert.IsNotNull(road.StartIx);
        Assert.IsNotNull(road.EndIx);
        Assert.IsNotNull(road.Spline);
        Assert.AreEqual(2, road.Lanes.Count);
        Assert.AreEqual(2, road.StartIx.NodeWithLane.Count);
    }

    [Test]
    public void BuildingOnEndCreatesConnection_OneLane()
    {
        BuildManager.Client = new MockClient(new() { 30, 60, 90, 90, 120, 150 });
        BuildManager.LaneCount = 1;

        BuildManagerTestHelper.CheckTwoOneLaneRoadsConnection(30, 90);
    }

    [Test]
    public void BuildingOnStartCreatesConnection_OneLane()
    {
        BuildManager.Client = new MockClient(new() { 90, 120, 150, 30, 60, 90 });
        BuildManager.LaneCount = 1;

        BuildManagerTestHelper.CheckTwoOneLaneRoadsConnection(30, 90);
    }

    [Test]
    public void SnappingCreatesIntersection_OneLane()
    {
        BuildManager.Client = new MockClient(new() { 30, 60, 90, 90 + (int) (GlobalConstants.SnapDistance - 1), 120, 150});
        BuildManagerTestHelper.CheckTwoOneLaneRoadsConnection(30, 90);
    }

    [Test]
    public void OutOfSnapRangeDoesNotCreatesIntersection()
    {
        int exitingRoadStartNode = 90 + (int) (GlobalConstants.SnapDistance + 1);
        BuildManager.Client = new MockClient(new() { 30, 60, 90, exitingRoadStartNode, 120, 150});
        BuildManager.LaneCount = 1;
        for (int i = 0; i < 6; i++)
        {
            BuildManager.HandleBuildCommand();
        }

        Road roadA = FindRoadWithStartNode(30);
        Road roadB = FindRoadWithStartNode(exitingRoadStartNode);
        Intersection intersection = roadA.EndIx;
        Assert.AreEqual(1, intersection.Roads.Count);
    }

    // [Test]
    public void BuildingOnEndCreatesConnection_TwoLanes()
    {
        BuildManager.Client = new MockClient(new() { 30, 60, 90, 90, 120, 150 });
        BuildManager.LaneCount = 2;
        for (int i = 0; i < 6; i++)
        {
            BuildManager.HandleBuildCommand();
        }

        Road road1 = BuildManager.RoadWatcher[0];
        Road road2 = BuildManager.RoadWatcher[1];
        Lane lane11 = road1.Lanes.First();
        Lane lane12 = road1.Lanes.Last();
        Lane lane21 = road2.Lanes.First();
        Lane lane22 = road2.Lanes.Last();
        Intersection intersection = road1.EndIx;
        HashSet<Lane> expectedLanes0 = new() { lane11, lane21 };
        HashSet<Lane> expectedLanes1 = new() { lane12, lane22 };

        Assert.True(expectedLanes0.SetEquals(intersection.NodeWithLane[lane11.End]));
        Assert.True(expectedLanes1.SetEquals(intersection.NodeWithLane[lane12.End]));

    }

    // [Test]
    public void BuildingOnStartCreatesConnection_TwoLanes()
    {
        BuildManager.Client = new MockClient(
            new() { 90, 120, 150, 30, 60, 90 });
        BuildManager.LaneCount = 2;
        for (int i = 0; i < 6; i++)
        {
            BuildManager.HandleBuildCommand();
        }

        Road road1 = FindRoadWithStartNode(30);
        Road road2 = FindRoadWithStartNode(90);
        Lane lane11 = road1.Lanes.First();
        Lane lane12 = road1.Lanes.Last();
        Lane lane21 = road2.Lanes.First();
        Lane lane22 = road2.Lanes.Last();
        Intersection intersection = road1.EndIx;
        HashSet<Lane> expectedLanes0 = new() { lane11, lane21 };
        HashSet<Lane> expectedLanes1 = new() { lane12, lane22 };

        Assert.AreEqual(expectedLanes0, intersection.NodeWithLane[lane11.End]);
        Assert.AreEqual(expectedLanes1, intersection.NodeWithLane[lane12.End]);

    }

    Road FindRoadWithStartNode(int start)
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

    public class MockClient : IBuildManagerBoundary
    {
        readonly List<int> MockPos;
        int count = 0;

        public MockClient(List<int> mockPos)
        {
            MockPos = mockPos;
        }

        public void EvaluateIntersection(Intersection intersection)
        {
            return;
        }

        public float3 GetPos()
        {
            return Grid.GetPosByID(MockPos[count++]);
        }

        public void InstantiateRoad(Road road)
        {
            return;
        }
    }

}