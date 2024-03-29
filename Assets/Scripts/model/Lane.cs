using UnityEngine.Splines;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Mathematics;
using System.IO;

public class Lane
{
    public Vertex StartVertex { get; set; }
    public Vertex EndVertex { get; set; }
    public Node StartNode { get; set; }
    public Node EndNode { get; set; }
    [JsonIgnore]
    public Spline Spline { get; set; }
    [JsonIgnore]
    public float3 StartPos
    {
        get
        {
            return StartNode.Pos;
        }
    }
    [JsonIgnore]
    public float3 EndPos
    {
        get
        {
            return EndNode.Pos;
        }
    }
    public Road Road { get; set; }
    public int LaneIndex { get; set; }
    public float Length { get; set; }

    // Empty constructor for JSON.Net deserialization
    public Lane() { }

    public Lane(Road road, int laneIndex)
    {
        LaneIndex = laneIndex;
        Road = road;
        InitSpline();
        InitNodes();
        Length = Spline.GetLength();
        InitVertices();
    }

    public void InitSpline()
    {
        Spline = GetLaneSpline(LaneIndex);
    }

    void InitNodes()
    {
        StartNode = new(Spline.EvaluatePosition(0), LaneIndex);
        EndNode = new(Spline.EvaluatePosition(1), LaneIndex);
        StartNode.AddLane(this, Direction.Out);
        EndNode.AddLane(this, Direction.In);
    }

    void InitVertices()
    {
        StartVertex = new(this, Side.Start);
        EndVertex = new(this, Side.End);
        ICurve curve = new SplineAdapter(Spline, StartVertex.Interpolation, EndVertex.Interpolation);
        Path path = new(curve, StartVertex, EndVertex);
        Game.AddVertex(StartVertex);
        Game.AddVertex(EndVertex);
        Game.AddEdge(path);
    }

    /// <summary>
    /// Requirement: Lane knows it parent road
    /// </summary>
    private Spline GetLaneSpline(int laneNumber)
    {
        int segCount = 10;
        Spline laneSpline = new();

        // Iterate by distance on curve
        for (int i = 0; i <= segCount; i++)
        {
            float t = 1 / (float)segCount * i;

            float3 pos = Road.InterpolateLanePos(t, laneNumber);
            laneSpline.Add(new BezierKnot(pos), TangentMode.AutoSmooth);
        }
        return laneSpline;
    }

    public override string ToString()
    {
        return "Lane " + LaneIndex + " of Road " + Road.Id;
    }

}