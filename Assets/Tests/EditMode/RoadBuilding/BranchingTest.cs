using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
public class BranchingTest
{
    float3 stride = Constants.MinLaneLength * new float3(1, 0.1f, 1);
    float3 offset = Constants.MinLaneLength * new float3(1, 0, 1);

    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
    }

    [Test]
    public void TwoToOneOne_OnEnd()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 2);
        float3 p1 = road.Lanes[0].EndPos;
        float3 p2 = road.Lanes[1].EndPos;
        Road branch1 = RoadBuilder.Single(p1, p1 + offset, p1 + 2 * offset, 1);
        Road branch2 = RoadBuilder.Single(p2, p2 + offset, p2 + 2 * offset, 1);

        Assert.AreSame(branch1.Lanes[0], road.Lanes[0].EndNode.OutLane);
        Assert.AreSame(branch2.Lanes[0], road.Lanes[1].EndNode.OutLane);
        CheckIntersection(new() { road }, new() { branch1, branch2 });
    }

    [Test]
    public void TwoToOneOne_OnStart()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 2);
        float3 p1 = road.Lanes[0].StartPos;
        float3 p2 = road.Lanes[1].StartPos;
        Road branch1 = RoadBuilder.Single(p1 - 2 * offset, p1 - offset, p1, 1);
        Road branch2 = RoadBuilder.Single(p2 - 2 * offset, p2 - offset, p2, 1);

        Assert.AreSame(road.Lanes[0].StartNode.InLane, branch1.Lanes[0]);
        Assert.AreSame(road.Lanes[1].StartNode.InLane, branch2.Lanes[0]);
        CheckIntersection(new() { branch1, branch2 }, new() { road });
    }

    [Test]
    public void TwoToOne_OnEnd()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 2);
        float3 p1 = road.Lanes[0].EndPos;
        Road branch1 = RoadBuilder.Single(p1, p1 + offset, p1 + 2 * offset, 1); ;

        Assert.AreSame(road.Lanes[0].EndNode.OutLane, branch1.Lanes[0]);
        Assert.Null(road.Lanes[1].EndNode.OutLane);
        CheckIntersection(new() { road }, new() { branch1 });
    }

    [Test]
    public void TwoToOne_OnStart()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 2);
        float3 p1 = road.Lanes[0].StartPos;
        Road branch1 = RoadBuilder.Single(p1 - 2 * offset, p1 - offset, p1, 1);

        Assert.AreSame(road.Lanes[0].StartNode.InLane, branch1.Lanes[0]);
        Assert.Null(road.Lanes[1].StartNode.InLane);
        CheckIntersection(new() { branch1 }, new() { road });
    }

    [Test]
    public void ThreetoTwoOne_OnEnd()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 3);
        float3 p1 = road.Lanes[0].EndPos;
        float3 p2 = (road.Lanes[1].EndPos + road.Lanes[2].EndPos) / 2;
        Road branch1 = RoadBuilder.Single(p1, p1 + offset, p1 + 2 * offset, 1);
        Road branch2 = RoadBuilder.Single(p2, p2 + offset, p2 + 2 * offset, 2);

        Assert.AreSame(road.Lanes[0].EndNode.OutLane, branch1.Lanes[0]);
        Assert.AreSame(road.Lanes[1].EndNode.OutLane, branch2.Lanes[0]);
        Assert.AreSame(road.Lanes[2].EndNode.OutLane, branch2.Lanes[1]);
        CheckIntersection(new() { road }, new() { branch1, branch2 });
    }

    [Test]
    public void ThreetoTwoOne_OnStart()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 3);
        float3 p1 = road.Lanes[0].StartPos;
        float3 p2 = (road.Lanes[1].StartPos + road.Lanes[2].StartPos) / 2;
        Road branch1 = RoadBuilder.Single(p1 - 2 * offset, p1 - offset, p1, 1);
        Road branch2 = RoadBuilder.Single(p2 - 2 * offset, p2 - offset, p2, 2);

        Assert.AreSame(road.Lanes[0].StartNode.InLane, branch1.Lanes[0]);
        Assert.AreSame(road.Lanes[1].StartNode.InLane, branch2.Lanes[0]);
        Assert.AreSame(road.Lanes[2].StartNode.InLane, branch2.Lanes[1]);
        CheckIntersection(new() { branch1, branch2 }, new() { road });
    }

    [Test]
    public void ThreetoOneOneOne_OnEnd()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 3);
        float3 p1 = road.Lanes[0].EndPos;
        float3 p2 = road.Lanes[1].EndPos;
        float3 p3 = road.Lanes[2].EndPos;
        Road branch1 = RoadBuilder.Single(p1, p1 + offset, p1 + 2 * offset, 1);
        Road branch2 = RoadBuilder.Single(p2, p2 + offset, p2 + 2 * offset, 1);
        Road branch3 = RoadBuilder.Single(p3, p3 + offset, p3 + 2 * offset, 1);

        Assert.AreSame(road.Lanes[0].EndNode.OutLane, branch1.Lanes[0]);
        Assert.AreSame(road.Lanes[1].EndNode.OutLane, branch2.Lanes[0]);
        Assert.AreSame(road.Lanes[2].EndNode.OutLane, branch3.Lanes[0]);
        CheckIntersection(new() { road }, new() { branch1, branch2, branch3 });
    }

    [Test]
    public void ThreetoOneOneOne_OnStart()
    {
        Road road = RoadBuilder.Single(0, stride, 2 * stride, 3);
        float3 p1 = road.Lanes[0].StartPos;
        float3 p2 = road.Lanes[1].StartPos;
        float3 p3 = road.Lanes[2].StartPos;
        Road branch1 = RoadBuilder.Single(p1 - 2 * offset, p1 - offset, p1, 1);
        Road branch2 = RoadBuilder.Single(p2 - 2 * offset, p2 - offset, p2, 1);
        Road branch3 = RoadBuilder.Single(p3 - 2 * offset, p3 - offset, p3, 1);

        Assert.AreSame(road.Lanes[0].StartNode.InLane, branch1.Lanes[0]);
        Assert.AreSame(road.Lanes[1].StartNode.InLane, branch2.Lanes[0]);
        Assert.AreSame(road.Lanes[2].StartNode.InLane, branch3.Lanes[0]);
        CheckIntersection(new() { branch1, branch2, branch3 }, new() { road });
    }

    # region Helpers

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

    # endregion
}