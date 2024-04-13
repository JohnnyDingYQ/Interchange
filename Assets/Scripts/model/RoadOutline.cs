using System.Collections.Generic;
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
}