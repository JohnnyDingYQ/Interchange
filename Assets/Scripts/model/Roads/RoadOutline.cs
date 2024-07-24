using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Mathematics;
using Unity.Plastic.Newtonsoft.Json;
using System;
using System.Collections;

public class RoadOutline : IEnumerable<float3>
{
    public List<float3> Start { get; set; }
    public IEnumerable<float3> Mid { get => MidCurve.GetOutline((int) (MidCurve.Length * Constants.MeshResolution)); }
    public List<float3> End { get; set; }
    public Curve StartCurve { get; set; }
    public Curve MidCurve { get; set; }
    public Curve EndCurve { get; set; }

    public RoadOutline()
    {
        Start = new();
        End = new();
    }

    public int GetSize()
    {
        int size = Start.Count + End.Count + Mid.Count();
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
        if (Start.Count != 0)
            if (!MyNumerics.IsApproxEqual(Start.Last(), Mid.First()))
            {
                Debug.Log("start misaligns with mid");
                Debug.Log("start end: " + Start.Last());
                Debug.Log("mid start: " + Mid.First());
                return false;
            }
        if (End.Count != 0)
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
        foreach (float3 pos in GetConcatenated())
            yield return pos;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}