using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

public class CarTest
{
    float3 stride = Constants.MinLaneLength * new float3(1, 0, 0);
    static readonly string saveName = "testSave";
    readonly SaveSystem testSaveSystem = new(saveName);

    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
        for (uint i = 1; i < 4; i++)
            Game.Zones.Add(i, new(i));
        Game.SetupZones();
    }

    [Test]
    public void SimplePathCompletion()
    {
        RoadBuilder.ZoneToZone(0, stride, 2 * stride, Game.Zones[1], Game.Zones[2]);
        CarScheduler.Schedule(0);
        Assert.AreEqual(1, Game.Cars.Count);
        Car car = Game.Cars.Values.Single();

        PassTime(4);
        Assert.AreEqual(0, Game.Cars.Count);
        Assert.AreEqual(1, Game.CarServiced);
    }

    [Test]
    public void SaveAndLoadCar()
    {
        Road road0 = RoadBuilder.Single(0, stride, 2 * stride, 1);
        RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        Road road2 = RoadBuilder.Single(4 * stride, 5 * stride, 6 * stride, 1);
        Game.Zones[1].AddVertex(road0.Lanes[0].StartVertex);
        Game.Zones[2].AddVertex(road2.Lanes[0].EndVertex);
        CarScheduler.FindNewConnection();
        CarScheduler.Schedule(0);
        Car car = Game.Cars.Values.Single();

        PassTime(4);
        Assert.AreEqual(0, Game.Cars.Count);
        float timeTaken = car.TimeTaken;

        ResetVertexCooldown();
        CarScheduler.Schedule(0);
        Assert.AreEqual(1, Game.Cars.Count);
        PassTime(timeTaken * 0.8f);
        Assert.AreSame(car.CurrentEdge, road2.Lanes[0].InnerEdge);

        testSaveSystem.SaveGame();
        testSaveSystem.LoadGame();

        PassTime(timeTaken * 0.2f);
        Assert.AreEqual(0, Game.Cars.Count);
    }

    [Test]
    public void DivideCancelsCarOnRoad()
    {
        float totalLength = 100;
        Road road = RoadBuilder.ZoneToZone(0, MyNumerics.Forward * totalLength / 2, MyNumerics.Forward * totalLength, Game.Zones[1], Game.Zones[2]);
        CarScheduler.Schedule(0);
        Car car = Game.Cars.Values.Single();

        Assert.AreEqual(1, Game.Cars.Count);
        PassTime(0.2f);
        Divide.DivideRoad(road, totalLength / 2);
        PassTime(0.2f);
        Assert.AreEqual(0, Game.CarServiced);
    }

    [Test]
    public void CarUnaffectedByDivideBefore()
    {
        Road road0 = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Road road1 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        Game.Zones[1].AddVertex(road0.Lanes[0].StartVertex);
        Game.Zones[2].AddVertex(road1.Lanes[0].EndVertex);
        CarScheduler.FindNewConnection();
        CarScheduler.Schedule(0);
        Assert.AreEqual(1, Game.Cars.Count);
        Car car = Game.Cars.Values.Single();

        PassTime(4);
        Assert.AreEqual(0, Game.Cars.Count);
        float timeTaken = car.TimeTaken;

        ResetVertexCooldown();
        CarScheduler.Schedule(0);
        car = Game.Cars.Values.Single();
        PassTime(timeTaken * 0.8f);
        Divide.DivideRoad(road0, math.length(stride));
        PassTime(0.2f);
        Assert.True(Graph.ContainsEdge(car.CurrentEdge));
        PassTime(timeTaken * 0.8f);
        Assert.AreEqual(0, Game.Cars.Count);
        Assert.AreEqual(2, Game.CarServiced);
    }

    [Test]
    public void CarUnaffectedByDivideAhead()
    {
        Road road0 = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Road road1 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        Game.Zones[1].AddVertex(road0.Lanes[0].StartVertex);
        Game.Zones[2].AddVertex(road1.Lanes[0].EndVertex);
        CarScheduler.FindNewConnection();
        CarScheduler.Schedule(0);
        Assert.AreEqual(1, Game.Cars.Count);
        Car car = Game.Cars.Values.Single();

        PassTime(4);
        Assert.AreEqual(0, Game.Cars.Count);
        float timeTaken = car.TimeTaken;

        ResetVertexCooldown();
        CarScheduler.Schedule(0);
        car = Game.Cars.Values.Single();
        PassTime(timeTaken * 0.2f);
        Divide.DivideRoad(road1, math.length(stride));
        CarControl.PassTime(0);
        Assert.True(Graph.ContainsEdge(car.CurrentEdge));
        PassTime(timeTaken);
        Assert.AreEqual(0, Game.Cars.Count);
        Assert.AreEqual(2, Game.CarServiced);
    }

    [Test]
    public void CarCancelsWhenRoadRemoved()
    {
        Road road = RoadBuilder.ZoneToZone(0, stride, 2 * stride, Game.Zones[1], Game.Zones[2]);
        CarScheduler.Schedule(0);
        Car car = Game.Cars.Values.Single();
        PassTime(0.2f);

        Game.RemoveRoad(road);
        PassTime(0.2f);

        Assert.AreEqual(0, Game.CarServiced);
    }

    void PassTime(float total, float interval = 0.1f)
    {
        for (float i = 0; i < total; i += interval)
            CarControl.PassTime(interval);
    }

    void ResetVertexCooldown()
    {
        foreach (Vertex vertex in Game.Vertices.Values)
            vertex.ScheduleCooldown = 0;
    }
}