using System;
using UnityEngine;

public class Main : MonoBehaviour
{
    void Start()
    {
        Application.targetFrameRate = 60;
        Physics.queriesHitTriggers = false;
        UnityEngine.Random.InitState((int)DateTime.Now.Ticks); // different seed for each game session
    }

    void FixedUpdate()
    {
        Build.HandleHover(InputSystem.MouseWorldPos);
    }

    void Update()
    {
        Game.PassTime(Time.deltaTime);
    }

}
