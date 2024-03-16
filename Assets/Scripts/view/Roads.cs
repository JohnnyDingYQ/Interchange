using Unity.Mathematics;
using UnityEngine;

public class Roads : MonoBehaviour
{
    [SerializeField] private GameObject roads;
    [SerializeField] private RoadGameObject roadPrefab;
    [SerializeField] private InputManager inputManager;

    void Start()
    {
        if (inputManager != null)
        {
            inputManager.BuildRoad += HandleBuildCommand;
            inputManager.Build1Lane += BuildMode_OneLane;
            inputManager.Build2Lane += BuildMode_TwoLanes;
            inputManager.Build3Lane += BuildMode_ThreeLanes;
            inputManager.DividRoad += DivideRoad;
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
            inputManager.DividRoad -= DivideRoad;
        }
    }

    void HandleBuildCommand()
    {
        BuildHandler.HandleBuildCommand(InputManager.MouseWorldPos);
    }

    void DivideRoad()
    {
        
    }

    void BuildMode_OneLane()
    {
        BuildHandler.LaneCount = 1;
    }

    void BuildMode_TwoLanes()
    {
        BuildHandler.LaneCount = 2;
    }

    void BuildMode_ThreeLanes()
    {
        BuildHandler.LaneCount = 3;
    }

    # region legacy code
    public void InstantiateRoad(Road road)
    {
        return;
        // RoadGameObject roadGameObject = Instantiate(roadPrefab, roads.transform, true);
        // roadGameObject.name = $"Road-{road.Id}";
        // road.RoadGameObject = roadGameObject;

        // Mesh mesh = RoadMesh.CreateMesh(road, road.Lanes.Count);
        // roadGameObject.GetComponent<MeshFilter>().mesh = mesh;
        // roadGameObject.OriginalMesh = Instantiate(mesh);
        // MeshCollider meshCollider = roadGameObject.GetComponent<MeshCollider>();
        // meshCollider.sharedMesh = mesh;

        // roadGameObject.Road = road;
    }

    public void RedrawAllRoads()
    {
        while (roads.transform.childCount > 0) {
            DestroyImmediate(roads.transform.GetChild(0).gameObject);
        }

        foreach (Road road in Game.Roads.Values)
        {
            InstantiateRoad(road);
        }
    }
    #endregion
}