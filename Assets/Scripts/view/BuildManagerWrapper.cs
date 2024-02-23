using Unity.Mathematics;
using UnityEngine;

public class BuildManagerWrapper : MonoBehaviour, IBuildManagerBoundary
{
    [SerializeField] private GameObject roads;
    [SerializeField] private RoadGameObject roadPrefab;
    [SerializeField] private InputManager inputManager;

    void Start()
    {
        BuildManager.Client = this;

        if (inputManager != null)
        {
            inputManager.BuildRoad += HandleBuildCommand;
            inputManager.Build1Lane += BuildMode_OneLane;
            inputManager.Build2Lane += BuildMode_TwoLanes;
            inputManager.Build3Lane += BuildMode_ThreeLanes;
        }



    }
    void OnDestroy()
    {
        if (inputManager != null)
        {
            inputManager.BuildRoad -= HandleBuildCommand;
            inputManager.Build1Lane -= BuildMode_OneLane;
            inputManager.Build2Lane -= BuildMode_TwoLanes;
            inputManager.Build3Lane -= BuildMode_ThreeLanes;
        }
    }

    void HandleBuildCommand()
    {
        BuildManager.HandleBuildCommand();
    }

    void BuildMode_OneLane()
    {
        BuildManager.LaneCount = 1;
    }

    void BuildMode_TwoLanes()
    {
        BuildManager.LaneCount = 2;
    }

    void BuildMode_ThreeLanes()
    {
        BuildManager.LaneCount = 3;
    }

    public void InstantiateRoad(Road road)
    {
        RoadGameObject roadGameObject = Instantiate(roadPrefab, roads.transform, true);
        roadGameObject.name = $"Road-{road.Id}";
        road.RoadGameObject = roadGameObject;

        Mesh mesh = RoadMesh.CreateMesh(road, road.Lanes.Count);
        roadGameObject.GetComponent<MeshFilter>().mesh = mesh;
        roadGameObject.OriginalMesh = Instantiate(mesh);
        MeshCollider meshCollider = roadGameObject.GetComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;

        roadGameObject.Road = road;
    }

    public float3 GetPos()
    {
        return Main.MouseWorldPos;
    }

    public void EvaluateIntersection(Intersection intersection)
    {
        RoadMesh.EvaluateIntersection(intersection);
    }

    public void RedrawAllRoads()
    {
        while (roads.transform.childCount > 0) {
            DestroyImmediate(roads.transform.GetChild(0).gameObject);
        }

        foreach (Road road in BuildManager.RoadWatcher.Values)
        {
            InstantiateRoad(road);
        }
        foreach (Road road in BuildManager.RoadWatcher.Values)
        {
            EvaluateIntersection(road.StartIx);
            EvaluateIntersection(road.EndIx);
        }
    }
}