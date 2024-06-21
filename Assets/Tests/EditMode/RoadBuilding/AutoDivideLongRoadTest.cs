using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;

public class AutoDivideLongRoadTest
{
    float3 direction = new(1, 0, 0);
    const float LengthDiffTolerance = 5f;
    Dictionary<uint, Road> Roads;


    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
        Roads = Game.Roads;
    }

    [Test]
    public void DivideToTwo()
    {
        RoadBuilder.B(
            0,
            direction * Constants.MaximumLaneLength / 2,
            direction * (Constants.MaximumLaneLength + 0.1f),
            1
        );
        Assert.AreEqual(2, Roads.Count);
    }

    [Test]
    public void DivideToFive()
    {
        float targetLength = Constants.MaximumLaneLength * 5 - 0.3f;
        RoadBuilder.B(
            0,
            direction * targetLength / 2,
            direction * targetLength,
            1
        );
        Assert.AreEqual(5, Roads.Count);
        float length = Roads.Values.First().Length;
        foreach (Road road in Roads.Values)
        {
            Assert.True(MyNumerics.AreNumericallyEqual(length, road.Length));
            length = road.Length;
        }
            
    }

    [Test]
    public void CurvedRoadDividedEvenly()
    {
        float targetLength = Constants.MaximumLaneLength * 5 - 0.3f;
        RoadBuilder.B(
            0,
            targetLength / 2 * new float3(1, 0, 0),
            targetLength * new float3(1, 0, 1),
            1
        );
        float length = Roads.Values.First().Length;
        foreach (Road road in Roads.Values)
        {
            Assert.True(MyNumerics.AreNumericallyEqual(length, road.Length, LengthDiffTolerance));
            length = road.Length;
        }
    }
}