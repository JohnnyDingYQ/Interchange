using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Roads : MonoBehaviour
{
    [SerializeField] private RoadGameObject roadPrefab;
    private static Dictionary<int, RoadGameObject> roadMapping;
    void Start()
    {
        Game.RoadAdded += InstantiateRoad;
        Game.RoadUpdated += UpdateRoadMesh;
        Game.RoadRemoved += DestroyRoad;
        roadMapping = new();
    }

    void OnDestroy()
    {
        Game.RoadAdded -= InstantiateRoad;
        Game.RoadUpdated -= UpdateRoadMesh;
        Game.RoadRemoved -= DestroyRoad;
    }

    void InstantiateRoad(Road road)
    {
        if (roadMapping.ContainsKey(road.Id))
            DestroyRoad(roadMapping[road.Id].Road);
        
        RoadGameObject roadGameObject = Instantiate(roadPrefab, transform, true);
        roadGameObject.name = $"Road-{road.Id}";
        roadGameObject.Road = road;
        roadMapping[road.Id] = roadGameObject;
        UpdateRoadMesh(road);
    }

    public static void UpdateRoadMesh(Road road)
    {
        Mesh m = MeshUtil.GetMesh(road);
        roadMapping[road.Id].GetComponent<MeshFilter>().mesh = m;
        roadMapping[road.Id].GetComponent<MeshCollider>().sharedMesh = m;
    }

    void DestroyRoad(Road road)
    {
        Destroy(roadMapping[road.Id].gameObject);
        roadMapping.Remove(road.Id);
    }
}