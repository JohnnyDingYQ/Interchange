using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.model.Roads;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

public class ZonePathTest
{
    float3 stride = Constants.MinLaneLength * new float3(1, 0, 0);

    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
        for (uint i = 1; i < 4; i++)
            Game.Zones.Add(i, new(i));
    }

    [Test]
    public void ConnectRoadEndToZone()
    {
        Build.HandleBuildCommand(0);
        Build.HandleBuildCommand(stride);
        Game.HoveredZone = Game.Zones[1];
        List<Road> roads = Build.HandleBuildCommand(2 * stride);

        Assert.AreEqual(roads.Last().Lanes.Single().EndVertex, Game.Zones[1].Vertices.Single());
    }

    [Test]
    public void ConnectRoadStartToZone()
    {
        Game.HoveredZone = Game.Zones[1];
        Build.HandleBuildCommand(0);
        Game.HoveredZone = null;
        Build.HandleBuildCommand(stride);
        List<Road> roads = Build.HandleBuildCommand(2 * stride);

        Assert.AreEqual(roads.First().Lanes.Single().StartVertex, Game.Zones[1].Vertices.Single());
    }

    [Test]
    public void ZoneAcceptsMinElevationRoadOnly()
    {
        Build.HandleBuildCommand(0);
        Build.HandleBuildCommand(stride);
        Game.HoveredZone = Game.Zones[1];
        Build.HandleBuildCommand(2 * stride + new float3(0, Constants.ElevationStep, 0));

        Assert.AreEqual(0, Game.Zones[1].Vertices.Count);
    }

    [Test]
    public void RemovingRoadRemovesVertexFromZone()
    {
        Game.HoveredZone = Game.Zones[1];
        Build.HandleBuildCommand(2 * stride);
        Game.HoveredZone = null;
        Build.HandleBuildCommand(3 * stride);
        Road road = Build.HandleBuildCommand(4 * stride).Single();
        Game.RemoveRoad(road);

        Assert.AreEqual(0, Game.Zones[1].Vertices.Count);
    }

    [Test]
    public void MultipleVerticesInZone()
    {
        Road road0 = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Road road1 = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Game.Zones[1].AddVertex(road1.Lanes[0].StartVertex);
        Game.Zones[1].AddVertex(road0.Lanes[0].StartVertex);
        Game.Zones[2].AddVertex(road0.Lanes[0].EndVertex);
        CarScheduler.FindNewConnection();

        Assert.AreNotEqual(0, Game.Zones[1].ConnectedTargets.Count);
    }


    [Test]
    public void SimpleZonePathFinding()
    {
        Road road0 = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Game.Zones[1].AddVertex(road0.Lanes[0].StartVertex);
        Game.Zones[2].AddVertex(road0.Lanes[0].EndVertex);
        Assert.AreEqual(0, Game.Zones[1].ConnectedTargets.Count);
        CarScheduler.FindNewConnection();
        Assert.AreEqual(1, Game.Zones[1].ConnectedTargets.Count);
    }

    [Test]
    public void BuildRoadBetweenZones()
    {
        Road road = RoadBuilder.ZoneToZone(0, stride, 2 * stride, Game.Zones[1], Game.Zones[2]);
        Assert.AreEqual(1, Game.Roads.Count);
        Assert.AreEqual(1, Game.Zones[1].ConnectedTargets.Count);
    }

    
    [Test]
    public void DivideFindsNewPath()
    {
        Road road = RoadBuilder.ZoneToZone(0, stride, 2 * stride, Game.Zones[1], Game.Zones[2]);
        Assert.AreEqual(1, Game.Zones[1].ConnectedTargets.Count);
        Divide.DivideRoad(road, math.length(stride));
        Assert.AreEqual(1, Game.Zones[1].ConnectedTargets.Count);
        Assert.AreEqual(3, Game.Zones[1].ConnectedTargets.Values.Single().Edges.Count);

        foreach (Edge edge in Game.Zones[1].ConnectedTargets.Values.Single().Edges)
            Assert.True(Graph.ContainsEdge(edge));
    }

    
    [Test]
    public void CombineFindsNewPath()
    {
        Road left = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Road right = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        Game.Zones[1].AddVertex(left.Lanes[0].StartVertex);
        Game.Zones[2].AddVertex(right.Lanes[0].EndVertex);
        CarScheduler.FindNewConnection();
        Assert.AreEqual(1, Game.Zones[1].ConnectedTargets.Count);

        Combine.CombineRoads(left.EndIntersection);
        Assert.AreEqual(1, Game.Zones[1].ConnectedTargets.Values.Single().Edges.Count);
        foreach (Edge edge in Game.Zones[1].ConnectedTargets.Values.Single().Edges)
            Assert.True(Graph.ContainsEdge(edge));
    }

    [Test]
    public void FindsPathToClosestVertex()
    {
        Road left = RoadBuilder.Single(-2 * stride, -stride, 0, 1);
        Road mid = RoadBuilder.Single(0, stride, 2 * stride, 2);
        Road longer = RoadBuilder.Single(mid.Lanes[1].EndPos, 4 * stride, 6 * stride, 1);
        Road shorter = RoadBuilder.Single(mid.Lanes[0].EndPos, 3 * stride, 4 * stride, 1);
        Game.Zones[1].AddVertex(left.Lanes[0].StartVertex);
        Game.Zones[2].AddVertex(longer.Lanes[0].EndVertex);
        Game.Zones[2].AddVertex(shorter.Lanes[0].EndVertex);
        CarScheduler.FindNewConnection();

        Assert.True(Game.Zones[1].ConnectedTargets[Game.Zones[2]].Edges.Contains(shorter.Lanes[0].InnerEdge));
    }

    [Test]
    public void ParallelBuildZoneConnection()
    {
        Build.ParallelBuildOn = true;
        Game.HoveredZone = Game.Zones[1];
        Build.HandleBuildCommand(0);
        Game.HoveredZone = null;
        Build.HandleBuildCommand(stride);
        Game.HoveredZone = Game.Zones[2];
        Build.HandleBuildCommand(2 * stride);

        Assert.True(Game.Zones[1].ConnectedTargets.ContainsKey(Game.Zones[2]));
    }

    [Test]
    public void ParallelBuildZoneConnectionReversed()
    {
        Build.ParallelBuildOn = true;
        Game.HoveredZone = Game.Zones[1];
        Build.HandleBuildCommand(0);
        Game.HoveredZone = null;
        Build.HandleBuildCommand(stride);
        Game.HoveredZone = Game.Zones[2];
        Build.HandleBuildCommand(2 * stride);

        Assert.True(Game.Zones[1].ConnectedTargets.ContainsKey(Game.Zones[2]));
    }

    [Test]
    public void ContinuousBuildingSetsZoneCorrectly()
    {
        Build.ContinuousBuilding = true;
        Build.HandleBuildCommand(0);
        Build.HandleBuildCommand(stride);
        Game.HoveredZone = Game.Zones[1];
        Build.HandleBuildCommand(2 * stride);
        Game.HoveredZone = null;
        Build.HandleBuildCommand(3 * stride);
        Assert.NotNull(Build.StartZone);
    }

    [Test]
    public void DoesNotFindPathToSelf()
    {
        Road left = RoadBuilder.ZoneToZone(
            0,
            MyNumerics.Forward * Constants.MinLaneLength,
            (MyNumerics.Forward + MyNumerics.Right) * Constants.MinLaneLength,
            Game.Zones[1],
            Game.Zones[2]
        );

        Road right = RoadBuilder.ZoneToZone(
            (MyNumerics.Forward + MyNumerics.Right) * Constants.MinLaneLength,
            MyNumerics.Right * Constants.MinLaneLength,
            0,
            Game.Zones[2],
            Game.Zones[1]
        );
        
        Assert.NotNull(left);
        Assert.NotNull(right);
        Assert.True(Game.Zones[1].ConnectedTargets.ContainsKey(Game.Zones[2]));
        Assert.True(Game.Zones[2].ConnectedTargets.ContainsKey(Game.Zones[1]));
        Assert.False(Game.Zones[1].ConnectedTargets.ContainsKey(Game.Zones[1]));
        Assert.False(Game.Zones[2].ConnectedTargets.ContainsKey(Game.Zones[2]));
    }
}