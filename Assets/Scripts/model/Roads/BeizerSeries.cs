using System.Collections.Generic;
using UnityEngine.Splines;
using CurveExtensions;
using UnityEngine;
using Unity.Mathematics;
using Unity.Plastic.Newtonsoft.Json;
using System.Linq;
using System;
using System.Collections.ObjectModel;
using UnityEngine.Assertions;

public class BezierSeries
{
    [JsonIgnore]
    private List<BezierCurve> curves;
    [JsonIgnore]
    public ReadOnlyCollection<BezierCurve> Curves { get { return curves.AsReadOnly(); } }
    public float Length { get; set; }
    [JsonProperty]
    public SeriesLocation StartLocation { get; private set; }
    [JsonProperty]
    public SeriesLocation EndLocation { get; private set; }
    private float startLocationDistance = 0;
    private float endLocationDistance = 0;
    [JsonProperty]
    public bool IsOffsetted { get; set; }
    [JsonProperty]
    private List<List<float3>> serializedCurves;

    [JsonConstructor]
    public BezierSeries(List<List<float3>> serializedCurves)
    {
        RestoreFromSerialization(serializedCurves);
    }

    public BezierSeries(List<BezierCurve> c, bool offsetted)
    {
        curves = c;
        StartLocation = new(0, 0);
        EndLocation = new(curves.Count - 1, 1);
        IsOffsetted = offsetted;
        Length = GetLength();
        PrepForSerialization();
    }

    public BezierSeries(BezierCurve c)
    {
        curves = new() { c };
        StartLocation = new(0, 0);
        EndLocation = new(0, 1);
        IsOffsetted = false;
        Length = GetLength();
        PrepForSerialization();
    }

    public void SetStartLocation(SeriesLocation location)
    {
        StartLocation = location;
        startLocationDistance = GetDistanceByLocation(location);
    }

    public void SetEndLocation(SeriesLocation location)
    {
        EndLocation = location;
        endLocationDistance = Length - GetDistanceByLocation(location);
    }

    float GetDistanceByLocation(SeriesLocation location)
    {
        float distance = 0;
        for (int i = 0; i < location.Index; i++)
            distance += CurveUtility.CalculateLength(curves[i], 10);
        CurveUtility.Split(curves[location.Index], location.Interpolation, out BezierCurve left, out BezierCurve right);
        distance += CurveUtility.CalculateLength(left, 10);
        return distance;
    }

    float GetLength()
    {
        float length = 0;
        foreach (BezierCurve curve in curves)
            length += CurveUtility.CalculateLength(curve, 10);
        return length;
    }

    public BezierSeries Offset(float distance)
    {
        List<BezierCurve> offset = new();
        if (curves.Count != 1)
        {
            foreach (BezierCurve c in curves)
                offset.Add(NaiveOffsetByNormal(c));
            return new(offset, true);
        }

        List<BezierCurve> divided = DivideCurves(curves);
        divided = DivideCurves(divided);
        foreach (BezierCurve c in divided)
            offset.Add(NaiveOffsetByNormal(c));

        return new(offset, true);

        BezierCurve NaiveOffsetByNormal(BezierCurve curve)
        {
            return new(
                curve.P0 + curve.Normalized2DNormal(0) * distance,
                curve.P1 + curve.Normalized2DNormal(curve.InterpolationOfPoint(curve.P1)) * distance,
                curve.P2 + curve.Normalized2DNormal(curve.InterpolationOfPoint(curve.P2)) * distance,
                curve.P3 + curve.Normalized2DNormal(1) * distance
            );
        }

        List<BezierCurve> DivideCurves(List<BezierCurve> curves)
        {
            List<BezierCurve> result = new();
            foreach (BezierCurve curve in curves)
            {
                float t = CurveUtility.GetDistanceToInterpolation(curve, 0.5f * CurveUtility.CalculateLength(curve));
                CurveUtility.Split(curve, t, out BezierCurve left, out BezierCurve right);
                result.Add(left);
                result.Add(right);
            }
            return result;
        }
    }

    public void PrepForSerialization()
    {
        serializedCurves = new();
        foreach (BezierCurve curve in curves)
            serializedCurves.Add(new List<float3>() { curve.P0, curve.P1, curve.P2, curve.P3 });
    }

    public void RestoreFromSerialization(List<List<float3>> serializeCurves)
    {
        curves = new();
        foreach (List<float3> a in serializeCurves)
            curves.Add(new(a[0], a[1], a[2], a[3]));
    }

