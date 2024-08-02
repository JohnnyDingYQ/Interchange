using System;
using UnityEngine;
using UnityEngine.Assertions;

public class Main : MonoBehaviour
{
    readonly bool debugMode = true;
    [SerializeField]
    DevPanel devPanel;
    [SerializeField]
    Roads roads;
    [SerializeField]
    Intersections intersections;
    uint frameElapsed = 0;

    void Start()
    {
        Application.targetFrameRate = 165;
        Physics.queriesHitTriggers = false;
        int now = (int)DateTime.Now.Ticks;
        UnityEngine.Random.InitState(now); // different seed for each game session
        Debug.Log("Seed: " + now);
        // UnityEngine.Random.InitState(1439289702);

        devPanel.gameObject.SetActive(debugMode);
    }

    void Update()
    {
        if (frameElapsed % 2 == 0)
        {
            Roads.UpdateHoveredRoad();
            Zones.UpdateHoveredZone();
            Build.HandleHover(InputSystem.MouseWorldPos);
        }
        frameElapsed++;
    }

    public static float GetHUDObjectHeight(HUDLayer layer)
    {
        return Constants.MaxElevation + ((int)layer + 1) * 0.01f;
    }

    public void ComplyToGameSave()
    {
        roads.DestoryAll();
        intersections.DestoryAll();
        foreach (Road road in Game.Roads.Values)
            Game.InvokeRoadAdded(road);
        foreach (Intersection ix in Game.Intersections.Values)
        {
            Game.InvokeIntersectionAdded(ix);
            Game.UpdateIntersectionRoads(ix);
        }

    }
}
