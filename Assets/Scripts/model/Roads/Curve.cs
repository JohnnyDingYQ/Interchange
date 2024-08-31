using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CurveExtensions;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;

/// <summary>
/// Container for Unity Spline's BezierCurve to allow offsetting/Stroking
/// </summary>
public class Curve : IPersistable
{
    public uint Id { get; set; }
    BezierCurve bCurve;
    float bCurveLength;
    float offsetDistance;
    float startDistance, endDistance;
    float startT = 0, endT = 1;
    [NotSaved]
    DistanceToInterpolation[] lut;
    [NotSaved]
    public float Length { get => bCurveLength - startDistance - endDistance; }
    [NotSaved]
    public float3 StartPos { get => GetStartPos(); }
    [NotSaved]
    public float3 EndPos { get => GetEndPos(); }
    [NotSaved]
    public float3 StartTangent { get => GetStartTangent(); }
    [NotSaved]
    public float3 EndTangent { get => GetEndTangent(); }
    [NotSaved]
    public float3 StartNormal { get => GetStartNormal(); }
    [NotSaved]
    public float3 EndNormal { get => GetEndNormal(); }
    [NotSaved]
    float3[] segmentCache;
    [NotSaved]
    const int baseSegmentCount = 2;
    [NotSaved]
    const float flatnessTolerance = 0.2f;
    [NotSaved]
    const float getNearestPointTolerance = 0.001f;
    [NotSaved]
    const int distanceToInterpolationCacheSize = 30;

    public Curve() { }

    #region Getters

    float3 GetStartPos()
    {
        return CurveUtility.EvaluatePosition(bCurve, startT) + StartNormal * offsetDistance;
    }

    float3 GetEndPos()
    {
        return CurveUtility.EvaluatePosition(bCurve, endT) + EndNormal * offsetDistance;
    }

    float3 GetStartTangent()
    {
        return math.normalize(CurveUtility.EvaluateTangent(bCurve, startT));
    }

    float3 GetEndTangent()
    {
        return math.normalize(CurveUtility.EvaluateTangent(bCurve, endT));
    }

    float3 GetStartNormal()
    {
        return Normalized2DNormal(bCurve, startT);
    }

    float3 GetEndNormal()
    {
        return Normalized2DNormal(bCurve, endT);
    }

