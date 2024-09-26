using UnityEngine;
using UnityEngine.Splines;

public class ZoneObject : MonoBehaviour
{
    public ZoneColor zoneColor;
    public Zone Zone { get; set; }
    public Renderer meshRenderer;

    public void Init(SplineContainer splineContainer)
    {
        Mesh mesh = MeshUtil.GetPolygonMesh(splineContainer, 0.2f);
        MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        UpdateMaterial();
    }

    void Start()
    {
        UpdateMaterial();
    }

    void Update()
    {
        if (Zone.Enabled)
            UpdateMaterial();
    }

    void UpdateMaterial()
    {
        if (!Zone.Enabled)
            meshRenderer.material.SetColor("_Color", zoneColor.Disbaled);
        else
            meshRenderer.material.SetColor(
                "_Color",
                zoneColor.FullyConnected * Zone.Connectedness + zoneColor.Unconnected * (1 - Zone.Connectedness)
            );
    }
}