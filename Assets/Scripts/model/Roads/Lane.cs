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
    public BezierSeries BezierSeries { get; private set; }
    [JsonIgnore]
    public float3 StartPos { get { return StartNode.Pos; } }
    [JsonIgnore]
    public float3 EndPos { get { return EndNode.Pos; } }
    [JsonIgnore]
    public Road Road { get; set; }
    public uint Road_ { get; set; }
    [JsonProperty]
    public int LaneIndex { get; private set; }
    public float Length { get; private set; }
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
        BezierSeries = Road.BezierSeries.Offset(((float)Road.LaneCount / 2 - 0.5f - LaneIndex) * Constants.LaneWidth);
        Length = BezierSeries.Length;
    }

    public void InitNodes()
    {
        StartNode = new(BezierSeries.EvaluatePosition(0), Road.StartPos.y, LaneIndex);
        EndNode = new(BezierSeries.EvaluatePosition(1), Road.EndPos.y, LaneIndex);
        StartNode.AddLane(this, Direction.Out);
        EndNode.AddLane(this, Direction.In);
    }

    public void InitVerticesAndInnerPath()
    {
        StartVertex = new(this, Side.Start);
        EndVertex = new(this, Side.End);
        float startInterpolation = Constants.VertexDistanceFromRoadEnds / BezierSeries.Length;
        float endInterpolation = (BezierSeries.Length - Constants.VertexDistanceFromRoadEnds) / BezierSeries.Length;
        BezierSeries bs = new(BezierSeries, startInterpolation, endInterpolation);
        Path path = new(bs, StartVertex, EndVertex);
        InnerPath = path;
    }

    public float3 EvaluatePosition(float t)
    {
        return BezierSeries.EvaluatePosition(t);
    }

    public float3 EvaluateTangent(float t)
    {
        return BezierSeries.EvaluateTangent(t);
    }

    public float3 Evaluate2DNormalizedNormal(float t)
    {
        return BezierSeries.Evaluate2DNormalizedNormal(t);
    }

    public override string ToString()
    {
        return "Lane " + LaneIndex + " of Road " + Road.Id;
    }

}