using System;
using System.Collections.Generic;
using UnityEngine.Splines;

public class Road
{

    public Intersection StartIx { get; set; } // Ix is shorthand for intersection
    public Intersection EndIx { get; set; }
    public int Id { get; set; }
    public RoadGameObject RoadGameObject { get; set; }
    public Spline Spline { get; set; }
    public List<Lane> Lanes { get; set; }
    public int StartNode {get; set;}
    public int PivotNode {get; set;}
    public int EndNode {get; set;}

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
            nodeWithLane[lane.Start] = new() { lane };
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
            nodeWithLane[lane.End] = new() { lane };
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

