using log4net.Appender;
using Unity.Mathematics;
using UnityEngine;

public class BuildManagerGameObject : MonoBehaviour, IBuildManagerBoundary
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
        roadGameObject.name = $"Road-{BuildManager.NextAvailableId}";
        road.RoadGameObject = roadGameObject;

        Mesh mesh = RoadView.CreateMesh(road, BuildManager.LaneCount);
        roadGameObject.GetComponent<MeshFilter>().mesh = mesh;
        roadGameObject.OriginalMesh = Instantiate(mesh);

        roadGameObject.Road = road;
    }

    public float3 GetPos()
    {
        return Main.MouseWorldPos;
    }

    public void EvaluateIntersection(Intersection intersection)
    {
        RoadView.EvaluateIntersection(intersection);
    }
}