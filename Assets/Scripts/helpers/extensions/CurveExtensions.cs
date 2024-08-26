using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace CurveExtensions
{
    public static class CurveExtensions
    {
        public static float3 Normalized2DNormal(this BezierCurve curve, float t)
        {
            float3 tangent = CurveUtility.EvaluateTangent(curve, t);
            float3 normal = new(-tangent.z, 0, tangent.x);
            return math.normalize(normal);
        }

        public static float InterpolationOfPoint(this BezierCurve curve, float3 pt)
        {
            pt.y = 0;
            Ray ray = new(pt, Vector3.up);
            CurveUtility.GetNearestPoint(curve, ray, out _, out float interpolation);
            return interpolation;
        }
    }
}