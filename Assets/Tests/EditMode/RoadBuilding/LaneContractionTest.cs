using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class LaneContractionTest
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
    public void ThreeLaneToOneLane_Mid()
    {
        Road road0 = RoadBuilder.Build(pos3, pos4, pos5, 1);
        Road road1 = RoadBuilder.Build(pos1, pos2 , pos3, 3);
        Lane lane00 = road0.Lanes[0];
        Lane lane10 = road1.Lanes[0];
        Lane lane11 = road1.Lanes[1];
        Lane lane12 = road1.Lanes[2];
        Assert.AreSame(lane00.StartNode, lane11.EndNode);
        Assert.AreEqual(7, Nodes.Count);
        Assert.True(lane10.EndNode.Lanes.SetEquals(new HashSet<Lane> { lane10 }));
        Assert.True(lane11.EndNode.Lanes.SetEquals(new HashSet<Lane> { lane11, lane00 }));
        Assert.True(lane12.EndNode.Lanes.SetEquals(new HashSet<Lane> { lane12 }));
    }
    // [Test]
    // public void ThreeLaneToOneLane_Left()
    // {
    //     Road road0 = RoadBuilder.Build(pos3, pos4, pos5, 1);
    //     Road road1 = RoadBuilder.Build(pos1, pos2 , pos3, 3);
    //     Lane lane00 = road0.Lanes[0];
    //     Lane lane10 = road1.Lanes[0];
    //     Lane lane11 = road1.Lanes[1];
    //     Lane lane12 = road1.Lanes[2];
    //     Assert.AreSame(lane00.StartNode, lane11.EndNode);
    //     Assert.AreEqual(7, Nodes.Count);
    //     Assert.True(lane10.EndNode.Lanes.SetEquals(new HashSet<Lane> { lane10 }));
    //     Assert.True(lane11.EndNode.Lanes.SetEquals(new HashSet<Lane> { lane11, lane00 }));
    //     Assert.True(lane12.EndNode.Lanes.SetEquals(new HashSet<Lane> { lane12 }));
    // }
}