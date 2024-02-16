using System;
using NUnit.Framework;
using UnityEngine;

public class GridTest
{
    [OneTimeSetUp]
    public void SetUp()
    {
        Grid.Height = 100;
        Grid.Width = 200;
        Grid.Dim = 1;
        Grid.Level = 0;
    }

    [Test]
    public void GetIdByPos_Expected()
    {
        Vector3 customPos = new(30.5f, 0, 30.5f);
        int expectedId = 30*100 + 30;
        Assert.AreEqual(expectedId, Grid.GetIdByPos(customPos));
    }

    [Test]
    public void GetIdByPos_Underflow()
    {
        Vector3 customPos = new(-30.5f, 0, 30.5f);
        int expectedId = -1;
        Assert.AreEqual(expectedId, Grid.GetIdByPos(customPos));
    }

    [Test]
    public void GetIdByPos_Overflow()
    {
        Vector3 customPos = new(200.5f, 0, 30.5f);
        int expectedId = -1;
        Assert.AreEqual(expectedId, Grid.GetIdByPos(customPos));
    }

    [Test]
    public void GetWorldPosByID_BadId()
    {
        int id = Grid.Height * Grid.Width + 1;
        Assert.Throws<ArgumentOutOfRangeException>(() => Grid.GetWorldPosByID(id));
    }

    [Test]
    public void GetWorldPosByID_Expected1()
    {
        int id = 30;
        float offset = (float)Grid.Dim/2;
        Vector3 expected = new(offset, 0, 30 + offset);
        Assert.AreEqual(expected, (Vector3) Grid.GetWorldPosByID(id));
    }

    [Test]
    public void GetWorldPosByID_Expected2()
    {
        int id = 270;
        float offset = (float)Grid.Dim/2;
        Vector3 expected = new(2 + offset, 0, 70 + offset);
        Assert.AreEqual(expected, (Vector3) Grid.GetWorldPosByID(id));
    }

    [Test]
    public void SnapPosToGrid()
    {
        Vector3 pos = new(30.25f, 0, 30.75f);
        Vector3 expected = new(30.5f, 0, 30.5f);
        Assert.AreEqual(1, Grid.Dim);
        Assert.AreEqual(expected, (Vector3) Grid.SnapPosToGrid(pos));
    }

}