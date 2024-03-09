using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;

public class DivideTest
{
    float3 direction = GConsts.MinimumRoadLength * new float3(1, 0, 1);
    SortedDictionary<int, Road> Roads;

    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
        Roads = Game.Roads;
    }

    [Test]
    public void DividingCreatesTwoRoads()
    {
        RoadBuilder.BuildRoad(0, direction, 2 * direction, 1);
        Road road = Roads.Values.First();

        Assert.AreEqual(1, Roads.Count);
        DivideHandler.DivideRoad(road, 0.5f);
        Assert.AreEqual(2, Roads.Count);
    }

    [Test]
    public void SubRoadsRetainsStartAndEndPos()
    {
        RoadBuilder.BuildRoad(0, direction, 2 * direction, 1);
        Road road = Roads.Values.First();
        DivideHandler.DivideRoad(road, 0.5f);

        Assert.NotNull(Utility.FindRoadWithStartPos(road.StartPos));
        Assert.NotNull(Utility.FindRoadWithEndPos(road.EndPos));
    }

    [Test]
    public void SubRoadsHasSameLaneCount()
    {
        for (int i = 1; i <= 3; i++)
        {
            RoadBuilder.BuildRoad(0, direction, 2 * direction, i);
            Road road = Roads.Values.First();
            DivideHandler.DivideRoad(road, 0.5f);
            Road leftRoad = Utility.FindRoadWithStartPos(road.StartPos);
            Road rightRoad = Utility.FindRoadWithEndPos(road.EndPos);

            Assert.AreEqual(i, leftRoad.LaneCount);
            Assert.AreEqual(i, rightRoad.LaneCount);

            Game.WipeState();
            Roads = Game.Roads;
        }

    }

    [Test]
    public void SubRoadsAreConnected()
    {
        for (int i = 1; i <= 3; i++)
        {
            RoadBuilder.BuildRoad(0, direction, 2 * direction, i);
            Road road = Roads.Values.First();
            DivideHandler.DivideRoad(road, 0.5f);
            Road leftRoad = Utility.FindRoadWithStartPos(road.StartPos);
            Road rightRoad = Utility.FindRoadWithEndPos(road.EndPos);


            for (int j = 0; j < leftRoad.LaneCount; j++)
            {
                Lane leftLane = leftRoad.Lanes[j];
                Lane rightLane = rightRoad.Lanes[j];
                Assert.AreSame(leftLane.StartNode, rightLane.EndNode);
                Assert.True(leftLane.StartNode.Lanes.SetEquals(new HashSet<Lane>() { leftLane, rightLane }));
            }


            Game.WipeState();
            Roads = Game.Roads;
        }

    }
}