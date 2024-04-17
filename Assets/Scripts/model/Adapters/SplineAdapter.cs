using Unity.Mathematics;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Splines;

public class SplineAdapter : ICurve
{
    [JsonIgnore]
    public Spline Spline { get; set; }
    public float StartT { get; set; }
    public float EndT { get; set; }
    public float Length { get; set; }
    public Lane Lane { get; set; }

    public SplineAdapter(Spline c, float startT, float endT, Lane lane)
    {
        StartT = startT;
        EndT = endT;
        Spline = c;
        Lane = lane;
    }
    public void Draw(float duration)
    {
        Gizmos.DrawSpline(Spline, StartT, EndT, Color.yellow, duration);
    }

    public float3 EvaluatePosition(float t)
    {
        return Spline.EvaluatePosition(StartT + (EndT - StartT) * t);
    }

    public float3 Evaluate2DNormal(float t)
    {
        float3 forward = Spline.EvaluateTangent(StartT + (EndT - StartT) * t);
        float3 upVector = Spline.EvaluateUpVector(StartT + (EndT - StartT) * t);
        float3 normal = Vector3.Cross(forward, upVector).normalized;
        normal.y = 0;
        return normal;
    }

    public void RestoreFromDeserialization()
    {
        Spline = Lane.Spline;
    }
}