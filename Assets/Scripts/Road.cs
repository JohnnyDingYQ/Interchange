using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class Road : MonoBehaviour
{
    public Spline Spline { get; set; }
    public int Start {get; set;}
    public int End {get; set;}
    public int Id {get; set;}

    public List<float3> LeftMesh {get; set;}
    public List<float3> RightMesh {get; set;}
    public Lane[] Lanes {get; set;}
    public HashSet<int> CrossedTiles{ get; set; }

}

