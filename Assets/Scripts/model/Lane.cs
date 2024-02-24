using UnityEngine.Splines;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

public class Lane
{
  public Vector3 StartPos { get; set; }
  public Vector3 EndPos { get; set; }

  public int StartNode { get; set; }
  public int EndNode { get; set; }

  [JsonIgnore]
  public Spline Spline { get; set; }

  public Road Road { get; set; }
}