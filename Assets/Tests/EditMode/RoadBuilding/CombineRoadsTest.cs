using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

public class CombineRoadsTest
{
    float3 stride = Constants.MinLaneLength * new float3(1, 0, 1);

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
        Assert.AreEqual(1, Game.Paths.Count);
        Assert.AreEqual(2, Game.Nodes.Count);
        Assert.AreEqual(2, Game.Vertices.Count);
        Assert.AreEqual(leftLength + right.Length, combined.Length);
        Assert.AreSame(right.EndIntersection, combined.EndIntersection);
        Assert.False(combined.EndIntersection.InRoads.Contains(right));
        Assert.False(combined.EndIntersection.IsEmpty());
        Assert.True(AllRoadsOutLineValid());
        for (int i = 0; i < combined.Lanes.Count; i++)
        {
            Assert.NotNull(combined.EndIntersection.InRoads.First().Lanes[i].EndVertex);
            Assert.AreSame(right.Lanes[i].EndNode, combined.Lanes[i].EndNode);
            Assert.AreSame(right.Lanes[i].EndVertex, combined.Lanes[i].EndVertex);
            Assert.NotNull(combined.Lanes[i].InnerPath);
        }
    }

    [Test]
    public void RemoveAfterCombining()
    {
        Road left = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Road right = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        Road toDelete = RoadBuilder.Single(4 * stride, 5 * stride, 6 * stride, 1);

        Road combined = Combine.CombineRoads(left.EndIntersection);
        Assert.NotNull(combined);
        Assert.True(Game.RemoveRoad(toDelete));
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