using Unity.Plastic.Newtonsoft.Json;
using Unity.Mathematics;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
public class Lane
{
    public uint Id { get; set; }
    public Vertex StartVertex { get; set; }
    public Vertex EndVertex { get; set; }
    public Node StartNode { get; set; }
    public Node EndNode { get; set; }
    public Curve Curve { get; private set; }
    public Road Road { get; set; }
    public int LaneIndex { get; private set; }
    public Path InnerPath { get; set; }
    public float3 StartPos { get => StartNode.Pos; }
    public float3 EndPos { get => EndNode.Pos; }
    public float Length { get => Curve.Length; }

    // Empty constructor for JSON.Net deserialization
    public Lane() { }

    public Lane(Road road, int laneIndex)
    {
        LaneIndex = laneIndex;
        Road = road;
        InitCurve();
    }

    public void InitCurve()
    {
        Assert.IsNotNull(Road);
        Curve = Road.Curve.Duplicate();
        Curve.Offset(((float)Road.LaneCount / 2 - 0.5f - LaneIndex) * Constants.LaneWidth);
    }

    public void InitNodes()
    {
        StartNode = new(Curve.StartPos, Road.StartPos.y, LaneIndex);
        EndNode = new(Curve.EndPos, Road.EndPos.y, LaneIndex);
        StartNode.OutLane = this;
        EndNode.InLane = this;
    }

    public void InitVertices()
    {
        StartVertex = new(this, Side.Start);
        EndVertex = new(this, Side.End);
    }

    public void InitInnerPath()
    {
        Curve curve = Curve.Duplicate();
        curve = curve.AddStartDistance(Constants.VertexDistanceFromRoadEnds);
        curve = curve.AddEndDistance(Constants.VertexDistanceFromRoadEnds);
        Path path = new(curve, StartVertex, EndVertex);
        InnerPath = path;
    }

    public override string ToString()
    {
        return "Lane " + LaneIndex + " of Road " + Road.Id;
    }

    public override bool Equals(object obj)
    {
        if (obj is Lane other)
        {
            return Id == other.Id;
        }
        else
        {
            return false;
        }
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

}