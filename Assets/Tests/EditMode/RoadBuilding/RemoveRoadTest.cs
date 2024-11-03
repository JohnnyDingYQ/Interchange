using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

public class RemoveRoadTest
{
    float3 stride = Constants.MinLaneLength * new float3(1, 0, 1);
    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
    }

    [Test]
    public void RoadToRemoveNotFound()
    {
        RoadBuilder.Single(0, stride, 2 * stride, 1);
        Road road = new();
        Assert.False(Game.RemoveRoad(road));
        Assert.AreEqual(1, Game.Roads.Count);
    }

    [Test]
    public void RemoveIsolatedRoad()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Assert.True(Game.RemoveRoad(road));
        Assert.AreEqual(0, Game.Roads.Count);
        Assert.AreEqual(0, Game.Nodes.Count);
        Assert.AreEqual(0, Game.Intersections.Count);
        Assert.AreEqual(0, Game.Curves.Count);
    }

    [Test]
    public void RemoveConnectedRoad()
    {
        Road road0 = RoadBuilder.Single(0, stride, 2 * stride, 2);
        Road road1 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 2);
        Assert.AreEqual(6, Game.Nodes.Count);
        Assert.True(Game.RemoveRoad(road0));
        Assert.AreEqual(4, Game.Nodes.Count);
        Assert.Null(road1.Lanes[0].StartNode.InLane);
        Assert.Null(road1.Lanes[1].StartNode.InLane);
    }

    [Test]
    public void RemovePathConnections()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Assert.True(Game.RemoveRoad(road));
        Assert.AreEqual(0, Graph.EdgeCount);
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
        Road road2 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        Assert.True(Game.RemoveRoad(road2));
        float3 updatedLeftEnd = road1.LeftOutline.End.Last();
        float3 updatedRightEnd = road1.RightOutline.End.Last();

        Assert.True(MyNumerics.IsApproxEqual(originalLeftEnd, updatedLeftEnd));
        Assert.True(MyNumerics.IsApproxEqual(originalRightEnd, updatedRightEnd));
    }

    [Test]
    public void RemoveLaneContraction()
    {
        RoadBuilder.Single(0, stride, 2 * stride, 3);
        Road road1 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        Game.RemoveRoad(road1);

        Assert.AreEqual(3, Graph.EdgeCount);
    }

    [Test]
    public void BasicRemoveBranchAtEnd()
    {
        Road road1 = RoadBuilder.Single(0, stride, 2 * stride, 2);
        float3 offset = road1.Lanes[0].EndPos - road1.EndPos;
        Road road2 = RoadBuilder.Single(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Assert.AreEqual(5, Graph.EdgeCount);
        Road road3 = RoadBuilder.Single(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
        Assert.AreEqual(6, Graph.EdgeCount);

        Assert.True(Game.RemoveRoad(road3));
        Assert.True(Graph.ContainsEdge(road1.Lanes[0], road2.Lanes[0]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[1], road2.Lanes[0]));
        Assert.AreEqual(5, Graph.EdgeCount);

        Assert.True(Game.RemoveRoad(road2));
        Assert.AreEqual(2, Graph.EdgeCount);
    }

    [Test]
    public void BasicRemoveBranchAtStart()
    {
        Road road1 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 2);
        float3 offset = road1.Lanes[0].EndPos - road1.EndPos;
        Road road2 = RoadBuilder.Single(0 + offset, stride + offset, 2 * stride + offset, 1);
        Road road3 = RoadBuilder.Single(0 - offset, stride - offset, 2 * stride - offset, 1);

        Assert.True(Game.RemoveRoad(road3));
        Assert.True(Graph.ContainsEdge(road2.Lanes[0], road1.Lanes[0]));
        Assert.True(Graph.ContainsEdge(road2.Lanes[0], road1.Lanes[1]));

        Assert.True(Game.RemoveRoad(road2));
        Assert.AreEqual(2, Graph.EdgeCount);
    }

    [Test]
    public void BasicRemoveIntersection()
    {
        Road road1 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 2);
        Assert.True(Game.RemoveRoad(road1));
        Assert.AreEqual(0, Game.Intersections.Count);
    }

    [Test]
    public void RemoveConnectedRoadCurve()
    {
        Road road0 = RoadBuilder.Single(0, stride, 2 * stride, 3);
        Road road1 = RoadBuilder.Single(0, stride, 2 * stride, 3);

        Assert.True(Game.RemoveRoad(road0));
        Assert.True(Game.RemoveRoad(road1));
        Assert.AreEqual(0, Game.Curves.Count);
    }

    [Test]
    public void CannotRemovePrimaryRoadTwotoOnOne()
    {
        Road two = RoadBuilder.Single(0, stride, 2 * stride, 2);
        RoadBuilder.Single(two.Lanes[0].EndPos, 4 * stride, 6 * stride, 1);
        RoadBuilder.Single(two.Lanes[1].EndPos, 3 * stride, 5 * stride, 1);

        Assert.False(Game.RemoveRoad(two));
    }

    [Test]
    public void RemoveSelectedBranchTwoToOneOne()
    {
        Road two = RoadBuilder.Single(0, stride, 2 * stride, 2);
        Road left = RoadBuilder.Single(two.Lanes[0].EndPos, 4 * stride, 6 * stride, 1);
        Road right = RoadBuilder.Single(two.Lanes[1].EndPos, 3 * stride, 5 * stride, 1);
        
        Game.SelectRoad(two);
        Game.SelectRoad(left);
        Game.SelectRoad(right);
        Game.BulkRemoveSelected();

        Assert.AreEqual(0, Game.Roads.Count);
    }

    [Test]
    public void CanOnlyRemovePlayerBuildRoads()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 2);
        road.RoadProperty = RoadProperty.PlayerBuilt;
        Assert.True(Game.RemoveRoad(road));
    
        road = RoadBuilder.Single(0, stride, 2 * stride, 2);
        road.RoadProperty = RoadProperty.InnateSource;
        Assert.False(Game.RemoveRoad(road));
    
        road = RoadBuilder.Single(0, stride, 2 * stride, 2);
        road.RoadProperty = RoadProperty.InnateTarget;
        Assert.False(Game.RemoveRoad(road));
    
    }
}