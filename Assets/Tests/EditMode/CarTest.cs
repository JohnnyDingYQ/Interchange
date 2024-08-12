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
        {
            Game.SourceZones.Add(i, new(i));
            Game.TargetZones.Add(i, new(i));
        }
    }

    [Test]
    public void SimplePathCompletion()
    {
        RoadBuilder.ZoneToZone(0, stride, 2 * stride, Game.SourceZones[1], Game.TargetZones[1]);
        CarScheduler.Schedule(0);
        Assert.AreEqual(1, Game.Cars.Count);
        Car car = Game.Cars.Values.Single();

        PassTime(4, 0.2f);
        Assert.AreEqual(0, Game.Cars.Count);
    }

    [Test]
    public void SaveAndLoadCar()
    {
        Road road0 = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Road road1 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        Road road2 = RoadBuilder.Single(4 * stride, 5 * stride, 6 * stride, 1);
        Game.SourceZones[1].AddVertex(road0.Lanes[0].StartVertex);
        Game.TargetZones[1].AddVertex(road2.Lanes[0].EndVertex);
        CarScheduler.FindNewConnection();
        CarScheduler.Schedule(0);
        Assert.AreEqual(1, Game.Cars.Count);
        Car car = Game.Cars.Values.Single();

        PassTime(4, 0.2f);
        Assert.AreEqual(0, Game.Cars.Count);
        float timeTaken = car.TimeTaken;
        
        ResetVertexCooldown();
        CarScheduler.Schedule(0);
        Assert.AreEqual(1, Game.Cars.Count);
        PassTime(timeTaken * 0.8f, 0.2f);
        Assert.AreSame(car.CurrentEdge, road2.Lanes[0].InnerEdge);

        testSaveSystem.SaveGame();
        testSaveSystem.LoadGame();
        CarScheduler.FindNewConnection();

        PassTime(timeTaken * 0.2f, 0.2f);
        Assert.AreEqual(0, Game.Cars.Count);
    }

    void PassTime(float total, float interval)
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