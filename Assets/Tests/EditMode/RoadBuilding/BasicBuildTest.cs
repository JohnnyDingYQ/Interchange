using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class BasicBuildTest
{
    float3 stride = Constants.MinimumLaneLength * new float3(1, 0, 1);
    SortedDictionary<int, Node> Nodes;
    SortedDictionary<int, Road> Roads;

    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
        Nodes = Game.Nodes;
        Roads = Game.Roads;
    }

    [Test]
    public void ResetSuccessful()
    {
        Assert.AreEqual(1, Build.LaneCount);
    }

    [Test]
    public void BuildOneLaneRoad()
    {
        Road road = RoadBuilder.B(0, stride, 2 * stride, 1);
        Lane lane = road.Lanes[0];

        Assert.AreEqual(1, Roads.Count);
        Assert.AreEqual(1, road.Lanes.Count);
        Assert.True(Utility.AreNumericallyEqual(0, lane.StartNode.Pos));
        Assert.True(Utility.AreNumericallyEqual(2 * stride, lane.EndNode.Pos));
        Assert.True(Nodes.ContainsKey(lane.StartNode.Id));
        Assert.True(Nodes.ContainsKey(lane.EndNode.Id));
        Assert.True(lane.StartNode.Lanes.SetEquals(new HashSet<Lane>() { lane }));
        Assert.True(lane.EndNode.Lanes.SetEquals(new HashSet<Lane>() { lane }));
        Assert.AreSame(lane.StartNode, road.StartIntersection.Nodes.Single());
        Assert.AreSame(lane.EndNode, road.EndIntersection.Nodes.Single());
        Assert.AreSame(road, road.StartIntersection.OutRoads.Single());
        Assert.AreSame(road, road.EndIntersection.InRoads.Single());
        Assert.AreSame(road.StartIntersection, lane.StartNode.Intersection);
        Assert.AreSame(road.EndIntersection, lane.EndNode.Intersection);
    }

    [Test]
    public void BuildTwoLaneRoad()
    {
        Road road = RoadBuilder.B(0, stride, 2 * stride, 2);
        Lane lane0 = road.Lanes[0];
        Lane lane1 = road.Lanes[1];

        Assert.AreEqual(1, Roads.Count);
        Assert.AreEqual(2, road.Lanes.Count);
        Assert.AreEqual(new float3(0), road.StartPos);
        Assert.AreEqual(2 * stride, road.EndPos);
        Assert.True(Nodes.ContainsKey(lane0.StartNode.Id));
        Assert.True(Nodes.ContainsKey(lane0.EndNode.Id));
        Assert.True(Nodes.ContainsKey(lane1.StartNode.Id));
        Assert.True(Nodes.ContainsKey(lane1.EndNode.Id));
        Assert.True(lane0.StartNode.Lanes.SetEquals(new HashSet<Lane>() { lane0 }));
        Assert.True(lane0.EndNode.Lanes.SetEquals(new HashSet<Lane>() { lane0 }));
        Assert.True(lane1.StartNode.Lanes.SetEquals(new HashSet<Lane>() { lane1 }));
        Assert.True(lane1.EndNode.Lanes.SetEquals(new HashSet<Lane>() { lane1 }));
        Assert.True(road.StartIntersection.Nodes.SequenceEqual(road.GetNodes(Side.Start)));
        Assert.True(road.EndIntersection.Nodes.SequenceEqual(road.GetNodes(Side.End)));
        Assert.AreSame(road, road.StartIntersection.OutRoads.Single());
        Assert.AreSame(road, road.EndIntersection.InRoads.Single());
    }

    [Test]
    public void ConnectionAtEnd_OneLane()
    {
        Road road0 = RoadBuilder.B(0, stride, 2 * stride, 1);
        Road road1 = RoadBuilder.B(2 * stride, 3 * stride, 4 * stride, 1);

        CheckLanesConnection(road0, road1, 1);
    }

    [Test]
    public void ConnectionAtStart_OneLane()
    {
        Road road0 = RoadBuilder.B(2 * stride, 3 * stride, 4 * stride, 1);
        Road road1 = RoadBuilder.B(0, stride, 2 * stride, 1);

        CheckLanesConnection(road1, road0, 1);
    }

    [Test]
    public void SnapAtEnd_OneLane()
    {
        Road road0 = RoadBuilder.B(0, stride, 2 * stride, 1);
        Road road1 = RoadBuilder.B(2 * stride + new float3(1, 0, 0) * (Constants.BuildSnapTolerance - 1), 3 * stride, 4 * stride, 1);

        CheckLanesConnection(road0, road1, 1);
    }

    [Test]
    public void OutOfSnapRangeDoesNotCreatesIntersection()
    {
        float3 exitingRoadStartPos = 2 * stride + new float3(Constants.BuildSnapTolerance + Constants.LaneWidth + 1, 0, 0);
        Road enteringRoad = RoadBuilder.B(0, stride, 2 * stride, 1);
        Road exitingRoad = RoadBuilder.B(exitingRoadStartPos, 3 * stride, 4 * stride, 1);

        Assert.AreEqual(1, enteringRoad.Lanes[0].EndNode.Lanes.Count);
        Assert.AreEqual(1, exitingRoad.Lanes[0].StartNode.Lanes.Count);
    }

    [Test]
    public void ConnectionAtEnd_TwoLanes()
    {
        Road road0 = RoadBuilder.B(0, stride, 2 * stride, 2);
        Road road1 = RoadBuilder.B(2 * stride, 3 * stride, 4 * stride, 2);

        CheckLanesConnection(road0, road1, 2);
    }

    [Test]
    public void ConnectionAtStart_TwoLanes()
    {
        Road road0 = RoadBuilder.B(2 * stride, 3 * stride, 4 * stride, 2);
        Road road1 = RoadBuilder.B(0, stride, 2 * stride, 2);

        CheckLanesConnection(road1, road0, 2);
    }

    [Test]
    public void ConnectionAtEnd_ThreeLanes()
    {
        Road road0 = RoadBuilder.B(0, stride, 2 * stride, 3);
        Road road1 = RoadBuilder.B(2 * stride, 3 * stride, 4 * stride, 3);

        CheckLanesConnection(road0, road1, 3);
    }

    [Test]
    public void ConnectionAtStart_ThreeLanes()
    {
        Road road0 = RoadBuilder.B(2 * stride, 3 * stride, 4 * stride, 3);
        Road road1 = RoadBuilder.B(0, stride, 2 * stride, 3);

        CheckLanesConnection(road1, road0, 3);
    }

    [Test]
    public void ConnectionAtBothSides_OneLane()
    {
        Road road1 = RoadBuilder.B(0, stride, 2 * stride, 1);
        Road road2 = RoadBuilder.B(2 * stride, 3 * stride, 4 * stride, 1);
        Road road3 = RoadBuilder.B(4 * stride, 5 * stride, 6 * stride, 1);
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
        Assert.AreEqual(0, Game.Graph.EdgeCount);
        RoadBuilder.B(
            new(0, 0, 0),
            new(0, 0, Constants.MinimumLaneLength / 2),
            new(0, 0, Constants.MinimumLaneLength - 0.01f),
            1
        );

        Assert.AreEqual(0, Game.Graph.EdgeCount);
        Assert.AreEqual(0, Roads.Count);
    }

    [Test]
    public void RoadTooBentShoudNotBuild()
    {
        RoadBuilder.B(0, stride, 0, 3);

        Assert.AreEqual(0, Roads.Count);
    }

    [Test]
    public void TwoRoadWithSameEnd()
    {
        RoadBuilder.B(0, stride, 2 * stride, 3);
        RoadBuilder.B(
            new float3(1, 0, 0) * Constants.MinimumLaneLength,
            new float3(1.3f, 0, 0.75f) * Constants.MinimumLaneLength,
            2 * stride,
            3
        );

        Assert.AreEqual(2, Roads.Count);
    }

    #region Helpers
    public void CheckLanesConnection(Road inRoad, Road outRoad, int laneCount)
    {
        Assert.NotNull(inRoad);
        Assert.NotNull(outRoad);
        Assert.AreEqual(laneCount * 3, Nodes.Count);
        Assert.AreSame(inRoad.EndIntersection, outRoad.StartIntersection);
        for (int i = 0; i < laneCount; i++)
        {
            Lane enteringLane = inRoad.Lanes[i];
            Lane exitingLane = outRoad.Lanes[i];
            HashSet<Lane> expectedLanes = new() { enteringLane, exitingLane };
            Assert.True(enteringLane.EndNode.Lanes.SetEquals(expectedLanes));
        }
        Assert.True(inRoad.EndIntersection.Nodes.SequenceEqual(inRoad.GetNodes(Side.End)));
        foreach (Node n in inRoad.GetNodes(Side.End))
            Assert.AreSame(n.Intersection, inRoad.EndIntersection);
    }
    #endregion
}