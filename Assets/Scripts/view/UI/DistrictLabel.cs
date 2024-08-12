using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class DistrictLabel
{
    readonly TextElement textElement;
    float connectivity;

    public DistrictLabel(TextElement textElement)
    {
        this.textElement = textElement;
        textElement.text = 0.ToString() + "% Connected";
    }

    public void Set(float newConnectivity)
    {
        if (connectivity == newConnectivity)
            return;
        connectivity = newConnectivity;
        textElement.text = connectivity.ToString() + "% Connected";
    }

    public void ApplyWorldPos(float3 worldPos)
    {
        float2 v = RuntimePanelUtils.CameraTransformWorldToPanel(textElement.panel, worldPos, Camera.main);
        textElement.transform.position = new float3(v.x - textElement.resolvedStyle.width / 2, v.y - textElement.resolvedStyle.height / 2, 0);
    }
}