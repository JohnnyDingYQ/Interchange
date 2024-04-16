using Unity.Mathematics;
using UnityEngine;

public class Roads : MonoBehaviour
{
    [SerializeField] private GameObject roads;
    // [SerializeField] private RoadGameObject roadPrefab;
    [SerializeField] private InputSystem inputManager;


    void HandleBuildCommand()
    {
        BuildHandler.HandleBuildCommand(InputSystem.MouseWorldPos);
    }

    # region legacy code
    // public void InstantiateRoad(Road road)
    // {
        // return;
        // RoadGameObject roadGameObject = Instantiate(roadPrefab, roads.transform, true);
        // roadGameObject.name = $"Road-{road.Id}";
        // road.RoadGameObject = roadGameObject;

        // Mesh mesh = RoadMesh.CreateMesh(road, road.Lanes.Count);
        // roadGameObject.GetComponent<MeshFilter>().mesh = mesh;
        // roadGameObject.OriginalMesh = Instantiate(mesh);
        // MeshCollider meshCollider = roadGameObject.GetComponent<MeshCollider>();
        // meshCollider.sharedMesh = mesh;

        // roadGameObject.Road = road;
    // }

    // public void RedrawAllRoads()
    // {
    //     while (roads.transform.childCount > 0) {
    //         DestroyImmediate(roads.transform.GetChild(0).gameObject);
    //     }

    //     foreach (Road road in Game.Roads.Values)
    //     {
    //         InstantiateRoad(road);
    //     }
    // }
    #endregion
}