using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

public class BuildTargetTest
{
    Vector3 pos1 = new(10, 10, 10);
    Vector3 pos2 = new(30, 12, 30);
    Vector3 pos3 = new(60, 14, 60);
    SortedDictionary<int, Road> Roads;

    [SetUp]
    public void SetUp()
    {
        BuildManager.Reset();
        Game.WipeGameState();
        Roads = Game.Roads;
    }

    [Test]
    public void NoSnap()
    {
        BuildTargets bt1 = new(pos1, 1, Side.Start);
        BuildTargets bt2 = new(pos1, 2, Side.Start);
        Assert.False(bt1.SnapNotNull);
        Assert.False(bt2.SnapNotNull);
        Assert.AreEqual(pos1, bt1.ClickPos);
        Assert.AreEqual(pos1, bt2.ClickPos);
    }

    [Test]
    public void RepeatingOneLaneRoad_OnEnd()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 1);
        BuildTargets bt = new(pos3, 1, Side.Start);
        Node node = bt.Nodes[0];
        Road road = Roads.Values.First();
        Lane lane = road.Lanes[0];

        Assert.True(bt.SnapNotNull);
        Assert.AreEqual(pos3, bt.MedianPoint);
        Assert.AreSame(lane.EndNode, node);
    }

    [Test]
    public void RepeatingTwoLaneRoad_OnStart()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 2);
        BuildTargets bt = new(pos1, 2, Side.End);
        Node node0 = bt.Nodes[0];
        Node node1 = bt.Nodes[1];
        Road road = Roads.Values.First();
        Lane lane0 = road.Lanes[0];
        Lane lane1 = road.Lanes[1];

        Assert.True(bt.SnapNotNull);
        Assert.AreEqual(pos1, bt.MedianPoint);
        Assert.AreEqual(lane0.StartNode, node0);
        Assert.AreEqual(lane1.StartNode, node1);
    }

    [Test]
    public void AttachOneLaneToTwoLane_OnEnd()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 2);
        Road road = Roads.Values.First();
        Lane lane = road.Lanes[0];
        BuildTargets bt = new(lane.EndPos + 0.9f * GlobalConstants.SnapTolerance * Vector3.back, 1, Side.Start);
        Assert.AreEqual(1, bt.Nodes.Count);
        Node node = bt.Nodes[0];


        Assert.True(bt.SnapNotNull);
        Assert.AreEqual(lane.EndPos, bt.MedianPoint);
        Assert.AreSame(lane.EndNode, node);
    }

    [Test]
    public void AttachTwoLaneToThreeLane_OnStart()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 3);
        Road road = Roads.Values.First();
        Lane lane0 = road.Lanes[0];
        Lane lane1 = road.Lanes[1];
        Vector3 midPoint = Vector3.Lerp(lane0.StartPos, lane1.StartPos, 0.5f);
        BuildTargets bt = new(midPoint + 0.9f * GlobalConstants.SnapTolerance * Vector3.left, 2, Side.End);
        Assert.AreEqual(2, bt.Nodes.Count);
        Node node0 = bt.Nodes[0];
        Node node1 = bt.Nodes[1];

        Assert.True(bt.SnapNotNull);
        Assert.AreEqual(midPoint, bt.MedianPoint);
        Assert.AreEqual(lane0.StartNode, node0);
        Assert.AreEqual(lane1.StartNode, node1);
    }

    [Test]
    public void LaneExpansionOneLaneToTwoLane_Left()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 1);
        Vector3 buildPoint = pos3 + 0.9f * GlobalConstants.SnapTolerance * Vector3.forward;
        BuildTargets bt = new(buildPoint, 2, Side.Start);
        Assert.AreEqual(2, bt.Nodes.Count);
        Node node0 = bt.Nodes[0];
        Node node1 = bt.Nodes[1];
        Lane lane = node1.Lanes.First();

        Assert.AreEqual(lane.Road.InterpolateLanePos(1, -1), (float3)node0.Pos);
        Assert.AreEqual(pos3, node1.Pos);
    }

    [Test]
    public void LaneExpansionOneLaneToTwoLane_Right()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 1);
        Vector3 buildPoint = pos3 + 0.9f * GlobalConstants.SnapTolerance * Vector3.back;
        BuildTargets bt = new(buildPoint, 2, Side.Start);
        Assert.AreEqual(2, bt.Nodes.Count);
        Node node0 = bt.Nodes[0];
        Node node1 = bt.Nodes[1];
        Lane lane = node0.Lanes.First();

        Assert.AreEqual(lane.Road.InterpolateLanePos(1, 1), (float3)node1.Pos);
        Assert.AreEqual(pos3, node0.Pos);
    }

    [Test]
    public void LaneExpansionTwoLaneToThreeLane_Left()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 2);
        Vector3 buildPoint = pos3 + 0.9f * GlobalConstants.SnapTolerance * Vector3.forward;
        BuildTargets bt = new(buildPoint, 3, Side.Start);
        Assert.AreEqual(3, bt.Nodes.Count);
        Node node0 = bt.Nodes[0];
        Node node1 = bt.Nodes[1];
        Node node2 = bt.Nodes[2];
        Road road = node1.Lanes.First().Road;
        Lane lane0 = road.Lanes[0];
        Lane lane1 = road.Lanes[1];

        Assert.AreEqual(road.InterpolateLanePos(1, -1), (float3)node0.Pos);
        Assert.AreSame(lane0.EndNode, node1);
        Assert.AreSame(lane1.EndNode, node2);
    }

    [Test]
    public void LaneExpansionTwoLaneToThreeLane_Right()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 2);
        Vector3 buildPoint = pos3 + 0.9f * GlobalConstants.SnapTolerance * Vector3.back;
        BuildTargets bt = new(buildPoint, 3, Side.Start);
        Assert.AreEqual(3, bt.Nodes.Count);
        Node node0 = bt.Nodes[0];
        Node node1 = bt.Nodes[1];
        Node node2 = bt.Nodes[2];
        Road road = node1.Lanes.First().Road;
        Lane lane0 = road.Lanes[0];
        Lane lane1 = road.Lanes[1];

        Assert.AreEqual(road.InterpolateLanePos(1, 2), (float3)node2.Pos);
        Assert.AreEqual(lane0.EndPos, node0.Pos);
        Assert.AreEqual(lane1.EndPos, node1.Pos);
        Assert.AreSame(node0, lane0.EndNode);
    }

    [Test]
    public void LaneExpansionOneLaneToThreeLane_Mid()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 1);
        BuildTargets bt = new(pos3, 3, Side.Start);
        Assert.AreEqual(3, bt.Nodes.Count);
        Node node0 = bt.Nodes[0];
        Node node1 = bt.Nodes[1];
        Node node2 = bt.Nodes[2];
        Road road = node1.Lanes.First().Road;

        Assert.AreEqual(road.InterpolateLanePos(1, -1), (float3)node0.Pos);
        Assert.AreSame(road.Lanes[0].EndNode, node1);
        Assert.AreEqual(road.InterpolateLanePos(1, 1), (float3)node2.Pos);
    }

    [Test]
    public void LaneExpansionOneLaneToThreeLane_Left()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 1);
        Vector3 buildPoint = pos3 + 1.5f * GlobalConstants.SnapTolerance * Vector3.left;
        BuildTargets bt = new(buildPoint, 3, Side.Start);
        Assert.AreEqual(3, bt.Nodes.Count);
        Node node0 = bt.Nodes[0];
        Node node1 = bt.Nodes[1];
        Node node2 = bt.Nodes[2];
        Road road = node2.Lanes.First().Road;

        Assert.AreEqual(road.InterpolateLanePos(1, -2), (float3)node0.Pos);
        Assert.AreEqual(road.InterpolateLanePos(1, -1), (float3)node1.Pos);
        Assert.AreSame(road.Lanes[0].EndNode, node2);
    }

    [Test]
    public void LaneExpansionOneLaneToThreeLane_Right()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 1);
        Vector3 buildPoint = pos3 + 1.5f * GlobalConstants.SnapTolerance * Vector3.right;
        BuildTargets bt = new(buildPoint, 3, Side.Start);
        Assert.AreEqual(3, bt.Nodes.Count);
        Node node0 = bt.Nodes[0];
        Node node1 = bt.Nodes[1];
        Node node2 = bt.Nodes[2];
        Road road = node0.Lanes.First().Road;

        Assert.AreEqual(road.InterpolateLanePos(1, 1), (float3)node1.Pos);
        Assert.AreEqual(road.InterpolateLanePos(1, 2), (float3)node2.Pos);
        Assert.AreSame(road.Lanes[0].EndNode, node0);
    }
}