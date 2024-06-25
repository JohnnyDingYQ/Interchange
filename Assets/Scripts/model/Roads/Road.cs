using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Splines;

public class Road
{
    public uint Id { get; set; }
    public BezierSeries BezierSeries { get; set; }
    [JsonIgnore]
    public List<Lane> Lanes { get; set; }
    public List<uint> Lanes_ { get; set; }
    [JsonProperty]
    public int LaneCount { get; private set; }
    [JsonProperty]
    public float3 StartPos { get; private set; }
    [JsonProperty]
    public float3 EndPos { get; private set; }
    [JsonProperty]
    public float Length { get; private set; }
    [JsonIgnore]
    public RoadOutline LeftOutline { get; set; }
    [JsonIgnore]
    public RoadOutline RightOutline { get; set; }
    [JsonIgnore]
    public Intersection StartIntersection { get; set; }
    [JsonIgnore]
    public Intersection EndIntersection { get; set; }
    public uint StartIntersection_ { get; set; }
    public uint EndIntersection_ { get; set; }
    [JsonIgnore]
    public bool IsGhost { get; set; }

    // Empty constructor for JSON.Net deserialization
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
        Length = BezierSeries.Length;
        InitLanes();
        if (HasLaneShorterThanMinimumLaneLength())
            return;
        foreach (Lane l in Lanes)
        {
            l.InitNodes();
            l.InitVertices();
        }
        StartIntersection = new(this, Side.Start);
        EndIntersection = new(this, Side.End);
        LeftOutline = new();
        RightOutline = new();
        EvaluateInnerOutline();
    }

    public void EvaluateInnerOutline()
    {
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
            return BezierSeries.EvaluatePosition(0)
            + ((float)LaneCount / 2 - 0.5f - laneIndex) * Constants.LaneWidth * BezierSeries.Evaluate2DNormalizedNormal(0);
        return BezierSeries.EvaluatePosition(1)
        + ((float)LaneCount / 2 - 0.5f - laneIndex) * Constants.LaneWidth * BezierSeries.Evaluate2DNormalizedNormal(1);
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