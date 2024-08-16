using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

public class Roads : MonoBehaviour
{
    [SerializeField]
    private RoadObject roadPrefab;
    [SerializeField]
    private GameObject arrowPrefab;
    [SerializeField]
    Texture oneLaneTex, twoLaneTex, threeLaneTex;
    [SerializeField]
    private SquareSelector squareSelectorPrefab;
    SquareSelector squareSelector;
    private static Dictionary<uint, RoadObject> roadMapping;
    public static RoadObject HoveredRoad { get; set; }
    public static List<RoadObject> SelectedRoads { get; set; }
    private const int MaxColliderHits = 100;
    private static readonly Collider[] hitColliders = new Collider[MaxColliderHits];
    private const string roadLayerName = "Roads";
    void Start()
    {
        Game.RoadAdded += InstantiateRoad;
        Game.RoadUpdated += UpdateRoad;
        Game.RoadRemoved += DestroyRoad;
        roadMapping = new();
        SelectedRoads = new();
        squareSelector = Instantiate(squareSelectorPrefab, transform);
    }

    void Update()
    {
        if (squareSelector.Performed)
            UpdateSquareSelector(InputSystem.MouseWorldPos);
    }

    void OnDestroy()
    {
        Game.RoadAdded -= InstantiateRoad;
        Game.RoadUpdated -= UpdateRoad;
        Game.RoadRemoved -= DestroyRoad;
    }

    void InstantiateRoad(Road road)
    {
        if (roadMapping.ContainsKey(road.Id))
            DestroyRoad(roadMapping[road.Id].Road);

        RoadObject roadComp = Instantiate(roadPrefab, transform, true);
        roadComp.name = $"Road-{road.Id}";
        roadComp.Road = road;
        roadComp.gameObject.isStatic = true;
        roadComp.gameObject.layer = LayerMask.NameToLayer(roadLayerName);
        roadMapping[road.Id] = roadComp;

        SetRoadArrow(roadComp);
        SetupTexture(roadComp);

        void SetupTexture(RoadObject roadGameObject)
        {
            Material material = roadGameObject.GetComponent<Renderer>().material;
            if (road.LaneCount == 1)
                material.SetTexture("_MainTex", oneLaneTex);
            else if (road.LaneCount == 2)
                material.SetTexture("_MainTex", twoLaneTex);
            else if (road.LaneCount == 3)
                material.SetTexture("_MainTex", threeLaneTex);
        }
    }

    void SetRoadArrow(RoadObject roadObject)
    {
        Assert.IsNotNull(roadObject.Road);
        GameObject arrow;
        if (roadObject.transform.childCount == 0)
            arrow = Instantiate(arrowPrefab, roadObject.transform);
        else
            arrow = roadObject.transform.GetChild(0).gameObject;
        float3 pos = roadObject.Road.Curve.EvaluateDistancePos(roadObject.Road.Curve.Length / 2);
        pos.y = MathF.Max(roadObject.Road.StartPos.y, roadObject.Road.EndPos.y);
        arrow.transform.position = pos;
        float3 tangent = roadObject.Road.Curve.EvaluateDistanceTangent(roadObject.Road.Curve.Length / 2);
        float angle = Vector3.Angle(tangent, Vector3.forward);
        if (math.cross(tangent, Vector3.forward).y > 0)
            angle = 360 - angle;
        arrow.transform.eulerAngles = new(
            0,
            180 + angle,
            0
        );
        arrow.transform.localScale = new(0.8f, 0.8f, 0.8f);
    }

    void UpdateRoad(Road road)
    {
        Mesh m = MeshUtil.GetRoadMesh(road);
        RoadObject roadObject = roadMapping[road.Id];
        roadObject.GetComponent<MeshFilter>().mesh = m;
        roadObject.GetComponent<MeshCollider>().sharedMesh = m;
        SetRoadArrow(roadObject);
    }

    void DestroyRoad(Road road)
    {
        Destroy(roadMapping[road.Id].gameObject);
        roadMapping.Remove(road.Id);
    }

    public void DestoryAll()
    {
        while (transform.childCount > 0)
            DestroyImmediate(transform.GetChild(0).gameObject);
        roadMapping.Clear();
    }

    public void BulkSelectStart(float3 pos)
    {
        squareSelector.StartPos = pos;
    }

    public void BulkSelectPerformed()
    {
        squareSelector.Performed = true;
    }

    public void UpdateSquareSelector(float3 pos)
    {
        squareSelector.gameObject.SetActive(true);
        float width = Math.Abs(squareSelector.StartPos.x - pos.x);
        float height = Math.Abs(squareSelector.StartPos.z - pos.z);
        squareSelector.SetTransform(width, height, squareSelector.StartPos + (pos - squareSelector.StartPos) / 2);
    }

    public void BulkSelect(float3 end)
    {
        ClearSelected();
        float3 diff = (squareSelector.StartPos - end) / 2;
        float3 sum = squareSelector.StartPos + (end - squareSelector.StartPos) / 2;
        float3 halfExtent = math.abs(new float3(diff.x, (Constants.MaxElevation + 0.5f) / 2, diff.z));
        float3 center = new(sum.x, Constants.MaxElevation / 2, sum.z);

        int layerMask = 1 << LayerMask.NameToLayer(roadLayerName) | 1 << LayerMask.NameToLayer("Outline");
        int colliderCount = Physics.OverlapBoxNonAlloc(center, halfExtent, hitColliders, Quaternion.identity, layerMask);
        for (int i = 0; i < colliderCount; i++)
        {
            Collider hitCollider = hitColliders[i];
            if (hitCollider.TryGetComponent<RoadObject>(out var roadComp))
            {
                HighLight(roadComp.gameObject);
                SelectedRoads.Add(roadComp);
            }
        }
        squareSelector.Performed = false;
        squareSelector.gameObject.SetActive(false);
    }

    public static void ClearSelected()
    {
        foreach (RoadObject g in SelectedRoads)
            if (g != null)
                UnHighLight(g.gameObject);
        SelectedRoads.Clear();
    }

    static void HighLight(GameObject g)
    {
        g.layer = LayerMask.NameToLayer("Outline");
    }

    public static void HighLight(Road road)
    {
        GameObject g = roadMapping[road.Id].gameObject;
        g.layer = LayerMask.NameToLayer("Outline");
    }

    static void UnHighLight(GameObject g)
    {
        g.layer = LayerMask.NameToLayer(roadLayerName);
    }

    public static void UnHighLight(Road road)
    {
        GameObject g = roadMapping[road.Id].gameObject;
        g.layer = LayerMask.NameToLayer(roadLayerName);
    }
}