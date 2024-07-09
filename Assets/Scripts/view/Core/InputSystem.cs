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
    private bool parallelSpacingDragEnabled;
    private bool bulkSelectPerformed;
    private float3 bulkSelectStartPos;
    private SquareSelector squareSelector;
    [SerializeField]
    private SquareSelector squareSelectorPrefab;
    [SerializeField]
    ModeToggle modeToggle;

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
        gameActions.InGame.IncreaseElevation.performed += IncreaseElevation;
        gameActions.InGame.DecreaseElevation.performed += DecreaseElevation;
        gameActions.InGame.ParallelSpacingDrag.performed += EnableParallelSpacingDrag;
        gameActions.InGame.ParallelSpacingDrag.canceled += DisableParallelSpacingDrag;
        gameActions.InGame.DragCamera.performed += DragScreenStarted;
        gameActions.InGame.DragCamera.canceled += DragScreenCanceled;
        gameActions.InGame.ToggleParallelBuildMode.performed += ToggleParallelBuild;
        gameActions.InGame.ToggleBuildMode.performed += ToggleBuildMode;
        gameActions.InGame.BulkSelect.started += BulkSelectStarted;
        gameActions.InGame.BulkSelect.performed += BulkSelectPerformed;
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
        gameActions.InGame.IncreaseElevation.performed -= IncreaseElevation;
        gameActions.InGame.DecreaseElevation.performed -= DecreaseElevation;
        gameActions.InGame.ParallelSpacingDrag.performed -= EnableParallelSpacingDrag;
        gameActions.InGame.ParallelSpacingDrag.canceled -= DisableParallelSpacingDrag;
        gameActions.InGame.DragCamera.performed -= DragScreenStarted;
        gameActions.InGame.DragCamera.canceled -= DragScreenCanceled;
        gameActions.InGame.ToggleParallelBuildMode.performed -= ToggleParallelBuild;
        gameActions.InGame.ToggleBuildMode.performed -= ToggleBuildMode;
        gameActions.InGame.BulkSelect.started -= BulkSelectStarted;
        gameActions.InGame.BulkSelect.performed -= BulkSelectPerformed;
        gameActions.InGame.BulkSelect.canceled -= BulkSelectEnd;

        gameActions.InGame.Disable();
    }

    void Update()
    {
        UpdateCameraPos();
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
        mouseWorldPos.y = Build.Elevation;
        MouseWorldPos = mouseWorldPos;

        prevScreenMousePos.x = Input.mousePosition.x;
        prevScreenMousePos.y = Input.mousePosition.y;
    }

    void UpdateSquareSelector()
    {
        if (bulkSelectPerformed)
        {
            squareSelector.gameObject.SetActive(true);
            float width = Math.Abs(bulkSelectStartPos.x - MouseWorldPos.x);
            float height = Math.Abs(bulkSelectStartPos.z - MouseWorldPos.z);
            squareSelector.SetTransform(width, height, bulkSelectStartPos + (MouseWorldPos - bulkSelectStartPos) / 2);
        }
        else
            squareSelector.gameObject.SetActive(false);
    }

    void ProcessParallelSpacingDrag()
    {
        if (parallelSpacingDragEnabled)
        {
            float delta = Input.mousePosition.y - prevScreenMousePos.y;
            Build.ParallelSpacing += delta * 0.03f;
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
            Divide.HandleDivideCommand(Roads.HoveredRoad.Road, MouseWorldPos);
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
        bulkSelectPerformed = false;
    }
    void IncreaseElevation(InputAction.CallbackContext context)
    {
        Build.IncreaseElevation();
    }
    void DecreaseElevation(InputAction.CallbackContext context)
    {
        Build.DecreaseElevation();
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
        modeToggle.ToggleMode();
    }
    void BulkSelectStarted(InputAction.CallbackContext context)
    {
        bulkSelectStartPos = MouseWorldPos;
    }
    void BulkSelectPerformed(InputAction.CallbackContext context)
    {
        bulkSelectPerformed = true;
    }
    void BulkSelectEnd(InputAction.CallbackContext context)
    {
        if (bulkSelectPerformed)
            Roads.BulkSelect(bulkSelectStartPos, MouseWorldPos);
        bulkSelectPerformed = false;
    }
}