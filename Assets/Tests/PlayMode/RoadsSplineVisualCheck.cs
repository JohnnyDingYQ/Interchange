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
    static GameObject roads;
    static RoadGameObject roadPrefab;
    static MockClient client;

    private const int Offset = 10;
    private static bool MeshOn;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        MeshOn = false;
        BuildManager.Reset();
        SceneManager.LoadScene("Build");
    }

    [UnityTest, Order(0)]
    public IEnumerator LoadClient()
    {
        return new MonoBehaviourTest<MockClient>();
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
        BuildManager.Client = client;
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
        BuildManager.Client = client;
        BuildManager.LaneCount = 3;
        for (int i = 0; i < 3; i++)
            BuildManager.HandleBuildCommand();
        yield return null;
    }

    [UnityTest, Order(4)]
    public IEnumerator DrawOneLaneRepeated()
    {
        float3 origin = new(0, 1, 20);
        client.LoadPosList(new List<float3>()
        {
            origin,
            origin + new float3(Offset, 0, 0),
            origin + new float3(Offset, 0, Offset),
            origin + new float3(Offset, 0, Offset),
            origin + new float3(Offset, 0, 2*Offset),
            origin + new float3(0, 0, 2*Offset)
        });
        BuildManager.Client = client;
        BuildManager.LaneCount = 1;
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

    private class MockClient : MonoBehaviour, IBuildManagerBoundary, IMonoBehaviourTest
    {
        List<float3> MockPos;
        int count = 0;
        bool finished = false;
        public bool IsTestFinished
        {
            get { return finished; }
        }

        void Start()
        {
            roads = GameObject.Find("Roads");
            string path = System.IO.Path.Combine(Application.streamingAssetsPath, "AssetBundles");
            path = System.IO.Path.Combine(path, "assetbundle");
            AssetBundle assetBundle = AssetBundle.LoadFromFile(path);
            if (assetBundle == null)
            {
                Debug.Log("Failed to load AssetBundle!");
                return;
            }
            var road = assetBundle.LoadAsset<GameObject>("Road");
            if (road == null)
                throw new InvalidOperationException("Road asset not found from assetbundle");
            roadPrefab = road.GetComponent<RoadGameObject>();
            client = this;
            finished = true;
        }

        public void LoadPosList(List<float3> pos)
        {
            MockPos = pos;
        }

        public MockClient(List<float3> mockCoord)
        {
            MockPos = mockCoord;
        }

        public float3 GetPos()
        {
            return MockPos[count++];
        }

        public void InstantiateRoad(Road road)
        {
            throw new NotImplementedException();
        }

        public void RedrawAllRoads()
        {
            throw new NotImplementedException();
        }
    }
}