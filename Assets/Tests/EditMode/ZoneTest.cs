using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

public class ZoneTest
{
    float3 stride = Constants.MinLaneLength * new float3(1, 0, 0);

    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
        Game.SourceZones = new();
        Game.TargetZones = new();
        for (uint i = 1; i < 4; i++)
        {
            Game.SourceZones.Add(i, new(i, ZoneType.Source));
            Game.TargetZones.Add(i, new(i, ZoneType.Target));
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

        Debug.Log(roads.First().Lanes.Single().StartVertex);
        Debug.Log(Game.SourceZones[1].Vertices.Count);

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
    public void ContinueRoadInSourceZone()
    {
        Game.HoveredZone = Game.SourceZones[1];
        Build.HandleBuildCommand(2 * stride);
        Game.HoveredZone = null;
        Build.HandleBuildCommand(3 * stride);
        Build.HandleBuildCommand(4 * stride);

        Game.HoveredZone = Game.SourceZones[1];
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);

        Assert.AreEqual(1, Game.SourceZones[1].Vertices.Count);
        Assert.AreEqual(road.Lanes.Single().StartVertex, Game.SourceZones[1].Vertices.Single());
    }

    [Test]
    public void ContinueRoadInTargetZone()
    {
        Build.HandleBuildCommand(0);
        Build.HandleBuildCommand(stride);
        Game.HoveredZone = Game.TargetZones[1];
        Build.HandleBuildCommand(2 * stride);
        Road road = RoadBuilder.Single(2  * stride, 3 * stride, 4 * stride, 1);

        Assert.AreEqual(1, Game.TargetZones[1].Vertices.Count);
        Assert.AreEqual(road.Lanes.Single().EndVertex, Game.TargetZones[1].Vertices.Single());
    }
}