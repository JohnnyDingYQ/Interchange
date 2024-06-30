using UnityEngine;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine.Splines;
using System.Linq;

public class BezierSeriesTest
{

    float3 stride = Constants.MinLaneLength * new float3(1, 0, 0);

    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
    }

    [Test]
    public void Offset()
    {
        BezierSeries bs = new(new BezierCurve(0, stride, 2 * stride));
        BezierSeries offsetted = bs.Offset(0);
        Assert.AreEqual(4, offsetted.Curves.Count);
        Assert.True(MyNumerics.AreNumericallyEqual(bs.Length, offsetted.Length));
        float length = CurveUtility.CalculateLength(offsetted.Curves.First());
        foreach (BezierCurve curve in offsetted.Curves)
            Assert.True(MyNumerics.AreNumericallyEqual(length, CurveUtility.CalculateLength(curve)));
    }

    [Test]
    public void Split()
    {
        BezierSeries bs = new(new BezierCurve(0, stride, 2 * stride));
        bs.Offset(5);
        bs.Split(0.5f, out BezierSeries left, out BezierSeries right);
        Assert.True(MyNumerics.AreNumericallyEqual(left.Length, right.Length));
    }

    [Test]
    public void TruncateConstructor()
    {
        BezierSeries bs = new(new BezierCurve(0, stride, 2 * stride));
        bs.Offset(5);
        BezierSeries truncated = new(
            bs,
            Constants.VertexDistanceFromRoadEnds / bs.Length,
            (bs.Length - Constants.VertexDistanceFromRoadEnds) / bs.Length
        );
        Assert.True(MyNumerics.AreNumericallyEqual(truncated.Length, bs.Length - Constants.VertexDistanceFromRoadEnds * 2));
    }
}