using Unity.Mathematics;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Assets.Scripts.model.Roads;

public class Lane : IPersistable
{
    public uint Id { get; set; }
    public int LaneIndex { get; private set; }
    [SaveID]
    public Vertex StartVertex { get; set; }
    [SaveID]
    public Vertex EndVertex { get; set; }
    [SaveID]
    public Node StartNode { get; set; }
    [SaveID]
    public Node EndNode { get; set; }
    [SaveID]
    public Road Road { get; set; }
    [NotSaved]
    public Curve Curve { get; set; }
    [SaveID]
    public Edge InnerEdge { get; set; }
    [NotSaved]
    public float3 StartPos { get => StartNode.Pos; }
    [NotSaved]
    public float3 EndPos { get => EndNode.Pos; }
    [NotSaved]
    public float Length { get => Curve.Length; }

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

    public void InitInnerEdge()
    {
        Curve curve = Curve.Duplicate();
        curve = curve.AddStartDistance(Constants.VertexDistanceFromRoadEnds);
        curve = curve.AddEndDistance(Constants.VertexDistanceFromRoadEnds);
        Edge edge = new(curve, StartVertex, EndVertex);
        InnerEdge = edge;
        InnerEdge.IsInnerEdge = true;
    }

    public override string ToString()
    {
        return "Lane " + LaneIndex + " of Road " + Road.Id;
    }

    public override bool Equals(object obj)
    {
        if (obj is Lane other)
            return Id == other.Id && IPersistable.Equals(StartVertex, other.StartVertex) && IPersistable.Equals(EndVertex, other.EndVertex)
                && IPersistable.Equals(StartNode, other.StartNode) && IPersistable.Equals(EndNode, other.EndNode)
                && IPersistable.Equals(Curve, other.Curve) && IPersistable.Equals(Road, other.Road)
                && LaneIndex == other.LaneIndex;
        else
            return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}