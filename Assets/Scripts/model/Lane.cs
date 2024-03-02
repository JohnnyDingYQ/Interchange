using UnityEngine.Splines;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using Unity.Mathematics;

public class Lane
{
    public Node StartNode { get; set; }
    public Node EndNode { get; set; }
    [JsonIgnore]
    public Spline Spline { get; set; }
    public Vector3 StartPos {
        get {
            return StartNode.Pos;
        }
    }
    public Vector3 EndPos {
        get {
            return EndNode.Pos;
        }
    }
    public Road Road { get; set; }
    public int LaneIndex { get; set; }

    public Lane() {}

    public Lane(Road road, int laneIndex)
    {
        LaneIndex = laneIndex;
        Road = road;
        InitSpline();
        StartNode = new(Spline.EvaluatePosition(0), laneIndex);
        EndNode = new(Spline.EvaluatePosition(1), laneIndex);
        StartNode.Lanes.Add(this);
        EndNode.Lanes.Add(this);
    }

    public void InitSpline()
    {
        Spline = GetLaneSpline(LaneIndex);
    }

    private Spline GetLaneSpline(int laneNumber)
    {
        int segCount = 10;
        Spline laneSpline = new();

        // Iterate by distance on curve
        for (int i = 0; i <= segCount; i++)
        {
            float t = 1 / (float)segCount * i;

            float3 pos = CurveUtility.EvaluatePosition(Road.Curve, t) + InterpolateLaneOffset(t, laneNumber);
            laneSpline.Add(new BezierKnot(pos), TangentMode.AutoSmooth);
        }
        return laneSpline;
    }

    private float3 InterpolateLaneOffset(float t, int lane)
    {
        float3 normal = GetNormal(t);
        return  normal * (GlobalConstants.LaneWidth * ((float) Road.LaneCount / 2 - 0.5f) - lane * GlobalConstants.LaneWidth);
    }
    private float3 GetNormal(float t)
    {
        float3 tangent = CurveUtility.EvaluateTangent(Road.Curve, t);
        tangent.y = 0;
        return Vector3.Cross(tangent, Vector3.up).normalized;
    }

    public override string ToString()
    {
        return "<Lane " + LaneIndex + " of Road " + Road.Id + ">";
    }

}