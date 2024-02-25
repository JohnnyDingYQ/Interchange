using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class RoadsSplineVisualCheck
{
    MockClient client;

    private const int Offset = 10;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        SceneManager.LoadScene("Build");
        BuildManager.Reset();
        client = new MockClient();
    }

    [UnityTest, Order(1)]
    public IEnumerator DrawOneLaneRoad()
    {
        float3 origin = new(0, 1, 10);
        client.LoadPosList(new List<float3>()
        {
            origin, origin + new float3(Offset, 0, 0), origin + new float3(Offset, 0, Offset)
        });
        BuildManager.Client = client;
        BuildManager.LaneCount = 1;
        for (int i = 0; i < 3; i++)
            BuildManager.HandleBuildCommand();
        yield return null;
    }

    [UnityTest, Order(2)]
    public IEnumerator DrawTwoLanesRoad()
    {
        float3 origin = new(20, 1, 10);
        client.LoadPosList(new List<float3>()
        {
            origin, origin + new float3(Offset, 0, 0), origin + new float3(Offset, 0, Offset)
        });
        BuildManager.LaneCount = 2;
        for (int i = 0; i < 3; i++)
            BuildManager.HandleBuildCommand();
        yield return null;
    }

    [UnityTest, Order(3)]
    public IEnumerator DrawThreeLanesRoad()
    {
        float3 origin = new(40, 1, 10);
        client.LoadPosList(new List<float3>()
        {
            origin, origin + new float3(Offset, 0, 0), origin + new float3(Offset, 0, Offset)
        });
        BuildManager.LaneCount = 3;
        for (int i = 0; i < 3; i++)
            BuildManager.HandleBuildCommand();
        yield return null;
    }

    [UnityTest, Order(4)]
    public IEnumerator DrawOneLaneRepeated()
    {
        float3 origin = new(0, 1, 30);
        client.LoadPosList(new List<float3>()
        {
            origin,
            origin + new float3(Offset, 0, 0),
            origin + new float3(Offset, 0, Offset),
            origin + new float3(Offset, 0, Offset),
            origin + new float3(Offset, 0, 2*Offset),
            origin + new float3(0, 0, 2*Offset)
        });
        BuildManager.LaneCount = 1;
        for (int i = 0; i < 6; i++)
            BuildManager.HandleBuildCommand();
        yield return null;
    }

    [UnityTest, Order(5)]
    public IEnumerator DrawTwoLaneRepeated()
    {
        float3 origin = new(20, 1, 30);
        client.LoadPosList(new List<float3>()
        {
            origin,
            origin + new float3(Offset, 0, 0),
            origin + new float3(Offset, 0, Offset),
            origin + new float3(Offset, 0, Offset),
            origin + new float3(Offset, 0, 2*Offset),
            origin + new float3(0, 0, 2*Offset)
        });
        BuildManager.LaneCount = 2;
        for (int i = 0; i < 6; i++)
            BuildManager.HandleBuildCommand();
        yield return null;
    }

    [UnityTest]
    public IEnumerator Blocker()
    {
        Utility.DrawAllSplines();
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
    }

    private class MockClient : IBuildManagerBoundary
    {
        List<float3> MockPos;
        int count = 0;

        public void LoadPosList(List<float3> pos)
        {
            count = 0;
            MockPos = pos;
        }

        public float3 GetPos()
        {
            return MockPos[count++];
        }

        public void InstantiateRoad(Road road)
        {
            return;
        }

        public void RedrawAllRoads()
        {
            throw new NotImplementedException();
        }
    }
}