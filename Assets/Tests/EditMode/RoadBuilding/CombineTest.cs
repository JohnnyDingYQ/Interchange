using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

public class CombineTest
{
    float3 stride = Constants.MinLaneLength * new float3(0, 0, 1);

    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
    }

    [Test]
    public void BasicCombineOneLaneRoads()
    {
        Road left = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Road right = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        float leftLength = left.Length;

        Assert.True(Combine.CombineIsValid(left.EndIntersection));
        Road combined = Combine.CombineRoads(left.EndIntersection);
        Assert.AreSame(combined, left);
        Assert.AreEqual(1, Game.Roads.Count);
        Assert.AreEqual(2, Game.Intersections.Count);
        Assert.AreEqual(1, Game.Edges.Count);
        Assert.AreEqual(1, Graph.EdgeCount);
        Assert.AreEqual(2, Game.Nodes.Count);
        Assert.AreEqual(1, Game.Lanes.Count);
        Assert.True(Game.Nodes.Values.Contains(combined.Lanes[0].StartNode));
        Assert.True(Game.Nodes.Values.Contains(combined.Lanes[0].EndNode));
        Assert.AreSame(combined.Lanes[0], combined.Lanes[0].StartNode.OutLane);
        Assert.AreSame(combined.Lanes[0], combined.Lanes[0].EndNode.InLane);
        Assert.AreEqual(2, Game.Vertices.Count);
        Assert.True(Game.Vertices.Values.Contains(combined.Lanes[0].StartVertex));
        Assert.True(Game.Vertices.Values.Contains(combined.Lanes[0].EndVertex));
        Assert.True(Graph.ContainsVertex(combined.Lanes[0].StartVertex));
        Assert.True(Graph.ContainsVertex(combined.Lanes[0].EndVertex));
        Assert.AreEqual(leftLength + right.Length, combined.Length);
        Assert.AreSame(right.EndIntersection, combined.EndIntersection);
        Assert.False(combined.EndIntersection.InRoads.Contains(right));
        Assert.False(combined.EndIntersection.IsEmpty());
        Assert.AreEqual(1, left.StartIntersection.Nodes.Count);
        Assert.AreEqual(1, left.EndIntersection.Nodes.Count);
        Assert.True(AllRoadsOutLineValid());
        for (int i = 0; i < combined.Lanes.Count; i++)
        {
            Assert.AreSame(right.Lanes[i].EndNode, combined.Lanes[i].EndNode);
            Assert.AreSame(right.Lanes[i].EndVertex, combined.Lanes[i].EndVertex);
            Assert.NotNull(combined.Lanes[i].InnerEdge);
        }
    }

    [Test]
    public void RemoveAfterCombining()
    {
        Road left = RoadBuilder.Single(0, stride, 2 * stride, 1);
        RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        Road toDelete = RoadBuilder.Single(4 * stride, 5 * stride, 6 * stride, 1);

        Road combined = Combine.CombineRoads(left.EndIntersection);
        Assert.NotNull(combined);
        Assert.True(Game.RemoveRoad(toDelete));

        Assert.AreEqual(1, Game.Roads.Count);
        Assert.AreEqual(2, Game.Intersections.Count);
        Assert.AreEqual(1, Graph.EdgeCount);
        Assert.AreEqual(1, Game.Edges.Count);
        Assert.AreEqual(2, Game.Nodes.Count);
        Assert.AreEqual(2, Game.Vertices.Count);
    }

    [Test]
    public void ConnectRoadWithCombined()
    {
        Road left = RoadBuilder.Single(0, stride, 2 * stride, 1);
        RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        Road combined = Combine.CombineRoads(left.EndIntersection);

        Road connected = RoadBuilder.Single(4 * stride, 5 * stride, 6 * stride, 1);

        Assert.AreEqual(2, Game.Roads.Count);
        Assert.AreSame(combined.Lanes[0].EndNode.InLane, combined.Lanes[0]);
        Assert.AreSame(combined.Lanes[0].EndNode.OutLane, connected.Lanes[0]);
        Assert.NotNull(Graph.GetEdge(combined.Lanes[0].EndVertex, connected.Lanes[0].StartVertex));
    }

    bool AllRoadsOutLineValid()
    {
        foreach (Road r in Game.Roads.Values)
        {
            if (!r.OutlinePlausible())
            {
                Debug.Log("Road " + r.Id + ": Outline not plausible");
                return false;
            }
            if (!r.HasNoneEmptyOutline())
            {
                Debug.Log("Road " + r.Id + ": Outline empty");
                return false;
            }
        }
        return true;
    }
}