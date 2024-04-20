using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Splines;

public class Road
{
    public int Id { get; set; }
    [JsonIgnore]
    public BezierCurve BezierCurve { get; set; }
    public List<Lane> Lanes { get; set; }
    public int LaneCount { get; set; }
    public float3 StartPos { get; set; }
    public float3 PivotPos { get; set; }
    public float3 EndPos { get; set; }
    public float Length { get; set; }
    public RoadOutline LeftOutline { get; set; }
    public RoadOutline RightOutline { get; set; }

    // Empty constructor for JSON.Net deserialization
    public Road() { }

    public Road(float3 startPos, float3 pivotPos, float3 endPos, int laneCount)
    {
        StartPos = startPos;
        PivotPos = pivotPos;
        EndPos = endPos;
        LaneCount = laneCount;
        InitCurve();

        InitRoad();
    }

    public Road(BezierCurve curve, int laneCount)
    {
        BezierCurve = curve;
        StartPos = curve.P0;
        PivotPos = curve.P1;
        EndPos = curve.P3;
        LaneCount = laneCount;

        InitRoad();
    }

    private void InitRoad()
    {
        Length = CurveUtility.CalculateLength(BezierCurve);
        InitLanes();
        LeftOutline = new();
        RightOutline = new();
        int numPoints = (int)((Length - Constants.MinimumLaneLength) * Constants.MeshResolution);

        LeftOutline.Mid = Lanes.First().InnerPath.GetOutline(numPoints, Orientation.Left);
        RightOutline.Mid = Lanes.Last().InnerPath.GetOutline(numPoints, Orientation.Right);
    }

    public void RestoreFromDeserialization()
    {
        InitCurve();
        foreach (Lane lane in Lanes)
            lane.InitSpline();
    }

    public void InitCurve()
    {
        BezierCurve = new(StartPos, PivotPos, EndPos);
    }


    public float3 InterpolateLanePos(float t, int lane)
    {
        float3 normal = GetNormal(t);
        float3 pos = CurveUtility.EvaluatePosition(BezierCurve, t);
        float3 offset = normal * (Constants.LaneWidth * ((float)LaneCount / 2 - 0.5f) - lane * Constants.LaneWidth);
        return pos + offset;
    }

    public float3 GetNormal(float t)
    {
        float3 tangent = CurveUtility.EvaluateTangent(BezierCurve, t);
        tangent.y = 0;
        return Vector3.Cross(tangent, Vector3.up).normalized;
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
        return LeftOutline.Mid.Count != 0 && RightOutline.Mid.Count != 0;
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

    void InitLanes()
    {
        Lanes = new();
        for (int i = 0; i < LaneCount; i++)
            Lanes.Add(new(this, i));
    }
}