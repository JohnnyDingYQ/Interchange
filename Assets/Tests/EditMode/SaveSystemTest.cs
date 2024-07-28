using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using QuikGraph;
using QuikGraph.Collections;
using Unity.Mathematics;
using UnityEngine;
public class SaveSystemTest
{
    float3 stride = Constants.MinLaneLength * new float3(1, 0, 1);

    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
    }

    [Test]
    public void RestoreCurve()
    {
        Curve original = new(new(0, stride, 2 * stride));
        original = original.AddStartDistance(Constants.MinLaneLength / 5);
        original = original.AddEndDistance(Constants.MinLaneLength / 5);
        original.Offset(3);
        original.Id = 5;
        Storage storage = new();
        storage.Save(original);
        Curve loaded = new();
        storage.Load(loaded);

        Assert.AreEqual(original, loaded);
    }

//     [Test]
//     public void RecoverSingleOneLaneRoad()
//     {
//         RoadBuilder.Single(0, stride, 2 * stride, 1);
//         SaveSystem.SaveGame();
//         SaveSystem.LoadGame();

//         Assert.AreEqual(1, Game.Roads.Count);
//         Road road = Game.Roads.Values.First();
//         Assert.AreEqual(new float3(0), road.StartPos);
//         Assert.AreEqual(2 * stride, road.EndPos);
//         Assert.AreEqual(1, road.Lanes.Count);
//         Lane lane = road.Lanes.First();
//         Assert.AreEqual(new float3(0), lane.StartPos);
//         Assert.True(MyNumerics.AreNumericallyEqual(2 * stride, lane.EndPos));
//         Assert.AreEqual(2, Game.Nodes.Count);
//         Assert.AreSame(lane.StartNode.OutLane, lane);
//         Assert.AreSame(lane.EndNode.InLane, lane);

//     }

//     [Test]
//     public void RecoverSingleThreeLaneRoad()
//     {
//         RoadBuilder.Single(0, stride, 2 * stride, 3);
//         SaveSystem.SaveGame();
//         SaveSystem.LoadGame();

//         Assert.AreEqual(1, Game.Roads.Count);
//         Road road = Game.Roads.Values.First();
//         Assert.AreEqual(new float3(0), road.StartPos); ;
//         Assert.AreEqual(2 * stride, road.EndPos);
//         Assert.AreEqual(3, road.Lanes.Count);
//         Lane lane = road.Lanes[1];
//         Assert.AreEqual(new float3(0), lane.StartPos);
//         Assert.True(MyNumerics.AreNumericallyEqual(2 * stride, lane.EndPos));
//         Assert.AreEqual(6, Game.Nodes.Count);
//         Assert.AreSame(road.Lanes[0].StartNode.OutLane, road.Lanes[0]);
//         Assert.AreSame(road.Lanes[0].EndNode.InLane, road.Lanes[0]);
//         Assert.AreSame(road.Lanes[1].StartNode.OutLane, road.Lanes[1]);
//         Assert.AreSame(road.Lanes[1].EndNode.InLane, road.Lanes[1]);
//         Assert.AreSame(road.Lanes[2].StartNode.OutLane, road.Lanes[2]);
//         Assert.AreSame(road.Lanes[2].EndNode.InLane, road.Lanes[2]);
//         Assert.Null(road.Lanes[0].StartNode.InLane);
//         Assert.Null(road.Lanes[0].EndNode.OutLane);
//         Assert.Null(road.Lanes[1].StartNode.InLane);
//         Assert.Null(road.Lanes[1].EndNode.OutLane);
//         Assert.Null(road.Lanes[2].StartNode.InLane);
//         Assert.Null(road.Lanes[2].EndNode.OutLane);
//     }

//     [Test]
//     public void RecoverTwoDisconnectedOneLaneRoad()
//     {
//         Road road0 = RoadBuilder.Single(0, stride, 2 * stride, 1);
//         Road road1 = RoadBuilder.Single(3 * stride, 4 * stride, 5 * stride, 1);

//         SaveSystem.SaveGame();
//         SaveSystem.LoadGame();

