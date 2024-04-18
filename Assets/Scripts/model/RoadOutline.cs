using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

public class RoadOutline
{
    public List<float3> Start { get; set; }
    public List<float3> End { get; set; }
    public List<float3> Mid { get; set; }

    public RoadOutline()
    {
        Start = new();
        End = new();
        Mid = new();
    }

    public int GetSize()
    {
        return Start.Count + End.Count + Mid.Count;
    }

    public List<float3> GetConcatenated()
    {
        List<float3> l = new(Start);
        l.AddRange(Mid);
        l.AddRange(End);
        return l;
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