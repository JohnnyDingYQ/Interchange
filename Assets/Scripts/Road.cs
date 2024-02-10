using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
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
    public Mesh OriginalMesh {get; set;}
    public Lane[] Lanes {get; set;}
    public HashSet<int> CrossedTiles{ get; set; }
    /// <summary>
    /// Key: node of road. Value: node of lanes that are on the same side
    /// </summary>
    public Dictionary<int, List<int>> LaneNodes {get; set;}

    public Lane GetLaneByNode(int node)
    {
        foreach (Lane lane in Lanes)
        {
            if (lane.Start == node)
            {
                return lane;
            }
            if (lane.End == node)
            {
                return lane;
            }
        }
        return null;
    }
}

