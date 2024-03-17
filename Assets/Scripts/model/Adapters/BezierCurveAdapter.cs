using UnityEngine;
using UnityEngine.Splines;

public class BezierCurveAdapter : ICurve
{
    public BezierCurve BezierCurve { get; set; }
    public float StartInterpolation { get; set; }
    public float EndInterpolation { get; set; }

    public BezierCurveAdapter(BezierCurve c, float startT, float endT)
    {
        StartInterpolation = startT;
        EndInterpolation = endT;
        BezierCurve = c;
    }
    public void Draw(float duration)
    {
        Utility.DrawBezierCurve(BezierCurve, StartInterpolation, EndInterpolation, Color.yellow, duration);
    }
}