using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class MyNumerics
{
    const float DefaultTolerance = 0.01f;

    public static bool AreNumericallyEqual(float3 a, float3 b)
    {
        return Vector3.Distance(a, b) < DefaultTolerance;
    }
    public static bool AreNumericallyEqual(float3 a, float3 b, float tolerance)
    {
        return Vector3.Distance(a, b) < tolerance;
    }
    public static bool AreNumericallyEqual(float a, float b)
    {
        return Math.Abs(a - b) < DefaultTolerance;
    }

    public static bool AreNumericallyEqual(float a, float b, float tolerance)
    {
        return Math.Abs(a - b) < tolerance;
    }

    public static bool AreNumericallyEqual(List<float3> a, List<float3> b)
    {
        return AreNumericallyEqual(a, b, DefaultTolerance);
    }

    public static bool AreNumericallyEqual(List<float3> a, List<float3> b, float tolerance)
    {
        if (a.Count != b.Count)
            return false;
        for (int i = 0; i < a.Count; i++)
            if (!AreNumericallyEqual(a[i], b[i], tolerance))
                return false;
            
        return true;
    }

    public static int GetRandomIndex(int length)
    {
        int index =  (int) (UnityEngine.Random.value * length);
        return index != length ? index : GetRandomIndex(length);
    }
}