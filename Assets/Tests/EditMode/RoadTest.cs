using System;
using System.Linq;
using System.Security.Permissions;
using NUnit.Framework;
using UnityEngine;

public class RoadTest
{
    Road road;
    Lane lane1;
    Lane lane2;

    [SetUp]
    public void SetUp()
    {
        road = new();
        lane1 = new()
        {
            Start = 0,
            End = 1
        };

        lane2 = new()
        {
            Start = 2,
            End = 3,
        };
    }

    [Test]
    public void DoubleInitStartIntersection_ThrowsException()
    {
        road.Lanes = new() {lane1};
        road.InitiateStartIntersection();
        Assert.Throws<InvalidOperationException>(() => road.InitiateStartIntersection());
    }

    [Test]
    public void DoubleInitEndIntersection_ThrowsException()
    {
        road.Lanes = new() {lane1};
        road.InitiateEndIntersection();
        Assert.Throws<InvalidOperationException>(() => road.InitiateEndIntersection());
    }

    [Test]
    public void InitiateStartIntersection_OneLane()
    {
        road.Lanes = new() {lane1};
        road.InitiateStartIntersection();
        Intersection intersection = road.Start;
        Assert.AreEqual(1, intersection.Roads.Count);
        Assert.AreSame(road, intersection.Roads.First());
        Assert.AreEqual(1 ,intersection.NodeWithLane.Count);
        Assert.AreSame(lane1, intersection.NodeWithLane[0].First());
    }

    [Test]
    public void InitiateStartIntersection_TwoLanes()
    {
        road.Lanes = new() {lane1, lane2};
        road.InitiateStartIntersection();
        Intersection intersection = road.Start;

        Assert.AreEqual(1, intersection.Roads.Count);
        Assert.AreSame(road, intersection.Roads.First());
        Assert.AreEqual(2 ,intersection.NodeWithLane.Count);
        Assert.AreSame(lane1, intersection.NodeWithLane[0].First());
        Assert.AreSame(lane2, intersection.NodeWithLane[2].First());
    }

    [Test]
    public void InitiateEndIntersection_OneLane()
    {
        road.Lanes = new() {lane1};
        road.InitiateEndIntersection();
        Intersection intersection = road.End;
        Assert.AreEqual(1, intersection.Roads.Count);
        Assert.AreSame(road, intersection.Roads.First());
        Assert.AreEqual(1 ,intersection.NodeWithLane.Count);
        Assert.AreSame(lane1, intersection.NodeWithLane[1].First());
    }

    [Test]
    public void InitiateEndIntersection_TwoLanes()
    {
        road.Lanes = new() {lane1, lane2};
        road.InitiateEndIntersection();
        Intersection intersection = road.End;

        Assert.AreEqual(1, intersection.Roads.Count);
        Assert.AreSame(road, intersection.Roads.First());
        Assert.AreEqual(2 ,intersection.NodeWithLane.Count);
        Assert.AreSame(lane1, intersection.NodeWithLane[1].First());
        Assert.AreSame(lane2, intersection.NodeWithLane[3].First());
    }

    [Test]
    public void LaneCount_OneLane()
    {
        road.Lanes = new() {lane1};
        Assert.AreEqual(1, road.GetLaneCount());
    }

    [Test]
    public void LaneCount_TwoLanes()
    {
        road.Lanes = new() {lane1, lane2};
        Assert.AreEqual(2, road.GetLaneCount());
    }
    
}