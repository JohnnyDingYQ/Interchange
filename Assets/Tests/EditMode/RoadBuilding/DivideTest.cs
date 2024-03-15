using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;

public class DivideTest
{
    float3 direction = GConsts.MinimumRoadLength * new float3(1, 0, 1);
    SortedDictionary<int, Road> Roads;
    SortedDictionary<int, Node> Nodes;

    public void ResetGame()
    {
        Game.WipeState();
        Roads = Game.Roads;
        Nodes = Game.Nodes;
    }

    [SetUp]
    public void SetUp()
    {
        ResetGame();
    }

    [Test]
    public void RoadToDivideInvalid()
    {
        Road road = new();
        Assert.IsNull(DivideHandler.DivideRoad(road, 0.5f));
    }

    [Test]
    public void DividingCreatesTwoRoads()
    {
        Road road = RoadBuilder.BuildRoad(0, direction, 2 * direction, 1);

        Assert.AreEqual(1, Roads.Count);
        DivideHandler.DivideRoad(road, 0.5f);
        Assert.AreEqual(2, Roads.Count);
    }

    [Test]
    public void SubRoadsRetainsStartAndEndPos()
    {
        Road road = RoadBuilder.BuildRoad(0, direction, 2 * direction, 1);
        DivideHandler.DivideRoad(road, 0.5f);

        Assert.NotNull(Utility.FindRoadWithStartPos(road.StartPos));
        Assert.NotNull(Utility.FindRoadWithEndPos(road.EndPos));
    }

    [Test]
    public void SubRoadsHasSameLaneCount()
    {
        for (int i = 1; i <= 3; i++)
        {
            Road road = RoadBuilder.BuildRoad(0, direction, 2 * direction, i);
            SubRoads subRoads = DivideHandler.DivideRoad(road, 0.5f);
            Road leftRoad = subRoads.Left;
            Road rightRoad = subRoads.Right;

            Assert.AreEqual(i, leftRoad.LaneCount);
            Assert.AreEqual(i, rightRoad.LaneCount);

            ResetGame();
        }

    }

    [Test]
    public void SubRoadsAreConnected()
    {
        for (int i = 1; i <= 3; i++)
        {
            Road road = RoadBuilder.BuildRoad(0, direction, 2 * direction, i);;
            SubRoads subRoads = DivideHandler.DivideRoad(road, 0.5f);
            Road leftRoad = subRoads.Left;
            Road rightRoad = subRoads.Right;


            for (int j = 0; j < leftRoad.LaneCount; j++)
            {
                Lane leftLane = leftRoad.Lanes[j];
                Lane rightLane = rightRoad.Lanes[j];
                Assert.AreSame(leftLane.EndNode, rightLane.StartNode);
                Assert.True(leftLane.EndNode.Lanes.SetEquals(new HashSet<Lane>() { leftLane, rightLane }));
            }

            ResetGame();
        }
    }

    [Test]
    public void SubRoadsNodesAreRegistered()
    {
        for (int i = 1; i <= 3; i++)
        {
            Road road = RoadBuilder.BuildRoad(0, direction, 2 * direction, i);
            DivideHandler.DivideRoad(road, 0.5f);

            Assert.AreEqual(3 * i, Nodes.Count);
            foreach(Node node in Nodes.Values)
            {
                Assert.True(node.Id != -1);
            }

            ResetGame();
        }
    }

    [Test]
    public void SubRoadsPreserveConnection()
    {
        Road left = RoadBuilder.BuildRoad(0, direction, 2 * direction, 2);
        Road mid = RoadBuilder.BuildRoad(2 * direction, 3 * direction, 4 * direction, 2);
        Road right = RoadBuilder.BuildRoad(4 * direction, 5 * direction, 6 * direction, 2);
        SubRoads subRoads = DivideHandler.DivideRoad(mid, 0.5f);
        Road subLeft = subRoads.Left;
        Road subRight = subRoads.Right;
        Assert.NotNull(subLeft);
        Assert.NotNull(subRight);
        for (int i = 0; i < 2; i ++)
        {
            Node l = left.Lanes[i].EndNode;
            Node r = subLeft.Lanes[i].StartNode;
            Assert.AreSame(l, r);
            Assert.True(l.Lanes.SetEquals(new HashSet<Lane>() { left.Lanes[i], subLeft.Lanes[i] }));
        }

        for (int i = 0; i < 2; i ++)
        {
            Node l = subRight.Lanes[i].EndNode;
            Node r = right.Lanes[i].StartNode;
            Assert.AreSame(l, r);
            Assert.True(l.Lanes.SetEquals(new HashSet<Lane>() { subRight.Lanes[i], right.Lanes[i] }));
        }
    }
}