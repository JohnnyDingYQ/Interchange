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
    public void HoverOneLaneWithThreeLane()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Build.LaneCount = 3;
        Build.HandleHover(stride);

        // Assert.NotNull(Build.StartTarget);
        // Assert.NotNull(Build.EndTarget);
    }
}