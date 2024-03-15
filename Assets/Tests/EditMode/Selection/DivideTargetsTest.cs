using System;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

public class DivideTargetsTest
{
    const float Tolerance = 0.05f;
    float3 direction = new(1, 0, 1);
    float3 offset = new(5, 0, 0);
    readonly float maxOffsetMult = GConsts.MaximumRoadLength / (float)Math.Sqrt(2f);

    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
        BuildHandler.Reset();
    }

    [Test]
    public void NoRoad()
    {
        DivideTargets dt = new(0, Game.Roads.Values);
        Assert.True(dt.IsValid == false);
    }

    [Test]
    public void OneRoad()
    {
        RoadBuilder.BuildRoad(0, GConsts.MinimumRoadLength * direction, 2 * GConsts.MinimumRoadLength * direction, 1);
        DivideTargets dt = new(GConsts.MinimumRoadLength * direction, Game.Roads.Values);
        Road road = Game.Roads.Values.First();

        Assert.True(dt.IsValid == true);
        Assert.AreSame(road, dt.Road);
        Assert.True(dt.Interpolation < 0.55 && dt.Interpolation > 0.45);
    }

    [Test]
    public void MaximumRoadLengthResolution_Delta8()
    {
        float delta = 8;
        RoadBuilder.BuildRoad(0, maxOffsetMult / 2 * direction, maxOffsetMult * direction, 1);
        DivideTargets dt = new((maxOffsetMult / 2 - delta) * direction, Game.Roads.Values);
        Road road = Game.Roads.Values.First();

        Assert.True(dt.IsValid == true);
        Assert.AreSame(road, dt.Road);
        float targetInterpolation = (maxOffsetMult / 2 - delta) / maxOffsetMult;
        Assert.True(dt.Interpolation < targetInterpolation + Tolerance && dt.Interpolation > targetInterpolation - Tolerance);
        Debug.Log("Difference is " + Math.Abs(dt.Interpolation - targetInterpolation));
    }

    [Test]
    public void MaximumRoadLengthResolution_Delta3()
    {
        float delta = 3;
        RoadBuilder.BuildRoad(0, maxOffsetMult / 2 * direction, maxOffsetMult * direction, 1);
        DivideTargets dt = new((maxOffsetMult / 2 - delta) * direction, Game.Roads.Values);
        Road road = Game.Roads.Values.First();

        Assert.True(dt.IsValid == true);
        Assert.AreSame(road, dt.Road);
        float targetInterpolation = (maxOffsetMult / 2 - delta) / maxOffsetMult;
        Assert.True(dt.Interpolation < targetInterpolation + Tolerance && dt.Interpolation > targetInterpolation - Tolerance);
        Debug.Log("Difference is " + Math.Abs(dt.Interpolation - targetInterpolation));
    }

    [Test]
    public void SnapsToNearerRoad()
    {
        Road road1 = RoadBuilder.BuildRoad(
            0, GConsts.MinimumRoadLength * direction,
            2 * GConsts.MinimumRoadLength * direction,
            1
        );
        Road road2 = RoadBuilder.BuildRoad(
            offset,
            GConsts.MinimumRoadLength * direction + offset,
            2 * GConsts.MinimumRoadLength * direction + offset,
            1
        );
        DivideTargets dt;

        dt = new(0.4f * offset, Game.Roads.Values);
        Assert.True(dt.IsValid == true);
        Assert.AreSame(road1, dt.Road);

        dt = new(0.6f * offset, Game.Roads.Values);
        Assert.True(dt.IsValid == true);
        Assert.AreSame(road2, dt.Road);
    }
}