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
    public void MulitpleAddDistance()
    {
        Curve curve = new(new(0, stride, 2 * stride));
        float decrement = curve.Length / 5;

        curve.AddStartDistance(decrement);
        float3 prevStart = curve.StartPos; 
        curve.AddStartDistance(decrement);
        Assert.AreNotEqual(prevStart, curve.StartPos);
        Assert.True(MyNumerics.IsApproxEqual(math.length(prevStart - curve.StartPos), decrement));

        curve.AddEndDistance(decrement);
        float3 prevEnd = curve.EndPos; 
        curve.AddEndDistance(decrement);
        Assert.AreNotEqual(prevStart, curve.EndPos);
        Assert.True(MyNumerics.IsApproxEqual(math.length(prevEnd - curve.EndPos), decrement));
    }

    [Test]
    public void AddStartDistanceMultipleSegments()
    {
        Curve curve = new(new(0, stride, 2 * stride));
        float singleSegmentLength = curve.Length;
        curve.Add(new(new(2 * stride, 3 * stride, 4 * stride)));
        
        curve.AddStartDistance(1.5f * singleSegmentLength);
        Assert.True(MyNumerics.IsApproxEqual(curve.Length, 0.5f * singleSegmentLength));
        Assert.True(MyNumerics.IsApproxEqual(3 * stride, curve.StartPos));
    }

    [Test]
    public void AddEndDistanceMultipleSegments()
    {
        Curve curve = new(new(0, stride, 2 * stride));
        float singleSegmentLength = curve.Length;
        curve.Add(new(new(2 * stride, 3 * stride, 4 * stride)));
        
        curve.AddEndDistance(1.5f * singleSegmentLength);
        Assert.True(MyNumerics.IsApproxEqual(curve.Length, 0.5f * singleSegmentLength));
        Assert.True(MyNumerics.IsApproxEqual(stride, curve.EndPos));
    }

    [Test]
    public void SplitSingleSegment()
    {
        Curve curve = new(new(0, stride, 2 * stride));
        curve.Split(curve.Length * 0.5f, out Curve left, out Curve right);

        Assert.True(MyNumerics.IsApproxEqual(right.Length, left.Length));
    }

    [Test]
    public void SplitMultiSegmentCurve()
    {
        Curve curve = new(new(0, stride, 2 * stride));
        float decrement = curve.Length;
        curve.Add(new(new(2 * stride, 3 * stride, 4 * stride)));
        float combinedLength = curve.Length;
        curve.Split(decrement * 1.5f, out Curve left, out Curve right);
        
        Assert.AreEqual(combinedLength, curve.Length);
        Assert.True(MyNumerics.IsApproxEqual(decrement * 1.5f, left.Length));
        Assert.True(MyNumerics.IsApproxEqual(decrement * 0.5f, right.Length));
    }

    [Test]
    public void AddAndSplitMultipleTimes()
    {
        Curve curve = new(new(0, stride, 2 * stride));
        float singleSegmentLength = curve.Length;
        curve.Add(new(new(2 * stride, 3 * stride, 4 * stride)));
        curve.Split(singleSegmentLength, out Curve left, out Curve right);
        for (int i = 0; i < 3; i++)
        {
            left.Add(right);
            left.Split(singleSegmentLength, out left, out right);
        }
        Assert.True(MyNumerics.IsApproxEqual(left.Length, right.Length));
    }
}