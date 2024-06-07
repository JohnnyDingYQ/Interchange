using System;
using NUnit.Framework;
using Unity.Mathematics;

public class ZoneTest
{
    Zone zone0;
    Zone zone1;
    float3 stride = Constants.MinimumLaneLength * new float3(1, 0, 0);

    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
        zone0 = new(0);
        zone1 = new(1);
        Game.Zones.Add(0, zone0);
        Game.Zones.Add(1, zone1);
    }

    [Test]
    public void ZoneIdModifySuccessful()
    {
        Road road0 = RoadBuilder.B(0, stride, 2 * stride, 1);
        zone0.AddOutRoad(road0);
        zone1.AddInRoad(road0);
        Assert.AreEqual(0, road0.StartZoneId);
        Assert.AreEqual(1, road0.EndZoneId);
        Assert.True(zone0.IsConsistent());
        Assert.True(zone1.IsConsistent());
    }

    [Test]
    public void InRoadVerticesRetrievable()
    {
        Road road0 = RoadBuilder.B(0, stride, 2 * stride, 1);
        zone0.AddInRoad(road0);
        Assert.NotNull(zone0.GetRandomInVertex());
        
    }

    [Test]
    public void OutRoadVerticesRetrievable()
    {
        Road road0 = RoadBuilder.B(0, stride, 2 * stride, 1);
        zone0.AddOutRoad(road0);
        Assert.NotNull(zone0.GetRandomOutVertex());
    }

    [Test]
    public void GetRandomVertexDoesNotReturnSameV()
    {
        Road road0 = RoadBuilder.B(0, stride, 2 * stride, 3);
        zone0.AddOutRoad(road0);
        Vertex v = zone0.GetRandomOutVertex();
        bool gotDifferent = false;
        for (int i = 0; i < 300; i++)
            if (v != zone0.GetRandomOutVertex())
            {
                gotDifferent = true;
                break;
            }
        Assert.True(gotDifferent);
    }

    [Test]
    public void GetVertexFromEmptyZoneReturnsNull()
    {
        Assert.Null(zone0.GetRandomOutVertex());
    }

    [Test]
    public void RemoveJustAddedRoadFromZone()
    {
        Road road0 = RoadBuilder.B(0, stride, 2 * stride, 3);
        zone0.AddOutRoad(road0);
        zone0.RemoveRoad(road0);
        Assert.Null(zone0.GetRandomOutVertex());
        Assert.True(zone0.IsConsistent());
    }

    [Test]
    public void RemoveRoadFromZoneDoesNotRemoveAll()
    {
        Road road0 = RoadBuilder.B(0, stride, 2 * stride, 3);
        Road road1 = RoadBuilder.B(0, stride, 2 * stride, 3);
        zone0.AddInRoad(road0);
        zone0.AddInRoad(road1);
        zone0.RemoveRoad(road0);
        Assert.NotNull(zone0.GetRandomInVertex());
        Assert.True(zone0.IsConsistent());
    }

    [Test]
    public void SimpleDemandSatisfied()
    {
        zone0.Demands[1] = 1;
        zone1.Demands[0] = 1;
        DemandsSatisfer.SatisfyDemands(0);
        Assert.AreEqual(1, zone0.Demands[1]);
        Assert.AreEqual(1, zone1.Demands[0]);

        Road road = RoadBuilder.B(0, stride, 2 * stride, 1);
        zone0.AddOutRoad(road);
        zone1.AddInRoad(road);
        DemandsSatisfer.SatisfyDemands(0);
        Assert.AreEqual(0, zone0.Demands[1]);
        Assert.AreEqual(1, zone1.Demands[0]);
        Assert.True(zone0.IsConsistent());
        Assert.True(zone1.IsConsistent());
    }

    [Test]
    public void RemoveRoadRemovesRoadFromZone()
    {
        Road road0 = RoadBuilder.B(0, stride, 2 * stride, 1);
        zone0.AddOutRoad(road0);
        zone1.AddInRoad(road0);
        Assert.True(Game.RemoveRoad(road0));
        Assert.Null(zone0.GetRandomOutVertex());
        Assert.Null(zone1.GetRandomInVertex());
        Assert.Null(DemandsSatisfer.AttemptSchedule(zone0, zone1));
        Assert.True(zone0.IsConsistent());
        Assert.True(zone1.IsConsistent());
    }
}