//         Assert.AreEqual(2, Game.Roads.Count);
//         Assert.AreEqual(new float3(0), road0.StartPos);
//         Assert.AreEqual(2 * stride, road0.EndPos);
//         Assert.AreEqual(3 * stride, road1.StartPos);
//         Assert.True(MyNumerics.AreNumericallyEqual(5 * stride, road1.EndPos));
//         Assert.AreEqual(4, Game.Nodes.Count);
//         Assert.AreSame(road0.Lanes[0].StartNode.OutLane, road0.Lanes[0]);
//         Assert.AreSame(road0.Lanes[0].EndNode.InLane, road0.Lanes[0]);
//         Assert.AreSame(road1.Lanes[0].StartNode.OutLane, road1.Lanes[0]);
//         Assert.AreSame(road1.Lanes[0].EndNode.InLane, road1.Lanes[0]);
//         Assert.Null(road0.Lanes[0].StartNode.InLane);
//         Assert.Null(road0.Lanes[0].EndNode.OutLane);
//         Assert.Null(road1.Lanes[0].StartNode.InLane);
//         Assert.Null(road1.Lanes[0].EndNode.OutLane);
//     }

//     [Test]
//     public void RecoverTwoConnectedOneLaneRoad()
//     {
//         Road road0 = RoadBuilder.Single(0, stride, 2 * stride, 1);
//         Road road1 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);

//         SaveSystem.SaveGame();
//         SaveSystem.LoadGame();

//         Assert.AreEqual(2, Game.Roads.Count);
//         Assert.AreEqual(new float3(0), road0.StartPos);
//         Assert.AreEqual(2 * stride, road0.EndPos);
//         Assert.True(MyNumerics.AreNumericallyEqual(2 * stride, road1.StartPos));
//         Assert.True(MyNumerics.AreNumericallyEqual(4 * stride, road1.EndPos));
//         Assert.AreEqual(3, Game.Nodes.Count);
//         Assert.AreSame(road0.Lanes[0].EndNode, road1.Lanes[0].StartNode);
//         Assert.AreSame(road0.Lanes[0].StartNode.OutLane, road0.Lanes[0]);
//         Assert.AreSame(road0.Lanes[0].EndNode.InLane, road0.Lanes[0]);
//         Assert.AreSame(road1.Lanes[0].StartNode.OutLane, road1.Lanes[0]);
//         Assert.AreSame(road1.Lanes[0].EndNode.InLane, road1.Lanes[0]);
//     }

//     [Test]
//     public void RecoverLanesCurves()
//     {
//         RoadBuilder.Single(0, stride, 2 * stride, 1);
//         RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
//         SaveSystem.SaveGame();
//         SaveSystem.LoadGame();

//         foreach (Road road in Game.Roads.Values)
//         {
//             Assert.NotNull(road.Curve);
//             foreach (Lane lane in road.Lanes)
//                 Assert.NotNull(lane.Curve);
//         }

//     }

//     [Test]
//     public void RecoverPathGraph()
//     {
//         RoadBuilder.Single(0, stride, 2 * stride, 1);
//         RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
//         SaveSystem.SaveGame();
//         SaveSystem.LoadGame();

//         Assert.AreEqual(3, Game.Paths.Count);
//         Assert.AreEqual(4, Game.Vertices.Count);
//         foreach (Path path in Game.Paths.Values)
//             Assert.NotNull(path.Curve);
//     }

//     [Test]
//     public void RecoverNodeLaneDirections()
//     {
//         Road road = RoadBuilder.Single(0, stride, 2 * stride, 1);
//         Lane lane = road.Lanes.First();
//         Assert.AreSame(lane, lane.StartNode.OutLane);
//         Assert.AreSame(lane, lane.EndNode.InLane);
//         SaveSystem.SaveGame();
//         SaveSystem.LoadGame();
//         road = Game.Roads.Values.First();
//         lane = road.Lanes.First();
//         Assert.AreSame(lane, lane.StartNode.OutLane);
//         Assert.AreSame(lane, lane.EndNode.InLane);

//     }

//     [Test]
//     public void RecoverRoadOutline()
//     {
//         Road saved1 = RoadBuilder.Single(0, stride, 2 * stride, 1);
//         Road saved2 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
//         Road saved3 = RoadBuilder.Single(4 * stride, 5 * stride, 6 * stride, 1);
//         SaveSystem.SaveGame();
//         SaveSystem.LoadGame();
//         Road restored1 = Game.Roads[saved1.Id];
//         Road restored2 = Game.Roads[saved2.Id];
//         Road restored3 = Game.Roads[saved3.Id];

//         Assert.True(saved1.LeftOutline.Equals(restored1.LeftOutline));
//         Assert.True(saved1.RightOutline.Equals(restored1.RightOutline));
//         Assert.True(saved2.LeftOutline.Equals(restored2.LeftOutline));
//         Assert.True(saved2.RightOutline.Equals(restored2.RightOutline));
//         Assert.True(saved3.LeftOutline.Equals(restored3.LeftOutline));
//         Assert.True(saved3.RightOutline.Equals(restored3.RightOutline));
//     }