    public float3 EvaluatePosition(SeriesLocation location)
    {
        return CurveUtility.EvaluatePosition(curves[location.Index], location.Interpolation);
    }
    public float3 EvaluatePosition(float t)
    {
        Assert.IsTrue(t <= 1);
        Assert.IsTrue(t >= 0);
        SeriesLocation location = InterpolationToLocation(t);
        return CurveUtility.EvaluatePosition(curves[location.Index], location.Interpolation);
    }
    public float3 EvaluateTangent(SeriesLocation location)
    {
        return CurveUtility.EvaluateTangent(curves[location.Index], location.Interpolation);
    }
    public float3 EvaluateTangent(float t)
    {
        Assert.IsTrue(t <= 1);
        Assert.IsTrue(t >= 0);
        SeriesLocation location = InterpolationToLocation(t);
        return CurveUtility.EvaluateTangent(curves[location.Index], location.Interpolation);
    }

    public float3 Evaluate2DNormalizedNormal(SeriesLocation location)
    {
        return curves[location.Index].Normalized2DNormal(location.Interpolation);
    }
    public float3 Evaluate2DNormalizedNormal(float t)
    {
        Assert.IsTrue(t <= 1);
        Assert.IsTrue(t >= 0);
        SeriesLocation location = InterpolationToLocation(t);
        return curves[location.Index].Normalized2DNormal(location.Interpolation);
    }

    public SeriesLocation InterpolationToLocation(float t)
    {
        Assert.IsTrue(t <= 1);
        Assert.IsTrue(t >= 0);
        float distanceOnCurve = t * (Length - startLocationDistance - endLocationDistance) + startLocationDistance;
        return GetLocationByDistance(distanceOnCurve);
    }

    public float LocationToInterpolation(SeriesLocation location)
    {
        float distance = GetDistanceByLocation(location);
        return (distance - startLocationDistance) / (Length - startLocationDistance - endLocationDistance);
    }

    public List<float3> GetOutline(Orientation orientation)
    {
        List<float3> results = new();
        for (int i = StartLocation.Index; i <= EndLocation.Index; i++)
        {
            BezierCurve curve = curves[i];
            int numPoints = (int)(CurveUtility.ApproximateLength(curve) * Constants.MeshResolution);
            for (int j = 0; j <= numPoints; j++)
            {
                float t = (float)j / numPoints;
                float lower = 0;
                float upper = 1;
                if (i == StartLocation.Index)
                    lower = StartLocation.Interpolation;
                if (i == EndLocation.Index)
                    upper = EndLocation.Interpolation;
                t = lower + t * (upper - lower);
                float3 normal = curve.Normalized2DNormal(t) * Constants.RoadOutlineSeparation;
                if (orientation == Orientation.Right)
                    normal *= -1;
                results.Add(CurveUtility.EvaluatePosition(curve, t) + normal);
            }
        }
        return results;
    }

    public SeriesLocation GetLocationByDistance(float distanceFromStart)
    {
        float distance = CurveUtility.CalculateLength(curves.First(), 10);
        int startIndex = 0;
        while (distanceFromStart - distance > 0.05f)
        {
            distanceFromStart -= distance;
            startIndex++;
            distance = CurveUtility.CalculateLength(curves[startIndex], 10);
        }
        float startInterpolation = CurveUtility.GetDistanceToInterpolation(curves[startIndex], distanceFromStart);
        return new(startIndex, startInterpolation);
    }

    public float GetNearestPoint(Ray ray, out float3 position, out SeriesLocation location)
    {
        float minDistance = float.MaxValue;
        int minIndex = 0;
        for (int i = 0; i < curves.Count; i++)
        {
            float distance = CurveUtility.GetNearestPoint(curves[i], ray, out float3 p, out float t);
            if (distance < minDistance)
            {
                minDistance = distance;
                minIndex = i;
            }
        }
        float _d = CurveUtility.GetNearestPoint(curves[minIndex], ray, out float3 _p, out float _t);
        position = _p;
        location = new(minIndex, _t);
        return _d;
    }

    public void Split(SeriesLocation location, out BezierSeries left, out BezierSeries right)
    {
        CurveUtility.Split(curves[location.Index], location.Interpolation, out BezierCurve l, out BezierCurve r);
        List<BezierCurve> leftSeries = new();
        List<BezierCurve> rightSeries = new();
        int count = 0;
        while (count != location.Index)
            leftSeries.Add(curves[count++]);
        leftSeries.Add(l);
        count = location.Index + 1;
        rightSeries.Add(r);
        while (count < curves.Count)
            rightSeries.Add(curves[count++]);
        left = new(leftSeries, IsOffsetted);
        right = new(rightSeries, IsOffsetted);
    }
}

public class SeriesLocation
{
    public int Index { get; set; }
    public float Interpolation { get; set; }

    public SeriesLocation(int index, float interpolation)
    {
        Index = index;
        Interpolation = interpolation;
    }
}