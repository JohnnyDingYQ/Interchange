using NUnit.Framework;
using Unity.Mathematics;

public class GhostRoadTest
{
    float3 stride = Constants.MinimumLaneLength * new float3(1, 0, 1);
    
    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
    }

    [Test]
    public void DoesNotBuildByItself()
    {
        Assert.False(Game.Roads.ContainsKey(Constants.GhostRoadId));
    }

    [Test]
    public void AfterAssigingStartGhostRoadDoesNotBuild()
    {
        Build.HandleBuildCommand(0);
        Assert.False(Game.Roads.ContainsKey(Constants.GhostRoadId));   
    }

    [Test]
    public void AfterAssigningPivotGhostRoadsBuildsOnHover()
    {
        Build.HandleBuildCommand(0);
        Build.HandleBuildCommand(stride);
        Assert.False(Game.Roads.ContainsKey(Constants.GhostRoadId));   
        Build.HandleHover(stride * 2);
        Assert.True(Game.Roads.ContainsKey(Constants.GhostRoadId));
    }

    [Test]
    public void GhostRoadDeletesItselfAfterRoadBuilt()
    {
        Build.HandleBuildCommand(0);
        Build.HandleBuildCommand(stride);
        Build.HandleHover(stride * 2);
        Build.HandleBuildCommand(2 * stride);
        Assert.False(Game.Roads.ContainsKey(Constants.GhostRoadId));   
        Assert.AreEqual(1, Game.Roads.Count);
    }

    [Test]
    public void GhostRoadDeletesItselfAfterBuildFailure()
    {
        Build.HandleBuildCommand(0);
        Build.HandleBuildCommand(stride);
        Build.HandleHover(stride * 2);
        Build.HandleBuildCommand(0); // should fail because road is too bent
        Assert.AreEqual(0, Game.Roads.Count);
        Assert.False(Game.Roads.ContainsKey(Constants.GhostRoadId));
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
        Assert.True(Game.Roads.ContainsKey(Constants.GhostRoadId));
    }
}