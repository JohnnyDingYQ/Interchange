using System;
using UnityEngine;

public class Main : MonoBehaviour
{

    bool flip = false;
    readonly bool debugMode = false;
    [SerializeField]
    DevPanel devPanel;

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
        flip = !flip;
        if (flip)
        {
            Build.HandleHover(InputSystem.MouseWorldPos);
            Roads.UpdateHoveredRoad();
        }
    }

}
