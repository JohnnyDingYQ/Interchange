using Unity.Plastic.Newtonsoft.Json;
using Unity.Mathematics;
using System.Linq;
using UnityEngine;
public class Lane
{
    public uint Id { get; set; }
    public Vertex StartVertex { get; set; }
    public Vertex EndVertex { get; set; }
    [JsonIgnore]
    public Node StartNode { get; set; }
    [JsonIgnore]
    public Node EndNode { get; set; }
    public uint StartNode_ { get; set; }
    public uint EndNode_ { get; set; }
    public BezierSeries BezierSeries { get; set; }
    [JsonIgnore]
    public float3 StartPos { get { return StartNode.Pos; } }
    [JsonIgnore]
    public float3 EndPos { get { return EndNode.Pos; } }
    [JsonIgnore]
    public Road Road { get; set; }
    public uint Road_ {get; set; }
    [JsonProperty]
    public int LaneIndex { get; private set; }
    [JsonProperty]
    public float Length { get; private set; }
    public Path InnerPath { get; set; }

    // Empty constructor for JSON.Net deserialization
    public Lane() { }

    public Lane(Road road, int laneIndex)
    {
        LaneIndex = laneIndex;
        BezierSeries = road.BezierSeries.Offset(((float)road.LaneCount / 2 - 0.5f - laneIndex) * Constants.LaneWidth);
        Road = road;
        Length = BezierSeries.Length;
    }

    public void InitNodes()
    {
        StartNode = new(BezierSeries.EvaluatePosition(0), Road.StartPos.y, LaneIndex);
        EndNode = new(BezierSeries.EvaluatePosition(1), Road.EndPos.y, LaneIndex);
        StartNode.AddLane(this, Direction.Out);
        EndNode.AddLane(this, Direction.In);
    }

    public void InitVertices()
    {
        StartVertex = new(this, Side.Start);
        EndVertex = new(this, Side.End);
        float startInterpolation = Constants.VertexDistanceFromRoadEnds / BezierSeries.Length;
        float endInterpolation = (BezierSeries.Length - Constants.VertexDistanceFromRoadEnds) / BezierSeries.Length;
        BezierSeries bs = new(BezierSeries, startInterpolation, endInterpolation);
        Path path = new(bs, StartVertex, EndVertex);
        InnerPath = path;
    }

    public override string ToString()
    {
        return "Lane " + LaneIndex + " of Road " + Road.Id;
    }

}