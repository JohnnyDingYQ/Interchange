using UnityEngine;
using UnityEngine.UIElements;

public class UI : MonoBehaviour
{
    private VisualElement root;
    private Toggle drawCenter;
    private Toggle drawLanes;
    private Toggle drawPaths;
    private Toggle drawOutline;
    private Toggle drawPx;
    private Toggle drawVertices;
    void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        root.RegisterCallback<MouseEnterEvent>(DisableGameWorldClick);
        root.RegisterCallback<MouseLeaveEvent>(EnableGameWorldClick);

        drawCenter = root.Q<Toggle>("RoadCenter");
        drawLanes = root.Q<Toggle>("RoadLanes");
        drawPaths = root.Q<Toggle>("RoadPaths");
        drawOutline = root.Q<Toggle>("RoadOutline");
        drawPx = root.Q<Toggle>("RoadPx");
        drawVertices = root.Q<Toggle>("RoadVertices");
        
        drawCenter.RegisterCallback<ChangeEvent<bool>>(TogglecCenter);
        drawCenter.value = false;
        drawPaths.RegisterCallback<ChangeEvent<bool>>(TogglePath);
        drawPaths.value = true;
        drawLanes.RegisterCallback<ChangeEvent<bool>>(ToggleLanes);
        drawLanes.value = true;
        drawPx.RegisterCallback<ChangeEvent<bool>>(TogglePx);
        drawPx.value = true;
        drawOutline.RegisterCallback<ChangeEvent<bool>>(ToggleOutline);
        drawOutline.value = true;
        drawVertices.RegisterCallback<ChangeEvent<bool>>(ToggleVertices);
        drawVertices.value = false;
    }

    void OnDisable()
    {
        root.UnregisterCallback<MouseEnterEvent>(DisableGameWorldClick);
        root.UnregisterCallback<MouseLeaveEvent>(EnableGameWorldClick);
        drawCenter.UnregisterCallback<ChangeEvent<bool>>(TogglecCenter);
        drawPaths.UnregisterCallback<ChangeEvent<bool>>(TogglePath);
        drawLanes.UnregisterCallback<ChangeEvent<bool>>(ToggleLanes);
        drawPx.UnregisterCallback<ChangeEvent<bool>>(TogglePx);
        drawOutline.UnregisterCallback<ChangeEvent<bool>>(ToggleOutline);
        drawVertices.UnregisterCallback<ChangeEvent<bool>>(ToggleVertices);
    }

    void DisableGameWorldClick(MouseEnterEvent e)
    {
        InputSystem.MouseInGameWorld = false;
    }

    void EnableGameWorldClick(MouseLeaveEvent e)
    {
        InputSystem.MouseInGameWorld = true;
    }
    
    void TogglecCenter(ChangeEvent<bool> e)
    {
        DrawGizmos.DrawCenter = e.newValue;
    }

    void TogglePath(ChangeEvent<bool> e)
    {
        DrawGizmos.DrawPaths = e.newValue;
    }

    void ToggleLanes(ChangeEvent<bool> e)
    {
        DrawGizmos.DrawLanes = e.newValue;
    }

    void TogglePx(ChangeEvent<bool> e)
    {
        DrawGizmos.DrawPx = e.newValue;
    }

    void ToggleOutline(ChangeEvent<bool> e)
    {
        DrawGizmos.DrawOutline = e.newValue;
    }

    void ToggleVertices(ChangeEvent<bool> e)
    {
        DrawGizmos.DrawVertices = e.newValue;
    }
}