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
    Cars cars;
    [SerializeField]
    GameSettings gameSettings;
    uint frameElapsed = 0;

    void Start()
    {
        Application.targetFrameRate = 165;
        Physics.queriesHitTriggers = false;
        int now = (int)DateTime.Now.Ticks;
        Debug.Log("Seed: " + now);
        // UnityEngine.Random.InitState(1439289702);

        devPanel.gameObject.SetActive(debugMode);
        Build.DisplaysGhost = gameSettings.displaysGhost;
        Build.ContinuousBuilding = gameSettings.continuousBuild;
    }

    void Update()
    {
        if (frameElapsed % 2 == 0)
        {
            Hover.UpdateHovered();
            Build.HandleHover(InputSystem.MouseWorldPos);
        }
        frameElapsed++;

        DevPanel.SetDebug1Text(Game.Cars.Count.ToString() + "Cars");
        // DevPanel.SetDebug2Text(Game.Cars.);

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

        foreach (Car car in Game.Cars.Values)
            Game.InvokeCarAdded(car);
    }
}