    float3 Normalized2DNormal(BezierCurve bezierCurve, float t)
    {
        float3 tangent = CurveUtility.EvaluateTangent(bezierCurve, t);
        float3 normal = new(-tangent.z, 0, tangent.x);
        return math.normalizesafe(normal);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Containerize a BeizerCurve to allow offsetting/stroking
    /// </summary>
    /// <param name="beizerCurve">BeizerCurve to offset</param>
    public Curve(BezierCurve beizerCurve)
    {
        bCurve = beizerCurve;
        bCurveLength = CurveUtility.CalculateLength(bCurve);
        CreateDistanceCache();
    }

    /// <summary>
    /// Creates a deep copy of the current curve
    /// </summary>
    /// <returns>Deep copy</returns>
    public Curve Duplicate()
    {
        return new()
        {
            bCurve = bCurve,
            bCurveLength = bCurveLength,
            offsetDistance = offsetDistance,
            startDistance = startDistance,
            endDistance = endDistance,
            startT = startT,
            endT = endT,
            lut = lut
        };
    }

    /// <summary>
    /// Remove a portion of the curve a given distance from the start
    /// </summary>
    /// <param name="distance">Distance from start</param>
    /// <returns>Truncated Curve</returns>
    public Curve AddStartDistance(float distance)
    {
        startDistance += distance;
        Assert.IsTrue(startDistance >= 0 && startDistance <= bCurveLength,
            $"startDistance: {startDistance}, bCurveLength: {bCurveLength}");
        startT = CurveUtility.GetDistanceToInterpolation(lut, startDistance);
        return this;
    }

    /// <summary>
    /// Remove a portion of the curve a given distance from the end
    /// </summary>
    /// <param name="distance">Distance from end</param>
    /// <returns>Truncated Curve</returns>
    public Curve AddEndDistance(float distance)
    {
        endDistance += distance;
        Assert.IsTrue(endDistance >= 0 && endDistance <= bCurveLength,
            $"endDistance: {endDistance}, bCurveLength: {bCurveLength}");
        endT = CurveUtility.GetDistanceToInterpolation(lut, bCurveLength - endDistance);
        return this;
    }

    /// <summary>
    /// Reverse the direction of the curve
    /// </summary>
    /// <returns>The reversed curve</returns>
    public Curve Reverse()
    {
        Curve reversed = new()
        {
            bCurve = bCurve.GetInvertedCurve(),
            startDistance = endDistance,
            endDistance = startDistance,
            bCurveLength = bCurveLength,
            offsetDistance = -offsetDistance,
        };
        reversed.CreateDistanceCache();
        reversed.startT = reversed.GetDistanceToInterpolation(startDistance);
        reversed.endT = reversed.GetDistanceToInterpolation(bCurveLength - endDistance);
        return reversed;
    }

    /// <summary>
    /// Offset the curve a given distance with respect to its 2D normal
    /// </summary>
    /// <param name="distance">Offset distance</param>
    /// <returns>Offsetted Curve</returns>
    public Curve Offset(float distance)
    {
        offsetDistance += distance;
        return this;
    }

    /// <summary>
    /// Get an IEnumerable of equally distanced points on the curve
    /// </summary>
    /// <param name="numPoints"></param>
    /// <returns></returns>
    public OutlineEnum GetOutline(int numPoints)
    {
        return new(this, numPoints);
    }

    /// <summary>
    /// Add the start of another curve to the end of this curve
    /// </summary>
    /// <param name="other">The other curve</param>
    public void Add(Curve other)
    {
        if (!bCurve.P0.Equals(other.bCurve.P0) || !bCurve.P1.Equals(other.bCurve.P1)
            || !bCurve.P2.Equals(other.bCurve.P2) || !bCurve.P3.Equals(other.bCurve.P3))
            throw new ArgumentException("given curve have different control points", "other");
        float length = Length;
        AddEndDistance(-other.Length);
        other.AddStartDistance(-length);
    }

    /// <summary>
    /// Evaluate the position of a given distance on curve 
    /// </summary>
    /// <param name="distance">The given distance</param>
    /// <returns>Position on curve</returns>
    /// <exception cref="ArgumentException">Given distance is negative</exception>
    public float3 EvaluatePosition(float distance)
    {
        if (distance < 0)
            throw new ArgumentException("distance cannot be negative", "distance");
        float t = CurveUtility.GetDistanceToInterpolation(lut, startDistance + distance);
        if (offsetDistance == 0)
            return CurveUtility.EvaluatePosition(bCurve, t);
        return CurveUtility.EvaluatePosition(bCurve, t) + Normalized2DNormal(bCurve, t) * offsetDistance;
    }

    /// <summary>
    /// Evaluate the tangent of a given distance on curve
    /// </summary>
    /// <param name="distance">The given distance</param>
    /// <returns>Tangent</returns>
    /// <exception cref="ArgumentException">Given distance is negative</exception>
    public float3 EvaluateTangent(float distance)
    {
        float t = CurveUtility.GetDistanceToInterpolation(lut, startDistance + distance);
        return math.normalize(CurveUtility.EvaluateTangent(bCurve, t));
    }

    /// <summary>
    /// Evaluate the normalized 2D (xz plane) normal of a given distance on curve
    /// </summary>
    /// <param name="distance">The given distance</param>
    /// <returns>Normalized 2D nomral</returns>
    /// <exception cref="ArgumentException">Given distance is negative</exception>
    public float3 Evaluate2DNormal(float distance)
    {
        float t = CurveUtility.GetDistanceToInterpolation(lut, startDistance + distance);
        return math.normalize(Normalized2DNormal(bCurve, t));
    }

    /// <summary>
    /// Compute the minimum distance between the curve and a given ray
    /// </summary>
    /// <param name="ray">The given ray</param>
    /// <param name="distanceOnCurve">The distance on curve when the said minimum occurs</param>
    /// <param name="resolution">Higher resolution prevents local minimum from being mistaken as global minimum</param>
    /// <returns>The minimum distance between the curve and a given ray</returns>
    public float GetNearestDistance(Ray ray, out float distanceOnCurve, int resolution = 10)
    {
        float minDistance = float.MaxValue;
        distanceOnCurve = 0;
        float distanceStep = Length / resolution;
        float localMin = 0;
        while (distanceOnCurve <= Length)
        {
            float3 pos = EvaluatePosition(distanceOnCurve);
            float distance = GetDistanceToCurve(pos);
            if (distance < minDistance)
            {
                minDistance = distance;
                localMin = distanceOnCurve;
            }
            distanceOnCurve += distanceStep;
        }
        float low = localMin - distanceStep >= 0 ? localMin - distanceStep : 0;
        float high = localMin + distanceStep <= Length ? localMin + distanceStep : Length;
        do
        {
            float mid = (low + high) / 2;
            if (GetDistanceToCurve(EvaluatePosition(Mathf.Max(0, mid - getNearestPointTolerance)))
                < GetDistanceToCurve(EvaluatePosition(Mathf.Min(Length, mid + getNearestPointTolerance))))
                high = mid;
            else
                low = mid;
        } while (high - low > getNearestPointTolerance);

        distanceOnCurve = low;
        return GetDistanceToCurve(EvaluatePosition(low));

        float GetDistanceToCurve(float3 pos)
        {
            return Vector3.Cross(ray.direction, (Vector3)pos - ray.origin).magnitude;
        }
    }

    /// <summary>
    /// Split the curve with a given distance on curve
    /// </summary>
    /// <param name="distance">The given distance</param>
    /// <param name="left">Left curve</param>
    /// <param name="right">Right curve</param>
    /// <exception cref="ArgumentException">Split distance is greater than curve length</exception>
    public void Split(float distance, out Curve left, out Curve right)
    {
        left = Duplicate();
        right = Duplicate();
        left = left.AddEndDistance(Length - distance);
        right = right.AddStartDistance(distance);
    }

    public float3 LerpPosition(float distance)
    {
        if (distance >= Length)
            return segmentCache[^1];
        if (segmentCache == null)
            InitSegmentCache();
        float segmentLength = Length / (segmentCache.Length - 1);
        int index = (int)(distance / segmentLength);
        if (index == segmentCache.Length - 1)
            index--;
        Assert.IsTrue(segmentLength != 0);
        if (index + 1 >= segmentCache.Length)
            index += 0;
        return math.lerp(segmentCache[index], segmentCache[index + 1], (distance - index * segmentLength) / segmentLength);
    }

    public void CreateDistanceCache()
    {
        lut = new DistanceToInterpolation[distanceToInterpolationCacheSize];
        CurveUtility.CalculateCurveLengths(bCurve, lut);
    }

    #endregion

    private void InitSegmentCache()
    {
        int segmentCount = Mathf.CeilToInt(Mathf.Sqrt(CalculateFlatness() / flatnessTolerance)) * baseSegmentCount;

        segmentCount = Mathf.Max(segmentCount, baseSegmentCount);
        segmentCache = new float3[segmentCount + 1];
        float segmentLength = Length / segmentCount;
        for (int i = 0; i <= segmentCount; i++)
            segmentCache[i] = EvaluatePosition(segmentLength * i);

        float CalculateFlatness()
        {
            Vector3 p0p3 = bCurve.P3 - bCurve.P0;
            Vector3 p0p1 = bCurve.P1 - bCurve.P0;
            Vector3 p3p2 = bCurve.P2 - bCurve.P3;

            float area1 = Vector3.Cross(p0p3, p0p1).magnitude;
            float area2 = Vector3.Cross(p0p3, p3p2).magnitude;

            float baseLength = p0p3.magnitude;
            float flatness = (area1 + area2) / baseLength;

            return flatness;
        }
    }

    float GetDistanceToInterpolation(float distance)
    {
        return CurveUtility.GetDistanceToInterpolation(lut, distance);
    }

    public override bool Equals(object obj)
    {
        if (obj is Curve other)
            return bCurve.P0.Equals(other.bCurve.P0) && bCurve.P1.Equals(other.bCurve.P1)
                && bCurve.P2.Equals(other.bCurve.P2) && bCurve.P3.Equals(other.bCurve.P3) && bCurveLength == other.bCurveLength
                && offsetDistance == other.offsetDistance && startDistance == other.startDistance && endDistance == other.endDistance
                && startT == other.startT && endT == other.endT;
        else
            return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

public class OutlineEnum : IEnumerable<float3>
{
    readonly Curve curve;
    readonly float numPoints;

    public OutlineEnum(Curve curve, float numPoints)
    {
        this.curve = curve;
        this.numPoints = numPoints;
    }

    public IEnumerator<float3> GetEnumerator()
    {
        float pointSeparation = curve.Length / (numPoints - 1);
        int count = 0;
        float currDistance = 0;
        while (count++ < numPoints)
        {
            yield return curve.EvaluatePosition(currDistance);
            currDistance += pointSeparation;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}