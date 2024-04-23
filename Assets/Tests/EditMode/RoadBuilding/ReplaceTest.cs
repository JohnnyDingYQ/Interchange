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
        Road road1 = RoadBuilder.Build(0, stride, 2 * stride, 3);
        Road road2 = RoadBuilder.Build(2 * stride, 3 * stride, 4 * stride, 3);
        Road road3 = RoadBuilder.Build(4 * stride, 5 * stride, 6 * stride, 3);

        Road road4 = RoadBuilder.Build(2 * stride, 3 * stride, 4 * stride, 1);
        
        Assert.AreEqual(3, Game.Roads.Count);
        Assert.AreEqual(12, Game.Nodes.Count);
    }
}