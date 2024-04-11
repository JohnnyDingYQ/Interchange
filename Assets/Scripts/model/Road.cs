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
        InitLanes();
        Length = CurveUtility.CalculateLength(BezierCurve);
        LeftOutline = new();
        RightOutline = new();
        int numPoints = (int)((Length - Constants.MinimumLaneLength) * Constants.MeshResolution);
        LeftOutline.Mid = GetOutline(Lanes.First().StartVertex, Lanes.First().EndVertex, numPoints, true);
        RightOutline.Mid = GetOutline(Lanes.Last().StartVertex, Lanes.Last().EndVertex, numPoints, false);
    }

    public void InitCurve()
    {
        BezierCurve = new BezierCurve(StartPos, PivotPos, EndPos);
    }

    public float3 InterpolateLanePos(float t, int lane)
    {
        float3 normal = GetNormal(t);
        float3 pos = CurveUtility.EvaluatePosition(BezierCurve, t);
        float3 offset = normal * (Constants.LaneWidth * ((float)LaneCount / 2 - 0.5f) - lane * Constants.LaneWidth);
        return pos + offset;
    }

    float3 GetNormal(float t)
    {
        float3 tangent = CurveUtility.EvaluateTangent(BezierCurve, t);
        tangent.y = 0;
        return Vector3.Cross(tangent, Vector3.up).normalized;
    }

    void InitLanes()
    {

        Lanes = new();

        for (int i = 0; i < LaneCount; i++)
        {
            Lanes.Add(new(this, i));
        }
    }

    List<float3> GetOutline(Vertex start, Vertex end, int numPoints, bool isLeft)
    {
        List<float3> results = new();
        if (Game.Graph.TryGetEdge(start, end, out Path left))
        {
            for (int i = 0; i <= numPoints; i++)
            {
                float t = (float)i / numPoints;
                float3 normal = left.Evaluate2DNormal(t) * Constants.LaneWidth / 2;
                normal.y = 0;
                if (isLeft)
                    results.Add(left.EvaluatePosition(t) + normal);
                else
                    results.Add(left.EvaluatePosition(t) - normal);
            }
            return results;
        }
        else
            throw new InvalidOperationException("fatal error: path not found");
    }

    public void UpdateInterRoadOutline()
    {
        if (Game.Graph.TryGetOutEdges(Lanes.First().EndVertex, out IEnumerable<Path> lEdges))
        {
            int numPoints = (int)(Constants.MinimumLaneLength * Constants.MeshResolution);
            List<Path> l = new(lEdges);
            l.Sort();
            LeftOutline.Right = GetOutline(Lanes.First().EndVertex, l.First().Target, numPoints, true);
        }
        if (Game.Graph.TryGetOutEdges(Lanes.Last().EndVertex, out IEnumerable<Path> rEdges))
        {
            int numPoints = (int)(Constants.MinimumLaneLength * Constants.MeshResolution);
            List<Path> l = new(rEdges);
            l.Sort();
            RightOutline.Right = GetOutline(Lanes.Last().EndVertex, l.Last().Target, numPoints, false);
        }
    }

}