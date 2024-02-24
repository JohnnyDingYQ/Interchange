using UnityEngine.Splines;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Mathematics;

public class Lane
{
  public float3 StartPos { get; set; }
  public float3 EndPos { get; set; }

  public int StartNode { get; set; }
  public int EndNode { get; set; }

  [JsonIgnore]
  public Spline Spline { get; set; }

  public Road Road { get; set; }
}