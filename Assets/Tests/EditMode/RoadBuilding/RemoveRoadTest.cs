using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

public class RemoveRoadTest
{
    float3 stride = Constants.MinimumLaneLength * new float3(1, 0, 1);
    SortedDictionary<ulong, Road> Roads;
    SortedDictionary<ulong, Node> Nodes;
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
        RoadBuilder.B(0, stride, 2 * stride, 1);
        Road road = new();
        Assert.False(Game.RemoveRoad(road));
        Assert.AreEqual(1, Roads.Count);
    }

    [Test]
    public void RemoveIsolatedRoad()
    {
        Road road = RoadBuilder.B(0, stride, 2 * stride, 1);
        Assert.True(Game.RemoveRoad(road));
        Assert.AreEqual(0, Roads.Count);
        Assert.AreEqual(0, Nodes.Count);
    }

    [Test]
    public void RemoveConnectedRoad()
    {
        Road road0 = RoadBuilder.B(0, stride, 2 * stride, 2);
        Road road1 = RoadBuilder.B(2 * stride, 3 * stride, 4 * stride, 2);
        Assert.AreEqual(6, Nodes.Count);
        Assert.True(Game.RemoveRoad(road0));
        Assert.AreEqual(4, Nodes.Count);
        Assert.True(road0.Lanes[0].EndNode.Lanes.SetEquals(new HashSet<Lane> { road1.Lanes[0] }));
        Assert.True(road0.Lanes[1].EndNode.Lanes.SetEquals(new HashSet<Lane> { road1.Lanes[1] }));
    }

    [Test]
    public void RemovePathConnections()
    {
        Road road = RoadBuilder.B(0, stride, 2 * stride, 1);
        Assert.True(Game.RemoveRoad(road));
        Assert.True(Game.Graph.IsEdgesEmpty);
    }

    [Test]
    public void RemoveVertex()
    {
        Road road = RoadBuilder.B(0, stride, 2 * stride, 1);
        Assert.True(Game.RemoveRoad(road));
        Assert.True(Game.Graph.IsVerticesEmpty);
    }

    [Test]
    public void ReevaluateConnectedRoadOutlines()
    {
        Road road1 = RoadBuilder.B(0, stride, 2 * stride, 3);
        float3 originalLeftEnd = road1.LeftOutline.End.Last();
        float3 originalRightEnd = road1.RightOutline.End.Last();
        Road road2 = RoadBuilder.B(2* stride, 3 * stride, 4 * stride, 1);
        Assert.True(Game.RemoveRoad(road2));
        float3 updatedLeftEnd = road1.LeftOutline.End.Last();
        float3 updatedRightEnd = road1.RightOutline.End.Last();
        
        Assert.True(MyNumerics.AreNumericallyEqual(originalLeftEnd, updatedLeftEnd));
        Assert.True(MyNumerics.AreNumericallyEqual(originalRightEnd, updatedRightEnd));
    }

    [Test]
    public void BasicRemoveBranchAtEnd()
    {
        Road road1 = RoadBuilder.B(0, stride, 2 * stride, 2);
        float3 offset = road1.Lanes[0].EndPos - road1.EndPos;
        Road road2 = RoadBuilder.B(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Road road3 = RoadBuilder.B(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);

        Assert.True(Game.RemoveRoad(road3));
        Assert.True(Game.HasEdge(road1.Lanes[0], road2.Lanes[0]));
        Assert.True(Game.HasEdge(road1.Lanes[1], road2.Lanes[0]));

        Assert.True(Game.RemoveRoad(road2));
        Assert.AreEqual(2, Game.Graph.EdgeCount);
    }

    [Test]
    public void BasicRemoveBranchAtStart()
    {
        Road road1 = RoadBuilder.B(2 * stride, 3 * stride, 4 * stride, 2);
        float3 offset = road1.Lanes[0].EndPos - road1.EndPos;
        Road road2 = RoadBuilder.B(0 + offset, stride + offset, 2 * stride + offset, 1);
        Road road3 = RoadBuilder.B(0 - offset, stride - offset, 2 * stride - offset, 1);

        Assert.True(Game.RemoveRoad(road3));
        Assert.True(Game.HasEdge(road2.Lanes[0], road1.Lanes[0]));
        Assert.True(Game.HasEdge(road2.Lanes[0], road1.Lanes[1]));

        Assert.True(Game.RemoveRoad(road2));
        Assert.AreEqual(2, Game.Graph.EdgeCount);
    }

    [Test]
    public void BasicRemoveIntersection()
    {
        Road road1 = RoadBuilder.B(2 * stride, 3 * stride, 4 * stride, 2);
        Assert.True(Game.RemoveRoad(road1));
    }
}