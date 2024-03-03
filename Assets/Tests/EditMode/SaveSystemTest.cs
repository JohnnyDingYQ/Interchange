using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
public class SaveSystemTest
{
    Vector3 pos1 = new(10, 10, 10);
    Vector3 pos2 = new(30, 12, 30);
    Vector3 pos3 = new(60, 14, 60);
    Vector3 pos4 = new(90, 16, 90);
    Vector3 pos5 = new(120, 16, 120);
    Vector3 pos6 = new(150, 16, 150);

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Game.SaveSystem = new SaveSystem();
    }

    [SetUp]
    public void SetUp()
    {
        BuildManager.Reset();
        Game.WipeGameState();
    }

    [Test]
    public void RecoverSingleOneLaneRoad()
    {
        BuildManager.Client = new MockClient(new List<float3>() { pos1, pos2, pos3 });
        for (int i = 0; i < 3; i++)
            BuildManager.HandleBuildCommand();
        Game.SaveGame();
        BuildManager.Reset();
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
        BuildManager.Client = new MockClient(new List<float3>() { pos1, pos2, pos3 });
        BuildManager.LaneCount = 3;
        for (int i = 0; i < 3; i++)
            BuildManager.HandleBuildCommand();
        Game.SaveGame();
        BuildManager.Reset();
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
        BuildManager.Client = new MockClient(new List<float3>() { pos1, pos2, pos3, pos4, pos5, pos6 });
        for (int i = 0; i < 6; i++)
            BuildManager.HandleBuildCommand();
        Game.SaveGame();
        BuildManager.Reset();
        Game.LoadGame();

        Assert.AreEqual(2, Game.Roads.Count);
        Road road0 = Game.Roads[0];
        Assert.AreEqual(pos1, road0.StartPos);
        Assert.AreEqual(pos2, road0.PivotPos);
        Assert.AreEqual(pos3, road0.EndPos);
        Road road1 = Game.Roads[1];
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
        BuildManager.Client = new MockClient(new List<float3>() { pos1, pos2, pos3, pos3, pos4, pos5 });
        for (int i = 0; i < 6; i++)
            BuildManager.HandleBuildCommand();
        Game.SaveGame();
        BuildManager.Reset();
        Game.LoadGame();

        Assert.AreEqual(2, Game.Roads.Count);
        Road road0 = Game.Roads[0];
        Assert.AreEqual(pos1, road0.StartPos);
        Assert.AreEqual(pos2, road0.PivotPos);
        Assert.AreEqual(pos3, road0.EndPos);
        Road road1 = Game.Roads[1];
        Assert.AreEqual(pos3, road1.StartPos);
        Debug.Log(pos4);
        Debug.Log(road1.PivotPos);
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
        BuildManager.Client = new MockClient(new List<float3>() { pos1, pos2, pos3, pos3, pos4, pos5 });
        for (int i = 0; i < 6; i++)
            BuildManager.HandleBuildCommand();
        Game.SaveGame();
        BuildManager.Reset();
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
        BuildManager.Client = new MockClient(new List<float3>() { pos1, pos2, pos3, pos3, pos4, pos5, pos3, 130, 160 });
        BuildManager.LaneCount = 3;
        for (int i = 0; i < 3; i++)
            BuildManager.HandleBuildCommand();
        BuildManager.LaneCount = 1;
        for (int i = 0; i < 3; i++)
            BuildManager.HandleBuildCommand();
        BuildManager.LaneCount = 2;
        for (int i = 0; i < 3; i++)
            BuildManager.HandleBuildCommand();
        Game.SaveGame();
        BuildManager.Reset();
        Game.LoadGame();

        Assert.AreEqual(3, Game.Roads.Count);
        Road road0 = Game.Roads[0];
        Road road1 = Game.Roads[1];
        Road road2 = Game.Roads[2];
    }

    private class MockClient : IBuildManagerBoundary
    {
        readonly List<float3> MockPos;
        int count = 0;

        public MockClient(List<float3> mockCoord)
        {
            MockPos = mockCoord;
        }

        public float3 GetPos()
        {
            return MockPos[count++];
        }

        public void InstantiateRoad(Road road)
        {
            return;
        }

        public void RedrawAllRoads()
        {
            throw new System.NotImplementedException();
        }
    }

}