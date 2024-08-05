using System;
using System.Collections.Generic;
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
    [SerializeField]
    CarDriver carDriver;
    [SerializeField]
    ModeToggle modeToggle;
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
        if (!Game.BuildModeOn)
            CarScheduler.Schedule(Time.deltaTime);
        if (frameElapsed % 2 == 0)
        {
            Roads.UpdateHoveredRoad();
            Zones.UpdateHoveredZoneAndDistrict();
            Build.HandleHover(InputSystem.MouseWorldPos);
        }
        frameElapsed++;
        
        if (Game.Districts.Count == 2)
        {
            DevPanel.SetDebug1Text(Game.Districts[1].Connectedness.ToString());
            DevPanel.SetDebug2Text(Game.Districts[2].Connectedness.ToString());
        }
    }

    public static float GetHUDObjectHeight(HUDLayer layer)
    {
        return Constants.MaxElevation + ((int)layer + 1) * 0.01f;
    }

    public void ComplyToGameSave()
    {
        roads.DestoryAll();
        intersections.DestoryAll();
        carDriver.DestoryAll();
        foreach (Road road in Game.Roads.Values)
            Game.InvokeRoadAdded(road);
        foreach (Intersection ix in Game.Intersections.Values)
        {
            Game.InvokeIntersectionAdded(ix);
            Game.UpdateIntersectionRoads(ix);
        }
        CarScheduler.DetermineZoneConnectedness();
        modeToggle.SwitchToBuildMode();
    }
}
