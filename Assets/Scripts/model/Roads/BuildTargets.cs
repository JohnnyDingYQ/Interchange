using System.Collections.Generic;
using Unity.Mathematics;

public class BuildTargets
{
    public float3 ClickPos { get; set; }
    public bool Snapped { get; set; }
    public float3 Pos { get; set; }
    public Intersection Intersection { get; set; }
    /// <summary>
    /// Is a node index
    /// </summary>
    public int Offset { get; set; }
    public float3 Tangent { get; set; }
    public bool TangentAssigned { get; set; }
    public Road SelectedRoad { get; set; }
    public List<float3> NodesPos { get; set; }
    public Side Side { get; set; }
    public bool IsReplaceSuggestion { get; set; }
}