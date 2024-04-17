using System;
using Unity.Mathematics;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Splines;

public class BezierCurveAdapter : ICurve
{
    [JsonIgnore]
    public BezierCurve BezierCurve { get; set; }
    public float StartT { get; set; }
    public float EndT { get; set; }
    public CurveType CurveType { get; set; }
    public float3 P0 { get; set; }
    public float3 P1 { get; set; }
    public float3 P2 { get; set; }
    public float3 P3 { get; set; }

    public BezierCurveAdapter(BezierCurve c)
    {
        StartT = 0;
        EndT = 1;
        BezierCurve = c;
        CurveType = CurveType.Beizer;
        P0 = c.P0;
        P1 = c.P1;
        P2 = c.P2;
        P3 = c.P3;
    }
    
    public void Draw(float duration)
    {
        Gizmos.DrawBezierCurve(BezierCurve, StartT, EndT, Color.yellow, duration);
    }

    public float3 EvaluatePosition(float t)
    {
        return CurveUtility.EvaluatePosition(BezierCurve, StartT + (EndT - StartT) * t);
    }

    public float3 Evaluate2DNormal(float t)
    {
        return Vector3.Cross(CurveUtility.EvaluateTangent(BezierCurve, t), Vector3.up).normalized;
    }

    public void RestoreFromDeserialization()
    {
        BezierCurve = new(P0, P1, P2, P3);
    }
}