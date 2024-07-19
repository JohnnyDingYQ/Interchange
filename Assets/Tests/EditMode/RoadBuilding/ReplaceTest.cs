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
        Game.HoveredRoad = road;
        Build.HandleHover(stride);

        Assert.NotNull(Build.StartTarget);
        Assert.NotNull(Build.EndTarget);
    }

    [Test]
    public void ReplaceOneLaneWithThreeLane()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Build.LaneCount = 3;
        Game.HoveredRoad = road;
        Build.HandleHover(stride);
        Build.HandleBuildCommand(stride);

        Assert.AreEqual(1, Game.Roads.Count);
        Assert.AreEqual(6, Game.Nodes.Count);
        Assert.AreEqual(6, Game.Vertices.Count);
        Assert.AreEqual(3, Game.Paths.Count);
        Assert.AreEqual(3, Game.Lanes.Count);
    }

    [Test]
    public void ReplaceThreeLaneWithOneLane()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 3);
        Build.LaneCount = 1;
        Game.HoveredRoad = road;
        Build.HandleHover(stride);
        Build.HandleBuildCommand(stride);

        Assert.AreEqual(1, Game.Roads.Count);
        Assert.AreEqual(2, Game.Nodes.Count);
        Assert.AreEqual(2, Game.Vertices.Count);
        Assert.AreEqual(1, Game.Paths.Count);
        Assert.AreEqual(1, Game.Lanes.Count);
    }
}