using Unity.Plastic.Newtonsoft.Json;
using Unity.Mathematics;
using System.Linq;
public class Lane
{
    public Vertex StartVertex { get; set; }
    public Vertex EndVertex { get; set; }
    public Node StartNode { get; set; }
    public Node EndNode { get; set; }
    public BezierSeries BezierSeries { get; set; }
    [JsonIgnore]
    public float3 StartPos { get { return StartNode.Pos; } }
    [JsonIgnore]
    public float3 EndPos { get { return EndNode.Pos; } }
    [JsonProperty]
    public Road Road { get; private set; }
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
        StartNode = new(BezierSeries.EvaluatePosition(0), LaneIndex);
        EndNode = new(BezierSeries.EvaluatePosition(1), LaneIndex);
        StartNode.AddLane(this, Direction.Out);
        EndNode.AddLane(this, Direction.In);
    }

    public void InitVertices()
    {
        StartVertex = new(this, Side.Start);
        EndVertex = new(this, Side.End);
        SeriesLocation l = BezierSeries.GetLocationByDistance(Constants.VertexDistanceFromRoadEnds);
        SeriesLocation r = BezierSeries.GetLocationByDistance(BezierSeries.Length - Constants.VertexDistanceFromRoadEnds);
        BezierSeries bs = new(BezierSeries.Curves.ToList(), BezierSeries.IsOffsetted);
        bs.SetStartLocation(l);
        bs.SetEndLocation(r);
        Path path = new(bs, StartVertex, EndVertex);
        InnerPath = path;
    }

    public override string ToString()
    {
        return "Lane " + LaneIndex + " of Road " + Road.Id;
    }

}