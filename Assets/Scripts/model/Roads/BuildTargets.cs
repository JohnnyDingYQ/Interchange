using System.Collections.Generic;
using Unity.Mathematics;

public class BuildTargets
{
    public List<Node> Nodes { get; set; }
    public float3 ClickPos { get; set; }
    public bool Snapped { get; set; }
    public float3 MedianPoint { get; set; }
    public Intersection Intersection { get; set; }
    public float3 Tangent { get; set; }
    public bool TangentAssigned { get; set; }
    public Road SelectedRoad { get; set; }
    public bool DividePossible { get; set; }
    public List<float3> NodesIfDivded { get; set; }
}