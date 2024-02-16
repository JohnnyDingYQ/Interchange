using System.Linq;
using System.Security.Permissions;
using NUnit.Framework;
using UnityEngine;

public class RoadTest
{
    Road road;
    [SetUp]
    public void Setup()
    {
        road = new();
    }

    [Test]
    public void InitiateStartIntersection_OneLane()
    {
        Lane lane = new()
        {
            Start = 0,
            End = 1
        };
        road.Lanes = new() {lane};
        road.InitiateStartIntersection();
        Intersection intersection = road.Start;
        Assert.AreEqual(1, intersection.Roads.Count);
        Assert.AreSame(road, intersection.Roads.First());
        Assert.AreEqual(1 ,intersection.NodeWithLane.Count);
        Assert.AreSame(lane, intersection.NodeWithLane[0].First());
    }
}