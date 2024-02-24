// using System;
// using NUnit.Framework;
// using Unity.Mathematics;
// using UnityEngine;

// public class GridTest
// {
//     [OneTimeSetUp]
//     public void SetUp()
//     {
//         Grid.Height = 100;
//         Grid.Width = 200;
//         Grid.Dim = 1;
//         Grid.Level = 0;
//     }

//     [Test]
//     public void GetIdByPos_Expected()
//     {
//         Vector3 customPos = new(30.5f, 0, 30.5f);
//         int expectedId = 30*100 + 30;
//         Assert.AreEqual(expectedId, Grid.GetIdByPos(customPos));
//     }

//     [Test]
//     public void GetIdByPos_Underflow()
//     {
//         Vector3 customPos = new(-30.5f, 0, 30.5f);
//         int expectedId = -1;
//         Assert.AreEqual(expectedId, Grid.GetIdByPos(customPos));
//     }

//     [Test]
//     public void GetIdByPos_Overflow()
//     {
//         Vector3 customPos = new(200.5f, 0, 30.5f);
//         int expectedId = -1;
//         Assert.AreEqual(expectedId, Grid.GetIdByPos(customPos));
//     }

//     [Test]
//     public void GetWorldPosByID_BadId()
//     {
//         int id = Grid.Height * Grid.Width + 1;
//         Assert.Throws<ArgumentOutOfRangeException>(() => Grid.GetPosByID(id));
//     }

//     [Test]
//     public void GetWorldPosByID_Expected1()
//     {
//         int id = 30;
//         float offset = (float)Grid.Dim/2;
//         Vector3 expected = new(offset, 0, 30 + offset);
//         Assert.AreEqual(expected, (Vector3) Grid.GetPosByID(id));
//     }

//     [Test]
//     public void GetWorldPosByID_Expected2()
//     {
//         int id = 270;
//         float offset = (float)Grid.Dim/2;
//         Vector3 expected = new(2 + offset, 0, 70 + offset);
//         Assert.AreEqual(expected, (Vector3) Grid.GetPosByID(id));
//     }

//     [Test]
//     public void Reciprocity()
//     {
//         int id = 5;
//         Assert.AreEqual(id, Grid.GetIdByPos(Grid.GetPosByID(5)));

//         Vector3 pos = new(30.5f, 0, 15.5f);
//         Assert.AreEqual(pos, (Vector3) Grid.GetPosByID(Grid.GetIdByPos(pos)));
//     }

//     [Test]
//     public void SnapPosToGrid()
//     {
//         Vector3 pos = new(30.25f, 0, 30.75f);
//         Vector3 expected = new(30.5f, 0, 30.5f);
//         Assert.AreEqual(1, Grid.Dim);
//         Assert.AreEqual(expected, (Vector3) Grid.SnapPosToGrid(pos));
//     }

//     [Test]
//     public void GetCoordinateBadArgument()
//     {
//         Assert.Throws<ArgumentOutOfRangeException>(() =>Grid.GetCoordinate(-1));
//         Assert.Throws<ArgumentOutOfRangeException>(() =>Grid.GetCoordinate(100*200 + 1));
        
//     }

//     [Test]
//     public void GetCoordinateZero()
//     {
//         Assert.AreEqual(new float2(0, 0), Grid.GetCoordinate(0));
//     }

//     [Test]
//     public void GetCoordinateNormal()
//     {
//         Assert.AreEqual(new float2(3, 0), Grid.GetCoordinate(3));
//         Assert.AreEqual(new float2(3, 5), Grid.GetCoordinate(503));
//     }

//     [Test]
//     public void GetDistanceBadArgument()
//     {
//         Assert.Throws<ArgumentOutOfRangeException>(() => Grid.GetDistance(-1, 100));
//         Assert.Throws<ArgumentOutOfRangeException>(() => Grid.GetDistance(100, Grid.Height * Grid.Width +  1));
//     }

//     [Test]
//     public void GetDistanceIdenticalNodesGiven()
//     {
//         Assert.AreEqual(0.0f, Grid.GetDistance(50, 50));
//     }

//     [Test]
//     public void GetDistanceDifferentNodes()
//     {
//         Assert.AreNotEqual(0.0f, Grid.GetDistance(50, 60));
//         Assert.AreEqual(10, Grid.GetDistance(50, 60));
//         Assert.AreEqual((float) Math.Sqrt(Math.Pow(50 - 70, 2) + Math.Pow(0 - 4, 2)), Grid.GetDistance(50, 470));
//     }

// }