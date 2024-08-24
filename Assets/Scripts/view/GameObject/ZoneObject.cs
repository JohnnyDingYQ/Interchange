using UnityEngine;
using UnityEngine.Splines;

public class ZoneObject : MonoBehaviour
{
    public ZoneMaterial zoneMaterial;
    public Zone Zone { get; set; }
    bool prev;
    Renderer meshRenderer;

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
        if (prev != Zone.Enabled)
            UpdateMaterial();
        prev = Zone.Enabled;
    }

    void UpdateMaterial()
    {
        if (!Zone.Enabled)
            meshRenderer.material = zoneMaterial.DisbaledMaterial;
        else
            meshRenderer.material = zoneMaterial.TargetMaterial;
    }
}