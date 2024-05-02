using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Roads : MonoBehaviour
{
    [SerializeField] private RoadGameObject roadPrefab;
    private static Dictionary<Road, RoadGameObject> roadMapping;
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
        RoadGameObject roadGameObject = Instantiate(roadPrefab, transform, true);
        roadGameObject.name = $"Road-{road.Id}";
        roadGameObject.Road = road;
        roadMapping[road] = roadGameObject;
        UpdateRoadMesh(road);
    }

    public static void UpdateRoadMesh(Road road)
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
}