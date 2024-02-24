using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Splines;

public class Road
{

    public Intersection StartIx { get; set; } // Ix is shorthand for intersection
    public Intersection EndIx { get; set; }
    public int Id { get; set; }

    [JsonIgnore]
    public RoadGameObject RoadGameObject { get; set; }

    [JsonIgnore]
    public Spline Spline { get; set; }

    public List<Lane> Lanes { get; set; }
    public float3 StartPos { get; set; }
    public float3 PivotPos { get; set; }
    public float3 EndPos { get; set; }

    public int SplineKnotCount { get; set; }

    public Road()
    {
        StartIx = null;
        EndIx = null;
    }

    public void InitiateStartIntersection()
    {
        if (StartIx != null)
            throw new InvalidOperationException("Intersection at Start is already initalized");
        Dictionary<int, HashSet<Lane>> nodeWithLane = new();
        foreach (Lane lane in Lanes)
        {
            nodeWithLane[lane.StartNode] = new() { lane };
        }
        StartIx = new Intersection()
        {
            NodeWithLane = nodeWithLane,
            Roads = new() { this }
        };
    }

    public void InitiateEndIntersection()
    {
        if (EndIx != null)
            throw new InvalidOperationException("Intersection at End is already initalized");
        Dictionary<int, HashSet<Lane>> nodeWithLane = new();
        foreach (Lane lane in Lanes)
        {
            nodeWithLane[lane.EndNode] = new() { lane };
        }
        EndIx = new Intersection()
        {
            NodeWithLane = nodeWithLane,
            Roads = new() { this }
        };
    }

    public int GetLaneCount()
    {
        return Lanes.Count;
    }
}

