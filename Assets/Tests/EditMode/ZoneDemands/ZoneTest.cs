using System;
using NUnit.Framework;
using Unity.Mathematics;

public class ZoneTest
{
    Zone zone1;
    Zone zone2;
    float3 stride = Constants.MinimumLaneLength * new float3(1, 0, 0);

    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
        zone1 = new(1);
        zone2 = new(2);
        Game.Zones.Add(1, zone1);
        Game.Zones.Add(2, zone2);
    }

    [Test]
    public void ZoneIdModifySuccessful()
    {
        Road road0 = RoadBuilder.B(0, stride, 2 * stride, 1);
        zone1.AddOutRoad(road0);
        zone2.AddInRoad(road0);
        Assert.AreEqual(1, road0.StartZoneId);
        Assert.AreEqual(2, road0.EndZoneId);
        Assert.True(zone1.IsConsistent());
        Assert.True(zone2.IsConsistent());
    }

    [Test]
    public void InRoadVerticesRetrievable()
    {
        Road road0 = RoadBuilder.B(0, stride, 2 * stride, 1);
        zone1.AddInRoad(road0);
        Assert.NotNull(zone1.GetRandomInVertex());
        
    }

    [Test]
    public void OutRoadVerticesRetrievable()
    {
        Road road0 = RoadBuilder.B(0, stride, 2 * stride, 1);
        zone1.AddOutRoad(road0);
        Assert.NotNull(zone1.GetRandomOutVertex());
    }

    [Test]
    public void GetRandomVertexDoesNotReturnSameV()
    {
        Road road0 = RoadBuilder.B(0, stride, 2 * stride, 3);
        zone1.AddOutRoad(road0);
        Vertex v = zone1.GetRandomOutVertex();
        bool gotDifferent = false;
        for (int i = 0; i < 300; i++)
            if (v != zone1.GetRandomOutVertex())
            {
                gotDifferent = true;
                break;
            }
        Assert.True(gotDifferent);
    }

    [Test]
    public void GetVertexFromEmptyZoneReturnsNull()
    {
        Assert.Null(zone1.GetRandomOutVertex());
    }

    [Test]
    public void RemoveJustAddedRoadFromZone()
    {
        Road road0 = RoadBuilder.B(0, stride, 2 * stride, 3);
        zone1.AddOutRoad(road0);
        zone1.RemoveRoad(road0);
        Assert.Null(zone1.GetRandomOutVertex());
        Assert.True(zone1.IsConsistent());
    }

    [Test]
    public void RemoveRoadFromZoneDoesNotRemoveAll()
    {
        Road road0 = RoadBuilder.B(0, stride, 2 * stride, 3);
        Road road1 = RoadBuilder.B(0, stride, 2 * stride, 3);
        zone1.AddInRoad(road0);
        zone1.AddInRoad(road1);
        zone1.RemoveRoad(road0);
        Assert.NotNull(zone1.GetRandomInVertex());
        Assert.True(zone1.IsConsistent());
    }

    [Test]
    public void SimpleDemandSatisfied()
    {
        zone1.Demands[2] = 1;
        zone2.Demands[1] = 1;
        DemandsSatisfer.SatisfyDemands(0);
        Assert.AreEqual(1, zone1.Demands[2]);
        Assert.AreEqual(1, zone2.Demands[1]);

        Road road = RoadBuilder.B(0, stride, 2 * stride, 1);
        zone1.AddOutRoad(road);
        zone2.AddInRoad(road);
        DemandsSatisfer.SatisfyDemands(0);
        Assert.AreEqual(0, zone1.Demands[2]);
        Assert.AreEqual(1, zone2.Demands[1]);
        Assert.True(zone1.IsConsistent());
        Assert.True(zone2.IsConsistent());
    }

    [Test]
    public void RemoveRoadRemovesRoadFromZone()
    {
        Road road0 = RoadBuilder.B(0, stride, 2 * stride, 1);
        zone1.AddOutRoad(road0);
        zone2.AddInRoad(road0);
        Assert.True(Game.RemoveRoad(road0));
        Assert.Null(zone1.GetRandomOutVertex());
        Assert.Null(zone2.GetRandomInVertex());
        Assert.Null(DemandsSatisfer.AttemptSchedule(zone1, zone2));
        Assert.True(zone1.IsConsistent());
        Assert.True(zone2.IsConsistent());
    }
}