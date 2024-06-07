using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystem : MonoBehaviour
{
    public static bool MouseIsInGameWorld { get; set; }
    private GameActions gameActions;
    private const int CameraSpeedMultiplier = 10;
    public static float3 MouseWorldPos { get; set; }

    void Awake()
    {
        gameActions = new();
    }
    void Start()
    {
        MouseIsInGameWorld = true;
        MouseWorldPos = new(0, Constants.HeightOffset, 0);
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

        float3 mouseWorldPos = new(0)
        {
            x = Input.mousePosition.x,
            y = Input.mousePosition.y,
            z = Camera.main.transform.position.y
        };
        mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseWorldPos);
        mouseWorldPos.y = Game.Elevation + Constants.HeightOffset;
        MouseWorldPos = mouseWorldPos;
    }

    void UpdateCameraPos()
    {
        Vector3 cameraOffset = gameActions.InGame.MoveCamera.ReadValue<Vector3>();
        float cameraHeight = Camera.main.transform.position.y;
        cameraOffset.x *= 1 + MathF.Pow(cameraHeight, 0.7f);
        cameraOffset.z *= 1 + MathF.Pow(cameraHeight, 0.7f);
        cameraOffset.y *= MathF.Pow(cameraHeight, 0.05f);
        Camera.main.transform.position += CameraSpeedMultiplier * Time.deltaTime * cameraOffset;
        Clamp();
        Camera.main.orthographicSize = cameraHeight * 0.8f;

        static void Clamp()
        {
            Vector3 pos = Camera.main.transform.position;
            pos.y = MathF.Max(32, pos.y);
            Camera.main.transform.position = pos;
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
    void Elevate(InputAction.CallbackContext context)
    {
        Game.SetElevation(Game.Elevation + 2);
    }
    void Lower(InputAction.CallbackContext context)
    {
        Game.SetElevation(Game.Elevation - 2);
    }
}