using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;

public class AutoCreateIntersectionTest
{
    float3 up = 2f * Constants.MinLaneLength * new float3(0, 0, 1);
    float3 upRight = new float3(1, 0, 1) * Constants.MinLaneLength;
    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
    }

    [Test]
    public void AutoCreateIntersectionAtStart()
    {
        Road road0 = RoadBuilder.Single(0, up, 2 * up, 1);
        Game.HoveredRoad = road0;
        Build.HandleBuildCommand(up);
        Game.HoveredRoad = null;
        Build.HandleBuildCommand(up + upRight);
        Road road1 = Build.HandleBuildCommand(up + upRight * 2).Single();
        IEnumerable<Path> paths = Graph.ShortestPathAStar(road0.Lanes.Single().StartVertex, road1.Lanes.Single().EndVertex);
        Assert.AreEqual(3, Game.Roads.Count);
        Assert.NotNull(paths);
    }

    [Test]
    public void AutoCreateIntersectionAtEnd()
    {
        Road road0 = RoadBuilder.Single(2 * up, up, 0, 1);
        Build.HandleBuildCommand(up + upRight * 2);
        Build.HandleBuildCommand(up + upRight);
        Game.HoveredRoad = road0;
        Road road1 = Build.HandleBuildCommand(up).Single();

        IEnumerable<Path> paths = Graph.ShortestPathAStar(road1.Lanes.Single().StartVertex, road0.Lanes.Single().EndVertex);
        Assert.AreEqual(3, Game.Roads.Count);
        Assert.NotNull(paths);
    }

    [Test]
    public void HoverAndAutoCreateAtEnd()
    {
        Road road0 = RoadBuilder.Single(2 * up, up, 0, 1);
        Build.HandleBuildCommand(up + upRight * 2);
        Build.HandleBuildCommand(up + upRight);
        Game.HoveredRoad = road0;
        Build.HandleHover(up);
        Road road1 = Build.HandleBuildCommand(up).Single();

        IEnumerable<Path> paths = Graph.ShortestPathAStar(road1.Lanes.Single().StartVertex, road0.Lanes.Single().EndVertex);
        Assert.AreEqual(3, Game.Roads.Count);
        Assert.NotNull(paths);
    }

    [Test]
    public void HoverAndAlignPivot()
    {
        Road road0 = RoadBuilder.Single(2 * up, up, 0, 1);
        Build.HandleBuildCommand(up + upRight * 2);
        Build.HandleBuildCommand(up + upRight);
        Game.HoveredRoad = road0;
        Build.HandleHover(up);
        Build.HandleHover(up);

        Road ghost = Game.Roads[Build.GhostRoads.Single()];
        float3 ghostTangent = math.normalize(ghost.BezierSeries.EvaluateTangent(1));
        float3 roadTangent = math.normalize(road0.BezierSeries.EvaluateTangent(1));
        Assert.True(MyNumerics.AreNumericallyEqual(ghostTangent, roadTangent));
    }
}