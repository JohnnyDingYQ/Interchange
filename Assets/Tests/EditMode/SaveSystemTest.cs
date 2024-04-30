using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using QuikGraph;
using QuikGraph.Collections;
using Unity.Mathematics;
using UnityEngine;
public class SaveSystemTest
{
    float3 stride = Constants.MinimumLaneLength * new float3(1, 0, 1);

    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
    }

    [Test]
    public void RecoverSingleOneLaneRoad()
    {
        RoadBuilder.B(0, stride, 2 * stride, 1);
        SaveSystem.SaveGame();
        Game.WipeState();
        SaveSystem.LoadGame();

        Assert.AreEqual(1, Game.Roads.Count);
        Road road = Game.Roads.Values.First();
        Assert.AreEqual(new float3(0), road.StartPos);
        Assert.AreEqual(2 * stride, road.EndPos);
        Assert.AreEqual(1, road.Lanes.Count);
        Lane lane = road.Lanes.First();
        Assert.AreEqual(new float3(0), lane.StartPos);
        Assert.AreEqual(2 * stride, lane.EndPos);
        HashSet<Lane> expected = new() { lane };
        Assert.AreEqual(2, Game.Nodes.Count);
        Assert.True(lane.StartNode.Lanes.SetEquals(expected));
        Assert.True(lane.EndNode.Lanes.SetEquals(expected));
    }

    [Test]
    public void RecoverSingleThreeLaneRoad()
    {
        Road r = RoadBuilder.B(0, stride, 2 * stride, 3);
        SaveSystem.SaveGame();
        Game.WipeState();
        SaveSystem.LoadGame();

        Assert.AreEqual(1, Game.Roads.Count);
        Road road = Game.Roads.Values.First();
        Assert.AreEqual(new float3(0), road.StartPos);;
        Assert.AreEqual(2 * stride, road.EndPos);
        Assert.AreEqual(3, road.Lanes.Count);
        Lane lane = road.Lanes[1];
        Assert.AreEqual(new float3(0), lane.StartPos);
        Assert.AreEqual(2 * stride, lane.EndPos);
        Assert.AreEqual(6, Game.Nodes.Count);
        Assert.True(road.Lanes[0].StartNode.Lanes.SetEquals(new HashSet<Lane> { road.Lanes[0] }));
        Assert.True(road.Lanes[0].EndNode.Lanes.SetEquals(new HashSet<Lane> { road.Lanes[0] }));
        Assert.True(road.Lanes[1].StartNode.Lanes.SetEquals(new HashSet<Lane> { road.Lanes[1] }));
        Assert.True(road.Lanes[1].EndNode.Lanes.SetEquals(new HashSet<Lane> { road.Lanes[1] }));
        Assert.True(road.Lanes[2].StartNode.Lanes.SetEquals(new HashSet<Lane> { road.Lanes[2] }));
        Assert.True(road.Lanes[2].EndNode.Lanes.SetEquals(new HashSet<Lane> { road.Lanes[2] }));

    }

    [Test]
    public void RecoverTwoDisconnectedOneLaneRoad()
    {
        Road road0 = RoadBuilder.B(0, stride, 2 * stride, 1);
        Road road1 = RoadBuilder.B(3 * stride, 4 * stride, 5 * stride, 1);

        SaveSystem.SaveGame();
        Game.WipeState();
        SaveSystem.LoadGame();

        Assert.AreEqual(2, Game.Roads.Count);
        Assert.AreEqual(new float3(0), road0.StartPos);
        Assert.AreEqual(2 * stride, road0.EndPos);
        Assert.AreEqual(3 * stride, road1.StartPos);
        Assert.AreEqual(5 * stride, road1.EndPos);
        Assert.AreEqual(4, Game.Nodes.Count);
        Assert.True(road0.Lanes[0].StartNode.Lanes.SetEquals(new HashSet<Lane> { road0.Lanes[0] }));
        Assert.True(road0.Lanes[0].EndNode.Lanes.SetEquals(new HashSet<Lane> { road0.Lanes[0] }));
        Assert.True(road1.Lanes[0].StartNode.Lanes.SetEquals(new HashSet<Lane> { road1.Lanes[0] }));
        Assert.True(road1.Lanes[0].EndNode.Lanes.SetEquals(new HashSet<Lane> { road1.Lanes[0] }));
    }

    [Test]
    public void RecoverTwoConnectedOneLaneRoad()
    {
        Road road0 = RoadBuilder.B(0, stride, 2 * stride, 1);
        Road road1 = RoadBuilder.B(2 * stride, 3 * stride, 4 * stride, 1);

        SaveSystem.SaveGame();
        Game.WipeState();
        SaveSystem.LoadGame();

        Assert.AreEqual(2, Game.Roads.Count);
        Assert.AreEqual(new float3(0), road0.StartPos);
        Assert.AreEqual(2 * stride, road0.EndPos);
        Assert.AreEqual(2 * stride, road1.StartPos);
        Assert.AreEqual(4 * stride, road1.EndPos);
        Assert.AreEqual(3, Game.Nodes.Count);
        Assert.True(road0.Lanes[0].StartNode.Lanes.SetEquals(new HashSet<Lane> { road0.Lanes[0] }));
        Assert.True(road0.Lanes[0].EndNode.Lanes.SetEquals(new HashSet<Lane> { road0.Lanes[0], road1.Lanes[0] }));
        Assert.True(road1.Lanes[0].StartNode.Lanes.SetEquals(new HashSet<Lane> { road0.Lanes[0], road1.Lanes[0] }));
        Assert.True(road1.Lanes[0].EndNode.Lanes.SetEquals(new HashSet<Lane> { road1.Lanes[0] }));
    }

    [Test]
    public void RecoverLanesSplines()
    {
        RoadBuilder.B(0, stride, 2 * stride, 1);
        RoadBuilder.B(2 * stride, 3 * stride, 4 * stride, 1);
        SaveSystem.SaveGame();
        Game.WipeState();
        SaveSystem.LoadGame();

        foreach (Road road in Game.Roads.Values)
        {
            Assert.NotNull(road.BezierSeries);
            foreach (Lane lane in road.Lanes)
                Assert.NotNull(lane.BezierSeries);
        }

    }

    [Test]
    public void RecoverPathGraph()
    {
        RoadBuilder.B(0, stride, 2 * stride, 1);
        RoadBuilder.B(2 * stride, 3 * stride, 4 * stride, 1);
        SaveSystem.SaveGame();
        Game.WipeState();
        SaveSystem.LoadGame();

        Assert.True(!Game.Graph.IsVerticesEmpty);
        Assert.True(!Game.Graph.IsEdgesEmpty);
        foreach (Path path in Game.Graph.Edges)
            Assert.NotNull(path.BezierSeries);
    }

    [Test]
    public void RecoverNodeLaneDirections()
    {
        Road road = RoadBuilder.B(0, stride, 2 * stride, 1);
        Lane lane = road.Lanes.First();
        Assert.AreSame(lane, lane.StartNode.GetLanes(Direction.Out).First());
        Assert.AreSame(lane, lane.EndNode.GetLanes(Direction.In).First());
        SaveSystem.SaveGame();
        Game.WipeState();
        SaveSystem.LoadGame();
        road = Game.Roads.Values.First();
        lane = road.Lanes.First();
        Assert.AreSame(lane, lane.StartNode.GetLanes(Direction.Out).First());
        Assert.AreSame(lane, lane.EndNode.GetLanes(Direction.In).First());

    }

    [Test]
    public void RecoverRoadOutline()
    {
        Road saved1 = RoadBuilder.B(0, stride, 2 * stride, 1);
        Road saved2 = RoadBuilder.B(2 * stride, 3 * stride, 4 * stride, 1);
        Road saved3 = RoadBuilder.B(4 * stride, 5 * stride, 6 * stride, 1);
        SaveSystem.SaveGame();
        Game.WipeState();
        SaveSystem.LoadGame();
        Road restored1 = Game.Roads[saved1.Id];
        Road restored2 = Game.Roads[saved2.Id];
        Road restored3 = Game.Roads[saved3.Id];

        Assert.True(saved1.LeftOutline.Equals(restored1.LeftOutline));
        Assert.True(saved1.RightOutline.Equals(restored1.RightOutline));
        Assert.True(saved2.LeftOutline.Equals(restored2.LeftOutline));
        Assert.True(saved2.RightOutline.Equals(restored2.RightOutline));
        Assert.True(saved3.LeftOutline.Equals(restored3.LeftOutline));
        Assert.True(saved3.RightOutline.Equals(restored3.RightOutline));
    }

    [Test]
    public void RecoverConnectedLanes_2to1and1()
    {
        Road saved1 = RoadBuilder.B(0, stride, 2 * stride, 2);
        float3 offset = saved1.Lanes[0].EndPos - saved1.EndPos;
        Road saved2 = RoadBuilder.B(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Road saved3 = RoadBuilder.B(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
        SaveSystem.SaveGame();
        Game.WipeState();
        SaveSystem.LoadGame();
        Road restored1 = Game.Roads[saved1.Id];
        Road restored2 = Game.Roads[saved2.Id];
        Road restored3 = Game.Roads[saved3.Id];

        Assert.True(Game.HasEdge(restored1.Lanes[0], restored2.Lanes[0]));
        Assert.True(Game.HasEdge(restored1.Lanes[1], restored3.Lanes[0]));
        Assert.AreEqual(2, restored1.Lanes[0].EndNode.Lanes.Count);
        Assert.AreEqual(2, restored1.Lanes[1].EndNode.Lanes.Count);
    }

    [Test]
    public void RecoverIntersectionNormal()
    {
        RoadBuilder.B(0, stride, 2 * stride, 2);
        SaveSystem.SaveGame();
        Game.WipeState();
        SaveSystem.LoadGame();

        Intersection i = Game.Roads.Values.First().EndIntersection;
        Assert.NotNull(i.Normal);
        Assert.NotNull(i.Tangent);
        Assert.NotNull(i.PointOnInSide);
    }

    // TODO: Complete further testing

}