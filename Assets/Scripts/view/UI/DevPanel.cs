using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

public class DevPanel : MonoBehaviour
{
    private VisualElement root;
    private Toggle drawCenter;
    private Toggle drawLanes;
    private Toggle drawPaths;
    private Toggle drawOutline;
    private Toggle drawPx;
    private Toggle drawVertices;
    private Toggle ghostRoad;
    private Toggle supportLines;
    private Toggle enforceTangent;
    private static TextElement elevation;
    private static TextElement carServiced;
    private readonly StringBuilder stringBuilder = new();
    void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        root.RegisterCallback<MouseEnterEvent>(MouseReturnGameWorld);
        root.RegisterCallback<MouseLeaveEvent>(MouseLeaveGameWorld);

        elevation = root.Q<TextElement>("Elevation");
        carServiced = root.Q<TextElement>("CarServiced");

        drawCenter = root.Q<Toggle>("RoadCenter");
        drawLanes = root.Q<Toggle>("RoadLanes");
        drawPaths = root.Q<Toggle>("RoadPaths");
        drawOutline = root.Q<Toggle>("RoadOutline");
        drawPx = root.Q<Toggle>("RoadPx");
        drawVertices = root.Q<Toggle>("RoadVertices");
        ghostRoad = root.Q<Toggle>("Ghost");
        supportLines = root.Q<Toggle>("SupportLines");
        enforceTangent = root.Q<Toggle>("EnforceTangent");

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
        supportLines.RegisterCallback<ChangeEvent<bool>>(ToggleSupportLines);
        supportLines.value = true;
        ghostRoad.RegisterCallback<ChangeEvent<bool>>(ToggleGhost);
        ghostRoad.value = true;
        enforceTangent.RegisterCallback<ChangeEvent<bool>>(EnforceTangent);
        enforceTangent.value = true;
    }

    void Update()
    {
        stringBuilder.Clear();
        stringBuilder.Append("Elevation: ");
        stringBuilder.Append(Build.Elevation);
        elevation.text = stringBuilder.ToString();

        stringBuilder.Clear();
        stringBuilder.Append("Cara Serviced: ");
        stringBuilder.Append(Game.CarServiced);
        carServiced.text = stringBuilder.ToString();
    }

    void OnDisable()
    {
        root.UnregisterCallback<MouseEnterEvent>(MouseReturnGameWorld);
        root.UnregisterCallback<MouseLeaveEvent>(MouseLeaveGameWorld);
        drawCenter.UnregisterCallback<ChangeEvent<bool>>(TogglecCenter);
        drawPaths.UnregisterCallback<ChangeEvent<bool>>(TogglePath);
        drawLanes.UnregisterCallback<ChangeEvent<bool>>(ToggleLanes);
        drawPx.UnregisterCallback<ChangeEvent<bool>>(TogglePx);
        drawOutline.UnregisterCallback<ChangeEvent<bool>>(ToggleOutline);
        drawVertices.UnregisterCallback<ChangeEvent<bool>>(ToggleVertices);
        supportLines.UnregisterCallback<ChangeEvent<bool>>(ToggleSupportLines);
        ghostRoad.UnregisterCallback<ChangeEvent<bool>>(ToggleGhost);
        enforceTangent.UnregisterCallback<ChangeEvent<bool>>(EnforceTangent);
    }

    void MouseReturnGameWorld(MouseEnterEvent e)
    {
        InputSystem.MouseIsInGameWorld = false;
    }
    void MouseLeaveGameWorld(MouseLeaveEvent e)
    {
        InputSystem.MouseIsInGameWorld = true;
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
    void ToggleGhost(ChangeEvent<bool> e)
    {
        Build.BuildsGhostRoad = e.newValue;
    }
    void ToggleSupportLines(ChangeEvent<bool> e)
    {
        DrawGizmos.DrawSupportLines = e.newValue;
    }
    void EnforceTangent(ChangeEvent<bool> e)
    {
        Build.EnforcesTangent = e.newValue;
    }
}