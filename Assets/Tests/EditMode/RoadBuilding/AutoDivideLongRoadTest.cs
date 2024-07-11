using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;

public class AutoDivideLongRoadTest
{
    float3 direction = new(1, 0, 0);
    const float LengthDiffTolerance = 5f;

    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
    }

    [Test]
    public void DivideToTwo()
    {
        RoadBuilder.Many(
            0,
            direction * Constants.MaxLaneLength / 2,
            direction * (Constants.MaxLaneLength + 0.1f),
            1
        );
        Assert.AreEqual(2, Game.Roads.Count);
    }

    [Test]
    public void DivideToFive()
    {
        float targetLength = Constants.MaxLaneLength * 5 - 0.3f;
        RoadBuilder.Many(
            0,
            direction * targetLength / 2,
            direction * targetLength,
            1
        );
        Assert.AreEqual(5, Game.Roads.Count);
        float length = Game.Roads.Values.First().Length;
        foreach (Road road in Game.Roads.Values)
        {
            Assert.True(MyNumerics.AreNumericallyEqual(length, road.Length, 1));
            length = road.Length;
        }
            
    }

    [Test]
    public void CurvedRoadDividedEvenly()
    {
        float targetLength = Constants.MaxLaneLength * 5 - 0.3f;
        RoadBuilder.Many(
            0,
            targetLength / 2 * new float3(1, 0, 0),
            targetLength * new float3(1, 0, 1),
            1
        );
        float length = Game.Roads.Values.First().Length;
        foreach (Road road in Game.Roads.Values)
        {
            Assert.True(MyNumerics.AreNumericallyEqual(length, road.Length, LengthDiffTolerance));
            length = road.Length;
        }
    }
}