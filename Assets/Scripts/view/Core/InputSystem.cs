using System;
using System.Collections.Generic;
using System.Linq;
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
    private bool bulkSelectStarted;
    private float3 bulkSelectStart;
    private SquareSelector squareSelector;
    [SerializeField]
    private SquareSelector squareSelectorPrefab;

    void Awake()
    {
        gameActions = new();
    }
    void Start()
    {
        MouseIsInGameWorld = true;
        MouseWorldPos = new(0, 0, 0);
        squareSelector = Instantiate(squareSelectorPrefab, transform);
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
        gameActions.InGame.AbandonBuild.performed += Deselect;
        gameActions.InGame.ElevationDrag.performed += EnableElevationDrag;
        gameActions.InGame.ElevationDrag.canceled += DisableElevationDrag;
        gameActions.InGame.ParallelSpacingDrag.performed += EnableParallelSpacingDrag;
        gameActions.InGame.ParallelSpacingDrag.canceled += DisableParallelSpacingDrag;
        gameActions.InGame.DragCamera.performed += DragScreenStarted;
        gameActions.InGame.DragCamera.canceled += DragScreenCanceled;
        gameActions.InGame.ToggleParallelBuildMode.performed += ToggleParallelBuild;
        gameActions.InGame.ToggleBuildMode.performed += ToggleBuildMode;
        gameActions.InGame.BulkSelect.performed += BulkSelectStart;
        gameActions.InGame.BulkSelect.canceled += BulkSelectEnd;

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
        gameActions.InGame.AbandonBuild.performed -= Deselect;
        gameActions.InGame.ElevationDrag.performed -= EnableElevationDrag;
        gameActions.InGame.ElevationDrag.canceled -= DisableElevationDrag;
        gameActions.InGame.ParallelSpacingDrag.performed -= EnableParallelSpacingDrag;
        gameActions.InGame.ParallelSpacingDrag.canceled -= DisableParallelSpacingDrag;
        gameActions.InGame.DragCamera.performed -= DragScreenStarted;
        gameActions.InGame.DragCamera.canceled -= DragScreenCanceled;
        gameActions.InGame.ToggleParallelBuildMode.performed -= ToggleParallelBuild;
        gameActions.InGame.ToggleBuildMode.performed -= ToggleBuildMode;
        gameActions.InGame.BulkSelect.performed -= BulkSelectStart;
        gameActions.InGame.BulkSelect.canceled -= BulkSelectEnd;

        gameActions.InGame.Disable();
    }

    void Update()
    {
        UpdateCameraPos();
        ProcessElevationDrag();
        ProcessParallelSpacingDrag();
        UpdateSquareSelector();
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

    void UpdateSquareSelector()
    {
        if (bulkSelectStarted)
        {
            squareSelector.gameObject.SetActive(true);
            float width = Math.Abs(bulkSelectStart.x - MouseWorldPos.x);
            float height = Math.Abs(bulkSelectStart.z - MouseWorldPos.z);
            squareSelector.SetTransform(width, height, bulkSelectStart + (MouseWorldPos - bulkSelectStart) / 2);
        }
        else
            squareSelector.gameObject.SetActive(false);
    }
    void ProcessElevationDrag()
    {
        if (elevationDragEnabled)
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
        if (Roads.HoveredRoad != null)
            DivideHandler.HandleDivideCommand(Roads.HoveredRoad.Road, MouseWorldPos);
    }
    void RemoveRoad(InputAction.CallbackContext context)
    {
        if (Roads.HoveredRoad != null)
            Game.RemoveRoad(Roads.HoveredRoad.Road);
        foreach (Road road in Roads.SelectedRoads.Select(r => r.Road))
            Game.RemoveRoad(road);
        Roads.ClearSelected();
    }
    void Deselect(InputAction.CallbackContext context)
    {
        Build.ResetSelection();
        Roads.ClearSelected();
        bulkSelectStarted = false;
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
    void BulkSelectStart(InputAction.CallbackContext context)
    {
        bulkSelectStart = MouseWorldPos;
        bulkSelectStarted = true;
    }
    void BulkSelectEnd(InputAction.CallbackContext context)
    {
        if (bulkSelectStarted)
            Roads.BulkSelect(bulkSelectStart, MouseWorldPos);
        bulkSelectStarted = false;
    }
}