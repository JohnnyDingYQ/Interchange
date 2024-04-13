using System.Collections;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class RoadsSplineVisualCheck
{
    private const int Offset = 10;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        SceneManager.LoadScene("Build");
        BuildHandler.Reset();
    }

    [UnityTest, Order(1)]
    public IEnumerator DrawOneLaneRoad()
    {
        float3 origin = new(0, 1, 10);
        RoadBuilder.Build(origin, origin + new float3(Offset, 0, 0), origin + new float3(Offset, 0, Offset), 1);
        yield return null;
    }

    [UnityTest, Order(2)]
    public IEnumerator DrawTwoLanesRoad()
    {
        float3 origin = new(20, 1, 10);
        RoadBuilder.Build(origin, origin + new float3(Offset, 0, 0), origin + new float3(Offset, 0, Offset), 2);
        yield return null;
    }

    [UnityTest, Order(3)]
    public IEnumerator DrawThreeLanesRoad()
    {
        float3 origin = new(40, 1, 10);
        RoadBuilder.Build(origin, origin + new float3(Offset, 0, 0), origin + new float3(Offset, 0, Offset), 3);
        yield return null;
    }

    [UnityTest, Order(4)]
    public IEnumerator DrawOneLaneRepeated()
    {
        float3 origin = new(0, 1, 30);
        RoadBuilder.Build(origin, origin + new float3(Offset, 0, 0), origin + new float3(Offset, 0, Offset), 1);

        RoadBuilder.Build(
            origin + new float3(Offset, 0, Offset),
            origin + new float3(Offset, 0, 2 * Offset),
            origin + new float3(0, 0, 2 * Offset),
            1
        );

        yield return null;
    }

    [UnityTest, Order(5)]
    public IEnumerator DrawTwoLaneRepeated()
    {
        float3 origin = new(20, 1, 30);

        RoadBuilder.Build(
            origin,
            origin + new float3(Offset, 0, 0),
            origin + new float3(Offset, 0, Offset), 2
        );
        RoadBuilder.Build(
            origin + new float3(Offset, 0, Offset),
            origin + new float3(Offset, 0, 2 * Offset),
            origin + new float3(0, 0, 2 * Offset),
            2
        );

        yield return null;
    }

    [UnityTest]
    public IEnumerator Blocker()
    {
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
    }
}