using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystem : MonoBehaviour
{

    private GameActions gameActions;

    private const int CameraSpeedMultiplier = 25;
    private const float CameraZoomMultiplier = 0.3f;
    public event Action ShowPaths;
    public event Action ShowRoadAndLanes;
    public float3 MouseWorldPos { get; set; }

    void Awake()
    {
        gameActions = new();
    }

    void OnEnable()
    {
        gameActions.InGame.Build.performed += OnBuild;
        gameActions.InGame.SetLaneWidthTo1.performed += OnSetLaneWidthTo1;
        gameActions.InGame.SetLaneWidthTo2.performed += OnSetLaneWidthTo2;
        gameActions.InGame.SetLaneWidthTo3.performed += OnSetLaneWidthTo3;
        gameActions.InGame.SaveGame.performed += SaveGame;
        gameActions.InGame.LoadGame.performed += LoadGame;
        gameActions.InGame.Enable();
    }

    void OnDisable()
    {
        gameActions.InGame.Build.performed -= OnBuild;
        gameActions.InGame.SetLaneWidthTo1.performed -= OnSetLaneWidthTo1;
        gameActions.InGame.SetLaneWidthTo2.performed -= OnSetLaneWidthTo2;
        gameActions.InGame.SetLaneWidthTo3.performed -= OnSetLaneWidthTo3;
        gameActions.InGame.SaveGame.performed -= SaveGame;
        gameActions.InGame.LoadGame.performed -= LoadGame;
        gameActions.InGame.Disable();
    }

    void Update()
    {
        Vector3 cameraOffset = gameActions.InGame.MoveCamera.ReadValue<Vector3>();
        cameraOffset.y *= CameraZoomMultiplier;
        Camera.main.transform.position += CameraSpeedMultiplier * Time.deltaTime * cameraOffset;

        if (Input.GetKeyDown(KeyCode.Keypad1))
            ShowRoadAndLanes.Invoke();
        if (Input.GetKeyDown(KeyCode.Keypad2))
            ShowPaths.Invoke();

        MouseWorldPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.z)
        {
            z = Camera.main.transform.position.y - 0
        };
        MouseWorldPos = Camera.main.ScreenToWorldPoint(MouseWorldPos);
        Debug.Log(MouseWorldPos);
    }

    void OnBuild(InputAction.CallbackContext context)
    {
        BuildHandler.HandleBuildCommand(MouseWorldPos);
    }
    void OnSetLaneWidthTo1(InputAction.CallbackContext context)
    {
        BuildHandler.LaneCount = 1;
    }
    void OnSetLaneWidthTo2(InputAction.CallbackContext context)
    {
        BuildHandler.LaneCount = 2;
    }
    void OnSetLaneWidthTo3(InputAction.CallbackContext context)
    {
        BuildHandler.LaneCount = 3;
    }
    void SaveGame(InputAction.CallbackContext context)
    {
        SaveSystem.SaveGame();
    }
    void LoadGame(InputAction.CallbackContext context)
    {
        SaveSystem.LoadGame();
    }
}