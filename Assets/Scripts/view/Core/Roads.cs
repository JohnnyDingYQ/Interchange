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
    private const int MaxColliderHits = 100;
    private static readonly Collider[] hitColliders = new Collider[MaxColliderHits];
    private const string roadLayerName = "Roads";
    void Start()
    {
        Game.RoadAdded += InstantiateRoad;
        Game.RoadUpdated += UpdateRoad;
        Game.RoadRemoved += DestroyRoad;
        Game.RoadSelected += Highlight;
        Game.RoadUnselected += Unhighlight;
        roadMapping = new();
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
        Game.RoadSelected -= Highlight;
        Game.RoadUnselected -= Unhighlight;
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
        float3 pos = roadObject.Road.Curve.EvaluatePosition(roadObject.Road.Curve.Length / 2);
        pos.y = MathF.Max(roadObject.Road.StartPos.y, roadObject.Road.EndPos.y);
        arrow.transform.position = pos;
        float3 tangent = roadObject.Road.Curve.EvaluateTangent(roadObject.Road.Curve.Length / 2);
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
        float3 reference = CameraControl.Quaternion * new float3(0, 0, -1);
        if (MyNumerics.Get2DVectorsIntersection(squareSelector.StartPos, reference, pos, new float3(reference.z, 0, -reference.x), out Vector3 point))
        {
            squareSelector.StartPos.y = 0;
            pos.y = 0;
            float height = math.length(squareSelector.StartPos - (float3)point);
            float width = math.length(pos - (float3)point);
            squareSelector.SetTransform(width, height, squareSelector.StartPos + (pos - squareSelector.StartPos) / 2, CameraControl.Quaternion);
        }
        else
        {
            squareSelector.gameObject.SetActive(false);
        }
    }

    public void BulkSelect()
    {
        if (!squareSelector.Performed)
            return;
        ClearSelected();
        float3 halfExtent = new(squareSelector.Width / 2, (Constants.MaxElevation - Constants.MinElevation) / 1.8f, squareSelector.Height / 2);

        int layerMask = 1 << LayerMask.NameToLayer(roadLayerName) | 1 << LayerMask.NameToLayer("Outline");
        int colliderCount = Physics.OverlapBoxNonAlloc(squareSelector.Center, halfExtent, hitColliders, squareSelector.Quaternion, layerMask);
        for (int i = 0; i < colliderCount; i++)
        {
            Collider hitCollider = hitColliders[i];
            if (hitCollider.TryGetComponent<RoadObject>(out var roadComp))
                Game.SelectRoad(roadComp.Road);
        }
        squareSelector.Performed = false;
        squareSelector.gameObject.SetActive(false);
    }

    public List<Road> GetRoadsInSight()
    {
        List<Road> roads = new();
        int layerMask = 1 << LayerMask.NameToLayer(roadLayerName) | 1 << LayerMask.NameToLayer("Outline");
        Camera camera = Camera.main;
        float cameraHeight = camera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * camera.aspect;
        float3 halfExtent = new(cameraWidth / 2, camera.transform.position.y, cameraHeight / 2);
        int colliderCount = Physics.OverlapBoxNonAlloc(camera.transform.position, halfExtent, hitColliders, squareSelector.Quaternion, layerMask);
        for (int i = 0; i < colliderCount; i++)
        {
            Collider hitCollider = hitColliders[i];
            if (hitCollider.TryGetComponent<RoadObject>(out var roadComp))
            {
                Road road = roadComp.Road;
                if (!road.IsGhost)
                    roads.Add(roadComp.Road);
            }
        }
        return roads;
    }


    public void ClearSelected()
    {
        IEnumerable<Road> copy = Game.SelectedRoads.ToList();
        foreach (Road road in copy)
            Game.UnselectRoad(road);
    }

    public static void Highlight(Road road)
    {
        if (!roadMapping.TryGetValue(road.Id, out RoadObject roadObject))
            return;
        GameObject g = roadObject.gameObject;
        g.layer = LayerMask.NameToLayer("Outline");
    }


    public static void Unhighlight(Road road)
    {
        if (Game.SelectedRoads.Contains(road))
            return;
        if (!roadMapping.TryGetValue(road.Id, out RoadObject roadObject))
            return;
        GameObject g = roadObject.gameObject;
        g.layer = LayerMask.NameToLayer(roadLayerName);
    }
}