using System;
using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField]
    CameraSettings cameraSettings;
    public static Vector3 CameraOffset;
    public static float CameraSpin;
    float minHeight;
    Vector3 cameraVelocity;
    public static Quaternion Quaternion { get => Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0); }

    void Start()
    {
        HUDLayer[] enums = (HUDLayer[])Enum.GetValues(typeof(HUDLayer));
        minHeight = Main.GetHUDObjectHeight(enums[^1]) + Camera.main.nearClipPlane + 1f;
        Camera.main.orthographicSize = math.lerp(minHeight, cameraSettings.MaxHeight, 0.5f);
        Camera.main.transform.position = new(Camera.main.transform.position.x, Camera.main.orthographicSize, Camera.main.transform.position.z);

    }

    void LateUpdate()
    {
        Camera.main.transform.Rotate(0, CameraSpin * Time.deltaTime * cameraSettings.SpinMuliplier, 0, Space.World);

        SetCameraVelocity(CameraOffset);
        ApplyCameraVelocity();
        ClampHeight();
        if (Game.CameraBoundOn)
            ClampToBounds();
        cameraVelocity *= math.pow(math.E, -Time.deltaTime * cameraSettings.driftDecayExponentMultiplier);
        
        Camera.main.orthographicSize = Camera.main.transform.position.y;
    }

    void SetCameraVelocity(Vector3 cameraOffset)
    {
        float cameraHeight = Camera.main.transform.position.y;
        float heightRatio = cameraHeight / (cameraSettings.MaxHeight - minHeight);
        cameraOffset.x *= cameraSettings.PanMultiplier * cameraSettings.PanSpeed.Evaluate(heightRatio);
        cameraOffset.z *= cameraSettings.PanMultiplier * cameraSettings.PanSpeed.Evaluate(heightRatio);
        cameraOffset.y *= cameraSettings.ZoomMultiplier * cameraSettings.ZoomSpeed.Evaluate(heightRatio);

        ApplyZoom();
        ApplyPan();

        void ApplyZoom()
        {
            if (cameraOffset.y == 0)
                return;
            Vector3 v = Camera.main.transform.position - (Vector3)InputSystem.MouseWorldPos;
            float t = (cameraHeight + cameraOffset.y - Camera.main.transform.position.y) / v.y;
            cameraVelocity.y = (t * v).y;
        }

        void ApplyPan()
        {
            if (cameraOffset.x != 0)
                cameraVelocity.x = cameraOffset.x;
            if (cameraOffset.z != 0)
                cameraVelocity.z = cameraOffset.z;
        }
    }

    void ApplyCameraVelocity()
    {
        Camera.main.transform.position += Quaternion * cameraVelocity * Time.deltaTime;
    }

    void ClampHeight()
    {
        Camera cam = Camera.main;
        float3 cameraPos = cam.transform.position;
        if (Game.CameraBoundOn)
            cameraPos.y = Math.Clamp(cameraPos.y, minHeight, math.min(Game.BoundaryRadius, Game.BoundaryRadius / cam.aspect) - 5);
        else
            cameraPos.y = Math.Clamp(cameraPos.y, minHeight, cameraSettings.MaxHeight);
        cam.transform.position = cameraPos;
    }

    void ClampToBounds()
    {
        // Get the main camera
        Camera cam = Camera.main;
        float maxX = Game.BoundaryCenter.x + Game.BoundaryRadius;
        float minX = Game.BoundaryCenter.x - Game.BoundaryRadius;
        float maxZ = Game.BoundaryCenter.y + Game.BoundaryRadius;
        float minZ = Game.BoundaryCenter.y - Game.BoundaryRadius;
        Vector3[] screenCorners = new Vector3[]
        {
            new(0, 0, Camera.main.transform.position.y),    // Bottom-left corner
            new(1, 0, Camera.main.transform.position.y),    // Bottom-right corner
            new(0, 1, Camera.main.transform.position.y),    // Top-left corner
            new(1, 1, Camera.main.transform.position.y)     // Top-right corner
        };

        Vector3 camPos = cam.transform.position;
        foreach (Vector3 corner in screenCorners)
        {
            Vector3 worldPos = cam.ViewportToWorldPoint(corner);
            if (worldPos.x > maxX)
                camPos.x += maxX - worldPos.x;
            if (worldPos.x < minX)
                camPos.x += minX - worldPos.x;
            if (worldPos.z > maxZ)
                camPos.z += maxZ - worldPos.z;
            if (worldPos.z < minZ)
                camPos.z += minZ - worldPos.z;
        }
        if (cam.transform.position != camPos)
        {
            cameraVelocity = Vector3.zero;
            cam.transform.position = camPos;
        }
    }
}