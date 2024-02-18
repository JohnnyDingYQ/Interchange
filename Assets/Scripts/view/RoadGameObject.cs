using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class RoadGameObject : MonoBehaviour
{
    public Road Road { get; set; }

    public List<float3> LeftMesh { get; set; }
    public List<float3> RightMesh { get; set; }
    public Mesh OriginalMesh { get; set; }
}