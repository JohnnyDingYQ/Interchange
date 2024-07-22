using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

public class LaneExpansionTest
{
    float3 stride = Constants.MinLaneLength * new float3(0, 0, 1);

    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
    }

    [Test]
    public void OneLaneToTwoLane_Left()
    {
        Road road0 = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Road road1 = RoadBuilder.Single(
            2 * stride + 0.9f * Constants.BuildSnapTolerance * new float3(0, 0, 1),
            3 * stride,
            4 * stride,
            2
        );
        Lane lane00 = road0.Lanes[0];
        Lane lane10 = road1.Lanes[0];
        Lane lane11 = road1.Lanes[1];

        Assert.AreSame(lane00.EndNode, lane11.StartNode);
        Assert.AreEqual(5, Game.Nodes.Count);
        Assert.Null(lane10.StartNode.InLane);
        Assert.AreSame(lane00.EndNode.OutLane, lane11);
        Assert.AreSame(lane00.EndNode, lane11.StartNode);
        CheckIntersection(new() { road0 }, new() { road1 });
    }

    [Test]
    public void TwoLaneToThreeLane_Right()
    {
        Road road0 = RoadBuilder.Single(0, stride, 2 * stride, 2);
        Road road1 = RoadBuilder.Single(2 * stride + 0.9f * Constants.BuildSnapTolerance * new float3(1, 0, 0), 3 * stride, 4 * stride, 3);
        Lane lane00 = road0.Lanes[0];
        Lane lane01 = road0.Lanes[1];
        Lane lane10 = road1.Lanes[0];
        Lane lane11 = road1.Lanes[1];
        Lane lane12 = road1.Lanes[2];

        Assert.AreSame(lane00.EndNode, lane10.StartNode);
        Assert.AreSame(lane01.EndNode, lane11.StartNode);
        Assert.AreEqual(8, Game.Nodes.Count);
        Assert.Null(lane12.StartNode.InLane);
        Assert.AreSame(lane10.StartNode.InLane, lane00);
        Assert.AreSame(lane11.StartNode.InLane, lane01);
        CheckIntersection(new() { road0 }, new() { road1 });
    }

    [Test]
    public void OneLaneToThreeLane_Mid()
    {
        Road road0 = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Road road1 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 3);
        Lane lane00 = road0.Lanes[0];
        Lane lane10 = road1.Lanes[0];
        Lane lane11 = road1.Lanes[1];
        Lane lane12 = road1.Lanes[2];

        Assert.AreSame(lane00.EndNode, lane11.StartNode);
        Assert.AreEqual(7, Game.Nodes.Count);
        Assert.Null(lane10.StartNode.InLane);
        Assert.AreSame(lane11.StartNode.InLane, lane00);
        Assert.Null(lane12.StartNode.InLane);
        CheckIntersection(new() { road0 }, new() { road1 });
    }

    [Test]
    public void OneLaneToThreeLane_Left()
    {
        Road road0 = RoadBuilder.Single(0, stride, 2 * stride, 1);
        float3 offset = road0.Curve.EndNormal * Constants.LaneWidth;
        Road road1 = RoadBuilder.Single(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 3);
        Lane lane00 = road0.Lanes[0];
        Lane lane10 = road1.Lanes[0];
        Lane lane11 = road1.Lanes[1];
        Lane lane12 = road1.Lanes[2];
        Assert.AreSame(lane00.EndNode, lane12.StartNode);
        Assert.AreEqual(7, Game.Nodes.Count);
        Assert.Null(lane10.StartNode.InLane);
        Assert.Null(lane11.StartNode.InLane);
        Assert.AreSame(lane12.StartNode.InLane, lane00);
        CheckIntersection(new() { road0 }, new() { road1 });
    }

    [Test]
    public void OneLaneToThreeLane_Right()
    {
        Road road0 = RoadBuilder.Single(0, stride, 2 * stride, 1);
        float3 offset = -1 * Constants.LaneWidth * road0.Curve.EndNormal;
        Road road1 = RoadBuilder.Single(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 3);
        Lane lane00 = road0.Lanes[0];
        Lane lane10 = road1.Lanes[0];
        Lane lane11 = road1.Lanes[1];
        Lane lane12 = road1.Lanes[2];
        Assert.AreSame(lane00.EndNode, lane10.StartNode);
        Assert.AreEqual(7, Game.Nodes.Count);
        Assert.AreSame(lane10.StartNode.InLane, lane00);
        Assert.Null(lane11.StartNode.InLane);
        Assert.Null(lane12.StartNode.InLane);
        CheckIntersection(new() { road0 }, new() { road1 });
    }

    [Test]
    public void ThreeLaneToOneLane_Mid()
    {
        Road road1 = RoadBuilder.Single(0, stride , 2 * stride, 3);
        Road road0 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        Lane lane00 = road0.Lanes[0];
        Lane lane10 = road1.Lanes[0];
        Lane lane11 = road1.Lanes[1];
        Lane lane12 = road1.Lanes[2];
        Assert.AreSame(lane00.StartNode, lane11.EndNode);
        Assert.AreEqual(7, Game.Nodes.Count);
        Assert.Null(lane10.EndNode.OutLane);
        Assert.AreSame(lane11.EndNode.OutLane, lane00);
        Assert.Null(lane12.EndNode.OutLane);
        CheckIntersection(new() { road1 }, new() { road0 });
    }
    
    # region helper

    public bool IsIsolatedNode(Node node)
    {
        return node.InLane == null ^ node.OutLane == null;
    }

    void CheckIntersection(HashSet<Road> inRoads, HashSet<Road> outRoads)
    {
        Intersection i = inRoads.First().EndIntersection;
        HashSet<Node> h = new();
        foreach (Road r in inRoads)
        {
            Assert.AreSame(i, r.EndIntersection);
            foreach (Node n in r.GetNodes(Side.End))
            {
                Assert.AreSame(i, n.Intersection);
                h.Add(n);
            }
            foreach (Node n in r.GetNodes(Side.Start))
            {
                Assert.True(IsIsolatedNode(n));
                h.Add(n);
            }
        }
        foreach (Road r in outRoads)
        {
            Assert.AreSame(i, r.StartIntersection);
            foreach (Node n in r.GetNodes(Side.Start))
            {
                Assert.AreSame(i, n.Intersection);
                h.Add(n);
            }
            foreach (Node n in r.GetNodes(Side.End))
            {
                Assert.True(IsIsolatedNode(n));
                h.Add(n);
            }
        }
        Assert.True(i.InRoads.SetEquals(inRoads));
        Assert.True(i.OutRoads.SetEquals(outRoads));
        Assert.True(Game.Nodes.Count == h.Count);
    }

    #endregion
}