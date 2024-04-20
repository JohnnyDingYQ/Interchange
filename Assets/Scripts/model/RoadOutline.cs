using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Mathematics;

public class RoadOutline
{
    public bool IsFixedAtStart { get; private set; }
    public bool IsFixedAtEnd { get; private set; }
    public float3 FixedStart { get; private set; }
    public List<float3> Start { get; set; }
    public List<float3> Mid { get; set; }
    public List<float3> End { get; set; }
    public float3 FixedEnd { get; private set; }

    public RoadOutline()
    {
        Start = new();
        End = new();
        Mid = new();
        IsFixedAtEnd = false;
        IsFixedAtEnd = false;
    }

    public int GetSize()
    {
        int size = Start.Count + End.Count + Mid.Count;
        if (IsFixedAtEnd)
            size++;
        if (IsFixedAtStart)
            size++;
        return size;
    }

    public List<float3> GetConcatenated()
    {
        List<float3> l = new();
        if (IsFixedAtStart)
            l.Add(FixedStart);
        l.AddRange(Start);
        l.AddRange(Mid);
        l.AddRange(End);
        if (IsFixedAtEnd)
            l.Add(FixedEnd);
        return l;
    }

    public void AddStartFixedPoint(float3 pt)
    {
        FixedStart = pt;
        IsFixedAtStart = true;
    }
    public void AddEndFixedPoint(float3 pt)
    {
        FixedEnd = pt;
        IsFixedAtEnd = true;
    }

    public bool IsPlausible()
    {
        if (Start.Count != 0)
            if (!Utility.AreNumericallyEqual(Start.Last(), Mid.First()))
                return false;
        if (End.Count != 0)
            if (!Utility.AreNumericallyEqual(End.First(), Mid.Last()))
                return false;
        return true;
    }
}