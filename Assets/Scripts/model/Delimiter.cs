using Unity.Mathematics;
using UnityEngine;

public class Delimiter
{
    public float3 LeftBound { get; set; }
    public float3 RightBound { get; set; }
    public Plane Plane { get; set; }
    public float3 UpVector { get; set; }

    public Delimiter(float3 leftBound, float3 rightBound, float3 upVector)
    {
        Plane = new(leftBound, leftBound + upVector, rightBound);
        LeftBound = leftBound;
        RightBound = rightBound;
        UpVector = upVector;
    }

}