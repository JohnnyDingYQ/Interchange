using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField]
    CameraSettings cameraSettings;
    public static Vector3 CameraOffset;
    float minHeight;
    Vector3 cameraVelocity;

    void Start()
    {
        HUDLayer[] enums = (HUDLayer[])Enum.GetValues(typeof(HUDLayer));
        minHeight = Main.GetHUDObjectHeight(enums[^1]) + Camera.main.nearClipPlane + 1f;
    }

    void Update()
    {
        SetCameraVelocity(CameraOffset);
        Camera.main.transform.position += cameraVelocity * Time.deltaTime;
        cameraVelocity *= math.pow(math.E, -Time.deltaTime * cameraSettings.driftDecayExponentMultiplier);
        ClampToBounds();
        Camera.main.orthographicSize = Camera.main.transform.position.y;
    }

    public void SetCameraVelocity(Vector3 cameraOffset)
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
    void ClampToBounds()
    {
        float3 cameraPos = Camera.main.transform.position;
        cameraPos.y = Math.Clamp(cameraPos.y, minHeight, cameraSettings.MaxHeight);
        Camera.main.transform.position = cameraPos;
    }
}