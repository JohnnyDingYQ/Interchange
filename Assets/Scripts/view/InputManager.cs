using System;
using Unity.Mathematics;
using UnityEngine;

public class InputManager: MonoBehaviour
{
    public event Action BuildRoad;
    public event Action MoveCameraLeft;
    public event Action MoveCameraRight;
    public event Action MoveCameraUp;
    public event Action MoveCameraDown;
    public event Action Build1Lane;
    public event Action Build2Lane;
    public event Action Build3Lane;
    public event Action SaveGame;
    public event Action LoadGame;
    public event Action DividRoad;
    public event Action ShowPaths;
    public event Action ShowRoadAndLanes;
    public static float3 MouseWorldPos;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            BuildRoad.Invoke();
        if (Input.GetKey(KeyCode.W))
            MoveCameraUp.Invoke();
        if (Input.GetKey(KeyCode.A))
            MoveCameraLeft.Invoke();
        if (Input.GetKey(KeyCode.S))
            MoveCameraDown.Invoke();
        if (Input.GetKey(KeyCode.D))
            MoveCameraRight.Invoke();
        if (Input.GetKeyDown(KeyCode.Alpha1))
            Build1Lane.Invoke();
        if (Input.GetKeyDown(KeyCode.Alpha2))
            Build2Lane.Invoke();
        if (Input.GetKeyDown(KeyCode.Alpha3))
            Build3Lane.Invoke();
        if (Input.GetKeyDown(KeyCode.O))
            SaveGame.Invoke();
        if (Input.GetKeyDown(KeyCode.P))
            LoadGame.Invoke();
        if (Input.GetMouseButtonDown(1))
            DividRoad.Invoke();
        if (Input.GetKeyDown(KeyCode.Keypad1))
            ShowRoadAndLanes.Invoke();
        if (Input.GetKeyDown(KeyCode.Keypad2))
            ShowPaths.Invoke();
        
        MouseWorldPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.z)
        {
            z = Camera.main.transform.position.y -0
        };
        MouseWorldPos = Camera.main.ScreenToWorldPoint(MouseWorldPos);
    }
}