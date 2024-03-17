using System;

public class Vertex
{
    public int Id { get; set; }

    public Vertex() {}

    public Vertex(Lane lane, Side side)
    {
        if (lane.Spline == null)
            throw new InvalidOperationException("Lane spline is null");
        if (side == Side.Start)
        {
            
        }
    }
}