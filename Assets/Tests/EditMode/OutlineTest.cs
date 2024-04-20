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
        Assert.True(AllRoadsOutLineValid());
    }

    [Test]
    public void OneLaneRepeated_OnStart()
    {
        RoadBuilder.Build(2 * stride, 3 * stride, 4 * stride, 1);
        RoadBuilder.Build(0, stride, 2 * stride, 1);
        Assert.True(AllRoadsOutLineValid());
    }

    [Test]
    public void ThreeLaneRepeated_OnEnd()
    {
        RoadBuilder.Build(0, stride, 2 * stride, 3);
        RoadBuilder.Build(2 * stride, 3 * stride, 4 * stride, 3);
        Assert.True(AllRoadsOutLineValid());
    }

    [Test]
    public void ThreeLaneRepeated_OnStart()
    {
        RoadBuilder.Build(2 * stride, 3 * stride, 4 * stride, 3);
        RoadBuilder.Build(0, stride, 2 * stride, 3);
        Assert.True(AllRoadsOutLineValid());
    }

    [Test]
    public void TwoLaneRoadBranched_OnStart()
    {
        Road road1 = RoadBuilder.Build(4 * stride, 5 * stride, 6 * stride, 2);
        float3 offset = road1.Lanes.First().StartPos - road1.StartPos;
        RoadBuilder.Build(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Assert.True(AllRoadsOutLineValid());
        RoadBuilder.Build(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
        Assert.True(AllRoadsOutLineValid());
    }

    [Test]
    public void TwoLaneRoadBranched_OnEnd()
    {
        Road road1 = RoadBuilder.Build(0, stride, 2 * stride, 2);
        float3 offset = road1.Lanes.First().EndPos - road1.EndPos;
        RoadBuilder.Build(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Assert.True(AllRoadsOutLineValid());
        RoadBuilder.Build(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
        Assert.True(AllRoadsOutLineValid());
    }

    [Test]
    public void OneTwoThreeLanesRoadInAtunction_OnEnd()
    {
        Road road1 = RoadBuilder.Build(0, stride, 2 * stride, 3);
        float3 offset = road1.Lanes.First().EndPos - road1.EndPos;
        RoadBuilder.Build(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Assert.True(AllRoadsOutLineValid());
        RoadBuilder.Build(2 * stride - offset / 2, 3 * stride - offset / 2, 4 * stride - offset / 2, 2);
        Assert.True(AllRoadsOutLineValid());
    }

    [Test]
    public void OneTwoThreeLanesRoadInAtunction_OnStart()
    {
        Road road1 = RoadBuilder.Build(4 * stride, 5 * stride, 6 * stride, 2);
        float3 offset = road1.Lanes.First().EndPos - road1.EndPos;
        RoadBuilder.Build(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Assert.True(AllRoadsOutLineValid());
        RoadBuilder.Build(2 * stride - offset / 2, 3 * stride - offset / 2, 4 * stride - offset / 2, 2);
        Assert.True(AllRoadsOutLineValid());
    }

    [Test]
    public void OneThreeLanetoThreeOneLane_SideFirst_OnEnd()
    {
        Road road1 = RoadBuilder.Build(0, stride, 2 * stride, 3);
        float3 offset = road1.Lanes.First().EndPos - road1.EndPos;
        RoadBuilder.Build(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Assert.True(AllRoadsOutLineValid());
        RoadBuilder.Build(2 * stride, 3 * stride, 4 * stride, 1);
        Assert.True(AllRoadsOutLineValid());
        RoadBuilder.Build(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
        Assert.True(AllRoadsOutLineValid());
    }

    [Test]
    public void OneThreeLanetoThreeOneLane_MidFirst_OnEnd()
    {
        Road road1 = RoadBuilder.Build(0, stride, 2 * stride, 3);
        float3 offset = road1.Lanes.First().EndPos - road1.EndPos;
        RoadBuilder.Build(2 * stride, 3 * stride, 4 * stride, 1);
        Assert.True(AllRoadsOutLineValid());
        RoadBuilder.Build(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Assert.True(AllRoadsOutLineValid());
        RoadBuilder.Build(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
        Assert.True(AllRoadsOutLineValid());
    }

    [Test]
    public void OneThreeLanetoThreeOneLane_SidesFirst_OnEnd()
    {
        Road road1 = RoadBuilder.Build(0, stride, 2 * stride, 3);
        float3 offset = road1.Lanes.First().EndPos - road1.EndPos;
        RoadBuilder.Build(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Assert.True(AllRoadsOutLineValid());
        RoadBuilder.Build(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
        Assert.True(AllRoadsOutLineValid());
        RoadBuilder.Build(2 * stride, 3 * stride, 4 * stride, 1);
        Assert.True(AllRoadsOutLineValid());
    }

    [Test]
    public void OneThreeLanetoThreeOneLane_SideFirst_OnStart()
    {
        Road road1 = RoadBuilder.Build(4 * stride, 5 * stride, 6 * stride, 3);
        float3 offset = road1.Lanes.First().StartPos - road1.StartPos;
        RoadBuilder.Build(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Assert.True(AllRoadsOutLineValid());
        RoadBuilder.Build(2 * stride, 3 * stride, 4 * stride, 1);
        Assert.True(AllRoadsOutLineValid());
        RoadBuilder.Build(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
        Assert.True(AllRoadsOutLineValid());

    }

    [Test]
    public void OneThreeLanetoThreeOneLane_MidFirst_OnStart()
    {
        Road road1 = RoadBuilder.Build(4 * stride, 5 * stride, 6 * stride, 3);
        float3 offset = road1.Lanes.First().StartPos - road1.StartPos;
        RoadBuilder.Build(2 * stride, 3 * stride, 4 * stride, 1);
        Assert.True(AllRoadsOutLineValid());
        RoadBuilder.Build(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Assert.True(AllRoadsOutLineValid());
        RoadBuilder.Build(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
        Assert.True(AllRoadsOutLineValid());
    }

    [Test]
    public void OneThreeLanetoThreeOneLane_SidesFirst_OnStart()
    {
        Road road1 = RoadBuilder.Build(4 * stride, 5 * stride, 6 * stride, 3);
        float3 offset = road1.Lanes.First().StartPos - road1.StartPos;
        RoadBuilder.Build(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Assert.True(AllRoadsOutLineValid());
        RoadBuilder.Build(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
        Assert.True(AllRoadsOutLineValid());
        RoadBuilder.Build(2 * stride, 3 * stride, 4 * stride, 1);
        Assert.True(AllRoadsOutLineValid());
    }

    bool AllRoadsOutLineValid()
    {
        foreach (Road r in Game.Roads.Values)
        {
            if (!r.OutLinePlausible())
                return false;
            if (!r.HasNoneEmptyOutline())
                return false;
        }
        return true;
    }
    
}