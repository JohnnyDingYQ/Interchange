using NUnit.Framework;
using Unity.Mathematics;

public class ReplaceTest
{
    float3 stride = Constants.MinimumLaneLength * new float3(1, 0, 1);

    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
    }

    [Test]
    public void Replace3LaneWith1Lane()
    {
        RoadBuilder.B(0, stride, 2 * stride, 3);
        RoadBuilder.B(2 * stride, 3 * stride, 4 * stride, 3);
        RoadBuilder.B(4 * stride, 5 * stride, 6 * stride, 3);

        RoadBuilder.B(2 * stride, 3 * stride, 4 * stride, 1);
        
        Assert.AreEqual(3, Game.Roads.Count);
        Assert.AreEqual(12, Game.Nodes.Count);
    }

    [Test]
    public void ShiftingRightByReplacing()
    {
        Road road1 = RoadBuilder.B(0, stride, 2 * stride, 3);
        RoadBuilder.B(road1.Lanes[2].StartPos, stride, road1.Lanes[2].EndPos, 3);
        Assert.AreEqual(1, Game.Roads.Count);
        Assert.AreEqual(6, Game.Nodes.Count);
    }

    [Test]
    public void ShiftingLeftByReplacing()
    {
        Road road1 = RoadBuilder.B(0, stride, 2 * stride, 3);
        RoadBuilder.B(road1.Lanes[0].StartPos, stride, road1.Lanes[0].EndPos, 3);
        Assert.AreEqual(1, Game.Roads.Count);
        Assert.AreEqual(6, Game.Nodes.Count);
    }
}