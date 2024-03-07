using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class DivisionTargetsTest
{
    const float Tolerance = 0.05f;

    [SetUp]
    public void SetUp()
    {
        Game.WipeGameState();
        BuildHandler.Reset();
    }

    [Test]
    public void OneRoad()
    {
        RoadBuilder.BuildRoad(new(0, 0, 0), new(GConsts.MinimumRoadLength, 4, 0), new(GConsts.MinimumRoadLength*2, 10, 0), 1);
        DivisionTargets dt = new(new(GConsts.MinimumRoadLength, 0, 0), Game.Roads.Values);
        Road road = Game.Roads.Values.First();
        
        Assert.AreSame(road, dt.Road);
        Assert.True(dt.Interpolation < 0.55 && dt.Interpolation > 0.45);
    }

    [Test]
    public void MaximumRoadLengthResolution_Delta8()
    {
        float delta = 8;
        RoadBuilder.BuildRoad(new(0, 0, 0), new(GConsts.MaximumRoadLength/2, 0, 0), new(GConsts.MaximumRoadLength, 0, 0), 1);
        DivisionTargets dt = new(new(GConsts.MaximumRoadLength/2 - delta, 0, 0), Game.Roads.Values);
        Road road = Game.Roads.Values.First();

        Assert.AreSame(road, dt.Road);
        Debug.Log(dt.Interpolation);
        float targetInterpolation = (GConsts.MaximumRoadLength/2 - delta) / GConsts.MaximumRoadLength;
        Debug.Log(targetInterpolation);
        Assert.True(dt.Interpolation < targetInterpolation + Tolerance && dt.Interpolation > targetInterpolation - Tolerance);
    }

    [Test]
    public void MaximumRoadLengthResolution_Delta3()
    {
        float delta = 3;
        RoadBuilder.BuildRoad(new(0, 0, 0), new(GConsts.MaximumRoadLength/2, 0, 0), new(GConsts.MaximumRoadLength, 0, 0), 1);
        DivisionTargets dt = new(new(GConsts.MaximumRoadLength/2 - delta, 0, 0), Game.Roads.Values);
        Road road = Game.Roads.Values.First();

        Assert.AreSame(road, dt.Road);
        Debug.Log(dt.Interpolation);
        float targetInterpolation = (GConsts.MaximumRoadLength/2 - delta) / GConsts.MaximumRoadLength;
        Debug.Log(targetInterpolation);
        Assert.True(dt.Interpolation < targetInterpolation + Tolerance && dt.Interpolation > targetInterpolation - Tolerance);
    }

}