using System;
using System.Collections.Generic;
using UnityEngine.Splines;

public class Road
{

    public Intersection Start { get; set; }
    public Intersection End { get; set; }
    public int Id { get; set; }
    public RoadGameObject RoadGameObject { get; set; }
    public Spline Spline { get; set; }
    public List<Lane> Lanes { get; set; }

    public Road()
    {
        Start = null;
        End = null;
    }

    public void InitiateStartIntersection()
    {
        if (Start != null)
            throw new InvalidOperationException("Intersection at Start is already initalized");
        Dictionary<int, List<Lane>> nodeWithLane = new();
        foreach (Lane lane in Lanes)
        {
            nodeWithLane[lane.Start] = new() { lane };
        }
        Start = new Intersection()
        {
            NodeWithLane = nodeWithLane,
            Roads = new() { this }
        };
    }

    public void InitiateEndIntersection()
    {
        if (End != null)
            throw new InvalidOperationException("Intersection at End is already initalized");
        Dictionary<int, List<Lane>> nodeWithLane = new();
        foreach (Lane lane in Lanes)
        {
            nodeWithLane[lane.End] = new() { lane };
        }
        End = new Intersection()
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

