using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

public class BuildTargetTest
{
    float3 stride = Constants.MinLaneLength * new float3(1, 0, 0);
    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
    }

    [Test]
    public void NoSnap()
    {
        BuildTargets bt1 = Snapping.Snap(0, 1, Side.End);
        BuildTargets bt2 = Snapping.Snap(0, 2, Side.Start);
        Assert.False(bt1.Snapped);
        Assert.False(bt2.Snapped);
        Assert.AreEqual(new float3(0), bt1.ClickPos);
        Assert.AreEqual(new float3(0), bt2.ClickPos);
    }

    [Test]
    public void RepeatingOneLaneRoad_OnEnd()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
        BuildTargets bt = Snapping.Snap(2 * stride, 1, Side.Start);

        Assert.True(bt.Snapped);
        Assert.True(MyNumerics.IsApproxEqual(2 * stride, bt.Pos));
        Assert.AreSame(road.EndIntersection, bt.Intersection);
        Assert.AreEqual(0, bt.Offset);
    }

    [Test]
    public void RepeatingTwoLaneRoad_OnStart()
    {
        RoadBuilder.Single(0, stride, 2 * stride, 2);
        BuildTargets bt = Snapping.Snap(0, 2, Side.End);
        Road road = Game.Roads.Values.First();

        Assert.True(bt.Snapped);
        Assert.AreEqual(new float3(0), bt.Pos);
        Assert.AreSame(road.StartIntersection, bt.Intersection);
        Assert.AreEqual(0, bt.Offset);
    }

    [Test]
    public void AttachOneLaneToTwoLane_OnEnd()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 2);
        Lane lane = road.Lanes[0];
        BuildTargets bt = Snapping.Snap(lane.EndPos + 0.9f * Constants.BuildSnapTolerance * new float3(1, 0, 0), 1, Side.Start);

        Assert.True(bt.Snapped);
        Assert.True(MyNumerics.IsApproxEqual(lane.EndPos, bt.Pos));
        Assert.AreSame(road.EndIntersection, bt.Intersection);
        Assert.AreEqual(0, bt.Offset);
    }

    [Test]
    public void AttachTwoLaneToThreeLane_OnStart()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 3);
        Lane lane0 = road.Lanes[0];
        Lane lane1 = road.Lanes[1];
        float3 midPoint = Vector3.Lerp(lane0.StartPos, lane1.StartPos, 0.5f);
        BuildTargets bt = Snapping.Snap(midPoint + 0.9f * Constants.BuildSnapTolerance * new float3(-1, 0, 0), 2, Side.End);

        Assert.True(bt.Snapped);
        Assert.AreEqual(midPoint, bt.Pos);
        Assert.AreSame(road.StartIntersection, bt.Intersection);
        Assert.AreEqual(0, bt.Offset);
    }

    [Test]
    public void LaneExpansionOneLaneToTwoLane_Left()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Vector3 buildPoint = 2 * stride + 0.9f * Constants.BuildSnapTolerance * new float3(0, 0, 1);
        BuildTargets bt = Snapping.Snap(buildPoint, 2, Side.Start);

        Assert.AreSame(road.EndIntersection, bt.Intersection);
        Assert.AreEqual(-1, bt.Offset);

    }

    [Test]
    public void LaneExpansionOneLaneToTwoLane_Right()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Vector3 buildPoint = 2 * stride + 0.9f * Constants.BuildSnapTolerance * new float3(0, 0, -1);
        BuildTargets bt = Snapping.Snap(buildPoint, 2, Side.Start);

        Assert.AreSame(road.EndIntersection, bt.Intersection);
        Assert.AreEqual(0, bt.Offset);
    }

    [Test]
    public void LaneExpansionTwoLaneToThreeLane_Left()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 2);
        Vector3 buildPoint = 2 * stride + 0.9f * Constants.BuildSnapTolerance * new float3(0, 0, 1);
        BuildTargets bt = Snapping.Snap(buildPoint, 3, Side.Start);

        Assert.AreSame(road.EndIntersection, bt.Intersection);
        Assert.AreEqual(-1, bt.Offset);
    }

    [Test]
    public void LaneExpansionTwoLaneToThreeLane_Right()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 2);
        Vector3 buildPoint = 2 * stride + 0.9f * Constants.BuildSnapTolerance * new float3(0, 0, -1);
        BuildTargets bt = Snapping.Snap(buildPoint, 3, Side.Start);
        
        Assert.AreSame(road.EndIntersection, bt.Intersection);
        Assert.AreEqual(0, bt.Offset);
    }

    [Test]
    public void LaneExpansionOneLaneToThreeLane_Mid()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
        BuildTargets bt = Snapping.Snap(2 * stride, 3, Side.Start);

        Assert.AreSame(road.EndIntersection, bt.Intersection);
        Assert.AreEqual(-1, bt.Offset);
    }

    [Test]
    public void LaneExpansionOneLaneToThreeLane_Left()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Vector3 buildPoint = 2 * stride + road.Curve.EndNormal * Constants.LaneWidth;
        BuildTargets bt = Snapping.Snap(buildPoint, 3, Side.Start);

        Assert.AreSame(road.EndIntersection, bt.Intersection);
        Assert.AreEqual(-2, bt.Offset);
    }

    [Test]
    public void LaneExpansionOneLaneToThreeLane_Right()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Vector3 buildPoint = 2 * stride + -1 * Constants.LaneWidth * road.Curve.EndNormal;
        BuildTargets bt = Snapping.Snap(buildPoint, 3, Side.Start);

        Assert.AreSame(road.EndIntersection, bt.Intersection);
        Assert.AreEqual(0, bt.Offset);
        
    }

    [Test]
    public void LaneContractionThreeLanetoOneLane()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
        BuildTargets bt = Snapping.Snap(0, 3, Side.End);

        Assert.AreSame(road.StartIntersection, bt.Intersection);
        Assert.AreEqual(-1, bt.Offset);
    }

    [Test]
    public void InitialRoadCanBeSnappedAtStart()
    {
        RoadBuilder.Single(0, stride, 2 * stride, 1);
        BuildTargets bt = Snapping.Snap(0, 3, Side.End);
        Assert.True(bt.Snapped);
    }

    [Test]
    public void NodeUnsnappableAtMinimumElevation()
    {
        RoadBuilder.Single(new float3(0, -2, 0), stride, 2 * stride, 1);
        BuildTargets bt = Snapping.Snap(0, 1, Side.End);
        
        Assert.False(bt.Snapped);
    }

    [Test]
    public void SnapEndButInLaneAlreadyExists()
    {
        Road road0 = RoadBuilder.Single(0, stride, 2 * stride, 1);
        BuildTargets bt = Snapping.Snap(2 * stride, 1, Side.End);

        Assert.False(bt.Snapped);
    }

    [Test]
    public void ProcessSnapDoesNotAddExtraNodesToIntersections()
    {
        Road road0 = RoadBuilder.Single(0, stride, 2 * stride, 2);
        BuildTargets bt = Snapping.Snap(2 * stride, 3, Side.Start);

        Assert.AreEqual(2, road0.EndIntersection.Nodes.Count);
    }
}