using System;
using Unity.Mathematics;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField]
    CameraSettings cameraSettings;
    private const float CameraSpeedMultiplier = 10;
    public static Vector3 CameraOffset;

    void Update()
    {
        AdjustCamera(CameraOffset);
    }

    public void AdjustCamera(Vector3 cameraOffset)
    {
        float cameraHeight = Camera.main.transform.position.y;
        float heightRatio = cameraHeight / (cameraSettings.MaxHeight - cameraSettings.MinHeight);
        cameraOffset.x *= cameraSettings.PanMultiplier * cameraSettings.PanSpeed.Evaluate(heightRatio);
        cameraOffset.z *= cameraSettings.PanMultiplier * cameraSettings.PanSpeed.Evaluate(heightRatio);
        cameraOffset.y *= cameraSettings.ZoomMultiplier * cameraSettings.ZoomSpeed.Evaluate(heightRatio);

        float3 newCameraPos = Camera.main.transform.position + CameraSpeedMultiplier * Time.deltaTime * cameraOffset;
        newCameraPos.y = Math.Clamp(newCameraPos.y, cameraSettings.MinHeight, cameraSettings.MaxHeight);
        
        Camera.main.transform.position = newCameraPos;
        Camera.main.orthographicSize = newCameraPos.y;
    }
}