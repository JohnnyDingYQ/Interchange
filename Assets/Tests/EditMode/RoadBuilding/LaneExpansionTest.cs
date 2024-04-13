using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class LaneExpansionTest
{
    Vector3 pos1 = new(0, 10, 0);
    Vector3 pos2 = new(0, 12, Constants.MinimumLaneLength);
    Vector3 pos3 = new(0, 14, Constants.MinimumLaneLength * 2);
    Vector3 pos4 = new(0, 16, Constants.MinimumLaneLength * 3);
    Vector3 pos5 = new(0, 16, Constants.MinimumLaneLength * 4);
    SortedDictionary<int, Node> Nodes;

    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
        Nodes = Game.Nodes;
    }

    [Test]
    public void OneLaneToTwoLane_Left()
    {
        Road road0 = RoadBuilder.Build(pos1, pos2, pos3, 1);
        Road road1 = RoadBuilder.Build(pos3 + 0.9f * Constants.BuildSnapTolerance * Vector3.forward, pos4, pos5, 2);
        Lane lane00 = road0.Lanes[0];
        Lane lane10 = road1.Lanes[0];
        Lane lane11 = road1.Lanes[1];

        Assert.AreSame(lane00.EndNode, lane11.StartNode);
        Assert.AreEqual(5, Nodes.Count);
        Assert.True(lane10.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane10 }));
        Assert.True(lane00.EndNode.Lanes.SetEquals(new HashSet<Lane> { lane00, lane11 }));
    }

    [Test]
    public void TwoLaneToThreeLane_Right()
    {   
        Road road0 = RoadBuilder.Build(pos1, pos2, pos3, 2);
        Road road1 = RoadBuilder.Build(pos3 + 0.9f * Constants.BuildSnapTolerance * Vector3.right, pos4, pos5, 3);
        Lane lane00 = road0.Lanes[0];
        Lane lane01 = road0.Lanes[1];
        Lane lane10 = road1.Lanes[0];
        Lane lane11 = road1.Lanes[1];
        Lane lane12 = road1.Lanes[2];

        Assert.AreSame(lane00.EndNode, lane10.StartNode);
        Assert.AreSame(lane01.EndNode, lane11.StartNode);
        Assert.AreEqual(8, Nodes.Count);
        Assert.True(lane12.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane12 }));
        Assert.True(lane10.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane00, lane10 }));
        Assert.True(lane11.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane01, lane11 }));
    }

    [Test]
    public void OneLaneToThreeLane_Mid()
    {
        Road road0 = RoadBuilder.Build(pos1, pos2, pos3, 1);
        Road road1 = RoadBuilder.Build(pos3, pos4, pos5, 3);
        Lane lane00 = road0.Lanes[0];
        Lane lane10 = road1.Lanes[0];
        Lane lane11 = road1.Lanes[1];
        Lane lane12 = road1.Lanes[2];

        Assert.AreSame(lane00.EndNode, lane11.StartNode);
        Assert.AreEqual(7, Nodes.Count);
        Assert.True(lane10.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane10 }));
        Assert.True(lane11.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane11, lane00 }));
        Assert.True(lane12.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane12 }));
    }

    [Test]
    public void OneLaneToThreeLane_Left()
    {
        Road road0 = RoadBuilder.Build(pos1, pos2, pos3, 1);
        Road road1 = RoadBuilder.Build(pos3 + 1.5f * Constants.BuildSnapTolerance * Vector3.left, pos4, pos5, 3);
        Lane lane00 = road0.Lanes[0];
        Lane lane10 = road1.Lanes[0];
        Lane lane11 = road1.Lanes[1];
        Lane lane12 = road1.Lanes[2];
        Assert.AreSame(lane00.EndNode, lane12.StartNode);
        Assert.AreEqual(7, Nodes.Count);
        Assert.True(lane10.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane10 }));
        Assert.True(lane11.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane11 }));
        Assert.True(lane12.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane12, lane00 }));
    }

    [Test]
    public void OneLaneToThreeLane_Right()
    {   
        Road road0 = RoadBuilder.Build(pos1, pos2, pos3, 1);
        Road road1 = RoadBuilder.Build(pos3 + 1.5f * Constants.BuildSnapTolerance * Vector3.right, pos4, pos5, 3);
        Lane lane00 = road0.Lanes[0];
        Lane lane10 = road1.Lanes[0];
        Lane lane11 = road1.Lanes[1];
        Lane lane12 = road1.Lanes[2];
        Assert.AreSame(lane00.EndNode, lane10.StartNode);
        Assert.AreEqual(7, Nodes.Count);
        Assert.True(lane10.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane10, lane00 }));
        Assert.True(lane11.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane11 }));
        Assert.True(lane12.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane12 }));
    }
}