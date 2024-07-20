using System;
using UnityEngine;
using UnityEngine.Assertions;

public class Main : MonoBehaviour
{

    bool flip = false;
    readonly bool debugMode = true;
    [SerializeField]
    DevPanel devPanel;

    void Start()
    {
        SanityCheck();
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
        flip = !flip;
        if (flip)
        {
            Build.HandleHover(InputSystem.MouseWorldPos);
            Roads.UpdateHoveredRoad();
        }
    }

    void SanityCheck()
    {
        Assert.IsTrue(Constants.MinLaneLength * 2 < Constants.MaxLaneLength);
        Assert.IsTrue(Constants.MinElevation < Constants.MaxElevation);
    }

    public static float GetHUDObjectHeight(HUDLayer layer)
    {
        return Constants.MaxElevation + ((int) layer + 1) * (float) (Constants.MaxElevation - Constants.MinElevation) * 0.01f;
    }
}
