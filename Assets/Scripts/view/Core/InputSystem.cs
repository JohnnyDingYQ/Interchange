using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystem : MonoBehaviour
{
    public static bool MouseIsInGameWorld { get; set; }
    private GameActions gameActions;
    public static float3 MouseWorldPos { get; set; }
    private float2 prevScreenMousePos;
    const float MouseDragScreenMultiplier = 0.17f;

    void Awake()
    {
        gameActions = new();
    }
    void Start()
    {
        MouseIsInGameWorld = true;
        MouseWorldPos = new(0, 0, 0);
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
        gameActions.InGame.RemoveRoad.performed += RemoveRoad;
        gameActions.InGame.AbandonBuild.performed += AbandonBuild;
        gameActions.InGame.Elevate.performed += Elevate;
        gameActions.InGame.Lower.performed += Lower;

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
        gameActions.InGame.RemoveRoad.performed -= RemoveRoad;
        gameActions.InGame.AbandonBuild.performed -= AbandonBuild;
        gameActions.InGame.Elevate.performed -= Elevate;
        gameActions.InGame.Lower.performed -= Lower;

        gameActions.InGame.Disable();
    }

    void Update()
    {
        UpdateCameraPos();

        float3 mouseWorldPos = new()
        {
            x = Input.mousePosition.x,
            y = Input.mousePosition.y,
            z = Camera.main.transform.position.y
        };
        mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseWorldPos);
        mouseWorldPos.y = Game.Elevation;
        MouseWorldPos = mouseWorldPos;

        prevScreenMousePos.x = Input.mousePosition.x;
        prevScreenMousePos.y = Input.mousePosition.y;
    }

    void UpdateCameraPos()
    {
        Vector3 cameraOffset = gameActions.InGame.MoveCamera.ReadValue<Vector3>();
        if (Input.GetMouseButton(1))
        {
            cameraOffset.x += (prevScreenMousePos.x - Input.mousePosition.x) * MouseDragScreenMultiplier;
            cameraOffset.z += (prevScreenMousePos.y - Input.mousePosition.y) * MouseDragScreenMultiplier;
        }
        CameraControl.CameraOffset = cameraOffset;
    }

    void OnBuild(InputAction.CallbackContext context)
    {
        if (MouseIsInGameWorld)
        {
            Build.HandleBuildCommand(MouseWorldPos);
        }
    }
    void OnSetLaneWidthTo1(InputAction.CallbackContext context)
    {
        Build.LaneCount = 1;
    }
    void OnSetLaneWidthTo2(InputAction.CallbackContext context)
    {
        Build.LaneCount = 2;
    }
    void OnSetLaneWidthTo3(InputAction.CallbackContext context)
    {
        Build.LaneCount = 3;
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
    void RemoveRoad(InputAction.CallbackContext context)
    {
        Game.RemoveSelectedRoad();
    }
    void AbandonBuild(InputAction.CallbackContext context)
    {
        Build.ResetSelection();
    }
    void Elevate(InputAction.CallbackContext context)
    {
        Game.SetElevation(Game.Elevation + 2);
    }
    void Lower(InputAction.CallbackContext context)
    {
        Game.SetElevation(Game.Elevation - 2);
    }
}