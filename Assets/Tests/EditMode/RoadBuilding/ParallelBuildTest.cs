using System.Diagnostics;
using NUnit.Framework;
using Unity.Mathematics;

public class ParallelBuildTest
{

    float3 stride = Constants.MinimumLaneLength * new float3(1, 0, 1);

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
        RoadBuilder.Many(0, stride, 2 * stride, 1);

        Assert.AreEqual(2, Game.Roads.Count);
    }

}