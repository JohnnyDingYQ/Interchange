using UnityEngine;
using NUnit.Framework;
using Unity.Mathematics;

public class GhostRoadTest
{
    float3 stride = Constants.MinimumLaneLength * new float3(1, 0, 1);
    float3 direction = new(1, 0, 0);

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

    // Uncomment if I decide to divide ghost roads again
    // [Test]
    // public void BasicDividedGhostRoad()
    // {
    //     Build.HandleBuildCommand(0);
    //     Build.HandleBuildCommand(Constants.MaximumLaneLength);
    //     Build.HandleHover(2 * Constants.MaximumLaneLength);
    //     Assert.IsTrue(Game.Roads.Count > 1);
    //     Assert.IsFalse(ThereIsNoGhostRoad());
    //     Build.HandleBuildCommand(2 * Constants.MaximumLaneLength);
    //     Assert.IsTrue(ThereIsNoGhostRoad());
    // }

    bool ThereIsNoGhostRoad()
    {
        foreach (Road r in Game.Roads.Values)
            if (r.IsGhost)
                return false;
        return Build.GhostRoads.Count == 0;
    }
}