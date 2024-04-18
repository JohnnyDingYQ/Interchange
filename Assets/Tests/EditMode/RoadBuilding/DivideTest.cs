using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

public class DivideTest
{
    float3 stride = Constants.MinimumLaneLength * new float3(1, 0, 1);
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
        Road road = RoadBuilder.Build(0, stride, 2 * stride, 1);

        Assert.AreEqual(1, Roads.Count);
        DivideHandler.DivideRoad(road, 0.5f);
        Assert.AreEqual(2, Roads.Count);
    }

    [Test]
    public void SubRoadsRetainsStartAndEndPos()
    {
        Road road = RoadBuilder.Build(0, stride, 2 * stride, 1);
        SubRoads subRoads = DivideHandler.DivideRoad(road, 0.5f);

        Assert.AreEqual(road.StartPos, subRoads.Left.StartPos);
        Assert.AreEqual(road.EndPos, subRoads.Right.EndPos);
    }

    [Test]
    public void SubRoadsHasSameLaneCount()
    {
        for (int i = 1; i <= 3; i++)
        {
            Road road = RoadBuilder.Build(0, stride, 2 * stride, i);
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
            Road road = RoadBuilder.Build(0, stride, 2 * stride, i); ;
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
            Road road = RoadBuilder.Build(0, stride, 2 * stride, i);
            DivideHandler.DivideRoad(road, 0.5f);

            Assert.AreEqual(3 * i, Nodes.Count);
            foreach (Node node in Nodes.Values)
            {
                Assert.True(node.Id != -1);
            }

            ResetGame();
        }
    }

    [Test]
    public void SubRoadsPreserveConnection()
    {
        Road left = RoadBuilder.Build(0, stride, 2 * stride, 2);
        Road mid = RoadBuilder.Build(2 * stride, 3 * stride, 4 * stride, 2);
        Road right = RoadBuilder.Build(4 * stride, 5 * stride, 6 * stride, 2);
        SubRoads subRoads = DivideHandler.DivideRoad(mid, 0.5f);
        Road subLeft = subRoads.Left;
        Road subRight = subRoads.Right;
        Assert.NotNull(subLeft);
        Assert.NotNull(subRight);

        // check leftend connection
        for (int i = 0; i < 2; i++)
        {
            Node lNode = left.Lanes[i].EndNode;
            Node rNode = subLeft.Lanes[i].StartNode;
            HashSet<Lane> expected = new() { left.Lanes[i], subLeft.Lanes[i] };
            Assert.AreSame(lNode, rNode);
            Assert.True(lNode.Lanes.SetEquals(expected));
        }

        // check rightend connection
        for (int i = 0; i < 2; i++)
        {
            Node lNode = subRight.Lanes[i].EndNode;
            Node rNode = right.Lanes[i].StartNode;
            HashSet<Lane> expected = new() { right.Lanes[i], subRight.Lanes[i] };
            Assert.AreSame(lNode, rNode);
            Assert.True(lNode.Lanes.SetEquals(expected));
        }
    }

    [Test]
    public void PreserveConnectionSpecificTest()
    {
        Road road = RoadBuilder.Build(0, stride, 2 * stride, 3);
        List<Road> connectedRoads = new();
        for (int i = 0; i < 3; i++)
        {
            float3 pos = road.Lanes[i].EndPos;
            connectedRoads.Add(RoadBuilder.Build(pos, pos + stride, pos + 2 * stride, 1));
        }
        Road roadRight = DivideHandler.DivideRoad(road, 0.5f).Right;
        for (int i = 0; i < 3; i++)
        {
            Assert.True(roadRight.Lanes[i].EndNode.Lanes.SetEquals(new HashSet<Lane>() {roadRight.Lanes[i], connectedRoads[i].Lanes[0]}));
        }
    }

    [Test]
    public void SubRoadsLengthBasicTest()
    {
        Road road = RoadBuilder.Build(0, stride, 2 * stride, 3);
        SubRoads subRoads = DivideHandler.HandleDivideCommand(road, stride);
        Assert.True(Utility.AreNumericallyEqual(subRoads.Left.Length, subRoads.Right.Length, Constants.RoadDivisionLengthTestTolerance));
    }

    [Test]
    public void SubRoadsPathConnected()
    {
        Road road = RoadBuilder.Build(0, stride, 2 * stride, 3);
        SubRoads subRoads = DivideHandler.HandleDivideCommand(road, stride);
        for (int i = 0; i < 3; i++)
        {
            if (i - 1 >= 0)
                Assert.True(Game.HasEdge(subRoads.Left.Lanes[i], subRoads.Right.Lanes[i - 1]));
            Assert.True(Game.HasEdge(subRoads.Left.Lanes[i], subRoads.Right.Lanes[i]));
            if (i + 1 < 3)
                Assert.True(Game.HasEdge(subRoads.Left.Lanes[i], subRoads.Right.Lanes[i + 1]));
        }
    }

    [Test]
    public void CurvedRoadDividedEvenly()
    {
        Road road = RoadBuilder.Build(
            0,
            Constants.MinimumLaneLength * new float3(1, 0, 0),
            2 * Constants.MinimumLaneLength * new float3(0, 0, 1),
            3
        );
        SubRoads subRoads = DivideHandler.DivideRoad(road, 0.5f);
        Assert.True(Utility.AreNumericallyEqual(subRoads.Left.Length, subRoads.Right.Length, Constants.RoadDivisionLengthTestTolerance));
    }

    [Test]
    public void RoadOutlineDividedProperly()
    {
        Road road = RoadBuilder.Build(0, stride, 2 * stride, 3);
        SubRoads subRoads = DivideHandler.HandleDivideCommand(road, stride);
        Assert.True(subRoads.Left.RoadOutLinePlausible());
        Assert.True(subRoads.Right.RoadOutLinePlausible());
        Assert.True(Utility.AreNumericallyEqual(subRoads.Left.LeftOutline.End.Last(), subRoads.Right.LeftOutline.Start.First()));
        Assert.True(Utility.AreNumericallyEqual(subRoads.Left.RightOutline.End.Last(), subRoads.Right.RightOutline.Start.First()));
    }
}