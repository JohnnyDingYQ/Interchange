using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;

public class PathTest
{
    float3 stride = Constants.MinimumLaneLength * new float3(1, 0, 0);

    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
    }

    [Test]
    public void OneLaneRoad()
    {
        Road road1 = RoadBuilder.BuildRoad(0, stride, 2 * stride, 1);
        Lane lane1 = road1.Lanes[0];
        Assert.True(Game.HasEdge(lane1.StartVertex, lane1.EndVertex));
        Road road2 = RoadBuilder.BuildRoad(2 * stride, 3 * stride, 4 * stride, 1);
        Lane lane2 = road2.Lanes[0];
        Assert.True(Game.HasEdge(lane1, lane2));
    }


    [Test]
    public void ThreeLaneRoadConnectedOnEnd()
    {
        Road road1 = RoadBuilder.BuildRoad(0, stride, 2 * stride, 3);
        for (int i = 0; i < 3; i++)
            Assert.True(Game.HasEdge(road1.Lanes[i].StartVertex, road1.Lanes[i].EndVertex));

        Road road2 = RoadBuilder.BuildRoad(2 * stride, 3 * stride, 4 * stride, 3);
        Assert.True(Game.HasEdge(road1.Lanes[0], road2.Lanes[0]));
        Assert.True(Game.HasEdge(road1.Lanes[1], road2.Lanes[1]));
        Assert.True(Game.HasEdge(road1.Lanes[2], road2.Lanes[2]));
        for (int i = 0; i < 3; i++)
        {
            if (i - 1 >= 0)
                Assert.True(Game.HasEdge(road1.Lanes[i], road2.Lanes[i - 1]));
            Assert.True(Game.HasEdge(road1.Lanes[i], road2.Lanes[i]));
            if (i + 1 < 3)
                Assert.True(Game.HasEdge(road1.Lanes[i], road2.Lanes[i + 1]));
        }
    }

    [Test]
    public void ThreeLaneRoadConnectedOnStart()
    {
        Road road2 = RoadBuilder.BuildRoad(2 * stride, 3 * stride, 4 * stride, 3);
        Road road1 = RoadBuilder.BuildRoad(0, stride, 2 * stride, 3);
        for (int i = 0; i < 3; i++)
        {
            if (i - 1 >= 0)
                Assert.True(Game.HasEdge(road1.Lanes[i], road2.Lanes[i - 1]));
            Assert.True(Game.HasEdge(road1.Lanes[i], road2.Lanes[i]));
            if (i + 1 < 3)
                Assert.True(Game.HasEdge(road1.Lanes[i], road2.Lanes[i + 1]));
        }
    }

    [Test]
    public void LaneExpansionOnEnd_1to3()
    {
        Road road1 = RoadBuilder.BuildRoad(0, stride, 2 * stride, 1);
        Road road2 = RoadBuilder.BuildRoad(2 * stride, 3 * stride, 4 * stride, 3);
        
        Assert.AreEqual(7, Game.Graph.EdgeCount);
        for (int i = 0; i < 3; i++)
            Assert.True(Game.HasEdge(road1.Lanes[0], road2.Lanes[i]));
    }

    [Test]
    public void LaneContractionOnEnd_3to1()
    {
        Road road1 = RoadBuilder.BuildRoad(0, stride, 2 * stride, 3);
        Road road2 = RoadBuilder.BuildRoad(2 * stride, 3 * stride, 4 * stride, 1);

        Assert.AreEqual(7, Game.Graph.EdgeCount);
        for (int i = 0; i < 3; i++)
            Assert.True(Game.HasEdge(road1.Lanes[i], road2.Lanes[0]));
    }

    [Test]
    public void ComplexBranchingJunction_SideFirst()
    {
        Road road1 = RoadBuilder.BuildRoad(0, stride, 2 * stride, 3);
        float3 offset = road1.Lanes.First().EndPos - 2 * stride;
        Road road2 = RoadBuilder.BuildRoad(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);

        Assert.AreEqual(7, Game.Graph.EdgeCount);
        for (int i = 0; i < 3; i++)
            Assert.True(Game.HasEdge(road1.Lanes[i], road2.Lanes[0]));
        
        Road road3 = RoadBuilder.BuildRoad(2 * stride, 3 * stride, 4 * stride, 1);

        Assert.AreEqual(8, Game.Graph.EdgeCount);
        Assert.True(Game.HasEdge(road1.Lanes[0], road2.Lanes[0]));
        Assert.True(Game.HasEdge(road1.Lanes[1], road3.Lanes[0]));
        Assert.True(Game.HasEdge(road1.Lanes[2], road3.Lanes[0]));

        Road road4 = RoadBuilder.BuildRoad(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);

        Assert.AreEqual(9, Game.Graph.EdgeCount);
        Assert.True(Game.HasEdge(road1.Lanes[0], road2.Lanes[0]));
        Assert.True(Game.HasEdge(road1.Lanes[1], road3.Lanes[0]));
        Assert.True(Game.HasEdge(road1.Lanes[2], road4.Lanes[0]));
    }

    [Test]
    public void ComplexBranchingJunction_MidFirst()
    {
        Road road1 = RoadBuilder.BuildRoad(0, stride, 2 * stride, 3);
        float3 offset = road1.Lanes.First().EndPos - 2 * stride;
        Road road2 = RoadBuilder.BuildRoad(2 * stride, 3 * stride, 4 * stride, 1);

        Assert.AreEqual(7, Game.Graph.EdgeCount);
        for (int i = 0; i < 3; i++)
            Assert.True(Game.HasEdge(road1.Lanes[i], road2.Lanes[0]));

        Road road3 = RoadBuilder.BuildRoad(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);

        Assert.AreEqual(8, Game.Graph.EdgeCount);
        Assert.True(Game.HasEdge(road1.Lanes[0], road3.Lanes[0]));
        Assert.True(Game.HasEdge(road1.Lanes[1], road2.Lanes[0]));
        Assert.True(Game.HasEdge(road1.Lanes[2], road2.Lanes[0]));

        Road road4 = RoadBuilder.BuildRoad(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);

        Assert.AreEqual(9, Game.Graph.EdgeCount);
        Assert.True(Game.HasEdge(road1.Lanes[0], road3.Lanes[0]));
        Assert.True(Game.HasEdge(road1.Lanes[1], road2.Lanes[0]));
        Assert.True(Game.HasEdge(road1.Lanes[2], road4.Lanes[0]));
    }

    [Test]
    public void ComplexBranchingJunction_SidesFirst()
    {
        Road road1 = RoadBuilder.BuildRoad(0, stride, 2 * stride, 3);
        float3 offset = road1.Lanes.First().EndPos - 2 * stride;
        Road road2 = RoadBuilder.BuildRoad(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);

        Assert.AreEqual(7, Game.Graph.EdgeCount);
        for (int i = 0; i < 3; i++)
            Assert.True(Game.HasEdge(road1.Lanes[i], road2.Lanes[0]));
        
        Road road3 = RoadBuilder.BuildRoad(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
        Assert.AreEqual(7, Game.Graph.EdgeCount);
        Assert.True(Game.HasEdge(road1.Lanes[0], road2.Lanes[0]));
        Assert.True(Game.HasEdge(road1.Lanes[2], road3.Lanes[0]));
    }
}