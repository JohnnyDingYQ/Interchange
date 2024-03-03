using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class LaneExpansionTest
{
    Vector3 pos1 = new(0, 10, 0);
    Vector3 pos2 = new(0, 12, 30);
    Vector3 pos3 = new(0, 14, 60);
    Vector3 pos4 = new(90, 16, 90);
    Vector3 pos5 = new(120, 16, 120);
    Vector3 pos6 = new(150, 16, 150);

    [SetUp]
    public void SetUp()
    {
        BuildManager.Reset();
        Game.WipeGameState();
    }

    [Test]
    public void OneLaneToTwoLane_Left()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 1);
        RoadBuilder.BuildRoad(pos3 + 0.9f * GlobalConstants.SnapTolerance * Vector3.forward, pos4, pos5, 2);
        Road road0 = Utility.FindRoadWithStartPos(pos1);
        Road road1 = Utility.FindRoadWithEndPos(pos5);
        Lane lane00 = road0.Lanes[0];
        Lane lane10 = road1.Lanes[0];
        Lane lane11 = road1.Lanes[1];

        Assert.AreSame(lane00.EndNode, lane11.StartNode);
        Assert.AreNotEqual(-1, lane10.StartNode.Id);
        Assert.True(lane10.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane10 }));
        Assert.True(lane00.EndNode.Lanes.SetEquals(new HashSet<Lane> { lane00, lane11 }));
    }

    [Test]
    public void TwoLaneToThreeLane_Right()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 2);
        RoadBuilder.BuildRoad(pos3 + 0.9f * GlobalConstants.SnapTolerance * Vector3.right, pos4, pos5, 3);
        Road road0 = Utility.FindRoadWithStartPos(pos1);
        Road road1 = Utility.FindRoadWithEndPos(pos5);
        Lane lane00 = road0.Lanes[0];
        Lane lane01 = road0.Lanes[1];
        Lane lane10 = road1.Lanes[0];
        Lane lane11 = road1.Lanes[1];
        Lane lane12 = road1.Lanes[2];

        Assert.AreSame(lane00.EndNode, lane10.StartNode);
        Assert.AreSame(lane01.EndNode, lane11.StartNode);
        Assert.AreNotEqual(-1, lane12.StartNode.Id);
        Assert.True(lane12.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane12 }));
        Assert.True(lane00.EndNode.Lanes.SetEquals(new HashSet<Lane> { lane00, lane10 }));
        Assert.True(lane01.EndNode.Lanes.SetEquals(new HashSet<Lane> { lane01, lane11 }));
    }
}