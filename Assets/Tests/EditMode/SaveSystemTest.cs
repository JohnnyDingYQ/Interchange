using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEditor.VersionControl;

public class SaveSystemTest
{

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Grid.Height = 100;
        Grid.Width = 200;
        Grid.Dim = 1;
        Grid.Level = 0;
        Game.SaveSystem = new SaveSystem();
    }

    [SetUp]
    public void SetUp()
    {
        BuildManager.Reset();
    }

    [Test]
    public void RecoverSingleOneLaneRoad()
    {
        BuildManager.Client = new MockClient(new List<int>() { 30, 60, 90 });
        for (int i = 0; i < 3; i++)
            BuildManager.HandleBuildCommand();
        Game.SaveGame();
        BuildManager.Reset();
        Game.LoadGame();

        Assert.AreEqual(1, BuildManager.RoadWatcher.Count);
        Road road = BuildManager.RoadWatcher.Values.First();
        Assert.AreEqual(30, road.StartNode);
        Assert.AreEqual(60, road.PivotNode);
        Assert.AreEqual(90, road.EndNode);
        Assert.AreEqual(1, road.Lanes.Count);
        Lane lane = road.Lanes.First();
        Assert.AreEqual(30, lane.Start);
        Assert.AreEqual(90, lane.End);
        Assert.NotNull(road.StartIx);
        Assert.NotNull(road.EndIx);
        Intersection startIx = road.StartIx;
        Intersection endIx = road.EndIx;
        Assert.AreSame(road, startIx.Roads.First());
        Assert.AreSame(road, endIx.Roads.First());
        Assert.IsTrue(startIx.NodeWithLane.ContainsKey(30));
        Assert.IsTrue(endIx.NodeWithLane.ContainsKey(90));
        HashSet<Lane> expected = new() {lane};
        Assert.True(startIx.NodeWithLane[30].SetEquals(expected));
        Assert.True(endIx.NodeWithLane[90].SetEquals(expected));
    }

    [Test]
    public void RecoverSingleThreeLaneRoad()
    {
        BuildManager.Client = new MockClient(new List<int>() { 30, 60, 90 });
        BuildManager.LaneCount = 3;
        for (int i = 0; i < 3; i++)
            BuildManager.HandleBuildCommand();
        Game.SaveGame();
        BuildManager.Reset();
        Game.LoadGame();

        Assert.AreEqual(1, BuildManager.RoadWatcher.Count);
        Road road = BuildManager.RoadWatcher.Values.First();
        Assert.AreEqual(30, road.StartNode);
        Assert.AreEqual(60, road.PivotNode);
        Assert.AreEqual(90, road.EndNode);
        Assert.AreEqual(3, road.Lanes.Count);
        Lane lane = road.Lanes[1];
        Assert.AreEqual(30, lane.Start);
        Assert.AreEqual(90, lane.End);
        Assert.NotNull(road.StartIx);
        Assert.NotNull(road.EndIx);
        Intersection startIx = road.StartIx;
        Intersection endIx = road.EndIx;
        Assert.AreSame(road, startIx.Roads.First());
        Assert.AreSame(road, endIx.Roads.First());
        Assert.IsTrue(startIx.NodeWithLane.ContainsKey(30));
        Assert.IsTrue(endIx.NodeWithLane.ContainsKey(90));
        Assert.AreEqual(3, startIx.NodeWithLane.Count);
        Assert.AreEqual(3, endIx.NodeWithLane.Count);
        HashSet<Lane> expected = new() {lane};
        Assert.True(startIx.NodeWithLane[30].SetEquals(expected));
        Assert.True(endIx.NodeWithLane[90].SetEquals(expected));
    }

    [Test]
    public void RecoverTwoDisconnectedOneLaneRoad()
    {
        BuildManager.Client = new MockClient(new List<int>() { 30, 60, 90, 120, 150, 180 });
        for (int i = 0; i < 6; i++)
            BuildManager.HandleBuildCommand();
        Game.SaveGame();
        BuildManager.Reset();
        Game.LoadGame();

        Assert.AreEqual(2, BuildManager.RoadWatcher.Count);
        Road road0 = BuildManager.RoadWatcher[0];
        Assert.AreEqual(30, road0.StartNode);
        Assert.AreEqual(60, road0.PivotNode);
        Assert.AreEqual(90, road0.EndNode);
        Road road1 = BuildManager.RoadWatcher[1];
        Assert.AreEqual(120, road1.StartNode);
        Assert.AreEqual(150, road1.PivotNode);
        Assert.AreEqual(180, road1.EndNode);
    }

    [Test]
    public void RecoverTwoConnectedOneLaneRoad()
    {
        BuildManager.Client = new MockClient(new List<int>() { 30, 60, 90, 90, 120, 150 });
        for (int i = 0; i < 6; i++)
            BuildManager.HandleBuildCommand();
        Game.SaveGame();
        BuildManager.Reset();
        Game.LoadGame();

        Assert.AreEqual(2, BuildManager.RoadWatcher.Count);
        Road road0 = BuildManager.RoadWatcher[0];
        Assert.AreEqual(30, road0.StartNode);
        Assert.AreEqual(60, road0.PivotNode);
        Assert.AreEqual(90, road0.EndNode);
        Road road1 = BuildManager.RoadWatcher[1];
        Assert.AreEqual(90, road1.StartNode);
        Assert.AreEqual(120, road1.PivotNode);
        Assert.AreEqual(150, road1.EndNode);
        Intersection ix = road0.EndIx;
        Assert.AreSame(ix, road1.StartIx);
        Assert.AreEqual(2, ix.Roads.Count);
        Assert.AreEqual(1, ix.NodeWithLane.Count);
        Lane lane0 = road0.Lanes[0];
        Lane lane1 = road1.Lanes[0];
        Assert.True(ix.NodeWithLane[90].SetEquals(new HashSet<Lane>() {lane0, lane1}));
    }

    // TODO: Complete further testing
    // [Test]
    public void RecoverConnectedLanes_ThreetoOneTwo()
    {
        BuildManager.Client = new MockClient(new List<int>() { 30, 60, 90, 90, 120, 150, 90, 130, 160 });
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
        
        Assert.AreEqual(3, BuildManager.RoadWatcher.Count);
        Road road0 = BuildManager.RoadWatcher[0];
        Road road1 = BuildManager.RoadWatcher[1];
        Road road2 = BuildManager.RoadWatcher[2];
        Intersection ix = road0.EndIx;
        Assert.AreSame(ix, road1.StartIx);
        Assert.AreSame(ix, road2.StartIx);
        Assert.AreSame(ix.GetMainRoad(), road0);
        Assert.True(ix.NodeWithLane[90].SetEquals(new HashSet<Lane>() {road0.Lanes[1], road2.Lanes[0]}));
    }

    private class MockClient : IBuildManagerBoundary
    {
        readonly List<int> MockID;
        int count = 0;

        public MockClient(List<int> mockPos)
        {
            MockID = mockPos;
        }

        public MockClient(List<float2> mockCoord)
        {
            List<int> m = new();
            foreach (float2 coord in mockCoord)
                m.Add((int)(Grid.Height * coord.y + coord.x));
            MockID = m;
        }

        public void EvaluateIntersection(Intersection intersection)
        {
            return;
        }

        public float3 GetPos()
        {
            return Grid.GetPosByID(MockID[count++]);
        }

        public void InstantiateRoad(Road road)
        {
            return;
        }
    }
}