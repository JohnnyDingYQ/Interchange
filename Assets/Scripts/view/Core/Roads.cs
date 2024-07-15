using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

public class Roads : MonoBehaviour
{
    [SerializeField]
    private RoadHumbleObject roadPrefab;
    [SerializeField]
    private GameObject arrowPrefab;
    [SerializeField]
    Texture oneLaneTex, twoLaneTex;
    private static Dictionary<uint, RoadHumbleObject> roadMapping;
    public static RoadHumbleObject HoveredRoad { get; set; }
    public static List<RoadHumbleObject> SelectedRoads { get; set; }
    private const int MaxRaycastHits = 10;
    private static readonly RaycastHit[] hitResults = new RaycastHit[MaxRaycastHits];
    private const int MaxColliderHits = 50;
    private static readonly Collider[] hitColliders = new Collider[MaxColliderHits];
    void Start()
    {
        Game.RoadAdded += InstantiateRoad;
        Game.RoadUpdated += UpdateRoadMesh;
        Game.RoadRemoved += DestroyRoad;
        roadMapping = new();
        SelectedRoads = new();
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
        CreateRoadArrows(roadGameObject);

        SetupTexture(roadGameObject);

        void SetupTexture(RoadHumbleObject roadGameObject)
        {
            Material material = roadGameObject.GetComponent<Renderer>().material;
            if (road.LaneCount == 1)
                material.SetTexture("_MainTex", oneLaneTex);
            else
                material.SetTexture("_MainTex", twoLaneTex);
        }
    }

    void CreateRoadArrows(RoadHumbleObject roadObject)
    {
        Assert.IsNotNull(roadObject.Road);
        foreach (float t in roadObject.Road.ArrowInterpolations)
        {
            GameObject arrow = Instantiate(arrowPrefab, roadObject.transform);
            float3 pos = roadObject.Road.EvaluatePosition(t);
            pos.y = Constants.MaxElevation + 1;
            arrow.transform.position = pos;
            float angle = Vector3.Angle(roadObject.Road.EvaluateTangent(t), Vector3.forward);
            if (math.cross(roadObject.Road.EvaluateTangent(t), Vector3.forward).y > 0)
                angle = 360 - angle;
            arrow.transform.eulerAngles = new(
                0,
                180 + angle,
                0
            );
            arrow.transform.localScale = new(0.8f, 0.8f, 0.8f);
        }
    }

    public void UpdateRoadMesh(Road road)
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
        if (HoveredRoad != null)
            UnHighLight(HoveredRoad.gameObject);
        float3 mousePos = InputSystem.MouseWorldPos;
        mousePos.y = Constants.MaxElevation + 1;

        // Perform the raycast and store the number of hits
        int hitCount = Physics.RaycastNonAlloc(new Ray(mousePos, new float3(0, -1, 0)), hitResults, Constants.MaxElevation + 2);

        HoveredRoad = null; // Reset HoveredRoad initially
        Game.HoveredRoad = null;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = hitResults[i];
            if (hit.collider.gameObject.TryGetComponent<RoadHumbleObject>(out var roadComp))
            {
                if (roadComp.Road.IsGhost)
                    continue;

                HoveredRoad = roadComp;
                Game.HoveredRoad = roadComp.Road;
                HighLight(roadComp.gameObject);
                return;
            }
        }
    }

    public static void BulkSelect(float3 start, float3 end)
    {
        ClearSelected();
        float3 diff = (start - end) / 2;
        float3 sum = start + (end - start) / 2;
        float3 halfExtent = math.abs(new float3(diff.x, (Constants.MaxElevation + 0.5f) / 2, diff.z));
        float3 center = new(sum.x, Constants.MaxElevation / 2, sum.z);

        int colliderCount = Physics.OverlapBoxNonAlloc(center, halfExtent, hitColliders, Quaternion.identity);
        for (int i = 0; i < colliderCount; i++)
        {
            Collider hitCollider = hitColliders[i];
            if (hitCollider.TryGetComponent<RoadHumbleObject>(out var roadComp))
            {
                HighLight(roadComp.gameObject);
                SelectedRoads.Add(roadComp);
            }
        }
    }

    public static void ClearSelected()
    {
        foreach (RoadHumbleObject g in SelectedRoads)
            if (g != null)
            UnHighLight(g.gameObject);
        SelectedRoads.Clear();
    }

    static void HighLight(GameObject g)
    {
        g.layer = LayerMask.NameToLayer("Outline");
    }

    static void UnHighLight(GameObject g)
    {
        g.layer = LayerMask.NameToLayer("Default");
    }
}