using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Roads : MonoBehaviour
{
    [SerializeField] private RoadHumbleObject roadPrefab;
    private static Dictionary<uint, RoadHumbleObject> roadMapping;
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
        
        RoadHumbleObject roadGameObject = Instantiate(roadPrefab, transform, true);
        roadGameObject.name = $"Road-{road.Id}";
        roadGameObject.Road = road;
        roadGameObject.gameObject.isStatic = true;
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

    public static void UpdateHoveredRoad()
    {
        if (Game.HoveredRoad != null && roadMapping.ContainsKey(Game.HoveredRoad.Id))
            roadMapping[Game.HoveredRoad.Id].gameObject.layer = LayerMask.NameToLayer("Default");
        float3 mousePos = InputSystem.MouseWorldPos;
        mousePos.y = Constants.MaxElevation + 1;
        RaycastHit[] hits;
        hits = Physics.RaycastAll(mousePos, new float3(0, -1, 0), 100);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.TryGetComponent<RoadHumbleObject>(out var roadObject))
            {
                if (roadObject.Road.IsGhost)
                    continue;
                Game.HoveredRoad = roadObject.Road;
                roadMapping[Game.HoveredRoad.Id].gameObject.layer = LayerMask.NameToLayer("Outline");
                return;
            }
        }
        Game.HoveredRoad = null;
    }
}