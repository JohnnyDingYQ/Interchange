using System.Collections.Generic;
using UnityEngine.Splines;
using CurveExtensions;
using UnityEngine;
using Unity.Mathematics;

public class BezierSeries
{
    public List<BezierCurve> Curves { get; private set; }
    public float StartInterpolation { get; set; }
    public float EndInterpolation { get; set; }

    public BezierSeries()
    {
        Curves = new();
        StartInterpolation = 0;
        EndInterpolation = 1;
    }

    public BezierSeries(List<BezierCurve> curves)
    {
        Curves = curves;
        StartInterpolation = 0;
        EndInterpolation = 1;
    }

    public BezierSeries(BezierCurve curve)
    {
        Curves = new() { curve };
        StartInterpolation = 0;
        EndInterpolation = 1;
    }

    public BezierSeries Offset(float distance, Orientation orientation)
    {
        if (orientation == Orientation.Right)
            distance *= -1;
        BezierSeries offset = new();
        foreach (BezierCurve curve in Curves)
        {
            float t = CurveUtility.GetDistanceToInterpolation(curve, 0.5f * CurveUtility.CalculateLength(curve));
            CurveUtility.Split(curve, t, out BezierCurve left, out BezierCurve right);
            offset.Curves.Add(NaiveOffsetByNormal(left));
            offset.Curves.Add(NaiveOffsetByNormal(right));
        }

        return offset;

        BezierCurve NaiveOffsetByNormal(BezierCurve curve)
        {
            return new(
                curve.P0 + curve.Normalized2DNormal(0) * distance,
                curve.P1 + curve.Normalized2DNormal(curve.InterpolationOfPoint(curve.P1)) * distance,
                curve.P2 + curve.Normalized2DNormal(curve.InterpolationOfPoint(curve.P2)) * distance,
                curve.P3 + curve.Normalized2DNormal(1) * distance
            );
        }
    }
}