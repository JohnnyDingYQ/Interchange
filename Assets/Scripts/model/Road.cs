using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Splines;

public class Road
{
    public int Id { get; set; }

    [JsonIgnore]
    public RoadGameObject RoadGameObject { get; set; }

    [JsonIgnore]
    public Spline Spline { get; set; }

    public List<Lane> Lanes { get; set; }
    public Vector3 StartPos { get; set; }
    public Vector3 PivotPos { get; set; }
    public Vector3 EndPos { get; set; }

    public int SplineKnotCount { get; set; }
}

