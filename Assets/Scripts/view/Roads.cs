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
        Game.DestroyRoad += DestroyRoad;
        roadMapping = new();
    }

    void OnDestroy()
    {
        Game.InstantiateRoad -= InstantiateRoad;
        Game.UpdateRoadMesh -= UpdateRoadMesh;
        Game.DestroyRoad -= DestroyRoad;
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
        Mesh m = MeshUtil.GetMesh(road);
        roadMapping[road].GetComponent<MeshFilter>().mesh = m;
        roadMapping[road].GetComponent<MeshCollider>().sharedMesh = m;
    }

    void DestroyRoad(Road road)
    {
        Destroy(roadMapping[road].gameObject);
        roadMapping.Remove(road);
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