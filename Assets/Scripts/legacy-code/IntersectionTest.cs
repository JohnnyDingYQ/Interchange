// using System.Collections.Generic;
// using NUnit.Framework;

// public class IntersectionTest
// {
//     Intersection intersection;
//     Road road1 = new(), road2 = new(), road3 = new(), road4 = new(), road5 = new();
//     Lane lane11, lane12, lane21, lane22, lane31, lane41, lane51, lane52, lane53;
//     [OneTimeSetUp]
//     public void OneTimeSetUp()
//     {
//         lane11 = new() { StartNode = 0, EndNode = 1, Road = road1 };
//         lane12 = new() { StartNode = 2, EndNode = 3, Road = road1 };

//         road1.Lanes = new() { lane11, lane12 };

//         lane21 = new() { StartNode = 1, EndNode = 4, Road = road2 };
//         lane22 = new() { StartNode = 3, EndNode = 11, Road = road2 };

//         road2.Lanes = new() { lane21, lane22 };

//         lane31 = new() { StartNode = 1, EndNode = 10, Road = road3 };
//         road3.Lanes = new() { lane31 };

//         lane41 = new() { StartNode = 3, EndNode = 15, Road = road4 };
//         road4.Lanes = new() { lane41 };

//         lane51 = new() { StartNode = 0, EndNode = 1, Road = road5 };
//         lane52 = new() { StartNode = 0, EndNode = 3, Road = road5 };
//         lane53 = new() { StartNode = 0, EndNode = 5, Road = road5 };
//         road5.Lanes = new() { lane51, lane52, lane53 };
//     }

//     [SetUp]
//     public void SetUp()
//     {
//         intersection = new();
//     }

//     [Test]
//     public void OnetoOne()
//     {
//         CreateIntersection();

//         Assert.True(intersection.IsRepeating());
//         Assert.True(intersection.IsNodeConnected(1));
//         List<int> nodes = intersection.GetNodes();
//         nodes.Sort();
//         Assert.AreEqual(new List<int>() { 1 }, nodes);
//         Assert.AreEqual(lane11.EndNode, lane21.StartNode);

//         void CreateIntersection()
//         {
//             Dictionary<int, HashSet<Lane>> d = new()
//             {
//                 [1] = new() { lane11, lane21 },
//             };
//             intersection.Roads = new() { road1, road2 };
//             intersection.NodeWithLane = d;
//         }
//     }

//     [Test]
//     public void TwotoTwo()
//     {
//         CreateIntersection();

//         Assert.True(intersection.IsRepeating());
//         Assert.True(intersection.IsNodeConnected(1));
//         Assert.True(intersection.IsNodeConnected(3));
//         List<int> nodes = intersection.GetNodes();
//         nodes.Sort();
//         Assert.AreEqual(new List<int>() { 1, 3 }, nodes);
//         Assert.AreEqual(lane11.EndNode, lane21.StartNode);
//         Assert.AreEqual(lane12.EndNode, lane22.StartNode);

//         void CreateIntersection()
//         {
//             Dictionary<int, HashSet<Lane>> d = new()
//             {
//                 [1] = new() { lane11, lane21 },
//                 [3] = new() { lane12, lane22 }
//             };
//             intersection.Roads = new() { road1, road2 };
//             intersection.NodeWithLane = d;
//         }
//     }

//     [Test]
//     public void TwotoOneOne()
//     {
//         CreateIntersection();

//         Assert.False(intersection.IsRepeating());
//         Assert.AreSame(road1, intersection.GetMainRoad());
//         Assert.True(intersection.IsNodeConnected(1));
//         Assert.True(intersection.IsNodeConnected(3));
//         List<int> nodes = intersection.GetNodes();
//         nodes.Sort();
//         Assert.AreEqual(new List<int>() { 1, 3 }, nodes);
//         Assert.AreSame(road3, intersection.GetMinorRoadofNode(1));
//         Assert.AreSame(road4, intersection.GetMinorRoadofNode(3));
//         Assert.AreEqual(lane11.EndNode, lane31.StartNode);
//         Assert.AreEqual(lane12.EndNode, lane41.StartNode);

//         void CreateIntersection()
//         {
//             Dictionary<int, HashSet<Lane>> d = new()
//             {
//                 [1] = new() { lane11, lane31 },
//                 [3] = new() { lane12, lane41 }
//             };
//             intersection.Roads = new() { road1, road3, road4 };
//             intersection.NodeWithLane = d;
//         }
//     }

//     [Test]
//     public void ThreetoTwoOne()
//     {
//         CreateIntersection();

//         Assert.False(intersection.IsRepeating());
//         Assert.AreSame(road5, intersection.GetMainRoad());
//         Assert.True(intersection.IsNodeConnected(1));
//         Assert.True(intersection.IsNodeConnected(3));
//         Assert.True(intersection.IsNodeConnected(5));
//         List<int> nodes = intersection.GetNodes();
//         nodes.Sort();
//         Assert.AreEqual(new List<int>() { 1, 3, 5 }, nodes);
//         Assert.AreSame(road1, intersection.GetMinorRoadofNode(1));
//         Assert.AreSame(road1, intersection.GetMinorRoadofNode(3));
//         Assert.AreSame(road3, intersection.GetMinorRoadofNode(5));

//         void CreateIntersection()
//         {
//             Dictionary<int, HashSet<Lane>> d = new()
//             {
//                 [1] = new() { lane51, lane11 },
//                 [3] = new() { lane52, lane12 },
//                 [5] = new() { lane53, lane31 }
//             };
//             intersection.Roads = new() { road1, road3, road5 };
//             intersection.NodeWithLane = d;
//         }
//     }

//     [Test]
//     public void TwotoOneNone()
//     {
//         CreateIntersection();

//         Assert.False(intersection.IsRepeating());
//         Assert.AreSame(road1, intersection.GetMainRoad());
//         Assert.True(intersection.IsNodeConnected(1));
//         Assert.False(intersection.IsNodeConnected(3));
//         List<int> nodes = intersection.GetNodes();
//         nodes.Sort();
//         Assert.AreEqual(new List<int>() { 1, 3 }, nodes);
//         Assert.AreSame(road3, intersection.GetMinorRoadofNode(1));
//         Assert.Null(intersection.GetMinorRoadofNode(3));

//         void CreateIntersection()
//         {
//             Dictionary<int, HashSet<Lane>> d = new()
//             {
//                 [1] = new() { lane11, lane31 },
//                 [3] = new() { lane12 }
//             };
//             intersection.Roads = new() { road1, road3 };
//             intersection.NodeWithLane = d;
//         }
//     }


//     [Test]
//     public void IsLazyIntializing()
//     {
//         Assert.IsNotNull(intersection.NodeWithLane);

//     }

// }