using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class Road : MonoBehaviour
{
    public Spline Spline { get; set; }
    public Intersection Start {get; set;}
    public Intersection End {get; set;}
    public int Id {get; set;}

    public List<float3> LeftMesh {get; set;}
    public List<float3> RightMesh {get; set;}
    public Mesh OriginalMesh {get; set;}
    public List<Lane> Lanes {get; set;}
    public HashSet<int> CrossedTiles{ get; set; }

    public void InitiateStartIntersection()
    {
        Dictionary<int, List<Lane>> nodeWithLane = new();
        foreach (Lane lane in Lanes)
        {
            nodeWithLane[lane.Start] = new() {lane};
        }
        Start = new Intersection() {
            NodeWithLane = nodeWithLane,
            Roads = new() {this}
        };
    }

    public void InitiateEndIntersection()
    {
        Dictionary<int, List<Lane>> nodeWithLane = new();
        foreach (Lane lane in Lanes)
        {
            nodeWithLane[lane.End] = new() {lane};
        }
        End = new Intersection() {
            NodeWithLane = nodeWithLane,
            Roads = new() {this}
        };
    }

    public int GetLaneCount()
    {
        return Lanes.Count;
    }
}

