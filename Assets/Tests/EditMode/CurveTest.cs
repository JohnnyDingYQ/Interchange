using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class CurveTest
{
    float3 stride = Constants.MinLaneLength * new float3(1, 0, 0);
    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
    }

    // [Test]
    // public void GetNearestPointStraightLine()
    // {
    //     Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
    //     float distance = road.GetNearestInterpolation(stride);

    //     Assert.True(MyNumerics.AreNumericallyEqual(road.Length / 2, distance, 0.1f));
    // }

    // [Test]
    // public void GetNearestPointCurve()
    // {
    //     // Road road = RoadBuilder.Single(0, stride, 1);
    //     float distance = road.GetNearestPoint(stride);
    // }

}