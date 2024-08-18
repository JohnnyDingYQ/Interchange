using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;

public class Road : IPersistable
{
    public uint Id { get; set; }
    [SaveID]
    public Curve Curve { get; set; }
    public int LaneCount { get; private set; }
    [SaveIDCollection]
    public List<Lane> Lanes { get; set; }
    [SaveID]
    public Intersection StartIntersection { get; set; }
    [SaveID]
    public Intersection EndIntersection { get; set; }
    [NotSaved]
    public bool IsGhost { get; set; }
    [NotSaved]
    public RoadOutline LeftOutline { get; set; }
    [NotSaved]
    public RoadOutline RightOutline { get; set; }
    [NotSaved]
    public float3 StartPos { get => Curve.StartPos; }
    [NotSaved]
    public float3 EndPos { get => Curve.EndPos; }
    [NotSaved]
    public float Length { get => Curve.Length; }
    [NotSaved]
    public bool IsMajorRoad { get => StartIntersection.IsMajorRoad(this) || EndIntersection.IsMajorRoad(this); }

    public Road()
    {
        LeftOutline = new();
        RightOutline = new();
    }

    public Road(Curve curve, int laneCount)
    {
        Curve = curve;
        LaneCount = laneCount;

        InitRoad();
    }

    private void InitRoad()
    {
        Lanes = new();
        for (int i = 0; i < LaneCount; i++)
            Lanes.Add(new(this, i));
        if (HasLaneShorterThanMinLaneLength())
            return;
    
        foreach (Lane l in Lanes)
        {
            l.InitNodes();
            l.InitVertices();
            l.InitInnerEdge();
        }
        StartIntersection = new(this, Direction.Out);
        EndIntersection = new(this, Direction.In);
        LeftOutline = new();
        RightOutline = new();
        SetInnerOutline();
    }

    public void SetInnerOutline()
    {
        LeftOutline.MidCurve = Lanes.First().InnerEdge.Curve.Duplicate().Offset(Constants.RoadOutlineSeparation);
        RightOutline.MidCurve = Lanes.Last().InnerEdge.Curve.Duplicate().Offset(-Constants.RoadOutlineSeparation);
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
            && LeftOutline.Start.Count() != 0 && RightOutline.Start.Count() != 0
            && LeftOutline.End.Count() != 0 && RightOutline.End.Count() != 0;
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

    public bool DistanceBetweenVertices(float distance)
    {
        return distance >= Constants.VertexDistanceFromRoadEnds && (Length - distance) >= Constants.VertexDistanceFromRoadEnds;
    }

    public float GetNearestDistance(float3 clickPos)
    {
        clickPos.y = Constants.MinElevation - 1;
        Ray ray = new(clickPos, Vector3.up);
        Curve.GetNearestPoint(ray, out float distanceOnCurve);
        return distanceOnCurve;
    }
    
    public override bool Equals(object obj)
    {
        if (obj is Road other)
        {
            return Id == other.Id && IPersistable.Equals(Curve, other.Curve) && LaneCount == other.LaneCount
                && Lanes.Select(l => l.Id).SequenceEqual(other.Lanes.Select(l => l.Id))
                && IPersistable.Equals(StartIntersection, other.StartIntersection)
                && IPersistable.Equals(EndIntersection, other.EndIntersection);
        }
        else
            return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string ToString()
    {
        return " Road " + Id;
    }
}