using System.Collections.Generic;
using NUnit.Framework;

public class IntersectionTest
{
    Intersection intersection;
    [SetUp]
    public void SetUp()
    {
        intersection = new();
    }

    [Test]
    public void IsLazyIntializing()
    {
        Assert.IsNotNull(intersection.NodeWithLane);
        
    }

    [Test]
    public void DetermineRepeatingRoad_1Lane()
    {
        Lane left = new() {
            Start = 0,
            End = 1
        };
        Lane right = new() {
            Start = 1,
            End = 2
        };
        List<Lane> lanes = new() {left, right};
        intersection.NodeWithLane[1] = lanes;
        // intersecito
        // Assert.True(intersection.IsRepeating());
    }
}