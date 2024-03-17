using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class BasicBuildTest
{
    float3 pos1 = new(0, 0, 0);
    float3 pos2 = Constants.MinimumLaneLength * new float3(1, 0, 1);
    float3 pos3 = Constants.MinimumLaneLength * 2 * new float3(1, 0, 1);
    float3 pos4 = Constants.MinimumLaneLength * 3 * new float3(1, 0, 1);
    float3 pos5 = Constants.MinimumLaneLength * 4 * new float3(1, 0, 1);
    float3 pos6 = Constants.MinimumLaneLength * 5 * new float3(1, 0, 1);
    float3 pos7 = Constants.MinimumLaneLength * 6 * new float3(1, 0, 1);
    SortedDictionary<int, Node> Nodes;
    SortedDictionary<int, Road> Roads;

    [SetUp]
    public void SetUp()
    {
        BuildHandler.Reset();
        Game.WipeState();
        Nodes = Game.Nodes;
        Roads = Game.Roads;
    }

    [Test]
    public void ResetSuccessful()
    {
        Assert.AreEqual(1, BuildHandler.LaneCount);
    }

    [Test]
    public void BuildOneLaneRoad()
    {
        Road road = RoadBuilder.BuildRoad(pos1, pos2, pos3, 1);
        Lane lane = road.Lanes[0];

        Assert.AreEqual(1, Roads.Count);
        Assert.IsNotNull(road.Curve);
        Assert.AreEqual(1, road.Lanes.Count);
        Assert.True(Utility.AreNumericallyEqual(pos1, lane.StartNode.Pos));
        Assert.True(Utility.AreNumericallyEqual(pos3, lane.EndNode.Pos));
        Assert.True(Nodes.ContainsKey(lane.StartNode.Id));
        Assert.True(Nodes.ContainsKey(lane.EndNode.Id));
        Assert.True(lane.StartNode.Lanes.SetEquals(new HashSet<Lane>() { lane }));
        Assert.True(lane.EndNode.Lanes.SetEquals(new HashSet<Lane>() { lane }));
    }

    [Test]
    public void BuildTwoLaneRoad()
    {
        Road road = RoadBuilder.BuildRoad(pos1, pos2, pos3, 2);
        Lane lane0 = road.Lanes[0];
        Lane lane1 = road.Lanes[1];

        Assert.AreEqual(1, Roads.Count);
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
        Road road0 = RoadBuilder.BuildRoad(pos1, pos2, pos3, 1);
        Road road1 = RoadBuilder.BuildRoad(pos3, pos4, pos5, 1);

        CheckLanesConnection(road0, road1, 1);
    }

    [Test]
    public void ConnectionAtStart_OneLane()
    {
        Road road0 = RoadBuilder.BuildRoad(pos3, pos4, pos5, 1);
        Road road1 = RoadBuilder.BuildRoad(pos1, pos2, pos3, 1);

        CheckLanesConnection(road1, road0, 1);
    }

    [Test]
    public void SnapAtEnd_OneLane()
    {
        Road road0 = RoadBuilder.BuildRoad(pos1, pos2, pos3, 1);
        Road road1 = RoadBuilder.BuildRoad(pos3 + new float3(1, 0, 0) * (Constants.BuildSnapTolerance - 1), pos4, pos5, 1);

        CheckLanesConnection(road0, road1, 1);
    }

    [Test]
    public void OutOfSnapRangeDoesNotCreatesIntersection()
    {
        float3 exitingRoadStartPos = pos3 + new float3(Constants.BuildSnapTolerance + Constants.LaneWidth + 1, 0, 0);
        Road enteringRoad = RoadBuilder.BuildRoad(pos1, pos2, pos3, 1);
        Road exitingRoad =  RoadBuilder.BuildRoad(exitingRoadStartPos, pos4, pos5, 1);
        
        Assert.AreEqual(1, enteringRoad.Lanes[0].EndNode.Lanes.Count);
        Assert.AreEqual(1, exitingRoad.Lanes[0].StartNode.Lanes.Count);
    }

    [Test]
    public void ConnectionAtEnd_TwoLanes()
    {
        Road road0 = RoadBuilder.BuildRoad(pos1, pos2, pos3, 2);
        Road road1 = RoadBuilder.BuildRoad(pos3, pos4, pos5, 2);

        CheckLanesConnection(road0, road1, 2);
    }

    [Test]
    public void ConnectionAtStart_TwoLanes()
    {
        Road road0 = RoadBuilder.BuildRoad(pos3, pos4, pos5, 2);
        Road road1 = RoadBuilder.BuildRoad(pos1, pos2, pos3, 2);

        CheckLanesConnection(road1, road0, 2);
    }

    [Test]
    public void ConnectionAtEnd_ThreeLanes()
    {
        Road road0 = RoadBuilder.BuildRoad(pos1, pos2, pos3, 3);
        Road road1 = RoadBuilder.BuildRoad(pos3, pos4, pos5, 3);

        CheckLanesConnection(road0, road1, 3);
    }

    [Test]
    public void ConnectionAtStart_ThreeLanes()
    {
        Road road0 = RoadBuilder.BuildRoad(pos3, pos4, pos5, 3);
        Road road1 = RoadBuilder.BuildRoad(pos1, pos2, pos3, 3);

        CheckLanesConnection(road1, road0, 3);
    }

    [Test]
    public void ConnectionAtBothSides_OneLane()
    {
        Road road1 = RoadBuilder.BuildRoad(pos1, pos2, pos3, 1);
        Road road2 = RoadBuilder.BuildRoad(pos3, pos4, pos5, 1);
        Road road3 = RoadBuilder.BuildRoad(pos5, pos6, pos7, 1);
        Lane lane1 = road1.Lanes[0];
        Lane lane2 = road2.Lanes[0];
        Lane lane3 = road3.Lanes[0];
        HashSet<Lane> expectedLanes0 = new() { lane1, lane2 };
        HashSet<Lane> expectedLanes1 = new() { lane2, lane3 };

        Assert.True(lane1.EndNode.Lanes.SetEquals(expectedLanes0));
        Assert.True(lane2.EndNode.Lanes.SetEquals(expectedLanes1));
        Assert.AreEqual(4, Nodes.Count);
    }


    [Test]
    public void LaneShorterThanMinimumLengthShouldNotBuild()
    {
        RoadBuilder.BuildRoad(
            new(0, 0, 0),
            new(0, 0, Constants.MinimumLaneLength / 2),
            new(0, 0, Constants.MinimumLaneLength - 0.01f),
            1
        );

        Assert.AreEqual(0, Roads.Count);
    }

    [Test]
    public void LaneLongerThanMaximumLengthShouldNotBuild()
    {
        RoadBuilder.BuildRoad(
            new(0, 0, 0),
            new(0, 0, Constants.MaximumLaneLength / 2),
            new(0, 0, Constants.MaximumLaneLength + 0.01f),
            1
        );
        Assert.AreEqual(0, Roads.Count);
    }

    #region Helpers
    public void CheckLanesConnection(Road enteringRoad, Road exitingRoad, int laneCount)
    {
        Assert.NotNull(enteringRoad);
        Assert.NotNull(exitingRoad);
        Assert.AreEqual(laneCount * 3, Nodes.Count);
        for (int i = 0; i < laneCount; i++)
        {
            Lane enteringLane = enteringRoad.Lanes[i];
            Lane exitingLane = exitingRoad.Lanes[i];
            HashSet<Lane> expectedLanes = new() { enteringLane, exitingLane };
            Assert.True(enteringLane.EndNode.Lanes.SetEquals(expectedLanes));
        }
    }
    #endregion
}