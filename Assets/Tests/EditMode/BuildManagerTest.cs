using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

public class BuildManagerTest
{
    float3 pos1 = new(10, 10, 10);
    float3 pos2 = new(30, 12, 30);
    float3 pos3 = new(60, 14, 60);
    float3 pos4 = new(90, 16, 90);
    float3 pos5 = new(90, 16, 90);
    float3 pos6 = new(120, 8, 120);
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
        BuildManager.Client = new MockClient(new List<float3>() { pos1, pos2, pos3 });
        for (int i = 0; i < 3; i++)
            BuildManager.HandleBuildCommand();
        Assert.AreEqual(1, BuildManager.RoadWatcher.Count);
        Road road = BuildManager.RoadWatcher.Values.First();
        Assert.IsNotNull(road.StartIx);
        Assert.IsNotNull(road.EndIx);
        Assert.IsNotNull(road.Spline);
        Assert.AreEqual(1, road.Lanes.Count);
        Assert.AreEqual(1, road.StartIx.NodeWithLane.Count);
        Assert.AreEqual(pos1, road.Lanes.First().StartPos);
        Assert.AreEqual(pos3, road.Lanes.First().EndPos);
    }

    [Test]
    public void BuildTwoLaneRoad()
    {
        BuildManager.Client = new MockClient(new List<float3>() { pos1, pos2, pos3 });
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
        BuildManager.Client = new MockClient(new List<float3>() { pos1, pos2, pos3, pos3, pos4, pos5 });
        BuildManager.LaneCount = 1;

        BuildManagerTestHelper.CheckTwoOneLaneRoadsConnection(pos1, pos3);
    }

    [Test]
    public void BuildingOnStartCreatesConnection_OneLane()
    {
        BuildManager.Client = new MockClient(new List<float3>() { pos3, pos4, pos5, pos1, pos2, pos3 });
        BuildManager.LaneCount = 1;

        BuildManagerTestHelper.CheckTwoOneLaneRoadsConnection(pos1, pos3);
    }

    [Test]
    public void SnappingCreatesIntersection_OneLane()
    {
        BuildManager.Client = new MockClient(
            new List<float3>() {
                pos1,
                pos2,
                pos3,
                pos3 + new float3(GlobalConstants.SnapDistance - 1, 0, 0),
                pos4,
                pos5
            }
        );
        BuildManagerTestHelper.CheckTwoOneLaneRoadsConnection(pos1, pos3);
    }

    [Test]
    public void OutOfSnapRangeDoesNotCreatesIntersection()
    {
        float3 exitingRoadStartNode = pos3 + new float3(GlobalConstants.SnapDistance + 1, 0, 0);
        BuildManager.Client = new MockClient(new List<float3>() { pos1, pos2, pos3, exitingRoadStartNode, pos4, pos5 });
        BuildManager.LaneCount = 1;
        for (int i = 0; i < 6; i++)
        {
            BuildManager.HandleBuildCommand();
        }

        Road roadA = FindRoadWithStartNode(30);
        Intersection intersection = roadA.EndIx;
        Assert.AreEqual(1, intersection.Roads.Count);
    }

    // TODO: Complete Further Testing
    // [Test]
    public void BuildingOnEndCreatesConnection_TwoLanes()
    {
        BuildManager.Client = new MockClient(new List<float3>() { pos1, pos2, pos3, pos3, pos4, pos5 });
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

        Assert.True(expectedLanes0.SetEquals(intersection.NodeWithLane[lane11.EndNode]));
        Assert.True(expectedLanes1.SetEquals(intersection.NodeWithLane[lane12.EndNode]));

    }

    // [Test]
    public void BuildingOnStartCreatesConnection_TwoLanes()
    {
        BuildManager.Client = new MockClient(
            new List<float3>() { pos3, pos4, pos5, pos1, pos2, pos3 });
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

        Assert.AreEqual(expectedLanes0, intersection.NodeWithLane[lane11.EndNode]);
        Assert.AreEqual(expectedLanes1, intersection.NodeWithLane[lane12.EndNode]);

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

    private class MockClient : IBuildManagerBoundary
    {
        readonly List<float3> MockPos;
        int count = 0;

        public MockClient(List<float3> mockCoord)
        {
            MockPos = mockCoord;
        }

        public void EvaluateIntersection(Intersection intersection)
        {
            return;
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
}