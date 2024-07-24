using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class MyNumerics
{
    const float DefaultTolerance = 0.01f;

    public static bool IsApproxEqual(float3 a, float3 b)
    {
        return Vector3.Distance(a, b) < DefaultTolerance;
    }
    public static bool IsApproxEqual(float3 a, float3 b, float tolerance)
    {
        return Vector3.Distance(a, b) < tolerance;
    }
    public static bool IsApproxEqual(float a, float b)
    {
        return Math.Abs(a - b) < DefaultTolerance;
    }

    public static bool IsApproxEqual(float a, float b, float tolerance)
    {
        return Math.Abs(a - b) < tolerance;
    }
    
    public static float AngleInDegrees(float3 a, float3 b)
    {
        if (math.length(a) == 0 || math.length(b) == 0)
            return 0;
        return MathF.Acos(math.dot(a, b) / math.length(a) / math.length(b)) / MathF.PI * 180;
    }

    public static int GetRandomIndex(int length)
    {
        int index =  (int) (UnityEngine.Random.value * length);
        return index != length ? index : GetRandomIndex(length);
    }
}