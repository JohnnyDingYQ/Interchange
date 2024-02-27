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
    Vector3 pos5 = new(120, 16, 120);
    Vector3 pos6 = new(150, 16, 150);
    Vector3 pos7 = new(180, 16, 180);
    SortedDictionary<int, HashSet<Lane>> NodeWithLane;
    SortedDictionary<int, Road> RoadWatcher;
    
    [SetUp]
    public void SetUp()
    {
        BuildManager.Reset();
        Game.WipeGameState();
        NodeWithLane = Game.NodeWithLane;
        RoadWatcher = Game.RoadWatcher;
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
        Assert.AreEqual(1, RoadWatcher.Count);
        Road road = RoadWatcher.Values.First();
        Assert.IsNotNull(road.Spline);
        Assert.AreEqual(1, road.Lanes.Count);
        Assert.AreEqual(pos1, road.Lanes[0].StartPos);
        Assert.AreEqual(pos3, road.Lanes[0].EndPos);
        Assert.True(NodeWithLane.ContainsKey(0));
        Assert.True(NodeWithLane.ContainsKey(1));
        Assert.AreSame(road.Lanes[0], NodeWithLane[0].First());
        Assert.AreSame(road.Lanes[0], NodeWithLane[1].First());
    }

    [Test]
    public void BuildTwoLaneRoad()
    {
        BuildManager.Client = new MockClient(new List<float3>() { pos1, pos2, pos3 });
        BuildManager.LaneCount = 2;
        for (int i = 0; i < 3; i++)
            BuildManager.HandleBuildCommand();

        Assert.AreEqual(1, RoadWatcher.Count);
        Road road = RoadWatcher.Values.First();
        Assert.IsNotNull(road.Spline);
        Assert.AreEqual(2, road.Lanes.Count);
        Assert.AreEqual(pos1, road.StartPos);
        Assert.AreEqual(pos3, road.EndPos);
        Assert.True(NodeWithLane.ContainsKey(0));
        Assert.True(NodeWithLane.ContainsKey(1));
        Assert.True(NodeWithLane.ContainsKey(2));
        Assert.True(NodeWithLane.ContainsKey(3));
        Assert.AreSame(road.Lanes[0], NodeWithLane[0].First());
        Assert.AreSame(road.Lanes[0], NodeWithLane[1].First());
        Assert.AreSame(road.Lanes[1], NodeWithLane[2].First());
        Assert.AreSame(road.Lanes[1], NodeWithLane[3].First());
    }

    [Test]
    public void ConnectionAtEnd_OneLane()
    {
        BuildManager.Client = new MockClient(new List<float3>() { pos1, pos2, pos3, pos3, pos4, pos5 });
        BuildManager.LaneCount = 1;

        CheckTwoOneLaneRoadsConnection(pos1, pos3);
    }

    [Test]
    public void ConnectionAtStart_OneLane()
    {
        BuildManager.Client = new MockClient(new List<float3>() { pos3, pos4, pos5, pos1, pos2, pos3 });
        BuildManager.LaneCount = 1;

        CheckTwoOneLaneRoadsConnection(pos1, pos3);
    }

    [Test]
    public void SnapAtEnd_OneLane()
    {
        BuildManager.Client = new MockClient(
            new List<float3>() {
                pos1,
                pos2,
                pos3,
                pos3 + new Vector3(GlobalConstants.SnapTolerance - 1, 0, 0),
                pos4,
                pos5
            }
        );
        CheckTwoOneLaneRoadsConnection(pos1, pos3);
    }

    [Test]
    public void OutOfSnapRangeDoesNotCreatesIntersection()
    {
        float3 exitingRoadStartPos = pos3 + new Vector3(GlobalConstants.SnapTolerance + GlobalConstants.LaneWidth + 1, 0, 0);
        BuildManager.Client = new MockClient(new List<float3>() { pos1, pos2, pos3, exitingRoadStartPos, pos4, pos5 });
        BuildManager.LaneCount = 1;
        for (int i = 0; i < 6; i++)
        {
            BuildManager.HandleBuildCommand();
        }

        Road enteringRoad = FindRoadWithStartPos(pos1);
        Road exitingRoad = FindRoadWithStartPos(exitingRoadStartPos);
        Assert.AreEqual(1, NodeWithLane[enteringRoad.Lanes[0].EndNode].Count);
        Assert.AreEqual(1, NodeWithLane[exitingRoad.Lanes[0].StartNode].Count);
    }

    [Test]
    public void ConnectionAtEnd_TwoLanes()
    {
        BuildManager.Client = new MockClient(new List<float3>() { pos1, pos2, pos3, pos3, pos4, pos5 });
        BuildManager.LaneCount = 2;
        for (int i = 0; i < 6; i++)
        {
            BuildManager.HandleBuildCommand();
        }

        Road road1 = RoadWatcher[0];
        Road road2 = RoadWatcher[1];
        Lane lane11 = road1.Lanes[0];
        Lane lane12 = road1.Lanes[1];
        Lane lane21 = road2.Lanes[0];
        Lane lane22 = road2.Lanes[1];
        HashSet<Lane> expectedLanes0 = new() { lane11, lane21 };
        HashSet<Lane> expectedLanes1 = new() { lane12, lane22 };
        Assert.AreEqual(6, NodeWithLane.Count);
        Assert.AreEqual(2, NodeWithLane[lane11.EndNode].Count);
        Assert.True(NodeWithLane[lane11.EndNode].SetEquals(expectedLanes0));
        Assert.True(NodeWithLane[lane12.EndNode].SetEquals(expectedLanes1));
    }

    [Test]
    public void ConnectionAtStart_TwoLanes()
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
        Assert.AreEqual(6, NodeWithLane.Count);
        Assert.True(NodeWithLane[lane11.EndNode].SetEquals(expectedLanes0));
        Assert.True(NodeWithLane[lane12.EndNode].SetEquals(expectedLanes1));
    }

    [Test]
    public void ConnectionAtEnd_ThreeLanes()
    {
        BuildManager.Client = new MockClient(new List<float3>() { pos1, pos2, pos3, pos3, pos4, pos5 });
        BuildManager.LaneCount = 3;
        for (int i = 0; i < 6; i++)
        {
            BuildManager.HandleBuildCommand();
        }

        Road road1 = RoadWatcher[0];
        Road road2 = RoadWatcher[1];
        Lane lane11 = road1.Lanes[0];
        Lane lane12 = road1.Lanes[1];
        Lane lane13 = road1.Lanes[2];
        Lane lane21 = road2.Lanes[0];
        Lane lane22 = road2.Lanes[1];
        Lane lane23 = road2.Lanes[2];
        HashSet<Lane> expectedLanes0 = new() { lane11, lane21 };
        HashSet<Lane> expectedLanes1 = new() { lane12, lane22 };
        HashSet<Lane> expectedLanes2 = new() { lane13, lane23 };
        Assert.AreEqual(9, NodeWithLane.Count);
        Assert.True(NodeWithLane[lane11.EndNode].SetEquals(expectedLanes0));
        Assert.True(NodeWithLane[lane12.EndNode].SetEquals(expectedLanes1));
        Assert.True(NodeWithLane[lane13.EndNode].SetEquals(expectedLanes2));
    }

    [Test]
    public void ConnectionAtStart_ThreeLanes()
    {
        BuildManager.Client = new MockClient(
            new List<float3>() { pos3, pos4, pos5, pos1, pos2, pos3 });
        BuildManager.LaneCount = 3;
        for (int i = 0; i < 6; i++)
        {
            BuildManager.HandleBuildCommand();
        }

        Road road1 = FindRoadWithStartPos(pos1);
        Road road2 = FindRoadWithStartPos(pos3);
        Lane lane11 = road1.Lanes[0];
        Lane lane12 = road1.Lanes[1];
        Lane lane13 = road1.Lanes[2];
        Lane lane21 = road2.Lanes[0];
        Lane lane22 = road2.Lanes[1];
        Lane lane23 = road2.Lanes[2];
        HashSet<Lane> expectedLanes0 = new() { lane11, lane21 };
        HashSet<Lane> expectedLanes1 = new() { lane12, lane22 };
        HashSet<Lane> expectedLanes2 = new() { lane13, lane23 };
        Assert.AreEqual(9, NodeWithLane.Count);
        Assert.True(NodeWithLane[lane11.EndNode].SetEquals(expectedLanes0));
        Assert.True(NodeWithLane[lane12.EndNode].SetEquals(expectedLanes1));
        Assert.True(NodeWithLane[lane13.EndNode].SetEquals(expectedLanes2));
    }

    [Test]
    public void ConnectionAtBothSides_OneLane()
    {
        BuildManager.Client = new MockClient(
            new List<float3>() {  pos1, pos2, pos3, pos5, pos6, pos7, pos3, pos4, pos5,  });
        BuildManager.LaneCount = 1;
        for (int i = 0; i < 9; i++)
        {
            BuildManager.HandleBuildCommand();
        }
        Road road1 = FindRoadWithStartPos(pos1);
        Road road2 = FindRoadWithStartPos(pos3);
        Road road3 = FindRoadWithStartPos(pos5);
        Lane lane1 = road1.Lanes[0];
        Lane lane2 = road2.Lanes[0];
        Lane lane3 = road3.Lanes[0];
        HashSet<Lane> expectedLanes0 = new() { lane1, lane2 };
        HashSet<Lane> expectedLanes1 = new() { lane2, lane3 };
        Assert.True(NodeWithLane[lane1.EndNode].SetEquals(expectedLanes0));
        Assert.True(NodeWithLane[lane2.EndNode].SetEquals(expectedLanes1));
        Assert.AreEqual(4, NodeWithLane.Count);
    }

    # region Helpers
    public void CheckTwoOneLaneRoadsConnection(float3 enteringRoadStartPos, float3 exitingRoadStartPos)
    {
        BuildManager.LaneCount = 1;
        for (int i = 0; i < 6; i++)
        {
            BuildManager.HandleBuildCommand();
        }

        Road enteringRoad = FindRoadWithStartPos(enteringRoadStartPos);
        Road exitingRoad = FindRoadWithStartPos(exitingRoadStartPos);
        Assert.NotNull(enteringRoad);
        Assert.NotNull(exitingRoad);
        HashSet<Lane> expectedLanes = new() { enteringRoad.Lanes.First(), exitingRoad.Lanes.First() };
  
        Assert.True(NodeWithLane[enteringRoad.Lanes[0].EndNode].SetEquals(expectedLanes));
    }

    Road FindRoadWithStartPos(float3 startPos)
    {
        foreach (var (key, value) in RoadWatcher)
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
    #endregion
}