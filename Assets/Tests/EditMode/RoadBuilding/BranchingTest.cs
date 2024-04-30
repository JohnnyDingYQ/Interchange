using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
public class BranchingTest
{
    float3 pos1 = new(0, 0, 0);
    float3 pos2 = Constants.MinimumLaneLength * new float3(1, 0, 1);
    float3 pos3 = Constants.MinimumLaneLength * 2 * new float3(1, 0, 1);
    float3 offset = Constants.MinimumLaneLength * new float3(1, 0, 1);

    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
    }

    [Test]
    public void TwoToOneOne_OnEnd()
    {
        Road road = RoadBuilder.B(pos1, pos2, pos3, 2);
        float3 p1 = road.Lanes[0].EndPos;
        float3 p2 = road.Lanes[1].EndPos;
        Road branch1 = RoadBuilder.B(p1, p1 + offset, p1 + 2 * offset, 1);
        Road branch2 = RoadBuilder.B(p2, p2 + offset, p2 + 2 * offset, 1);

        Assert.True(road.Lanes[0].EndNode.Lanes.SetEquals(new HashSet<Lane>() { branch1.Lanes[0], road.Lanes[0] }));
        Assert.True(road.Lanes[1].EndNode.Lanes.SetEquals(new HashSet<Lane>() { branch2.Lanes[0], road.Lanes[1] }));
        CheckIntersection(new() { road }, new() { branch1, branch2 });
    }

    [Test]
    public void TwoToOneOne_OnStart()
    {
        Road road = RoadBuilder.B(pos1, pos2, pos3, 2);
        float3 p1 = road.Lanes[0].StartPos;
        float3 p2 = road.Lanes[1].StartPos;
        Road branch1 = RoadBuilder.B(p1 - 2 * offset, p1 - offset, p1, 1);
        Road branch2 = RoadBuilder.B(p2 - 2 * offset, p2 - offset, p2, 1);

        Assert.True(road.Lanes[0].StartNode.Lanes.SetEquals(new HashSet<Lane>() { branch1.Lanes[0], road.Lanes[0] }));
        Assert.True(road.Lanes[1].StartNode.Lanes.SetEquals(new HashSet<Lane>() { branch2.Lanes[0], road.Lanes[1] }));
        CheckIntersection(new() { branch1, branch2 }, new() { road });
    }

    [Test]
    public void TwoToOne_OnEnd()
    {
        Road road = RoadBuilder.B(pos1, pos2, pos3, 2);
        float3 p1 = road.Lanes[0].EndPos;
        Road branch1 = RoadBuilder.B(p1, p1 + offset, p1 + 2 * offset, 1); ;

        Assert.True(road.Lanes[0].EndNode.Lanes.SetEquals(new HashSet<Lane>() { branch1.Lanes[0], road.Lanes[0] }));
        Assert.True(road.Lanes[1].EndNode.Lanes.SetEquals(new HashSet<Lane>() { road.Lanes[1] }));
        CheckIntersection(new() { road }, new() { branch1 });
    }

    [Test]
    public void TwoToOne_OnStart()
    {
        Road road = RoadBuilder.B(pos1, pos2, pos3, 2);
        float3 p1 = road.Lanes[0].StartPos;
        Road branch1 = RoadBuilder.B(p1 - 2 * offset, p1 - offset, p1, 1);

        Assert.True(road.Lanes[0].StartNode.Lanes.SetEquals(new HashSet<Lane>() { branch1.Lanes[0], road.Lanes[0] }));
        Assert.True(road.Lanes[1].StartNode.Lanes.SetEquals(new HashSet<Lane>() { road.Lanes[1] }));
        CheckIntersection(new() { branch1 }, new() { road });
    }

    [Test]
    public void ThreetoTwoOne_OnEnd()
    {
        Road road = RoadBuilder.B(pos1, pos2, pos3, 3);
        float3 p1 = road.Lanes[0].EndPos;
        float3 p2 = (road.Lanes[1].EndPos + road.Lanes[2].EndPos) / 2;
        Road branch1 = RoadBuilder.B(p1, p1 + offset, p1 + 2 * offset, 1);
        Road branch2 = RoadBuilder.B(p2, p2 + offset, p2 + 2 * offset, 2);

        Assert.True(road.Lanes[0].EndNode.Lanes.SetEquals(new HashSet<Lane>() { branch1.Lanes[0], road.Lanes[0] }));
        Assert.True(road.Lanes[1].EndNode.Lanes.SetEquals(new HashSet<Lane>() { branch2.Lanes[0], road.Lanes[1] }));
        Assert.True(road.Lanes[2].EndNode.Lanes.SetEquals(new HashSet<Lane>() { branch2.Lanes[1], road.Lanes[2] }));
        CheckIntersection(new() { road }, new() { branch1, branch2 });
    }

    [Test]
    public void ThreetoTwoOne_OnStart()
    {
        Road road = RoadBuilder.B(pos1, pos2, pos3, 3);
        float3 p1 = road.Lanes[0].StartPos;
        float3 p2 = (road.Lanes[1].StartPos + road.Lanes[2].StartPos) / 2;
        Road branch1 = RoadBuilder.B(p1 - 2 * offset, p1 - offset, p1, 1);
        Road branch2 = RoadBuilder.B(p2 - 2 * offset, p2 - offset, p2, 2);

        Assert.True(road.Lanes[0].StartNode.Lanes.SetEquals(new HashSet<Lane>() { branch1.Lanes[0], road.Lanes[0] }));
        Assert.True(road.Lanes[1].StartNode.Lanes.SetEquals(new HashSet<Lane>() { branch2.Lanes[0], road.Lanes[1] }));
        Assert.True(road.Lanes[2].StartNode.Lanes.SetEquals(new HashSet<Lane>() { branch2.Lanes[1], road.Lanes[2] }));
        CheckIntersection(new() { branch1, branch2 }, new() { road });
    }

    [Test]
    public void ThreetoOneOneOne_OnEnd()
    {
        Road road = RoadBuilder.B(pos1, pos2, pos3, 3);
        float3 p1 = road.Lanes[0].EndPos;
        float3 p2 = road.Lanes[1].EndPos;
        float3 p3 = road.Lanes[2].EndPos;
        Road branch1 = RoadBuilder.B(p1, p1 + offset, p1 + 2 * offset, 1);
        Road branch2 = RoadBuilder.B(p2, p2 + offset, p2 + 2 * offset, 1);
        Road branch3 = RoadBuilder.B(p3, p3 + offset, p3 + 2 * offset, 1);

        Assert.True(road.Lanes[0].EndNode.Lanes.SetEquals(new HashSet<Lane>() { branch1.Lanes[0], road.Lanes[0] }));
        Assert.True(road.Lanes[1].EndNode.Lanes.SetEquals(new HashSet<Lane>() { branch2.Lanes[0], road.Lanes[1] }));
        Assert.True(road.Lanes[2].EndNode.Lanes.SetEquals(new HashSet<Lane>() { branch3.Lanes[0], road.Lanes[2] }));
        CheckIntersection(new() { road }, new() { branch1, branch2, branch3 });
    }

    [Test]
    public void ThreetoOneOneOne_OnStart()
    {
        Road road = RoadBuilder.B(pos1, pos2, pos3, 3);
        float3 p1 = road.Lanes[0].StartPos;
        float3 p2 = road.Lanes[1].StartPos;
        float3 p3 = road.Lanes[2].StartPos;
        Road branch1 = RoadBuilder.B(p1 - 2 * offset, p1 - offset, p1, 1);
        Road branch2 = RoadBuilder.B(p2 - 2 * offset, p2 - offset, p2, 1);
        Road branch3 = RoadBuilder.B(p3 - 2 * offset, p3 - offset, p3, 1);

        Assert.True(road.Lanes[0].StartNode.Lanes.SetEquals(new HashSet<Lane>() { branch1.Lanes[0], road.Lanes[0] }));
        Assert.True(road.Lanes[1].StartNode.Lanes.SetEquals(new HashSet<Lane>() { branch2.Lanes[0], road.Lanes[1] }));
        Assert.True(road.Lanes[2].StartNode.Lanes.SetEquals(new HashSet<Lane>() { branch3.Lanes[0], road.Lanes[2] }));
        CheckIntersection(new() { branch1, branch2, branch3 }, new() { road });
    }

    # region Helpers

    public bool IsIsolatedNode(Node node)
    {
        return node.Lanes.Count == 1;
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