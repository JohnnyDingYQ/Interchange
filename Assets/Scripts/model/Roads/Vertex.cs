using System;
using Unity.Mathematics;
using Unity.Plastic.Newtonsoft.Json;

public class Vertex
{
    public uint Id { get; set; }
    [JsonIgnore]
    public float3 Pos { get => GetPos(); }
    [JsonIgnore]
    public float3 Tangent { get => GetTangent(); }
    [JsonIgnore]
    public Lane Lane { get; set; }
    public uint Lane_ { get; set; }
    [JsonIgnore]
    public Road Road { get { return Lane.Road; } }
    [JsonIgnore]
    public int ScheduledCars { get; set; }
    [JsonProperty]
    Side side;

    public Vertex() { }

    public Vertex(Lane lane, Side side)
    {
        SetOwnerLane(lane, side);
    }

    public void SetOwnerLane(Lane l, Side side)
    {
        Lane = l;
        this.side = side;
    }

    float3 GetPos()
    {
        Curve curve = Lane.Curve;
        return side == Side.Start
            ? curve.EvaluateDistancePos(Constants.VertexDistanceFromRoadEnds)
            : curve.EvaluateDistancePos(curve.Length - Constants.VertexDistanceFromRoadEnds);
    }

    float3 GetTangent()
    {
        Curve curve = Lane.Curve;
        return side == Side.Start
            ? math.normalize(curve.EvaluateDistanceTangent(Constants.VertexDistanceFromRoadEnds))
            : math.normalize(curve.EvaluateDistanceTangent(curve.Length - Constants.VertexDistanceFromRoadEnds));
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
        return Id == ((Vertex)obj).Id;
    }
}