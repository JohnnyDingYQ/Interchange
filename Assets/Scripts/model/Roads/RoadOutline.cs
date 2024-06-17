using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Mathematics;
using Unity.Plastic.Newtonsoft.Json;
using System;

public class RoadOutline : IEquatable<RoadOutline>
{
    public List<float3> Start { get; set; }
    public List<float3> Mid { get; set; }
    public List<float3> End { get; set; }

    public RoadOutline()
    {
        Start = new();
        End = new();
        Mid = new();
    }

    public int GetSize()
    {
        int size = Start.Count + End.Count + Mid.Count;
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
            if (!MyNumerics.AreNumericallyEqual(Start.Last(), Mid.First()))
            {
                Debug.Log("start misaligns with mid");
                Debug.Log("start end: " + Start.Last());
                Debug.Log("mid start: " + Mid.First());
                return false;
            }
        if (End.Count != 0)
            if (!MyNumerics.AreNumericallyEqual(End.First(), Mid.Last()))
            {
                Debug.Log("mid misaligns with end");
                return false;
            }
        return true;
    }

    public bool Equals(RoadOutline other)
    {
        return MyNumerics.AreNumericallyEqual(Start, other.Start)
        && MyNumerics.AreNumericallyEqual(Mid, other.Mid)
        && MyNumerics.AreNumericallyEqual(End, other.End);
    }
}