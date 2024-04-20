using System.Collections.Generic;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

public class RemoveRoadTest
{
    float3 direction = Constants.MinimumLaneLength * new float3(1, 0, 1);
    SortedDictionary<int, Road> Roads;
    SortedDictionary<int, Node> Nodes;
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
        RoadBuilder.Build(0, direction, 2 * direction, 1);
        Road road = new();
        Assert.False(Game.RemoveRoad(road));
        Assert.AreEqual(1, Roads.Count);
    }

    [Test]
    public void RemoveIsolatedRoad()
    {
        Road road = RoadBuilder.Build(0, direction, 2 * direction, 1);
        Assert.True(Game.RemoveRoad(road));
        Assert.AreEqual(0, Roads.Count);
        Assert.AreEqual(0, Nodes.Count);
    }

    [Test]
    public void RemoveConnectedRoad()
    {
        Road road0 = RoadBuilder.Build(0, direction, 2 * direction, 2);
        Road road1 = RoadBuilder.Build(2 * direction, 3 * direction, 4 * direction, 2);
        Assert.AreEqual(6, Nodes.Count);
        Assert.True(Game.RemoveRoad(road0));
        Assert.AreEqual(4, Nodes.Count);
        Assert.True(road0.Lanes[0].EndNode.Lanes.SetEquals(new HashSet<Lane> { road1.Lanes[0] }));
        Assert.True(road0.Lanes[1].EndNode.Lanes.SetEquals(new HashSet<Lane> { road1.Lanes[1] }));
    }

    [Test]
    public void RemovePathConnections()
    {
        Road road = RoadBuilder.Build(0, direction, 2 * direction, 1);
        Assert.True(Game.RemoveRoad(road));
        Assert.True(Game.Graph.IsEdgesEmpty);
    }

    [Test]
    public void RemoveVertex()
    {
        Road road = RoadBuilder.Build(0, direction, 2 * direction, 1);
        Assert.True(Game.RemoveRoad(road));
        Assert.True(Game.Graph.IsVerticesEmpty);
    }
}