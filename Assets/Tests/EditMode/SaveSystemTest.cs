using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

public class SaveSystemTest
{
    float3 stride = Constants.MinLaneLength * new float3(1, 0, 1);

    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
    }

    [Test]
    public void VertexUnitTest()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Vertex original = road.Lanes[0].StartVertex;

        Storage storage = new();
        int writtenBytes = storage.Save(original);
        Vertex loaded = new();
        int readBytes = storage.Load(loaded);

        Assert.AreEqual(writtenBytes, readBytes);
        Assert.AreEqual(original, loaded);
    }

    [Test]
    public void CurveUnitTest()
    {
        Curve original = new(new(0, stride, 2 * stride));
        original = original.AddStartDistance(Constants.MinLaneLength / 5);
        original = original.AddEndDistance(Constants.MinLaneLength / 5);
        original.Offset(3);
        original.Id = 5;
        Storage storage = new();
        storage.Save(original);
        Curve loaded = new();
        storage.Load(loaded);

        Assert.AreEqual(original, loaded);
    }

    [Test]
    public void IntersectionUnitTest()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
        RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        Intersection original = road.EndIntersection;
        Storage storage = new();
        storage.Save(original);
        Intersection loaded = new();
        storage.Load(loaded);

        Assert.AreEqual(original, loaded);
    }

    [Test]
    public void IPersistableNotInDict()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
        road.Lanes[0].StartNode.OutLane = new() { Id = road.Lanes[0].StartNode.OutLane.Id };
        Assert.False(Game.GameSave.IPersistableAreInDict());
    }

    [Test]
    public void RecoverSingleOneLaneRoad()
    {
        RoadBuilder.Single(0, stride, 2 * stride, 1);

        GameSave oldSave = Game.GameSave;
        SaveSystem.SaveGame();
        SaveSystem.LoadGame();
        Assert.AreEqual(oldSave, Game.GameSave);
    }

    [Test]
    public void RecoverSingleThreeLaneRoad()
    {
        RoadBuilder.Single(0, stride, 2 * stride, 3);
        GameSave oldSave = Game.GameSave;
        SaveSystem.SaveGame();
        SaveSystem.LoadGame();
        Assert.AreEqual(oldSave, Game.GameSave);
    }

    [Test]
    public void RecoverTwoDisconnectedOneLaneRoad()
    {
        RoadBuilder.Single(0, stride, 2 * stride, 1);
        RoadBuilder.Single(3 * stride, 4 * stride, 5 * stride, 1);

        GameSave oldSave = Game.GameSave;
        SaveSystem.SaveGame();
        SaveSystem.LoadGame();
        Assert.AreEqual(oldSave, Game.GameSave);
    }

    [Test]
    public void RecoverTwoConnectedOneLaneRoad()
    {
        RoadBuilder.Single(0, stride, 2 * stride, 1);
        RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);

        GameSave oldSave = Game.GameSave;
        SaveSystem.SaveGame();
        SaveSystem.LoadGame();
        Assert.AreEqual(oldSave, Game.GameSave);
    }

    [Test]
    public void RecoverRoadOutline()
    {
        RoadBuilder.Single(0, stride, 2 * stride, 1);
        RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        RoadBuilder.Single(4 * stride, 5 * stride, 6 * stride, 1);

        GameSave oldSave = Game.GameSave;
        SaveSystem.SaveGame();
        SaveSystem.LoadGame();
        Assert.AreEqual(oldSave, Game.GameSave);
    }

    [Test]
    public void RecoverConnectedLanes_2to1and1()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 2);
        float3 offset = road.Lanes[0].EndPos - road.EndPos;
        RoadBuilder.Single(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
        RoadBuilder.Single(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
        GameSave oldSave = Game.GameSave;
        SaveSystem.SaveGame();
        SaveSystem.LoadGame();
        Assert.AreEqual(oldSave, Game.GameSave);
    }

    [Test]
    public void SavingAfterRemovingRoad()
    {
        RoadBuilder.Single(0, stride, 2 * stride, 3);
        Road road1 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        Game.RemoveRoad(road1);

        GameSave oldSave = Game.GameSave;
        SaveSystem.SaveGame();
        SaveSystem.LoadGame();
        Assert.AreEqual(oldSave, Game.GameSave);
    }

    [Test]
    public void SimpleBuildAfterSaveLoad()
    {
        RoadBuilder.Single(0, stride, 2 * stride, 1);
        GameSave oldSave = Game.GameSave;
        SaveSystem.SaveGame();
        SaveSystem.LoadGame();
        Assert.AreEqual(oldSave, Game.GameSave);
        RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);

        Assert.AreEqual(2, Game.Roads.Count);
        Assert.AreEqual(3, Game.Nodes.Count);
    }
}