using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Splines;

public class Road
{
    public int Id { get; set; }
    [JsonIgnore]
    public RoadGameObject RoadGameObject { get; set; }
    [JsonIgnore]
    public BezierCurve Curve { get; set; }
    public List<Lane> Lanes { get; set; }
    public int LaneCount { get; set; }
    public Vector3 StartPos { get; set; }
    public Vector3 PivotPos { get; set; }
    public Vector3 EndPos { get; set; }

    public Road() {}

    public Road(float3 startPos, float3 pivotPos, float3 endPos, int laneCount)
    {
        StartPos = startPos;
        PivotPos = pivotPos;
        EndPos = endPos;
        LaneCount = laneCount;
        InitCurve();

        InitLanes();
    }

    public void InitCurve()
    {
        Curve = new BezierCurve(StartPos, PivotPos, EndPos);
    }

    private void InitLanes()
    {

        Lanes = new();

        for (int i = 0; i < LaneCount; i++)
        {
            Lanes.Add(new(this, i));
        }
    }

}

