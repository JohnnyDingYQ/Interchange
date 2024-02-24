using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class BuildManagerTest
{
    Vector3 pos1 = new(10, 10, 10);
    Vector3 pos2 = new(30, 12, 30);
    Vector3 pos3 = new(60, 14, 60);
    Vector3 pos4 = new(90, 16, 90);
    Vector3 pos5 = new(90, 16, 90);
    
    [SetUp]
    public void SetUp()
    {
        BuildManager.Reset();
        Game.WipeGameState();
    }

    [Test]
    public void ResetSuccessful()
    {
        Assert.IsNull(BuildManager.Client);
        Assert.AreEqual(1, BuildManager.LaneCount);
    }

    [Test]
    public void BuildOneLaneRoad()
    {
        BuildManager.Client = new MockClient(new List<float3>() { pos1, pos2, pos3 });
        for (int i = 0; i < 3; i++)
            BuildManager.HandleBuildCommand();
        Assert.AreEqual(1, Game.RoadWatcher.Count);
        Road road = Game.RoadWatcher.Values.First();
        Assert.IsNotNull(road.Spline);
        Assert.AreEqual(1, road.Lanes.Count);
        Assert.AreEqual(pos1, road.Lanes[0].StartPos);
        Assert.AreEqual(pos3, road.Lanes[0].EndPos);
        Assert.True(Game.NodeWithLane.ContainsKey(0));
        Assert.True(Game.NodeWithLane.ContainsKey(1));
        Assert.AreSame(road.Lanes[0], Game.NodeWithLane[0].First());
        Assert.AreSame(road.Lanes[0], Game.NodeWithLane[1].First());
    }

    [Test]
    public void BuildTwoLaneRoad()
    {
        BuildManager.Client = new MockClient(new List<float3>() { pos1, pos2, pos3 });
        BuildManager.LaneCount = 2;
        for (int i = 0; i < 3; i++)
            BuildManager.HandleBuildCommand();

        Assert.AreEqual(1, Game.RoadWatcher.Count);
        Road road = Game.RoadWatcher.Values.First();
        Assert.IsNotNull(road.Spline);
        Assert.AreEqual(2, road.Lanes.Count);
        Assert.AreEqual(pos1, road.StartPos);
        Assert.AreEqual(pos3, road.EndPos);
        Assert.True(Game.NodeWithLane.ContainsKey(0));
        Assert.True(Game.NodeWithLane.ContainsKey(1));
        Assert.True(Game.NodeWithLane.ContainsKey(2));
        Assert.True(Game.NodeWithLane.ContainsKey(3));
        Assert.AreSame(road.Lanes[0], Game.NodeWithLane[0].First());
        Assert.AreSame(road.Lanes[0], Game.NodeWithLane[1].First());
        Assert.AreSame(road.Lanes[1], Game.NodeWithLane[2].First());
        Assert.AreSame(road.Lanes[1], Game.NodeWithLane[3].First());
    }

    [Test]
    public void BuildingOnEndCreatesConnection_OneLane()
    {
        BuildManager.Client = new MockClient(new List<float3>() { pos1, pos2, pos3, pos3, pos4, pos5 });
        BuildManager.LaneCount = 1;

        CheckTwoOneLaneRoadsConnection(pos1, pos3);
    }

    [Test]
    public void BuildingOnStartCreatesConnection_OneLane()
    {
        BuildManager.Client = new MockClient(new List<float3>() { pos3, pos4, pos5, pos1, pos2, pos3 });
        BuildManager.LaneCount = 1;

        CheckTwoOneLaneRoadsConnection(pos1, pos3);
    }

    [Test]
    public void SnappingCreatesIntersection_OneLane()
    {
        BuildManager.Client = new MockClient(
            new List<float3>() {
                pos1,
                pos2,
                pos3,
                pos3 + new Vector3(GlobalConstants.SnapDistance - 1, 0, 0),
                pos4,
                pos5
            }
        );
        CheckTwoOneLaneRoadsConnection(pos1, pos3);
    }

    [Test]
    public void OutOfSnapRangeDoesNotCreatesIntersection()
    {
        float3 exitingRoadStartPos = pos3 + new Vector3(GlobalConstants.SnapDistance + 1, 0, 0);
        BuildManager.Client = new MockClient(new List<float3>() { pos1, pos2, pos3, exitingRoadStartPos, pos4, pos5 });
        BuildManager.LaneCount = 1;
        for (int i = 0; i < 6; i++)
        {
            BuildManager.HandleBuildCommand();
        }

        Road enteringRoad = FindRoadWithStartPos(pos1);
        Road exitingRoad = FindRoadWithStartPos(exitingRoadStartPos);
        Assert.AreEqual(1, Game.NodeWithLane[enteringRoad.Lanes[0].EndNode].Count);
        Assert.AreEqual(1, Game.NodeWithLane[exitingRoad.Lanes[0].StartNode].Count);
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

        Road road1 = Game.RoadWatcher[0];
        Road road2 = Game.RoadWatcher[1];
        Lane lane11 = road1.Lanes.First();
        Lane lane12 = road1.Lanes.Last();
        Lane lane21 = road2.Lanes.First();
        Lane lane22 = road2.Lanes.Last();
        HashSet<Lane> expectedLanes0 = new() { lane11, lane21 };
        HashSet<Lane> expectedLanes1 = new() { lane12, lane22 };

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

        Road road1 = FindRoadWithStartPos(pos1);
        Road road2 = FindRoadWithStartPos(pos3);
        Lane lane11 = road1.Lanes.First();
        Lane lane12 = road1.Lanes.Last();
        Lane lane21 = road2.Lanes.First();
        Lane lane22 = road2.Lanes.Last();
        HashSet<Lane> expectedLanes0 = new() { lane11, lane21 };
        HashSet<Lane> expectedLanes1 = new() { lane12, lane22 };

    }

    public static void CheckTwoOneLaneRoadsConnection(float3 enteringRoadStartPos, float3 exitingRoadStartPos)
    {
        BuildManager.LaneCount = 1;
        for (int i = 0; i < 6; i++)
        {
            BuildManager.HandleBuildCommand();
        }

        Road enteringRoad = FindRoadWithStartPos(enteringRoadStartPos);
        Road exitingRoad = FindRoadWithStartPos(exitingRoadStartPos);
        HashSet<Lane> expectedLanes = new() { enteringRoad.Lanes.First(), exitingRoad.Lanes.First() };

        Assert.NotNull(enteringRoad);
        Assert.NotNull(exitingRoad);
        Assert.True(Game.NodeWithLane[enteringRoad.Lanes[0].EndNode].SetEquals(expectedLanes));
    }

    static Road FindRoadWithStartPos(float3 startPos)
    {
        foreach (var (key, value) in Game.RoadWatcher)
        {
            if (value.StartPos.Equals(startPos))
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