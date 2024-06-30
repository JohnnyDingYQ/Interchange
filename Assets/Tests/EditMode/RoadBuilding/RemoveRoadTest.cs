using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

public class RemoveRoadTest
{
    float3 stride = Constants.MinLaneLength * new float3(1, 0, 1);
    Dictionary<uint, Road> Roads;
    Dictionary<uint, Node> Nodes;
    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
        Roads = Game.Roads;
        Nodes = Game.Nodes;
    }

    [Test]
    public void RoadToRemoveNotFound()
    {
        RoadBuilder.Single(0, stride, 2 * stride, 1);
        Road road = new();
        Assert.False(Game.RemoveRoad(road));
        Assert.AreEqual(1, Roads.Count);
    }

    [Test]
    public void RemoveIsolatedRoad()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Assert.True(Game.RemoveRoad(road));
        Assert.AreEqual(0, Roads.Count);
        Assert.AreEqual(0, Nodes.Count);
    }

    [Test]
    public void RemoveConnectedRoad()
    {
        Road road0 = RoadBuilder.Single(0, stride, 2 * stride, 2);
        Road road1 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 2);
        Assert.AreEqual(6, Nodes.Count);
        Assert.True(Game.RemoveRoad(road0));
        Assert.AreEqual(4, Nodes.Count);
        Assert.True(road0.Lanes[0].EndNode.Lanes.SetEquals(new HashSet<Lane> { road1.Lanes[0] }));
        Assert.True(road0.Lanes[1].EndNode.Lanes.SetEquals(new HashSet<Lane> { road1.Lanes[1] }));
    }

    [Test]
    public void RemovePathConnections()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Assert.True(Game.RemoveRoad(road));
        Assert.AreEqual(0, Game.Paths.Count);
    }

    [Test]
    public void RemoveVertex()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Assert.True(Game.RemoveRoad(road));
        Assert.AreEqual(0, Game.Vertices.Count);
    }

    [Test]
    public void ReevaluateConnectedRoadOutlines()
    {
        Road road1 = RoadBuilder.Single(0, stride, 2 * stride, 3);
        float3 originalLeftEnd = road1.LeftOutline.End.Last();
        float3 originalRightEnd = road1.RightOutline.End.Last();
        Road road2 = RoadBuilder.Single(2* stride, 3 * stride, 4 * stride, 1);
        Assert.True(Game.RemoveRoad(road2));
        float3 updatedLeftEnd = road1.LeftOutline.End.Last();
        float3 updatedRightEnd = road1.RightOutline.End.Last();
        
        Assert.True(MyNumerics.AreNumericallyEqual(originalLeftEnd, updatedLeftEnd));
        Assert.True(MyNumerics.AreNumericallyEqual(originalRightEnd, updatedRightEnd));
    }

    [Test]
    public void RemoveLaneContraction()
    {
        Road road0 = RoadBuilder.Single(0, stride, 2 * stride, 3);
        Road road1 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        Game.RemoveRoad(road1);

        Assert.AreEqual(3, Game.Paths.Count);
    }

    [Test]
    public void BasicRemoveBranchAtEnd()
    {
        Road road1 = RoadBuilder.Single(0, stride, 2 * stride, 2);
        float3 offset = road1.Lanes[0].EndPos - road1.EndPos;
        Road road2 = RoadBuilder.Single(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Assert.AreEqual(5, Game.Paths.Count);
        Road road3 = RoadBuilder.Single(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
        Assert.AreEqual(6, Game.Paths.Count);

        Assert.True(Game.RemoveRoad(road3));
        Assert.True(Graph.ContainsPath(road1.Lanes[0], road2.Lanes[0]));
        Assert.True(Graph.ContainsPath(road1.Lanes[1], road2.Lanes[0]));
        Assert.AreEqual(5, Game.Paths.Count);

        Assert.True(Game.RemoveRoad(road2));
        Assert.AreEqual(2, Game.Paths.Count);
    }

    [Test]
    public void BasicRemoveBranchAtStart()
    {
        Road road1 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 2);
        float3 offset = road1.Lanes[0].EndPos - road1.EndPos;
        Road road2 = RoadBuilder.Single(0 + offset, stride + offset, 2 * stride + offset, 1);
        Road road3 = RoadBuilder.Single(0 - offset, stride - offset, 2 * stride - offset, 1);

        Assert.True(Game.RemoveRoad(road3));
        Assert.True(Graph.ContainsPath(road2.Lanes[0], road1.Lanes[0]));
        Assert.True(Graph.ContainsPath(road2.Lanes[0], road1.Lanes[1]));

        Assert.True(Game.RemoveRoad(road2));
        Assert.AreEqual(2, Game.Paths.Count);
    }

    [Test]
    public void BasicRemoveIntersection()
    {
        Road road1 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 2);
        Assert.True(Game.RemoveRoad(road1));
    }
}