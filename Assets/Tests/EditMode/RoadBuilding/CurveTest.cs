using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

public class CurveTest
{
    float3 stride = Constants.MinLaneLength * new float3(1, 0, 0);

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Game.WipeState();
    }

    [Test]
    public void SplitSingleSegment()
    {
        Curve curve = new(new(0, stride, 2 * stride));
        curve.Split(curve.Length * 0.5f, out Curve left, out Curve right);

        Assert.True(MyNumerics.AreNumericallyEqual(right.Length, left.Length));
    }

    [Test]
    public void SplitMultiSegmentCurve()
    {
        Curve curve = new(new(0, stride, 2 * stride));
        float length = curve.Length;
        curve.Add(new(new(2 * stride, 3 * stride, 4 * stride)));
        float combinedLength = curve.Length;
        curve.Split(length * 1.5f, out Curve left, out Curve right);
        
        Assert.AreEqual(combinedLength, curve.Length);
        Assert.True(MyNumerics.AreNumericallyEqual(length * 1.5f, left.Length));
        Assert.True(MyNumerics.AreNumericallyEqual(length * 0.5f, right.Length));
    }
}