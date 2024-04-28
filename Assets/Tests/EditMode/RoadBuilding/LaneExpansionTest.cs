using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

public class LaneExpansionTest
{
    float3 stride = Constants.MinimumLaneLength * new float3(0, 0, 1);
    SortedDictionary<int, Node> Nodes;

    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
        Nodes = Game.Nodes;
    }

    [Test]
    public void OneLaneToTwoLane_Left()
    {
        Road road0 = RoadBuilder.B(0, stride, 2 * stride, 1);
        Road road1 = RoadBuilder.B(2 * stride + 0.9f * Constants.BuildSnapTolerance * new float3(0, 0, 1), 3 * stride, 4 * stride, 2);
        Lane lane00 = road0.Lanes[0];
        Lane lane10 = road1.Lanes[0];
        Lane lane11 = road1.Lanes[1];

        Assert.AreSame(lane00.EndNode, lane11.StartNode);
        Assert.AreEqual(5, Nodes.Count);
        Assert.True(lane10.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane10 }));
        Assert.True(lane00.EndNode.Lanes.SetEquals(new HashSet<Lane> { lane00, lane11 }));
        CheckIntersection(new() { road0 }, new() { road1 });
    }

    [Test]
    public void TwoLaneToThreeLane_Right()
    {
        Road road0 = RoadBuilder.B(0, stride, 2 * stride, 2);
        Road road1 = RoadBuilder.B(2 * stride + 0.9f * Constants.BuildSnapTolerance * new float3(1, 0, 0), 3 * stride, 4 * stride, 3);
        Lane lane00 = road0.Lanes[0];
        Lane lane01 = road0.Lanes[1];
        Lane lane10 = road1.Lanes[0];
        Lane lane11 = road1.Lanes[1];
        Lane lane12 = road1.Lanes[2];

        Assert.AreSame(lane00.EndNode, lane10.StartNode);
        Assert.AreSame(lane01.EndNode, lane11.StartNode);
        Assert.AreEqual(8, Nodes.Count);
        Assert.True(lane12.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane12 }));
        Assert.True(lane10.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane00, lane10 }));
        Assert.True(lane11.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane01, lane11 }));
        CheckIntersection(new() { road0 }, new() { road1 });
    }

    [Test]
    public void OneLaneToThreeLane_Mid()
    {
        Road road0 = RoadBuilder.B(0, stride, 2 * stride, 1);
        Road road1 = RoadBuilder.B(2 * stride, 3 * stride, 4 * stride, 3);
        Lane lane00 = road0.Lanes[0];
        Lane lane10 = road1.Lanes[0];
        Lane lane11 = road1.Lanes[1];
        Lane lane12 = road1.Lanes[2];

        Assert.AreSame(lane00.EndNode, lane11.StartNode);
        Assert.AreEqual(7, Nodes.Count);
        Assert.True(lane10.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane10 }));
        Assert.True(lane11.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane11, lane00 }));
        Assert.True(lane12.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane12 }));

        CheckIntersection(new() { road0 }, new() { road1 });
    }

    [Test]
    public void OneLaneToThreeLane_Left()
    {
        Road road0 = RoadBuilder.B(0, stride, 2 * stride, 1);
        Road road1 = RoadBuilder.B(2 * stride + 1.5f * Constants.BuildSnapTolerance * new float3(-1, 0, 0), 3 * stride, 4 * stride, 3);
        Lane lane00 = road0.Lanes[0];
        Lane lane10 = road1.Lanes[0];
        Lane lane11 = road1.Lanes[1];
        Lane lane12 = road1.Lanes[2];
        Assert.AreSame(lane00.EndNode, lane12.StartNode);
        Assert.AreEqual(7, Nodes.Count);
        Assert.True(lane10.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane10 }));
        Assert.True(lane11.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane11 }));
        Assert.True(lane12.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane12, lane00 }));

        CheckIntersection(new() { road0 }, new() { road1 });
    }

    [Test]
    public void OneLaneToThreeLane_Right()
    {
        Road road0 = RoadBuilder.B(0, stride, 2 * stride, 1);
        Road road1 = RoadBuilder.B(2 * stride + 1.5f * Constants.BuildSnapTolerance * new float3(1, 0, 0), 3 * stride, 4 * stride, 3);
        Lane lane00 = road0.Lanes[0];
        Lane lane10 = road1.Lanes[0];
        Lane lane11 = road1.Lanes[1];
        Lane lane12 = road1.Lanes[2];
        Assert.AreSame(lane00.EndNode, lane10.StartNode);
        Assert.AreEqual(7, Nodes.Count);
        Assert.True(lane10.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane10, lane00 }));
        Assert.True(lane11.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane11 }));
        Assert.True(lane12.StartNode.Lanes.SetEquals(new HashSet<Lane> { lane12 }));

        CheckIntersection(new() { road0 }, new() { road1 });
    }

    [Test]
    public void ThreeLaneToOneLane_Mid()
    {
        Road road0 = RoadBuilder.B(2 * stride, 3 * stride, 4 * stride, 1);
        Road road1 = RoadBuilder.B(0, stride , 2 * stride, 3);
        Lane lane00 = road0.Lanes[0];
        Lane lane10 = road1.Lanes[0];
        Lane lane11 = road1.Lanes[1];
        Lane lane12 = road1.Lanes[2];
        Assert.AreSame(lane00.StartNode, lane11.EndNode);
        Assert.AreEqual(7, Nodes.Count);
        Assert.True(lane10.EndNode.Lanes.SetEquals(new HashSet<Lane> { lane10 }));
        Assert.True(lane11.EndNode.Lanes.SetEquals(new HashSet<Lane> { lane11, lane00 }));
        Assert.True(lane12.EndNode.Lanes.SetEquals(new HashSet<Lane> { lane12 }));
        CheckIntersection(new() { road1 }, new() { road0 });
    }
    [Test]
    public void ThreeLaneToOneLane_Left()
    {
        Road road0 = RoadBuilder.B(2 * stride, 3 * stride, 4 * stride, 1);
        Road road1 = RoadBuilder.B(0, stride , 2 * stride, 3);
        Lane lane00 = road0.Lanes[0];
        Lane lane10 = road1.Lanes[0];
        Lane lane11 = road1.Lanes[1];
        Lane lane12 = road1.Lanes[2];
        Assert.AreSame(lane00.StartNode, lane11.EndNode);
        Assert.AreEqual(7, Nodes.Count);
        Assert.True(lane10.EndNode.Lanes.SetEquals(new HashSet<Lane> { lane10 }));
        Assert.True(lane11.EndNode.Lanes.SetEquals(new HashSet<Lane> { lane11, lane00 }));
        Assert.True(lane12.EndNode.Lanes.SetEquals(new HashSet<Lane> { lane12 }));
        CheckIntersection(new() { road1 }, new() { road0 });
    }
    # region helper

    public bool IsIsolatedNode(Node node)
    {
        return node.Lanes.Count == 1;
    }

    void CheckIntersection(HashSet<Road> inRoads, HashSet<Road> outRoads)
    {
        Assert.AreEqual(outRoads.Count + inRoads.Count + 1, Game.Intersections.Count);
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