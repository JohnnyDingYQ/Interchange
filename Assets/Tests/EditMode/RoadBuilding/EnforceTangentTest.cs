using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class EnforcesTangentTest
{
    float3 stride = Constants.MinLaneLength * new float3(1, 0, 1);
    float3 up = new(0, 0, Constants.MinLaneLength);
    float3 right = new(Constants.MinLaneLength, 0, 0);
    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
    }

    [Test]
    public void EnforcesTangentAtEnd()
    {
        Road road0 = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Road road1 = RoadBuilder.Single(2 * stride, 3 * stride + new float3(Constants.MinLaneLength, 0, 0), 4 * stride, 1);
        float3 inTangent = road0.Curve.EndNormal;
        float3 outTangent = road1.Curve.StartNormal;

        Assert.True(MyNumerics.IsApproxEqual(inTangent, outTangent));
    }
    [Test]
    public void EnforcesTangentAtStart()
    {
        Road road1 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        Road road0 = RoadBuilder.Single(0, stride + new float3(Constants.MinLaneLength, 0, 0), 2 * stride, 1);
        float3 inTangent = road0.Curve.EndNormal;
        float3 outTangent = road1.Curve.StartNormal;

        Assert.True(MyNumerics.IsApproxEqual(inTangent, outTangent));
    }

    [Test]
    public void EnforcesTangentAtBothEndsParallel()
    {
        Road start = RoadBuilder.Single(0, up, 2 * up, 1);
        Road end = RoadBuilder.Single(right + 3 * up, right + 4 * up, right + 5 * up, 1);
        Road mid = RoadBuilder.Single(2 * up, Vector3.Lerp(2 * up, right + 3 * up, 0.5f), right + 3 * up, 1);

        Assert.True(MyNumerics.IsApproxEqual(start.Curve.EndTangent, mid.Curve.StartTangent));
        Assert.True(MyNumerics.IsApproxEqual(mid.Curve.EndTangent, end.Curve.StartTangent));
    }

    [Test]
    public void EnforcesTangentAtBothEndsRightAngle()
    {
        Road start = RoadBuilder.Single(-2 * up, -up, 0, 1);
        Road end = RoadBuilder.Single(2 * up + 2 * right, 2 * up + 3 * right, 2 * up + 4 * right, 1);
        Road mid = RoadBuilder.Single(0, math.lerp(0, 2 * up + 2 * right, 0.5f) ,2 * up + 2 * right, 1);

        Assert.True(MyNumerics.IsApproxEqual(start.Curve.EndTangent, mid.Curve.StartTangent));
        Assert.True(MyNumerics.IsApproxEqual(mid.Curve.EndTangent, end.Curve.StartTangent));
    }

    [Test]
    public void StraightModeAtStart()
    {
        Road other = RoadBuilder.Single(2 * right + 2 * up, 2 * right + 3 * up, 2 * right + 4 * up, 1);
        Build.StraightMode = true;
        Road straight = RoadBuilder.Single(0, right + up, 2 * right + 2 * up, 1);

        Assert.AreEqual(3, Game.Intersections.Count);
        Assert.True(MyNumerics.IsApproxEqual(other.Curve.StartTangent, straight.Curve.EndTangent));
    }

    [Test]
    public void StraightModeAtEnd()
    {
        Road other = RoadBuilder.Single(0, right + up, 2 * right + 2 * up, 1);
        Build.StraightMode = true;
        Road straight = RoadBuilder.Single(2 * right + 2 * up, 2 * right + 3 * up, 2 * right + 4 * up, 1);

        Assert.AreEqual(3, Game.Intersections.Count);
        Assert.True(MyNumerics.IsApproxEqual(other.Curve.EndTangent, straight.Curve.StartTangent));
    }
}