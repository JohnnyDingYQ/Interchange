using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;

public class ReplaceTest
{
    float3 stride = Constants.MinLaneLength * new float3(1, 0, 1);

    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
    }

    [Test]
    public void Replace3LaneWith1Lane()
    {
        RoadBuilder.Single(0, stride, 2 * stride, 3);
        RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 3);
        RoadBuilder.Single(4 * stride, 5 * stride, 6 * stride, 3);

        RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        
        Assert.AreEqual(3, Game.Roads.Count);
        Assert.AreEqual(12, Game.Nodes.Count);
    }

    [Test]
    public void ShiftingRightByReplacing()
    {
        Road road1 = RoadBuilder.Single(0, stride, 2 * stride, 3);
        RoadBuilder.Single(road1.Lanes[2].StartPos, stride, road1.Lanes[2].EndPos, 3);
        Assert.AreEqual(1, Game.Roads.Count);
        Assert.AreEqual(6, Game.Nodes.Count);
    }

    [Test]
    public void ShiftingLeftByReplacing()
    {
        Road road1 = RoadBuilder.Single(0, stride, 2 * stride, 3);
        RoadBuilder.Single(road1.Lanes[0].StartPos, stride, road1.Lanes[0].EndPos, 3);
        Assert.AreEqual(1, Game.Roads.Count);
        Assert.AreEqual(6, Game.Nodes.Count);
    }

    [Test]
    public void ReplaceTwoRoadsAtOnce()
    {
        Road road1 = RoadBuilder.Single(0, stride, 2 * stride, 3);
        Road road2 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 3);
        RoadBuilder.Many(road1.Lanes[0].StartPos, 2 * stride, road2.Lanes[0].EndPos, 2);
        Assert.False(Game.Roads.Values.Contains(road1));
        Assert.False(Game.Roads.Values.Contains(road2));
    }

    [Test]
    public void ReplaceThreeRoadsAtOnce()
    {
        Road road1 = RoadBuilder.Single(0, stride, 2 * stride, 3);
        Road road2 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 3);
        Road road3 = RoadBuilder.Single(4 * stride, 5 * stride, 6 * stride, 3);
        RoadBuilder.Many(road1.Lanes[0].StartPos, 2 * stride, road3.Lanes[0].EndPos, 1);

        Assert.False(Game.Roads.Values.Contains(road1));
        Assert.False(Game.Roads.Values.Contains(road2));
        Assert.False(Game.Roads.Values.Contains(road3));
    }
}