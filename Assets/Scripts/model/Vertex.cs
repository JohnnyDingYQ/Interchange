using System;
using Unity.Mathematics;
using UnityEngine.Splines;

public class Vertex : IComparable<Vertex>
{
    public int Id { get; set; }
    public float3 Pos { get; set; }
    public float Interpolation { get; set; }
    public float3 Tangent { get; set; }
    public Lane Lane { get; private set; }
    public Road Road { get { return Lane.Road; } }

    public Vertex() { }

    public Vertex(Lane lane, Side side)
    {
        Lane = lane;
        if (lane.Spline == null)
            throw new InvalidOperationException("Lane spline is null");
        if (side == Side.Start)
        {
            Interpolation = Constants.MinimumLaneLength / 2 / lane.Length;
        }
        else
        {
            Interpolation = (lane.Length - Constants.MinimumLaneLength / 2) / lane.Length;
        }
        Pos = lane.Spline.EvaluatePosition(Interpolation);
        Tangent = math.normalizesafe(lane.Spline.EvaluateTangent(Interpolation));
    }

    public int CompareTo(Vertex other)
    {
        return Lane.Order.CompareTo(other.Lane.Order);
    }
}