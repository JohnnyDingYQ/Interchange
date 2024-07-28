using System;
using Unity.Mathematics;

public class Vertex : IPersistable
{
    public uint Id { get; set; }
    public float3 Pos { get => GetPos(); }
    public float3 Tangent { get => GetTangent(); }
    public Lane Lane { get; set; }
    public int ScheduledCars { get; set; }
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
            ? curve.EvaluateDistanceTangent(Constants.VertexDistanceFromRoadEnds)
            : curve.EvaluateDistanceTangent(curve.Length - Constants.VertexDistanceFromRoadEnds);
    }

    public void Save(Writer writer)
    {
        writer.Write(Id);
        writer.Write(ScheduledCars);
        writer.Write((int)side);
        writer.Write(Lane.Id);
    }

    public void Load(Reader reader)
    {
        Id = reader.ReadUint();
        ScheduledCars = reader.ReadInt();
        side = (Side)reader.ReadInt();
        uint laneID = reader.ReadUint();
        Lane = laneID == 0 ? null : Lane = new() { Id = laneID };
            
    }
    public override string ToString()
    {
        return "Vertex " + Id;
    }

    public override bool Equals(object obj)
    {
        if (obj is Vertex other)
            return Id == other.Id && ScheduledCars == other.ScheduledCars && side == other.side && Lane == other.Lane;
        else
            return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

}