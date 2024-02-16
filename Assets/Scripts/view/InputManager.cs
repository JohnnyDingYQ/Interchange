using System;
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
        if (Input.GetKey(KeyCode.Alpha1))
            Build1Lane.Invoke();
        if (Input.GetKey(KeyCode.Alpha2))
            Build2Lane.Invoke();
        if (Input.GetKey(KeyCode.Alpha3))
            Build3Lane.Invoke();
    }
}