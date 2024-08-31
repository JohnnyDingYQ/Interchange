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

        curve = curve.AddStartDistance(decrement);
        float3 prevStart = curve.StartPos;
        curve = curve.AddStartDistance(decrement);
        Assert.AreNotEqual(prevStart, curve.StartPos);
        Assert.True(MyNumerics.IsApproxEqual(math.length(prevStart - curve.StartPos), decrement));

        curve.AddEndDistance(decrement);
        float3 prevEnd = curve.EndPos;
        curve.AddEndDistance(decrement);
        Assert.AreNotEqual(prevStart, curve.EndPos);
        Assert.True(MyNumerics.IsApproxEqual(math.length(prevEnd - curve.EndPos), decrement));
    }

    [Test]
    public void SplitSingleSegment()
    {
        Curve curve = new(new(0, stride, 2 * stride));
        curve.Split(curve.Length * 0.5f, out Curve left, out Curve right);

        Assert.True(MyNumerics.IsApproxEqual(right.Length, left.Length));
    }

    [Test]
    public void SplitLongCurve()
    {
        float longLength = 2000;
        float3 up = new(0, 0, 1);
        Curve curve = new(new(0, up * longLength / 2, up * longLength));
        curve.Split(1, out Curve left, out Curve right);

        Assert.IsTrue(MyNumerics.IsApproxEqual(1, left.Length));
        Assert.IsTrue(MyNumerics.IsApproxEqual(longLength - 1, right.Length));
    }

    [Test]
    public void GetNearestDistanceLongCurve()
    {
        float longLength = 2000;
        float3 up = new(0, 0, 1);
        Curve curve = new(new(0, up * longLength / 2, up * longLength));
        curve.GetNearestDistance(new Ray(up + new float3(0, 1, 0), new(0, -1, 0)), out float distanceOnCurve);

        Assert.IsTrue(MyNumerics.IsApproxEqual(1, distanceOnCurve));
    }

    [Test]
    public void ReverseTest()
    {
        float3 up = new(0, 0, 500);
        float3 right = new(500, 0, 0);
        Curve curve = new(new(0, up, up + right));
        Curve reversed = curve.Duplicate().Offset(1).Reverse();

        Assert.AreEqual(curve.StartPos + curve.StartNormal, reversed.EndPos);
        Assert.AreEqual(curve.EndPos + curve.EndNormal, reversed.StartPos);
    }

    [Test]
    public void GetNearestDistanceAtEnd()
    {
        for (float length = 100; length < 2000; length += 50)
        {
            Curve curve = new(new(0, MyNumerics.Right * length / 2, MyNumerics.Right * length));
            curve.GetNearestDistance(new(new(length - 1, -1, 1), new(0, 1, 0)), out float distA);
            curve.GetNearestDistance(new(new(length - 2, -1, 1), new(0, 1, 0)), out float distB);

            Assert.AreNotEqual(distA, distB);
        }
    }

    [Test]
    public void LerpPositionTest()
    {
        float3 up = new(0, 0, 100);
        float3 right = new(100, 0, 0);
        Curve curve = new(new(0, up, up + right));

        int numPoints = 20;
        for (float i = 0; i <= curve.Length; i += curve.Length / numPoints)
            Assert.True(MyNumerics.IsApproxEqual(curve.EvaluatePosition(i), curve.LerpPosition(i), 0.2f),
            $"Expected: {curve.EvaluatePosition(i)}, Actural: {curve.LerpPosition(i)}");
    }
}