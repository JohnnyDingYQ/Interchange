using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Splines;

public class Road
{
    public int Id { get; set; }
    public BezierSeries BezierSeries { get; set; }
    [JsonProperty]
    public List<Lane> Lanes { get; private set; }
    [JsonProperty]
    public int LaneCount { get; private set; }
    [JsonProperty]
    public float3 StartPos { get; private set; }
    [JsonProperty]
    public float3 EndPos { get; private set; }
    [JsonProperty]
    public float Length { get; private set; }
    public Intersection StartIntersection { get; set; }
    public Intersection EndIntersection { get; set; }
    public RoadOutline LeftOutline { get; set; }
    public RoadOutline RightOutline { get; set; }
    public bool IsGhost { get; set; }

    // Empty constructor for JSON.Net deserialization
    [JsonConstructor]
    public Road()
    {
        LeftOutline = new();
        RightOutline = new();
    }

    public Road(float3 startPos, float3 pivotPos, float3 endPos, int laneCount)
    {
        LaneCount = laneCount;
        BezierSeries = new(new BezierCurve(startPos, pivotPos, endPos));
        StartPos = startPos;
        EndPos = endPos;

        InitRoad();
    }

    public Road(BezierSeries bs, int laneCount)
    {
        BezierSeries = bs;
        LaneCount = laneCount;
        StartPos = bs.Curves.First().P0;
        EndPos = bs.Curves.Last().P3;

        InitRoad();
    }

    private void InitRoad()
    {
        IsGhost = false;
        Length = BezierSeries.Length;
        InitLanes();
        StartIntersection = new();
        StartIntersection.AddRoad(this, Side.Start);
        EndIntersection = new();
        EndIntersection.AddRoad(this, Side.End);
        if (HasLaneShorterThanMinimumLaneLength())
            return;
        LeftOutline = new();
        RightOutline = new();
        foreach (Lane l in Lanes)
            l.InitVertices();
        LeftOutline.Mid = Lanes.First().InnerPath.GetOutline(Orientation.Left);
        RightOutline.Mid = Lanes.Last().InnerPath.GetOutline(Orientation.Right);
    }

    public List<Node> GetNodes(Side side)
    {
        List<Node> n = new();
        foreach (Lane lane in Lanes)
            n.Add(side == Side.Start ? lane.StartNode : lane.EndNode);
        return n;
    }

    public bool HasNoneEmptyOutline()
    {
        return LeftOutline.Mid.Count != 0 && RightOutline.Mid.Count != 0
            && LeftOutline.Start.Count != 0 && RightOutline.Start.Count != 0
            && LeftOutline.End.Count != 0 && RightOutline.End.Count != 0;
    }

    public bool OutLinePlausible()
    {
        return HasNoneEmptyOutline() && LeftOutline.IsPlausible() && RightOutline.IsPlausible();
    }

    public bool HasLaneShorterThanMinimumLaneLength()
    {
        foreach (Lane lane in Lanes)
            if (lane.Length < Constants.MinimumLaneLength)
                return true;
        return false;
    }

    public float3 ExtrapolateNodePos(Side side, int laneIndex)
    {
        if (side == Side.Start)
            return BezierSeries.EvaluatePosition(BezierSeries.StartLocation)
            + ((float)LaneCount / 2 - 0.5f - laneIndex) * Constants.LaneWidth * BezierSeries.Evaluate2DNormalizedNormal(BezierSeries.StartLocation);
        return BezierSeries.EvaluatePosition(BezierSeries.EndLocation)
        + ((float)LaneCount / 2 - 0.5f - laneIndex) * Constants.LaneWidth * BezierSeries.Evaluate2DNormalizedNormal(BezierSeries.EndLocation);
    }

    void InitLanes()
    {
        Lanes = new();
        for (int i = 0; i < LaneCount; i++)
            Lanes.Add(new(this, i));
    }

    public HashSet<Road> GetConnectedRoads(Side side)
    {
        HashSet<Road> r = new();
        foreach (Lane lane in Lanes)
        {
            if (side == Side.Both)
            {
                r.UnionWith(lane.StartNode.GetRoads(Direction.In));
                r.UnionWith(lane.EndNode.GetRoads(Direction.Out));
            }
            if (side == Side.Start)
                r.UnionWith(lane.StartNode.GetRoads(Direction.In));
            if (side == Side.End)
                r.UnionWith(lane.EndNode.GetRoads(Direction.Out));
        }
        return r;
    }

    public override string ToString()
    {
        return " Road " + Id;
    }
}