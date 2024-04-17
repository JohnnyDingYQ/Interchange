using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using QuikGraph;
using Unity.Mathematics;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Serialization;

[JsonObject]
public class Path : IEdge<Vertex>, IComparable<Path>
{
    public ICurve Curve { get; set; }
    public ICurveSaveData CurveSaveData { get; set; }
    public Vertex Source { get; set; }
    public Vertex Target { get; set; }
    /// <summary>
    /// straight path: Span = 0 ||
    /// left turn path: Span = -1 ||
    /// right turn path: Span = 1, 
    /// </summary>
    public int Span { get; set; }

    public Path() { }

    public Path(ICurve curve, Vertex source, Vertex target, int span)
    {
        Curve = curve;
        Source = source;
        Target = target;
        Span = span;
    }
    public float3 EvaluatePosition(float t)
    {
        return Curve.EvaluatePosition(t);
    }

    public float3 Evaluate2DNormal(float t)
    {
        return Curve.Evaluate2DNormal(t);
    }

    public List<float3> GetOutline(int numPoints, Orientation orientation)
    {
        List<float3> results = new();

        for (int i = 0; i <= numPoints; i++)
        {
            float t = (float)i / numPoints;
            float3 normal = Evaluate2DNormal(t) * Constants.RoadOutlineSeparation;
            if (orientation == Orientation.Left)
                results.Add(EvaluatePosition(t) + normal);
            else
                results.Add(EvaluatePosition(t) - normal);
        }
        return results;

    }

    public int CompareTo(Path other)
    {
        return Span.CompareTo(other.Span);
    }
}