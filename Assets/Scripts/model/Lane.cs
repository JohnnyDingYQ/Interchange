using UnityEngine.Splines;
using Unity.Plastic.Newtonsoft.Json;

public class Lane
{
  public int Start { get; set; }
  public int End { get; set; }

  [JsonIgnore]
  public Spline Spline { get; set; }

  public Road Road { get; set; }
}