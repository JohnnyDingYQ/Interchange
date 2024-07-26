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
        Road left = RoadBuilder.Single(0, stride, 2 * stride, 1);
        RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
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
        Road left = RoadBuilder.Single(0, 2 * stride, 4 * stride, 2);
        RoadBuilder.Single(4 * stride, 6 * stride, 8 * stride, 2);
        Road combined = Combine.CombineRoads(left.EndIntersection);

        SubRoads subRoads = Divide.HandleDivideCommand(combined, 4.5f * stride);

        for (int i = 0; i < 10; i++)
        {
            combined = Combine.CombineRoads(subRoads.Left.EndIntersection);
            Assert.True(MyNumerics.IsApproxEqual(4 * math.length(2 * stride), combined.Length));
            subRoads = Divide.HandleDivideCommand(combined, 4.5f * stride + new float3(0, 2, 2));
            Assert.NotNull(subRoads);
        }

        // static float GetRandomFloat(System.Random random, float minValue, float maxValue)
        // {
        //     double range = maxValue - minValue;
        //     double sample = random.NextDouble(); // Generates a random double between 0.0 and 1.0
        //     double scaled = (sample * range) + minValue; // Scales the random value to the desired range
        //     return (float)scaled;
        // }
    }
}