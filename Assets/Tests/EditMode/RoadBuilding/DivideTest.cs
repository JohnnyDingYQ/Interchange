using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

public class DivideTest
{
    float3 stride = Constants.MinLaneLength * new float3(1, 0, 1);
    const float LengthDiffTolerance = 5f;
    Dictionary<uint, Road> Roads;
    Dictionary<uint, Node> Nodes;

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
    public void DividingCreatesTwoRoads()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);

        Assert.AreEqual(1, Roads.Count);
        Divide.DivideRoad(road, 0.5f);
        Assert.AreEqual(2, Roads.Count);
    }

    [Test]
    public void SubRoadsRetainsStartAndEndPos()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
        SubRoads subRoads = Divide.DivideRoad(road, 0.5f);

        Assert.AreEqual(road.StartPos, subRoads.Left.StartPos);
        Assert.AreEqual(road.EndPos, subRoads.Right.EndPos);
    }

    [Test]
    public void SubRoadsHasSameLaneCount()
    {
        for (int i = 1; i <= 3; i++)
        {
            Road road = RoadBuilder.Single(0, stride, 2 * stride, i);
            SubRoads subRoads = Divide.DivideRoad(road, 0.5f);
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
            Road road = RoadBuilder.Single(0, stride, 2 * stride, i); ;
            SubRoads subRoads = Divide.DivideRoad(road, 0.5f);
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
            Road road = RoadBuilder.Single(0, stride, 2 * stride, i);
            Divide.DivideRoad(road, 0.5f);

            Assert.AreEqual(3 * i, Nodes.Count);
            foreach (Node node in Nodes.Values)
            {
                Assert.True(node.Id != 0);
            }

            ResetGame();
        }
    }

    [Test]
    public void SubRoadsPreserveConnection()
    {
        Road left = RoadBuilder.Single(0, stride, 2 * stride, 2);
        Road mid = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 2);
        Road right = RoadBuilder.Single(4 * stride, 5 * stride, 6 * stride, 2);
        SubRoads subRoads = Divide.DivideRoad(mid, 0.5f);
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
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 3);
        List<Road> connectedRoads = new();
        for (int i = 0; i < 3; i++)
        {
            float3 pos = road.Lanes[i].EndPos;
            connectedRoads.Add(RoadBuilder.Single(pos, pos + stride, pos + 2 * stride, 1));
        }
        Road roadRight = Divide.DivideRoad(road, 0.5f).Right;
        for (int i = 0; i < 3; i++)
        {
            Assert.True(roadRight.Lanes[i].EndNode.Lanes.SetEquals(new HashSet<Lane>() { roadRight.Lanes[i], connectedRoads[i].Lanes[0] }));
        }
    }

    [Test]
    public void SubRoadsLengthBasicTest()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 3);
        SubRoads subRoads = Divide.HandleDivideCommand(road, stride);
        Assert.True(MyNumerics.AreNumericallyEqual(subRoads.Left.Length, subRoads.Right.Length, LengthDiffTolerance));
    }

    [Test]
    public void CurvedRoadDividedEvenly()
    {
        Road road = RoadBuilder.Single(
            0,
            Constants.MinLaneLength * new float3(1, 0, 0),
            2 * Constants.MinLaneLength * new float3(1, 0, 1),
            3
        );
        SubRoads subRoads = Divide.DivideRoad(road, 0.5f);
        Assert.True(MyNumerics.AreNumericallyEqual(subRoads.Left.Length, subRoads.Right.Length, LengthDiffTolerance));
    }

    [Test]
    public void RoadOutlineDividedProperly()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 3);
        Road road1 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 3);
        SubRoads subRoads = Divide.HandleDivideCommand(road, stride);
        Assert.True(subRoads.Left.OutLinePlausible());
        Assert.True(subRoads.Right.OutLinePlausible());
        Assert.True(MyNumerics.AreNumericallyEqual(subRoads.Left.LeftOutline.End.Last(), subRoads.Right.LeftOutline.Start.First()));
        Assert.True(MyNumerics.AreNumericallyEqual(subRoads.Left.RightOutline.End.Last(), subRoads.Right.RightOutline.Start.First()));
        Assert.True(MyNumerics.AreNumericallyEqual(subRoads.Right.LeftOutline.End.Last(), road1.LeftOutline.Start.First()));
        Assert.True(MyNumerics.AreNumericallyEqual(subRoads.Right.RightOutline.End.Last(), road1.RightOutline.Start.First()));
    }

    [Test]
    public void IntersectionHandledProperly()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 3);
        SubRoads subRoads = Divide.HandleDivideCommand(road, stride);
        Assert.AreSame(subRoads.Left.EndIntersection, subRoads.Right.StartIntersection);
        Assert.AreSame(subRoads.Left.StartIntersection, road.StartIntersection);
        Assert.AreSame(subRoads.Right.EndIntersection, road.EndIntersection);
        Assert.True(subRoads.Left.StartIntersection.OutRoads.Contains(subRoads.Left));
        Assert.True(subRoads.Right.EndIntersection.InRoads.Contains(subRoads.Right));
    }

    [Test]
    public void PathsAreValid()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 3);
        SubRoads subRoads = Divide.HandleDivideCommand(road, stride);

        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                if (Math.Abs(i - j) < 2)
                {
                    Assert.True(Graph.ContainsPath(subRoads.Left.Lanes[i], subRoads.Right.Lanes[j]));
                    Assert.NotNull(Graph.ShortestPathAStar(subRoads.Left.Lanes[i].StartVertex, subRoads.Right.Lanes[j].EndVertex));
                }
    }

    [Test]
    public void DividingRoadPreservesIntersectionPaths()
    {
        Road road1 = RoadBuilder.Single(0, stride, 2 * stride, 3);
        Road road2 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 3);
        SubRoads subRoads = Divide.HandleDivideCommand(road2, 3 * stride);

        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                if (Math.Abs(i - j) < 2)
                    Assert.True(Graph.ContainsPath(road1.Lanes[i], subRoads.Left.Lanes[j]));
    }

    [Test]
    public void LaneOfVertexUpdatedCorrectly()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 3);
        SubRoads subRoads = Divide.HandleDivideCommand(road, stride);

        foreach (Lane l in subRoads.Left.Lanes)
            foreach (Vertex v in new Vertex[] { l.StartVertex, l.EndVertex })
                Assert.AreSame(l, v.Lane);
    }

    [Test]
    public void DivideDoesNotCreateExtraVertices()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Divide.HandleDivideCommand(road, stride);
        
        Assert.AreEqual(4, Game.Vertices.Count);
    }

    [Test]
    public void SubRoadsInheritGhostStatus()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
        SubRoads subRoads = Divide.HandleDivideCommand(road, stride);

        Assert.AreEqual(road.IsGhost, subRoads.Left.IsGhost);
        Assert.AreEqual(road.IsGhost, subRoads.Right.IsGhost);

        road = RoadBuilder.Single(0, stride, 2 * stride, 1);
        road.IsGhost = true;
        subRoads = Divide.HandleDivideCommand(road, stride);

        Assert.AreEqual(road.IsGhost, subRoads.Left.IsGhost);
        Assert.AreEqual(road.IsGhost, subRoads.Right.IsGhost);
    }

    [Test]
    public void DivideExtremelyShortRoad()
    {
        Road road = RoadBuilder.Single(
            0,
            Vector3.up * Constants.MinLaneLength,
            2.01f * Constants.MinLaneLength * Vector3.up,
            3
        );
        SubRoads subRoads = Divide.DivideRoad(road, 0.5f);
        Assert.NotNull(subRoads);
        Assert.AreEqual(2, Game.Roads.Count);
    }
}