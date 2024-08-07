using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

public class IntersectionSafetyTest
{
    float3 stride = Constants.MinLaneLength * new float3(0, 0, 1);

    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
    }

    [Test]
    public void SimpleOneLaneRoadIsSafe()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);

        Assert.IsTrue(road.StartIntersection.IsSafe);
        Assert.IsTrue(road.EndIntersection.IsSafe);
    }


    [Test]
    public void ConnectedOneLaneRoadIsSafe()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Road connected = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);

        Assert.IsTrue(road.StartIntersection.IsSafe);
        Assert.IsTrue(road.EndIntersection.IsSafe);
        Assert.IsTrue(connected.EndIntersection.IsSafe);
    }

    [Test]
    public void ConnectedThreeLaneRoadIsSafe()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 3);
        Road connected = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 3);

        Assert.IsTrue(road.StartIntersection.IsSafe);
        Assert.IsTrue(road.EndIntersection.IsSafe);
        Assert.IsTrue(connected.EndIntersection.IsSafe);
    }

    [Test]
    public void OneToTwoLaneIsSafe()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Road connected = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 2);

        Assert.IsTrue(road.StartIntersection.IsSafe);
        Assert.IsTrue(road.EndIntersection.IsSafe);
        Assert.AreSame(road.EndIntersection, connected.StartIntersection);
        Assert.IsTrue(connected.EndIntersection.IsSafe);
    }

    [Test]
    public void TwoToThreeIsSafe()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 2);
        Road connected = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 3);

        Assert.IsTrue(road.StartIntersection.IsSafe);
        Assert.IsTrue(road.EndIntersection.IsSafe);
        Assert.IsTrue(connected.EndIntersection.IsSafe);
    }

    [Test]
    public void ThreeToTwoIsSafe()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 3);
        Road connected = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 2);

        Assert.IsTrue(road.StartIntersection.IsSafe);
        Assert.IsTrue(road.EndIntersection.IsSafe);
        Assert.IsTrue(connected.EndIntersection.IsSafe);
    }

    [Test]
    public void OneLaneChangeThanTwoToThreeIsSafe()
    {
        Road forLaneChange = RoadBuilder.Single(-2 * stride, -stride, 0, 2);
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 2);
        Road connected = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 3);

        Assert.IsTrue(forLaneChange.StartIntersection.IsSafe);
        Assert.IsTrue(road.StartIntersection.IsSafe);
        Assert.IsTrue(road.EndIntersection.IsSafe);
        Assert.IsTrue(connected.EndIntersection.IsSafe);
    }

    [Test]
    public void TwoLaneChangeThanThreeToTwoIsSafe()
    {
        Road laneChange1 = RoadBuilder.Single(-4 * stride, -3 * stride, -2 * stride, 3);
        Road laneChange2 = RoadBuilder.Single(-2 * stride, -stride, 0, 3);
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 3);
        Road connected = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 2);

        Assert.IsTrue(laneChange1.StartIntersection.IsSafe);
        Assert.IsTrue(laneChange2.StartIntersection.IsSafe);
        Assert.IsTrue(road.StartIntersection.IsSafe);
        Assert.IsTrue(road.EndIntersection.IsSafe);
        Assert.IsTrue(connected.EndIntersection.IsSafe);
    }

    [Test]
    public void TwoOneToThreeBranchIsSafe()
    {
        Road threeLane = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 3);
        float3 twoLaneEndPoint = (threeLane.Lanes[0].StartPos + threeLane.Lanes[1].StartPos) / 2;
        float3 oneLaneEndPoint = threeLane.Lanes[2].StartPos;
        Road twoLane = RoadBuilder.Single(twoLaneEndPoint - 2 * stride, twoLaneEndPoint - stride, twoLaneEndPoint, 2);
        Road oneLane = RoadBuilder.Single(oneLaneEndPoint - 2 * stride, oneLaneEndPoint - stride, oneLaneEndPoint, 1);

        Assert.IsTrue(twoLane.StartIntersection.IsSafe);
        Assert.IsTrue(oneLane.StartIntersection.IsSafe);
        Assert.IsTrue(threeLane.StartIntersection.IsSafe);
    }

    [Test]
    public void TwoOneToThreeSafeScenario()
    {
        Road threeLane = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 3);
        float3 twoLaneEndPoint = (threeLane.Lanes[0].StartPos + threeLane.Lanes[1].StartPos) / 2;
        float3 oneLaneEndPoint = threeLane.Lanes[2].StartPos;
        Road twoLane = RoadBuilder.Single(twoLaneEndPoint - 2 * stride, twoLaneEndPoint - stride, twoLaneEndPoint, 2);
        Road twoLaneBefore = RoadBuilder.Single(twoLaneEndPoint - 4 * stride, twoLaneEndPoint - 3 * stride, twoLaneEndPoint - 2 * stride, 2);
        Road oneLane = RoadBuilder.Single(oneLaneEndPoint - 2 * stride, oneLaneEndPoint - stride, oneLaneEndPoint, 1);

        Assert.IsTrue(twoLaneBefore.StartIntersection.IsSafe);
        Assert.IsTrue(twoLane.StartIntersection.IsSafe);
        Assert.IsTrue(oneLane.StartIntersection.IsSafe);
        Assert.IsTrue(threeLane.StartIntersection.IsSafe);
    }

    [Test]
    public void LaneChangeUpdatesUnsafeIntersectionInFront()
    {
        Road threeLane = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 3);
        float3 otherStart = threeLane.Lanes[0].EndPos;
        RoadBuilder.Single(4 * stride, 5 * stride, 6 * stride, 1);
        RoadBuilder.Single(otherStart, otherStart + stride, otherStart + 2 * stride, 1);
        Assert.IsFalse(threeLane.EndIntersection.IsSafe);

        Road laneChangeA = RoadBuilder.Single(0, stride, 2 * stride, 3);
        Assert.IsFalse(threeLane.EndIntersection.IsSafe);

        Road laneChangeB = RoadBuilder.Single(-2 * stride, -stride, 0, 3);
        Assert.IsTrue(threeLane.EndIntersection.IsSafe);

        Game.RemoveRoad(laneChangeB);
        Assert.IsFalse(threeLane.EndIntersection.IsSafe);
    }

    [Test]
    public void LaneChangeLikeIntersectionTest()
    {
        Road threeLane = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 3);
        float3 otherStart = threeLane.Lanes[0].EndPos;
        RoadBuilder.Single(4 * stride, 5 * stride, 6 * stride, 1);
        RoadBuilder.Single(otherStart, otherStart + stride, otherStart + 2 * stride, 1);
        Assert.IsFalse(threeLane.EndIntersection.IsSafe);

        Road laneChangeA = RoadBuilder.Single(0, stride, 2 * stride, 3);
        Assert.IsFalse(threeLane.EndIntersection.IsSafe);

        Road laneChangeB = RoadBuilder.Single(-2 * stride, -stride, 0, 2);
        Assert.IsTrue(threeLane.EndIntersection.IsSafe);
    }
}