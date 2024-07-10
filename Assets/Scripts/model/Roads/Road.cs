using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;
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
    [JsonIgnore]
    public float3 StartPos { get => BezierSeries.EvaluatePosition(0); }
    [JsonIgnore]
    public float3 EndPos { get => BezierSeries.EvaluatePosition(1); }
    [JsonIgnore]
    public float Length { get => BezierSeries.Length; }
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
    [JsonIgnore]
    public List<float> ArrowInterpolations { get; private set; }

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

        InitRoad();
    }

    public Road(BezierSeries bs, int laneCount)
    {
        BezierSeries = bs;
        LaneCount = laneCount;

        InitRoad();
    }

    private void InitRoad()
    {
        InitLanes();
        if (HasLaneShorterThanMinLaneLength())
            return;
        foreach (Lane l in Lanes)
        {
            l.InitNodes();
            l.InitVerticesAndInnerPath();
        }
        StartIntersection = new(this, Side.Start);
        EndIntersection = new(this, Side.End);
        LeftOutline = new();
        RightOutline = new();
        EvaluateInnerOutline();
        SetArrowPositions();
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

    public bool OutlinePlausible()
    {
        return HasNoneEmptyOutline() && LeftOutline.IsPlausible() && RightOutline.IsPlausible();
    }

    public bool HasLaneShorterThanMinLaneLength()
    {
        foreach (Lane lane in Lanes)
            if (lane.Length < Constants.MinLaneLength)
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

    public bool SplitIsValid(float interpolation)
    {
        Assert.IsTrue(interpolation <= 1 && interpolation >= 0);
        if (interpolation > 0.5f)
            interpolation = 1 - interpolation;
        foreach (Lane l in Lanes)
            if (l.Length * interpolation < Constants.MinLaneLength)
                return false;
        return true;
    }

    public void SetArrowPositions()
    {
        Assert.IsNotNull(BezierSeries);
        ArrowInterpolations = new();
        int arrowCount = (int) (BezierSeries.Length / Constants.RoadArrowSeparation);
        if (arrowCount == 0)
            arrowCount++;
        for (float i = 1; i <= arrowCount + 1; i++)
            ArrowInterpolations.Add(i / (arrowCount + 1));
        // foreach (float3 pos in ArrowPositions)
        //     DebugExtension.DebugPoint(pos, Color.cyan, 2, 5);
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
        return " Road " + Id;
    }
}