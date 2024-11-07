using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField]
    CameraSettings cameraSettings;
    public static Vector3 CameraOffset;
    public static float CamearSpin;
    float minHeight;
    Vector3 cameraVelocity;
    public static Quaternion Quaternion { get => Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);}

    void Start()
    {
        HUDLayer[] enums = (HUDLayer[])Enum.GetValues(typeof(HUDLayer));
        minHeight = Main.GetHUDObjectHeight(enums[^1]) + Camera.main.nearClipPlane + 1f;
        Camera.main.orthographicSize = math.lerp(minHeight, cameraSettings.MaxHeight, 0.5f);
        Camera.main.transform.position = new(Camera.main.transform.position.x, Camera.main.orthographicSize, Camera.main.transform.position.z);

    }

    void Update()
    {
        Camera.main.transform.Rotate(0, CamearSpin * Time.deltaTime * cameraSettings.SpinMuliplier, 0, Space.World);

        SetCameraVelocity(CameraOffset);
        ApplyCameraVelocity();
        cameraVelocity *= math.pow(math.E, -Time.deltaTime * cameraSettings.driftDecayExponentMultiplier);
        ClampToBounds();
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

    void ClampToBounds()
    {
        float3 cameraPos = Camera.main.transform.position;
        cameraPos.y = Math.Clamp(cameraPos.y, minHeight, cameraSettings.MaxHeight);
        Camera.main.transform.position = cameraPos;
    }
}