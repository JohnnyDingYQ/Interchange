using NUnit.Framework;
using Unity.Mathematics;

public class PathTest
{
    float3 direction  = new(1, 0, 1);
    float3 stride = Constants.MinimumLaneLength * new float3(1, 0, 1);
    
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        
    }

    [Test]
    public void OneLaneRoad()
    {
        Road road1 = RoadBuilder.BuildRoad(0, stride, 2 * stride, 1);
        Lane lane1 = road1.Lanes[0];
        Assert.True(HasEdge(lane1.StartVertex, lane1.EndVertex));
        Road road2 = RoadBuilder.BuildRoad(2 * stride, 3 * stride, 4 * stride, 1);
        Lane lane2 = road2.Lanes[0];
        Assert.True(HasEdge(lane1, lane2));
    }


    [Test]
    public void ThreeLaneRoadConnectedOnEnd()
    {
        Road road1 = RoadBuilder.BuildRoad(0, stride, 2 * stride, 3);
        for (int i = 0; i < 3; i++)
            Assert.True(HasEdge(road1.Lanes[i].StartVertex, road1.Lanes[i].EndVertex));

        Road road2 = RoadBuilder.BuildRoad(2 * stride, 3 * stride, 4 * stride, 3);
        for (int i = 0; i < 3; i++)
        {
            if (i - 1 >= 0)
                Assert.True(HasEdge(road1.Lanes[i], road2.Lanes[i - 1]));
            Assert.True(HasEdge(road1.Lanes[i], road2.Lanes[i]));
            if (i + 1 < 3)
                Assert.True(HasEdge(road1.Lanes[i], road2.Lanes[i + 1]));
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
                Assert.True(HasEdge(road1.Lanes[i], road2.Lanes[i - 1]));
            Assert.True(HasEdge(road1.Lanes[i], road2.Lanes[i]));
            if (i + 1 < 3)
                Assert.True(HasEdge(road1.Lanes[i], road2.Lanes[i + 1]));
        }
    }

    public bool HasEdge(Lane from, Lane to)
    {
        return HasEdge(from.StartVertex, to.EndVertex);
    }

    public bool HasEdge(Vertex from, Vertex to)
    {
        if (Game.Graph.ContainsEdge(new Path() {Source = from, Target = to}))
            return false;
        return true;
    }
}