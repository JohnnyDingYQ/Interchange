using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor.VersionControl;
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
    SortedDictionary<int, Node> Nodes;
    SortedDictionary<int, Road> Roads;

    [SetUp]
    public void SetUp()
    {
        BuildManager.Reset();
        Game.WipeGameState();
        Nodes = Game.Nodes;
        Roads = Game.Roads;
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
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 1);

        Assert.AreEqual(1, Roads.Count);
        Road road = Roads.Values.First();
        Lane lane = road.Lanes[0];
        Assert.IsNotNull(road.Curve);
        Assert.AreEqual(1, road.Lanes.Count);
        Assert.AreEqual(pos1, lane.StartNode.Pos);
        Assert.AreEqual(pos3, lane.EndNode.Pos);
        Assert.True(Nodes.ContainsKey(lane.StartNode.Id));
        Assert.True(Nodes.ContainsKey(lane.EndNode.Id));
        Assert.True(lane.StartNode.Lanes.SetEquals(new HashSet<Lane>() { lane }));
        Assert.True(lane.EndNode.Lanes.SetEquals(new HashSet<Lane>() { lane }));
    }

    [Test]
    public void BuildTwoLaneRoad()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 2);

        Assert.AreEqual(1, Roads.Count);
        Road road = Roads.Values.First();
        Lane lane0 = road.Lanes[0];
        Lane lane1 = road.Lanes[1];

        Assert.IsNotNull(road.Curve);
        Assert.AreEqual(2, road.Lanes.Count);
        Assert.AreEqual(pos1, road.StartPos);
        Assert.AreEqual(pos3, road.EndPos);
        Assert.True(Nodes.ContainsKey(lane0.StartNode.Id));
        Assert.True(Nodes.ContainsKey(lane0.EndNode.Id));
        Assert.True(Nodes.ContainsKey(lane1.StartNode.Id));
        Assert.True(Nodes.ContainsKey(lane1.EndNode.Id));
        Assert.True(lane0.StartNode.Lanes.SetEquals(new HashSet<Lane>() { lane0 }));
        Assert.True(lane0.EndNode.Lanes.SetEquals(new HashSet<Lane>() { lane0 }));
        Assert.True(lane1.StartNode.Lanes.SetEquals(new HashSet<Lane>() { lane1 }));
        Assert.True(lane1.EndNode.Lanes.SetEquals(new HashSet<Lane>() { lane1 }));
    }

    [Test]
    public void ConnectionAtEnd_OneLane()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 1);
        RoadBuilder.BuildRoad(pos3, pos4, pos5, 1);

        CheckTwoOneLaneRoadsConnection(pos1, pos3);
    }

    [Test]
    public void ConnectionAtStart_OneLane()
    {
        RoadBuilder.BuildRoad(pos3, pos4, pos5, 1);
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 1);

        CheckTwoOneLaneRoadsConnection(pos1, pos3);
    }

    [Test]
    public void SnapAtEnd_OneLane()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 1);
        RoadBuilder.BuildRoad(pos3 + Vector3.right * (GlobalConstants.SnapTolerance - 1), pos4, pos5, 1);

        CheckTwoOneLaneRoadsConnection(pos1, pos3);
    }

    [Test]
    public void OutOfSnapRangeDoesNotCreatesIntersection()
    {
        float3 exitingRoadStartPos = pos3 + new Vector3(GlobalConstants.SnapTolerance + GlobalConstants.LaneWidth + 1, 0, 0);
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 1);
        RoadBuilder.BuildRoad(exitingRoadStartPos, pos4, pos5, 1);

        Road enteringRoad = Utility.FindRoadWithStartPos(pos1);
        Road exitingRoad = Utility.FindRoadWithStartPos(exitingRoadStartPos);
        Assert.AreEqual(1, enteringRoad.Lanes[0].EndNode.Lanes.Count);
        Assert.AreEqual(1, exitingRoad.Lanes[0].StartNode.Lanes.Count);
    }

    [Test]
    public void ConnectionAtEnd_TwoLanes()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 2);
        RoadBuilder.BuildRoad(pos3, pos4, pos5, 2);

        CheckTwoTwoLaneRoadsConnection(pos1, pos3);
    }

    [Test]
    public void ConnectionAtStart_TwoLanes()
    {
        RoadBuilder.BuildRoad(pos3, pos4, pos5, 2);
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 2);

        CheckTwoTwoLaneRoadsConnection(pos1, pos3);
    }

    [Test]
    public void ConnectionAtEnd_ThreeLanes()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 3);
        RoadBuilder.BuildRoad(pos3, pos4, pos5, 3);

        CheckTwoThreeLaneRoadsConnection(pos1, pos3);
    }

    [Test]
    public void ConnectionAtStart_ThreeLanes()
    {
        RoadBuilder.BuildRoad(pos3, pos4, pos5, 3);
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 3);

        CheckTwoThreeLaneRoadsConnection(pos1, pos3);
    }

    [Test]
    public void ConnectionAtBothSides_OneLane()
    {
        BuildManager.Client = new MockBuildClient(
            new List<float3>() { pos1, pos2, pos3, pos5, pos6, pos7, pos3, pos4, pos5, });
        BuildManager.LaneCount = 1;
        for (int i = 0; i < 9; i++)
        {
            BuildManager.HandleBuildCommand();
        }
        Road road1 = Utility.FindRoadWithStartPos(pos1);
        Road road2 = Utility.FindRoadWithStartPos(pos3);
        Road road3 = Utility.FindRoadWithStartPos(pos5);
        Lane lane1 = road1.Lanes[0];
        Lane lane2 = road2.Lanes[0];
        Lane lane3 = road3.Lanes[0];
        HashSet<Lane> expectedLanes0 = new() { lane1, lane2 };
        HashSet<Lane> expectedLanes1 = new() { lane2, lane3 };
        Assert.True(lane1.EndNode.Lanes.SetEquals(expectedLanes0));
        Assert.True(lane2.EndNode.Lanes.SetEquals(expectedLanes1));
        Assert.AreEqual(4, Nodes.Count);
    }

    # region Helpers
    public void CheckTwoOneLaneRoadsConnection(float3 enteringRoadStartPos, float3 exitingRoadStartPos)
    {
        Road enteringRoad = Utility.FindRoadWithStartPos(enteringRoadStartPos);
        Road exitingRoad = Utility.FindRoadWithStartPos(exitingRoadStartPos);
        Assert.NotNull(enteringRoad);
        Assert.NotNull(exitingRoad);
        HashSet<Lane> expectedLanes = new() { enteringRoad.Lanes.First(), exitingRoad.Lanes.First() };

        Assert.True(enteringRoad.Lanes[0].EndNode.Lanes.SetEquals(expectedLanes));
    }

    public void CheckTwoTwoLaneRoadsConnection(float3 enteringRoadStartPos, float3 exitingRoadStartPos)
    {
        Road road1 = Utility.FindRoadWithStartPos(enteringRoadStartPos);
        Road road2 = Utility.FindRoadWithStartPos(exitingRoadStartPos);
        Lane lane11 = road1.Lanes[0];
        Lane lane12 = road1.Lanes[1];
        Lane lane21 = road2.Lanes[0];
        Lane lane22 = road2.Lanes[1];
        HashSet<Lane> expectedLanes0 = new() { lane11, lane21 };
        HashSet<Lane> expectedLanes1 = new() { lane12, lane22 };
        Assert.AreEqual(6, Nodes.Count);
        Assert.AreEqual(2, lane11.EndNode.Lanes.Count);
        Assert.True(lane11.EndNode.Lanes.SetEquals(expectedLanes0));
        Assert.True(lane12.EndNode.Lanes.SetEquals(expectedLanes1));
    }

    public void CheckTwoThreeLaneRoadsConnection(float3 enteringRoadStartPos, float3 exitingRoadStartPos)
    {
        Road road1 = Utility.FindRoadWithStartPos(enteringRoadStartPos);
        Road road2 = Utility.FindRoadWithStartPos(exitingRoadStartPos);
        Lane lane11 = road1.Lanes[0];
        Lane lane12 = road1.Lanes[1];
        Lane lane13 = road1.Lanes[2];
        Lane lane21 = road2.Lanes[0];
        Lane lane22 = road2.Lanes[1];
        Lane lane23 = road2.Lanes[2];
        HashSet<Lane> expectedLanes0 = new() { lane11, lane21 };
        HashSet<Lane> expectedLanes1 = new() { lane12, lane22 };
        HashSet<Lane> expectedLanes2 = new() { lane13, lane23 };
        Assert.AreEqual(9, Nodes.Count);
        Assert.True(lane11.EndNode.Lanes.SetEquals(expectedLanes0));
        Assert.True(lane12.EndNode.Lanes.SetEquals(expectedLanes1));
        Assert.True(lane13.EndNode.Lanes.SetEquals(expectedLanes2));
    }
    #endregion
}