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
    public void RemoveJustAddedRoad()
    {
        Road road0 = RoadBuilder.B(0, stride, 2 * stride, 3);
        zone0.AddOutRoad(road0);
        zone0.RemoveRoad(road0);
        Assert.Null(zone0.GetRandomOutVertex());
    }

    [Test]
    public void RemoveRoadDoesNotRemoveAll()
    {
        Road road0 = RoadBuilder.B(0, stride, 2 * stride, 3);
        Road road1 = RoadBuilder.B(0, stride, 2 * stride, 3);
        zone0.AddInRoad(road0);
        zone0.AddInRoad(road1);
        zone0.RemoveRoad(road0);
        Assert.NotNull(zone0.GetRandomInVertex());
    }
}