//     [Test]
//     public void RecoverConnectedLanes_2to1and1()
//     {
//         Road saved1 = RoadBuilder.Single(0, stride, 2 * stride, 2);
//         float3 offset = saved1.Lanes[0].EndPos - saved1.EndPos;
//         Road saved2 = RoadBuilder.Single(2 * stride + offset, 3 * stride + offset, 4 * stride + offset, 1);
//         Road saved3 = RoadBuilder.Single(2 * stride - offset, 3 * stride - offset, 4 * stride - offset, 1);
//         SaveSystem.SaveGame();
//         SaveSystem.LoadGame();
//         Road restored1 = Game.Roads[saved1.Id];
//         Road restored2 = Game.Roads[saved2.Id];
//         Road restored3 = Game.Roads[saved3.Id];

//         Assert.True(Graph.ContainsPath(restored1.Lanes[0], restored2.Lanes[0]));
//         Assert.True(Graph.ContainsPath(restored1.Lanes[1], restored3.Lanes[0]));
//         Assert.AreSame(restored1.Lanes[0].EndNode.OutLane, restored2.Lanes[0]);
//         Assert.AreSame(restored1.Lanes[1].EndNode.OutLane, restored3.Lanes[0]);
//     }

//     [Test]
//     public void RecoverIntersection()
//     {
//         Road saved = RoadBuilder.Single(0, stride, 2 * stride, 2);
//         SaveSystem.SaveGame();
//         SaveSystem.LoadGame();

//         Road restored = Game.Roads.Values.First();
//         Assert.NotNull(restored.StartIntersection);
//         Assert.NotNull(restored.EndIntersection);
//         Assert.AreSame(restored.StartIntersection, restored.Lanes[0].StartNode.Intersection);
//         Assert.AreSame(restored.EndIntersection, restored.Lanes[0].EndNode.Intersection);
//         Assert.AreSame(restored.StartIntersection, restored.Lanes[1].StartNode.Intersection);
//         Assert.AreSame(restored.EndIntersection, restored.Lanes[1].EndNode.Intersection);
//         Assert.True(MyNumerics.AreNumericallyEqual(saved.EndIntersection.Normal, restored.EndIntersection.Normal));
//         Assert.True(MyNumerics.AreNumericallyEqual(saved.EndIntersection.Tangent, restored.EndIntersection.Tangent));
//         Assert.True(MyNumerics.AreNumericallyEqual(saved.EndIntersection.PointOnInSide, restored.EndIntersection.PointOnInSide));

//     }

//     [Test]
//     public void RecoverInterweavingPath()
//     {
//         Road road0 = RoadBuilder.Single(0, stride, 2 * stride, 2);
//         RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 2);
//         uint id = road0.Id;
//         Assert.NotNull(Graph.GetOutPaths(road0.Lanes.First().EndVertex).Last().InterweavingPath);
        
//         SaveSystem.SaveGame();
//         SaveSystem.LoadGame();
//         Road recovered = Game.Roads[id];
//         Assert.NotNull(Graph.GetOutPaths(recovered.Lanes.First().EndVertex).Last().InterweavingPath);

//     }

//     [Test]
//     public void SavingAfterRemovingRoad()
//     {
//         RoadBuilder.Single(0, stride, 2 * stride, 3);
//         Road road1 = RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);
//         Game.RemoveRoad(road1);

//         SaveSystem.SaveGame();
//         SaveSystem.LoadGame();
//     }

//     [Test]
//     public void SimpleBuildAfterSaveLoad()
//     {
//         RoadBuilder.Single(0, stride, 2 * stride, 1);
//         SaveSystem.SaveGame();
//         SaveSystem.LoadGame();
//         RoadBuilder.Single(2 * stride, 3 * stride, 4 * stride, 1);

//         Assert.AreEqual(2, Game.Roads.Count);
//         Assert.AreEqual(3, Game.Nodes.Count);
//     }

//     [Test]
//     public void RecoverArrowPositions()
//     {
//         RoadBuilder.Single(0, stride, 2 * stride, 1);
//         SaveSystem.SaveGame();
//         SaveSystem.LoadGame();
//         Assert.NotNull(Game.Roads.Values.First().ArrowInterpolations);
//     }

//     // TODO: Complete further testing

}