using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

public class CompoundOperationTest
{
    float3 stride = Constants.MinLaneLength * new float3(1, 0, 0);

    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
    }

    [Test]
    public void CombineAndReplace()
    {
        Road road = RoadBuilder.Single(0, 2 * stride, 4 * stride, 1);
        Road left = Divide.DivideRoad(road, road.Length / 2).Left;
        Road combined = Combine.CombineRoads(left.EndIntersection);
        Game.HoveredRoad = combined;

        Build.HandleHover(3 * stride);
        Assert.True(Build.ReplaceSuggestionOn);
        Assert.True(Build.StartTarget.Snapped);
        Assert.True(Build.EndTarget.Snapped);

        Build.HandleBuildCommand(3 * stride);
    }
    [Test]
    public void CombineAndDivideMultipleTimes()
    {
        Road left = RoadBuilder.Single(0, 4 * stride, 8 * stride, 2);

        SubRoads subRoads = Divide.HandleDivideCommand(left, 4.5f * stride);

        Road combined;
        for (int i = 0; i < 5; i++)
        {
            combined = Combine.CombineRoads(subRoads.Left.EndIntersection);
            Assert.True(MyNumerics.IsApproxEqual(4 * math.length(2 * stride), combined.Length));
            subRoads = Divide.HandleDivideCommand(combined, 4.5f * stride + new float3(0, 2, 2));
            Assert.NotNull(subRoads);
        }
    }

    [Test]
    public void SlidingCombindAndDivide()
    {
        Road combined = RoadBuilder.Single(0, 2 * stride, 4 * stride, 2);
        float length = combined.Length;
        SubRoads subRoads;
        float step = math.length(2 * stride) / 10;

        for (float distance = math.length(stride + step); distance < math.length(3 * stride - step); distance += step)
        {
            subRoads = Divide.DivideRoad(combined, distance);
            combined = Combine.CombineRoads(subRoads.Left.EndIntersection);
            Assert.AreEqual(2, Game.Intersections.Count);
        }
    }
}