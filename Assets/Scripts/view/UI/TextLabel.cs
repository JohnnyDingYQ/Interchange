using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class TextLabel : MonoBehaviour
{
    private VisualElement root;
    private Label label;
    void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        label = root.Q<Label>();
    }

    void LateUpdate()
    {
        
    }

    public void ApplyWorldPos(float3 worldPos)
    {
        float2 v = RuntimePanelUtils.CameraTransformWorldToPanel(root.panel, worldPos, Camera.main);
        root.transform.position = new float3(v.x, v.y, 0);
    }

    public void SetText(string s)
    {
        label.text = s;
    }
}