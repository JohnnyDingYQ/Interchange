using System;
using Unity.Mathematics;

public class Vertex : IPersistable
{
    public uint Id { get; set; }
    Side side;
    [SaveID]
    public Lane Lane { get; set; }
    [NotSaved]
    public float ScheduleCooldown { get; set; }
    [NotSaved]
    public float3 Pos { get => GetPos(); }
    [NotSaved]
    public float3 Tangent { get => GetTangent(); }

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
            ? curve.EvaluateDistanceTangent(Constants.VertexDistanceFromRoadEnds)
            : curve.EvaluateDistanceTangent(curve.Length - Constants.VertexDistanceFromRoadEnds);
    }

    public override string ToString()
    {
        return "Vertex " + Id;
    }

    public override bool Equals(object obj)
    {
        if (obj is Vertex other)
            return Id == other.Id && side == other.side && Lane.Id == other.Lane.Id;
        else
            return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

}