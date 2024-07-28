using Unity.Plastic.Newtonsoft.Json;
using Unity.Mathematics;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
public class Lane
{
    public uint Id { get; set; }
    [JsonIgnore]
    public Vertex StartVertex { get; set; }
    [JsonIgnore]
    public Vertex EndVertex { get; set; }
    public uint StartVertex_ { get; set; }
    public uint EndVertex_ { get; set; }
    [JsonIgnore]
    public Node StartNode { get; set; }
    [JsonIgnore]
    public Node EndNode { get; set; }
    public uint StartNode_ { get; set; }
    public uint EndNode_ { get; set; }
    public Curve Curve { get; private set; }
    [JsonIgnore]
    public float3 StartPos { get { return StartNode.Pos; } }
    [JsonIgnore]
    public float3 EndPos { get { return EndNode.Pos; } }
    [JsonIgnore]
    public Road Road { get; set; }
    public uint Road_ { get; set; }
    [JsonProperty]
    public int LaneIndex { get; private set; }
    [JsonIgnore]
    public float Length { get => Curve.Length; }
    [JsonIgnore]
    public Path InnerPath { get; set; }
    [JsonProperty]
    public uint InnerPath_ { get; set; }

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

}