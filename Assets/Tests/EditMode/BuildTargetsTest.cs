using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

public class BuildTargetTest
{
    float3 stride = Constants.MinLaneLength * new float3(1, 0, 0);
    Dictionary<uint, Road> Roads;

    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
        Roads = Game.Roads;
    }

    [Test]
    public void NoSnap()
    {
        BuildTargets bt1 = new(0, 1, Game.Nodes.Values);
        BuildTargets bt2 = new(0, 2, Game.Nodes.Values);
        Assert.False(bt1.Snapped);
        Assert.False(bt2.Snapped);
        Assert.AreEqual(new float3(0), bt1.ClickPos);
        Assert.AreEqual(new float3(0), bt2.ClickPos);
    }

    [Test]
    public void RepeatingOneLaneRoad_OnEnd()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
        BuildTargets bt = new(2 * stride, 1, Game.Nodes.Values);
        Node node = bt.Nodes[0];
        Lane lane = road.Lanes[0];

        Assert.True(bt.Snapped);
        Assert.True(MyNumerics.AreNumericallyEqual(2 * stride, bt.MedianPoint));
        Assert.AreSame(lane.EndNode, node);
    }

    [Test]
    public void RepeatingTwoLaneRoad_OnStart()
    {
        RoadBuilder.Single(0, stride, 2 * stride, 2);
        BuildTargets bt = new(0, 2, Game.Nodes.Values);
        Node node0 = bt.Nodes[0];
        Node node1 = bt.Nodes[1];
        Road road = Roads.Values.First();
        Lane lane0 = road.Lanes[0];
        Lane lane1 = road.Lanes[1];

        Assert.True(bt.Snapped);
        Assert.AreEqual(new float3(0), bt.MedianPoint);
        Assert.AreEqual(lane0.StartNode, node0);
        Assert.AreEqual(lane1.StartNode, node1);
    }

    [Test]
    public void AttachOneLaneToTwoLane_OnEnd()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 2);
        Lane lane = road.Lanes[0];
        BuildTargets bt = new(lane.EndPos + 0.9f * Constants.BuildSnapTolerance * new float3(1, 0, 0), 1, Game.Nodes.Values);
        Assert.AreEqual(1, bt.Nodes.Count);
        Node node = bt.Nodes[0];


        Assert.True(bt.Snapped);
        Assert.True(MyNumerics.AreNumericallyEqual(lane.EndPos, bt.MedianPoint));
        Assert.AreSame(lane.EndNode, node);
    }

    [Test]
    public void AttachTwoLaneToThreeLane_OnStart()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 3);
        Lane lane0 = road.Lanes[0];
        Lane lane1 = road.Lanes[1];
        float3 midPoint = Vector3.Lerp(lane0.StartPos, lane1.StartPos, 0.5f);
        BuildTargets bt = new(midPoint + 0.9f * Constants.BuildSnapTolerance * new float3(-1, 0, 0), 2, Game.Nodes.Values);
        Assert.AreEqual(2, bt.Nodes.Count);
        Node node0 = bt.Nodes[0];
        Node node1 = bt.Nodes[1];

        Assert.True(bt.Snapped);
        Assert.AreEqual(midPoint, bt.MedianPoint);
        Assert.AreEqual(lane0.StartNode, node0);
        Assert.AreEqual(lane1.StartNode, node1);
    }

    [Test]
    public void LaneExpansionOneLaneToTwoLane_Left()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Vector3 buildPoint = 2 * stride + 0.9f * Constants.BuildSnapTolerance * new float3(0, 0, 1);
        BuildTargets bt = new(buildPoint, 2, Game.Nodes.Values);
        Assert.AreEqual(2, bt.Nodes.Count);
        Node node0 = bt.Nodes[0];
        Node node1 = bt.Nodes[1];

        Assert.True(MyNumerics.AreNumericallyEqual(road.ExtrapolateNodePos(Side.End, -1), node0.Pos));
        Assert.True(MyNumerics.AreNumericallyEqual(2 * stride, node1.Pos));
    }

    [Test]
    public void LaneExpansionOneLaneToTwoLane_Right()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Vector3 buildPoint = 2 * stride + 0.9f * Constants.BuildSnapTolerance * new float3(0, 0, -1);
        BuildTargets bt = new(buildPoint, 2, Game.Nodes.Values);
        Assert.AreEqual(2, bt.Nodes.Count);
        Node node0 = bt.Nodes[0];
        Node node1 = bt.Nodes[1];

        Assert.True(MyNumerics.AreNumericallyEqual(road.ExtrapolateNodePos(Side.End, 1), node1.Pos));
        Assert.True(MyNumerics.AreNumericallyEqual(2 * stride, node0.Pos));
    }

    [Test]
    public void LaneExpansionTwoLaneToThreeLane_Left()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 2);
        Vector3 buildPoint = 2 * stride + 0.9f * Constants.BuildSnapTolerance * new float3(0, 0, 1);
        BuildTargets bt = new(buildPoint, 3, Game.Nodes.Values);
        Assert.AreEqual(3, bt.Nodes.Count);
        Node node0 = bt.Nodes[0];
        Node node1 = bt.Nodes[1];
        Node node2 = bt.Nodes[2];
        Lane lane0 = road.Lanes[0];
        Lane lane1 = road.Lanes[1];

        Assert.True(MyNumerics.AreNumericallyEqual(road.ExtrapolateNodePos(Side.End, -1), node0.Pos));
        Assert.AreSame(lane0.EndNode, node1);
        Assert.AreSame(lane1.EndNode, node2);
    }

    [Test]
    public void LaneExpansionTwoLaneToThreeLane_Right()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 2);
        Vector3 buildPoint = 2 * stride + 0.9f * Constants.BuildSnapTolerance * new float3(0, 0, -1);
        BuildTargets bt = new(buildPoint, 3, Game.Nodes.Values);
        Assert.AreEqual(3, bt.Nodes.Count);
        Node node0 = bt.Nodes[0];
        Node node1 = bt.Nodes[1];
        Node node2 = bt.Nodes[2];
        Lane lane0 = road.Lanes[0];
        Lane lane1 = road.Lanes[1];

        Assert.True(MyNumerics.AreNumericallyEqual(road.ExtrapolateNodePos(Side.End, 2), node2.Pos));
        Assert.AreEqual(lane0.EndPos, node0.Pos);
        Assert.AreEqual(lane1.EndPos, node1.Pos);
        Assert.AreSame(node0, lane0.EndNode);
    }

    [Test]
    public void LaneExpansionOneLaneToThreeLane_Mid()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
        BuildTargets bt = new(2 * stride, 3, Game.Nodes.Values);
        Assert.AreEqual(3, bt.Nodes.Count);
        Node node0 = bt.Nodes[0];
        Node node1 = bt.Nodes[1];
        Node node2 = bt.Nodes[2];

        Assert.True(MyNumerics.AreNumericallyEqual(road.ExtrapolateNodePos(Side.End, -1), node0.Pos));
        Assert.AreSame(road.Lanes[0].EndNode, node1);
        Assert.True(MyNumerics.AreNumericallyEqual(road.ExtrapolateNodePos(Side.End, 1), node2.Pos));
    }

    [Test]
    public void LaneExpansionOneLaneToThreeLane_Left()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Vector3 buildPoint = 2 * stride + road.BezierSeries.Evaluate2DNormalizedNormal(1) * Constants.LaneWidth;
        BuildTargets bt = new(buildPoint, 3, Game.Nodes.Values);
        Assert.AreEqual(3, bt.Nodes.Count);
        Node node0 = bt.Nodes[0];
        Node node1 = bt.Nodes[1];
        Node node2 = bt.Nodes[2];

        Assert.AreEqual(road.ExtrapolateNodePos(Side.End, -2), node0.Pos);
        Assert.AreEqual(road.ExtrapolateNodePos(Side.End, -1), node1.Pos);
        Assert.AreSame(road.Lanes[0].EndNode, node2);
    }

    [Test]
    public void LaneExpansionOneLaneToThreeLane_Right()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Vector3 buildPoint = 2 * stride + -1 * Constants.LaneWidth * road.BezierSeries.Evaluate2DNormalizedNormal(1);
        BuildTargets bt = new(buildPoint, 3, Game.Nodes.Values);
        Assert.AreEqual(3, bt.Nodes.Count);
        Node node0 = bt.Nodes[0];
        Node node1 = bt.Nodes[1];
        Node node2 = bt.Nodes[2];

        Assert.AreSame(road.Lanes[0].EndNode, node0);
        Assert.AreEqual(road.ExtrapolateNodePos(Side.End, 1), node1.Pos);
        Assert.AreEqual(road.ExtrapolateNodePos(Side.End, 2), node2.Pos);
        
    }

    [Test]
    public void LaneContractionThreeLanetoOneLane()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
        BuildTargets bt = new(0, 3, Game.Nodes.Values);
        Assert.AreEqual(3, bt.Nodes.Count);
        Node node0 = bt.Nodes[0];
        Node node1 = bt.Nodes[1];
        Node node2 = bt.Nodes[2];

        Assert.AreEqual(road.ExtrapolateNodePos(Side.Start, -1), node0.Pos);
        Assert.AreEqual(road.Lanes[0].StartPos, node1.Pos);
        Assert.AreEqual(road.ExtrapolateNodePos(Side.Start, 1), node2.Pos);
    }

    [Test]
    public void InitialRoadCanBeSnappedAtStart()
    {
        RoadBuilder.Single(0, stride, 2 * stride, 1);
        BuildTargets bt = new(0, 3, Game.Nodes.Values);
        Assert.True(bt.Snapped);
    }

    [Test]
    public void NodeUnsnappableAtMinimumElevation()
    {
        RoadBuilder.Single(new float3(0, -2, 0), stride, 2 * stride, 1);
        BuildTargets bt = new(0, 1, Game.Nodes.Values);
        
        Assert.False(bt.Snapped);
    }

    [Test]
    public void NodeBelongsToPointOnlyAcceptsOneLaneSnap()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
        road.Lanes[0].StartNode.BelongsToPoint = true;

        BuildTargets bt = new(0, 1, Game.Nodes.Values);
        Assert.True(bt.Snapped);

        bt = new(0, 2, Game.Nodes.Values);
        Assert.False(bt.Snapped);

        bt = new(0, 3, Game.Nodes.Values);
        Assert.False(bt.Snapped);
    }
}