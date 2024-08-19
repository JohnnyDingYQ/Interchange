using UnityEngine;
using NUnit.Framework;
using Unity.Mathematics;
using System.Collections.Generic;
using System.Linq;


public class GoreAreaTest
{
    float3 stride = Constants.MinLaneLength * new float3(0, 0, 1);

    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
    }

    [Test]
    public void TwoToOne()
    {
        Road left = RoadBuilder.Single(0, stride, 2 * stride, 2);
        float3 offset = left.Curve.EndNormal * Constants.LaneWidth / 2;
        RoadBuilder.Single(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        IEnumerable<GoreArea> goreAreas = left.EndIntersection.GetGoreAreas();

        Assert.AreEqual(0, goreAreas.Count());
    }

    [Test]
    public void TwoToOneOne()
    {
        Road two = RoadBuilder.Single(0, stride, 2 * stride, 2);
        float3 offset = two.Curve.EndNormal * Constants.LaneWidth / 2;
        Road left = RoadBuilder.Single(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Road right = RoadBuilder.Single(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
        IEnumerable<GoreArea> goreAreas = two.EndIntersection.GetGoreAreas();

        Assert.AreEqual(1, goreAreas.Count());
        HashSet<GoreArea> expected = new() { new(left, right, Side.Start) };
        Assert.True(expected.SetEquals(goreAreas));
    }

    [Test]
    public void ThreeToOneOneOne()
    {
        Road three = RoadBuilder.Single(0, stride, 2 * stride, 3);
        float3 offset = three.Curve.EndNormal * Constants.LaneWidth;
        Road left = RoadBuilder.Single(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Road mid = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        Road right = RoadBuilder.Single(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
        IEnumerable<GoreArea> goreAreas = three.EndIntersection.GetGoreAreas();

        Assert.AreEqual(2, goreAreas.Count());
        HashSet<GoreArea> expected = new() { new(left, mid, Side.Start), new(mid, right, Side.Start) };
        Assert.True(expected.SetEquals(goreAreas));
    }

    [Test]
    public void ThreeToTwoOne()
    {
        Road three = RoadBuilder.Single(0, stride, 2 * stride, 3);
        float3 offset = three.Curve.EndNormal * Constants.LaneWidth;
        Road two = RoadBuilder.Single(2 * stride + offset / 2, 3 * stride + offset / 2, 4 * stride + offset / 2, 2);
        Road one = RoadBuilder.Single(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
        IEnumerable<GoreArea> goreAreas = three.EndIntersection.GetGoreAreas();

        Assert.AreEqual(1, goreAreas.Count());
        HashSet<GoreArea> expected = new() { new(two, one, Side.Start) };
        Assert.True(expected.SetEquals(goreAreas));
    }
}