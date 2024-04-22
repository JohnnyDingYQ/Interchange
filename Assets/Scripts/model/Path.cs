using System;
using System.Collections.Generic;
using QuikGraph;
using Unity.Mathematics;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Serialization;

[JsonObject]
public class Path : IEdge<Vertex>, IComparable<Path>
{
    [JsonProperty]
    public BezierSeries BezierSeries { get; private set; }
    [JsonProperty]
    public Vertex Source { get; private set; }
    [JsonProperty]
    public Vertex Target { get; private set; }
    /// <summary>
    /// straight path: Span = 0 ||
    /// left turn path: Span = -1 ||
    /// right turn path: Span = 1, 
    /// </summary>
    [JsonProperty]
    public int Span { get; private set; }

    public Path() { }

    public Path(BezierSeries bezierSeries, Vertex source, Vertex target, int span)
    {
        BezierSeries = bezierSeries;
        Source = source;
        Target = target;
        Span = span;
    }

    public List<float3> GetOutline(Orientation orientation)
    {
        return BezierSeries.GetOutline(orientation);
    }

    public int CompareTo(Path other)
    {
        return Span.CompareTo(other.Span);
    }
}