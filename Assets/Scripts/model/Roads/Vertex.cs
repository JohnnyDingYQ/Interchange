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
    public float SeriesInterpolation { get; private set; }
    [JsonProperty]
    public float3 Tangent { get; private set; }
    [JsonProperty]
    public Lane Lane { get; private set; }
    [JsonIgnore]
    public Road Road { get { return Lane.Road; } }
    [JsonIgnore]
    public int ScheduledCars { get; set; }

    public Vertex() { }

    public Vertex(Lane lane, Side side)
    {
        SetOwnerLane(lane, side);
        
        BezierSeries bs = lane.BezierSeries;
        Pos = bs.EvaluatePosition(SeriesInterpolation);
        Tangent = math.normalizesafe(bs.EvaluateTangent(SeriesInterpolation));
    }

    public void SetOwnerLane(Lane l, Side side)
    {
        Lane = l;
        BezierSeries bs = l.BezierSeries;
        if (side == Side.Start)
            SeriesInterpolation = Constants.MinimumLaneLength / 2 / bs.Length;
        else
            SeriesInterpolation = (bs.Length - Constants.MinimumLaneLength / 2) / bs.Length;
    }
}