using UnityEngine;
using NUnit.Framework;
using Unity.Mathematics;

public class GhostRoadTest
{
    float3 stride = Constants.MinLaneLength * new float3(1, 0, 1);

    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
    }

    [Test]
    public void DoesNotBuildByItself()
    {
        Assert.True(ThereIsNoGhostRoad());
    }

    [Test]
    public void AfterAssigingStartGhostRoadDoesNotBuild()
    {
        Build.HandleBuildCommand(0);
        Assert.True(ThereIsNoGhostRoad());
    }

    [Test]
    public void AfterAssigningPivotGhostRoadsBuildsOnHover()
    {
        Build.HandleBuildCommand(0);
        Build.HandleBuildCommand(stride);
        Assert.True(ThereIsNoGhostRoad());
        Build.HandleHover(stride * 2);
        Assert.False(ThereIsNoGhostRoad());
    }

    [Test]
    public void GhostRoadDeletesItselfAfterRoadBuilt()
    {
        Build.HandleBuildCommand(0);
        Build.HandleBuildCommand(stride);
        Build.HandleHover(stride * 2);
        Build.HandleBuildCommand(2 * stride);
        Assert.True(ThereIsNoGhostRoad());
        Assert.AreEqual(1, Game.Roads.Count);
    }

    [Test]
    public void GhostRoadDeletesItselfAfterBuildFailure()
    {
        Build.HandleBuildCommand(0);
        Build.HandleBuildCommand(stride);
        Build.HandleHover(stride * 2);
        Build.HandleBuildCommand(0); // build should fail because road is too bent
        Assert.AreEqual(0, Game.Roads.Count);
        Assert.True(ThereIsNoGhostRoad());
    }

    [Test]
    public void MultipleHoversDoesNotCreateMultipleGhostRoad()
    {
        Build.HandleBuildCommand(0);
        Build.HandleBuildCommand(stride);
        Build.HandleHover(stride * 2);
        Build.HandleHover(stride * 2);
        Build.HandleHover(stride * 2);
        Assert.AreEqual(1, Game.Roads.Count);
        Assert.False(ThereIsNoGhostRoad());
    }

    [Test]
    public void HoverSetsStartTarget()
    {
        Build.HandleHover(0);
        Assert.NotNull(Build.StartTarget);
    }

    [Test]
    public void GhostRoadDoesNotInterfereSnap()
    {
        Road road0 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        Build.HandleBuildCommand(0);
        Build.HandleBuildCommand(stride);
        Build.HandleHover(2 * stride);
        Assert.True(Build.EndTarget.Snapped);
        Build.HandleHover(2 * stride);
        Assert.True(Build.EndTarget.Snapped);
    }

    [Test]
    public void SnapAtTwoEndsAndGhostRoad()
    {
        float3 up = new(0, 0, Constants.MinLaneLength);
        float3 offset = new(Constants.MinLaneLength, 0, 0);
        RoadBuilder.Single(0, up, 2 * up, 2);
        RoadBuilder.Single(offset + 3 * up, offset + 4 * up, offset + 5 * up, 2);

        Build.LaneCount = 2;
        Build.HandleBuildCommand(2 * up);

        Build.HandleHover(Vector3.Lerp(2 * up, offset + 3 * up, 0.5f));
        Build.HandleBuildCommand(Vector3.Lerp(2 * up, offset + 3 * up, 0.5f));
        Build.HandleHover(Vector3.Lerp(2 * up, offset + 3 * up, 0.5f));
        
        Build.HandleHover(offset + 3 * up);
        Build.HandleBuildCommand(offset + 3 * up);
    }

    [Test]
    public void OneToThreeTestBuildNoError()
    {
        RoadBuilder.Single(0, stride, 2 * stride, 1);
        Build.LaneCount = 3;
        Build.HandleHover(2 * stride);
        Build.HandleBuildCommand(2 * stride);
        Build.HandleHover(3 * stride);
        Build.HandleBuildCommand(3 * stride);
        Build.HandleHover(4 * stride);
        Assert.AreEqual(2, Game.Roads.Count);
        Build.HandleBuildCommand(4 * stride);
        Assert.AreEqual(2, Game.Roads.Count);
    }


    bool ThereIsNoGhostRoad()
    {
        foreach (Road r in Game.Roads.Values)
            if (r.IsGhost)
                return false;
        return Build.GhostRoads.Count == 0;
    }
}