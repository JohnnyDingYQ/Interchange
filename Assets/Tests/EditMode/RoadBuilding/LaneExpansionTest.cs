using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class LaneExpansionTest
{
    Vector3 pos1 = new(0, 10, 0);
    Vector3 pos2 = new(0, 12, GConsts.MinimumRoadLength);
    Vector3 pos3 = new(0, 14, GConsts.MinimumRoadLength * 2);
    Vector3 pos4 = new(0, 16, GConsts.MinimumRoadLength * 3);
    Vector3 pos5 = new(0, 16, GConsts.MinimumRoadLength * 4);

    [SetUp]
    public void SetUp()
    {
        BuildHandler.Reset();
        Game.WipeState();
    }

    [Test]
    public void OneLaneToTwoLane_Left()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 1);
        RoadBuilder.BuildRoad(pos3 + 0.9f * GConsts.BuildSnapTolerance * Vector3.forward, pos4, pos5, 2);
        Road road0 = Utility.FindRoadWithStartPos(pos1);
        Road road1 = Utility.FindRoadWithEndPos(pos5);
        Lane lane00 = road0.Lanes[0];
        Lane lane10 = road1.Lanes[0];
        Lane lane11 = road1.Lanes[1];

        Assert.AreSame(lane00.EndNode, lane11.StartNode);
        Assert.True(lane10.StartNode.IsRegistered());
        Assert.True(lane10.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane10 }));
        Assert.True(lane00.EndNode.Lanes.SetEquals(new HashSet<Lane> { lane00, lane11 }));
    }

    [Test]
    public void TwoLaneToThreeLane_Right()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 2);
        RoadBuilder.BuildRoad(pos3 + 0.9f * GConsts.BuildSnapTolerance * Vector3.right, pos4, pos5, 3);
        Road road0 = Utility.FindRoadWithStartPos(pos1);
        Road road1 = Utility.FindRoadWithEndPos(pos5);
        Lane lane00 = road0.Lanes[0];
        Lane lane01 = road0.Lanes[1];
        Lane lane10 = road1.Lanes[0];
        Lane lane11 = road1.Lanes[1];
        Lane lane12 = road1.Lanes[2];

        Assert.AreSame(lane00.EndNode, lane10.StartNode);
        Assert.AreSame(lane01.EndNode, lane11.StartNode);
        Assert.True(lane12.StartNode.IsRegistered());
        Assert.True(lane12.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane12 }));
        Assert.True(lane10.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane00, lane10 }));
        Assert.True(lane11.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane01, lane11 }));
    }

    [Test]
    public void OneLaneToThreeLane_Mid()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 1);
        RoadBuilder.BuildRoad(pos3, pos4, pos5, 3);
        Road road0 = Utility.FindRoadWithStartPos(pos1);
        Road road1 = Utility.FindRoadWithEndPos(pos5);
        Lane lane00 = road0.Lanes[0];
        Lane lane10 = road1.Lanes[0];
        Lane lane11 = road1.Lanes[1];
        Lane lane12 = road1.Lanes[2];

        Assert.AreSame(lane00.EndNode, lane11.StartNode);
        Assert.True(lane10.StartNode.IsRegistered());
        Assert.True(lane12.StartNode.IsRegistered());
        Assert.True(lane10.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane10 }));
        Assert.True(lane11.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane11, lane00 }));
        Assert.True(lane12.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane12 }));
    }

    [Test]
    public void OneLaneToThreeLane_Left()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 1);
        RoadBuilder.BuildRoad(pos3 + 1.5f * GConsts.BuildSnapTolerance * Vector3.left, pos4, pos5, 3);
        Road road0 = Utility.FindRoadWithStartPos(pos1);
        Road road1 = Utility.FindRoadWithEndPos(pos5);
        Lane lane00 = road0.Lanes[0];
        Lane lane10 = road1.Lanes[0];
        Lane lane11 = road1.Lanes[1];
        Lane lane12 = road1.Lanes[2];
        Assert.AreSame(lane00.EndNode, lane12.StartNode);
        Assert.True(lane11.StartNode.IsRegistered());
        Assert.True(lane10.StartNode.IsRegistered());
        Assert.True(lane10.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane10 }));
        Assert.True(lane11.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane11 }));
        Assert.True(lane12.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane12, lane00 }));
    }

    [Test]
    public void OneLaneToThreeLane_Right()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 1);
        RoadBuilder.BuildRoad(pos3 + 1.5f * GConsts.BuildSnapTolerance * Vector3.right, pos4, pos5, 3);
        Road road0 = Utility.FindRoadWithStartPos(pos1);
        Road road1 = Utility.FindRoadWithEndPos(pos5);
        Lane lane00 = road0.Lanes[0];
        Lane lane10 = road1.Lanes[0];
        Lane lane11 = road1.Lanes[1];
        Lane lane12 = road1.Lanes[2];
        Assert.AreSame(lane00.EndNode, lane10.StartNode);
        Assert.True(lane11.StartNode.IsRegistered());
        Assert.True(lane10.StartNode.IsRegistered());
        Assert.True(lane10.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane10, lane00 }));
        Assert.True(lane11.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane11 }));
        Assert.True(lane12.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane12 }));
    }
}