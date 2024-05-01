using System;
using System.Collections.Generic;
using QuikGraph;
using Unity.Mathematics;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Serialization;

[JsonObject]
public class Path : IEdge<Vertex>
{
    [JsonProperty]
    public BezierSeries BezierSeries { get; private set; }
    [JsonProperty]
    public Vertex Source { get; private set; }
    [JsonProperty]
    public Vertex Target { get; private set; }

    public Path() { }

    public Path(BezierSeries bezierSeries, Vertex source, Vertex target)
    {
        BezierSeries = bezierSeries;
        Source = source;
        Target = target;
    }

    public List<float3> GetOutline(Orientation orientation)
    {
        return BezierSeries.GetOutline(orientation);
    }
}