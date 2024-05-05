using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystem : MonoBehaviour
{
    public static bool MouseIsInGameWorld { get; set; }
    private GameActions gameActions;
    private static float elevation;
    public static float Elevation
    {
        get { return elevation; }
        set
        {
            elevation = value;
            ReflectElevationChange();
        }
    }
    private const int CameraSpeedMultiplier = 10;
    public static float3 MouseWorldPos { get; set; }

    void Awake()
    {
        gameActions = new();
    }
    void Start()
    {
        MouseIsInGameWorld = true;
        Elevation = 0;
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

        float3 mouseWorldPos = new(Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.z)
        {
            z = Camera.main.transform.position.y - (Elevation + Constants.ElevationOffset)
        };
        MouseWorldPos = Camera.main.ScreenToWorldPoint(mouseWorldPos);
    }

    void UpdateCameraPos()
    {
        Vector3 cameraOffset = gameActions.InGame.MoveCamera.ReadValue<Vector3>();
        cameraOffset.x *= 1 + MathF.Pow(Camera.main.transform.position.y, 0.7f);
        cameraOffset.z *= 1 + MathF.Pow(Camera.main.transform.position.y, 0.7f);
        cameraOffset.y *= MathF.Pow(Camera.main.transform.position.y, 0.05f);
        Camera.main.transform.position += CameraSpeedMultiplier * Time.deltaTime * cameraOffset;
    }

    static void ReflectElevationChange()
    {
        DevPanel.Elevation.text = "Elevation: " + Elevation;
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
        Build.Reset();
    }
    void Elevate(InputAction.CallbackContext context)
    {
        if (Elevation + 2 < 30)
            Elevation += 2;
        else
            Elevation = 30;
    }
    void Lower(InputAction.CallbackContext context)
    {
        if (Elevation - 2 > 0)
            Elevation -= 2;
        else
            Elevation = 0;
    }
}