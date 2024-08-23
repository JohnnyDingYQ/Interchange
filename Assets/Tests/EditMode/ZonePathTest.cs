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
        {
            Game.SourceZones.Add(i, new(i));
            Game.TargetZones.Add(i, new(i));
        }
    }

    [Test]
    public void ConnectRoadEndToTargetZone()
    {
        Build.HandleBuildCommand(0);
        Build.HandleBuildCommand(stride);
        Game.HoveredZone = Game.TargetZones[1];
        List<Road> roads = Build.HandleBuildCommand(2 * stride);

        Assert.AreEqual(roads.Last().Lanes.Single().EndVertex, Game.TargetZones[1].Vertices.Single());
    }

    [Test]
    public void ConnectRoadStartToSourceZone()
    {
        Game.HoveredZone = Game.SourceZones[1];
        Build.HandleBuildCommand(0);
        Game.HoveredZone = null;
        Build.HandleBuildCommand(stride);
        List<Road> roads = Build.HandleBuildCommand(2 * stride);

        Assert.AreEqual(roads.First().Lanes.Single().StartVertex, Game.SourceZones[1].Vertices.Single());
    }

    [Test]
    public void ZoneAcceptsMinElevationRoadOnly()
    {
        Build.HandleBuildCommand(0);
        Build.HandleBuildCommand(stride);
        Game.HoveredZone = Game.TargetZones[1];
        Build.HandleBuildCommand(2 * stride + new float3(0, Constants.ElevationStep, 0));

        Assert.AreEqual(0, Game.TargetZones[1].Vertices.Count);
    }

    [Test]
    public void RemovingRoadRemovesVertexFromZone()
    {
        Game.HoveredZone = Game.SourceZones[1];
        Build.HandleBuildCommand(2 * stride);
        Game.HoveredZone = null;
        Build.HandleBuildCommand(3 * stride);
        Road road = Build.HandleBuildCommand(4 * stride).Single();
        Game.RemoveRoad(road);

        Assert.AreEqual(0, Game.SourceZones[1].Vertices.Count);
    }

    [Test]
    public void MultipleVerticesInSourceZone()
    {
        Road road0 = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Road road1 = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Game.SourceZones[1].AddVertex(road1.Lanes[0].StartVertex);
        Game.SourceZones[1].AddVertex(road0.Lanes[0].StartVertex);
        Game.TargetZones[1].AddVertex(road0.Lanes[0].EndVertex);
        CarScheduler.FindNewConnection();

        Assert.AreNotEqual(0, Game.SourceZones[1].ConnectedTargets.Count);
    }

    [Test]
    public void MultipleVerticesInTargetZone()
    {
        Road road0 = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Road road1 = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Game.SourceZones[1].AddVertex(road0.Lanes[0].StartVertex);
        Game.TargetZones[1].AddVertex(road1.Lanes[0].EndVertex);
        Game.TargetZones[1].AddVertex(road0.Lanes[0].EndVertex);
        CarScheduler.FindNewConnection();

        Assert.AreNotEqual(0, Game.SourceZones[1].ConnectedTargets.Count);
    }

    [Test]
    public void SimpleZonePathFinding()
    {
        Road road0 = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Game.SourceZones[1].AddVertex(road0.Lanes[0].StartVertex);
        Game.TargetZones[1].AddVertex(road0.Lanes[0].EndVertex);
        Assert.AreEqual(0, Game.SourceZones[1].ConnectedTargets.Count);
        CarScheduler.FindNewConnection();
        Assert.AreEqual(1, Game.SourceZones[1].ConnectedTargets.Count);
    }

    [Test]
    public void BuildRoadBetweenZones()
    {
        Road road = RoadBuilder.ZoneToZone(0, stride, 2 * stride, Game.SourceZones[1], Game.TargetZones[1]);
        Assert.AreEqual(1, Game.Roads.Count);
        Assert.AreEqual(1, Game.SourceZones[1].ConnectedTargets.Count);
    }

    
    [Test]
    public void DivideFindsNewPath()
    {
        Road road = RoadBuilder.ZoneToZone(0, stride, 2 * stride, Game.SourceZones[1], Game.TargetZones[1]);
        Assert.AreEqual(1, Game.SourceZones[1].ConnectedTargets.Count);
        Divide.DivideRoad(road, math.length(stride));
        Assert.AreEqual(1, Game.SourceZones[1].ConnectedTargets.Count);
        Assert.AreEqual(3, Game.SourceZones[1].ConnectedTargets.Values.Single().Edges.Count);

        foreach (Edge edge in Game.SourceZones[1].ConnectedTargets.Values.Single().Edges)
            Assert.True(Graph.ContainsEdge(edge));
    }

    
    [Test]
    public void CombineFindsNewPath()
    {
        Road left = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Road right = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        Game.SourceZones[1].AddVertex(left.Lanes[0].StartVertex);
        Game.TargetZones[1].AddVertex(right.Lanes[0].EndVertex);
        CarScheduler.FindNewConnection();
        Assert.AreEqual(1, Game.SourceZones[1].ConnectedTargets.Count);

        Combine.CombineRoads(left.EndIntersection);
        Assert.AreEqual(1, Game.SourceZones[1].ConnectedTargets.Values.Single().Edges.Count);
        foreach (Edge edge in Game.SourceZones[1].ConnectedTargets.Values.Single().Edges)
            Assert.True(Graph.ContainsEdge(edge));
    }

    [Test]
    public void FindsPathToClosestVertex()
    {
        Road left = RoadBuilder.Single(-2 * stride, -stride, 0, 1);
        Road mid = RoadBuilder.Single(0, stride, 2 * stride, 2);
        Road longer = RoadBuilder.Single(mid.Lanes[1].EndPos, 4 * stride, 6 * stride, 1);
        Road shorter = RoadBuilder.Single(mid.Lanes[0].EndPos, 3 * stride, 4 * stride, 1);
        Game.SourceZones[1].AddVertex(left.Lanes[0].StartVertex);
        Game.TargetZones[1].AddVertex(longer.Lanes[0].EndVertex);
        Game.TargetZones[1].AddVertex(shorter.Lanes[0].EndVertex);
        CarScheduler.FindNewConnection();

        Assert.True(Game.SourceZones[1].ConnectedTargets[Game.TargetZones[1]].Edges.Contains(shorter.Lanes[0].InnerEdge));
    }

    [Test]
    public void ParallelBuildZoneConnection()
    {
        Build.ParallelBuildOn = true;
        Game.HoveredZone = Game.SourceZones[1];
        Build.HandleBuildCommand(0);
        Game.HoveredZone = null;
        Build.HandleBuildCommand(stride);
        Game.HoveredZone = Game.TargetZones[1];
        Build.HandleBuildCommand(2 * stride);

        Assert.True(Game.SourceZones[1].ConnectedTargets.ContainsKey(Game.TargetZones[1]));
    }

    [Test]
    public void ParallelBuildZoneConnectionReversed()
    {
        Build.ParallelBuildOn = true;
        Game.HoveredZone = Game.TargetZones[1];
        Build.HandleBuildCommand(0);
        Game.HoveredZone = null;
        Build.HandleBuildCommand(stride);
        Game.HoveredZone = Game.SourceZones[1];
        Build.HandleBuildCommand(2 * stride);

        Assert.True(Game.SourceZones[1].ConnectedTargets.ContainsKey(Game.TargetZones[1]));
    }

    [Test]
    public void ContinuousBuildingSetsZoneCorrectly()
    {
        Build.ContinuousBuilding = true;
        Build.HandleBuildCommand(0);
        Build.HandleBuildCommand(stride);
        Game.HoveredZone = Game.TargetZones[1];
        Build.HandleBuildCommand(2 * stride);
        Game.HoveredZone = null;
        Build.HandleBuildCommand(3 * stride);
        Assert.NotNull(Build.StartZone);
    }
}