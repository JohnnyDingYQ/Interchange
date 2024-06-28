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
    private bool isDraggingCamera;
    private bool elevationDragEnabled;
    private bool parallelSpacingDragEnabled;

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
        gameActions.InGame.ElevationDrag.performed += EnableElevationDrag;
        gameActions.InGame.ElevationDrag.canceled += DisableElevationDrag;
        gameActions.InGame.ParallelSpacingDrag.performed += EnableParallelSpacingDrag;
        gameActions.InGame.ParallelSpacingDrag.canceled += DisableParallelSpacingDrag;
        gameActions.InGame.DragCamera.performed += DragScreenStarted;
        gameActions.InGame.DragCamera.canceled += DragScreenCanceled;
        gameActions.InGame.ToggleParallelBuildMode.performed += ToggleParallelBuild;
        gameActions.InGame.ToggleBuildMode.performed += ToggleBuildMode;

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
        gameActions.InGame.ElevationDrag.performed -= EnableElevationDrag;
        gameActions.InGame.ElevationDrag.canceled -= DisableElevationDrag;
        gameActions.InGame.ParallelSpacingDrag.performed -= EnableParallelSpacingDrag;
        gameActions.InGame.ParallelSpacingDrag.canceled -= DisableParallelSpacingDrag;
        gameActions.InGame.DragCamera.performed -= DragScreenStarted;
        gameActions.InGame.DragCamera.canceled -= DragScreenCanceled;
        gameActions.InGame.ToggleParallelBuildMode.performed -= ToggleParallelBuild;
        gameActions.InGame.ToggleBuildMode.performed -= ToggleBuildMode;

        gameActions.InGame.Disable();
    }

    void Update()
    {
        UpdateCameraPos();
        ProcessElevationDrag();
        ProcessParallelSpacingDrag();
        UpdateMouseWorldPos();
    }

    void UpdateCameraPos()
    {
        Vector3 cameraOffset = gameActions.InGame.MoveCamera.ReadValue<Vector3>();
        if (isDraggingCamera)
        {
            cameraOffset.x += (prevScreenMousePos.x - Input.mousePosition.x) * MouseDragScreenMultiplier;
            cameraOffset.z += (prevScreenMousePos.y - Input.mousePosition.y) * MouseDragScreenMultiplier;
        }
        CameraControl.CameraOffset = cameraOffset;
    }

    void UpdateMouseWorldPos()
    {
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

    void ProcessElevationDrag()
    {
        if(elevationDragEnabled)
        {
            float delta = Input.mousePosition.y - prevScreenMousePos.y;
            Game.SetElevation(Game.Elevation + delta * 0.02f);
        }
    }

    void ProcessParallelSpacingDrag()
    {
        if (parallelSpacingDragEnabled)
        {
            float delta = Input.mousePosition.y - prevScreenMousePos.y;
            Build.ParallelSpacing += delta * 0.03f;
            Debug.Log(Build.ParallelSpacing);
        }
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
    void EnableElevationDrag(InputAction.CallbackContext context)
    {
        elevationDragEnabled = true;
    }
    void DisableElevationDrag(InputAction.CallbackContext context)
    {
        elevationDragEnabled = false;
    }
    void EnableParallelSpacingDrag(InputAction.CallbackContext context)
    {
        parallelSpacingDragEnabled = true;
    }
    void DisableParallelSpacingDrag(InputAction.CallbackContext context)
    {
        parallelSpacingDragEnabled = false;
    }
    void DragScreenStarted(InputAction.CallbackContext context)
    {
        isDraggingCamera = true;
    }
    void DragScreenCanceled(InputAction.CallbackContext context)
    {
        isDraggingCamera = false;
    }
    void ToggleParallelBuild(InputAction.CallbackContext context)
    {
        Build.ToggletParallelBuild();
    }
    void ToggleBuildMode(InputAction.CallbackContext context)
    {
        ModeToggle.ToggleMode();
    }
}