using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class BranchingTest
{
    Vector3 pos1 = new(0, 0, 0);
    Vector3 pos2 = GConsts.MinimumRoadLength * new Vector3(1, 0, 1);
    Vector3 pos3 = GConsts.MinimumRoadLength * 2 * new Vector3(1, 0, 1);
    Vector3 offset = GConsts.MinimumRoadLength * new Vector3(1, 0, 1);
    SortedDictionary<int, Road> Roads;

    [SetUp]
    public void SetUp()
    {
        BuildHandler.Reset();
        Game.WipeGameState();
        Roads = Game.Roads;
    }

    [Test]
    public void TwoToOneOne_OnEnd()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 2);
        Road road = Roads.Values.First();
        Vector3 p1 = road.Lanes[0].EndPos;
        Vector3 p2 = road.Lanes[1].EndPos;
        RoadBuilder.BuildRoad(p1, p1 + offset, p1 + 2 * offset, 1);
        RoadBuilder.BuildRoad(p2, p2 + offset, p2 + 2 * offset, 1);
        Road branch1 = Utility.FindRoadWithStartPos(p1);
        Road branch2 = Utility.FindRoadWithStartPos(p2);

        Assert.True(road.Lanes[0].EndNode.Lanes.SetEquals(new HashSet<Lane>() { branch1.Lanes[0], road.Lanes[0] }));
        Assert.True(road.Lanes[1].EndNode.Lanes.SetEquals(new HashSet<Lane>() { branch2.Lanes[0], road.Lanes[1] }));
        Assert.True(IsIsolatedNode(road.Lanes[0].StartNode));
        Assert.True(IsIsolatedNode(road.Lanes[1].StartNode));
        Assert.True(IsIsolatedNode(branch1.Lanes[0].EndNode));
        Assert.True(IsIsolatedNode(branch2.Lanes[0].EndNode));
    }

    [Test]
    public void TwoToOneOne_OnStart()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 2);
        Road road = Roads.Values.First();
        Vector3 p1 = road.Lanes[0].StartPos;
        Vector3 p2 = road.Lanes[1].StartPos;
        RoadBuilder.BuildRoad(p1 - 2 * offset, p1 - offset, p1, 1);
        RoadBuilder.BuildRoad(p2 - 2 * offset, p2 - offset, p2, 1);
        Road branch1 = Utility.FindRoadWithEndPos(p1);
        Road branch2 = Utility.FindRoadWithEndPos(p2);

        Assert.True(road.Lanes[0].StartNode.Lanes.SetEquals(new HashSet<Lane>() { branch1.Lanes[0], road.Lanes[0] }));
        Assert.True(road.Lanes[1].StartNode.Lanes.SetEquals(new HashSet<Lane>() { branch2.Lanes[0], road.Lanes[1] }));
        Assert.True(IsIsolatedNode(road.Lanes[0].EndNode));
        Assert.True(IsIsolatedNode(road.Lanes[1].EndNode));
        Assert.True(IsIsolatedNode(branch1.Lanes[0].StartNode));
        Assert.True(IsIsolatedNode(branch2.Lanes[0].StartNode));
    }

    [Test]
    public void TwoToOne_OnEnd()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 2);
        Road road = Roads.Values.First();
        Vector3 p1 = road.Lanes[0].EndPos;
        RoadBuilder.BuildRoad(p1, p1 + offset, p1 + 2 * offset, 1);
        Road branch1 = Utility.FindRoadWithStartPos(p1);

        Assert.True(road.Lanes[0].EndNode.Lanes.SetEquals(new HashSet<Lane>() { branch1.Lanes[0], road.Lanes[0] }));
        Assert.True(road.Lanes[1].EndNode.Lanes.SetEquals(new HashSet<Lane>() { road.Lanes[1] }));
        Assert.True(IsIsolatedNode(road.Lanes[0].StartNode));
        Assert.True(IsIsolatedNode(road.Lanes[1].StartNode));
        Assert.True(IsIsolatedNode(branch1.Lanes[0].EndNode));
    }

    [Test]
    public void TwoToOne_OnStart()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 2);
        Road road = Roads.Values.First();
        Vector3 p1 = road.Lanes[0].StartPos;
        RoadBuilder.BuildRoad( p1 - 2 * offset, p1 - offset, p1, 1);
        Road branch1 = Utility.FindRoadWithEndPos(p1);

        Assert.True(road.Lanes[0].StartNode.Lanes.SetEquals(new HashSet<Lane>() { branch1.Lanes[0], road.Lanes[0] }));
        Assert.True(road.Lanes[1].StartNode.Lanes.SetEquals(new HashSet<Lane>() { road.Lanes[1] }));
        Assert.True(IsIsolatedNode(road.Lanes[0].EndNode));
        Assert.True(IsIsolatedNode(road.Lanes[1].EndNode));
        Assert.True(IsIsolatedNode(branch1.Lanes[0].StartNode));
    }

    [Test]
    public void ThreetoTwoOne_OnEnd()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 3);
        Road road = Roads.Values.First();
        Vector3 p1 = road.Lanes[0].EndPos;
        Vector3 p2 = (road.Lanes[1].EndPos + road.Lanes[2].EndPos) / 2;
        RoadBuilder.BuildRoad(p1, p1 + offset, p1 + 2 * offset, 1);
        RoadBuilder.BuildRoad(p2, p2 + offset, p2 + 2 * offset, 2);
        Road branch1 = Utility.FindRoadWithStartPos(p1);
        Road branch2 = Utility.FindRoadWithStartPos(p2);

        Assert.True(road.Lanes[0].EndNode.Lanes.SetEquals(new HashSet<Lane>() { branch1.Lanes[0], road.Lanes[0] }));
        Assert.True(road.Lanes[1].EndNode.Lanes.SetEquals(new HashSet<Lane>() { branch2.Lanes[0], road.Lanes[1] }));
        Assert.True(road.Lanes[2].EndNode.Lanes.SetEquals(new HashSet<Lane>() { branch2.Lanes[1], road.Lanes[2] }));
        Assert.True(IsIsolatedNode(road.Lanes[0].StartNode));
        Assert.True(IsIsolatedNode(road.Lanes[1].StartNode));
        Assert.True(IsIsolatedNode(road.Lanes[2].StartNode));
        Assert.True(IsIsolatedNode(branch1.Lanes[0].EndNode));
        Assert.True(IsIsolatedNode(branch2.Lanes[0].EndNode));
        Assert.True(IsIsolatedNode(branch2.Lanes[1].EndNode));
    }

    [Test]
    public void ThreetoTwoOne_OnStart()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 3);
        Road road = Roads.Values.First();
        Vector3 p1 = road.Lanes[0].StartPos;
        Vector3 p2 = (road.Lanes[1].StartPos + road.Lanes[2].StartPos) / 2;
        RoadBuilder.BuildRoad(p1 - 2 * offset, p1 - offset, p1, 1);
        RoadBuilder.BuildRoad(p2 - 2 * offset, p2 - offset, p2, 2);
        Road branch1 = Utility.FindRoadWithEndPos(p1);
        Road branch2 = Utility.FindRoadWithEndPos(p2);

        Assert.True(road.Lanes[0].StartNode.Lanes.SetEquals(new HashSet<Lane>() { branch1.Lanes[0], road.Lanes[0] }));
        Assert.True(road.Lanes[1].StartNode.Lanes.SetEquals(new HashSet<Lane>() { branch2.Lanes[0], road.Lanes[1] }));
        Assert.True(road.Lanes[2].StartNode.Lanes.SetEquals(new HashSet<Lane>() { branch2.Lanes[1], road.Lanes[2] }));
        Assert.True(IsIsolatedNode(road.Lanes[0].EndNode));
        Assert.True(IsIsolatedNode(road.Lanes[1].EndNode));
        Assert.True(IsIsolatedNode(road.Lanes[2].EndNode));
        Assert.True(IsIsolatedNode(branch1.Lanes[0].StartNode));
        Assert.True(IsIsolatedNode(branch2.Lanes[0].StartNode));
        Assert.True(IsIsolatedNode(branch2.Lanes[1].StartNode));
    }
    [Test]
    public void ThreetoOneOneOne_OnEnd()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 3);
        Road road = Roads.Values.First();
        Vector3 p1 = road.Lanes[0].EndPos;
        Vector3 p2 = road.Lanes[1].EndPos;
        Vector3 p3 = road.Lanes[2].EndPos;
        RoadBuilder.BuildRoad(p1, p1 + offset, p1 + 2 * offset, 1);
        RoadBuilder.BuildRoad(p2, p2 + offset, p2 + 2 * offset, 1);
        RoadBuilder.BuildRoad(p3, p3 + offset, p3 + 2 * offset, 1);
        Road branch1 = Utility.FindRoadWithStartPos(p1);
        Road branch2 = Utility.FindRoadWithStartPos(p2);
        Road branch3 = Utility.FindRoadWithStartPos(p3);

        Assert.True(road.Lanes[0].EndNode.Lanes.SetEquals(new HashSet<Lane>() { branch1.Lanes[0], road.Lanes[0] }));
        Assert.True(road.Lanes[1].EndNode.Lanes.SetEquals(new HashSet<Lane>() { branch2.Lanes[0], road.Lanes[1] }));
        Assert.True(road.Lanes[2].EndNode.Lanes.SetEquals(new HashSet<Lane>() { branch3.Lanes[0], road.Lanes[2] }));
        Assert.True(IsIsolatedNode(road.Lanes[0].StartNode));
        Assert.True(IsIsolatedNode(road.Lanes[1].StartNode));
        Assert.True(IsIsolatedNode(road.Lanes[2].StartNode));
        Assert.True(IsIsolatedNode(branch1.Lanes[0].EndNode));
        Assert.True(IsIsolatedNode(branch2.Lanes[0].EndNode));
        Assert.True(IsIsolatedNode(branch3.Lanes[0].EndNode));
    }

    [Test]
    public void ThreetoOneOneOne_OnStart()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 3);
        Road road = Roads.Values.First();
        Vector3 p1 = road.Lanes[0].StartPos;
        Vector3 p2 = road.Lanes[1].StartPos;
        Vector3 p3 = road.Lanes[2].StartPos;
        RoadBuilder.BuildRoad(p1 - 2 * offset, p1 - offset, p1, 1);
        RoadBuilder.BuildRoad(p2 - 2 * offset, p2 - offset, p2, 1);
        RoadBuilder.BuildRoad(p3 - 2 * offset, p3 - offset, p3, 1);
        Road branch1 = Utility.FindRoadWithEndPos(p1);
        Road branch2 = Utility.FindRoadWithEndPos(p2);
        Road branch3 = Utility.FindRoadWithEndPos(p3);

        Assert.True(road.Lanes[0].StartNode.Lanes.SetEquals(new HashSet<Lane>() { branch1.Lanes[0], road.Lanes[0] }));
        Assert.True(road.Lanes[1].StartNode.Lanes.SetEquals(new HashSet<Lane>() { branch2.Lanes[0], road.Lanes[1] }));
        Assert.True(road.Lanes[2].StartNode.Lanes.SetEquals(new HashSet<Lane>() { branch3.Lanes[0], road.Lanes[2] }));
        Assert.True(IsIsolatedNode(road.Lanes[0].EndNode));
        Assert.True(IsIsolatedNode(road.Lanes[1].EndNode));
        Assert.True(IsIsolatedNode(road.Lanes[2].EndNode));
        Assert.True(IsIsolatedNode(branch1.Lanes[0].StartNode));
        Assert.True(IsIsolatedNode(branch2.Lanes[0].StartNode));
        Assert.True(IsIsolatedNode(branch3.Lanes[0].StartNode));
    }

    # region Helpers

    public bool IsIsolatedNode(Node node)
    {
        return node.Lanes.Count == 1;
    }

    # endregion
}