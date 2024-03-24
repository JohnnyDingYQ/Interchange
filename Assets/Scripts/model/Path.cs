using System.Collections.Generic;
using QuikGraph;

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
}