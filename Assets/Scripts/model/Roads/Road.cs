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
    public Curve Curve { get; set; }
    [JsonIgnore]
    public List<Lane> Lanes { get; set; }
    public List<uint> Lanes_ { get; set; }
    [JsonProperty]
    public int LaneCount { get; private set; }
    [JsonIgnore]
    public float3 StartPos { get => Curve.StartPos; }
    [JsonIgnore]
    public float3 EndPos { get => Curve.EndPos; }
    [JsonIgnore]
    public float Length { get => Curve.Length; }
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
        Curve = new(new BezierCurve(startPos, pivotPos, endPos));

        InitRoad();
    }

    public Road(Curve bs, int laneCount)
    {
        Curve = bs;
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
            l.InitVertices();
            l.InitInnerPath();
        }
        StartIntersection = new(this, Direction.Out);
        EndIntersection = new(this, Direction.In);
        LeftOutline = new();
        RightOutline = new();
        SetInnerOutline();
    }

    public void SetInnerOutline()
    {
        LeftOutline.MidCurve = Lanes.First().InnerPath.Curve.Duplicate().Offset(Constants.RoadOutlineSeparation);
        RightOutline.MidCurve = Lanes.Last().InnerPath.Curve.Duplicate().Offset(-Constants.RoadOutlineSeparation);
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
        return LeftOutline.MidCurve != null && RightOutline.MidCurve != null
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

    void InitLanes()
    {
        Lanes = new();
        for (int i = 0; i < LaneCount; i++)
            Lanes.Add(new(this, i));
    }

    public bool DistanceBetweenVertices(float distance)
    {
        return distance >= Constants.VertexDistanceFromRoadEnds && (Length - distance) >= Constants.VertexDistanceFromRoadEnds;
    }

    public float GetNearestDistance(float3 clickPos)
    {
        clickPos.y = 0;
        Ray ray = new(clickPos, Vector3.up);
        Curve.GetNearestPoint(ray, out float distanceOnCurve);
        return distanceOnCurve;
    }

    public override string ToString()
    {
        return " Road " + Id;
    }
}