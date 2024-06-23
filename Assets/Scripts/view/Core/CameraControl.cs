using System;
using Unity.Mathematics;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField]
    CameraSettings cameraSettings;
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

        ApplyZoom();
        ApplyPan();
        ClampToBounds();

        Camera.main.orthographicSize = Camera.main.transform.position.y;

        void ApplyZoom()
        {
            Vector3 v = Camera.main.transform.position - (Vector3)InputSystem.MouseWorldPos;
            float t = (cameraHeight + cameraOffset.y - Camera.main.transform.position.y) / v.y;
            Camera.main.transform.position +=  t * Time.deltaTime * v;
        }

        void ApplyPan()
        {
            float3 newCameraPos = Camera.main.transform.position;
            newCameraPos.x += Time.deltaTime * cameraOffset.x;
            newCameraPos.z += Time.deltaTime * cameraOffset.z;
            Camera.main.transform.position = newCameraPos;
        }

        void ClampToBounds()
        {
            float3 cameraPos = Camera.main.transform.position;
            cameraPos.y = Math.Clamp(cameraPos.y, cameraSettings.MinHeight, cameraSettings.MaxHeight);
            Camera.main.transform.position = cameraPos;
        }
    }
}