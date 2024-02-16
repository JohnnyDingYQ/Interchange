using System.Collections.Generic;
using NUnit.Framework;

public class IntersectionTest
{
    Intersection intersection;
    Road road1 = new(), road2 = new(), road3 =new(), road4 = new();
    Lane lane11, lane12, lane21, lane22, lane31, lane41;
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        lane11 = new() { Start = 0, End = 1, Road = road1 };
        lane12 = new() { Start = 2, End = 3, Road = road1 };

        road1.Lanes = new() { lane11, lane12 };

        lane21 = new() { Start = 1, End = 4, Road = road2 };
        lane22 = new() { Start = 3, End = 11, Road = road2 };

        road2.Lanes = new() { lane21, lane22 };

        lane31 = new() { Start = 1, End = 10, Road = road3 };
        road3.Lanes = new() { lane31 };

        lane41 = new() { Start = 3, End = 15, Road = road4 };
        road4.Lanes = new() { lane41 };
    }

    [SetUp]
    public void SetUp()
    {
        intersection = new();
    }

    [Test]
    public void OnetoOne()
    {
        Dictionary<int, List<Lane>> d = new()
        {
            [1] = new() { lane11, lane21 },
        };
        intersection.Roads = new() {road1, road2};
        intersection.NodeWithLane = d;

        Assert.True(intersection.IsRepeating());
        Assert.True(intersection.IsNodeConnected(1));
        List<int> nodes = intersection.GetNodes();
        nodes.Sort();
        Assert.AreEqual(new List<int>() {1}, nodes);
    }

    [Test]
    public void SetUp_TwotoTwo()
    {
        Dictionary<int, List<Lane>> d = new()
        {
            [1] = new() { lane11, lane21 },
            [3] = new() { lane12, lane22 }
        };
        intersection.Roads = new() {road1, road2};
        intersection.NodeWithLane = d;

        Assert.True(intersection.IsRepeating());
        Assert.True(intersection.IsNodeConnected(1));
        Assert.True(intersection.IsNodeConnected(3));
        List<int> nodes = intersection.GetNodes();
        nodes.Sort();
        Assert.AreEqual(new List<int>() {1, 3}, nodes);
    }

    [Test]
    public void TwotoOneOne()
    {
        Dictionary<int, List<Lane>> d = new()
        {
            [1] = new() { lane11, lane31 },
            [3] = new() { lane12, lane41 }
        };
        intersection.Roads = new() {road1, road3, road4};
        intersection.NodeWithLane = d;

        Assert.False(intersection.IsRepeating());
        Assert.AreSame(road1, intersection.GetMainRoad());
        Assert.True(intersection.IsNodeConnected(1));
        Assert.True(intersection.IsNodeConnected(3));
        List<int> nodes = intersection.GetNodes();
        nodes.Sort();
        Assert.AreEqual(new List<int>() {1, 3}, nodes);
        Assert.AreSame(road3, intersection.GetMinorRoadofNode(1));
        Assert.AreSame(road4, intersection.GetMinorRoadofNode(3));
    }

    [Test]
    public void ThreetoTwoOne()
    {
        Road road5 = new();
        Lane lane51 = new() { Start = 0, End = 1, Road = road5 };
        Lane lane52 = new() { Start = 0, End = 3, Road = road5 };
        Lane lane53 = new() { Start = 0, End = 5, Road = road5 };
        road5.Lanes = new() { lane51, lane52, lane53};

        Dictionary<int, List<Lane>> d = new()
        {
            [1] = new() { lane51, lane11 },
            [3] = new() { lane52, lane12 },
            [5] = new() { lane53, lane31 }
        };
        intersection.Roads = new() {road1, road3, road5};
        intersection.NodeWithLane = d;

        Assert.False(intersection.IsRepeating());
        Assert.AreSame(road5, intersection.GetMainRoad());
        Assert.True(intersection.IsNodeConnected(1));
        Assert.True(intersection.IsNodeConnected(3));
        Assert.True(intersection.IsNodeConnected(5));
        List<int> nodes = intersection.GetNodes();
        nodes.Sort();
        Assert.AreEqual(new List<int>() {1, 3, 5}, nodes);
        Assert.AreSame(road1, intersection.GetMinorRoadofNode(1));
        Assert.AreSame(road1, intersection.GetMinorRoadofNode(3));
        Assert.AreSame(road3, intersection.GetMinorRoadofNode(5));
    }

    [Test]
    public void TwotoOneNone()
    {
        Dictionary<int, List<Lane>> d = new()
        {
            [1] = new() { lane11, lane31 },
            [3] = new() { lane12 }
        };
        intersection.Roads = new() {road1, road3 };
        intersection.NodeWithLane = d;

        Assert.False(intersection.IsRepeating());
        Assert.AreSame(road1, intersection.GetMainRoad());
        Assert.True(intersection.IsNodeConnected(1));
        Assert.False(intersection.IsNodeConnected(3));
        List<int> nodes = intersection.GetNodes();
        nodes.Sort();
        Assert.AreEqual(new List<int>() {1, 3}, nodes);
        Assert.AreSame(road3, intersection.GetMinorRoadofNode(1));
        Assert.Null(intersection.GetMinorRoadofNode(3));
    }


    [Test]
    public void IsLazyIntializing()
    {
        Assert.IsNotNull(intersection.NodeWithLane);

    }

}