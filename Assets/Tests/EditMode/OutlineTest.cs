using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;

public class OutlineTest
{
    float3 stride = Constants.MinimumLaneLength * new float3(1, 0, 0);

    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
    }

    [Test]
    public void OneLaneRepeated_OnEnd()
    {
        RoadBuilder.Build(0, stride, 2 * stride, 1);
        RoadBuilder.Build(2 * stride, 3 * stride, 4 * stride, 1);
        Assert.True(AllRoadsOutLinePlausible());
    }

    [Test]
    public void OneLaneRepeated_OnStart()
    {
        RoadBuilder.Build(2 * stride, 3 * stride, 4 * stride, 1);
        RoadBuilder.Build(0, stride, 2 * stride, 1);
        Assert.True(AllRoadsOutLinePlausible());
    }

    [Test]
    public void ThreeLaneRepeated_OnEnd()
    {
        RoadBuilder.Build(0, stride, 2 * stride, 3);
        RoadBuilder.Build(2 * stride, 3 * stride, 4 * stride, 3);
        Assert.True(AllRoadsOutLinePlausible());
    }

    [Test]
    public void ThreeLaneRepeated_OnStart()
    {
        RoadBuilder.Build(2 * stride, 3 * stride, 4 * stride, 3);
        RoadBuilder.Build(0, stride, 2 * stride, 3);
        Assert.True(AllRoadsOutLinePlausible());
    }

    [Test]
    public void TwoLaneRoadBranched_OnStart()
    {
        Road road1 = RoadBuilder.Build(4 * stride, 5 * stride, 6 * stride, 2);
        float3 offset = road1.Lanes.First().StartPos - road1.StartPos;
        RoadBuilder.Build(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Assert.True(AllRoadsOutLinePlausible());
        RoadBuilder.Build(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
        Assert.True(AllRoadsOutLinePlausible());
    }

    [Test]
    public void TwoLaneRoadBranched_OnEnd()
    {
        Road road1 = RoadBuilder.Build(0, stride, 2 * stride, 2);
        float3 offset = road1.Lanes.First().EndPos - road1.EndPos;
        RoadBuilder.Build(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Assert.True(AllRoadsOutLinePlausible());
        RoadBuilder.Build(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
        Assert.True(AllRoadsOutLinePlausible());
    }

    [Test]
    public void OneTwoThreeLanesRoadInAJunction_OnEnd()
    {
        Road road1 = RoadBuilder.Build(0, stride, 2 * stride, 3);
        float3 offset = road1.Lanes.First().EndPos - road1.EndPos;
        RoadBuilder.Build(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Assert.True(AllRoadsOutLinePlausible());
        RoadBuilder.Build(2 * stride - offset / 2, 3 * stride - offset / 2, 4 * stride - offset / 2, 2);
        Assert.True(AllRoadsOutLinePlausible());
    }

    [Test]
    public void OneTwoThreeLanesRoadInAJunction_OnStart()
    {
        Road road1 = RoadBuilder.Build(4 * stride, 5 * stride, 6 * stride, 2);
        float3 offset = road1.Lanes.First().EndPos - road1.EndPos;
        RoadBuilder.Build(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Assert.True(AllRoadsOutLinePlausible());
        RoadBuilder.Build(2 * stride - offset / 2, 3 * stride - offset / 2, 4 * stride - offset / 2, 2);
        Assert.True(AllRoadsOutLinePlausible());
    }

    [Test]
    public void OneThreeLanetoThreeOneLane_SideFirst_OnEnd()
    {
        Road road1 = RoadBuilder.Build(0, stride, 2 * stride, 3);
        float3 offset = road1.Lanes.First().EndPos - road1.EndPos;
        RoadBuilder.Build(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Assert.True(AllRoadsOutLinePlausible());
        RoadBuilder.Build(2 * stride, 3 * stride, 4 * stride, 1);
        Assert.True(AllRoadsOutLinePlausible());
        RoadBuilder.Build(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
        Assert.True(AllRoadsOutLinePlausible());
    }

    [Test]
    public void OneThreeLanetoThreeOneLane_MidFirst_OnEnd()
    {
        Road road1 = RoadBuilder.Build(0, stride, 2 * stride, 3);
        float3 offset = road1.Lanes.First().EndPos - road1.EndPos;
        RoadBuilder.Build(2 * stride, 3 * stride, 4 * stride, 1);
        Assert.True(AllRoadsOutLinePlausible());
        RoadBuilder.Build(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Assert.True(AllRoadsOutLinePlausible());
        RoadBuilder.Build(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
        Assert.True(AllRoadsOutLinePlausible());
    }

    [Test]
    public void OneThreeLanetoThreeOneLane_SidesFirst_OnEnd()
    {
        Road road1 = RoadBuilder.Build(0, stride, 2 * stride, 3);
        float3 offset = road1.Lanes.First().EndPos - road1.EndPos;
        RoadBuilder.Build(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Assert.True(AllRoadsOutLinePlausible());
        RoadBuilder.Build(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
        Assert.True(AllRoadsOutLinePlausible());
        RoadBuilder.Build(2 * stride, 3 * stride, 4 * stride, 1);
        Assert.True(AllRoadsOutLinePlausible());
    }

    [Test]
    public void OneThreeLanetoThreeOneLane_SideFirst_OnStart()
    {
        Road road1 = RoadBuilder.Build(4 * stride, 5 * stride, 6 * stride, 3);
        float3 offset = road1.Lanes.First().StartPos - road1.StartPos;
        RoadBuilder.Build(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Assert.True(AllRoadsOutLinePlausible());
        RoadBuilder.Build(2 * stride, 3 * stride, 4 * stride, 1);
        Assert.True(AllRoadsOutLinePlausible());
        RoadBuilder.Build(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
        Assert.True(AllRoadsOutLinePlausible());

    }

    [Test]
    public void OneThreeLanetoThreeOneLane_MidFirst_OnStart()
    {
        Road road1 = RoadBuilder.Build(4 * stride, 5 * stride, 6 * stride, 3);
        float3 offset = road1.Lanes.First().StartPos - road1.StartPos;
        RoadBuilder.Build(2 * stride, 3 * stride, 4 * stride, 1);
        Assert.True(AllRoadsOutLinePlausible());
        RoadBuilder.Build(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Assert.True(AllRoadsOutLinePlausible());
        RoadBuilder.Build(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
        Assert.True(AllRoadsOutLinePlausible());
    }

    [Test]
    public void OneThreeLanetoThreeOneLane_SidesFirst_OnStart()
    {
        Road road1 = RoadBuilder.Build(4 * stride, 5 * stride, 6 * stride, 3);
        float3 offset = road1.Lanes.First().StartPos - road1.StartPos;
        RoadBuilder.Build(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Assert.True(AllRoadsOutLinePlausible());
        RoadBuilder.Build(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
        Assert.True(AllRoadsOutLinePlausible());
        RoadBuilder.Build(2 * stride, 3 * stride, 4 * stride, 1);
        Assert.True(AllRoadsOutLinePlausible());
    }

    bool AllRoadsOutLinePlausible()
    {
        foreach (Road r in Game.Roads.Values)
            if (!RoadOutLinePlausible(r))
                return false;
        return true;
    }
    bool RoadOutLinePlausible(Road road)
    {
        foreach (RoadOutline outline in new List<RoadOutline>() { road.LeftOutline, road.RightOutline })
        {
            if (outline.Start.Count != 0)
                if (!Utility.AreNumericallyEqual(outline.Start.Last(), outline.Mid.First()))
                    return false;
            if (outline.End.Count != 0)
                if (!Utility.AreNumericallyEqual(outline.End.First(), outline.Mid.Last()))
                    return false;
        }
        return true;
    }
}