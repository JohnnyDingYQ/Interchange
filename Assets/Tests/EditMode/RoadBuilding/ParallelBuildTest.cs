using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;

public class ParallelBuildTest
{

    float3 stride = Constants.MinLaneLength * new float3(1, 0, 0);

    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
    }

    [Test]
    public void ParallelModeOffByDefault()
    {
        Assert.IsFalse(Build.ParallelBuildOn);
    }

    [Test]
    public void BuildsOneLaneStraightParallelRoad()
    {
        Build.ToggletParallelBuild();
        List<Road> roads = RoadBuilder.Parallel(0, stride, 2 * stride, 1);

        Assert.AreEqual(2, Game.Roads.Count);
        Assert.AreEqual(2, roads.Count);
        Assert.AreEqual(Constants.DefaultParallelSpacing, roads.Last().StartPos.z);
        Assert.AreEqual(Constants.DefaultParallelSpacing, roads.Last().EndPos.z);
    }
    
    [Test]
    public void ConnectedParallelOneLane()
    {
        Build.ToggletParallelBuild();
        List<Road> roads0 = RoadBuilder.Parallel(0, stride, 2 * stride, 1);
        List<Road> roads1 = RoadBuilder.Parallel(2 * stride, 3 * stride, 4 * stride, 1);
        Assert.IsTrue(AreConnected(roads0.First(), roads1.First()));
        Assert.IsTrue(AreConnected(roads1.Last(), roads0.Last()));
    }

    [Test]
    public void ConnectedParallelThreeLane()
    {
        Build.ToggletParallelBuild();
        List<Road> roads0 = RoadBuilder.Parallel(0, stride, 2 * stride, 3);
        List<Road> roads1 = RoadBuilder.Parallel(2 * stride, 3 * stride, 4 * stride, 3);
        Assert.IsTrue(AreConnected(roads0.First(), roads1.First()));
        Assert.IsTrue(AreConnected(roads1.Last(), roads0.Last()));
    }

    [Test]
    public void BuildingBentParallelWorks()
    {
        Build.ToggletParallelBuild();
        List<Road> roads0 = RoadBuilder.Parallel(-2 * stride, -stride, 0, 2);
        List<Road> roads1 = RoadBuilder.Parallel(
            0,
            new(2 * Constants.MinLaneLength, 0, 0),
            new(2 * Constants.MinLaneLength, 0, 2 * Constants.MinLaneLength),
            2
        );
    }

    bool AreConnected(Road left, Road right)
    {
        return left.Lanes.Select(l => l.EndNode).ToHashSet().SetEquals(right.Lanes.Select(l => l.StartNode));
    }

}