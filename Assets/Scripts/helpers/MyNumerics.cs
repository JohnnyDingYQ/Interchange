using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

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

    public static float2 Get2DVectorsIntersection(Vector2 p1, Vector2 v1, Vector2 p2, Vector2 v2)
    {
        Assert.IsFalse(Vector2.Distance(math.normalize(v1), math.normalize(v2)) < 0.01f);
        float t2 = (p1.y * v1.x + p2.x * v1.y - p1.x*v1.y - p2.y * v1.x) / (v2.y * v1.x - v2.x * v1.y);
        return p2 + t2 * v2;
    }
}