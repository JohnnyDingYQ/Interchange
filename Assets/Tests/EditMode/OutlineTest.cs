using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

public class OutlineTest
{
    float3 stride = Constants.MinimumLaneLength * new float3(1, 0, 0);

    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
    }

    [Test]
    public void SingleOneLaneRoad()
    {
        RoadBuilder.Single(0, stride, 2 * stride, 1);
        Assert.True(AllRoadsOutLineValid());
    }

    [Test]
    public void OneLaneRepeated_OnEnd()
    {
        RoadBuilder.Single(0, stride, 2 * stride, 1);
        RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        Assert.True(AllRoadsOutLineValid());
    }

    [Test]
    public void OneLaneRepeated_OnStart()
    {
        RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        RoadBuilder.Single(0, stride, 2 * stride, 1);
        Assert.True(AllRoadsOutLineValid());
    }

    [Test]
    public void ThreeLaneRepeated_OnEnd()
    {
        RoadBuilder.Single(0, stride, 2 * stride, 3);
        RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 3);
        Assert.True(AllRoadsOutLineValid());
    }

    [Test]
    public void ThreeLaneRepeated_OnStart()
    {
        RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 3);
        RoadBuilder.Single(0, stride, 2 * stride, 3);
        Assert.True(AllRoadsOutLineValid());
    }

    [Test]
    public void TwoLaneRoadBranched_OnStart()
    {
        Road road1 = RoadBuilder.Single(4 * stride, 5 * stride, 6 * stride, 2);
        float3 offset = road1.Lanes.First().StartPos - road1.StartPos;
        RoadBuilder.Single(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Assert.True(AllRoadsOutLineValid());
        RoadBuilder.Single(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
        Assert.True(AllRoadsOutLineValid());
    }

    [Test]
    public void TwoLaneRoadBranched_OnEnd()
    {
        Road road1 = RoadBuilder.Single(0, stride, 2 * stride, 2);
        float3 offset = road1.Lanes.First().EndPos - road1.EndPos;
        RoadBuilder.Single(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Assert.True(AllRoadsOutLineValid());
        RoadBuilder.Single(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
        Assert.True(AllRoadsOutLineValid());
    }

    [Test]
    public void OneTwoThreeLanesRoadInAtunction_OnEnd()
    {
        Road road1 = RoadBuilder.Single(0, stride, 2 * stride, 3);
        float3 offset = road1.Lanes.First().EndPos - road1.EndPos;
        RoadBuilder.Single(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Assert.True(AllRoadsOutLineValid());
        RoadBuilder.Single(2 * stride - offset / 2, 3 * stride - offset / 2, 4 * stride - offset / 2, 2);
        Assert.True(AllRoadsOutLineValid());
    }

    [Test]
    public void OneTwoThreeLanesRoadInAtunction_OnStart()
    {
        Road road1 = RoadBuilder.Single(4 * stride, 5 * stride, 6 * stride, 3);
        float3 offset = road1.Lanes.First().EndPos - road1.EndPos;
        RoadBuilder.Single(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Assert.True(AllRoadsOutLineValid());
        RoadBuilder.Single(2 * stride - offset / 2, 3 * stride - offset / 2, 4 * stride - offset / 2, 2);
        Assert.True(AllRoadsOutLineValid());
    }

    [Test]
    public void OneThreeLanetoThreeOneLane_SideFirst_OnEnd()
    {
        Road road1 = RoadBuilder.Single(0, stride, 2 * stride, 3);
        float3 offset = road1.Lanes.First().EndPos - road1.EndPos;
        RoadBuilder.Single(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Assert.True(AllRoadsOutLineValid());
        RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        Assert.True(AllRoadsOutLineValid());
        RoadBuilder.Single(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
        Assert.True(AllRoadsOutLineValid());
    }

    [Test]
    public void OneThreeLanetoThreeOneLane_MidFirst_OnEnd()
    {
        Road road1 = RoadBuilder.Single(0, stride, 2 * stride, 3);
        float3 offset = road1.Lanes.First().EndPos - road1.EndPos;
        RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        Assert.True(AllRoadsOutLineValid());
        RoadBuilder.Single(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Assert.True(AllRoadsOutLineValid());
        RoadBuilder.Single(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
        Assert.True(AllRoadsOutLineValid());
    }

    [Test]
    public void OneThreeLanetoThreeOneLane_SidesFirst_OnEnd()
    {
        Road road1 = RoadBuilder.Single(0, stride, 2 * stride, 3);
        float3 offset = road1.Lanes.First().EndPos - road1.EndPos;
        RoadBuilder.Single(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Assert.True(AllRoadsOutLineValid());
        RoadBuilder.Single(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
        Assert.True(AllRoadsOutLineValid());
        RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        Assert.True(AllRoadsOutLineValid());
    }

    [Test]
    public void OneThreeLanetoThreeOneLane_SideFirst_OnStart()
    {
        Road road1 = RoadBuilder.Single(4 * stride, 5 * stride, 6 * stride, 3);
        float3 offset = road1.Lanes.First().StartPos - road1.StartPos;
        RoadBuilder.Single(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Assert.True(AllRoadsOutLineValid());
        RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        Assert.True(AllRoadsOutLineValid());
        RoadBuilder.Single(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
        Assert.True(AllRoadsOutLineValid());

    }

    [Test]
    public void OneThreeLanetoThreeOneLane_MidFirst_OnStart()
    {
        Road road1 = RoadBuilder.Single(4 * stride, 5 * stride, 6 * stride, 3);
        float3 offset = road1.Lanes.First().StartPos - road1.StartPos;
        RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        Assert.True(AllRoadsOutLineValid());
        RoadBuilder.Single(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Assert.True(AllRoadsOutLineValid());
        RoadBuilder.Single(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
        Assert.True(AllRoadsOutLineValid());
    }

    [Test]
    public void OneThreeLanetoThreeOneLane_SidesFirst_OnStart()
    {
        Road road1 = RoadBuilder.Single(4 * stride, 5 * stride, 6 * stride, 3);
        float3 offset = road1.Lanes.First().StartPos - road1.StartPos;
        RoadBuilder.Single(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        Assert.True(AllRoadsOutLineValid());
        RoadBuilder.Single(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
        Assert.True(AllRoadsOutLineValid());
        RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        Assert.True(AllRoadsOutLineValid());
    }

    bool AllRoadsOutLineValid()
    {
        foreach (Road r in Game.Roads.Values)
        {
            r.LeftOutline.IsPlausible();
            // Debug.Log(r.HasNoneEmptyOutline());
            if (!r.OutLinePlausible())
            {
                Debug.Log("Road " + r.Id + ": Outline not plausible");   
                return false;
            }
            if (!r.HasNoneEmptyOutline())
            {
                Debug.Log(r.Id);   
                return false;
            }
        }
        return true;
    }
    
}