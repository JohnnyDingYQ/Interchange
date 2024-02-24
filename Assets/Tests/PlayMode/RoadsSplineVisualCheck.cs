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

        Grid.Height = 100;
        Grid.Width = 200;
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
        float2 origin = new(0, 10);
        client.LoadCoordinates(new List<float2>()
        {
            origin, origin + new float2(Offset, 0), origin + new float2(Offset, Offset)
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
        float2 origin = new(20, 10);
        client.LoadCoordinates(new List<float2>()
        {
            origin, origin + new float2(Offset, 0), origin + new float2(Offset, Offset)
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
        float2 origin = new(40, 10);
        client.LoadCoordinates(new List<float2>()
        {
            origin, origin + new float2(Offset, 0), origin + new float2(Offset, Offset)
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
        float2 origin = new(0, 20);
        client.LoadCoordinates(new List<float2>()
        {
            origin,
            origin + new float2(Offset, 0),
            origin + new float2(Offset, Offset),
            origin + new float2(Offset, Offset),
            origin + new float2(Offset, 2*Offset),
            origin + new float2(0, 2*Offset)
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
        List<int> MockID;
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

        public void LoadCoordinates(List<float2> mockCoord)
        {
            List<int> m = new();
            foreach (float2 coord in mockCoord)
                m.Add((int)(Grid.Height * coord.y + coord.x));
            MockID = m;
            count = 0;
        }

        public void EvaluateIntersection(Intersection intersection)
        {
            if (!MeshOn)
                return;
            RoadMesh.EvaluateIntersection(intersection);
        }

        public float3 GetPos()
        {
            return Grid.GetPosByID(MockID[count++]);
        }

        public void InstantiateRoad(Road road)
        {
            if (!MeshOn)
                return;
            RoadGameObject roadGameObject = Instantiate(roadPrefab, roads.transform, true);
            roadGameObject.name = $"Road-{road.Id}";
            road.RoadGameObject = roadGameObject;

            Mesh mesh = RoadMesh.CreateMesh(road, BuildManager.LaneCount);
            roadGameObject.GetComponent<MeshFilter>().mesh = mesh;
            roadGameObject.OriginalMesh = Instantiate(mesh);

            roadGameObject.Road = road;
        }

        public void RedrawAllRoads()
        {
            throw new System.NotImplementedException();
        }
    }
}