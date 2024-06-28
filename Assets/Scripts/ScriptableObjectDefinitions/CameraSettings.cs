using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Camera Settings", menuName = "Camera Settings", order = 51)]
public class CameraSettings : ScriptableObject
{
    public AnimationCurve ZoomSpeed;
    public AnimationCurve PanSpeed;
    public float MinHeight;
    public float MaxHeight;
    public float ZoomMultiplier;
    public float PanMultiplier;
}