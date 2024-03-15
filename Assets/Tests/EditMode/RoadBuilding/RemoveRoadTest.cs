using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;

public class RemoveRoadTest
{
    float3 direction = GConsts.MinimumRoadLength * new float3(1, 0, 1);
    SortedDictionary<int, Road> Roads;
    SortedDictionary<int, Node> Nodes;
    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
        Roads = Game.Roads;
        Nodes = Game.Nodes;
    }

    [Test]
    public void RoadToRemoveNotFound()
    {
        RoadBuilder.BuildRoad(0, direction, 2 * direction, 1);
        Road road = new();
        Assert.False(Game.RemoveRoad(road));
        Assert.AreEqual(1, Roads.Count);
    }

    [Test]
    public void RemoveIsolatedRoad()
    {
        RoadBuilder.BuildRoad(0, direction, 2 * direction, 1);
        Road road = Utility.FindRoadWithStartPos(0);
        Assert.True(Game.RemoveRoad(road));
        Assert.AreEqual(0, Roads.Count);
        Assert.AreEqual(0, Nodes.Count);
    }

    [Test]
    public void RemoveConnectedRoad()
    {
        RoadBuilder.BuildRoad(0, direction, 2 * direction, 2);
        RoadBuilder.BuildRoad(2 * direction, 3 * direction, 4 * direction, 2);
        Assert.AreEqual(6, Nodes.Count);
        Road road0 = Utility.FindRoadWithStartPos(0);
        Road road1 = Utility.FindRoadWithStartPos(2 * direction);
        Assert.True(Game.RemoveRoad(road0));
        Assert.AreEqual(4, Nodes.Count);
        Assert.True(road0.Lanes[0].EndNode.Lanes.SetEquals(new HashSet<Lane> { road1.Lanes[0] }));
        Assert.True(road0.Lanes[1].EndNode.Lanes.SetEquals(new HashSet<Lane> { road1.Lanes[1] }));
    }
}