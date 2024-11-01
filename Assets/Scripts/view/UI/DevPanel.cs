using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Model.Roads;
using UnityEngine;
using UnityEngine.UIElements;

public class DevPanel : MonoBehaviour
{
    private VisualElement root;
    private Toggle drawCenter;
    private Toggle drawLanes;
    private Toggle drawEdges;
    private Toggle drawOutline;
    private Toggle drawPx;
    private Toggle drawVertices;
    private Toggle ghostRoad;
    private Toggle supportLines;
    private Toggle continuousBuilding;
    private static TextElement elevation;
    private static TextElement carServiced;
    private static TextElement connectedness;
    private static TextElement debug1;
    private static TextElement debug2;
    private Button button1;
    private Button button2;
    private static readonly StringBuilder stringBuilder = new();
    void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        root.RegisterCallback<MouseEnterEvent>(MouseReturnGameWorld);
        root.RegisterCallback<MouseLeaveEvent>(MouseLeaveGameWorld);

        elevation = root.Q<TextElement>("Elevation");
        carServiced = root.Q<TextElement>("CarServiced");
        connectedness = root.Q<TextElement>("Connectedness");
        debug1 = root.Q<TextElement>("Debug1");
        debug2 = root.Q<TextElement>("Debug2");

        drawCenter = root.Q<Toggle>("RoadCenter");
        drawLanes = root.Q<Toggle>("RoadLanes");
        drawEdges = root.Q<Toggle>("RoadPaths");
        drawOutline = root.Q<Toggle>("RoadOutline");
        drawPx = root.Q<Toggle>("RoadPx");
        drawVertices = root.Q<Toggle>("RoadVertices");
        ghostRoad = root.Q<Toggle>("Ghost");
        supportLines = root.Q<Toggle>("SupportLines");
        continuousBuilding = root.Q<Toggle>("ContinuousBuilding");
        button1 = root.Q<Button>("DrawGizmos");
        button2 = root.Q<Button>("DeleteCars");

        drawCenter.RegisterCallback<ChangeEvent<bool>>(TogglecCenter);
        drawCenter.value = false;
        drawEdges.RegisterCallback<ChangeEvent<bool>>(ToggleEdge);
        drawEdges.value = true;
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
        continuousBuilding.RegisterCallback<ChangeEvent<bool>>(ToggleContinuousBuilding);
        continuousBuilding.value = true;
        button1.RegisterCallback((ClickEvent evt) => DrawGizmos.Draw());
        button2.RegisterCallback((ClickEvent evt) =>
        {
            List<uint> ids = Game.Cars.Keys.ToList();
            foreach (uint id in ids) Game.RemoveCar(Game.Cars[id]);
            foreach (Edge edge in Game.Edges.Values) {edge.IncomingCar = null; edge.Cars.Clear(); }
            foreach (Vertex vertex in Game.Vertices.Values) vertex.ScheduleCooldown = 0;
        });
    }

    void Update()
    {
        stringBuilder.Clear();
        stringBuilder.Append("Elevation: ");
        stringBuilder.Append(Build.Elevation);
        elevation.text = stringBuilder.ToString();

        stringBuilder.Clear();
        stringBuilder.Append("Cars Serviced: ");
        stringBuilder.Append(Game.CarServiced);
        carServiced.text = stringBuilder.ToString();
    }

    public static void SetDebug1Text(string s)
    {
        stringBuilder.Clear();
        stringBuilder.Append("Debug1: ");
        stringBuilder.Append(s);
        debug1.text = stringBuilder.ToString();
    }

    public static void SetDebug2Text(string s)
    {
        stringBuilder.Clear();
        stringBuilder.Append("Debug2: ");
        stringBuilder.Append(s);
        debug2.text = stringBuilder.ToString();
    }

    void OnDisable()
    {
        root.UnregisterCallback<MouseEnterEvent>(MouseReturnGameWorld);
        root.UnregisterCallback<MouseLeaveEvent>(MouseLeaveGameWorld);
        drawCenter.UnregisterCallback<ChangeEvent<bool>>(TogglecCenter);
        drawEdges.UnregisterCallback<ChangeEvent<bool>>(ToggleEdge);
        drawLanes.UnregisterCallback<ChangeEvent<bool>>(ToggleLanes);
        drawPx.UnregisterCallback<ChangeEvent<bool>>(TogglePx);
        drawOutline.UnregisterCallback<ChangeEvent<bool>>(ToggleOutline);
        drawVertices.UnregisterCallback<ChangeEvent<bool>>(ToggleVertices);
        supportLines.UnregisterCallback<ChangeEvent<bool>>(ToggleSupportLines);
        ghostRoad.UnregisterCallback<ChangeEvent<bool>>(ToggleGhost);
        continuousBuilding.UnregisterCallback<ChangeEvent<bool>>(ToggleContinuousBuilding);
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
    void ToggleEdge(ChangeEvent<bool> e)
    {
        DrawGizmos.DrawEdges = e.newValue;
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
    void ToggleContinuousBuilding(ChangeEvent<bool> e)
    {
        Build.ContinuousBuilding = e.newValue;
    }
}