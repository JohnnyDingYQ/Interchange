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

    public BezierCurveAdapter(BezierCurve c)
    {
        StartT = 0;
        EndT = 1;
        BezierCurve = c;
    }
    public void Draw(float duration)
    {
        Utility.DrawBezierCurve(BezierCurve, StartT, EndT, Color.yellow, duration);
    }

    public float3 EvaluatePosition(float t)
    {
        return CurveUtility.EvaluatePosition(BezierCurve, StartT + (EndT - StartT) * t);
    }

    public float3 EvaluateNormal(float t)
    {
        throw new NotImplementedException();
    }
}