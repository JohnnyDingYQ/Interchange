using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using NUnit.Framework;
using Unity.Mathematics;

public class ReplaceTest
{
    float3 stride = Constants.MinLaneLength * new float3(0, 0, 1);

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

        Assert.True(Build.ReplaceSuggestionOn);
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
        Assert.AreEqual(2, Game.Intersections.Count);
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
        Assert.AreEqual(2, Game.Intersections.Count);
    }

    [Test]
    public void ReplaceConnectedThreeLaneWithOneLane()
    {
        RoadBuilder.Single(-2 * stride, -stride, 0, 3);
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 3);
        Build.LaneCount = 1;
        Game.HoveredRoad = road;
        Build.HandleHover(stride);
        Build.HandleBuildCommand(stride);

        Assert.AreEqual(2, Game.Roads.Count);
        Assert.AreEqual(7, Game.Nodes.Count);
        Assert.AreEqual(8, Game.Vertices.Count);
        Assert.AreEqual(7, Game.Paths.Count);
        Assert.AreEqual(4, Game.Lanes.Count);
        Assert.AreEqual(3, Game.Intersections.Count);
    }

    [Test]
    public void HoverNotBetweenVerticesDoesNotTriggerReplaceSuggestion()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 3);
        Build.LaneCount = 1;
        Game.HoveredRoad = road;

        Build.HandleHover(0);
        Assert.False(Build.ReplaceSuggestionOn);

        Build.HandleHover(1);
        Assert.False(Build.ReplaceSuggestionOn);
    }

    [Test]
    public void SuggestionTargetsCancelsProperly()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 3);
        Build.LaneCount = 1;
        Game.HoveredRoad = road;

        Build.HandleHover(stride);
        Build.HandleHover(0);
        Assert.False(Build.ReplaceSuggestionOn);
        Assert.NotNull(Build.StartTarget);
        Assert.Null(Build.EndTarget);
    }

    [Test]
    public void SuggestionCanceledWhenRoadHoverCancel()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 3);
        Build.LaneCount = 1;
        Game.HoveredRoad = road;

        Build.HandleHover(stride);
        Game.HoveredRoad = null;
        Build.HandleHover(-1);
        Assert.False(Build.ReplaceSuggestionOn);
        Assert.NotNull(Build.StartTarget);
        Assert.Null(Build.EndTarget);
    }

    [Test]
    public void ReplaceThreeLaneWithTwoLane_Left()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 3);
        Build.LaneCount = 2;
        Game.HoveredRoad = road;
        Build.HandleHover(stride - new float3(Constants.LaneWidth / 2, 0, 0));
        Assert.True(Build.ReplaceSuggestionOn);
        Assert.AreEqual(0, Build.StartTarget.Offset);
        Assert.AreEqual(0, Build.EndTarget.Offset);

        List<Road> built = Build.HandleBuildCommand(stride - new float3(Constants.LaneWidth / 2, 0, 0));
        Assert.AreEqual(1, Game.Roads.Count);
        Assert.AreEqual(4, Game.Nodes.Count);
        Assert.AreEqual(4, Game.Vertices.Count);
        Assert.AreEqual(2, Game.Paths.Count);
        Assert.AreEqual(2, Game.Lanes.Count);
        Assert.AreEqual(2, Game.Intersections.Count);
        float3 expectedPos = (road.Lanes[0].StartNode.Pos + road.Lanes[1].StartNode.Pos) / 2;
        Assert.True(MyNumerics.IsApproxEqual(built.Single().StartPos, expectedPos));
    }

    [Test]
    public void DebuggingTest()
    {
        Road original = RoadBuilder.Single(0, stride, 2 * stride, 3);
        Build.LaneCount = 2;
        Game.HoveredRoad = original;
        Build.HandleHover(stride + new float3(Constants.LaneWidth / 2, 0, 0));
        Road intermediate = Build.HandleBuildCommand(stride + new float3(Constants.LaneWidth / 2, 0, 0)).Single();
        Build.LaneCount = 3;
        Game.HoveredRoad = intermediate;
        Build.HandleHover(stride + new float3(Constants.LaneWidth, 0, 0));
        Assert.AreEqual(1, Build.StartTarget.Offset);
        Assert.AreEqual(1, Build.EndTarget.Offset);
        Assert.AreEqual(1, Build.StartTarget.Intersection.Nodes.First().NodeIndex);

        Build.HandleBuildCommand(stride + new float3(Constants.LaneWidth, 0, 0)).Single();
        Assert.AreEqual(1, Game.Roads.Count);
        Assert.AreEqual(6, Game.Nodes.Count);
        Assert.AreEqual(6, Game.Vertices.Count);
        Assert.AreEqual(3, Game.Paths.Count);
        Assert.AreEqual(3, Game.Lanes.Count);
        Assert.AreEqual(2, Game.Intersections.Count);
    }
}