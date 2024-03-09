using NUnit.Framework;
using UnityEngine;

public class RoadDivisionTest
{
    Vector3 pos1 = new(0, 10, 0);
    Vector3 pos2 = new(0, 12, 30);
    Vector3 pos3 = new(0, 14, 60);
    Vector3 pos4 = new(90, 16, 90);
    Vector3 pos5 = new(120, 16, 120);
    Vector3 pos6 = new(150, 16, 150);

    [SetUp]
    public void SetUp()
    {
        BuildHandler.Reset();
        Game.WipeState();
    }
}