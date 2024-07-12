using System;
using Unity.Mathematics;
using Unity.Plastic.Newtonsoft.Json;

public class Vertex
{
    public uint Id { get; set; }
    [JsonProperty]
    public float3 Pos { get; private set; }
    [JsonProperty]
    public float SeriesInterpolation { get; private set; }
    [JsonIgnore]
    public float3 Tangent { get => math.normalizesafe(Lane.BezierSeries.EvaluateTangent(SeriesInterpolation)); }
    [JsonIgnore]
    public Lane Lane { get; set; }
    public uint Lane_ { get; set; }
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
    }

    public void SetOwnerLane(Lane l, Side side)
    {
        Lane = l;
        BezierSeries bs = l.BezierSeries;
        if (side == Side.Start)
            SeriesInterpolation = Constants.VertexDistanceFromRoadEnds / bs.Length;
        else
            SeriesInterpolation = (bs.Length - Constants.VertexDistanceFromRoadEnds) / bs.Length;
    }

    public override string ToString()
    {
        return "Vertex " + Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;
        return Id == ((Vertex) obj).Id;
    }
}