using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Main : MonoBehaviour
{
    readonly bool debugMode = false;
    [SerializeField]
    DevPanel devPanel;
    [SerializeField]
    Roads roads;
    [SerializeField]
    Intersections intersections;
    [SerializeField]
    Cars cars;
    [SerializeField]
    ModeToggle modeToggle;
    [SerializeField]
    Districts zones;
    uint frameElapsed = 0;

    void Start()
    {
        Application.targetFrameRate = 165;
        Physics.queriesHitTriggers = false;
        int now = (int)DateTime.Now.Ticks;
        Debug.Log("Seed: " + now);
        // UnityEngine.Random.InitState(1439289702);

        Game.Districts[1].Enable();
        devPanel.gameObject.SetActive(debugMode);
    }

    void Update()
    {
        if (!Game.BuildModeOn)
            CarScheduler.Schedule(Time.deltaTime);
        if (frameElapsed % 2 == 0)
        {
            Hover.UpdateHovered();
            Build.HandleHover(InputSystem.MouseWorldPos);
        }
        frameElapsed++;

        DevPanel.SetDebug1Text(Game.Districts[1].Connectedness.ToString());
        DevPanel.SetDebug2Text(Game.Districts[2].Connectedness.ToString());

    }

    public static float GetHUDObjectHeight(HUDLayer layer)
    {
        return Constants.MaxElevation + ((int)layer + 1) * 0.01f;
    }

    public void ComplyToGameSave()
    {
        roads.DestoryAll();
        intersections.DestoryAll();
        cars.DestoryAll();
        foreach (Road road in Game.Roads.Values)
            Game.InvokeRoadAdded(road);
        foreach (Intersection ix in Game.Intersections.Values)
            Game.InvokeIntersectionAdded(ix);
        foreach (Intersection ix in Game.Intersections.Values)
            Game.UpdateIntersection(ix);

        CarScheduler.FindNewConnection();

        foreach (Car car in Game.Cars.Values)
            Game.InvokeCarAdded(car);

        modeToggle.SwitchToBuildMode();
        zones.UpdateZoneObjectReferences();
    }
}
