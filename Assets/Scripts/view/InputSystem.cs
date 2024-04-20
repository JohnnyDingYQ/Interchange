using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystem : MonoBehaviour
{
    public static bool MouseInGameWorld { get; set; }
    private GameActions gameActions;

    private const int CameraSpeedMultiplier = 25;
    private const float CameraZoomMultiplier = 0.3f;
    public float3 MouseWorldPos { get; set; }

    void Awake()
    {
        gameActions = new();
        MouseInGameWorld = true;
    }

    void OnEnable()
    {
        gameActions.InGame.Build.performed += OnBuild;
        gameActions.InGame.SetLaneWidthTo1.performed += OnSetLaneWidthTo1;
        gameActions.InGame.SetLaneWidthTo2.performed += OnSetLaneWidthTo2;
        gameActions.InGame.SetLaneWidthTo3.performed += OnSetLaneWidthTo3;
        gameActions.InGame.SaveGame.performed += SaveGame;
        gameActions.InGame.LoadGame.performed += LoadGame;
        gameActions.InGame.DivideRoad.performed += DivideRoad;
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
        gameActions.InGame.DivideRoad.performed -= DivideRoad;
        gameActions.InGame.Disable();
    }

    void Update()
    {
        Vector3 cameraOffset = gameActions.InGame.MoveCamera.ReadValue<Vector3>();
        cameraOffset.y *= CameraZoomMultiplier;
        Camera.main.transform.position += CameraSpeedMultiplier * Time.deltaTime * cameraOffset;

        float3 mouseWorldPos = new(Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.z)
        {
            z = Camera.main.transform.position.y - 0
        };
        MouseWorldPos = Camera.main.ScreenToWorldPoint(mouseWorldPos);
    }

    void OnBuild(InputAction.CallbackContext context)
    {
        if (MouseInGameWorld)
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
    void DivideRoad(InputAction.CallbackContext context)
    {
        Game.DivideSelectedRoad(MouseWorldPos);
    }
}