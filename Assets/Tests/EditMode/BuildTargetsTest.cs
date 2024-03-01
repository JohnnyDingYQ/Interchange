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
    public void RepeatingRoad_OneLaneOnEnd()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 1);
        BuildTargets bt = new(pos3, 1);
        BuildNode bn = bt.BuildNodes[0];
        Road road = RoadWatcher.Values.First();
        Lane lane  = road.Lanes[0];

        Assert.AreSame(road, bt.Road);
        Assert.True(bt.SnapNotNull);
        Assert.AreEqual(pos3, bt.MedianPoint);
        Assert.AreEqual(NodeType.EndNode, bt.NodeType);
        Assert.AreEqual(lane.EndNode, bn.Node);
        Assert.AreEqual(pos3, bn.Pos);
        Assert.AreSame(lane, bn.Lane);
    }

    [Test]
    public void RepeatingRoad_TwoLaneOnStart()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 2);
        BuildTargets bt = new(pos1, 2);
        BuildNode bn0 = bt.BuildNodes[0];
        BuildNode bn1 = bt.BuildNodes[1];
        Road road = RoadWatcher.Values.First();
        Lane lane0  = road.Lanes[0];
        Lane lane1  = road.Lanes[1];

        Assert.AreSame(road, bt.Road);
        Assert.True(bt.SnapNotNull);
        Assert.AreEqual(pos1, bt.MedianPoint);
        Assert.AreEqual(NodeType.StartNode, bt.NodeType);
        Assert.AreEqual(lane0.StartNode, bn0.Node);
        Assert.AreEqual(lane1.StartNode, bn1.Node);
        Assert.AreEqual(lane0.StartPos, (Vector3) bn0.Pos);
        Assert.AreEqual(lane1.StartPos, (Vector3) bn1.Pos);
        Assert.AreSame(lane0, bn0.Lane);
        Assert.AreSame(lane1, bn1.Lane);
    }

    [Test]
    public void MergeAndBranch_TwotoOneOnEnd()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 2);
        Road road = RoadWatcher.Values.First();
        Lane lane  = road.Lanes[0];
        BuildTargets bt = new(lane.EndPos, 1);
        Assert.AreEqual(1, bt.BuildNodes.Count);
        BuildNode bn = bt.BuildNodes[0];
        
        
        Assert.AreSame(road, bt.Road);
        Assert.True(bt.SnapNotNull);
        Assert.AreEqual(lane.EndPos, (Vector3) bt.MedianPoint);
        Assert.AreEqual(NodeType.EndNode, bt.NodeType);
        Assert.AreEqual(lane.EndNode, bn.Node);
        Assert.AreEqual(lane.EndPos, (Vector3) bn.Pos);
        Assert.AreSame(lane, bn.Lane);
    }
}