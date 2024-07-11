using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

public class CombineRoadsTest
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
        Debug.Log(right.LeftOutline.End.First());

        Assert.True(Combine.CombineIsValid(left.EndIntersection));
        Road combined = Combine.CombineRoads(left.EndIntersection);
        Assert.AreSame(combined, left);
        Assert.AreEqual(1, Game.Roads.Count);
        Assert.AreEqual(2, Game.Intersections.Count);
        Assert.AreEqual(1, Game.Paths.Count);
        Assert.AreEqual(2, Game.Nodes.Count);
        Assert.AreEqual(2, Game.Vertices.Count);
        Assert.AreEqual(leftLength + right.Length, combined.Length);
        Assert.True(AllRoadsOutLineValid());
        foreach (Lane lane in combined.Lanes)
            Assert.NotNull(lane.InnerPath);
    }

    bool AllRoadsOutLineValid()
    {
        foreach (Road r in Game.Roads.Values)
        {
            // r.LeftOutline.IsPlausible();
            // Debug.Log(r.HasNoneEmptyOutline());
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