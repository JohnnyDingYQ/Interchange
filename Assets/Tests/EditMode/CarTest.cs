using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

public class CarTest
{
    float3 stride = Constants.MinimumLaneLength * new float3(1, 0, 0);
    Zone zone0;
    Zone zone1;
    const float deltaTime = 0.1f;
    const float maxGivenTime = 60;
    
    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
        zone0 = new(0);
        zone1 = new(1);
    }

    [Test]
    public void SimplePathCompletion()
    {
        Road road = RoadBuilder.B(0, stride, 2 * stride, 1);
        zone0.AddOutRoad(road);
        zone1.AddInRoad(road);
        Car car = new(zone0, zone1, DemandsSatisfer.FindPath(zone0, zone1).ToArray());
        for (float i = 0; i < maxGivenTime; i += deltaTime)
            car.Move(deltaTime);
        Assert.True(car.ReachedDestination);
    }

    [Test]
    public void LongPathsCompletion()
    {
        Road road0 = RoadBuilder.B(0, stride, 2 * stride, 1);
        RoadBuilder.B(2 * stride, 3 * stride, 4 * stride, 1);
        Road road2 = RoadBuilder.B(4 * stride, 5 * stride, 6 * stride, 1);
        zone0.AddOutRoad(road0);
        zone1.AddInRoad(road2);
        Car car = new(zone0, zone1, DemandsSatisfer.FindPath(zone0, zone1).ToArray());
        for (float i = 0; i < maxGivenTime; i += deltaTime)
            car.Move(deltaTime);
        Assert.True(car.ReachedDestination);
    }

    [Test]
    public void LongPathsMultipleCarsCompletion()
    {
        Road road0 = RoadBuilder.B(0, stride, 2 * stride, 1);
        RoadBuilder.B(2 * stride, 3 * stride, 4 * stride, 1);
        Road road2 = RoadBuilder.B(4 * stride, 5 * stride, 6 * stride, 1);
        zone0.AddOutRoad(road0);
        zone1.AddInRoad(road2);
        List<Car> cars = new();
        for (int i = 0; i < 10; i++)
            cars.Add(new(zone0, zone1, DemandsSatisfer.FindPath(zone0, zone1).ToArray()));
        for (float i = 0; i < maxGivenTime; i += deltaTime)
            foreach (Car car in cars)
                car.Move(deltaTime);
        foreach (Car car in cars)
            Assert.True(car.ReachedDestination);
    }

    [Test]
    public void LaneChangingMultipleCarsCompletion()
    {
        int now = (int)DateTime.Now.Ticks;
        UnityEngine.Random.InitState(now);

        Road road0 = RoadBuilder.B(0, stride, 2 * stride, 3);
        RoadBuilder.B(2 * stride, 3 * stride, 4 * stride, 3);
        Road road2 = RoadBuilder.B(4 * stride, 5 * stride, 6 * stride, 3);
        zone0.AddOutRoad(road0);
        zone1.AddInRoad(road2);
        List<Car> cars = new();
        for (int i = 0; i < 20; i++)
            cars.Add(new(zone0, zone1, DemandsSatisfer.FindPath(zone0, zone1).ToArray()));
        for (float i = 0; i < maxGivenTime; i += deltaTime)
            foreach (Car car in cars)
                car.Move(deltaTime);
        foreach (Car car in cars)
        {
            if (!car.ReachedDestination)
                Debug.Log("Seed: " + now);
            Assert.True(car.ReachedDestination);
        }
    }
}