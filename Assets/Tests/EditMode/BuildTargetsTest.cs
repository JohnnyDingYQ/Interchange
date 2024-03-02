using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class BuildTargetTest
{
    float3 pos1 = new(10, 10, 10);
    float3 pos2 = new(30, 12, 30);
    float3 pos3 = new(60, 14, 60);
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
        BuildTargets bt1 = new(pos1, 1);
        BuildTargets bt2 = new(pos1, 2);
        Assert.False(bt1.SnapNotNull);
        Assert.False(bt2.SnapNotNull);
        Assert.AreEqual(pos1, bt1.ClickPos);
        Assert.AreEqual(pos1, bt2.ClickPos);
    }

    [Test]
    public void RepeatingOneLaneRoad_OnEnd()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 1);
        BuildTargets bt = new(pos3, 1);
        Node node = bt.Nodes[0];
        Road road = Roads.Values.First();
        Lane lane  = road.Lanes[0];

        Assert.True(bt.SnapNotNull);
        Assert.AreEqual(pos3, bt.MedianPoint);
        Assert.AreSame(lane.EndNode, node);
    }

    [Test]
    public void RepeatingTwoLaneRoad_OnStart()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 2);
        BuildTargets bt = new(pos1, 2);
        Node node0 = bt.Nodes[0];
        Node node1 = bt.Nodes[1];
        Road road = Roads.Values.First();
        Lane lane0  = road.Lanes[0];
        Lane lane1  = road.Lanes[1];

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
        Lane lane  = road.Lanes[0];
        BuildTargets bt = new(lane.EndPos + 0.9f * GlobalConstants.SnapTolerance * Vector3.back, 1);
        Assert.AreEqual(1, bt.Nodes.Count);
        Node node = bt.Nodes[0];
        
        
        Assert.True(bt.SnapNotNull);
        Assert.AreEqual(lane.EndPos, (Vector3) bt.MedianPoint);
        Assert.AreSame(lane.EndNode, node);
    }

    [Test]
    public void AttachTwoLaneToThreeLane_OnStart()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 3);
        Road road = Roads.Values.First();
        Lane lane0  = road.Lanes[0];
        Lane lane1  = road.Lanes[1];
        Vector3 midPoint = Vector3.Lerp(lane0.StartPos, lane1.StartPos, 0.5f);
        BuildTargets bt = new(midPoint + 0.9f * GlobalConstants.SnapTolerance * Vector3.left, 2);
        Assert.AreEqual(2, bt.Nodes.Count);
        Node node0 = bt.Nodes[0];
        Node node1 = bt.Nodes[1];

        Assert.True(bt.SnapNotNull);
        Assert.AreEqual(midPoint, (Vector3) bt.MedianPoint);
        Assert.AreEqual(lane0.StartNode, node0);
        Assert.AreEqual(lane1.StartNode, node1);
    }

    [Test]
    public void AttachTwoLaneToOneLane_OnEnd()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 1);
    }
}