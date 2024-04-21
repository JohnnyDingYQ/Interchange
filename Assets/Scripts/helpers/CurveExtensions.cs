using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace CurveExtensions
{
    public static class MyExtensions
    {
        public static float3 Normalized2DNormal(this BezierCurve curve, float t)
        {
            float3 tangent = CurveUtility.EvaluateTangent(curve, t);
            tangent.y = 0;
            return Vector3.Cross(Vector3.up, tangent).normalized;
        }

        public static float InterpolationOfPoint(this BezierCurve curve, float3 pt)
        {
            pt.y = 0;
            Ray ray = new(pt, Vector3.up);
            float distance = CurveUtility.GetNearestPoint(curve, ray, out float3 position, out float interpolation);
            return interpolation;
        }
    }
}