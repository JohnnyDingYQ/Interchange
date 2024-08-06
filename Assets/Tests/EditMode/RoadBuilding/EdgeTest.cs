using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;
using Assets.Scripts.model.Roads;

public class EdgeTest
{
    float3 stride = Constants.MinLaneLength * new float3(1, 0, 0);

    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
    }

    [Test]
    public void OneLaneRoad()
    {
        Road road1 = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Lane lane1 = road1.Lanes[0];
        Assert.True(Graph.ContainsEdge(lane1.StartVertex, lane1.EndVertex));
        Road road2 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        Lane lane2 = road2.Lanes[0];
        Assert.True(Graph.ContainsEdge(lane1, lane2));
    }


    [Test]
    public void ThreeLaneRoadConnectedOnEnd()
    {
        Road road1 = RoadBuilder.Single(0, stride, 2 * stride, 3);
        for (int i = 0; i < 3; i++)
            Assert.True(Graph.ContainsEdge(road1.Lanes[i].StartVertex, road1.Lanes[i].EndVertex));

        Road road2 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 3);
        Assert.True(Graph.ContainsEdge(road1.Lanes[0], road2.Lanes[0]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[1], road2.Lanes[1]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[2], road2.Lanes[2]));
        for (int i = 0; i < 3; i++)
        {
            if (i - 1 >= 0)
                Assert.True(Graph.ContainsEdge(road1.Lanes[i], road2.Lanes[i - 1]));
            Assert.True(Graph.ContainsEdge(road1.Lanes[i], road2.Lanes[i]));
            if (i + 1 < 3)
                Assert.True(Graph.ContainsEdge(road1.Lanes[i], road2.Lanes[i + 1]));
        }
    }

    [Test]
    public void ThreeLaneRoadConnectedOnStart()
    {
        Road road2 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 3);
        Road road1 = RoadBuilder.Single(0, stride, 2 * stride, 3);
        for (int i = 0; i < 3; i++)
        {
            if (i - 1 >= 0)
                Assert.True(Graph.ContainsEdge(road1.Lanes[i], road2.Lanes[i - 1]));
            Assert.True(Graph.ContainsEdge(road1.Lanes[i], road2.Lanes[i]));
            if (i + 1 < 3)
                Assert.True(Graph.ContainsEdge(road1.Lanes[i], road2.Lanes[i + 1]));
        }
    }

    [Test]
    public void LaneExpansionOnEnd_1to3()
    {
        Road road1 = RoadBuilder.Single(0, stride, 2 * stride, 1);
        Road road2 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 3);
        
        foreach (Edge p in Game.Edges.Values)
            Assert.True(p.Curve.Length <= road1.Curve.Length);

        Assert.AreEqual(7, Graph.EdgeCount);
        for (int i = 0; i < 3; i++)
            Assert.True(Graph.ContainsEdge(road1.Lanes[0], road2.Lanes[i]));
    }

    [Test]
    public void LaneContractionOnEnd_3to1()
    {
        Road road1 = RoadBuilder.Single(0, stride, 2 * stride, 3);
        Road road2 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);

        foreach (Edge p in Game.Edges.Values)
            Assert.True(p.Curve.Length <= road1.Curve.Length);

        Assert.AreEqual(7, Graph.EdgeCount);
        for (int i = 0; i < 3; i++)
            Assert.True(Graph.ContainsEdge(road1.Lanes[i], road2.Lanes[0]));
    }

    [Test]
    public void ComplexBranchingJunction_SideFirst_OnEnd()
    {
        Road road1 = RoadBuilder.Single(0, stride, 2 * stride, 3);
        float3 offset = road1.Lanes.First().EndPos - road1.EndPos;
        Road road2 = RoadBuilder.Single(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);

        Assert.AreEqual(7, Graph.EdgeCount);
        for (int i = 0; i < 3; i++)
            Assert.True(Graph.ContainsEdge(road1.Lanes[i], road2.Lanes[0]));
        
        Road road3 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);

        Assert.AreEqual(8, Graph.EdgeCount);
        Assert.True(Graph.ContainsEdge(road1.Lanes[0], road2.Lanes[0]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[1], road3.Lanes[0]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[2], road3.Lanes[0]));

        Road road4 = RoadBuilder.Single(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);

        Assert.AreEqual(9, Graph.EdgeCount);
        Assert.True(Graph.ContainsEdge(road1.Lanes[0], road2.Lanes[0]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[1], road3.Lanes[0]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[2], road4.Lanes[0]));
    }

    [Test]
    public void ComplexBranchingJunction_MidFirst_OnEnd()
    {
        Road road1 = RoadBuilder.Single(0, stride, 2 * stride, 3);
        float3 offset = road1.Lanes.First().EndPos - road1.EndPos;
        Road road2 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);

        Assert.AreEqual(7, Graph.EdgeCount);
        for (int i = 0; i < 3; i++)
            Assert.True(Graph.ContainsEdge(road1.Lanes[i], road2.Lanes[0]));

        Road road3 = RoadBuilder.Single(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);

        Assert.AreEqual(8, Graph.EdgeCount);
        Assert.True(Graph.ContainsEdge(road1.Lanes[0], road3.Lanes[0]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[1], road2.Lanes[0]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[2], road2.Lanes[0]));

        Road road4 = RoadBuilder.Single(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);

        Assert.AreEqual(9, Graph.EdgeCount);
        Assert.True(Graph.ContainsEdge(road1.Lanes[0], road3.Lanes[0]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[1], road2.Lanes[0]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[2], road4.Lanes[0]));
    }

    [Test]
    public void ComplexBranchingJunction_SidesFirst_OnEnd()
    {
        Road road1 = RoadBuilder.Single(0, stride, 2 * stride, 3);
        float3 offset = road1.Lanes.First().EndPos - road1.EndPos;
        Road road2 = RoadBuilder.Single(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);

        Assert.AreEqual(7, Graph.EdgeCount);
        for (int i = 0; i < 3; i++)
            Assert.True(Graph.ContainsEdge(road1.Lanes[i], road2.Lanes[0]));
        
        Road road3 = RoadBuilder.Single(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
        Assert.AreEqual(7, Graph.EdgeCount);
        Assert.True(Graph.ContainsEdge(road1.Lanes[0], road2.Lanes[0]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[2], road3.Lanes[0]));

        Road road4 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride , 1);
        Assert.AreEqual(9, Graph.EdgeCount);
        Assert.True(Graph.ContainsEdge(road1.Lanes[0], road2.Lanes[0]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[1], road4.Lanes[0]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[2], road3.Lanes[0]));
    }

    [Test]
    public void ComplexBranchingJunction_SideFirst_OnStart()
    {
        Road road1 = RoadBuilder.Single(4 * stride, 5 * stride, 6 * stride, 3);
        float3 offset = road1.Lanes.First().StartPos - road1.StartPos;
        Road road2 = RoadBuilder.Single(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);

        Assert.AreEqual(7, Graph.EdgeCount);
        for (int i = 0; i < 3; i++)
            Assert.True(Graph.ContainsEdge(road1.Lanes[i], road2.Lanes[0]));
        
        Road road3 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);

        Assert.AreEqual(8, Graph.EdgeCount);
        Assert.True(Graph.ContainsEdge(road1.Lanes[0], road2.Lanes[0]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[1], road3.Lanes[0]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[2], road3.Lanes[0]));

        Road road4 = RoadBuilder.Single(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);

        Assert.AreEqual(9, Graph.EdgeCount);
        Assert.True(Graph.ContainsEdge(road1.Lanes[0], road2.Lanes[0]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[1], road3.Lanes[0]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[2], road4.Lanes[0]));
    }

    [Test]
    public void ComplexBranchingJunction_MidFirst_OnStart()
    {
        Road road1 = RoadBuilder.Single(4 * stride, 5 * stride, 6 * stride, 3);
        float3 offset = road1.Lanes.First().StartPos - road1.StartPos;
        Road road2 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);

        Assert.AreEqual(7, Graph.EdgeCount);
        for (int i = 0; i < 3; i++)
            Assert.True(Graph.ContainsEdge(road1.Lanes[i], road2.Lanes[0]));

        Road road3 = RoadBuilder.Single(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);

        Assert.AreEqual(8, Graph.EdgeCount);
        Assert.True(Graph.ContainsEdge(road1.Lanes[0], road3.Lanes[0]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[1], road2.Lanes[0]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[2], road2.Lanes[0]));

        Road road4 = RoadBuilder.Single(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);

        Assert.AreEqual(9, Graph.EdgeCount);
        Assert.True(Graph.ContainsEdge(road1.Lanes[0], road3.Lanes[0]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[1], road2.Lanes[0]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[2], road4.Lanes[0]));
    }

    [Test]
    public void ComplexBranchingJunction_SidesFirst_OnStart()
    {
        Road road1 = RoadBuilder.Single(4 * stride, 5 * stride, 6 * stride, 3);
        float3 offset = road1.Lanes.First().StartPos - road1.StartPos;
        Road road2 = RoadBuilder.Single(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);

        Assert.AreEqual(7, Graph.EdgeCount);
        for (int i = 0; i < 3; i++)
            Assert.True(Graph.ContainsEdge(road1.Lanes[i], road2.Lanes[0]));
        
        Road road3 = RoadBuilder.Single(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
        Assert.AreEqual(7, Graph.EdgeCount);
        Assert.True(Graph.ContainsEdge(road1.Lanes[0], road2.Lanes[0]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[2], road3.Lanes[0]));

        Road road4 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
        Assert.AreEqual(9, Graph.EdgeCount);
        Assert.True(Graph.ContainsEdge(road1.Lanes[0], road2.Lanes[0]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[1], road4.Lanes[0]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[2], road3.Lanes[0]));
    }

    [Test]
    public void Expand3LanesTo2LanesAnd2Lanes_OnEnd()
    {
        Road road1 = RoadBuilder.Single(0, stride, 2 * stride, 3);
        float3 offset = road1.Lanes.First().StartPos - road1.StartPos;
        Road road2 = RoadBuilder.Single(2 * stride + offset * 2, 3 * stride + offset * 2, 4 * stride + offset * 2, 2);

        Assert.AreEqual(9, Graph.EdgeCount);
        Assert.True(Graph.ContainsEdge(road1.Lanes[0], road2.Lanes[0]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[0], road2.Lanes[1]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[1], road2.Lanes[1]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[2], road2.Lanes[1]));

        Road road3 = RoadBuilder.Single(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 2);
        Assert.AreEqual(13, Graph.EdgeCount);
        Assert.True(Graph.ContainsEdge(road1.Lanes[0], road2.Lanes[0]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[0], road2.Lanes[1]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[1], road3.Lanes[0]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[2], road3.Lanes[1]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[1], road3.Lanes[1]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[2], road3.Lanes[0]));
    }

    [Test]
    public void Expand3LanesTo2LanesAnd2Lanes_OnStart()
    {
        Road road1 = RoadBuilder.Single(4 * stride, 5 * stride, 6 * stride, 3);
        float3 offset = road1.Lanes.First().StartPos - road1.StartPos;
        Road road2 = RoadBuilder.Single(2 * stride + offset * 2, 3 * stride + offset * 2, 4 * stride + offset * 2, 2);

        Assert.AreEqual(9, Graph.EdgeCount);
        Assert.True(Graph.ContainsEdge(road1.Lanes[0], road2.Lanes[0]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[0], road2.Lanes[1]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[1], road2.Lanes[1]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[2], road2.Lanes[1]));

        Road road3 = RoadBuilder.Single(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 2);
        Assert.AreEqual(13, Graph.EdgeCount);
        Assert.True(Graph.ContainsEdge(road1.Lanes[0], road2.Lanes[0]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[0], road2.Lanes[1]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[1], road3.Lanes[0]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[2], road3.Lanes[1]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[1], road3.Lanes[1]));
        Assert.True(Graph.ContainsEdge(road1.Lanes[2], road3.Lanes[0]));
    }

    [Test]
    public void BasicInterweavingEdge_2Lanes()
    {
        Road road1 = RoadBuilder.Single(0, stride, 2 * stride, 2);
        Road road2 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 2);
        Edge rightEdge = Graph.GetEdge(road1.Lanes[0].EndVertex, road2.Lanes[1].StartVertex);
        Edge leftEdge = Graph.GetEdge(road1.Lanes[1].EndVertex, road2.Lanes[0].StartVertex);
        Assert.AreSame(leftEdge, rightEdge.InterweavingEdge);
        Assert.AreSame(rightEdge, leftEdge.InterweavingEdge);
    }

    [Test]
    public void BasicInterweavingEdge_3Lanes()
    {
        Road road1 = RoadBuilder.Single(0, stride, 2 * stride, 3);
        Road road2 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 3);
        Edge rightEdge = Graph.GetEdge(road1.Lanes[0].EndVertex, road2.Lanes[1].StartVertex);
        Edge leftEdge = Graph.GetEdge(road1.Lanes[1].EndVertex, road2.Lanes[0].StartVertex);
        Assert.AreSame(leftEdge, rightEdge.InterweavingEdge);
        Assert.AreSame(rightEdge, leftEdge.InterweavingEdge);
        rightEdge = Graph.GetEdge(road1.Lanes[1].EndVertex, road2.Lanes[2].StartVertex);
        leftEdge = Graph.GetEdge(road1.Lanes[2].EndVertex, road2.Lanes[1].StartVertex);
        Assert.AreSame(leftEdge, rightEdge.InterweavingEdge);
        Assert.AreSame(rightEdge, leftEdge.InterweavingEdge);
    }

    [Test]
    public void BasicInterweavingPath_2to3Lanes()
    {
        Road road1 = RoadBuilder.Single(0, stride, 2 * stride, 2);
        float3 offset = road1.Lanes.First().StartPos - road1.StartPos;
        Road road2 = RoadBuilder.Single(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 3);
        Edge rightEdge = Graph.GetEdge(road1.Lanes[0].EndVertex, road2.Lanes[2].StartVertex);
        Edge leftEdge = Graph.GetEdge(road1.Lanes[1].EndVertex, road2.Lanes[1].StartVertex);
        Assert.AreSame(leftEdge, rightEdge.InterweavingEdge);
        Assert.AreSame(rightEdge, leftEdge.InterweavingEdge);
    }
}