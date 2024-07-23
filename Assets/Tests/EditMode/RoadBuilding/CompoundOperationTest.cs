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
}