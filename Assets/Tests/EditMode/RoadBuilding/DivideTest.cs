using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.model.Roads;
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
        Divide.DivideRoad(road, road.Length / 2);
        Assert.AreEqual(2, Roads.Count);
    }

    [Test]
    public void RemovesOriginalEdges()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);

        Assert.AreEqual(1, Roads.Count);
        Divide.DivideRoad(road, road.Length / 2);
        
        Assert.False(Graph.ContainsEdge(road.Lanes[0].InnerEdge));
    }

    [Test]
    public void SubRoadsRetainsStartAndEndPos()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
        SubRoads subRoads = Divide.DivideRoad(road, road.Length / 2);

        Assert.AreEqual(road.StartPos, subRoads.Left.StartPos);
        Assert.AreEqual(road.EndPos, subRoads.Right.EndPos);
    }

    [Test]
    public void SubRoadsHasSameLaneCount()
    {
        for (int i = 1; i <= 3; i++)
        {
            Road road = RoadBuilder.Single(0, stride, 2 * stride, i);
            SubRoads subRoads = Divide.DivideRoad(road, road.Length / 2);
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
            SubRoads subRoads = Divide.DivideRoad(road, road.Length / 2);
            Road leftRoad = subRoads.Left;
            Road rightRoad = subRoads.Right;


            for (int j = 0; j < leftRoad.LaneCount; j++)
            {
                Lane leftLane = leftRoad.Lanes[j];
                Lane rightLane = rightRoad.Lanes[j];
                Assert.AreSame(leftLane.EndNode, rightLane.StartNode);
                Assert.AreSame(leftLane, leftLane.EndNode.InLane);
                Assert.AreSame(rightLane, leftLane.EndNode.OutLane);
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
            Divide.DivideRoad(road, road.Length / 2);

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
        SubRoads subRoads = Divide.DivideRoad(mid, mid.Length / 2);
        Road subLeft = subRoads.Left;
        Road subRight = subRoads.Right;
        Assert.NotNull(subLeft);
        Assert.NotNull(subRight);

        // check leftend connection
        for (int i = 0; i < 2; i++)
        {
            Node lNode = left.Lanes[i].EndNode;
            Node rNode = subLeft.Lanes[i].StartNode;
            Assert.AreSame(lNode, rNode);
            Assert.AreSame(lNode.InLane, left.Lanes[i]);
            Assert.AreSame(lNode.OutLane, subLeft.Lanes[i]);
        }

        // check rightend connection
        for (int i = 0; i < 2; i++)
        {
            Node lNode = subRight.Lanes[i].EndNode;
            Node rNode = right.Lanes[i].StartNode;
            Assert.AreSame(lNode, rNode);
            Assert.AreSame(lNode.InLane, subRight.Lanes[i]);
            Assert.AreSame(lNode.OutLane, right.Lanes[i]);
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
        Road roadRight = Divide.DivideRoad(road, road.Length / 2).Right;
        for (int i = 0; i < 3; i++)
        {
            Assert.AreSame(roadRight.Lanes[i].EndNode.InLane, roadRight.Lanes[i]);
            Assert.AreSame(roadRight.Lanes[i].EndNode.OutLane, connectedRoads[i].Lanes[0]);
        }
    }

    [Test]
    public void SubRoadsLengthBasicTest()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 3);
        SubRoads subRoads = Divide.HandleDivideCommand(road, stride);
        Assert.True(MyNumerics.IsApproxEqual(subRoads.Left.Length, subRoads.Right.Length, LengthDiffTolerance));
    }

    [Test]
    public void CurvedRoadDividedEvenly()
    {
        Road road = RoadBuilder.Single(
            0,
            2 * Constants.MinLaneLength * new float3(1, 0, 0),
            2 * Constants.MinLaneLength * new float3(1, 0, 1),
            3
        );
        SubRoads subRoads = Divide.HandleDivideCommand(road, 2 * Constants.MinLaneLength * new float3(1, 0, 0));
        Assert.True(MyNumerics.IsApproxEqual(subRoads.Left.Length, subRoads.Right.Length, LengthDiffTolerance));
    }

    [Test]
    public void RoadOutlineDividedProperly()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 3);
        Road road1 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 3);
        SubRoads subRoads = Divide.HandleDivideCommand(road, stride);
        Assert.True(subRoads.Left.OutlinePlausible());
        Assert.True(subRoads.Right.OutlinePlausible());
        Assert.True(MyNumerics.IsApproxEqual(subRoads.Left.LeftOutline.End.Last(), subRoads.Right.LeftOutline.Start.First()));
        Assert.True(MyNumerics.IsApproxEqual(subRoads.Left.RightOutline.End.Last(), subRoads.Right.RightOutline.Start.First()));
        Assert.True(MyNumerics.IsApproxEqual(subRoads.Right.LeftOutline.End.Last(), road1.LeftOutline.Start.First()));
        Assert.True(MyNumerics.IsApproxEqual(subRoads.Right.RightOutline.End.Last(), road1.RightOutline.Start.First()));
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
    public void EdgesAreValid()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 3);
        SubRoads subRoads = Divide.HandleDivideCommand(road, stride);

        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                if (Math.Abs(i - j) < 2)
                {
                    Assert.True(Graph.ContainsEdge(subRoads.Left.Lanes[i], subRoads.Right.Lanes[j]));
                    Assert.NotNull(Graph.AStar(subRoads.Left.Lanes[i].StartVertex, subRoads.Right.Lanes[j].EndVertex));
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
                    Assert.True(Graph.ContainsEdge(road1.Lanes[i], subRoads.Left.Lanes[j]));
    }

    [Test]
    public void LaneOfVertexUpdatedCorrectly()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 3);
        SubRoads subRoads = Divide.HandleDivideCommand(road, stride);

        foreach (Lane l in subRoads.Left.Lanes)
        {
            Assert.AreSame(l, l.StartVertex.Lane);
            Assert.AreSame(l, l.EndVertex.Lane);
        }
    }

    [Test]
    public void VertexCreation()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
        SubRoads subRoads = Divide.HandleDivideCommand(road, stride);
        
        Assert.AreEqual(4, Game.Vertices.Count);
        Assert.True(Game.Vertices.Values.Contains(subRoads.Left.Lanes[0].StartVertex));
        Assert.True(Game.Vertices.Values.Contains(subRoads.Left.Lanes[0].EndVertex));
        Assert.True(Game.Vertices.Values.Contains(subRoads.Right.Lanes[0].StartVertex));
        Assert.True(Game.Vertices.Values.Contains(subRoads.Right.Lanes[0].EndVertex));
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
        SubRoads subRoads = Divide.DivideRoad(road, road.Length / 2);
        Assert.NotNull(subRoads);
        Assert.AreEqual(2, Game.Roads.Count);
    }
}