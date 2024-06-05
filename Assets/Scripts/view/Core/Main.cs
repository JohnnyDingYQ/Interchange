using System;
using UnityEngine;

public class Main : MonoBehaviour
{
    void Start()
    {
        Application.targetFrameRate = 60;
        Physics.queriesHitTriggers = false;
        // UnityEngine.Random.InitState((int)DateTime.Now.Ticks); // different seed for each game session
        // Debug.Log((int)DateTime.Now.Ticks);
        UnityEngine.Random.InitState(1631369204);
    }

    void FixedUpdate()
    {
        Build.HandleHover(InputSystem.MouseWorldPos);
    }

}
