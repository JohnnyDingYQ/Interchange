using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
public class SaveSystemTest
{
    float3 pos1 = new(0, 0, 0);
    float3 pos2 = new(GConsts.MinimumRoadLength, 0, 0);
    float3 pos3 = new(GConsts.MinimumRoadLength * 2, 0, 0);
    float3 pos4 = new(GConsts.MinimumRoadLength * 3, 0, 0);
    float3 pos5 = new(GConsts.MinimumRoadLength * 4, 0, 0);
    float3 pos6 = new(GConsts.MinimumRoadLength * 5, 0, 0);

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Game.SaveSystem = new SaveSystemImpl();
    }

    [SetUp]
    public void SetUp()
    {
        BuildHandler.Reset();
        Game.WipeState();
    }

    [Test]
    public void RecoverSingleOneLaneRoad()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 1);
        Game.SaveGame();
        BuildHandler.Reset();
        Game.LoadGame();

        Assert.AreEqual(1, Game.Roads.Count);
        Road road = Game.Roads.Values.First();
        Assert.AreEqual(pos1, road.StartPos);
        Assert.AreEqual(pos2, road.PivotPos);
        Assert.AreEqual(pos3, road.EndPos);
        Assert.AreEqual(1, road.Lanes.Count);
        Lane lane = road.Lanes.First();
        Assert.AreEqual(pos1, lane.StartPos);
        Assert.AreEqual(pos3, lane.EndPos);
        HashSet<Lane> expected = new() { lane };
        Assert.AreEqual(2, Game.Nodes.Count);
        Assert.True(lane.StartNode.Lanes.SetEquals(expected));
        Assert.True(lane.EndNode.Lanes.SetEquals(expected));
    }

    [Test]
    public void RecoverSingleThreeLaneRoad()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 3);
        Game.SaveGame();
        BuildHandler.Reset();
        Game.LoadGame();

        Assert.AreEqual(1, Game.Roads.Count);
        Road road = Game.Roads.Values.First();
        Assert.AreEqual(pos1, road.StartPos);
        Assert.AreEqual(pos2, road.PivotPos);
        Assert.AreEqual(pos3, road.EndPos);
        Assert.AreEqual(3, road.Lanes.Count);
        Lane lane = road.Lanes[1];
        Assert.AreEqual(pos1, lane.StartPos);
        Assert.AreEqual(pos3, lane.EndPos);
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
        Road road0 = RoadBuilder.BuildRoad(pos1, pos2, pos3, 1);
        Road road1 = RoadBuilder.BuildRoad(pos4, pos5, pos6, 1);
        
        Game.SaveGame();
        BuildHandler.Reset();
        Game.LoadGame();

        Assert.AreEqual(2, Game.Roads.Count);
        Assert.AreEqual(pos1, road0.StartPos);
        Assert.AreEqual(pos2, road0.PivotPos);
        Assert.AreEqual(pos3, road0.EndPos);
        Assert.AreEqual(pos4, road1.StartPos);
        Assert.AreEqual(pos5, road1.PivotPos);
        Assert.AreEqual(pos6, road1.EndPos);
        Assert.AreEqual(4, Game.Nodes.Count);
        Assert.True(road0.Lanes[0].StartNode.Lanes.SetEquals(new HashSet<Lane> { road0.Lanes[0] }));
        Assert.True(road0.Lanes[0].EndNode.Lanes.SetEquals(new HashSet<Lane> { road0.Lanes[0] }));
        Assert.True(road1.Lanes[0].StartNode.Lanes.SetEquals(new HashSet<Lane> { road1.Lanes[0] }));
        Assert.True(road1.Lanes[0].EndNode.Lanes.SetEquals(new HashSet<Lane> { road1.Lanes[0] }));
    }

    [Test]
    public void RecoverTwoConnectedOneLaneRoad()
    {
        Road road0 = RoadBuilder.BuildRoad(pos1, pos2, pos3, 1);
        Road road1 = RoadBuilder.BuildRoad(pos3, pos4, pos5, 1);
        
        Game.SaveGame();
        BuildHandler.Reset();
        Game.LoadGame();

        Assert.AreEqual(2, Game.Roads.Count);
        Assert.AreEqual(pos1, road0.StartPos);
        Assert.AreEqual(pos2, road0.PivotPos);
        Assert.AreEqual(pos3, road0.EndPos);
        Assert.AreEqual(pos3, road1.StartPos);
        Assert.True(Vector3.Distance(pos4, road1.PivotPos) < 0.01f);
        Assert.AreEqual(pos5, road1.EndPos);
        Assert.AreEqual(3, Game.Nodes.Count);
        Assert.True(road0.Lanes[0].StartNode.Lanes.SetEquals(new HashSet<Lane> { road0.Lanes[0] }));
        Assert.True(road0.Lanes[0].EndNode.Lanes.SetEquals(new HashSet<Lane> { road0.Lanes[0], road1.Lanes[0] }));
        Assert.True(road1.Lanes[0].StartNode.Lanes.SetEquals(new HashSet<Lane> { road0.Lanes[0], road1.Lanes[0] }));
        Assert.True(road1.Lanes[0].EndNode.Lanes.SetEquals(new HashSet<Lane> { road1.Lanes[0] }));
    }

    [Test]
    public void RecoverLanesSplines()
    {
        RoadBuilder.BuildRoad(pos1, pos2, pos3, 1);
        RoadBuilder.BuildRoad(pos3, pos4, pos5, 1);
        Game.SaveGame();
        BuildHandler.Reset();
        Game.LoadGame();

        foreach (Road road in Game.Roads.Values)
        {
            Assert.True(road.Curve != null);
            foreach (Lane lane in road.Lanes)
                Assert.True(lane.Spline != null);
        }
            
    }

    // TODO: Complete further testing
    // [Test]
    public void RecoverConnectedLanes_ThreetoOneTwo()
    {

    }

}