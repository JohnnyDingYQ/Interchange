using System.Collections.Generic;
using QuikGraph;
using Unity.Mathematics;

public class Path : IEdge<Vertex>
{
    public ICurve Curve { get; set; }

    public Vertex Source { get; set; }

    public Vertex Target { get; set; }

    public Path() { }

    public Path(ICurve curve, Vertex source, Vertex target)
    {
        Curve = curve;
        Source = source;
        Target = target;
    }
    public float3 EvaluatePosition(float t)
    {
        return Curve.EvaluatePosition(t);
    }

        public float3 Evaluate2DNormal(float t)
    {
        return Curve.Evaluate2DNormal(t);
    }
}