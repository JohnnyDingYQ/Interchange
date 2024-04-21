using System;
using Unity.Mathematics;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine.Splines;

public class Vertex
{
    public int Id { get; set; }
    [JsonProperty]
    public float3 Pos { get; private set; }
    [JsonProperty]
    public float Interpolation { get; private set; }
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
        if (lane.Spline == null)
            throw new InvalidOperationException("Lane spline is null");
        if (side == Side.Start)
            Interpolation = Constants.MinimumLaneLength / 2 / lane.Length;
        else
            Interpolation = (lane.Length - Constants.MinimumLaneLength / 2) / lane.Length;
        Pos = lane.Spline.EvaluatePosition(Interpolation);
        Tangent = math.normalizesafe(lane.Spline.EvaluateTangent(Interpolation));
    }
}