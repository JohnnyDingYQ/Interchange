using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Mathematics;
using Unity.Plastic.Newtonsoft.Json;
using System;
using System.Collections;

public class RoadOutline : IEnumerable<float3>
{
    public IEnumerable<float3> Start { get => GetCurveOutline(StartCurve, EndsNumPoint); }
    public IEnumerable<float3> Mid { get => GetCurveOutline(MidCurve, MidNumPoint); }
    public IEnumerable<float3> End { get => GetCurveOutline(EndCurve, EndsNumPoint); }
    public Curve StartCurve { get; set; }
    public Curve MidCurve { get; set; }
    public Curve EndCurve { get; set; }
    public const int EndsNumPoint = 10;
    public const int MidNumPoint = 20;

    IEnumerable<float3> GetCurveOutline(Curve curve, int numPoints)
    {
        if (curve == null)
            return GetEmptyFloat3Enumerator();
        return curve.GetOutline(numPoints);
    }

    public IEnumerable<float3> GetEmptyFloat3Enumerator()
    {
        yield break;
    }

    public int GetSize()
    {
        int size = Start.Count() + End.Count() + Mid.Count();
        return size;
    }

    public List<float3> GetConcatenated()
    {
        List<float3> l = new();
        l.AddRange(Start);
        l.AddRange(Mid);
        l.AddRange(End);
        return l;
    }

    public bool IsPlausible()
    {
        if (Start.Count() != 0)
            if (!MyNumerics.IsApproxEqual(Start.Last(), Mid.First()))
            {
                Debug.Log("start misaligns with mid");
                Debug.Log("start end: " + Start.Last());
                Debug.Log("mid start: " + Mid.First());
                return false;
            }
        if (End.Count() != 0)
            if (!MyNumerics.IsApproxEqual(End.First(), Mid.Last()))
            {
                Debug.Log("mid misaligns with end");
                Debug.Log("mid end: " + Mid.Last());
                Debug.Log("end start: " + End.First());
                return false;
            }
        return true;
    }

    public IEnumerator<float3> GetEnumerator()
    {
        foreach (float3 pos in Start)
            yield return pos;
        foreach (float3 pos in Mid)
            yield return pos;
        foreach (float3 pos in End)
            yield return pos;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override bool Equals(object obj)
    {
        if (obj is RoadOutline other)
            return IPersistable.Equals(StartCurve, other.StartCurve) && IPersistable.Equals(MidCurve, other.MidCurve)
                && IPersistable.Equals(EndCurve, other.EndCurve);
        else
            return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}