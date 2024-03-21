using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Splines;

public class SplineAdapter : ICurve
{
    [JsonIgnore]
    public Spline Spline { get; set; }
    public float StartInterpolation { get; set; }
    public float EndInterpolation { get; set; }

    public SplineAdapter(Spline c, float startT, float endT)
    {
        StartInterpolation = startT;
        EndInterpolation = endT;
        Spline = c;
    }
    public void Draw(float duration)
    {
        Utility.DrawSpline(Spline, StartInterpolation, EndInterpolation, Color.yellow, duration);
    }
}