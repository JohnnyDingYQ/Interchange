using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Model.Roads;
using UnityEngine;
using UnityEngine.UIElements;

public class DevPanel : MonoBehaviour
{
    VisualElement root;
    Toggle drawCenter, drawLanes, drawEdges, drawOutline, drawPx, drawVertices, supportLines;
    static TextElement elevation, debug1, debug2;
    Button button1, button2;
    static readonly StringBuilder stringBuilder = new();
    void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        root.RegisterCallback<MouseEnterEvent>(MouseReturnGameWorld);
        root.RegisterCallback<MouseLeaveEvent>(MouseLeaveGameWorld);

        elevation = root.Q<TextElement>("Elevation");
        debug1 = root.Q<TextElement>("Debug1");
        debug2 = root.Q<TextElement>("Debug2");

        drawCenter = root.Q<Toggle>("RoadCenter");
        drawLanes = root.Q<Toggle>("RoadLanes");
        drawEdges = root.Q<Toggle>("RoadPaths");
        drawOutline = root.Q<Toggle>("RoadOutline");
        drawPx = root.Q<Toggle>("RoadPx");
        drawVertices = root.Q<Toggle>("RoadVertices");
        supportLines = root.Q<Toggle>("SupportLines");
        button1 = root.Q<Button>("DrawGizmos");
        button2 = root.Q<Button>("DeleteCars");

        drawCenter.RegisterCallback<ChangeEvent<bool>>(ToggleCenter);
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
        button1.RegisterCallback((ClickEvent evt) => DrawGizmos.Draw());
        button2.RegisterCallback((ClickEvent evt) =>
        {
            List<uint> ids = Game.Cars.Keys.ToList();
            foreach (uint id in ids)
                Game.RemoveCar(Game.Cars[id]);
            foreach (Edge edge in Game.Edges.Values)
            {
                edge.IncomingCar = null;
                edge.Cars.Clear();
            }
            foreach (Vertex vertex in Game.Vertices.Values)
                vertex.ScheduleCooldown = 0;
        });
    }

    void Update()
    {
        stringBuilder.Clear();
        stringBuilder.Append("Elevation: ");
        stringBuilder.Append(Build.Elevation);
        elevation.text = stringBuilder.ToString();
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

    void MouseReturnGameWorld(MouseEnterEvent e)
    {
        InputSystem.MouseIsInGameWorld = false;
    }
    void MouseLeaveGameWorld(MouseLeaveEvent e)
    {
        InputSystem.MouseIsInGameWorld = true;
    }
    void ToggleCenter(ChangeEvent<bool> e)
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
    void ToggleSupportLines(ChangeEvent<bool> e)
    {
        DrawGizmos.DrawSupportLines = e.newValue;
    }
}