using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

public class CarTest
{
    float3 stride = Constants.MinLaneLength * new float3(1, 0, 0);
    const float deltaTime = 0.1f;
    const float maxGivenTime = 60;
    
    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
    }

    [Test]
    public void SimplePathCompletion()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Car car = DemandsSatisfer.AttemptSchedule(road, road);
        Assert.NotNull(car);
        Game.RegisterCar(car);
        for (float i = 0; i < maxGivenTime; i += deltaTime)
            CarControl.PassTime(deltaTime);
        Assert.True(car.IsDone);
    }

    [Test]
    public void LongPathsCompletion()
    {
        Road road0 = RoadBuilder.Single(0, stride, 2 * stride, 1);
        RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        Road road2 = RoadBuilder.Single(4 * stride, 5 * stride, 6 * stride, 1);
        Car car = DemandsSatisfer.AttemptSchedule(road0, road2);
        Assert.NotNull(car);
        Game.RegisterCar(car);
        for (float i = 0; i < maxGivenTime; i += deltaTime)
            CarControl.PassTime(deltaTime);
        Assert.True(car.IsDone);
    }

    [Test]
    public void LongPathsMultipleCarsCompletion()
    {
        Road road0 = RoadBuilder.Single(0, stride, 2 * stride, 1);
        RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        Road road2 = RoadBuilder.Single(4 * stride, 5 * stride, 6 * stride, 1);
        for (int i = 0; i < Constants.MaxVertexWaitingCar; i++)
        {
            Car car = DemandsSatisfer.AttemptSchedule(road0, road2);
            Assert.NotNull(car);
            Game.RegisterCar(car);
        }
        for (float i = 0; i < maxGivenTime; i += deltaTime)
            CarControl.PassTime(deltaTime);
        foreach (Car car in Game.Cars.Values)
            Assert.True(car.IsDone);
    }

    [Test]
    public void LaneChangingMultipleCarsCompletion()
    {
        int now = (int)DateTime.Now.Ticks;
        UnityEngine.Random.InitState(now);

        Road road0 = RoadBuilder.Single(0, stride, 2 * stride, 3);
        RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 3);
        Road road2 = RoadBuilder.Single(4 * stride, 5 * stride, 6 * stride, 3);
        for (int i = 0; i < Constants.MaxVertexWaitingCar; i++)
        {
            Car car = DemandsSatisfer.AttemptSchedule(road0, road2);
            Assert.NotNull(car);
            Game.RegisterCar(car);
        }
        for (float i = 0; i < maxGivenTime; i += deltaTime)
            CarControl.PassTime(deltaTime);
        foreach (Car car in Game.Cars.Values)
        {
            if (!car.IsDone)
                Debug.Log("Seed: " + now);
            Assert.True(car.IsDone);
        }
    }

    [Test]
    public void TooManyCarsScheduledForVertex()
    {
        Road road0 = RoadBuilder.Single(0, stride, 2 * stride, 1);
        RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        Road road2 = RoadBuilder.Single(4 * stride, 5 * stride, 6 * stride, 1);
        for (int i = 0; i <= Constants.MaxVertexWaitingCar; i++)
        {
            Car car = DemandsSatisfer.AttemptSchedule(road0, road2);
            if (i < Constants.MaxVertexWaitingCar)
                Assert.NotNull(car);
            else
                Assert.Null(car);
        }
    }

    [Test]
    public void PathRemoved()
    {
        Road road0 = RoadBuilder.Single(0, stride, 2 * stride, 1);
        RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        Road road2 = RoadBuilder.Single(4 * stride, 5 * stride, 6 * stride, 1);
        Car car = DemandsSatisfer.AttemptSchedule(road0, road2);
        Game.RegisterCar(car);
        for (float i = 0; i < 0.3f; i += deltaTime)
            CarControl.PassTime(deltaTime);
        Assert.True(Game.RemoveRoad(road0));
        CarControl.PassTime(deltaTime);
        Assert.True(car.IsDone);
    }
}