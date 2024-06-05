using System;
using UnityEngine;

public class Main : MonoBehaviour
{
    void Start()
    {
        Application.targetFrameRate = 60;
        Physics.queriesHitTriggers = false;
        int now = (int)DateTime.Now.Ticks;
        UnityEngine.Random.InitState(now); // different seed for each game session
        Debug.Log("Seed: " + now);
        // UnityEngine.Random.InitState(1439289702);
    }

    void FixedUpdate()
    {
        Build.HandleHover(InputSystem.MouseWorldPos);
    }

}
