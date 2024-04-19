using System;
using Unity.Mathematics;
using UnityEngine;

public static class Utility
{
    public static bool AreNumericallyEqual(float3 a, float3 b)
    {
        return Vector3.Distance(a, b) < Constants.NumericallyEqualMaxTolerance;
    }
    public static bool AreNumericallyEqual(float3 a, float3 b, float tolerance)
    {
        return Vector3.Distance(a, b) < tolerance;
    }
    public static bool AreNumericallyEqual(float a, float b)
    {
        return Math.Abs(a - b) < Constants.NumericallyEqualMaxTolerance;
    }

    public static bool AreNumericallyEqual(float a, float b, float tolerance)
    {
        return Math.Abs(a - b) < tolerance;
    }
}