using UnityEngine;

public class ZoneObject : MonoBehaviour
{
    public ZoneMaterial zoneMaterial;
    public Zone Zone { get; set; }
    bool prev;

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
        Renderer renderer = GetComponent<Renderer>();
        if (!Zone.Enabled)
            renderer.material = zoneMaterial.DisbaledMaterial;
        else if (Zone is SourceZone)
            renderer.material = zoneMaterial.SourceMaterial;
        else if (Zone is TargetZone)
            renderer.material = zoneMaterial.TargetMaterial;
    }
}