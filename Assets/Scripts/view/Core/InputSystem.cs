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
    [SerializeField]
    Main main;
    [SerializeField]
    Roads roads;

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
        gameActions.InGame.AbandonBuild.performed += Deselect;
        gameActions.InGame.IncreaseElevation.performed += IncreaseElevation;
        gameActions.InGame.DecreaseElevation.performed += DecreaseElevation;
        gameActions.InGame.ParallelSpacingDrag.performed += EnableParallelSpacingDrag;
        gameActions.InGame.ParallelSpacingDrag.canceled += DisableParallelSpacingDrag;
        gameActions.InGame.DragCamera.performed += DragScreenStarted;
        gameActions.InGame.DragCamera.canceled += DragScreenCanceled;
        gameActions.InGame.ToggleParallelBuildMode.performed += ToggleParallelBuild;
        gameActions.InGame.BulkSelect.started += BulkSelectStarted;
        gameActions.InGame.BulkSelect.performed += BulkSelectPerformed;
        gameActions.InGame.BulkSelect.canceled += BulkSelectEnd;
        gameActions.InGame.StraightMode.performed += EnableStraightMode;
        gameActions.InGame.LevelEditorSelect.performed += LevelEditorSelectEnabled;

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
        gameActions.InGame.BulkSelect.started -= BulkSelectStarted;
        gameActions.InGame.BulkSelect.performed -= BulkSelectPerformed;
        gameActions.InGame.BulkSelect.canceled -= BulkSelectEnd;
        gameActions.InGame.StraightMode.performed -= EnableStraightMode;
        gameActions.InGame.LevelEditorSelect.performed -= LevelEditorSelectEnabled;


        gameActions.InGame.Disable();
    }

    void Update()
    {
        UpdateCameraPos();
        ProcessParallelSpacingDrag();
        UpdateMouseWorldPos();
    }

    void UpdateCameraPos()
    {
        Vector3 cameraOffset = gameActions.InGame.MoveCamera.ReadValue<Vector3>();
        
        if (isDraggingCamera)
        {
            cameraOffset.x += (prevScreenMousePos.x - Mouse.current.position.ReadValue().x) * MouseDragScreenMultiplier;
            cameraOffset.z += (prevScreenMousePos.y - Mouse.current.position.ReadValue().y) * MouseDragScreenMultiplier;
        }
        CameraControl.CameraOffset = cameraOffset;
        CameraControl.CameraSpin = gameActions.InGame.SpinCamera.ReadValue<float>();
    }

    void UpdateMouseWorldPos()
    {
        float3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorldPos.y = Build.Elevation;
        MouseWorldPos = mouseWorldPos;

        prevScreenMousePos.x = Mouse.current.position.ReadValue().x;
        prevScreenMousePos.y = Mouse.current.position.ReadValue().y;
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
        SaveSystem saveSystem = new(System.IO.Path.Combine(Application.persistentDataPath, "testSave"));
        saveSystem.SaveGame();
        Debug.Log("Game Saved");
    }
    void LoadGame(InputAction.CallbackContext context)
    {
        SaveSystem saveSystem = new(System.IO.Path.Combine(Application.persistentDataPath, "testSave"));
        saveSystem.LoadGame();
        main.ComplyToGameSave();
        Debug.Log("Game Loaded");
    }
    void DivideRoad(InputAction.CallbackContext context)
    {
        Intersection ix = Snapping.Snap(MouseWorldPos, Build.LaneCount, Side.Both).Intersection;
        if (ix != null)
        {
            Combine.CombineRoads(ix);
            return;
        }
        if (Game.HoveredRoad != null)
            Divide.HandleDivideCommand(Game.HoveredRoad, MouseWorldPos);
        
    }
    void RemoveRoad(InputAction.CallbackContext context)
    {
        if (Game.HoveredRoad != null)
            Game.RemoveRoad(Game.HoveredRoad);
            
        Game.BulkRemoveSelected();
        roads.ClearSelected();
    }
    void Deselect(InputAction.CallbackContext context)
    {
        Build.UndoBuildCommand();
        roads.ClearSelected();
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
    void BulkSelectStarted(InputAction.CallbackContext context)
    {
        roads.BulkSelectStart(MouseWorldPos);
    }
    void BulkSelectPerformed(InputAction.CallbackContext context)
    {
        roads.BulkSelectPerformed();
    }
    void BulkSelectEnd(InputAction.CallbackContext context)
    {
        roads.BulkSelect();
    }
    void EnableStraightMode(InputAction.CallbackContext context)
    {
        Build.StraightMode = true;
    }
    void LevelEditorSelectEnabled(InputAction.CallbackContext context)
    {
        LevelEditor.SetSelected();
    }
}