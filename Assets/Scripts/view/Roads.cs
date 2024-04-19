using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Roads : MonoBehaviour
{
    [SerializeField] private RoadGameObject roadPrefab;
    private Dictionary<Road, RoadGameObject> roadMapping;
    void Start()
    {
        Game.InstantiateRoad += InstantiateRoad;
        Game.UpdateRoadMesh += UpdateRoadMesh;
        roadMapping = new();
    }

    void OnDestroy()
    {
        Game.InstantiateRoad -= InstantiateRoad;
        Game.UpdateRoadMesh -= UpdateRoadMesh;
    }

    void InstantiateRoad(Road road)
    {
        RoadGameObject roadGameObject = Instantiate(roadPrefab, transform, true);
        roadGameObject.name = $"Road-{road.Id}";
        roadGameObject.Road = road;
        roadMapping[road] = roadGameObject;
        UpdateRoadMesh(road);
    }

    void UpdateRoadMesh(Road road)
    {
        roadMapping[road].GetComponent<MeshFilter>().mesh = MeshUtil.GetMesh(road);
    }

    #region legacy code
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
    #endregion
}