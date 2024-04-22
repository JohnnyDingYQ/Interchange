using System;
using Unity.Mathematics;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Splines;

public class Vertex
{
    public int Id { get; set; }
    [JsonProperty]
    public float3 Pos { get; private set; }
    [JsonProperty]
    public SeriesLocation SeriesLocation { get; private set; }
    [JsonProperty]
    public float3 Tangent { get; private set; }
    [JsonProperty]
    public Lane Lane { get; private set; }
    [JsonIgnore]
    public Road Road { get { return Lane.Road; } }

    public Vertex() { }

    public Vertex(Lane lane, Side side)
    {
        Lane = lane;
        BezierSeries bs = lane.BezierSeries;
        if (side == Side.Start)
            SeriesLocation = bs.GetLocationByDistance(Constants.MinimumLaneLength / 2);
        else
            SeriesLocation = bs.GetLocationByDistance(bs.Length - Constants.MinimumLaneLength / 2);
            
        Pos = bs.EvaluatePosition(SeriesLocation);
        Tangent = math.normalizesafe(bs.EvaluateTangent(SeriesLocation));
    }
}